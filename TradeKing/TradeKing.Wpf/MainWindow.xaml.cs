using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TradeKing.Wpf.ViewModels;

namespace TradeKing.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null)
                return;

            var ticker = listView.SelectedItem as string;
            if (ticker == null)
                return;

            _viewModel.TickerTabsViewModel.AddTab(ticker);
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var vm = (sender as MenuItem).DataContext as TickerTabItemViewModel;
            if (vm == null)
                return;

            _viewModel.TickerTabsViewModel.RemoveTab(vm);
        }
    }
}
