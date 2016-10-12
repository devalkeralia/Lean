using System;
using System.Collections.Generic;
using System.IO;
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
            Parallel.ForEach(_symbolPaths, symbolPath =>
            {
                var files = Directory.EnumerateFiles(symbolPath);
                var symbol = new DirectoryInfo(symbolPath);
                var faultyDataList = new List<string>();
                var overnightPrice = 0m;
                var lastLine = "";

                foreach (var file in files)
                {
                    var isOvernightChecked = false;
                    var isFileDetected = false;
                    var previousPrice = 0m;
                    var zipInfo = new FileInfo(file);
                    using (var fileStream = File.OpenRead(zipInfo.FullName))
                    using (var reader = Compression.UnzipStream(fileStream))
                    {
                        if (reader == null) Console.WriteLine("READER WAS NULL!!!!" + zipInfo);
                        string line;
                        while (reader != null && (line = reader.ReadLine()) != null)
                        {
                            var csv = line.Split(',');
                            var price = Convert.ToDecimal(csv[1]);

                            //Overnight Price check
                            if (overnightPrice != 0m && !isOvernightChecked)
                            {
                                isOvernightChecked = true;
                                if (IsPriceDeltaLarge(overnightPrice, price, _priceDeltaPercent))
                                {
                                    if (!isFileDetected)
                                        faultyDataList.Add(symbol.Name + " - " + zipInfo.Name.Substring(0,8));
                                    isFileDetected = true;
                                }
                            }

                            //Check between first two entries of the file
                            if (previousPrice == 0)
                            {
                                line = reader.ReadLine();
                                var csv2 = line.Split(',');
                                previousPrice = Convert.ToDecimal(csv2[1]);

                                if (IsPriceDeltaLarge(price, previousPrice, _priceDeltaPercent))
                                {
                                    if (!isFileDetected)
                                        faultyDataList.Add(symbol.Name + " - " + zipInfo.Name.Substring(0, 8));
                                    isFileDetected = true;
                                }
                            }
                            // Check for the rest of the entries
                            else
                            {
                                if (IsPriceDeltaLarge(price, previousPrice, _priceDeltaPercent))
                                {
                                    if (!isFileDetected)
                                        faultyDataList.Add(symbol.Name + " - " + zipInfo.Name.Substring(0, 8));
                                    isFileDetected = true;
                                }
                            }
                            previousPrice = price;

                            //Takes the last minute data from the stream
                            if (reader.Peek() == -1)
                            {
                                lastLine = line;
                            }
                        }
                        var csvOvernight = lastLine.Split(',');
                        overnightPrice = Convert.ToDecimal(csvOvernight[1]);
                        Log.LogHandler.Debug("Done with file " + file);
                    }
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
