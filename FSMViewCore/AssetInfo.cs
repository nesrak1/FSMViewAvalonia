

namespace FSMViewAvalonia2;



public class AssetInfo
{
    public long id;
    public string goName;
    public string name;
    public string nameBase;
    public string path;
    public string assetFile;
    public bool isTemplate;
    public virtual DataProviderType ProviderType => DataProviderType.Json;
    public IAssemblyProvider assemblyProvider = null;
    public virtual string Name => path + goName + "-" + name + (isTemplate ? " (template)" : "");
    public enum DataProviderType
    {
        Assets, Json
    }
}

