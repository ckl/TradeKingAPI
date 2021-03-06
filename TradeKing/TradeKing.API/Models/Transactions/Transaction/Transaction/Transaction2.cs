﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TradeKing.API.Models.Transactions
{

    public class Transaction2
    {

        [JsonProperty("accounttype")]
        public string Accounttype { get; set; }

        [JsonProperty("commission")]
        public string Commission { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("fee")]
        public string Fee { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        [JsonProperty("secfee")]
        public string Secfee { get; set; }

        [JsonProperty("security")]
        public Security Security { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("tradedate")]
        public string Tradedate { get; set; }

        [JsonProperty("transactionid")]
        public string Transactionid { get; set; }

        [JsonProperty("transactiontype")]
        public string Transactiontype { get; set; }

        [JsonProperty("settlementdate")]
        public string Settlementdate { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }
    }

}
