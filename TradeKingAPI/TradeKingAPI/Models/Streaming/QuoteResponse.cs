using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKingAPI.Base.Responses;
using TradeKingAPI.Models.Responses;

namespace TradeKingAPI.Models.Streaming
{
    public class QuoteResponse : BaseResponse
    {
        [JsonProperty("quote")]
        public Quote Response { get; set; }
    }
}
