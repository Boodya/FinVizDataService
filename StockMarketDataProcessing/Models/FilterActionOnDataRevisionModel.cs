using StockMarketServiceDatabase.Models.FinViz;

namespace StockMarketDataProcessing.Models
{
    internal enum OperationAction
    {
        Buy,
        Sell
    }
    internal class FilterActionOnDataRevisionModel
    {
       
        public int DataRevision { get; set; }
        public List<FinVizDataItem> QueriedData { get; set; }
        public List<ActionModel> Actions { get; set; }
    }

    internal class ActionModel
    {
        public string Ticker { get; set; }
        public OperationAction Action { get; set; }
        public decimal Price { get; set; }
    }
}
