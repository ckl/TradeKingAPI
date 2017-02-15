using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKing.Wpf.Views.UserControls;

namespace TradeKing.Wpf.ViewModels
{
    public class TickerTabsViewModel
    {
        public ObservableCollection<TickerTabItemViewModel> Tabs { get; set; }

        public TickerTabsViewModel()
        {
            var tab1 = new TickerTabContent();
            var tab2 = new TickerTabContent();

            Tabs = new ObservableCollection<TickerTabItemViewModel>();
            var sspxfViewModel = new TickerTabItemViewModel("SSPXF", "SSPXF", tab1);
            var canwfViewModel = new TickerTabItemViewModel("CANWF", "CANWF", tab2);

            Tabs.Add(sspxfViewModel);
            Tabs.Add(canwfViewModel);

            tab1.SetViewModel(sspxfViewModel);
            tab2.SetViewModel(canwfViewModel);

        }
    }
}
