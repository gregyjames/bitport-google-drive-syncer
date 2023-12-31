using Autofac;
using BitPortLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace bitport_google_drive_syncer;

public static class ClientBuilder
{
    public static BitPortClient client { get; set; }
    
    public static async Task<HashSet<(string name, string code)>> getFolders()
    {
        var folders = (await client.ByPath("")).data;
        var folders_cache = folders.SelectMany(x => x.folders).Select(x => (x.name, x.code)).ToHashSet();
        return folders_cache;
    }
    public static GoogleDriveConfig Build()
    {
        var google = new GoogleDriveConfig();
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json");
        var configuration = configurationBuilder.Build();
        configuration.Bind(google);
        var seriLog = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Async(a => a.Console(theme: AnsiConsoleTheme.Sixteen))
            .CreateLogger();
        var factory = LoggerFactory.Create(logging =>
        {
            logging.AddSerilog(seriLog);
        });

        var builder = new ContainerBuilder();
        builder.RegisterType<SyncJob>().As<IJob>();
        builder.RegisterInstance(configuration).As<IConfiguration>();
        builder.RegisterInstance(factory).As<ILoggerFactory>();
        builder.RegisterModule(File.Exists("token.json")
            ? new AutoFacModule(AuthorizationTypes.USER_CODE_FILE_AUTH)
            : new AutoFacModule(AuthorizationTypes.USER_CODE_AUTH));
        var ctx = builder.Build();
        client = ctx.Resolve<BitPortClient>();

        return google;
    }
}