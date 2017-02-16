using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeKing.API.Database;
using TradeKing.API.Models.Article;
using TradeKing.API.Models.Streaming;
using TradeKing.API.Requests;

namespace TradeKing_ConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            GetWatchLists();

            //DisplayMenu();

            //GetNews();

            //GetSavedQuotesAndTrades();

            Console.ReadLine();
        }

        private static async void GetWatchLists()
        {
            var listsReq = new WatchListsRequest();
            var watchLists = await listsReq.Execute();

            foreach (var watchList in watchLists)
            {
                var listReq = new WatchListRequest(watchList.Id);
                var list = await listReq.Execute();

                Console.WriteLine(watchList.Id);
                Console.WriteLine(new string('-', watchList.Id.Length));

                Console.WriteLine(string.Join("\n", list.Watchlist.Watchlistitem.OrderBy(c => c.Instrument.Sym).Select(c => c.Instrument.Sym)));
            }
        }

        private static void GetSavedQuotesAndTrades()
        {
            using (var db = DbFactory.GetDbSource())
            {
                var x = db.GetAllStreamQuotes();
                var y = db.GetAllStreamTrades();
            }
        }

        private static async void GetNews()
        {
            using (var db = DbFactory.GetDbSource())
            {
                var tickers = db.GetTickers();

                foreach (var ticker in tickers)
                {
                    var x = new NewsRequest(new List<string> { ticker });
                    var articles = await x.Execute();

                    Console.WriteLine(new string('-', ticker.Length));
                    Console.WriteLine(ticker);
                    Console.WriteLine(new string('-', ticker.Length));
                    Console.WriteLine(string.Join("\n\n", articles.Select(a => a.Headline)));
                }
            }
        }

        private static void DisplayMenu()
        {
            while (true)
            {
                var title = "TradeKing Console App";

                Console.WriteLine(new string('-', title.Length));
                Console.WriteLine(title);
                Console.WriteLine(new string('-', title.Length));

                Console.WriteLine("[1] Add/Remove OAuth Keys");
                Console.WriteLine("[2] Add/remove ticker to watch list");
                Console.WriteLine("[3] Stream watch list data");
                Console.WriteLine("[4] Exit");

                Console.Write(">> ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        break;
                    case "2":
                        AddRemoveTickers();
                        break;
                    case "3":
                        StreamingData_EntryPoint();
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Invalid option, please select another");
                        break;
                }
            }
        }

        private static void AddRemoveTickers()
        {
            List<string> tickers;

            using (var db = DbFactory.GetDbSource())
            {
                string line = string.Empty;

                while (true)
                {
                    tickers = db.GetTickers();

                    for (int i = 0; i < tickers.Count; ++i)
                    {
                        Console.WriteLine("[{0}] {1}", i + 1, tickers[i]);
                    }

                    Console.WriteLine("(A TICKER / D #) >> ");
                    line = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }

                    var tokens = line.Split(' ');
                    var action = tokens[0];
                    var ticker = tokens[1];

                    switch (action.ToUpper())
                    {
                        case "A":
                            db.AddTicker(ticker.ToUpper());
                            break;
                        case "D":
                            var i = Convert.ToInt32(ticker);
                            db.DeleteTicker(tickers[i - 1]);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static void StreamingData_EntryPoint()
        {
            var tokenSource = new CancellationTokenSource();
            var ct = tokenSource.Token;
            List<string> tickers;

            using (var db = DbFactory.GetDbSource())
            {
                tickers = db.GetTickers();
            }

            var quoteStream = new QuoteStreamRequest(tickers);

            var task = Task.Factory.StartNew(() =>
            {
                // Were we already canceled?
                ct.ThrowIfCancellationRequested();

                StartStreamingData(quoteStream, ct, tokenSource);
                
            }, tokenSource.Token); 

            try
            {
                task.Wait();

                var exit = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(exit))
                {
                    Console.WriteLine("Cancel requested");
                    tokenSource.Cancel();
                }
            }
            finally
            {
                tokenSource.Dispose();
            }

            quoteStream.CloseStream();
            quoteStream = null;
        }

        private static void StartStreamingData(QuoteStreamRequest quoteStream, CancellationToken cancelToken, CancellationTokenSource tokenSource)
        {
            Task.Run(() => {
                Console.WriteLine("Starting data stream, press enter to cancel...");

                // Poll on this property if you have to do other cleanup before throwing.
                if (cancelToken.IsCancellationRequested)
                {
                    cancelToken.ThrowIfCancellationRequested();
                }

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
}
