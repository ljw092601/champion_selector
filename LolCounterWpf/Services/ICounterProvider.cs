using LolCounterWpf.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LolCounterWpf.Services;

public interface ICounterProvider
{
    Task<IReadOnlyList<CounterEntry>> GetCountersAsync(string championIdOrName);
    Task AddCounterAsync(string championId, CounterEntry newEntry);
    Task UpdateCounterAsync(string championId, string originalOpponentName, CounterEntry updatedEntry);
    Task DeleteCounterAsync(string championId, string opponentName);
}