

namespace FSMViewAvalonia2.Json
{
    public class JsonDataProvider : IDataProvider
    {
        private readonly JToken _token;

        public JsonDataProvider(JToken token)
        {
            _token = token;
        }

        private static T Convert<T>(JToken token)
        {
            if(typeof(T) == typeof(INamedAssetProvider))
            {
                return (T)(object)new JsonNamedAssetProvider(token);
            }
            if (token == null) return default;
            if(typeof(T) == typeof(IDataProvider))
            {
                return (T)(object)new JsonDataProvider(token);
            }
            if(typeof(T) == typeof(IDataProvider[]))
            {
                return (T)(object)token.Children().Select(x => x == null ? null : new JsonDataProvider(x)).ToArray();
            }
            
            return token.ToObject<T>();
        }

        T IDataProvider.As<T>() => Convert<T>(_token);

        T IDataProvider.GetValue<T>(string key)
        {
            var token = _token[key];
            if(token == null) return default;
            return Convert<T>(token);
        }
    }
}
