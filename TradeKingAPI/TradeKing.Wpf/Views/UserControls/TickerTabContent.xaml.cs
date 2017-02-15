using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TradeKing.Wpf.ViewModels;

namespace TradeKing.Wpf.Views.UserControls
{
    /// <summary>
    /// Interaction logic for TickerTabContent.xaml
    /// </summary>
    public partial class TickerTabContent : UserControl
    {
        public TickerTabContent()
        {
            InitializeComponent();
        }

        public void SetViewModel(TickerTabItemViewModel vm)
        {
            this.DataContext = vm;
        }
    }
}
