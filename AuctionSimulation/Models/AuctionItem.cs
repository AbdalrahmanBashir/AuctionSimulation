namespace AuctionSimulation.Models
{
    public class AuctionItem
    {
        public string ItemId { get; set; } = "";
        public string ItemName { get; set; } = "";
        public decimal CurrentHighest { get; set; }
        public string HighestBidder { get; set; } = "";
        public DateTime EndsAt { get; set; }
        public bool Running { get; set; } = true;
        public List<string> LastBids { get; set; } = new();
    }
}
