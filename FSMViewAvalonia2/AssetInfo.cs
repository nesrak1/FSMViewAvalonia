

namespace FSMViewAvalonia2;

public class AssetInfoUnity : AssetInfo
{
    public required AssetsManager am;
    public required uint size;
    public bool loadAsDep;
    public required AssetFileInfo assetInfo;
    public required AssetsFileInstance assetFI;
    public long fsmId;
    public long goId;
    public required AssetTypeTemplateField templateField;
    public override DataProviderType ProviderType => DataProviderType.Assets;
    public override string Name => loadAsDep ? "[LoadAsDep]" + path + name + " (" + Path.GetFileNameWithoutExtension(assetFile) + ")" : path + name;
}


public struct SceneInfo
{
    public long id;
    public string name;
    public bool level;

    public string Name => name;
}
