using System;
using System.Collections;
using System.Runtime.CompilerServices;

using AssetsTools.NET;

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
        _ = nameBuilder.Append(GetAssetNameFastModded(extObj.info, extObj.baseField, extObj.file,
            out bool isGameObject));
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

    private string GetAssetNameFastModded(AssetFileInfo info, AssetTypeValueField fields,
        AssetsFileInstance assetsFileInstance,
        out bool isGameObject)
    {
        var type = (AssetClassID) info.TypeId;
        isGameObject = type == AssetClassID.GameObject;
        AssetTypeValueField name = fields["m_Name"];
        string result;
        if (name.IsDummy)
        {
            var gof = fields["m_GameObject"];
            if (!gof.IsDummy && gof.TypeName == "PPtr<GameObject>")
            {
                AssetExternal go = am.GetExtAsset(assetsFileInstance, gof);
                result = GetAssetNameFastModded(go.info, go.baseField, go.file, out _);
            }
            else
            {
                result = $"pathId_{info.PathId}";
            }

        }
        else
        {
            result = name.AsString;
        }
        if (type != AssetClassID.GameObject)
        {
            string typeName;
            if (type == AssetClassID.MonoBehaviour)
            {
                AssetExternal script = am.GetExtAsset(assetsFileInstance, fields["m_Script"]);
                typeName = "Script " + script.baseField["m_Name"].AsString;
            }
            else
            {
                typeName = type.ToString();
            }
            result += $" ({typeName})";
        }
        

        return result;
    }
    private NamedAssetPPtr ReadNamedAssetPPtr(AssetTypeValueField field)
    {
        int fileId = field.Get("m_FileID").AsInt;
        long pathId = field.Get("m_PathID").AsLong;
        return GetNamedPtr(new AssetPPtr(fileId, pathId));
    }

    private T GetValue<T>(AssetTypeValueField field) => (T) (GetValue(typeof(T), field) ?? default(T));
    private object GetValue(Type resultType, AssetTypeValueField field)
    {
        if (field.IsDummy)
        {
            return default;
        }

        AssetTypeValue val = field.Value;
        if (resultType.IsArray)
        {
            if (field.Children.Count == 1 &&
                field.Children[0].FieldName == "Array" &&
                field.Value == null)
            {
                field = field.Children[0];
                val = field.Value;
            }


            if (resultType == typeof(byte[]))
            {
                return field.AsByteArray;
            }

            Type elType = resultType.GetElementType();



            if (resultType == typeof(IDataProvider[]))
            {
                var providers = new IDataProvider[field.Children.Count];
                for (int i = 0; i < providers.Length; i++)
                {
                    providers[i] = field.Children[i].IsDummy ? null : new AssetsDataProvider(field.Children[i], _resovler);
                }

                return providers;
            }

            if (val.ValueType == AssetValueType.ByteArray
                && elType.IsAssignableTo(typeof(IConvertible))
                && elType.IsPrimitive
                && elType.IsValueType)
            {
                using var ms = new MemoryStream(val.AsByteArray);
                var result = new List<object>();
                var reader = new BinaryReader(ms);

                while (ms.Position < ms.Length)
                {
                    if (elType == typeof(bool))
                    {
                        result.Add(reader.ReadByte());
                    }
                    else if (elType == typeof(short))
                    {
                        result.Add(reader.ReadInt16());
                    }
                    else if (elType == typeof(int))
                    {
                        result.Add(reader.ReadInt32());
                    }
                    else if (elType == typeof(long))
                    {
                        result.Add(reader.ReadInt64());
                    }
                    else if (elType == typeof(float))
                    {
                        result.Add(reader.ReadSingle());
                    }
                    else if (elType == typeof(double))
                    {
                        result.Add(reader.ReadDouble());
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }

                return result.Select(x => ((IConvertible) x).ToType(elType, null))
                    .ToArray()
                    .Convert(elType);
            }

            return GetValue<IDataProvider[]>(field)
                .Cast<AssetsDataProvider>()
                .Select(x => x.GetValue(elType, x._provider))
                .ToArray()
                .Convert(elType);
        }

        if (resultType == typeof(IDataProvider))
        {
            return new AssetsDataProvider(field, _resovler);
        }

        if (resultType == typeof(INamedAssetProvider))
        {
            return ReadNamedAssetPPtr(field);
        }

        object r = val.ValueType switch
        {
            AssetValueType.Bool => val.AsBool,
            AssetValueType.Array => val.AsObject,
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
        return val.ValueType != AssetValueType.String && r is IConvertible ? Convert.ChangeType(r, resultType) : r;
    }

    T IDataProvider.As<T>() => GetValue<T>(_provider);

    T IDataProvider.GetValue<T>(string key)
    {
        AssetTypeValueField field = _provider.Get(key);
        return GetValue<T>(field);
    }
}
