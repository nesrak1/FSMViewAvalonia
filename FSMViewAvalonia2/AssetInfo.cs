

namespace FSMViewAvalonia2;

public class AssetInfo
{
    public long id;
    public uint size;
    public string name;
    public string nameBase;
    public string path;
    public string assetFile;
    public long fsmId;
    public long goId;
    public bool loadAsDep;
    public AssetsFileInstance assetFI;
    public DataProviderType providerType = DataProviderType.Assets;
    public string Name
    {
        get
        {
            if(loadAsDep)
            {
                return "[LoadAsDep]" + path + name + " (" + Path.GetFileNameWithoutExtension(assetFile) + ")";
            } else
            {
                return path + name;
            }
        }
    }
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
