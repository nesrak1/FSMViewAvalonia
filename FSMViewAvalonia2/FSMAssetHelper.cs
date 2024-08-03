

using System.Diagnostics;

namespace FSMViewAvalonia2;


public static class FSMAssetHelper
{
    public static AssemblyProvider defaultProvider;


    private readonly static Dictionary<GameId, AssemblyProvider> assemblyProviders = [];
    
    public static AssemblyProvider GetAssemblyProvider(AssetsFileInstance assetsFile)
    {
        var path = assetsFile.path;
        var gameId = GameId.FromPath(path);
        if (gameId.IsNone)
        {
            return GetAssemblyProvider(GameFileHelper.CurrentGameId);
        }

        return GetAssemblyProvider(gameId);
    }
    public static AssemblyProvider GetAssemblyProvider(GameId gameId)
    {
        if (gameId.IsNone)
        {
            return defaultProvider;
        }

        if (assemblyProviders.TryGetValue(gameId, out AssemblyProvider mono))
        {
            return mono;
        }

        mono = CreateAssemblyProvider(
            GameFileHelper.FindGameFilePath("Managed", false, GameFileHelper.GetGameInfoFromId(gameId))
            );
        assemblyProviders[gameId] = mono;
        return mono;
    }

    public static MonoCecilTempGenerator GetMonoCTG(AssetsFileInstance assetsFile)
    {
        return GetAssemblyProvider(assetsFile).mono;
    }

    private static AssemblyProvider CreateAssemblyProvider(string managedPath)
    {
        if(!Directory.Exists(managedPath))
        {
            return defaultProvider;
        }
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

        var defaultManaged = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default_managed");
        if (Directory.Exists(defaultManaged))
        {
            defaultProvider = CreateAssemblyProvider(defaultManaged);
        }
        else
        {
            defaultManaged = null;
        }
        
    }
    public static AssetsManager CreateAssetManager()
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
