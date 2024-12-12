namespace StockMarketServiceDatabase.Models.User
{
    public class UserDataServiceConfigModel
    {
        public string DatabaseConnectionString { get; set; }
        public string DatabaseType { get; set; }
        public TimeSpan QueryCalculationTime { get; set; }
        public bool QueryCalculationOnStart { get; set; }
    }
}
