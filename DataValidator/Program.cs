using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuantConnect;
using QuantConnect.Configuration;
using QuantConnect.Logging;

namespace DataValidator
{
    public class Program
    {
        private static readonly string InputDirectory = Config.Get("data-input-directory");

        public static void Main(string[] args)
        {
            //Sets up email credentials
            var host = Config.Get("mail-host");
            var username = Config.Get("mail-username");
            var password = Config.Get("mail-password");
            var mail = new Mail(host, username, password);

            var outputErrors = "";
            
            var priceDeltaPercent = Config.GetInt("price-delta-percent");

            //Validates price jumps
            var symbolPaths = GetSymbolPath(InputDirectory, Resolution.Daily);
            var priceJumpValidator = new PriceJumpValidator(symbolPaths, priceDeltaPercent);
            var priceJumpErrors = priceJumpValidator.Process();
            
            //Validates missing dates
            var symbolPathsTick = GetSymbolPath(InputDirectory, Resolution.Tick);
            var missingDatesValidator = new MissingDatesValidator(symbolPathsTick);
            var missingDatesErrors = missingDatesValidator.Process();

            if(priceJumpErrors != "") outputErrors += "Symbols and Dates with price jumps are: \n" + priceJumpErrors + "\n\n";
            if (missingDatesErrors != "") outputErrors += "Symbols and Dates with missing dates are: \n" + missingDatesErrors;

//          Sends an e-mail if any errors are found
            if(outputErrors != "")
                mail.Error("Data Validator Error Report", outputErrors);
        }

        private static IEnumerable<string> GetSymbolPath(string directory, Resolution resolution)
        {
            var securityTypes = Enum.GetValues(typeof(SecurityType)).OfType<SecurityType>().ToArray();
            foreach (var securityType in securityTypes)
            {
                if(securityType == SecurityType.Option) continue;

                var securityTypeDirectoryPath = Path.Combine(InputDirectory, securityType.ToString().ToLower());
                if (!Directory.Exists(securityTypeDirectoryPath))
                {
                    Log.Error("Security does not exist: " + securityTypeDirectoryPath);
                    continue;
                }

                var markets = Directory.EnumerateDirectories(securityTypeDirectoryPath);

                foreach (var market in markets)
                {
                    var resolutionDirectoryPath = Path.Combine(market, resolution.ToLower());
                    if (!Directory.Exists(resolutionDirectoryPath))
                    {
                        Log.Error("Directory does not exist: " + resolutionDirectoryPath);
                        continue;
                    }

                    IEnumerable<string> minuteSymbols;
                    if (resolution == Resolution.Daily || resolution == Resolution.Hour)
                    {
                        minuteSymbols = Directory.EnumerateFiles(market + "\\" + resolution.ToLower());
                    }
                    else
                    {
                        minuteSymbols = Directory.EnumerateDirectories(market + "\\" + resolution.ToLower());
                    }
                    
                    foreach (var symbol in minuteSymbols)
                    {
                        yield return symbol;
                    }
                }
            }
        }
    }
}
