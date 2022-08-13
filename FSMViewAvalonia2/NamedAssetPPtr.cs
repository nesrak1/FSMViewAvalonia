using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Text;

namespace FSMViewAvalonia2
{
    public class NamedAssetPPtr : AssetPPtr
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
        public NamedAssetPPtr pptr;
        public override string ToString() => pptr.ToString();
    }
}
