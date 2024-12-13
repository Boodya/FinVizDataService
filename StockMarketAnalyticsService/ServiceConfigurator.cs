﻿using StockMarketServiceDatabase.Models.FinViz;
using FinVizScreener.Services;
using Serilog;
using StockMarketAnalyticsService.Services;
using StockMarketServiceDatabase.Models.User;
using StockMarketServiceDatabase.Services.User;
using StockMarketDataProcessing.Services;
using StockMarketDataProcessing.Processors.FilterResults;
using StockMarketServiceDatabase.Services.FinViz;
using StockMarketServiceDatabase.Services.Query;
using Microsoft.Extensions.DependencyInjection;
using StockMarketDataProcessing.Processors.FilterQuery;

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
                return DBAdapterFactory.Resolve(
                    finVizConfig.DatabaseType,
                    finVizConfig.DatabaseConnectionString);
            });
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
            builder.Services.AddSingleton<IUserQueriesDataService>(provider =>
            {
                if (userDataConfig.DatabaseType == "LiteDB")
                    return new LiteDBUserQueriesService(userDataConfig.DatabaseConnectionString);
                throw new Exception($"Unable to initialize user queries data service - " +
                    $"unknown dbtype [{userDataConfig.DatabaseType}]");
            });

            builder.Services.AddSingleton(provider =>
            {
                var userQueriesService = provider.GetRequiredService<IUserQueriesDataService>();
                var finVizDb = provider.GetRequiredService<IFinvizDBAdapter>();
                var finvizProcessor = new FinVizDataIncrementalFilterProcessor(
                    finVizDb,
                    userQueriesService,
                    new MapBasedLinqQueryProcessor<FinVizDataItem>("ItemProperties")
                );

                return new FilterCalculationService(
                    finvizProcessor, userQueriesService,
                    provider.GetRequiredService<ILogger<FilterCalculationService>>(),
                    userDataConfig);
            });

            builder.Services.AddDistributedMemoryCache(); // Use in-memory cache for session storage
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        public static void PostConfigure(WebApplication app)
        {
            _ = app.Services.GetRequiredService<StockScreenerService>();
            _ = app.Services.GetRequiredService<FilterCalculationService>();
        }
    }
}
