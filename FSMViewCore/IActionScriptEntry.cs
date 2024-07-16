using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2
{
    public interface IActionScriptEntry
    {
        public record class PropertyInfo(string Name, object RawValue, object Value, 
            UIHint? UIHint, string PropGroup = null);
        string Name { get; set; }
        List<PropertyInfo> Values { get; set; }
        bool Enabled { get; set; }
    }
}
