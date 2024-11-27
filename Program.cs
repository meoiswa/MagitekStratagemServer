using System.Runtime.InteropServices;
using MagitekStratagemServer.Hubs;
using MagitekStratagemServer.Services;
using MagitekStratagemServer.Trackers.Eyeware.Bindings;
using MagitekStratagemServer.Trackers.Tobii.Bindings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
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
    } else if (libraryName == TrackerClient.Library)
    {
        return eyewareDllResolver.ResolveEyewareDll();
    }
    return IntPtr.Zero;
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapHub<MagitekStratagemHub>("/hub");

app.Run();
