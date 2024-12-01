using FinVizScreener.DB;
using FinVizScreener.Models;
using FinVizScreener.Services;
using Serilog;
using StockMarketAnalyticsService.Services;
using StockMarketServiceDatabase.Models;
using StockMarketServiceDatabase.Services;

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
                throw new Exception("Unnable to parse finviz service database connection string from config file.");

            builder.Services.AddSingleton(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<FinVizScrapperService>>();
                return new FinVizScrapperService(finVizConfig, logger);
            });
            builder.Services.AddSingleton<StockScreenerService>();

            var userDataConfig = new UserDataServiceConfigModel();
            builder.Configuration.GetSection("UserDataServiceConfigModel").Bind(userDataConfig);
            if (string.IsNullOrEmpty(userDataConfig.DatabaseConnectionString))
                throw new Exception("Unnable to parse user data service database connection string from config file.");
            builder.Services.AddSingleton<IUserDataService>(provider =>
            {
                if (userDataConfig.DatabaseType == "LiteDB")
                    return new LiteDBUserDataService(userDataConfig.DatabaseConnectionString);
                throw new Exception($"Unable to initialize user data service - " +
                    $"unknown dbtype [{userDataConfig.DatabaseType}]");
            });

            builder.Services.AddDistributedMemoryCache(); // Use in-memory cache for session storage
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        public static void PostConfigure(IServiceProvider app)
        {
            _ = app.GetRequiredService<StockScreenerService>();
        }
    }
}
