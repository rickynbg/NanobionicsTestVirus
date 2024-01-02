using NanobionicsTestVirusDesktop.Models;
using NanobionicsTestVirusDesktop.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NanobionicsTestVirusDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BarTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MeasuresList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is not TreeView tv)
            {
                return;
            }

            if (tv.DataContext is not MainViewModel vm)
            {
                return;
            }

            if (tv.SelectedItem is FileMeasure)
            {
                vm.Ms.SelectedItem = tv.SelectedItem as FileMeasure;
            }
            else
            {
                vm.Ms.SelectDM = tv.SelectedItem as DataMeasure;
            }
        }
    }
}
