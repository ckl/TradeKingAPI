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

        public void NotifyPropertyChanged()
        {
            OnPropertyChanged("ConsoleMessages");
        }

        private StringBuilder _consoleMessages = new StringBuilder();
        public string ConsoleMessages
        {
            get { return ConsoleMessageLogger.Instance.ConsoleMessageString; }
            private set { }
        }
    }
}
