namespace FSMViewAvalonia2;

internal class GlobalGameManagers
{
    public static GlobalGameManagers instance;
    public AssetsManager am;
    public AssetsFileInstance file;

    public GlobalGameManagers(AssetsManager am)
    {
        instance = this;
        string ggmPath = GameFileHelper.FindGameFilePath("globalgamemanagers");
        file = am.LoadAssetsFile(ggmPath, false);
        _ = am.LoadClassDatabaseFromPackage(file.file.typeTree.unityVersion);
        this.am = am;
    }

    public AssetTypeInstance GetAsset(AssetClassID id) => am.GetTypeInstance(file, file.table.GetAssetsOfType((int) id)[0]);
}
