﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TradeKingAPI.Models.Accounts.AccountBalance;
using TradeKingAPI.Models.Accounts.AccountHoldings;

namespace TradeKingAPI.Models.Accounts
{

    public class Accountsummary
    {

        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("accountbalance")]
        public Accountbalance Accountbalance { get; set; }

        [JsonProperty("accountholdings")]
        public Accountholdings Accountholdings { get; set; }
    }

}
