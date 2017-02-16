using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TradeKing.Wpf.Base;
using TradeKing.Wpf.Helpers;
using TradeKing.Wpf.Models;

namespace TradeKing.Wpf.ViewModels
{
    public class WatchListsViewModel : NotifyPropertyChangedBase
    {
        public ObservableCollection<WatchList> WatchLists { get; set; }

        public WatchListsViewModel()
        {
            WatchLists = new ObservableCollection<WatchList>();
        }

        public async Task LoadWatchLists()
        {
            ConsoleMessageLogger.Instance.Log("Loading WatchLists...");

            var watchlists = await TkApiRequestWrapper.GetWatchLists();

            foreach (var w in watchlists)
            {
                WatchLists.Add(new WatchList
                {
                    Name = w.Name,
                    Tickers = w.Tickers
                });
            }

            SelectedItem = WatchLists.FirstOrDefault();

            ConsoleMessageLogger.Instance.Log("Done loading WatchLists, found " + WatchLists.Count);
        }

        private WatchList _selectedItem;
        public WatchList SelectedItem
        {
            get { return _selectedItem; }
            set { SetField(ref _selectedItem, value, "SelectedItem"); }
        }
    }
}
