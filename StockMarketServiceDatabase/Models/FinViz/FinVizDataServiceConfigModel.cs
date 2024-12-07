namespace StockMarketServiceDatabase.Models.FinViz
{
    public class FinVizDataServiceConfigModel
    {
        public string EndpointUrl { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public string DatabaseType {  get; set; }
        public string DatabaseConnectionString { get; set; }
        public bool IsSyncOnStart { get; set; }
    }
}
