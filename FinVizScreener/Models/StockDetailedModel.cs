using FinVizScreener.Helpers;

namespace FinVizScreener.Models
{
    public class StockDetailedModel : StockOverviewModel, IStockModel
    {
        [MapToTitle("Index")]
        public string Index { get; set; }

        [MapToTitle("Market Cap")]
        public string? MarketCap { get; set; }

        [MapToTitle("Income")]
        public string? Income { get; set; }

        [MapToTitle("Sales")]
        public string? Sales { get; set; }

        [MapToTitle("Book/sh")]
        public string? BookPerShare { get; set; }

        [MapToTitle("Cash/sh")]
        public string? CashPerShare { get; set; }

        [MapToTitle("Dividend Est.")]
        public string DividendEstimate { get; set; }

        [MapToTitle("Dividend TTM")]
        public string DividendTTM { get; set; }

        [MapToTitle("Dividend Ex-Date")]
        public string DividendExDate { get; set; }

        [MapToTitle("Employees")]
        public string Employees { get; set; }

        [MapToTitle("Option/Short")]
        public string OptionShort { get; set; }

        [MapToTitle("Sales Surprise")]
        public string SalesSurprise { get; set; }

        [MapToTitle("SMA20")]
        public string SMA20 { get; set; }

        [MapToTitle("P/E")]
        public string? PE { get; set; }

        [MapToTitle("Forward P/E")]
        public string ForwardPE { get; set; }

        [MapToTitle("PEG")]
        public string? PEG { get; set; }

        [MapToTitle("P/S")]
        public string PS { get; set; }

        [MapToTitle("P/B")]
        public string PB { get; set; }

        [MapToTitle("P/C")]
        public string PC { get; set; }

        [MapToTitle("P/FCF")]
        public string PFCF { get; set; }

        [MapToTitle("Quick Ratio")]
        public string QuickRatio { get; set; }

        [MapToTitle("Current Ratio")]
        public string CurrentRatio { get; set; }

        [MapToTitle("Debt/Eq")]
        public string DebtEquity { get; set; }

        [MapToTitle("LT Debt/Eq")]
        public string LT_DebtEquity { get; set; }

        [MapToTitle("EPS (ttm)")]
        public string EPS_TTM { get; set; }

        [MapToTitle("EPS next Y")]
        public string EPS_NextYear { get; set; }

        [MapToTitle("EPS next Q")]
        public string EPS_NextQuarter { get; set; }

        [MapToTitle("EPS this Y")]
        public string EPS_ThisYear { get; set; }

        [MapToTitle("EPS next 5Y")]
        public string EPS_Next5Y { get; set; }

        [MapToTitle("EPS past 5Y")]
        public string EPS_Past5Y { get; set; }

        [MapToTitle("Sales past 5Y")]
        public string Sales_Past5Y { get; set; }

        [MapToTitle("EPS Y/Y TTM")]
        public string EPS_Y_Y_TTM { get; set; }

        [MapToTitle("Sales Y/Y TTM")]
        public string Sales_Y_Y_TTM { get; set; }

        [MapToTitle("EPS Q/Q")]
        public string EPS_Q_Q { get; set; }

        [MapToTitle("Sales Q/Q")]
        public string Sales_Q_Q { get; set; }

        [MapToTitle("Insider Own")]
        public string InsiderOwn { get; set; }

        [MapToTitle("Insider Trans")]
        public string InsiderTrans { get; set; }

        [MapToTitle("Inst Own")]
        public string InstOwn { get; set; }

        [MapToTitle("Inst Trans")]
        public string InstTrans { get; set; }

        [MapToTitle("ROA")]
        public string ROA { get; set; }

        [MapToTitle("ROE")]
        public string ROE { get; set; }

        [MapToTitle("ROI")]
        public string ROI { get; set; }

        [MapToTitle("Gross Margin")]
        public string GrossMargin { get; set; }

        [MapToTitle("Oper. Margin")]
        public string OperMargin { get; set; }

        [MapToTitle("Profit Margin")]
        public string ProfitMargin { get; set; }

        [MapToTitle("Payout")]
        public string Payout { get; set; }

        [MapToTitle("Earnings")]
        public string Earnings { get; set; }

        [MapToTitle("Shs Outstand")]
        public string ShsOutstand { get; set; }

        [MapToTitle("Shs Float")]
        public string ShsFloat { get; set; }

        [MapToTitle("Short Float")]
        public string ShortFloat { get; set; }

        [MapToTitle("Short Ratio")]
        public string ShortRatio { get; set; }

        [MapToTitle("Short stringerest")]
        public string Shortstringerest { get; set; }

        [MapToTitle("52W Range")]
        public string W52_Range { get; set; }

        [MapToTitle("52W High")]
        public string W52_High { get; set; }

        [MapToTitle("52W Low")]
        public string W52_Low { get; set; }

        [MapToTitle("Beta")]
        public string Beta { get; set; }

        [MapToTitle("ATR (14)")]
        public string ATR14 { get; set; }

        [MapToTitle("RSI (14)")]
        public string RSI14 { get; set; }

        [MapToTitle("Volatility")]
        public string Volatility { get; set; }

        [MapToTitle("Recom")]
        public string Recomm { get; set; }

        [MapToTitle("Rel Volume")]
        public string RelVolume { get; set; }

        [MapToTitle("Avg Volume")]
        public string AvgVolume { get; set; }

        [MapToTitle("Volume")]
        public string Volume { get; set; }

        [MapToTitle("Target Price")]
        public string TargetPrice { get; set; }

        [MapToTitle("Prev Close")]
        public string PrevClose { get; set; }

        [MapToTitle("Price")]
        public string Price { get; set; }

        [MapToTitle("Change")]
        public string Change { get; set; }

        [MapToTitle("Perf Week")]
        public string PerfWeek { get; set; }

        [MapToTitle("Perf Month")]
        public string PerfMonth { get; set; }

        [MapToTitle("Perf Quarter")]
        public string PerfQuarter { get; set; }

        [MapToTitle("Perf Half Y")]
        public string PerfHalfY { get; set; }

        [MapToTitle("Perf Year")]
        public string PerfYear { get; set; }

        [MapToTitle("Perf YTD")]
        public string PerfYTD { get; set; }

        public StockDetailedModel()
        {

        }

        public StockDetailedModel(StockOverviewModel model)
        {
            Ticker = model.Ticker;
            Url = model.Url;
            Company = model.Company;
            Sector = model.Sector;
            Industry = model.Industry;
            Country = model.Country;
            MarketCap = model.MarketCap;
            PERatio = model.PERatio;
            Price = model.Price;
            Change = model.Change;
            Volume = model.Volume;
        }
    }
}
