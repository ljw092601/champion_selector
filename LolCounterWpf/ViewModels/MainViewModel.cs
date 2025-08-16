using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LolCounterWpf.Models;
using LolCounterWpf.Services;
using LolCounterWpf.Utils;

namespace LolCounterWpf.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly DataDragonService _ddragon;
    private readonly ICounterProvider _counterProvider;
    private IReadOnlyList<Champion> _champions = Array.Empty<Champion>();
    private Champion? _currentChampion;

    public ObservableCollection<Champion> Suggestions { get; } = new();
    public ObservableCollection<CounterEntry> Counters { get; } = new();

    private string _title = "챔피언을 검색하세요";
    public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

    private string _query = string.Empty;
    public string Query
    {
        get => _query;
        set { _query = value; OnPropertyChanged(); UpdateSuggestions(); }
    }

    private Champion? _selectedSuggestion;
    public Champion? SelectedSuggestion
    {
        get => _selectedSuggestion;
        set { _selectedSuggestion = value; OnPropertyChanged(); if (value is not null) _ = LoadCountersAsync(value); }
    }

    private CounterEntry? _selectedCounter;
    public CounterEntry? SelectedCounter
    {
        get => _selectedCounter;
        set
        {
            _selectedCounter = value;
            OnPropertyChanged();
            if (value is not null)
            {
                EditOpponentName = value.OpponentName;
                EditDetail = value.Detail;
            }
            UpdateCounterCommand.RaiseCanExecuteChanged();
            DeleteCounterCommand.RaiseCanExecuteChanged();
        }
    }

    private string _editOpponentName = string.Empty;
    public string EditOpponentName { get => _editOpponentName; set { _editOpponentName = value; OnPropertyChanged(); } }

    private string _editDetail = string.Empty;
    public string EditDetail { get => _editDetail; set { _editDetail = value; OnPropertyChanged(); } }


    public Visibility SuggestionsVisible => Suggestions.Count > 0 && !string.IsNullOrWhiteSpace(Query)
        ? Visibility.Visible : Visibility.Collapsed;

    public RelayCommand SearchCommand { get; }
    public RelayCommand AddCounterCommand { get; }
    public RelayCommand UpdateCounterCommand { get; }
    public RelayCommand DeleteCounterCommand { get; }


    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public MainViewModel(DataDragonService ddragon, ICounterProvider counterProvider)
    {
        _ddragon = ddragon;
        _counterProvider = counterProvider;

        SearchCommand = new RelayCommand(async _ => await SearchAsync());
        AddCounterCommand = new RelayCommand(async _ => await AddCounterAsync(), _ => _currentChampion is not null);
        UpdateCounterCommand = new RelayCommand(async _ => await UpdateCounterAsync(), _ => SelectedCounter is not null);
        DeleteCounterCommand = new RelayCommand(async _ => await DeleteCounterAsync(), _ => SelectedCounter is not null);

        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        _champions = await _ddragon.GetChampionsAsync("ko_KR");
    }

    private void UpdateSuggestions()
    {
        Suggestions.Clear();
        if (string.IsNullOrWhiteSpace(Query)) { OnPropertyChanged(nameof(SuggestionsVisible)); return; }
        var query = Query.Trim();
        foreach (var c in _champions.Where(c => c.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)))
            Suggestions.Add(c);
        OnPropertyChanged(nameof(SuggestionsVisible));
    }

    private async Task SearchAsync()
    {
        var c = _champions.FirstOrDefault(c => c.Name.Equals(Query.Trim(), StringComparison.CurrentCultureIgnoreCase))
              ?? Suggestions.FirstOrDefault();
        if (c is not null) await LoadCountersAsync(c);
    }

    private async Task LoadCountersAsync(Champion champ)
    {
        _currentChampion = champ;
        AddCounterCommand.RaiseCanExecuteChanged();

        Title = $"{champ.Name}의 카운터";
        Suggestions.Clear(); OnPropertyChanged(nameof(SuggestionsVisible));
        Counters.Clear();

        var entries = await _counterProvider.GetCountersAsync(champ.Id) // Id 우선 조회
                   ?? await _counterProvider.GetCountersAsync(champ.Name);

        foreach (var e in entries)
        {
            var target = _champions.FirstOrDefault(x => x.Id.Equals(e.OpponentId, StringComparison.OrdinalIgnoreCase))
                      ?? _champions.FirstOrDefault(x => x.Name.Equals(e.OpponentName, StringComparison.OrdinalIgnoreCase));
            if (target is not null) e.ImageUrl = target.ImageUrl;
            Counters.Add(e);
        }
        if (Counters.Count == 0) Title += " — 데이터 없음";
    }

    private async Task AddCounterAsync()
    {
        if (_currentChampion is null || string.IsNullOrWhiteSpace(EditOpponentName)) return;

        var opponent = _champions.FirstOrDefault(c => c.Name.Equals(EditOpponentName.Trim(), StringComparison.CurrentCultureIgnoreCase));
        if (opponent is null)
        {
            MessageBox.Show("존재하지 않는 챔피언 이름입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var newEntry = new CounterEntry
        {
            OpponentId = opponent.Id,
            OpponentName = opponent.Name,
            Detail = EditDetail
        };

        await _counterProvider.AddCounterAsync(_currentChampion.Id, newEntry);
        await LoadCountersAsync(_currentChampion); // Refresh
    }

    private async Task UpdateCounterAsync()
    {
        if (_currentChampion is null || SelectedCounter is null || string.IsNullOrWhiteSpace(EditOpponentName)) return;

        var opponent = _champions.FirstOrDefault(c => c.Name.Equals(EditOpponentName.Trim(), StringComparison.CurrentCultureIgnoreCase));
        if (opponent is null)
        {
            MessageBox.Show("존재하지 않는 챔피언 이름입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var updatedEntry = new CounterEntry
        {
            OpponentId = opponent.Id,
            OpponentName = opponent.Name,
            Detail = EditDetail
        };

        await _counterProvider.UpdateCounterAsync(_currentChampion.Id, SelectedCounter.OpponentName, updatedEntry);
        await LoadCountersAsync(_currentChampion); // Refresh
    }

    private async Task DeleteCounterAsync()
    {
        if (_currentChampion is null || SelectedCounter is null) return;

        await _counterProvider.DeleteCounterAsync(_currentChampion.Id, SelectedCounter.OpponentName);
        await LoadCountersAsync(_currentChampion); // Refresh
    }
}