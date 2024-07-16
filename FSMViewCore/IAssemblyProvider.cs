using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2
{
    public interface IAssemblyProvider
    {
        public TypeDefinition GetType(string name);
    }

    public class DefaultAssemblyProvider : IAssemblyProvider
    {
        public static readonly DefaultAssemblyProvider instance = new();
        public TypeDefinition GetType(string name)
        {
            return null;
        }
    }
}
