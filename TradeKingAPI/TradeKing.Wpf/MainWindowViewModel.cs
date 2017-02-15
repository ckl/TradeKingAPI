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

        public MainWindowViewModel()
        {
            TickerTabsViewModel = new TickerTabsViewModel();

            StartStreamingData();
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

                quoteStream.Execute((items) => {

                    foreach (var item in items)
                    {
                        var ticker = string.Format("[{0}]", item.Symbol);
                        var time = DateTime.Now.ToShortTimeString();
                        var tab = TickerTabsViewModel.Tabs.Where(t => t.Ticker == item.Symbol).FirstOrDefault();

                        if (item is Quote)
                        {
                            Quote quote = (Quote)item;
                            int askSz = Convert.ToInt32(quote.Asksz);
                            int bidSz = Convert.ToInt32(quote.Bidsz);

                            var str = string.Format("{0} [Qu] {1} Ask:  {2} Bid: {3} AskSz: {4} BidSz: {5}\n", time, ticker.PadRight(5, ' '), quote.Ask.PadRight(6, ' '), quote.Bid.PadRight(5, ' '), askSz.ToString("N0").PadRight(9, ' '), bidSz.ToString("N0"));
                            Console.Write(str);

                            tab.OnNewStreamItem(quote);

                            using (var db = DbFactory.GetDbSource())
                            {
                                db.SaveStreamQuote(quote);
                            }
                        }
                        else if (item is Trade)
                        {
                            Trade trade = (Trade)item;
                            Console.WriteLine("{0} [Tr] {1} Last: {2} Vol: {3}", time, ticker.PadRight(6, ' '), trade.Last.PadRight(5, ' '), trade.Vl);

                            tab.OnNewStreamItem(trade);

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
