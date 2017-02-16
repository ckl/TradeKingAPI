using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TradeKing.Wpf.Base;
using TradeKing.Wpf.Models;
using TradeKing.API.Database;
using TradeKing.API.Models.Streaming;

namespace TradeKing.Wpf.ViewModels
{
    public class TickerTabItemViewModel : NotifyPropertyChangedBase
    {
        public TickerTabItemViewModel(string tabTitle, string ticker, UserControl tabContent)
        {
            Ticker = ticker;
            TabTitle = tabTitle;
            TabContent = tabContent;
            GraphTitle = ticker + " graph";

            GraphQuotes = new ObservableCollection<QuoteGraphDataPoint>();

            LoadQuotes();
        }

        private void LoadQuotes()
        {
            using (var db = DbFactory.GetDbSource())
            {
                Quotes = new ObservableCollection<Quote>(db.GetStreamQuotes(Ticker));

                foreach (var quote in Quotes)
                {
                    GraphQuotes.Add(new QuoteGraphDataPoint
                    {
                        Ask = Convert.ToDecimal(quote.Ask),
                        Bid = Convert.ToDecimal(quote.Bid),
                        Time = DateTime.Parse(quote.Datetime)
                    });

                    var ask = Convert.ToDouble(quote.Ask);
                    var bid = Convert.ToDouble(quote.Bid);

                    if (ask > _xAxisMax)
                        _xAxisMax = ask;
                    if (ask < _xAxisMin)
                        _xAxisMin = ask;

                    if (bid > _xAxisMax)
                        _xAxisMax = bid;
                    if (bid < _xAxisMin)
                        _xAxisMin = bid;
                }
            }
        }

        public void OnNewStreamItem(Quote quote)
        {
            Quotes.Add(quote);
            GraphQuotes.Add(new QuoteGraphDataPoint
            {
                Ask = Convert.ToDecimal(quote.Ask),
                Bid = Convert.ToDecimal(quote.Bid),
                Time = DateTime.Parse(quote.Datetime)
            });
        }

        public void OnNewStreamItem(Trade trade)
        {

        }

        private double _xAxisMin = Double.MaxValue;
        private double _xAxisMax = Double.MinValue;

        public double xAxisMin
        {
            get { return _xAxisMin - (_xAxisMin * 0.10); }
            set { SetField(ref _xAxisMin, value, "xAxisMin"); }
        }
        public double xAxisMax
        {
            get { return _xAxisMax + (_xAxisMax * 0.10); }
            set { SetField(ref _xAxisMax, value, "xAxisMax"); }
        }

        public string Ticker { get; set; }
        public string TabTitle { get; set; }
        public UserControl TabContent { get; set; }
        public string GraphTitle { get; set; }
        public ObservableCollection<Quote> Quotes { get; set; }
        public ObservableCollection<QuoteGraphDataPoint> GraphQuotes { get; set; }
    }

    
}
