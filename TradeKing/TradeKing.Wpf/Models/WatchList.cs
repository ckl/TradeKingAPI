using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKing.Wpf.Base;

namespace TradeKing.Wpf.Models
{
    public class WatchList : NotifyPropertyChangedBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value, "Name"); }

        }
        public ObservableCollection<string> Tickers { get; set; }
        //{
        //    get { return _tickers; }
        //    set { SetField(ref _tickers, value, "Tickers");  }
        //}
    }
}
