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
        private static QuoteStreamRequest _quoteStreamRequest;
        private static bool _cancelingStream = false;
        //private static CancellationToken _cancelToken;
        //private static CancellationTokenSource _cancelTokenSource;

        public static async void ExecuteStreamRequest(List<string> tickers, Action<List<StreamDataItem>> callback)
        {

            _quoteStreamRequest = new QuoteStreamRequest(tickers);

            try
            {
                _cancelingStream = false;

                await _quoteStreamRequest.Execute(callback);
            }
            catch (WebException ex)
            {
                Console.WriteLine("[Quote Stream] Web exception: " + ex.Message);
                if (_quoteStreamRequest != null)
                    _quoteStreamRequest.CloseStream();

                if (! _cancelingStream)
                    DoRetry(5000, tickers, callback);
            }
            catch (IOException ex)
            {
                // TK API closed the connection, cleanup and retry in 1 second
                Console.WriteLine("[Quote Stream] IO exception: " + ex.Message);
                if (_quoteStreamRequest != null)
                    _quoteStreamRequest.CloseStream();
                if (! _cancelingStream)
                    DoRetry(1000, tickers, callback);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("[Quote Stream] NullReferenceException. StackTrace: " + ex.StackTrace);
                if (_quoteStreamRequest != null)
                    _quoteStreamRequest.CloseStream();
                if (! _cancelingStream)
                    DoRetry(1000, tickers, callback);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[Quote Stream] Unhandled exception: {0} [{1}]", ex.Message, ex.GetType().ToString()));
                Console.WriteLine();
                if (_quoteStreamRequest != null)
                    _quoteStreamRequest.CloseStream();
                if (! _cancelingStream)
                    DoRetry(1000, tickers, callback);
            }

        }

        public static void CancelStreamRequest()
        {
            _cancelingStream = true;

            if (_quoteStreamRequest != null)
            {
                _quoteStreamRequest.CloseStream();
                _quoteStreamRequest = null;
            }
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
