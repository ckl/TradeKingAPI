using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeKingAPI.Database;
using TradeKingAPI.Models.Streaming;
using TradeKingAPI.Requests;

namespace TradeKing_ConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var title = "TradeKing Console App";

                Console.WriteLine("-", title.Length);
                Console.WriteLine(title);
                Console.WriteLine("-", title.Length);

                Console.WriteLine("[1] Add OAuth Keys");
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

        private static void StreamingData_EntryPoint()
        {
            var tokenSource = new CancellationTokenSource();
            var ct = tokenSource.Token;
            var quoteStream = new QuoteStreamRequest(); ;

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

            quoteStream.Close();
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

                            Console.WriteLine("{0} [Qu] {1} Ask:  {2} Bid: {3} AskSz: {4} BidSz: {5}", time, ticker.PadRight(5, ' '), quote.Ask.PadRight(6, ' '), quote.Bid.PadRight(6, ' '), askSz.ToString("N0").PadRight(9, ' '), bidSz.ToString("N0"));

                            using (var db = new SqliteWrapper())
                            {
                                db.SaveStreamQuote(quote);
                            }
                        }
                        else if (item is Trade)
                        {
                            Trade trade = (Trade)item;
                            Console.WriteLine("{0} [Tr] {1} Last: {2} Vol: {3}", time, ticker.PadRight(5, ' '), trade.Last.PadRight(6, ' '), trade.Vl);

                            using (var db = new SqliteWrapper())
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
