using System.Reflection;
using MagitekStratagemServer.Attributes;
using MagitekStratagemServer.Trackers.Update;

namespace MagitekStratagemServer.Services
{
    public class TrackerServiceProvider : ITrackerServiceProvider, IDisposable
    {
        private Dictionary<string, ITrackerService> trackerServices = new();
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;

        public TrackerServiceProvider(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<TrackerServiceProvider>();

            logger.LogTrace("Tracker Service Provider Initialized");
        }

        public ITrackerService? GetTracker(string fullName)
        {
            logger.LogTrace($"Getting Tracker: {fullName}");
            var tracker = trackerServices.GetValueOrDefault(fullName);

            if (tracker == null)
            {
                var trackerServiceType = typeof(ITrackerService);
                var trackerType = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => trackerServiceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.IsAssignableTo(typeof(BaseTrackerService)))
                    .FirstOrDefault(t => t.FullName == fullName);
                if (trackerType != null)
                {
                    tracker = (ITrackerService?)Activator.CreateInstance(trackerType, [loggerFactory]);
                    if (tracker != null)
                    {
                        trackerServices.Add(fullName, tracker);
                    }
                }
            }

            logger.LogTrace($"Tracker: {tracker?.Name ?? "null"}");

            return tracker;
        }

        public IEnumerable<Type> ListTrackers()
        {
            logger.LogTrace("Listing Trackers");
            var trackerServiceType = typeof(ITrackerService);
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => trackerServiceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.IsAssignableTo(typeof(BaseTrackerService)));

            logger.LogTrace($"Found {types.Count()} Trackers");
            return types;
        }

        public void Dispose()
        {
            foreach (var tracker in trackerServices.Values)
            {
                tracker.Dispose();
            }
            trackerServices.Clear();
        }
    }
}
