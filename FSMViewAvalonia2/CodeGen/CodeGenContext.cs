using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2.CodeGen;
internal class CodeGenContext
{
    public string className = "";
    public string nameSpace = "";
    public int BlockSpaceCount = 4;
    public StringBuilder builder = new();
    public int SpaceCount { get; set; }
    public void AppendEmptyLine()
    {
        _ = builder.AppendLine();
    }
    public void AppendSpace()
    {
        if (SpaceCount <= 0)
            return;
        _ = builder.Append(' ', SpaceCount);
    }
    public void AppendLine(string text)
    {
        AppendSpace();
        _ = builder.AppendLine(text);
    }
    public void EnterBlock()
    {
        SpaceCount += BlockSpaceCount;
    }
    public void ExitBlock()
    {
        SpaceCount -= BlockSpaceCount;
    }
    public void EnterCodeBlock()
    {
        AppendLine("{");
        EnterBlock();
    }
    public void ExitCodeBlock()
    {
        ExitBlock();
        AppendLine("}");
    }
    public void EnterCodeBlock(Action<CodeGenContext> ctx)
    {
        EnterCodeBlock();
        ctx(this);
        ExitCodeBlock();
    }
    public string GetFriendlyName(string orig, string defaultVal = "")
    {
        if(string.IsNullOrWhiteSpace(orig)) orig = defaultVal;
        return orig.Replace(" ", "_")
                    .Replace('-', '_')
                    .Replace("?", "")
                    .Replace("!", "")
                    .Trim();
    }
    public string GetStringExp(string content)
    {
        content = content.Replace("\n", "\\n");
        return $"\"{content}\"";
    }
}
