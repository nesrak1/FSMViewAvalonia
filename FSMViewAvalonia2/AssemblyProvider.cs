using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2;
public class AssemblyProvider
{
    public required MonoCecilTempGenerator mono;
    public required List<AssemblyDefinition> assemblies;

    public TypeDefinition GetType(string name)
    {
        return assemblies.FindType(name);
    }
}
