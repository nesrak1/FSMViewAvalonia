using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FSMViewAvalonia2
{
    public static class FSMAssetHelper
    {
        public static AssetsManager CreateAssetManager()
        {
            AssetsManager am = new AssetsManager();
            am.updateAfterLoad = false;
            am.useTemplateFieldCache = true;
            am.LoadClassDatabase(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cldb.dat"));
            return am;
        }
    }
}
