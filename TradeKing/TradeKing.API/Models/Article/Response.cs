﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TradeKing.API.Models.Article
{

    public class Response
    {

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("articles")]
        public Articles Articles { get; set; }

        [JsonProperty("article")]
        public Article Article { get; set; }
    }

}