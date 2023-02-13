

namespace FSMViewAvalonia2;

public static class FSMAssetHelper
{
    public static MonoCecilTempGenerator mono;
    public static AssetsManager CreateAssetManager()
    {
        AssetsManager am = new()
        {
            updateAfterLoad = false,
            useTemplateFieldCache = true
        };
        if (!File.Exists("classdata.tpk"))
        {
            return null;
        }
        mono = new(GameFileHelper.FindGameFilePath("Managed"));
        //am.SetMonoTempGenerator(
        //    mono
        //    );
        
        _ = am.LoadClassPackage("classdata.tpk");
        return am;
    }
}
