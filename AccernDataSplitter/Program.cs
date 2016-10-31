﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using QuantConnect;
using QuantConnect.Configuration;
using QuantConnect.Logging;
using QuantConnect.Util;

namespace AccernDataSplitter
{
    public class Program
    {
        private static readonly string BaseFolder = Config.Get("accern-data-folder");
        private static readonly string CacheDirectory = Config.Get("data-cache-directory");
        private static int _count;

        public static void Main()
        {
            var month = DateTime.MinValue;
            var symbolDictionary = new Dictionary<string, List<string>>(); 
            foreach (var fileName in Directory.EnumerateFiles(BaseFolder))
            {
                Log.Trace("Extracting " + new FileInfo(fileName).Name);
                var file = Compression.UnGZip(fileName, CacheDirectory);
                Log.Trace("Extraction Completed Successfully");
                
                var reader = new StreamReader(file);

                //Skipping Headings Line
                reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSplit = line.Split(',');
                    var date = DateTime.ParseExact(lineSplit[2], "yyyy-MM-dd HH:mm:ss UTC", CultureInfo.InvariantCulture);
                    var symbol = lineSplit[4];

                    if (month == DateTime.MinValue)
                    {
                        month = date;
                    }

                    //First time the symbol is detected for the month
                    if (!symbolDictionary.ContainsKey(symbol))
                    {
                        symbolDictionary.Add(symbol, new List<string>());
                    }
                    
                    if (month.Month != date.Month && symbolDictionary.ContainsKey(symbol))
                    {
                        WriteLinesToDisk(month, symbolDictionary);

                        Log.Trace("Done with month {0} and year {1}", month.Month, month.Year);

                        month = date;
                    }

                    symbolDictionary[symbol].Add(line);
                }

                Log.Trace("Done With the file {0}" , new FileInfo(fileName).Name);
                
                reader.Close();
                File.Delete(file);            
            }

            //At the end of the file write the data that is left
            WriteLinesToDisk(month, symbolDictionary);

            Log.Trace("Done writing a total of {0} files", _count);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

        }

        /// <summary>
        /// Dumps all the processed lines into appropriate files and zips them
        /// </summary>
        /// <param name="month">The month for the processed data</param>
        /// <param name="symbolDictionary">Dictionary of symbol, List of lines</param>
        public static void WriteLinesToDisk(DateTime month, Dictionary<string, List<string>> symbolDictionary)
        {
            foreach (var symbol in symbolDictionary.Keys)
            {
                if (!symbolDictionary[symbol].IsNullOrEmpty())
                {
                    if (!Directory.Exists(GenerateDirectoryPath(symbol)))
                        Directory.CreateDirectory(GenerateDirectoryPath(symbol));

                    var filePath = GenerateFilePath(symbol, month);

                    File.AppendAllLines(filePath, symbolDictionary[symbol]);
                    Compression.Zip(filePath);

                    if (_count % 10000 == 0 && _count > 0) Log.Trace("Done writing {0} files", _count);
                    _count++;

                    symbolDictionary[symbol].Clear();
                }
            }
        }

        public static string GenerateDirectoryPath(string symbol)
        {
            return Path.Combine(Globals.DataFolder, "news", "accern", symbol.ToLower());
        }

        public static string GenerateFilePath(string symbol, DateTime date)
        {
            return Path.Combine(GenerateDirectoryPath(symbol), date.ToString("yyyyMM") + ".csv");
        }
    }
}
