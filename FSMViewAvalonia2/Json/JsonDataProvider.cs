

namespace FSMViewAvalonia2.Json;

public class JsonDataProvider : IDataProvider
{
    private readonly JToken _token;

    public JsonDataProvider(JToken token) => _token = token;

    private static object Convert(Type type, JToken token) => type == typeof(INamedAssetProvider)
            ? new JsonNamedAssetProvider(token)
            : token == null
            ? default
            : type == typeof(IDataProvider)
            ? new JsonDataProvider(token)
            : type == typeof(IDataProvider[])
            ? token.Children().Select(x => x == null ? null : new JsonDataProvider(x)).ToArray()
            : type.IsArray
            ? token.Children().Select(x => x.ToObject(type.GetElementType())).ToArray().Convert(type.GetElementType())
            : token.ToObject(type);

    T IDataProvider.As<T>() => (T) Convert(typeof(T), _token);

    T IDataProvider.GetValue<T>(string key)
    {
        JToken token = _token[key];
        return token == null ? default : (T) Convert(typeof(T), token);
    }
}
