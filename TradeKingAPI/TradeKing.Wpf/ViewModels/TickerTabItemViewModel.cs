using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TradeKingAPI.Database;
using TradeKingAPI.Models.Streaming;

namespace TradeKing.Wpf.ViewModels
{
    public class TickerTabItemViewModel : INotifyPropertyChanged
    {
        public TickerTabItemViewModel(string tabTitle, string ticker, UserControl tabContent)
        {
            Ticker = ticker;
            TabTitle = tabTitle;
            TabContent = tabContent;
            GraphTitle = ticker + " graph";

            GraphQuotes = new ObservableCollection<QuoteDataPoint>();

            LoadQuotes();
        }

        private void LoadQuotes()
        {
            using (var db = DbFactory.GetDbSource())
            {
                Quotes = new ObservableCollection<Quote>(db.GetStreamQuotes(Ticker));

                foreach (var quote in Quotes)
                {
                    GraphQuotes.Add(new QuoteDataPoint
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
        public ObservableCollection<QuoteDataPoint> GraphQuotes { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }
    }

    public class QuoteDataPoint
    {
        public decimal Ask { get; set; }
        public decimal Asksz { get; set; }
        public decimal Bid { get; set; }
        public decimal Bidsz { get; set; }
        public DateTime Time { get; set; }
        public string Symbol { get; set; }
    }
}
