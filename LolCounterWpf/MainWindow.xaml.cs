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
        var counterProvider = new LocalJsonCounterProvider("assets/counters.json");
        var counterManager = new CounterManagerService("assets/counters.json");
        DataContext = new MainViewModel(ddragon, counterProvider, counterManager);
    }
}