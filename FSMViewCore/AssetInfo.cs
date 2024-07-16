

namespace FSMViewAvalonia2;



public class AssetInfo
{
    public long id;
    public string name;
    public string nameBase;
    public string path;
    public string assetFile;
    public virtual DataProviderType ProviderType => DataProviderType.Json;
    public IAssemblyProvider assemblyProvider = null;
    public virtual string Name => path + name;
    public enum DataProviderType
    {
        Assets, Json
    }
}

