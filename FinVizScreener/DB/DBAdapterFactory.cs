namespace FinVizScreener.DB
{
    public static class DBAdapterFactory
    {
        public static IFinvizDBAdapter Resolve(string dbAdapterType, string dbConnectionString)
        {
            switch (dbAdapterType)
            {
                case "LiteDB": return new LocalLiteDBFinvizAdapter(dbConnectionString);
                case "LocalJSON": return new LocalJSONFinvizDBAdapter(dbConnectionString);
                default: return new LocalLiteDBFinvizAdapter(dbConnectionString);
            }
        }
    }
}
