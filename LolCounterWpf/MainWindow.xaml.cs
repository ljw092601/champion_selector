using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LolCounterWpf.Services;
using LolCounterWpf.ViewModels;

namespace LolCounterWpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var ddragon = new DataDragonService();
        var counterService = new CounterService("assets/counters.json");
        DataContext = new MainViewModel(ddragon, counterService);
    }

    private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        DependencyObject current = e.OriginalSource as DependencyObject;

        while (current != null && current != this)
        {
            if (current is ScrollViewer scrollViewer && scrollViewer.ScrollableHeight > 0)
            {
                if (e.Delta < 0)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 48);
                }
                else
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 48);
                }
                e.Handled = true;
                return;
            }
            current = VisualTreeHelper.GetParent(current);
        }
    }
}
