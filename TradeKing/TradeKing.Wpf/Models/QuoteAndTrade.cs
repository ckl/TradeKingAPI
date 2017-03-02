using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeKing.Wpf.Models
{
    public class QuoteAndTrade
    {
        public string Ask { get; set; }
        public string Asksz { get; set; }
        public string Bid { get; set; }
        public string Bidsz { get; set; }
        public string Datetime { get; set; }
        public string Timestamp { get; set; }
        public string Symbol { get; set; }
        public string LastPrice { get; set; }
        public string Volume { get; set; }
    }
}
