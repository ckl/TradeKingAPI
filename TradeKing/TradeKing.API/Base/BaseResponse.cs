﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKing.API.Interfaces;

namespace TradeKing.API.Base.Responses
{
    public abstract class BaseResponse : IResponse
    {
        public string Data { get; set; }

        /*public BaseResponse(string data)
        {
            Data = data;
        }*/
    }
}
