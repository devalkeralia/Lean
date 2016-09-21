using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using QuantConnect;
using QuantConnect.Brokerages.Fxcm;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.ToolBox;

namespace FxDataAdjuster
{
    public class Program
    {
        private static string _dataInputDirectory = Config.Get("data-input-directory");
        private static string _dataOutputDirectory = Config.Get("data-output-directory");
        private static string _csvMarkUpFile = Config.Get("mark-up-csv-file");

        public static void Main()
        {
            if (!Directory.Exists(_dataOutputDirectory)) Directory.CreateDirectory(_dataOutputDirectory);
            
            var markUpData = GetMarkUpDictionary();

            foreach (var symbolDirectory in Directory.EnumerateDirectories(_dataInputDirectory))
            {
                var symbol = symbolDirectory.Replace(_dataInputDirectory + "\\", "");
                var pipSpread = markUpData[symbol];
                var fxSymbol =
                    new FxcmSymbolMapper().GetLeanSymbol(FxcmSymbolMapper.ConvertLeanSymbolToFxcmSymbol(symbol.ToUpper()),
                        SecurityType.Forex, Market.FXCM);

                var parallelism = 4;
                var options = new ParallelOptions {MaxDegreeOfParallelism = parallelism};
                
                // set of files for each symbol
                var files = Directory.EnumerateFiles(symbolDirectory);

                var directory = symbolDirectory;
                Parallel.ForEach(files, options, file =>
                {
                    var date = DateTime.ParseExact(file.Replace(directory + "\\", "").Replace("_quote.zip", ""), DateFormat.EightCharacter, null);
                    var baseData = new List<BaseData>();
                    var fxDataWriter = new LeanDataWriter(Resolution.Tick, fxSymbol, _dataOutputDirectory, TickType.Quote);
                    using (var fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = Compression.UnzipStream(fileStream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (line == null) break;
                            var csv = line.Split(',');
                            var time = date + TimeSpan.FromMilliseconds(Convert.ToDouble(csv[0])); 
                            var newBid = Convert.ToDecimal(csv[1]) + pipSpread; 
                            var newAsk = Convert.ToDecimal(csv[2]) - pipSpread;

                            baseData.Add(new Tick(time, fxSymbol, newBid, newAsk));
                        }
                        fxDataWriter.Write(baseData);
                    }
                });

                Console.WriteLine("Done With " + symbol);
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static Dictionary<string, decimal> GetMarkUpDictionary()
        {
            var markUpFilestream = new StreamReader(_csvMarkUpFile);

            var markUpDictionary = new Dictionary<string, decimal>();

            // Skipping the header line
            markUpFilestream.ReadLine();

            string line;
            while ((line = markUpFilestream.ReadLine()) != null)
            {
                var lineSplit = line.Split(',');
                var symbol = lineSplit[0].Replace("/", "").ToLower(); 
                var pipSpread = Convert.ToDecimal(lineSplit[3]);
                pipSpread = symbol.Contains("jpy") ? pipSpread*0.01m : pipSpread*0.0001m;
                markUpDictionary.Add(symbol, pipSpread);
            }
            return markUpDictionary;
        }
    }
}
