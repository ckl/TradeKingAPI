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
    public class TradeResponse : BaseResponse
    {
        [JsonProperty("trade")]
        public Trade Response { get; set; }
    }
}
