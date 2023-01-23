

namespace FSMViewAvalonia2.Json;

public class JsonDataProvider : IDataProvider
{
    private readonly JToken _token;

    public JsonDataProvider(JToken token) => _token = token;

    private static T Convert<T>(JToken token) => typeof(T) == typeof(INamedAssetProvider)
            ? (T) (object) new JsonNamedAssetProvider(token)
            : token == null
            ? default
            : typeof(T) == typeof(IDataProvider)
            ? (T) (object) new JsonDataProvider(token)
            : typeof(T) == typeof(IDataProvider[])
            ? (T) (object) token.Children().Select(x => x == null ? null : new JsonDataProvider(x)).ToArray()
            : token.ToObject<T>();

    T IDataProvider.As<T>() => Convert<T>(_token);

    T IDataProvider.GetValue<T>(string key)
    {
        JToken token = _token[key];
        return token == null ? default : Convert<T>(token);
    }
}
