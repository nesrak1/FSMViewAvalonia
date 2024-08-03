using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2;
internal static class Utils2
{
    public static string GetGameObjectPath(AssetsManager am, AssetTypeValueField goAti, AssetsFileInstance curFile)
    {
        try
        {
            StringBuilder pathBuilder = new();

            AssetTypeValueField c_Transform = am.GetExtAsset(curFile, goAti.Get("m_Component").Get(0).Get(0).Get(0)).baseField;
            while (true)
            {
                AssetTypeValueField father = c_Transform.Children.FirstOrDefault(x => x.FieldName == "m_Father");
                if (father == null)
                {
                    break;
                }

                c_Transform = am.GetExtAsset(curFile, father).baseField;
                if (c_Transform == null)
                {
                    break;
                }

                AssetTypeValueField m_GameObject = am.GetExtAsset(curFile, c_Transform.Get("m_GameObject")).baseField;
                string name = m_GameObject.Get("m_Name").AsString;
                _ = pathBuilder.Insert(0, name + "/");
            }

            return pathBuilder.ToString();
        }
        catch (Exception)
        {
            return "";
        }
    }
}
