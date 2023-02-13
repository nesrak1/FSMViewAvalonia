

namespace FSMViewAvalonia2;

public class AssetInfoUnity : AssetInfo
{
    public AssetTypeValueField data;
    public uint size;
    public bool loadAsDep;
    public AssetsFileInstance assetFI;
    public long fsmId;
    public long goId;
    public override DataProviderType ProviderType => DataProviderType.Assets;
    public override string Name => loadAsDep ? "[LoadAsDep]" + path + name + " (" + Path.GetFileNameWithoutExtension(assetFile) + ")" : path + name;
}

public class AssetInfo
{
    public long id;
    public string name;
    public string nameBase;
    public string path;
    public string assetFile;
    public virtual DataProviderType ProviderType => DataProviderType.Json;
    public virtual string Name => path + name;
    public enum DataProviderType
    {
        Assets, Json
    }
}

public struct SceneInfo
{
    public long id;
    public string name;
    public bool level;

    public string Name => name;
}
