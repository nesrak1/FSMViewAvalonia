namespace FSMViewAvalonia2
{
    internal class AssemblyResolver : BaseAssemblyResolver
    {
        public AssemblyResolver(string dir)
        {
            AddSearchDirectory(dir);
        }
    }
}
