using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
