namespace StockMarketServiceDatabase.Services.FinViz
{
    public static class DBAdapterFactory
    {
        public static IFinvizDBAdapter Resolve(string dbAdapterType, string dbConnectionString)
        {
            switch (dbAdapterType)
            {
                case "LiteDB": return new LocalLiteDBAdapter(dbConnectionString);
                case "LiteDBSeparate": return new LocalLiteDBSeparateFilesAdapter(dbConnectionString);
                default: return new LocalLiteDBAdapter(dbConnectionString);
            }
        }
    }
}
