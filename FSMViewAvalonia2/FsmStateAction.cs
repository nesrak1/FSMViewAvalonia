using System.Diagnostics;
using System.Runtime.Versioning;


namespace FSMViewAvalonia2;
public class FsmStateActionUI
{
    public int Index { get; set; }
    public FsmStateAction action;
    public FsmStateActionUI(FsmStateAction action)
    {
        this.action = action;
    }
    public virtual async void BuildView(StackPanel stack, int index)
    {
        Index = index;
        string actionName = action.Name;

        stack.Children.Add(CreateSidebarHeader(actionName, index, action.Enabled));

        foreach (var field in action.Values)
        {

            _ = await App.mainWindow.CreateSidebarRow(action.AssemblyProvider, field, stack);
        }
    }

    private Grid CreateSidebarHeader(string text, int index, bool enabled)
    {
        Grid valueContainer = new()
        {
            Height = 28,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
        };
        var header = new TextBlock()
        {
            Text = "(" + index + ") " + text,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Padding = new Thickness(5),
            Height = 28,
            FontWeight = FontWeight.Bold
        };
        if (!enabled)
        {
            header.Background = Brushes.Red;
            header.Text += " (disabled)";
        }

        valueContainer.Children.Add(header);
        #region Open in Dnspy
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && action.Type != null)
        {
            Button btn = new()
            {
                Padding = new Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Width = 100,
                Content = "Open in ..."
            };
            btn.Click += Btn_Click;
            string filename = System.IO.Path.GetFileName(Config.config.SpyPath);
            if (filename.Equals("dnspy.exe", StringComparison.OrdinalIgnoreCase))
            {
                btn.Content = "Open in DnSpy";
            }
            else if (filename.Equals("ilspy.exe", StringComparison.OrdinalIgnoreCase))
            {
                btn.Content = "Open in ILSpy";
            }

            valueContainer.Children.Add(btn);
        }
        #endregion
        return valueContainer;
    }

    [SupportedOSPlatform("windows")]
    private async void Btn_Click(object sender, RoutedEventArgs e)
    {
        if (action.Type == null)
        {
            return;
        }

    SELECT:
        if (string.IsNullOrEmpty(Config.config.SpyPath) || !File.Exists(Config.config.SpyPath))
        {
            OpenFileDialog ofd = new()
            {
                AllowMultiple = false
            };
            ofd.Filters.Add(new()
            {
                Name = @"DnSpy\ILSpy",
                Extensions =
                [
                        "exe"
                    ]
            });
            string[] dnspy = await ofd.ShowAsync(App.mainWindow);
            if (dnspy == null || dnspy.Length == 0)
            {
                return;
            }

            Config.config.SpyPath = dnspy[0];
        }

        string filename = System.IO.Path.GetFileName(Config.config.SpyPath);
        if (!filename.Equals("dnspy.exe", StringComparison.OrdinalIgnoreCase) && !filename.Equals("ilspy.exe", StringComparison.OrdinalIgnoreCase))
        {
            _ = await MessageBoxUtil.ShowDialog(App.mainWindow, "DnSpy/ILSpy", "Invalid DnSpy or ILSpy path");
            Config.config.SpyPath = null;
            goto SELECT;
        }

        string arg = " \"" + action.Type.Module.FileName + "\" ";
        arg = filename.Equals("dnspy.exe", StringComparison.OrdinalIgnoreCase)
        ? arg + "--select T:" + action.FullName
        : arg + "/navigateTo:T:" + action.FullName;
        _ = System.Diagnostics.Process.Start(Config.config.SpyPath, arg);
    }
}
