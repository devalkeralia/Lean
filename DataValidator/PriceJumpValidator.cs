using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuantConnect;
using QuantConnect.Logging;

namespace DataValidator
{
    public class PriceJumpValidator
    {
        public string outputErrors = "";
        private readonly IEnumerable<string> _symbolPaths;
        private readonly int _priceDeltaPercent;

        public PriceJumpValidator(IEnumerable<string> symbolPaths, int priceDeltaPercent)
        {
            _symbolPaths = symbolPaths;
            _priceDeltaPercent = priceDeltaPercent;
        }

        public string Process()
        {
            Parallel.ForEach(_symbolPaths, file =>
            {
                if(!file.Contains("equity")) return;

                var symbol = new FileInfo(file).Name.Replace(".zip", "");
                var first = false;
                var faultyDataList = new List<string>();
                string firstDate;
                var yahooDataCollector = new YahooDataCollector();
                var isDataCollectedSuccessfully = false;
                var zipInfo = new FileInfo(file);
                using (var fileStream = File.OpenRead(zipInfo.FullName))
                using (var reader = Compression.UnzipStream(fileStream))
                {
                    if (reader == null) Console.WriteLine("READER WAS NULL!!!!" + zipInfo);
                    string line;
                    while (reader != null && (line = reader.ReadLine()) != null)
                    {
                        var csv = line.Split(',');
                        var price = Convert.ToDecimal(csv[4]);
                        var date = csv[0].Substring(0, 8);

                        if (!first)
                        {
                            firstDate = csv[0].Substring(0, 8);
                            isDataCollectedSuccessfully = yahooDataCollector.GetStockPriceHistory(symbol, 0, DateTime.ParseExact(firstDate, "yyyyMMdd", null), DateTime.Today.AddDays(-1));
                            first = true;
                        }

                        if (!isDataCollectedSuccessfully)
                        {
                            Log.Error("Expired or not data found for " + symbol);
                            break;
                        }

                        if (!yahooDataCollector.CloseData.ContainsKey(date))
                        {
                            continue;
                        }

                        var yahooPrice = Convert.ToDecimal(yahooDataCollector.CloseData[date]);

                        if (IsPriceDeltaLarge(price, yahooPrice, _priceDeltaPercent))
                        {
                            faultyDataList.Add(symbol + " - " + date);
                        }
                    }
                    Log.LogHandler.Debug("Done with file " + file);
                }
                outputErrors += string.Join("\n", faultyDataList);
            });

            return outputErrors;
        }

        private static bool IsPriceDeltaLarge(decimal price1, decimal price2, int priceDeltaPercent)
        {
            var delta = Math.Abs(((price2 - price1) / price1) * 100);

            return delta > priceDeltaPercent;
        }
    }
}
