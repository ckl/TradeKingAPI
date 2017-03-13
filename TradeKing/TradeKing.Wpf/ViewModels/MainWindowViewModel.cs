using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TradeKing.API.Database;
using TradeKing.API.Models.Streaming;
using TradeKing.Wpf.Base;
using TradeKing.Wpf.Helpers;
using TradeKing.Wpf.Models;

namespace TradeKing.Wpf.ViewModels
{
    public class MainWindowViewModel : NotifyPropertyChangedBase
    {
        public TickerTabsViewModel TickerTabsViewModel { get; set; }
        public WatchListsViewModel WatchListsViewModel { get; set; }
        public ConsoleMessageViewModel ConsoleMessageViewModel { get; set; }
        public ObservableCollection<QuoteAndTrade> StreamQuotes { get; set; }

        public MainWindowViewModel()
        {
            TickerTabsViewModel = new TickerTabsViewModel();
            ConsoleMessageViewModel = new ConsoleMessageViewModel();
            WatchListsViewModel = new WatchListsViewModel();
            StreamQuotes = new ObservableCollection<QuoteAndTrade>();

            LoadTradeKingStuff();
        }

        private async void LoadTradeKingStuff()
        {
            await WatchListsViewModel.LoadWatchLists();
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

            var tickers = WatchListsViewModel.SelectedItem.Tickers.ToList();

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

                    UpdateStreamQuote(quote);
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

                    UpdateStreamQuote(trade);
                }

                ExecuteOnMainThread(tab, item);
            }
        }

        private void UpdateStreamQuote(Quote quote)
        {

            var element = StreamQuotes.FirstOrDefault(c => c.Symbol == quote.Symbol);

            if (element == null)
            {
                element = new QuoteAndTrade
                {
                    Ask = quote.Ask,
                    Asksz = quote.Asksz,
                    Bid = quote.Bid,
                    Bidsz = quote.Bidsz,
                    Datetime = quote.Datetime,
                    Symbol = quote.Symbol,
                    Timestamp = quote.Timestamp
                };

            }
            else
            {
                element.Ask = quote.Ask;
                element.Asksz = quote.Asksz;
                element.Bid = quote.Bid;
                element.Bidsz = quote.Bidsz;
                element.Datetime = quote.Datetime;
                element.Timestamp = quote.Timestamp;
            }

            AddStreamQuoteOnMainThread(element);
        }

        private void UpdateStreamQuote(Trade trade)
        {
            var element = StreamQuotes.FirstOrDefault(c => c.Symbol == trade.Symbol);

            if (element == null)
            {
                element = new QuoteAndTrade
                {
                    LastPrice = trade.Last,
                    Volume = trade.Vl,
                    Datetime = trade.Datetime,
                    Symbol = trade.Symbol,
                    Timestamp = trade.Timestamp
                };

            }
            else
            {
                element.LastPrice = trade.Last;
                element.Volume = trade.Vl;
                element.Datetime = trade.Datetime;
                element.Timestamp = trade.Datetime;
            }
            AddStreamQuoteOnMainThread(element);
        }

        private void AddStreamQuoteOnMainThread(QuoteAndTrade quote)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() =>
                        {
                            StreamQuotes.Remove(quote);
                            StreamQuotes.Add(quote);
                        }
                        ));
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
