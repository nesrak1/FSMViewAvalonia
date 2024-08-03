using FSMViewAvalonia2.Assets;
using FSMViewAvalonia2.Context;

using Path = System.IO.Path;

namespace FSMViewAvalonia2;

public class FSMLoader
{
    private readonly MainWindow window;

    
    private readonly DefaultGameIsolate<GameContext> gameCtx = new(id => new(GameFileHelper.GetGameInfoFromId(id), id));
    public GameContext CurrentGame => gameCtx.Get(GameFileHelper.CurrentGameId);
    public FSMLoader(MainWindow window)
    {
        this.window = window;
    }
    public List<AssetInfo> LoadAllFSMsFromBundle(string path, bool loadAsDep = false)
    {
        var ctx = gameCtx.Get(GameId.FromPath(path));
        return ctx.LoadAllFSMsFromBundle(path, loadAsDep);
    }

    public List<AssetInfo> LoadAllFSMsFromFile(string path, bool loadAsDep = false, bool forceOnly = false)
    {
        var ctx = gameCtx.Get(GameId.FromPath(path));

        return ctx.LoadAllFSMsFromFile(path, loadAsDep, forceOnly);
    }
    public FsmDataInstance LoadFSMWithAssets(long id, AssetInfoUnity assetInfo)
    {
        AssetNameResolver namer = new(assetInfo.context.assetsManager, assetInfo.assetFI);
        AssetTypeValueField fsm = assetInfo.templateField.MakeValue(assetInfo.assetFI.file.Reader,
            assetInfo.assetInfo.GetAbsoluteByteOffset(assetInfo.assetFI.file))["fsm"];
        return new(assetInfo, new AssetsDataProvider(fsm, namer));
    }
    public static FsmDataInstance LoadJsonFSM(string text, AssetInfo assetInfo) => assetInfo.ProviderType != AssetInfo.DataProviderType.Json
            ? throw new NotSupportedException()
            : new(assetInfo, new JsonDataProvider(JToken.Parse(text)));

    public List<SceneInfo> LoadSceneList()
    {
        return CurrentGame.LoadSceneList();
    }
}
