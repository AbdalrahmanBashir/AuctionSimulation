using AuctionSimulation.Hubs;
using AuctionSimulation.Models;
using Microsoft.AspNetCore.SignalR;

namespace AuctionSimulation.Services
{
    public class AuctionSimulationService : BackgroundService
    {
        private readonly ILogger<AuctionSimulationService> _logger;
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly Dictionary<string, AuctionItem> _items = new();
        private readonly string[] _bidders = new[]
        {
            "Bashir", "Smith", "Charlie", "Dave", "Eve", "Frank", "Grace", "Hannah", "Ian", "Jack"
        };
        private readonly Random _rng = new();

        public AuctionSimulationService(ILogger<AuctionSimulationService> logger, IHubContext<AuctionHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;

            InitializeItems();

            _logger.LogInformation("AuctionSimulationService initialized with {Count} items", _items.Count);
        }
        private void InitializeItems()
        {
            _items.Clear();
            for (int i = 1; i <= 6; i++)
            {
                _items[$"item{i}"] = new AuctionItem
                {
                    ItemId = $"item{i}",
                    ItemName = $"Auction Item {i}",
                    CurrentHighest = Math.Round((decimal)(_rng.NextDouble() * 50 + 10), 2),
                    HighestBidder = "StartingPrice",
                    EndsAt = DateTime.UtcNow.AddSeconds(_rng.Next(20, 40)),
                    Running = true,
                    LastBids = new List<string>()
                };
            }
        }

        public object GetSnapshot()
        {
            lock (_items)
            {
                return _items.Values.Select(i => new
                {
                    i.ItemId,
                    i.ItemName,
                    i.CurrentHighest,
                    i.HighestBidder,
                    TimeLeftSeconds = Math.Max(0, (int)(i.EndsAt - DateTime.UtcNow).TotalSeconds),
                    i.Running,
                    i.LastBids
                }).ToArray();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                lock (_items)
                {
                    foreach (var item in _items.Values)
                    {
                        if (!item.Running)
                            continue;

                        // Auto-bid simulation
                        if (_rng.NextDouble() < 0.3) // 30% chance per tick
                        {
                            var bidder = _bidders[_rng.Next(_bidders.Length)];
                            var increment = Math.Round((decimal)(_rng.NextDouble() * 20 + 1), 2);
                            item.CurrentHighest += increment;
                            item.HighestBidder = bidder;
                            item.LastBids.Insert(0, $"{bidder} bid ${item.CurrentHighest:F2}");
                            if (item.LastBids.Count > 5) item.LastBids.RemoveAt(5);

                            // extend auction if close to end (10s)
                            if ((item.EndsAt - DateTime.UtcNow).TotalSeconds < 10)
                                item.EndsAt = item.EndsAt.AddSeconds(10);

                            _logger.LogInformation("Auto-bid: {Bidder} bid {Amount} on {Item}", bidder, item.CurrentHighest, item.ItemName);
                        }

                        // Check if auction ended
                        if (DateTime.UtcNow >= item.EndsAt)
                        {
                            item.Running = false;
                            _logger.LogInformation("{Item} ended. Pausing before restart...", item.ItemName);
                        }
                    }
                }

                // Broadcast current snapshot
                var snapshot = GetSnapshot();
                await _hubContext.Clients.All.SendAsync("UpdateItems", snapshot, cancellationToken: stoppingToken);

                // Pause for 1 second per tick
                await Task.Delay(1000, stoppingToken);

                
                lock (_items)
                {
                    foreach (var item in _items.Values.Where(i => !i.Running))
                    {
                        // Pause for 10 seconds before restarting
                        if (!item.LastBids.Contains("PAUSE_MARKER"))
                        {
                            item.LastBids.Add("PAUSE_MARKER"); // marker to track paused state
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(10000, stoppingToken); // 10s pause
                                lock (_items)
                                {
                                    item.CurrentHighest = Math.Round((decimal)(_rng.NextDouble() * 50 + 10), 2);
                                    item.HighestBidder = "StartingPrice";
                                    item.EndsAt = DateTime.UtcNow.AddSeconds(_rng.Next(20, 40));
                                    item.Running = true;
                                    item.LastBids.Clear();
                                    _logger.LogInformation("{Item} restarted with new auction", item.ItemName);
                                }
                            }, stoppingToken);
                        }
                    }
                }
            }
        }

    }
}
