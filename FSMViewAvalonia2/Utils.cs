using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2;
internal static class Utils
{
    public static IEnumerable<AssemblyDefinition> SafeResolveReferences(this AssemblyDefinition assembly)
    {
        return assembly.MainModule.AssemblyReferences.Select(x =>
        {
            try
            {
                return assembly.MainModule.AssemblyResolver.Resolve(x);
            } catch(Exception e)
            {
                Console.Error.WriteLine(e);
                return null;
            }
        }).Where(x => x != null);
    }
}
