using FinVizScreener.DB;
using FinVizScreener.Models;
using FinVizScreener.Services;
using Serilog;

namespace FinVizDataService
{
    internal static class ServiceConfigurator
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, config) =>
            {
                config
                    .MinimumLevel.Information() 
                    .WriteTo.Console()
                    .WriteTo.File(
                        Path.Combine(AppContext.BaseDirectory, "logs", "log.txt"),
                        rollingInterval: RollingInterval.Day, // Creates a new log file each day
                        retainedFileCountLimit: 7,            // Retains log files for the last 7 days
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
                    );
            });

            var finVizConfig = new FinVizDataServiceConfigModel();
            builder.Configuration.GetSection("FinVizDataServiceConfigModel").Bind(finVizConfig);
            builder.Services.AddTransient<IFinvizDBAdapter>(provider =>
                new LocalLiteDBFinvizAdapter(finVizConfig.DatabaseConnectionString));

            builder.Services.AddSingleton(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<FinVizScrapperService>>();
                return new FinVizScrapperService(finVizConfig, logger);
            });
        }

        public static void PostConfigure(IServiceProvider app)
        {
            _ = app.GetRequiredService<FinVizScrapperService>();
        }
    }
}
