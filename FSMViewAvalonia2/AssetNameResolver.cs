using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Linq;
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
            string name = GetAssetNameFastModded(extObj.file.file, am.classFile, extObj.info, out _);
            return name;
        }

        public NamedAssetPPtr GetNamedPtr(AssetPPtr pptr)
        {
            if (pptr.pathID == 0)
            {
                return new NamedAssetPPtr(pptr.fileID, pptr.pathID, string.Empty, string.Empty);
            }
            AssetExternal extObj = am.GetExtAsset(inst, pptr.fileID, pptr.pathID, true);
            StringBuilder nameBuilder = new();
            nameBuilder.Append(GetAssetNameFastModded(extObj.file.file, am.classFile, extObj.info, out var isGameObject));
            if (isGameObject && extObj.instance != null)
            {
                AssetTypeInstance c_Transform = am.GetExtAsset(extObj.file, extObj.instance.GetBaseField().Get("m_Component").Get(0).Get(0).Get(0)).instance;
                while (true)
                {
                    var father = c_Transform.GetBaseField().children.FirstOrDefault(x => x.GetName() == "m_Father");
                    if (father == null) break;
                    c_Transform = am.GetExtAsset(extObj.file, father).instance;
                    if (c_Transform == null) break;
                    var m_GameObject = am.GetExtAsset(extObj.file, c_Transform.GetBaseField().Get("m_GameObject")).instance;
                    string name = m_GameObject.GetBaseField().Get("m_Name").GetValue().AsString();
                    nameBuilder.Insert(0, name + "/");
                }
            }

            string file = extObj.file.name;
            return new NamedAssetPPtr(pptr.fileID, pptr.pathID, nameBuilder.ToString(), file);
        }

        private string GetAssetNameFastModded(AssetsFile file, ClassDatabaseFile cldb, AssetFileInfoEx info, out bool isGameObject)
        {
            ClassDatabaseType type = AssetHelper.FindAssetClassByID(cldb, info.curFileType);
            AssetsFileReader reader = file.reader;
            isGameObject = false;
            if (type.fields.Count == 0) return type.name.GetString(cldb);
            if (type.fields.Count > 1 && type.fields[1].fieldName.GetString(cldb) == "m_Name")
            {
                reader.Position = info.absoluteFilePos;
                return reader.ReadCountStringInt32();
            }
            else if (type.name.GetString(cldb) == "GameObject")
            {
                isGameObject = true;
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
