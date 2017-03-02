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

        private void TabHeader_OnLeftClick(object sender, MouseButtonEventArgs e)
        {
            var vm = (sender as TextBlock).DataContext as TickerTabItemViewModel;
            if (vm == null)
                return;

            _viewModel.TickerTabsViewModel.SelectedTab.SetRead();
        }

        private void ComboWatchLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null)
                return;

            var current = comboBox.SelectedItem as Models.WatchList;
            if (current == null)
                return;

            _viewModel.CancelStream();
            _viewModel.StreamQuotes.Clear();
            _viewModel.StartStreamingData();

        }

        private void GoogleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var vm = (sender as MenuItem).DataContext as TickerTabItemViewModel;
            if (vm == null)
                return;

            System.Diagnostics.Process.Start("https://www.google.com/finance?q=" + vm.Ticker);
        }
    }
}
