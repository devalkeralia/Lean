using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuantConnect;
using QuantConnect.Logging;
using RestSharp;
using RestSharp.Deserializers;

namespace BenzingaApi
{
    class Program
    {
        static void Main(string[] args)
        {
            DividendsJson dividendsList = null;
            SplitsJson splitsList = null;
            var restClient = new RestClient("http://api.benzinga.com/api/v2/");
            restClient.ClearHandlers();
            restClient.AddHandler("application/json", new JsonDeserializer());

            // For Dividends
            var restRequestDividends = new RestRequest("calendar/dividends", Method.GET);
            var pageCountDividends = 1;

            restRequestDividends.AddParameter("token", "a50191ff74314687a4ceb5f293695502");
            restRequestDividends.AddParameter("page", 3);
            restRequestDividends.AddParameter("parameters[date_from]", DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));
            restRequestDividends.AddParameter("pagesize", 1000);

            var restResponseDividends = restClient.Execute(restRequestDividends);

            if (restResponseDividends.Content == "[]")
            {
                Log.Error("No Data returned for Dividends");
            }
            else
            {
                dividendsList = JsonConvert.DeserializeObject<DividendsJson>(restResponseDividends.Content);
            }

            while (dividendsList != null && dividendsList.Dividends.Count % 1000 == 0)
            {
                restRequestDividends.Parameters.Find(x => x.Name == "page").Value = pageCountDividends;
                restResponseDividends = restClient.Execute(restRequestDividends);
                if (restResponseDividends.Content != "[]")
                    dividendsList.Dividends.AddRange(JsonConvert.DeserializeObject<DividendsJson>(restResponseDividends.Content).Dividends);
                pageCountDividends++;
            }

            if (dividendsList != null)
            {
                foreach (var dividend in dividendsList.Dividends)
                {
                    Log.Debug("Dividends: " + dividend.Ticker + " last updated at " + Time.UnixTimeStampToDateTime(Convert.ToDouble(dividend.Updated)));
                }
            }

            // For Splits
            var restRequestSplits = new RestRequest("calendar/splits", Method.GET);
            var pageCountSplits = 1;

            restRequestSplits.AddParameter("token", "a50191ff74314687a4ceb5f293695502");
            restRequestSplits.AddParameter("page", 0);
            restRequestSplits.AddParameter("parameters[date_from]", DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));
            restRequestSplits.AddParameter("pagesize", "1000");

            var restResponseSplits = restClient.Execute(restRequestSplits);

            if (restResponseSplits.Content == "[]")
            {
                Log.Error("No Data returned for Splits");
            }
            else
            {
                splitsList = JsonConvert.DeserializeObject<SplitsJson>(restResponseSplits.Content);
            }

            while (splitsList != null && splitsList.Splits.Count % 1000 == 0)
            {
                restRequestSplits.Parameters.Find(x => x.Name == "page").Value = pageCountSplits;
                restResponseSplits = restClient.Execute(restRequestDividends);
                if (restResponseSplits.Content != "[]")
                    splitsList.Splits.AddRange(JsonConvert.DeserializeObject<SplitsJson>(restResponseSplits.Content).Splits);
                pageCountSplits++;
            }

            if (splitsList != null)
            {
                foreach (var splits in splitsList.Splits)
                {
                    Log.Debug("Splits: " + splits.Ticker + " last updated at " + Time.UnixTimeStampToDateTime(Convert.ToDouble(splits.Updated)));
                }
            }

        }
    }
}
