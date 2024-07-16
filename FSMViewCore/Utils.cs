using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2;
public static class Utils
{
    public static string GetFsmEnumString(TypeDefinition enumType, int val)
    {
        string fn = enumType.FullName;
        if (enumType.IsEnum)
        {
            bool isFlag = enumType.CustomAttributes.Any(x => x.AttributeType.FullName == "System.FlagAttribute");
            StringBuilder sb = isFlag ? new() : null;
            foreach (FieldDefinition v in enumType.Fields.Where(x => x.IsLiteral && x.Constant is int))
            {
                int fv = (int)v.Constant;
                if (isFlag)
                {
                    if ((fv & val) == val)
                    {
                        if (sb.Length != 0)
                        {
                            _ = sb.Append(',');
                        }

                        _ = sb.Append(v.Name);
                    }
                }
                else
                {
                    if (fv == val)
                    {
                        return $"{fn}::{v.Name}";
                    }
                }
            }

            if (sb?.Length != 0)
            {
                return $"{fn}::{sb}";
            }
        }

        return $"({fn}) {val}";
    }
    public static IEnumerable<AssemblyDefinition> SafeResolveReferences(this AssemblyDefinition assembly) => assembly.MainModule.AssemblyReferences.Select(x =>
                                                                                                                  {
                                                                                                                      try
                                                                                                                      {
                                                                                                                          return assembly.MainModule.AssemblyResolver.Resolve(x);
                                                                                                                      } catch (Exception e)
                                                                                                                      {
                                                                                                                          Console.Error.WriteLine(e);
                                                                                                                          return null;
                                                                                                                      }
                                                                                                                  }).Where(x => x != null);
    public static TypeDefinition FindType(this IEnumerable<AssemblyDefinition> asm, string name) => asm.Select(x => x.MainModule.GetType(name)).FirstOrDefault(x => x != null);
    public static Array Convert(this Array src, Type type)
    {
        var result = Array.CreateInstance(type, src.Length);
        for (int i = 0; i < src.Length; i++)
        {
            result.SetValue(src.GetValue(i), i);
        }

        return result;
    }
    public static bool IsSubclassOf(this TypeDefinition type, string parentType)
    {
        while(type!= null)
        {
            if(type.FullName == parentType)
            {
                return true;
            }

            type = type.BaseType?.Resolve();
        }

        return false;
    }
}
