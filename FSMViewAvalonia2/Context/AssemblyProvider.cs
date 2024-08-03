namespace FSMViewAvalonia2.Context;
public class AssemblyProvider : IAssemblyProvider
{
    public required MonoCecilTempGenerator mono;
    public required List<AssemblyDefinition> assemblies;

    public TypeDefinition GetType(string name)
    {
        return assemblies.FindType(name);
    }
}
