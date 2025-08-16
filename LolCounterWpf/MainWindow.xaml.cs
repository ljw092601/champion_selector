using System.Windows;
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
}