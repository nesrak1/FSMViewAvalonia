using FSMViewAvalonia2.Assets;

using Path = System.IO.Path;

namespace FSMViewAvalonia2;

public class FSMLoader
{
    private readonly MainWindow window;

    private readonly Dictionary<(string, string), List<AssetInfo>> fsmListCache = [];
    public FSMLoader(MainWindow window)
    {
        this.window = window;
    }
    public List<AssetInfo> LoadAllFSMsFromBundle(string path, bool loadAsDep = false)
    {
        AssetsManager am = FSMAssetHelper.GetAssetsManager(path);

        BundleFileInstance file = am.LoadBundleFile(path, false);
        _ = am.LoadClassDatabaseFromPackage(file.file.Header.EngineVersion);

        return file.file.GetAllFileNames()
            .Select(x =>
            {
                try
                {
                    return am.LoadAssetsFileFromBundle(file, x, true);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    throw;
                }
            }
                )
            .Where(x => x != null)
            .SelectMany(x => GetFSMInfos(am, x, false))
            .ToList();
    }
    public List<AssetInfo> LoadAllFSMsFromFile(string path, bool loadAsDep = false, bool forceOnly = false)
    {
        bool isLevel = Path.GetFileNameWithoutExtension(path).StartsWith("level");

        AssetsManager am = FSMAssetHelper.GetAssetsManager(path);
        AssetsFileInstance curFile = am.LoadAssetsFile(path, true);
        am.LoadDependencies(curFile);

        _ = am.LoadClassDatabaseFromPackage(curFile.file.Metadata.UnityVersion);

        AssetsFile file = curFile.file;

        List<AssetInfo> result = GetFSMInfos(am, curFile, loadAsDep);
        if (isLevel && Config.config.option_includeSharedassets && !forceOnly)
        {
            foreach (AssetsFileExternal dep in curFile.file.Metadata.Externals.OrderBy(
                a =>
                {
                    string fn = Path.GetFileNameWithoutExtension(a.PathName);
                    return !fn.StartsWith("sharedassets") ? 0 : int.TryParse(fn.AsSpan("sharedassets".Length), out int id) ? id : 0;
                }
                )
                )
            {
                if (Path.GetFileNameWithoutExtension(dep.PathName) == "resources")
                {
                    continue;
                }

                result.AddRange(LoadAllFSMsFromFile(
                    Path.Combine(Path.GetDirectoryName(curFile.path), dep.PathName), true));
            }
        }

        return result;
    }
    public FsmDataInstance LoadFSMWithAssets(long id, AssetInfoUnity assetInfo)
    {
        AssetNameResolver namer = new(assetInfo.am, assetInfo.assetFI);
        AssetTypeValueField fsm = assetInfo.templateField.MakeValue(assetInfo.assetFI.file.Reader,
            assetInfo.assetInfo.GetAbsoluteByteOffset(assetInfo.assetFI.file))["fsm"];
        return new(assetInfo, new AssetsDataProvider(fsm, namer));
    }
    public static FsmDataInstance LoadJsonFSM(string text, AssetInfo assetInfo) => assetInfo.ProviderType != AssetInfo.DataProviderType.Json
            ? throw new NotSupportedException()
            : new(assetInfo, new JsonDataProvider(JToken.Parse(text)));

    public string GetGameObjectPath(AssetsManager am, AssetTypeValueField goAti, AssetsFileInstance curFile)
    {
        try
        {
            StringBuilder pathBuilder = new();

            AssetTypeValueField c_Transform = am.GetExtAsset(curFile, goAti.Get("m_Component").Get(0).Get(0).Get(0)).baseField;
            while (true)
            {
                AssetTypeValueField father = c_Transform.Children.FirstOrDefault(x => x.FieldName == "m_Father");
                if (father == null)
                {
                    break;
                }

                c_Transform = am.GetExtAsset(curFile, father).baseField;
                if (c_Transform == null)
                {
                    break;
                }

                AssetTypeValueField m_GameObject = am.GetExtAsset(curFile, c_Transform.Get("m_GameObject")).baseField;
                string name = m_GameObject.Get("m_Name").AsString;
                _ = pathBuilder.Insert(0, name + "/");
            }

            return pathBuilder.ToString();
        }
        catch (Exception)
        {
            return "";
        }
    }

    private List<AssetInfo> GetFSMInfos(
        AssetsManager am, AssetsFileInstance assetsFile, bool loadAsDep
        )
    {
        if (!Config.config.option_enableFSMListCache)
        {
            return GetFSMInfosDirect(am, assetsFile, loadAsDep);
        }

        (string path, string name) key = (assetsFile.path, assetsFile.name);
        if (!fsmListCache.TryGetValue(key, out List<AssetInfo> result))
        {
            result = GetFSMInfosDirect(am, assetsFile, loadAsDep);
            fsmListCache[key] = result;
        }

        foreach (AssetInfo v in result)
        {
            if (v is AssetInfoUnity vu)
            {
                vu.loadAsDep = loadAsDep;
            }
        }

        return result;
    }

