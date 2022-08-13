using System;
using System.Collections.Generic;
using System.Text;

namespace FSMViewAvalonia2
{
    public struct AssetInfo
    {
        public long id;
        public uint size;
        public string name;
        public string path;
        public string assetFile;
        public string Name
        {
            get
            {
                return path + name;
            }
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
