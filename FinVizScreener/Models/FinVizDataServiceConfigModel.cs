using FinVizScreener.DB;

namespace FinVizScreener.Models
{
    public class FinVizDataServiceConfigModel
    {
        public string EndpointUrl { get; set; }
        public TimeSpan DataFetchPeriod { get; set; }
        public TimeSpan StartTime { get; set; }
        public IFinvizDBAdapter Db {  get; set; }
    }
}
