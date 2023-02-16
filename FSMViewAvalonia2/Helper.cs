using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2;
internal static class Helper
{
    public static Array Convert(this Array src, Type type)
    {
        var result = Array.CreateInstance(type, src.Length);
        for(int i = 0; i < src.Length; i++)
        {
            result.SetValue(src.GetValue(i), i);
        }

        return result;
    }
}
