using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Task.Run(() => {
                Console.WriteLine("Starting data stream...");
                var x = new QuoteStreamRequest();
                x.Execute((items) => {
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

            while (true)
            {
                var line = Console.ReadLine();
                if (line == ".")
                {
                    break;
                }
            }
        }
    }
}
