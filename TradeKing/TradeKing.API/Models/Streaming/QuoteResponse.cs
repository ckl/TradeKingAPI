using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKing.API.Base.Responses;
using TradeKing.API.Models.Responses;

namespace TradeKing.API.Models.Streaming
{
    public class QuoteResponse : BaseResponse
    {
        [JsonProperty("quote")]
        public Quote Response { get; set; }
    }
}
