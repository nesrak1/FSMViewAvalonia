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

        public string Name
        {
            get
            {
                return name;
            }
        }
    }
}
