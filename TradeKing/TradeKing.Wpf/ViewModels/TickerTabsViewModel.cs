using System.Collections.ObjectModel;
using System.Linq;
using TradeKing.Wpf.Base;
using TradeKing.Wpf.Views.UserControls;

namespace TradeKing.Wpf.ViewModels
{
    public class TickerTabsViewModel : NotifyPropertyChangedBase
    {
        public ObservableCollection<TickerTabItemViewModel> Tabs { get; set; }
        public TickerTabItemViewModel SelectedTab
        {
            get { return _selectedTab; }
            set { SetField(ref _selectedTab, value, "SelectedTab"); }
        }

        public TickerTabsViewModel()
        {
            Tabs = new ObservableCollection<TickerTabItemViewModel>();
        }

        public void AddTab(string ticker)
        {
            var tab = Tabs.FirstOrDefault(t => t.Ticker == ticker);

            if (tab != null)
            {
                SelectedTab = tab;
            }
            else
            {
                var tabView = new TickerTabContent();
                var viewModel = new TickerTabItemViewModel(ticker, ticker, tabView);
                tabView.SetViewModel(viewModel);
                Tabs.Add(viewModel);
                SelectedTab = viewModel;
            }
        }

        public void RemoveTab(TickerTabItemViewModel viewModel)
        {
            if (SelectedTab == viewModel && Tabs.Count == 1)
            {
                SelectedTab = null;
            }
            else
            {
                // TODO: activate next tab
                //var tabIndexToClose = Tabs.IndexOf(viewModel);
                //int nextTab;
                //if (tabIndexToClose == Tabs.Count)
                //    nextTab = 
                    
            }
            Tabs.Remove(viewModel);
        }

        private TickerTabItemViewModel _selectedTab;
    }
}
