

namespace FSMViewAvalonia2
{
    public static class FSMAssetHelper
    {
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
            am.LoadClassPackage("classdata.tpk");
            return am;
        }
    }
}
