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
