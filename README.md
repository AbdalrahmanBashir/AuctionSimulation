
# Auction Simulation Dashboard with SignalR

A real-time auction simulation web application built with **ASP.NET Core**, **SignalR**, and **React**. The dashboard allows users to see live auction updates with automatic bidding bots and dynamic item updates.

## Features

* Real-time updates using **SignalR**.
* Auto-bidding simulation with random bidders and amounts.
* Continuous auction loop, items restart after ending with random values.
* Countdown timer and progress bars for each auction item.
* Professional, responsive dashboard layout in React.
* Skeleton loading and highlight animation on new high bids.

## Tech Stack

* **Backend:** ASP.NET Core, SignalR, C#
* **Frontend:** React, JavaScript, CSS
* **Communication:** WebSockets via SignalR
* **Hosting:** Runs locally with HTTPS support

## Getting Started

### Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
* [Node.js](https://nodejs.org/) and npm
* Modern browser with WebSocket support

### Running the Backend

```bash
cd AuctionSimulation
dotnet run
```

The backend runs on:

* HTTPS: `https://localhost:7129`
* HTTP: `http://localhost:5089`

### Running the Frontend

```bash
cd Frontend
npm install
npm run dev
```

Default frontend URL: `http://localhost:5173`

Make sure **CORS** in the backend allows this origin.

## Usage

* Open the frontend in your browser.
* Watch live auction items update in real time.
* Observe auto-bidder activity and countdown timers.
* Each auction item restarts automatically after ending.

