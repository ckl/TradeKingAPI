using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKing.Wpf.Base;
using TradeKing.Wpf.Views.UserControls;
using TradeKingAPI.Models.Streaming;

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
            var tabView = new TickerTabContent();
            var viewModel = new TickerTabItemViewModel(ticker, ticker, tabView);
            tabView.SetViewModel(viewModel);
            Tabs.Add(viewModel);
            SelectedTab = viewModel;
        }

        public void RemoveTab(TickerTabItemViewModel viewModel)
        {
            Tabs.Remove(viewModel);
        }

        private TickerTabItemViewModel _selectedTab;
    }
}
