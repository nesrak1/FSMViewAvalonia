

namespace FSMViewAvalonia2.Data;

public interface IDataProvider
{
    T GetValue<T>(string key);
    T Get<T>(string key) => GetValue<T>(key);
    T As<T>();
}
