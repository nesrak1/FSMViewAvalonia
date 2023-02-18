using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2.CodeGen.FsmProxy;
internal class TestCodeGen : FSMProxyGenBase
{
    public static class StateNames
    {
        public const string State1 = "State 1";
    }
    protected override void GenerateClassBody(CodeGenContext ctx)
    {
        base.GenerateClassBody(ctx);

        InsertComment(ctx, "Hello, World; Only One Line");
        InsertComment(ctx, "Hello, \n World!\nThis is two lines");

        ctx.AppendEmptyLine();

        InsertFieldDef(ctx, "TEST1", "int", true, "static", "3");
        InsertFieldDef(ctx, "TEST2", "int", false, "", "3");

        GenerateClass(ctx, ctx =>
        {
            InsertFieldDef(ctx, "Init", "string", true, "const", ctx.GetStringExp("Init"));
            InsertFieldDef(ctx, "State1", "string", true, "const", ctx.GetStringExp("State 1"));
        }, "StateNames");
    }
}
