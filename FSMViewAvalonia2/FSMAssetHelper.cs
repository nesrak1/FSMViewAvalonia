using System.Diagnostics;

namespace FSMViewAvalonia2;


public static class FSMAssetHelper
{
    public static AssemblyProvider defaultProvider;


    private readonly static Dictionary<string, AssemblyProvider> assemblyProviders = [];
    private readonly static Dictionary<string, AssetsManager> assetsManagers = [];
    private static MonoCecilTempGenerator hkMono;

    public static string GetGameId(string gamePath)
    {
        string path = Path.GetDirectoryName(Path.GetFullPath(gamePath));
        if (string.IsNullOrEmpty(path) || !Directory.Exists(Path.Combine(path, "Managed"))) //Default as hk
        {
            return null;
        }

        string dataName = Path.GetFileName(path);
        if (!dataName.Contains("_Data", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return dataName.ToLower();
    }

    public static AssemblyProvider GetAssemblyProvider(AssetsFileInstance assetsFile)
    {
        string dataName = GetGameId(assetsFile.path);
        if(dataName == null)
        {
            return defaultProvider;
        }

        if (assemblyProviders.TryGetValue(dataName, out AssemblyProvider mono))
        {
            return mono;
        }

        mono = CreateAssemblyProvider(
            Path.Combine(
            Path.GetDirectoryName(Path.GetFullPath(assetsFile.path)),
            "Managed"));
        assemblyProviders[dataName] = mono;
        return mono;
    }
    public static AssetsManager GetAssetsManager(string path)
    {
        var gd = GetGameId(path);
        if(gd == null)
        {
            return CreateAssetManager();
        }
        if(assetsManagers.TryGetValue(gd, out AssetsManager manager))
        {
            return manager;
        }
        manager = CreateAssetManager();
        assetsManagers[gd] = manager;
        return manager;
    }
    public static MonoCecilTempGenerator GetMonoCTG(AssetsFileInstance assetsFile)
    {
        return GetAssemblyProvider(assetsFile).mono;
    }

    private static AssemblyProvider CreateAssemblyProvider(string managedPath)
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
    public static void Init()
    {
        defaultProvider = CreateAssemblyProvider(GameFileHelper.FindGameFilePath("Managed"));

        hkMono = defaultProvider.mono;
        assemblyProviders["hollow_knight"] = defaultProvider;
        assemblyProviders["hollow knight"] = defaultProvider;

    }
    private static AssetsManager CreateAssetManager()
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
}
