using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeKing.API.Models.Streaming;
using TradeKing.API.Models.WatchLists;
using TradeKing.API.Requests;
using TradeKing.Wpf.Models;
using TradeKing.Wpf.ViewModels;

namespace TradeKing.Wpf.Helpers
{
    public class TkApiRequestWrapper
    {
        public static async void ExecuteStreamRequest(List<string> tickers, Action<List<StreamDataItem>> callback)
        {
            var quoteStream = new QuoteStreamRequest(tickers);

            try
            {
                await quoteStream.Execute(callback);
            }
            catch (WebException ex)
            {
                ConsoleMessageLogger.Instance.Log("[Quote Stream] Web exception: " + ex.Message);
                quoteStream.CloseStream();
                DoRetry(5000, tickers, callback);
            }
            catch (IOException ex)
            {
                // TK API closed the connection, cleanup and retry in 1 second
                quoteStream.CloseStream();
                DoRetry(1000, tickers, callback);
            }
            catch (NullReferenceException ex)
            {
                ConsoleMessageLogger.Instance.Log("[Quote Stream] NullReferenceException. StackTrace: " + ex.StackTrace);
                quoteStream.CloseStream();
                DoRetry(1000, tickers, callback);
            }
            catch (Exception ex)
            {
                ConsoleMessageLogger.Instance.Log(string.Format("[Quote Stream] Unhandled exception: {0} [{1}]", ex.Message, ex.GetType().ToString()));
                Console.WriteLine();
                quoteStream.CloseStream();
            }

            var x = 1;
        }

        private static void DoRetry(int delay, List<string> tickers, Action<List<StreamDataItem>> callback)
        {
            Thread.Sleep(delay);
            ExecuteStreamRequest(tickers, callback);
        }

        public static async Task<IEnumerable<WatchList>> GetWatchLists()
        {
            var retval = new List<WatchList>();

            var listsReq = new WatchListsRequest();
            IEnumerable<Watchlist> watchLists;

            try {
                watchLists = await listsReq.Execute();

                foreach (var watchList in watchLists)
                {
                    var listReq = new WatchListRequest(watchList.Id);
                    var list = await listReq.Execute();

                    retval.Add(new WatchList
                    {
                        Name = watchList.Id,
                        Tickers = new ObservableCollection<string>(list.Watchlist.Watchlistitem.OrderBy(t => t.Instrument.Sym).Select(t => t.Instrument.Sym).ToList())
                    });

                }

            }
            catch (Exception ex)
            {

                ConsoleMessageLogger.Instance.Log("Error retrieving Watch Lists: " + ex.Message);
            }

            return retval;
        }
    }
}
