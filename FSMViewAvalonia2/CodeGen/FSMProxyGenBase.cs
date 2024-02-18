using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FSMViewAvalonia2.CodeGen.FsmProxy;

namespace FSMViewAvalonia2.CodeGen;
internal abstract class FSMProxyGenBase
{
    protected virtual IEnumerable<string> Usings { get; } = new List<string>()
    {
        "System",
        "System.Collections",
        "System.Collections.Generic",
        "System.Linq",
        "HutongGames.PlayMaker"
    };

    public FsmDataInstance Fsm { get; set; }
    protected virtual void GenerateUsings(CodeGenContext ctx)
    {
        foreach (string v in Usings)
        {
            ctx.AppendLine($"using {v};");
        }

        ctx.AppendEmptyLine();
    }
    protected virtual void GenerateNameSpace(CodeGenContext ctx)
    {
        if (!string.IsNullOrWhiteSpace(ctx.nameSpace))
        {
            ctx.AppendLine($"namespace {ctx.nameSpace};");
            ctx.AppendEmptyLine();
        }
    }
    protected virtual void GenerateClass(CodeGenContext ctx, Action<CodeGenContext> body, string className, string modify = "public")
    {
        ctx.AppendLine($"{modify} class {className}");
        ctx.EnterCodeBlock(body);
    }
    protected virtual void InsertFieldDef(CodeGenContext ctx, string name,
        string type, bool isPublic = false, string modify = "", string initExp = "")
    {
        ctx.AppendLine($"{
            (isPublic ? "public" : "private")
            } {modify} {type} {
            ctx.GetFriendlyName(name)
            }{
            (string.IsNullOrEmpty(initExp) ? "" : " = " + initExp)
            };");
    }
    protected virtual void InsertComment(CodeGenContext ctx, string text)
    {
        string[] lines = text.Trim().Split('\n');
        if(lines.Length == 1)
        {
            ctx.AppendLine(@"// " + text);
        }
        else
        {
            ctx.AppendLine("/**");
            ctx.EnterBlock();
            foreach(string v in lines)
            {
                ctx.AppendLine(v);
            }

            ctx.AppendLine("**/");
            ctx.ExitBlock();
        }
    }
    protected virtual void InsertMethod(CodeGenContext ctx, Action<CodeGenContext> generator,
        string name, string ret, string modify, params string[] args)
    {
        ctx.AppendLine($@"{modify} {ret} {ctx.GetFriendlyName(name)}({
            string.Join(',', args)
            })");
        ctx.EnterCodeBlock(generator);
    }
    protected virtual void InsertGetter(CodeGenContext ctx, Action<CodeGenContext> generator,
        string name, string ret, string modify = "public")
    {
        ctx.AppendLine($"{modify} {ret} {ctx.GetFriendlyName(name)}");
        ctx.EnterCodeBlock(ctx =>
        {
            ctx.AppendLine("get");
            ctx.EnterCodeBlock(generator);
        });
    }
    protected virtual void GenerateClassBody(CodeGenContext ctx)
    {
        GenerateStateNames(ctx);
        GenerateVariableNames(ctx);
        GenerateEventNames(ctx);
        GenerateVariableHolder(ctx);
        ctx.AppendLine("public Fsm FSM { get; }");
        ctx.AppendLine("public FsmVariablesHolder Variables { get; }");
        GenerateCtor(ctx);
    }
    protected virtual void GenerateStateNames(CodeGenContext ctx)
    {
        GenerateClass(ctx, ctx =>
        {
            foreach(FsmStateData v in Fsm.states)
            {
                InsertFieldDef(ctx, v.Name, "string", true, "const", ctx.GetStringExp(v.Name));
            }
        }, "StateNames", "public static");
    }
    protected virtual void GenerateVariableNames(CodeGenContext ctx)
    {
        GenerateClass(ctx, ctx =>
        {
            foreach (Tuple<string, object> v in Fsm.variables.SelectMany(x => x.Values))
            {
                InsertFieldDef(ctx, v.Item1, "string", true, "const", ctx.GetStringExp(v.Item1));
            }
        }, "VariableNames", "public static");
    }
    protected virtual void GenerateEventNames(CodeGenContext ctx)
    {
        GenerateClass(ctx, ctx =>
        {
            foreach (FsmEventData v in Fsm.events)
            {
                InsertFieldDef(ctx, v.Name, "string", true, "const", ctx.GetStringExp(v.Name));
            }
        }, "EventNames", "public static");
    }
    protected virtual void GenerateVariableHolder(CodeGenContext ctx)
    {
        GenerateClass(ctx, ctx =>
        {
            InsertFieldDef(ctx, "fsmVariables", "FsmVariables", true);
            foreach(FsmVariableData vars in Fsm.variables)
            {
                string type = "Fsm" + vars.VariableType.ToString();
                foreach(Tuple<string, object> v in vars.Values)
                {
                    InsertGetter(ctx, ctx =>
                    {
                        ctx.AppendLine($"return fsmVariables.Find{type}(VariableNames.{ctx.GetFriendlyName(v.Item1)});");
                    }, v.Item1, type, "public");
                }
            }
        }, "FsmVariablesHolder");
    }
    protected virtual void GenerateCtor(CodeGenContext ctx)
    {
        InsertMethod(ctx, ctx =>
        {
            ctx.AppendLine("this.FSM = fsm;");
            ctx.AppendLine("this.Variables = new FsmVariablesHolder();");
            ctx.AppendLine("this.Variables.fsmVariables = fsm.Variables;");
        }, ctx.className, "", "public", "Fsm fsm");
        ctx.AppendLine($"public {ctx.GetFriendlyName(ctx.className)}(PlayMakerFSM fsm) : this(fsm.Fsm) {{}}");
    }
    public virtual void Generate(CodeGenContext ctx)
    {
        InsertComment(ctx, $@"
This file was generated by FSMViewerAvalonia (https://github.com/HKLab/FSMViewAvalonia).
Generator Type: {GetType().FullName}
");
        GenerateUsings(ctx);
        GenerateNameSpace(ctx);
        GenerateClass(ctx, GenerateClassBody, ctx.GetFriendlyName(ctx.className, "ClassName"), "internal");
    }

    public static readonly List<FSMProxyGenBase> SupportedGenerator =
    [
        new DefaultFsmProxy()
    ];
}
