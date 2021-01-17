using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace FSMViewAvalonia2
{
    public class AssetNameResolver
    {
        private AssetsManager am;
        private AssetsFileInstance inst;
        public AssetNameResolver(AssetsManager am, AssetsFileInstance inst)
        {
            this.am = am;
            this.inst = inst;
        }

        public string GetName(AssetPPtr pptr)
        {
            if (pptr.pathID == 0)
            {
                return string.Empty;
            }
            AssetExternal extObj = am.GetExtAsset(inst, pptr.fileID, pptr.pathID, true);
            string name = GetAssetNameFastModded(extObj.file.file, am.classFile, extObj.info);
            return name;
        }

        public NamedAssetPPtr GetNamedPtr(AssetPPtr pptr)
        {
            if (pptr.pathID == 0)
            {
                return new NamedAssetPPtr(pptr.fileID, pptr.pathID, string.Empty, string.Empty);
            }
            AssetExternal extObj = am.GetExtAsset(inst, pptr.fileID, pptr.pathID, true);
            string name = GetAssetNameFastModded(extObj.file.file, am.classFile, extObj.info);
            string file = extObj.file.name;
            return new NamedAssetPPtr(pptr.fileID, pptr.pathID, name, file);
        }

        private string GetAssetNameFastModded(AssetsFile file, ClassDatabaseFile cldb, AssetFileInfoEx info)
        {
            ClassDatabaseType type = AssetHelper.FindAssetClassByID(cldb, info.curFileType);

            AssetsFileReader reader = file.reader;

            if (type.fields.Count == 0) return type.name.GetString(cldb);
            if (type.fields.Count > 1 && type.fields[1].fieldName.GetString(cldb) == "m_Name")
            {
                reader.Position = info.absoluteFilePos;
                return reader.ReadCountStringInt32();
            }
            else if (type.name.GetString(cldb) == "GameObject")
            {
                reader.Position = info.absoluteFilePos;
                int size = reader.ReadInt32();
                int componentSize = file.header.format > 0x10 ? 0xC : 0x10;
                reader.Position += size * componentSize;
                reader.Position += 4;
                return reader.ReadCountStringInt32();
            }
            else if (type.name.GetString(cldb) == "MonoBehaviour")
            {
                reader.Position = info.absoluteFilePos;
                reader.Position += 28;
                string name = reader.ReadCountStringInt32();
                if (name != "")
                {
                    return name;
                }
            }
            return $"pathId_{info.index}";
        }
    }
}
