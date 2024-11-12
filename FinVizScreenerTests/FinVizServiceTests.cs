﻿using FinVizDataService.Models;
using FinVizScreener.DB;
using FinVizScreener.Services;

namespace FinVizScreenerTests
{
    public class FinVizServiceTests
    {
        private string _scrapeUrl = "https://finviz.com/screener.ashx?v=152&c=0,1,2,79,3,4,5,6,7,8,9,10,11,12,13,73,74,75,14,15,16,77,17,18,19,20,21,23,22,82,78,127,128,24,25,85,26,27,28,29,30,31,84,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,125,126,59,68,70,80,83,76,60,61,62,63,64,67,69,81,86,87,88,65,66,103,100,107,108,109,112,113,114,115,116,117,120,121,122,105";
        public FinVizServiceTests()
        {

        }

        [Fact]
        public async void OnServiceDataDownloadedTest()
        {
            var service = new FinvizScheduledScrapperService(new FinVizScreener.Models.FinVizDataServiceConfigModel()
            {
                EndpointUrl = _scrapeUrl,
                Db = new LocalJSONFinvizDBAdapter("tests-local-folder"),
                StartTime = TimeSpan.FromHours(08),
                DataFetchPeriod = TimeSpan.FromDays(1),
            });
            FinVizDataPack fetchedData=null;
            service.SubscribeOnDataUpdated((data) =>
            {
                fetchedData = data;
            });
            while (fetchedData == null)
                Thread.Sleep(1000);
            Assert.True(fetchedData != null);
            Assert.True(fetchedData.Items.Count() > 8000);
        }
    }
}
