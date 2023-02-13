namespace FSMViewAvalonia2.Data;
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
        if (pptr.PathId == 0)
        {
            return new NamedAssetPPtr(pptr.FileId, pptr.PathId, string.Empty, string.Empty);
        }

        AssetExternal extObj = am.GetExtAsset(inst, pptr.FileId, pptr.PathId);
        StringBuilder nameBuilder = new();
        _ = nameBuilder.Append(GetAssetNameFastModded(extObj.info, extObj.baseField, out bool isGameObject));
        if (isGameObject && extObj.baseField != null)
        {
            AssetTypeValueField c_Transform = am.GetExtAsset(extObj.file, extObj.baseField.Get("m_Component").Get(0).Get(0).Get(0)).baseField;
            while (true)
            {
                AssetTypeValueField father = c_Transform.Children.FirstOrDefault(x => x.FieldName == "m_Father");
                if (father == null)
                {
                    break;
                }

                c_Transform = am.GetExtAsset(extObj.file, father).baseField;
                if (c_Transform == null)
                {
                    break;
                }

                AssetTypeValueField m_GameObject = am.GetExtAsset(extObj.file, c_Transform.Get("m_GameObject")).baseField;
                string name = m_GameObject.Get("m_Name").AsString;
                _ = nameBuilder.Insert(0, name + "/");
            }
        }

        string file = extObj.file.name;
        return new NamedAssetPPtr(pptr.FileId, pptr.PathId, nameBuilder.ToString(), file);
    }

    private static string GetAssetNameFastModded(AssetFileInfo info, AssetTypeValueField fields,
        out bool isGameObject)
    {
        isGameObject = false;
        AssetTypeValueField name = fields["m_Name"];
        if (name == null)
        {
            return $"pathId_{info.PathId}";
        }
        return name.AsString;
    }
    private INamedAssetProvider ReadNamedAssetPPtr(AssetTypeValueField field)
    {
        int fileId = field.Get("m_FileID").AsInt;
        long pathId = field.Get("m_PathID").AsLong;
        return GetNamedPtr(new AssetPPtr(fileId, pathId));
    }
    private T GetValue<T>(AssetTypeValueField field)
    {
        if (field.IsDummy)
        {
            return default;
        }

        AssetTypeValue val = field.Value;
        if (typeof(T) == typeof(IDataProvider[]))
        {
            var providers = new IDataProvider[field.Children.Count];
            for (int i = 0; i < providers.Length; i++)
            {
                providers[i] = field.Children[i].IsDummy ? null : new AssetsDataProvider(field.Children[i], _resovler);
            }

            return (T) (object) providers;
        }

        if (typeof(T) == typeof(IDataProvider))
        {
            return (T) (object) new AssetsDataProvider(field, _resovler);
        }

        if (typeof(T) == typeof(INamedAssetProvider))
        {
            return (T) ReadNamedAssetPPtr(field);
        }

        object r = val.ValueType switch
        {
            AssetValueType.Bool => val.AsBool,
            AssetValueType.ByteArray => val.AsByteArray,
            AssetValueType.Double => val.AsDouble,
            AssetValueType.Float => val.AsFloat,
            AssetValueType.Int16 => val.AsShort,
            AssetValueType.Int32 => val.AsInt,
            AssetValueType.Int64 => val.AsLong,
            AssetValueType.Int8 => val.AsByte,
            AssetValueType.None => null,
            AssetValueType.String => val.AsString,
            AssetValueType.UInt16 => val.AsUShort,
            AssetValueType.UInt32 => val.AsUInt,
            AssetValueType.UInt64 => val.AsULong,
            AssetValueType.UInt8 => val.AsByte,
            _ => null
        };
        return val.ValueType != AssetValueType.String && r is IConvertible ? (T) Convert.ChangeType(r, typeof(T)) : (T) r;
    }

    T IDataProvider.As<T>() => GetValue<T>(_provider);

    T IDataProvider.GetValue<T>(string key)
    {
        AssetTypeValueField field = _provider.Get(key);
        return GetValue<T>(field);
    }
}
