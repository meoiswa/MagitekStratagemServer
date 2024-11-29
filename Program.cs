using System.Runtime.InteropServices;
using MagitekStratagemServer.Hubs;
using MagitekStratagemServer.Services;
using MagitekStratagemServer.Trackers.Eyeware.Bindings;
using MagitekStratagemServer.Trackers.Tobii.Bindings;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.Configure<JsonHubProtocolOptions>(options =>
{
    options.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, SourceGenerationContext.Default);
});


builder.Services.AddSingleton<ITrackerServiceProvider, TrackerServiceProvider>();
builder.Services.AddSingleton<TobiiDllResolver>();
builder.Services.AddSingleton<EyewareDllResolver>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var tobiiDllResolver = app.Services.GetRequiredService<TobiiDllResolver>();
var eyewareDllResolver = app.Services.GetRequiredService<EyewareDllResolver>();

NativeLibrary.SetDllImportResolver(typeof(Program).Assembly, (libraryName, assembly, searchPath) =>
{
    logger.LogTrace($"Resolving {libraryName} for {assembly.FullName} from {searchPath}");
    if (libraryName == StreamEngine.Library)
    {
        return tobiiDllResolver.ResolveTobiiGameIntegrationDll();
    }
    else if (libraryName == TrackerClient.Library)
    {
        return eyewareDllResolver.ResolveEyewareDll();
    }
    return IntPtr.Zero;
});

app.UseDefaultFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapHub<MagitekStratagemHub>("/hub");

app.Run();
