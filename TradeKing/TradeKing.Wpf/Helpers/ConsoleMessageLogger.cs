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
            _viewModel.NotifyPropertyChanged();
        }

        private static object myLock = new object();
        private static volatile ConsoleMessageLogger myLogger = null; // 'volatile' is unnecessary in .NET 2.0 and later
        private ConsoleMessageViewModel _viewModel = null;

        private StringBuilder _consoleMessageString = new StringBuilder();
        public string ConsoleMessageString
        {
            get { return _consoleMessageString.ToString(); }
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
