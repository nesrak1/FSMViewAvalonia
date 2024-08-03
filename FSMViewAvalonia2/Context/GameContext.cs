using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2.Context;
public class GameContext
{

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
}
