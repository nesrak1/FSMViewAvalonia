using FSMViewAvalonia2.Context;

namespace FSMViewAvalonia2;

internal class GlobalGameManagers
{
    private static readonly DefaultGameIsolate<GlobalGameManagers> managers = new(id =>
    {
        var info = GameFileHelper.GetGameInfoFromId(id);
        if (info == null)
        {
            return null;
        }

        return new(info);

    });
    public AssetsManager am;
    public AssetsFileInstance file;

    private GlobalGameManagers(GameFileHelper.GameInfo info)
    {
        am = FSMAssetHelper.CreateAssetManager();
        string ggmPath = GameFileHelper.FindGameFilePath("globalgamemanagers", true, info);
        file = am.LoadAssetsFile(ggmPath, false);
        _ = am.LoadClassDatabaseFromPackage(file.file.Metadata.UnityVersion);
    }

    public static GlobalGameManagers Get(GameId id)
    {
        if(id.IsNone)
        {
            return null;
        }

        return managers.Get(id);
    }

    public AssetTypeValueField GetAsset(AssetClassID id) => am.GetBaseField(file, file.file.GetAssetsOfType((int) id)[0]);
}
