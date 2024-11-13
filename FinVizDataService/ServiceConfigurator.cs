using FinVizScreener.DB;
using FinVizScreener.Models;
using FinVizScreener.Services;

namespace FinVizDataService
{
    internal static class ServiceConfigurator
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            var finVizConfig = new FinVizDataServiceConfigModel();
            builder.Configuration.GetSection("FinVizDataServiceConfigModel").Bind(finVizConfig);
            builder.Services.AddTransient<IFinvizDBAdapter>(provider =>
                new LocalLiteDBFinvizAdapter(finVizConfig.DatabaseConnectionString));
            builder.Services.AddSingleton(provider => 
                new FinvizScheduledScrapperService(finVizConfig));
        }

        public static void PostConfigure(IServiceProvider app)
        {
            var scrapper = app.GetService<FinvizScheduledScrapperService>();
        }
    }
}
