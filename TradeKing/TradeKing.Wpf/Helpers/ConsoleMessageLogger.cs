using System;
using System.Text;
using TradeKing.Wpf.ViewModels;

namespace TradeKing.Wpf.Helpers
{
    public class ConsoleMessageLogger
    {
        public void Log(string str)
        {
            var msg = DateTime.Now + ": " + str;
            //_consoleMessageString.AppendLine(msg);
            _consoleMessageString.Insert(0, msg + Environment.NewLine);
            Console.WriteLine(msg);
            _viewModel.NotifyPropertyChanged("ConsoleMessages");
        }

        public void LogQuoteStreamMessage(string str)
        {
            var msg = DateTime.Now + ": " + str;
            //_consoleMessageString.AppendLine(msg);
            _quoteStreamMessageString.Insert(0, msg + Environment.NewLine);
            //Console.WriteLine(msg);
            _viewModel.NotifyPropertyChanged("QuoteStreamMessages");
        }

        private static object myLock = new object();
        private static ConsoleMessageLogger myLogger = null; 
        private ConsoleMessageViewModel _viewModel = null;

        private StringBuilder _consoleMessageString = new StringBuilder();
        public string ConsoleMessageString
        {
            get { return _consoleMessageString.ToString(); }
            private set { }
        }

        private StringBuilder _quoteStreamMessageString = new StringBuilder();
        public string QuoteStreamMessageString
        {
            get { return _quoteStreamMessageString.ToString(); }
            private set { }
        }

        private ConsoleMessageLogger()
        {
        }
        
        public void SetViewModel(ConsoleMessageViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public static ConsoleMessageLogger Instance
        {
            get
            {
                if (myLogger == null)
                {
                    lock (myLock)
                    {
                        if (myLogger == null)
                        {
                            myLogger = new ConsoleMessageLogger();

                        }
                    }
                }


                return myLogger;
            }
            private set {  }
        }
    }
}
