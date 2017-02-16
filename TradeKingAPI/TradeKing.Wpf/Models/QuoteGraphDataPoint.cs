using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeKing.Wpf.Models
{
    public class QuoteGraphDataPoint
    {
        public decimal Ask { get; set; }
        public decimal Asksz { get; set; }
        public decimal Bid { get; set; }
        public decimal Bidsz { get; set; }
        public DateTime Time { get; set; }
        public string Symbol { get; set; }
    }
}