    private List<AssetInfo> GetFSMInfosDirect(
        AssetsManager am, AssetsFileInstance assetsFile, bool loadAsDep
        )
    {
        AssetTypeTemplateField t_pmBS = null;
        AssetTypeTemplateField t_pmBS_data = null;
        AssetTypeTemplateField t_fsmTemplateBS = null;
        AssetTypeTemplateField t_fsmTemplateBS_data = null;
        AssetTypeTemplateField t_fsm = null;


        AssetsFile file = assetsFile.file;
        file.GenerateQuickLookup();
        var assetInfos = new List<AssetInfo>();
        int assetCount = file.AssetInfos.Count;
        AssemblyProvider apr = FSMAssetHelper.GetAssemblyProvider(assetsFile);

        foreach (AssetFileInfo info in file.GetAssetsOfType(AssetClassID.MonoBehaviour))
        {
            AssetTypeTemplateField t_monoBF = am.GetTemplateBaseField(assetsFile, info,
                                AssetReadFlags.SkipMonoBehaviourFields);
            AssetTypeValueField monoBf = am.GetBaseField(assetsFile, info);
            AssetExternal ext = am.GetExtAsset(assetsFile, monoBf.Get("m_Script"));
            if (ext.baseField == null)
            {
                continue;
            }

            AssetTypeValueField scriptAti = ext.baseField;
            string m_ClassName = scriptAti.Get("m_ClassName").AsString;

            if (m_ClassName != "PlayMakerFSM" && m_ClassName != "FsmTemplate")
            {
                continue;
            }

            AssetExternal goAE = am.GetExtAsset(assetsFile, monoBf.Get("m_GameObject"));
            AssetTypeValueField goAti = goAE.baseField;

            string m_Name = "Unnamed";
            if (goAti != null)
            {
                m_Name = goAti.Get("m_Name").AsString;
            }

            if (m_ClassName == "PlayMakerFSM")
            {
                if (t_pmBS == null)
                {
                    t_pmBS = assetsFile.GetTypeTemplateFieldFromAsset(info, "PlayMaker", "",
                                                               "PlayMakerFSM", t_monoBF.Children)
                        .RemoveFieldsAfter("fsm", true);
                    t_fsm ??= t_pmBS.GetField("fsm").RemoveFieldsAfter("name", true);

                }

                t_pmBS_data ??= assetsFile.GetTypeTemplateFieldFromAsset(info, "PlayMaker", "",
                                                               "PlayMakerFSM", t_monoBF.Children);

                AssetTypeValueField pmBf = t_pmBS.MakeValue(file.Reader, info.GetAbsoluteByteOffset(file));
                string fsmName = pmBf["fsm"]["name"].AsString;

                assetInfos.Add(new AssetInfoUnity()
                {
                    id = info.PathId,
                    templateField = t_pmBS_data,
                    assetInfo = info,
                    size = info.ByteSize,
                    name = fsmName,
                    path = GetGameObjectPath(am, goAti, assetsFile),
                    goId = goAE.info.PathId,
                    fsmId = info.PathId,
                    assetFile = assetsFile.path,
                    nameBase = m_Name,
                    assetFI = assetsFile,
                    loadAsDep = loadAsDep,
                    goName = m_Name,
                    assemblyProvider = apr,
                    am = am,
                    isTemplate = false
                });
            }
            else if (m_ClassName == "FsmTemplate") //TODO
            {
                if (t_fsmTemplateBS == null)
                {
                    t_fsmTemplateBS = assetsFile.GetTypeTemplateFieldFromAsset(info, "PlayMaker", "",
                                                               "FsmTemplate", t_monoBF.Children)
                        .RemoveFieldsAfter("fsm", true);
                    t_fsm ??= t_fsmTemplateBS.GetField("fsm").RemoveFieldsAfter("name", true);
                }

                t_fsmTemplateBS_data ??= assetsFile.GetTypeTemplateFieldFromAsset(info, "PlayMaker", "",
                                                               "FsmTemplate", t_monoBF.Children);

                AssetTypeValueField pmBf = t_fsmTemplateBS.MakeValue(file.Reader, info.GetAbsoluteByteOffset(file));
                string fsmName = pmBf["fsm"]["name"].AsString;
                string m_MonoName = monoBf["m_Name"].AsString;

                assetInfos.Add(new AssetInfoUnity()
                {
                    id = info.PathId,
                    templateField = t_fsmTemplateBS_data,
                    assetInfo = info,
                    size = info.ByteSize,
                    name = fsmName,
                    path = "",
                    goId = 0,
                    fsmId = info.PathId,
                    assetFile = assetsFile.path,
                    nameBase = m_Name,
                    assetFI = assetsFile,
                    loadAsDep = loadAsDep,
                    goName = m_Name,
                    assemblyProvider = apr,
                    am = am,
                    isTemplate = true
                });
            }

        }

        assetInfos.Sort((x, y) => x.Name.CompareTo(y.Name));

        return assetInfos;
    }



    public static List<SceneInfo> LoadSceneList()
    {
        AssetTypeValueField buildSettings = GlobalGameManagers.instance.GetAsset(AssetClassID.BuildSettings);
        AssetTypeValueField scenes = buildSettings.Get("scenes").Get("Array");
        int sceneCount = scenes.AsArray.size;

        List<SceneInfo> sceneInfos = [];
        for (int i = 0; i < sceneCount; i++)
        {
            sceneInfos.Add(new SceneInfo()
            {
                id = i,
                name = scenes[i].AsString + " (level" + i + ")",
                level = true
            });
        }

        for (int i = 0; i < sceneCount; i++)
        {
            sceneInfos.Add(new SceneInfo()
            {
                id = i,
                name = scenes[i].AsString + " (sharedassets" + i + ".assets)",
                level = false
            });
        }

        return sceneInfos;
    }
}
