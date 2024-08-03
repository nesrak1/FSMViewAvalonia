using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FSMViewAvalonia2.Assets;

namespace FSMViewAvalonia2.Context;
public class GameContext
{
    private readonly Dictionary<(string, string), List<AssetInfo>> fsmListCache = [];
    public readonly AssemblyProvider assemblyProvider;
    public readonly AssetsManager assetsManager;
    public readonly GameId gameId;
    public readonly GameInfo gameInfo;

    public readonly GlobalGameManagers gameManagers;

    public GameContext(GameInfo gameInfo, GameId? id)
    {
        this.gameInfo = gameInfo;
        gameId = id ?? GameId.FromName(gameInfo.Name);
        assetsManager = CreateAssetManager();
        gameManagers = new(this);
        assemblyProvider = CreateAssemblyProvider(GameFileHelper.FindGameFilePath("Managed", false, gameInfo));
    }
    private AssetsManager CreateAssetManager()
    {
        AssetsManager am = new()
        {
            UseQuickLookup = true,
            UseTemplateFieldCache = true
        };
        if (!File.Exists("classdata.tpk"))
        {
            return null;
        }

        _ = am.LoadClassPackage("classdata.tpk");
        return am;
    }
    private AssemblyProvider CreateAssemblyProvider(string managedPath)
    {
        var assemblies = new List<AssemblyDefinition>();
        foreach (string v in Directory.EnumerateFiles(managedPath, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                var dll = AssemblyDefinition.ReadAssembly(v, new()
                {
                    AssemblyResolver = new AssemblyResolver(managedPath)
                });
                assemblies.Add(dll);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        var mono = new MonoCecilTempGenerator(managedPath);

        return new()
        {
            mono = mono,
            assemblies = assemblies
        };
    }

    private List<AssetInfo> GetFSMInfosDirect(
        AssetsFileInstance assetsFile, bool loadAsDep
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
        AssemblyProvider apr = assemblyProvider;
        var am = assetsManager;

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
                    t_pmBS = assetsFile.GetTypeTemplateFieldFromAsset(this, info, "PlayMaker", "",
                                                               "PlayMakerFSM", t_monoBF.Children)
                        .RemoveFieldsAfter("fsm", true);
                    t_fsm ??= t_pmBS.GetField("fsm").RemoveFieldsAfter("name", true);

                }

                t_pmBS_data ??= assetsFile.GetTypeTemplateFieldFromAsset(this, info, "PlayMaker", "",
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
                    path = Utils2.GetGameObjectPath(am, goAti, assetsFile),
                    goId = goAE.info.PathId,
                    fsmId = info.PathId,
                    assetFile = assetsFile.path,
                    nameBase = m_Name,
                    assetFI = assetsFile,
                    loadAsDep = loadAsDep,
                    goName = m_Name,
                    assemblyProvider = apr,
                    context = this,
                    isTemplate = false
                });
            }
            else if (m_ClassName == "FsmTemplate") //TODO
            {
                if (t_fsmTemplateBS == null)
                {
                    t_fsmTemplateBS = assetsFile.GetTypeTemplateFieldFromAsset(this, info, "PlayMaker", "",
                                                               "FsmTemplate", t_monoBF.Children)
                        .RemoveFieldsAfter("fsm", true);
                    t_fsm ??= t_fsmTemplateBS.GetField("fsm").RemoveFieldsAfter("name", true);
                }

                t_fsmTemplateBS_data ??= assetsFile.GetTypeTemplateFieldFromAsset(this, info, "PlayMaker", "",
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
                    context = this,
                    isTemplate = true
                });
            }

        }

        assetInfos.Sort((x, y) => x.Name.CompareTo(y.Name));

        return assetInfos;
    }

    public List<AssetInfo> GetFSMInfos(
        AssetsFileInstance assetsFile, bool loadAsDep
       )
    {
        var am = assetsManager;
        if (!Config.config.option_enableFSMListCache)
        {
            return GetFSMInfosDirect(assetsFile, loadAsDep);
        }

        (string path, string name) key = (assetsFile.path, assetsFile.name);
        if (!fsmListCache.TryGetValue(key, out List<AssetInfo> result))
        {
            result = GetFSMInfosDirect(assetsFile, loadAsDep);
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

    public List<AssetInfo> LoadAllFSMsFromFile(string path, bool loadAsDep = false, bool forceOnly = false)
    {
        bool isLevel = Path.GetFileNameWithoutExtension(path).StartsWith("level");
        
        AssetsManager am = assetsManager;
        AssetsFileInstance curFile = am.LoadAssetsFile(path, true);
        am.LoadDependencies(curFile);

        _ = am.LoadClassDatabaseFromPackage(curFile.file.Metadata.UnityVersion);

        AssetsFile file = curFile.file;

        List<AssetInfo> result = GetFSMInfos(curFile, loadAsDep);
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

    public List<AssetInfo> LoadAllFSMsFromBundle(string path, bool loadAsDep = false)
    {
        AssetsManager am = assetsManager;

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
            .SelectMany(x => GetFSMInfos(x, false))
            .ToList();
    }
    public List<SceneInfo> LoadSceneList()
    {
        AssetTypeValueField buildSettings = gameManagers.GetAsset(AssetClassID.BuildSettings);
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
