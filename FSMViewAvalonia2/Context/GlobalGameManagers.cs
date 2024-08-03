namespace FSMViewAvalonia2.Context;

public class GlobalGameManagers
{
    public readonly GameContext context;
    private readonly AssetsFileInstance file;
    internal GlobalGameManagers(GameContext ctx)
    {
        context = ctx;
        string ggmPath = GameFileHelper.FindGameFilePath("globalgamemanagers", true, context.gameInfo);

        file = context.assetsManager.LoadAssetsFile(ggmPath, false);
        _ = context.assetsManager.LoadClassDatabaseFromPackage(file.file.Metadata.UnityVersion);
    }

    public AssetTypeValueField GetAsset(AssetClassID id) => context.assetsManager.GetBaseField(file, file.file.GetAssetsOfType((int) id)[0]);
}
