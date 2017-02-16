using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeKing.API.Models.Streaming
{
    public class StreamDataItem
    {
        [JsonProperty("datetime")]
        public string Datetime { get; set; }

        [JsonProperty("exch")]
        public Exch Exch { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
