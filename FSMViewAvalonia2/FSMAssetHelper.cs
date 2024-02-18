

using System.Diagnostics;

namespace FSMViewAvalonia2;


public static class FSMAssetHelper
{
    public static AssemblyProvider defaultProvider;


    private readonly static Dictionary<string, AssemblyProvider> assemblyProviders = [];
    private static MonoCecilTempGenerator hkMono;

    public static AssemblyProvider GetAssemblyProvider(AssetsFileInstance assetsFile)
    {
        string path = Path.GetDirectoryName(Path.GetFullPath(assetsFile.path));
        if (string.IsNullOrEmpty(path) || !Directory.Exists(Path.Combine(path, "Managed"))) //Default as hk
        {
            return defaultProvider;
        }

        string dataName = Path.GetFileName(path);
        if (!dataName.Contains("_Data", StringComparison.OrdinalIgnoreCase))
        {
            return defaultProvider;
        }

        dataName = dataName[..dataName.IndexOf("_Data", StringComparison.OrdinalIgnoreCase)].ToLower();
        if (assemblyProviders.TryGetValue(dataName, out AssemblyProvider mono))
        {
            return mono;
        }

        mono = CreateAssemblyProvider(Path.Combine(path, "Managed"));
        assemblyProviders[dataName] = mono;
        return mono;
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

        defaultProvider = CreateAssemblyProvider(GameFileHelper.FindGameFilePath("Managed"));

        hkMono = defaultProvider.mono;
        assemblyProviders["hollow_knight"] = defaultProvider;
        assemblyProviders["hollow knight"] = defaultProvider;

        _ = am.LoadClassPackage("classdata.tpk");
        return am;
    }
}
