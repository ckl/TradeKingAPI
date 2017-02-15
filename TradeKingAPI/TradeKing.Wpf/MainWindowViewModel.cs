using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKingAPI.Database;
using TradeKingAPI.Models.Streaming;
using TradeKingAPI.Requests;
using TradeKing.Wpf.ViewModels;

namespace TradeKing.Wpf
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public TickerTabsViewModel TickerTabsViewModel { get; set; }
        public PlotModel SSPXFGraphModel { get; set; }
        public ObservableCollection<QuoteDataPoint> SSPXFGraphQuotes { get; private set; }
        public ObservableCollection<QuoteDataPoint> CANWFQuotes { get; private set; }
        public ObservableCollection<Quote> SSPXFQuotes { get; set; }

        private string _sspxfQuoteString;
        public string SSPXFQuoteString
        {
            get { return _sspxfQuoteString; }
            set { SetField(ref _sspxfQuoteString, value, "SSPXFQuoteString");  }
        }

        public string CANWFQuoteString { get; set; }

        private double _sspxfMin = Double.MaxValue;
        public double SSPXFMin
        { get { return _sspxfMin - (_sspxfMin * 0.15); }
          set { SetField(ref _sspxfMin, value, "SSPXFMin"); }
        }

        private double _sspxfMax = Double.MinValue;
        public double SSPXFMax
        {
            get { return _sspxfMax + (_sspxfMax * 0.15); }
            set { SetField(ref _sspxfMax, value, "SSPXFMin"); }
        }

        public MainWindowViewModel()
        {
            TickerTabsViewModel = new TickerTabsViewModel();

            SSPXFQuoteString = string.Empty;
            CANWFQuoteString = "This is a test\nMore testing data";

            this.SSPXFQuotes = new ObservableCollection<Quote>();
            this.SSPXFGraphQuotes = new ObservableCollection<QuoteDataPoint>();
            this.CANWFQuotes = new ObservableCollection<QuoteDataPoint>();
            LoadQuotes();

            //this.SSPXFGraphModel = CreateSSPXFGraph();

            StartStreamingData();
        }

        private void LoadQuotes()
        {
            using (var db = DbFactory.GetDbSource())
            {
                var quotes = db.GetAllStreamQuotes();

                foreach (var q in quotes)
                {
                    if (q.Symbol == "SSPXF")
                    {
                        this.SSPXFGraphQuotes.Add(new QuoteDataPoint
                        {
                            Ask = Convert.ToDecimal(q.Ask),
                            Bid = Convert.ToDecimal(q.Bid),
                            Time = DateTime.Parse(q.Datetime)
                        });

                        this.SSPXFQuotes.Add(q);
                        
                        var time = q.Datetime;
                        var ticker = string.Format("[{0}]", q.Symbol);
                        int askSz = Convert.ToInt32(q.Asksz);
                        int bidSz = Convert.ToInt32(q.Bidsz);
                        var str = string.Format("{0} [Qu] {1} Ask:  {2} Bid: {3} AskSz: {4} BidSz: {5}\n", time, ticker.PadRight(5, ' '), q.Ask.ToString().PadRight(6, ' '), q.Bid.PadRight(5, ' '), askSz.ToString("N0").PadRight(9, ' '), bidSz.ToString("N0"));

                        SSPXFQuoteString += str;

                        var ask = Convert.ToDouble(q.Ask);
                        var bid = Convert.ToDouble(q.Bid);

                        if (ask > SSPXFMax)
                            SSPXFMax = ask;
                        if (ask < SSPXFMin)
                            SSPXFMin = ask;

                        if (bid > SSPXFMax)
                            SSPXFMax = bid;
                        if (bid < SSPXFMin)
                            SSPXFMin = bid;
                        
                    }
                    else if (q.Symbol == "CANWF")
                    {
                        this.CANWFQuotes.Add(new QuoteDataPoint
                        {
                            Ask = Convert.ToDecimal(q.Ask),
                            Bid = Convert.ToDecimal(q.Bid),
                            Time = DateTime.Parse(q.Datetime)
                        });
                    }
                }

                    
            }
        }

        private void StartStreamingData()
        {
            Task.Run(() => {
                List<string> tickers;
                using (var db = DbFactory.GetDbSource())
                {
                    tickers = db.GetTickers();
                }

                var quoteStream = new QuoteStreamRequest(tickers);
                Console.WriteLine("Starting data stream");

                // Poll on this property if you have to do other cleanup before throwing.

                quoteStream.Execute((items) => {

                    foreach (var item in items)
                    {
                        var ticker = string.Format("[{0}]", item.Symbol);
                        var time = DateTime.Now.ToShortTimeString();

                        if (item is Quote)
                        {
                            Quote quote = (Quote)item;
                            int askSz = Convert.ToInt32(quote.Asksz);
                            int bidSz = Convert.ToInt32(quote.Bidsz);

                            var str = string.Format("{0} [Qu] {1} Ask:  {2} Bid: {3} AskSz: {4} BidSz: {5}\n", time, ticker.PadRight(5, ' '), quote.Ask.PadRight(6, ' '), quote.Bid.PadRight(5, ' '), askSz.ToString("N0").PadRight(9, ' '), bidSz.ToString("N0"));
                            Console.Write(str);

                            if (quote.Symbol == "SSPXF")
                            {
                                App.Current.Dispatcher.Invoke(delegate
                                {
                                    this.SSPXFGraphQuotes.Add(new QuoteDataPoint
                                    {
                                        Ask = Convert.ToDecimal(quote.Ask),
                                        Bid = Convert.ToDecimal(quote.Bid),
                                        Time = DateTime.Parse(quote.Datetime)
                                    });

                                    this.SSPXFQuotes.Add(quote);
                                });

                                SSPXFQuoteString += str;
                                
                            }
                            else if (quote.Symbol == "CANWF")
                            {
                                App.Current.Dispatcher.Invoke(delegate
                                {
                                    this.CANWFQuotes.Add(new QuoteDataPoint
                                    {
                                        Ask = Convert.ToDecimal(quote.Ask),
                                        Bid = Convert.ToDecimal(quote.Bid),
                                        Time = DateTime.Parse(quote.Datetime)
                                    });
                                });
                            }

                            using (var db = DbFactory.GetDbSource())
                            {
                                db.SaveStreamQuote(quote);
                            }
                        }
                        else if (item is Trade)
                        {
                            Trade trade = (Trade)item;
                            Console.WriteLine("{0} [Tr] {1} Last: {2} Vol: {3}", time, ticker.PadRight(6, ' '), trade.Last.PadRight(5, ' '), trade.Vl);

                            using (var db = DbFactory.GetDbSource())
                            {
                                db.SaveStreamTrade(trade);
                            }
                        }
                    }
                });
            });
        }

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
