using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeKing.Wpf.Base;
using TradeKing.Wpf.Helpers;

namespace TradeKing.Wpf.ViewModels
{
    public class ConsoleMessageViewModel : NotifyPropertyChangedBase
    {
        public ConsoleMessageViewModel()
        {
            ConsoleMessageLogger.Instance.SetViewModel(this);
        }

        public void NotifyPropertyChanged(string property)
        {
            OnPropertyChanged(property);
        }

        public string ConsoleMessages
        {
            get { return ConsoleMessageLogger.Instance.ConsoleMessageString; }
            private set { }
        }

        public string QuoteStreamMessages
        {
            get { return ConsoleMessageLogger.Instance.QuoteStreamMessageString; }
            private set { }
        }
    }
}
