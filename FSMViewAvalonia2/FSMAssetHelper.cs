using AssetsTools.NET.Extra;
using MessageBox.Avalonia;
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
            if (!File.Exists("classdata.tpk"))
            {
                return null;
            }
            am.LoadClassPackage("classdata.tpk");
            return am;
        }
    }
}
