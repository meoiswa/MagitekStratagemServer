using Microsoft.AspNetCore.SignalR;
using MagitekStratagemServer.Services;
using Microsoft.Extensions.ObjectPool;

namespace MagitekStratagemServer.Hubs
{
    public class MagitekStratagemHub : Hub
    {
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly ITrackerServiceProvider trackerServiceProvider;
        private readonly IHubContext<MagitekStratagemHub> hubContext;

        public MagitekStratagemHub(
            ILoggerFactory loggerFactory,
            ITrackerServiceProvider trackerServiceProvider,
            IHubContext<MagitekStratagemHub> hubContext
        )
        {
            logger = loggerFactory.CreateLogger<MagitekStratagemHub>();
            this.loggerFactory = loggerFactory;
            this.trackerServiceProvider = trackerServiceProvider;
            this.hubContext = hubContext;
        }

        public async Task StartTracking(string fullName)
        {
            logger.LogInformation($"{Context.ConnectionId}: StartTracking: {fullName}");
            var trackerService = trackerServiceProvider.GetTracker(fullName);

            if (trackerService == null || trackerService.IsTracking)
            {
                return;
            }

            trackerService.StartTracking((tracker) =>
            {
                hubContext.Clients.All.SendAsync("TrackerUpdate", tracker.Name, tracker.LastGazeTimestamp, tracker.LastGazeX, tracker.LastGazeY);
            });

            if (trackerService.IsTracking)
            {
                await Clients.All.SendAsync("TrackingStarted", fullName);
            }
        }

        public async Task StopTracking(string fullName)
        {
            logger.LogInformation($"{Context.ConnectionId}: StopTracking: {fullName}");
            var trackerService = trackerServiceProvider.GetTracker(fullName);
            if (trackerService == null || !trackerService.IsTracking)
            {
                return;
            }

            trackerService.StopTracking();

            if (!trackerService.IsTracking)
            {
                await Clients.All.SendAsync("TrackingStopped", fullName);
            }
        }

        public async Task GetTrackerServices()
        {
            logger.LogInformation($"{Context.ConnectionId}: Getting Tracker Services");
            var implementations = trackerServiceProvider.ListTrackers()
                .Select(impl => new { impl.FullName, impl.Name });
            await Clients.Caller.SendAsync("TrackerServices", implementations);
        }
    }
}
