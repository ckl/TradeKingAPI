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
using System.Windows;
using System.Windows.Threading;
using OxyPlot.Axes;

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
                    var gq = new QuoteGraphDataPoint
                    {
                        Ask = Convert.ToDecimal(quote.Ask),
                        Bid = Convert.ToDecimal(quote.Bid),
                        Time = DateTime.Parse(quote.Datetime)
                    };

                    GraphQuotes.Add(gq);

                    var ask = Convert.ToDouble(quote.Ask);
                    var bid = Convert.ToDouble(quote.Bid);

                    // TODO - make date checking generic to check within the selected date range
                    if (gq.Time>= _xAxisMin && gq.Time<= _xAxisMax)
                    {
                        if (ask > _yAxisMax)
                            _yAxisMax = ask;
                        if (ask < _yAxisMin)
                            _yAxisMin = ask;

                        if (bid > _yAxisMax)
                            _yAxisMax = bid;
                        if (bid < _yAxisMin)
                            _yAxisMin = bid;
                    }
                }

                Quotes.Clear();
            }
        }

        public void OnNewStreamItem(Quote quote)
        {
            //Quotes.Add(quote);
            var gq = new QuoteGraphDataPoint
            {
                Ask = Convert.ToDecimal(quote.Ask),
                Bid = Convert.ToDecimal(quote.Bid),
                Time = DateTime.Parse(quote.Datetime)
            };

            var ask = Convert.ToDouble(quote.Ask);
            var bid = Convert.ToDouble(quote.Bid);

            if (ask > _yAxisMax)
                yAxisMax = ask;
            if (ask < _yAxisMin)
                yAxisMin = ask;

            if (bid > _yAxisMax)
                yAxisMax = bid;
            if (bid < _yAxisMin)
                yAxisMin = bid;

            GraphQuotes.Add(gq);
        }

        public void OnNewStreamItem(Trade trade)
        {

        }

        public void SetUnread()
        {
            if (! TabTitle.EndsWith("*"))
                TabTitle = TabTitle + "*";
        }

        public void SetRead()
        {
            TabTitle = TabTitle.TrimEnd('*');
        }

        private double _yAxisMin = Double.MaxValue;
        private double _yAxisMax = Double.MinValue;

        public double yAxisMin
        {
            //get { return _yAxisMin - (_yAxisMin * 0.025); }
            get { return _yAxisMin; }
            set { SetField(ref _yAxisMin, value, "yAxisMin"); }
        }
        public double yAxisMax
        {
            //get { return _yAxisMax + (_yAxisMax * 0.025); }
            get { return _yAxisMax; }
            set { SetField(ref _yAxisMax, value, "yAxisMax"); }
        }

        private DateTime _xAxisMin = DateTime.Today.AddHours(9);
        private DateTime _xAxisMax = DateTime.Today.AddHours(17);

        public double xAxisMin
        {
            get { return DateTimeAxis.ToDouble(_xAxisMin); }
            set { SetField(ref _xAxisMin, DateTimeAxis.ToDateTime(value), "xAxisMin"); }
        }

        public double xAxisMax
        {
            get { return DateTimeAxis.ToDouble(_xAxisMax); }
            set { SetField(ref _xAxisMax, DateTimeAxis.ToDateTime(value), "xAxisMax"); }
        }

        public string Ticker { get; set; }
        private string _tabTitle;
        public string TabTitle
        {
            get { return _tabTitle; } 
            set { SetField(ref _tabTitle, value, "TabTitle"); }
        }
        public UserControl TabContent { get; set; }
        public string GraphTitle { get; set; }
        public ObservableCollection<Quote> Quotes { get; set; }
        public ObservableCollection<QuoteGraphDataPoint> GraphQuotes { get; set; }
    }

    
}
