using System.Collections.Generic;
using Newtonsoft.Json;

namespace BenzingaApi
{
    public class DividendsJson
    {
        [JsonProperty(PropertyName = "dividends")]
        public List<Dividends> Dividends;
    }

    public class Dividends
    {
        [JsonProperty(PropertyName = "id")]
        public string Id;

        [JsonProperty(PropertyName = "date")]
        public string Date;

        [JsonProperty(PropertyName = "ticker")]
        public string Ticker;

        [JsonProperty(PropertyName = "name")]
        public string Name;

        [JsonProperty(PropertyName = "exchange")]
        public string Exchange;

        [JsonProperty(PropertyName = "updated")]
        public int Updated;

        [JsonProperty(PropertyName = "active")]
        public int Active;

        [JsonProperty(PropertyName = "time")]
        public string Time;
        [JsonProperty(PropertyName = "frequency")]
        public string Frequency;

        [JsonProperty(PropertyName = "dividend")]
        public string Dividend;

        [JsonProperty(PropertyName = "dividend_prior")]
        public string DividendPrior;

        [JsonProperty(PropertyName = "dividend_type")]
        public string DividendType;

        [JsonProperty(PropertyName = "dividend_yield")]
        public string DividendYield;

        [JsonProperty(PropertyName = "ex_dividend_date")]
        public string DividendExDate;

        [JsonProperty(PropertyName = "payable_date")]
        public string PayableDate;

        [JsonProperty(PropertyName = "record_date")]
        public string RecordDate;

        [JsonProperty(PropertyName = "importance")]
        public string Importance;

    }
}
