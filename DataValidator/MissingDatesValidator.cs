using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Util;

namespace DataValidator
{
    public class MissingDatesValidator
    {
        public string outputErrors = "";
        private readonly IEnumerable<string> _symbolPaths;
        private readonly string _outputDirectory;

        public MissingDatesValidator(IEnumerable<string> symbolPaths)
        {
            _symbolPaths = symbolPaths;
        }

        public string Process()
        {
            Parallel.ForEach(_symbolPaths, symbolPath =>
            {
                try
                {
                    var files = Directory.EnumerateFiles(symbolPath).ToList();
                    var missingDataList = new List<string>();

                    var symbolPathSplit = symbolPath.Split('\\');
                    var market = symbolPathSplit[symbolPathSplit.Length - 3];
                    var securityTypeValue = symbolPathSplit[symbolPathSplit.Length - 4];
                    var ticker = new DirectoryInfo(symbolPath).Name;
                    SecurityType securityType;
                    Enum.TryParse(securityTypeValue, true, out securityType);
                    var start = new FileInfo(files.First()).Name.Substring(0, 8);
                    var end = new FileInfo(files.Last()).Name.Substring(0, 8);

                    var dates = TradableDates.GetTradeableDates(start, end, securityType, ticker, market);

                    foreach (var date in dates)
                    {
                        var date1 = date;
                        var dateMatch = from file in files
                            where new FileInfo(file).Name.Substring(0, 8) == date1
                            select file;
                        if (dateMatch.IsNullOrEmpty())
                        {
                            missingDataList.Add(ticker + " - " + date);
                        }
                    }
                    outputErrors += string.Join("\n", missingDataList);
                }
                catch(Exception err)
                {
                    Log.Error(err, err.Message + " " + err.StackTrace);
                }
            });
            return outputErrors;
        }
    }
}
