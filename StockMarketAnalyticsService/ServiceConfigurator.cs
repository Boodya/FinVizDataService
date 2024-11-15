using FinVizScreener.DB;
using FinVizScreener.Models;
using FinVizScreener.Services;
using Serilog;

namespace StockMarketAnalyticsService
{
    internal static class ServiceConfigurator
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            builder.Configuration
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.Host.UseSerilog((context, config) =>
            {
                config
                    .MinimumLevel.Information() 
                    .WriteTo.Console()
                    .WriteTo.File(
                        Path.Combine(AppContext.BaseDirectory, "logs", "log.txt"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
                    );
            });

            var finVizConfig = new FinVizDataServiceConfigModel();
            builder.Configuration.GetSection("FinVizDataServiceConfigModel").Bind(finVizConfig);
            if (string.IsNullOrEmpty(finVizConfig.DatabaseConnectionString))
                throw new Exception("Unnable to parse database connection string from config file.");
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
