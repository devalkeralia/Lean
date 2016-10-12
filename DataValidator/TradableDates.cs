using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Securities;
using QuantConnect.Securities.Cfd;
using QuantConnect.Securities.Equity;
using QuantConnect.Securities.Forex;

namespace DataValidator
{
    class TradableDates
    {
        public static List<string> GetTradeableDates(string startDate, string endDate, SecurityType securityType, string ticker,
            string market)
        {
            var start = DateTime.ParseExact(startDate, "yyyyMMdd", null);
            var end = DateTime.ParseExact(endDate, "yyyyMMdd", null);
            var tradeableTotal = 0;
            var tradeableDates = new List<string>();
            var symbol = Symbol.Create(ticker, securityType, market);
            var marketHoursDatabase = MarketHoursDatabase.FromDataFolder();
            var marketHoursDbEntry = marketHoursDatabase.GetEntry(symbol.ID.Market, symbol.Value, symbol.ID.SecurityType);

            SecurityExchange exchange;
            try
            {
                switch (securityType)
                {
                    default:
                    case SecurityType.Equity:
                        exchange = new EquityExchange();
                        break;
                    case SecurityType.Forex:
                        exchange = new ForexExchange(marketHoursDbEntry.ExchangeHours);
                        break;
                    case SecurityType.Cfd:
                        exchange = new CfdExchange(marketHoursDbEntry.ExchangeHours);
                        break;
                    case SecurityType.Future:
                        exchange = null;
                        break;
                }

                foreach (var day in Time.EachDay(start, end))
                {
                    if (exchange != null && exchange.IsOpenDuringBar(day.Date, day.Date.AddDays(1), false))
                    {
                        tradeableTotal++;
                        var date = day.Date.ToString("yyyyMMdd");
                        tradeableDates.Add(date);
                    }
                }
            }
            catch (Exception err)
            {
                Log.Error(err, err.Message + " " + err.StackTrace);
            }
            return tradeableDates;
        }
    }
}
