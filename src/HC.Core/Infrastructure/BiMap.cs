using System;
using System.Collections.Generic;

namespace HC.Core.Infrastructure;

public class BiMap
{
    private readonly Dictionary<string, Type> _firstToSecond = [];
    private readonly Dictionary<Type, string> _secondToFirst = [];

    public void Add(string first, Type second)
    {
        if (_firstToSecond.ContainsKey(first) || _secondToFirst.ContainsKey(second))
            throw new ArgumentException("Duplicate first or second");
        _firstToSecond.Add(first, second);
        _secondToFirst.Add(second, first);
    }

    public bool TryGetByFirst(string first, out Type? second)
        => _firstToSecond.TryGetValue(first, out second);

    public bool TryGetBySecond(Type second, out string? first)
        => _secondToFirst.TryGetValue(second, out first);
}
