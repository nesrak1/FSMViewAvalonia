namespace FSMViewAvalonia2.Data
{
    public class AssetsDataProvider : IDataProvider
    {
        private readonly AssetTypeValueField _provider;
        private readonly AssetNameResolver _resovler;
        private readonly AssetsManager am;
        private readonly AssetsFileInstance inst;
        public AssetsDataProvider(AssetTypeValueField field, AssetNameResolver resovler)
        {
            _provider = field;
            _resovler = resovler;
            am = resovler.am;
            inst = resovler.inst;
        }

        private NamedAssetPPtr GetNamedPtr(AssetPPtr pptr)
        {
            if (pptr.pathID == 0)
            {
                return new NamedAssetPPtr(pptr.fileID, pptr.pathID, string.Empty, string.Empty);
            }
            AssetExternal extObj = am.GetExtAsset(inst, pptr.fileID, pptr.pathID);
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

        private static string GetAssetNameFastModded(AssetsFile file, ClassDatabaseFile cldb, AssetFileInfoEx info, 
            out bool isGameObject)
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
        private INamedAssetProvider ReadNamedAssetPPtr(AssetTypeValueField field)
        {
            int fileId = field.Get("m_FileID").GetValue().AsInt() ;
            long pathId = field.Get("m_PathID").GetValue().AsInt64();
            return GetNamedPtr(new AssetPPtr(fileId, pathId));
        }
        private T GetValue<T>(AssetTypeValueField field)
        {
            if (field.IsDummy()) return default;
            var val = field.GetValue();
            if (typeof(T) == typeof(IDataProvider[]))
            {
                var providers = new IDataProvider[field.childrenCount];
                for (int i = 0; i < providers.Length; i++)
                {
                    providers[i] = field.children[i].IsDummy() ? null : new AssetsDataProvider(field.children[i], _resovler);
                }
                return (T)(object)providers;
            }
            if (typeof(T) == typeof(IDataProvider))
            {
                return (T)(object)new AssetsDataProvider(field, _resovler);
            }
            if(typeof(T) == typeof(INamedAssetProvider))
            {
                return (T)ReadNamedAssetPPtr(field);
            }
            var r = (object)(val.type switch
            {
                EnumValueTypes.Bool => val.value.asBool,
                EnumValueTypes.ByteArray => val.value.asByteArray,
                EnumValueTypes.Double => val.value.asDouble,
                EnumValueTypes.Float => val.value.asFloat,
                EnumValueTypes.Int16 => val.value.asInt16,
                EnumValueTypes.Int32 => val.value.asInt32,
                EnumValueTypes.Int64 => val.value.asInt64,
                EnumValueTypes.Int8 => val.value.asInt8,
                EnumValueTypes.None => null,
                EnumValueTypes.String => val.AsString(),
                EnumValueTypes.UInt16 => val.value.asUInt16,
                EnumValueTypes.UInt32 => val.value.asUInt32,
                EnumValueTypes.UInt64 => val.value.asUInt64,
                EnumValueTypes.UInt8 => val.value.asUInt8,
                _ => null
            });
            if(val.type != EnumValueTypes.String && r is IConvertible)
            {
                return (T)Convert.ChangeType(r, typeof(T));
            }
            return (T)r;
        }

        T IDataProvider.As<T>()
        {
            return GetValue<T>(_provider);
        }

        T IDataProvider.GetValue<T>(string key)
        {
            var field = _provider.Get(key);
            return GetValue<T>(field);
        }
    }
}
