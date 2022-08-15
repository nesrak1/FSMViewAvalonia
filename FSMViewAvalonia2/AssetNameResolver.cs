

namespace FSMViewAvalonia2
{
    public class AssetNameResolver
    {
        internal AssetsManager am;
        internal AssetsFileInstance inst;
        public AssetNameResolver(AssetsManager am, AssetsFileInstance inst)
        {
            this.am = am;
            this.inst = inst;
        }
    }
}
