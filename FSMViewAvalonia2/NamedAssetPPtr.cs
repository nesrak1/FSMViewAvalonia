

namespace FSMViewAvalonia2
{
    public class NamedAssetPPtr : AssetPPtr, INamedAssetProvider
    {
        public string file;
        public string name;

        public NamedAssetPPtr(int fileID, long pathID) : base(fileID, pathID)
        {
            this.file = string.Empty;
            this.name = string.Empty;
        }
        public NamedAssetPPtr(int fileID, long pathID, string name) : base(fileID, pathID)
        {
            this.file = string.Empty;
            this.name = name;
        }
        public NamedAssetPPtr(int fileID, long pathID, string name, string file) : base(fileID, pathID)
        {
            this.file = file;
            this.name = name;
        }

        string INamedAssetProvider.name => name;

        bool INamedAssetProvider.isNull => pathID == 0;

        string INamedAssetProvider.file => file;

        public override string ToString()
        {
            if (file == string.Empty)
                return name;
            else
                return $"{name} ({file})";
        }
    }
    public class GameObjectPPtrHolder
    {
        public INamedAssetProvider pptr;
        public override string ToString() => pptr.ToString();
    }
}
