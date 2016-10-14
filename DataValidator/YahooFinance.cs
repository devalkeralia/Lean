using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;

namespace DataValidator
{
    public class YahooDataCollector
    {
        private const string HistoryUrl =
            "http://ichart.finance.yahoo.com/table.csv?s={0}&a={1}&b={2}&c={3}&d={4}&e={5}&f={6}&g=d&ignore.csv";

        public Dictionary<string, List<YahooFinance>> StockDataCollection = new Dictionary<string, List<YahooFinance>>();

        public Dictionary<string, long> CloseData = new Dictionary<string, long>();
        public List<double> Close = new List<double>();

        private static string GetResponse(string url, string symbol)
        {
            using (var client = new WebClient())
            {
                try
                {
                    return client.DownloadString(url);
                }
                catch (Exception err)
                {
                    return "";
                }
            }
        }

        public bool GetStockPriceHistory(string code, long exchangeId, DateTime fromDate, DateTime toDate)
        {
            CloseData.Clear();
            var now = DateTime.Now;
            if (fromDate >= now)
            {
                return false;
            }

            var url = string.Format(HistoryUrl, code, fromDate.Month - 1, fromDate.Day - 5, fromDate.Year, toDate.Month - 1, toDate.Day, toDate.Year);
            const int index = 0;
            var provider = new CultureInfo("en-US", true);
            var response = GetResponse(url, code);
            if(response == "") return false;

            try
            {
                var list2 = new List<YahooFinance>();
                using (var reader = new StringReader(response))
                {
                    reader.ReadLine();
                    while (reader.Peek() > -1)
                    {
                        string[] strArray = reader.ReadLine().Split(',');
                        DateTime time2 = DateTime.Parse(strArray[index].Replace("\"", ""), provider);
                        if (time2 < fromDate) continue;
                        var yahooItem = new YahooFinance
                                            {
                                                Date = time2,
                                                Open = Convert.ToDecimal(strArray[1 + index]),
                                                High = Convert.ToDecimal(strArray[2 + index]),
                                                Low = Convert.ToDecimal(strArray[3 + index]),
                                                Close = Convert.ToDecimal(strArray[4 + index]),
                                                Volume = Convert.ToInt64(strArray[5 + index]),
                                                AjustedClose = Convert.ToDecimal(strArray[6 + index])
                                            };
                        list2.Add(yahooItem);

                        var close = Convert.ToInt64(yahooItem.Close*10000);
                        Close.Add(close);
                        CloseData.Add(time2.ToString("yyyyMMdd"), close);
                    }
                }
                return true;
            }
            catch (Exception err)
            {
                // ignored
            }
            return true;
        }
    }


    public struct YahooFinance
    {
        public string Symbol;
        public decimal Open;
        public decimal Close;
        public decimal High;
        public decimal Low;
        public decimal Volume;
        public decimal AjustedClose;
        public DateTime Date;
    }
}