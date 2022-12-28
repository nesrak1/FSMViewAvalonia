

namespace FSMViewAvalonia2
{
    public class AssetInfo
    {
        public long id;
        public uint size;
        public string name;
        public string nameBase;
        public string path;
        public string assetFile;
        public long fsmId;
        public long goId;
        public DataProviderType providerType = DataProviderType.Assets;
        public string Name
        {
            get
            {
                return path + name;// + $" [G:{(goId > 0 ? goId.ToString() : "0")}][F:{fsmId}]";
            }
        }
        public enum DataProviderType
        {
            Assets, Json
        }
    }

    public struct SceneInfo
    {
        public long id;
        public string name;
        public bool level;

        public string Name
        {
            get
            {
                return name;
            }
        }
    }
}
