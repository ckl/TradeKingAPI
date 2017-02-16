using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeKing.API.Database;
using TradeKing.API.Models.Streaming;
using TradeKing.API.Requests;

namespace TradeKing.WpfGraphs
{
    public class MainWindowViewModel
    {
        public ObservableCollection<QuoteDataPoint> SSPXFQuotes { get; private set; }
        public ObservableCollection<QuoteDataPoint> CANWFQuotes { get; private set; }
        public MainWindowViewModel()
        {
            this.SSPXFQuotes = new ObservableCollection<QuoteDataPoint>();
            this.CANWFQuotes = new ObservableCollection<QuoteDataPoint>();
            LoadQuotes();
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
                        this.SSPXFQuotes.Add(new QuoteDataPoint
                        {
                            Ask = Convert.ToDecimal(q.Ask),
                            Bid = Convert.ToDecimal(q.Bid),
                            Time = DateTime.Parse(q.Datetime)
                        });
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

                            Console.WriteLine("{0} [Qu] {1} Ask:  {2} Bid: {3} AskSz: {4} BidSz: {5}", time, ticker.PadRight(5, ' '), quote.Ask.PadRight(6, ' '), quote.Bid.PadRight(5, ' '), askSz.ToString("N0").PadRight(9, ' '), bidSz.ToString("N0"));

                            if (quote.Symbol == "SSPXF")
                            {
                                App.Current.Dispatcher.Invoke(delegate
                                {
                                    this.SSPXFQuotes.Add(new QuoteDataPoint
                                    {
                                        Ask = Convert.ToDecimal(quote.Ask),
                                        Bid = Convert.ToDecimal(quote.Bid),
                                        Time = DateTime.Parse(quote.Datetime)
                                    });
                                });
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
    }

    public class QuoteDataPoint
    {
        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public DateTime Time { get; set; }
        //public string Datetime {
        //    get
        //    {
        //        return Time.ToShortTimeString();
        //    }

        //    set
        //    {
        //        Time = DateTime.Parse(value);
        //    }
        //}
    }
}
