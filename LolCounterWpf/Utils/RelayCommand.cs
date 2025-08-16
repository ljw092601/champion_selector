using System.Threading.Tasks;
using System;
using System.Windows.Input;

namespace LolCounterWpf.Utils;

public sealed class RelayCommand : ICommand
{
    private readonly Predicate<object?>? _can;
    private readonly Func<object?, Task> _execAsync;

    public RelayCommand(Func<object?, Task> execAsync, Predicate<object?>? can = null)
    { _execAsync = execAsync; _can = can; }

    public bool CanExecute(object? parameter) => _can?.Invoke(parameter) ?? true;

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();

    public async void Execute(object? parameter) => await _execAsync(parameter);
}