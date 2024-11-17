namespace FinVizScreener.DB
{
    public static class DBAdapterFactory
    {
        public static IFinvizDBAdapter Resolve(string dbAdapterType, string dbConnectionString)
        {
            switch (dbAdapterType)
            {
                case "LiteDB": return new LocalLiteDBAdapter(dbConnectionString);
                case "LocalJSON": return new LocalJSONDBAdapter(dbConnectionString);
                case "LiteDBSeparate": return new LocalLiteDBSeparateFilesAdapter(dbConnectionString);
                default: return new LocalLiteDBAdapter(dbConnectionString);
            }
        }
    }
}
