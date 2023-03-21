using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia.Input.Platform;

using FSMViewAvalonia2.CodeGen;
using FSMViewAvalonia2.CodeGen.FsmProxy;

namespace FSMViewAvalonia2;
public partial class MainWindow
{
    private void InitFSMProxy()
    {
        generateFSMProxy.Click += GenerateFSMProxy_Click;
        copyFSMProxyCode.Click += CopyFSMProxyCode_Click;
    }

    private void CopyFSMProxyCode_Click(object sender, RoutedEventArgs e)
    {
        _ = Application.Current.Clipboard.SetTextAsync(fsmProxyCode.Text);
    }

    private void GenerateFSMProxy_Click(object sender, RoutedEventArgs e)
    {
        var ctx = new CodeGenContext()
        {
            className = !string.IsNullOrWhiteSpace(fsmProxyClassName.Text) ? fsmProxyClassName.Text : ("FSMProxy_" + currentFSMData?.goName + "_" + currentFSMData?.fsmName),
            nameSpace = "FSMProxy"
        };
        if(fsmProxyLibrary.SelectedIndex >= FSMProxyGenBase.SupportedGenerator.Count)
        {
            ctx.AppendLine("// Not Support Generator");
        }
        else
        {
            FSMProxyGenBase generator = FSMProxyGenBase.SupportedGenerator[fsmProxyLibrary.SelectedIndex];
            generator.Fsm = currentFSMData;
            generator.Generate(ctx);
        }

        fsmProxyCode.Text = ctx.builder.ToString();
    }
}
