using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TradeKing.API.Database;
using TradeKing.API.Models.Streaming;
using TradeKing.Wpf.Base;
using TradeKing.Wpf.Helpers;

namespace TradeKing.Wpf.ViewModels
{
    public class MainWindowViewModel : NotifyPropertyChangedBase
    {
        public TickerTabsViewModel TickerTabsViewModel { get; set; }
        public WatchListsViewModel WatchListsViewModel { get; set; }
        public ConsoleMessageViewModel ConsoleMessageViewModel { get; set; }

        public MainWindowViewModel()
        {
            TickerTabsViewModel = new TickerTabsViewModel();
            ConsoleMessageViewModel = new ConsoleMessageViewModel();
            WatchListsViewModel = new WatchListsViewModel();

            LoadTradeKingStuff();
        }

        private async void LoadTradeKingStuff()
        {
            await WatchListsViewModel.LoadWatchLists();

            //StartStreamingData();
        }

        private CancellationTokenSource _tokenSource = null;
        public  void StartStreamingData()
        {
            //Task.Run(() => {
            //    List<string> tickers;
            //    using (var db = DbFactory.GetDbSource())
            //    {
            //        //tickers = db.GetTickers();
            //        tickers = WatchListsViewModel.SelectedItem.Tickers.ToList();
            //    }

            //    ConsoleMessageLogger.Instance.Log("Starting data stream...");
            //    TkApiRequestWrapper.ExecuteStreamRequest(tickers, HandleStreamData);
            //});

            _tokenSource = new CancellationTokenSource();
            var ct = _tokenSource.Token;

            List<string> tickers;
            using (var db = DbFactory.GetDbSource())
            {
                tickers = WatchListsViewModel.SelectedItem.Tickers.ToList();
            }

            var task = Task.Factory.StartNew(() =>
            {
                // Were we already canceled?
                ct.ThrowIfCancellationRequested();

                ConsoleMessageLogger.Instance.Log("Starting data stream with " + tickers.Count + " tickers...");
                TkApiRequestWrapper.ExecuteStreamRequest(tickers, HandleStreamData);

            }, _tokenSource.Token);

            //try
            //{
            //    ConsoleMessageLogger.Instance.Log("Starting data stream...");
            //    task.Wait();

            //    //ConsoleMessageLogger.Instance.Log("Starting data stream...");
            //}
            //catch (Exception ex)
            //{
            //    ConsoleMessageLogger.Instance.Log("Exception in Stream Data: " + ex.Message);
            //}
            //finally
            //{
            //    Console.WriteLine("xyz 123");
            //}

            task.Wait();
            Console.WriteLine("Test 123");
        }

        public void CancelStream()
        {
            if (_tokenSource != null)
            {
                TkApiRequestWrapper.CancelStreamRequest();
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                ConsoleMessageLogger.Instance.Log("Stopping data stream...");
            }
        }

        private void HandleStreamData(List<StreamDataItem> items)
        {
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

                    var str = string.Format("{0} [Qu] {1} Ask:  {2} Bid: {3} AskSz: {4} BidSz: {5}", time, ticker.PadRight(5, ' '), quote.Ask.PadRight(6, ' '), quote.Bid.PadRight(5, ' '), askSz.ToString("N0").PadRight(9, ' '), bidSz.ToString("N0"));
                    //Console.Write(str);
                    ConsoleMessageLogger.Instance.LogQuoteStreamMessage(str);

                    using (var db = DbFactory.GetDbSource())
                    {
                        db.SaveStreamQuote(quote);
                    }
                }
                else if (item is Trade)
                {
                    Trade trade = (Trade)item;
                    var str = string.Format("{0} [Tr] {1} Last: {2} Vol: {3}", time, ticker.PadRight(6, ' '), trade.Last.PadRight(5, ' '), trade.Vl);
                    //Console.WriteLine("{0} [Tr] {1} Last: {2} Vol: {3}", time, ticker.PadRight(6, ' '), trade.Last.PadRight(5, ' '), trade.Vl);
                    ConsoleMessageLogger.Instance.LogQuoteStreamMessage(str);

                    using (var db = DbFactory.GetDbSource())
                    {
                        db.SaveStreamTrade(trade);
                    }
                }

                ExecuteOnMainThread(tab, item);
            }
        }

        private void ExecuteOnMainThread(TickerTabItemViewModel tab, StreamDataItem item)
        {
            if (tab == null)
                return;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item is Quote)
                        tab.OnNewStreamItem(item as Quote);
                    else if (item is Trade)
                        tab.OnNewStreamItem(item as Trade);
                }));

            if (TickerTabsViewModel.SelectedTab != tab)
            {
                tab.SetUnread();
            }
        }
    }
}
