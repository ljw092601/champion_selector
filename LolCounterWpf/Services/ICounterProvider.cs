using LolCounterWpf.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LolCounterWpf.Services;

public interface ICounterProvider
{
    Task<IReadOnlyList<CounterEntry>> GetCountersAsync(string championIdOrName);
}