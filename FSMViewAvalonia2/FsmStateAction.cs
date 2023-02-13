using System.Diagnostics;

using FSMViewAvalonia2.UEP;

namespace FSMViewAvalonia2;
public class FsmStateAction : IActionScriptEntry
{
    public FsmStateAction(ActionData actionData, int index, int dataVersion, FsmState state,
        FsmDataInstance dataInstance)
    {
        string actionName = actionData.actionNames[index];
        FullName = actionName;
        Type = FSMLoader.mainAssembly?.MainModule?.GetType(FullName);
        if (actionName.Contains('.'))
        {
            actionName = actionName[(actionName.LastIndexOf(".") + 1)..];
        }

        int startIndex = actionData.actionStartIndex[index];
        int endIndex = index == actionData.actionNames.Count - 1 ? actionData.paramDataType.Count : actionData.actionStartIndex[index + 1];

        HashSet<string> parmaNames = new HashSet<string>();
        for (int j = startIndex; j < endIndex; j++)
        {
            string paramName = actionData.paramName[j];
            if(string.IsNullOrEmpty(paramName))
            {
                break;
            }

            object obj = ActionReader.GetFsmObject(actionData, ref j, dataVersion);
            FieldDefinition field = Type?.Fields?.FirstOrDefault(x => x.Name == paramName);

            if (obj is NamedVariable nv)
            {
                if (!string.IsNullOrEmpty(nv.name))
                {
                    if (!dataInstance.variableNames.Contains(nv.name))
                    {
                        nv.isGlobal = true;
                    }
                }
            }

            if (field != null)
            {
                TypeDefinition ftype = field.FieldType.Resolve();
                CustomAttribute UIHintAttr = field.CustomAttributes
                        .FirstOrDefault(x => x.AttributeType.FullName == "HutongGames.PlayMaker.UIHintAttribute");
                var uitype = (UIHint?) (int?) UIHintAttr?.ConstructorArguments[0].Value;
                if (ftype.IsEnum && obj is int val)
                {
                    obj = MainWindow.GetFsmEnumString(ftype, val);
                } else if (uitype == UIHint.Layer && obj is FsmInt or int)
                {
                    var fi = obj as FsmInt;
                    int layer = fi?.value ?? (int) obj;
                    AssetTypeValueField tagManager = GlobalGameManagers.instance.GetAsset(AssetClassID.TagManager);
                    string curLayer = tagManager.Get("layers").Get(0).Get(layer).AsString;

                    obj = $"{obj} ({curLayer})";
                }

                if (uitype != null)
                {
                    paramName = $"{paramName} [{uitype}]";
                }
            }

            Values.Add(new Tuple<string, object>(paramName, obj));
        }

        State = state;
        Name = actionName;
        Enabled = actionData.actionEnabled[index];
    }
    public string FullName { get; set; }
    public string Name { get; set; }
    public List<Tuple<string, object>> Values { get; set; } = new();
    public bool Enabled { get; set; } = true;
    public int Index { get; set; }
    public FsmState State { get; init; }
    public TypeDefinition Type { get; set; }

    public virtual async void BuildView(StackPanel stack, int index)
    {
        Index = index;
        string actionName = Name;
        List<Tuple<string, object>> fields = Values;

        stack.Children.Add(CreateSidebarHeader(actionName, index, Enabled));

        foreach (Tuple<string, object> field in fields)
        {
            string key = field.Item1;
            object value = field.Item2;

            _ = await App.mainWindow.CreateSidebarRow(key, value, stack);
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
        #region Inspect
        if (UEPConnect.UEPConnected && State.fsm.info.ProviderType == AssetInfo.DataProviderType.Json)
        {
            Button inspect_btn = new()
            {
                Padding = new Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Content = "Inspect",
                Margin = new Thickness(0, 0, 100, 0)
            };
            inspect_btn.Click += Inspect_btn_Click;
            valueContainer.Children.Add(inspect_btn);
        }
        #endregion
        #region Open in Dnspy
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
        } else if (filename.Equals("ilspy.exe", StringComparison.OrdinalIgnoreCase))
        {
            btn.Content = "Open in ILSpy";
        }

        valueContainer.Children.Add(btn);
        #endregion
        return valueContainer;
    }

    private void Inspect_btn_Click(object sender, RoutedEventArgs e) => UEPConnect.Send("INSPECT-ACTION\n" + State.fsm.info.id + "\n" + State.name + "\n" + Index);

    private async void Btn_Click(object sender, RoutedEventArgs e)
    {
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
                Extensions = new()
                {
                        "exe"
                    }
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

        string arg = " \"" + GameFileHelper.FindGameFilePath(await GameFileHelper.FindHollowKnightPath(App.mainWindow),
            System.IO.Path.Combine("Managed", "Assembly-CSharp.dll")) + "\" ";
        arg = filename.Equals("dnspy.exe", StringComparison.OrdinalIgnoreCase)
        ? arg + "--select T:" + FullName
        : arg + "/navigateTo:T:" + FullName;
        _ = System.Diagnostics.Process.Start(Config.config.SpyPath, arg);
    }
}
