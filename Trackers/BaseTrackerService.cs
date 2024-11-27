using System.Diagnostics;
using System.Reflection;
using MagitekStratagemServer.Attributes;

namespace MagitekStratagemServer.Trackers.Update;

internal abstract class BaseTrackerService : ITrackerService
{
    private Thread? updateThread;
    private CancellationTokenSource? cancellationToken;
    protected readonly ILoggerFactory loggerFactory;
    protected readonly ILogger logger;

    public string Name => GetType().GetCustomAttribute<TrackerServiceAttribute>()?.Name ?? GetType().Name;

    public virtual bool IsTracking { get; protected set; }

    public virtual long LastGazeTimestamp { get; protected set; }

    public virtual float LastGazeX { get; protected set; }

    public virtual float LastGazeY { get; protected set; }

    private int rate = 1000 / 120;

    public BaseTrackerService(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
        logger = loggerFactory.CreateLogger(Name);
        logger.LogInformation($"{Name} Initialized");
    }

    protected void Start(Action<ITrackerService> callback)
    {
        if (updateThread != null)
        {
            cancellationToken!.Cancel();
            updateThread.Join();
        }

        cancellationToken = new CancellationTokenSource();
        updateThread = new Thread(() =>
        {
            logger.LogTrace("Update Thread Started");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!cancellationToken.Token.IsCancellationRequested)
            {
                stopwatch.Restart();
                DoUpdate();
                callback(this);
                var sleepTime = rate - (stopwatch.ElapsedTicks / 1000);
                if (sleepTime > 0)
                {
                    Thread.Sleep((int)sleepTime);
                }
            }
            logger.LogTrace("Update Thread Finished");
        });
        updateThread.Start();
    }

    protected void Stop()
    {
        if (updateThread != null)
        {
            cancellationToken!.Cancel();
            try
            {
                updateThread.Join();
            }
            catch (ThreadStateException)
            {
                logger.LogWarning("ThreadStateException caught");
            }
        }
    }

    protected abstract void DoUpdate();

    public abstract void DoStartTracking();

    public abstract void DoStopTracking();

    public abstract void DoDispose();

    public void Dispose()
    {
        logger.LogInformation($"Disposing {Name} Service");
        Stop();
        DoDispose();
    }

    public void StartTracking(Action<ITrackerService> callback)
    {
        logger.LogInformation($"Starting {Name} Tracking");
        DoStartTracking();
        Start(callback);
    }

    public void StopTracking()
    {
        logger.LogInformation($"Stopping {Name} Tracking");
        Stop();
        DoStopTracking();
    }
}
