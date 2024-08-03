using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2.Context;
public abstract class GameIsolate<T>
{
    private readonly ConcurrentDictionary<GameId, T> games = [];

    public bool Exists(GameId id) => games.ContainsKey(id);
    public void Remove(GameId id) => games.Remove(id, out _);

    protected abstract T Create(GameId id);
    public T Get(GameId id)
    {
        if(!games.TryGetValue(id, out var result))
        {
            result = games.AddOrUpdate(id, Create, (_, orig) => orig);
        }
        return result;
    }
}
public class DefaultGameIsolate<T>
{
    private readonly ConcurrentDictionary<GameId, T> games = [];

    public bool Exists(GameId id) => games.ContainsKey(id);
    public void Remove(GameId id) => games.Remove(id, out _);

    private readonly Func<GameId, T> factory;

    public DefaultGameIsolate(Func<GameId, T> factory)
    {
        this.factory = factory;
    }

    protected T Create(GameId id)
    {
        return factory(id);
    }
    public T Get(GameId id)
    {
        if (!games.TryGetValue(id, out var result))
        {
            result = games.AddOrUpdate(id, Create, (_, orig) => orig);
        }
        return result;
    }
}
