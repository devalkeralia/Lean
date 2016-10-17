using System.Collections.Generic;
using Newtonsoft.Json;

namespace BenzingaApi
{
    public class SplitsJson
    {
        [JsonProperty(PropertyName = "splits")]
        public List<Splits> Splits;
    }

    public class Splits
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

        [JsonProperty(PropertyName = "date_ex")]
        public string ExDate;

        [JsonProperty(PropertyName = "date_distribution")]
        public string DistributionDate;

        [JsonProperty(PropertyName = "date_recorded")]
        public string RecordDate;

        [JsonProperty(PropertyName = "ratio")]
        public string Ratio;

        [JsonProperty(PropertyName = "optionable")]
        public string Optionable;

        [JsonProperty(PropertyName = "importance")]
        public string Importance;
    }
}
