using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssetsTools.NET;

namespace FSMViewAvalonia2.Assets;
internal static class AssetsReaderHelper
{
    public static AssetTypeTemplateField GetField(this AssetTypeTemplateField field, string name)
    {
        return field.Children.First(x => x.Name == name);
    }
    public static AssetTypeTemplateField RemoveFieldsAfter(this AssetTypeTemplateField field,
        string name, bool includeLatest = false)
    {
        AssetTypeTemplateField l = includeLatest ? field.GetField(name) : null;
        field.Children = field.Children.TakeWhile(x => x.Name != name).ToList();
        if(l != null)
        {
            field.Children.Add(l);
        }

        return field;
    }
    public static AssetTypeTemplateField GetTypeTemplateFieldFromAsset(this AssetsFile file,
        AssetFileInfo info,
        string assemblyName, string nameSpace, string typeName,
        List<AssetTypeTemplateField> parent = null)
    {
        AssetTypeTemplateField result = new()
        {
            Children = (parent?.ToList()) ?? new()
        };
        if(file.Metadata.TypeTreeEnabled)
        {
            result.FromTypeTree(file.Metadata.FindTypeTreeTypeByID(info.GetTypeId(file), file.GetScriptIndex(info)));
        }
        else
        {
            result = FSMAssetHelper.mono.GetTemplateField(result, assemblyName, nameSpace,
                                                               typeName, new(file.Metadata.UnityVersion));
        }

        return result;
    }
}
