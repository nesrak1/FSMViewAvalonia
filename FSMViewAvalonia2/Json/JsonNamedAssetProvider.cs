

namespace FSMViewAvalonia2.Json
{
    public class JsonNamedAssetProvider : INamedAssetProvider
    {
        public JsonNamedAssetProvider(JToken token)
        {
            if(token == null || !token.HasValues)
            {
                isNull = true;
                return;
            }
            name = token["objName"].ToString();
            file = token["objFile"]?.ToString();
        }
        public bool isNull { get; init; }

        public string name { get; init; }

        public string file { get; init; }
        public override string ToString() => name + (file == null ? $"[{file}]" : "");
    }
}
