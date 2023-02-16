

namespace FSMViewAvalonia2;

public static class FSMAssetHelper
{
    public static MonoCecilTempGenerator mono;
    public static AssetsManager CreateAssetManager()
    {
        AssetsManager am = new()
        {
            UpdateAfterLoad = false,
            UseTemplateFieldCache = true
        };
        if (!File.Exists("classdata.tpk"))
        {
            return null;
        }

        if (FSMLoader.mainAssembly == null)
        {
            string map = Path.Combine(GameFileHelper.FindGameFilePath("Managed"), "Assembly-CSharp.dll");
            FSMLoader. mainAssembly = AssemblyDefinition.ReadAssembly(map, new()
            {
                AssemblyResolver = new AssemblyResolver(GameFileHelper.FindGameFilePath("Managed"))
            });
        }

        mono = new(GameFileHelper.FindGameFilePath("Managed"));
        //am.SetMonoTempGenerator(
        //    mono
        //    );
        _ = am.LoadClassPackage("classdata.tpk");
        return am;
    }
}
