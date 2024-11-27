using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MagitekStratagemServer.Trackers.Eyeware.Bindings;

internal class EyewareDllResolver
{
    private ILoggerFactory LoggerFactory;
    private ILogger logger;

    public EyewareDllResolver(ILoggerFactory loggerFactory)
    {
        this.LoggerFactory = loggerFactory;
        this.logger = loggerFactory.CreateLogger<EyewareDllResolver>();
    }

    public IntPtr ResolveEyewareDll()
    {
        logger.LogInformation($"Searching for Eyeware Tracker Client DLL...");
        var runDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);

        if (runDir != null)
        {
            var lib = Path.Join(runDir, "Trackers", "Eyeware", "lib", TrackerClient.Library);
            logger.LogInformation("Loading Eyeware Tracker Client DLL from " + lib);
            try
            {
                return NativeLibrary.Load(lib);
            }
            catch (Exception e)
            {
                logger.LogError("Failed to load Eyeware Tracker Client DLL: " + e.Message);
            }
        }
        else
        {
            logger.LogInformation("Eyeware Tracker Client DLL not found.");
        }

        return IntPtr.Zero;
    }
}
