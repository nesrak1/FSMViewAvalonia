using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2
{
    public class FsmStateAction : IActionScriptEntry
    {
        public FsmStateAction(ActionData actionData, int index, int dataVersion, FsmState state)
        {
            string actionName = actionData.actionNames[index];
            FullName = actionName;
            if (actionName.Contains("."))
                actionName = actionName.Substring(actionName.LastIndexOf(".") + 1);

            int startIndex = actionData.actionStartIndex[index];
            int endIndex;
            if (index == actionData.actionNames.Count - 1)
                endIndex = actionData.paramDataType.Count;
            else
                endIndex = actionData.actionStartIndex[index + 1];

            for (int j = startIndex; j < endIndex; j++)
            {
                string paramName = actionData.paramName[j];
                object obj = ActionReader.GetFsmObject(actionData, j, dataVersion);

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
        public async virtual void BuildView(StackPanel stack, int index)
        {
            Index = index;
            string actionName = Name;
            var fields = Values;

            stack.Children.Add(CreateSidebarHeader(actionName, index, Enabled));

            foreach (var field in fields)
            {
                string key = field.Item1;
                object value = field.Item2;

                stack.Children.Add(await App.mainWindow.CreateSidebarRow(key, value));
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
            if (UEPConnect.UEPConnected && State.fsm.info.providerType == AssetInfo.DataProviderType.Json)
            {
                Button inspect_btn = new()
                {
                    Padding = new Thickness(5),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Content = "Inspect",
                    Margin = new Thickness(0,0,100,0)
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
                Width = 100
            };
            btn.Content = "Open in ...";
            btn.Click += Btn_Click;
            var filename = System.IO.Path.GetFileName(Config.config.SpyPath);
            if (filename.Equals("dnspy.exe", StringComparison.OrdinalIgnoreCase))
            {
                btn.Content = "Open in DnSpy";
            }
            else if (filename.Equals("ilspy.exe", StringComparison.OrdinalIgnoreCase))
            {
                btn.Content = "Open in ILSpy";
            }
            valueContainer.Children.Add(btn);
            #endregion
            return valueContainer;
        }

        private void Inspect_btn_Click(object sender, RoutedEventArgs e)
        {
            UEPConnect.Send("INSPECT-ACTION\n" + State.fsm.info.id + "\n" + State.name + "\n" + Index);
        }

        private async void Btn_Click(object sender, RoutedEventArgs e)
        {
            SELECT:
            if (string.IsNullOrEmpty(Config.config.SpyPath) || !File.Exists(Config.config.SpyPath))
            {
                OpenFileDialog ofd = new();
                ofd.AllowMultiple = false;
                ofd.Filters.Add(new()
                {
                    Name = @"DnSpy\ILSpy",
                    Extensions = new()
                    {
                        "exe"
                    }
                });
                string[] dnspy = await ofd.ShowAsync(App.mainWindow);
                if (dnspy == null || dnspy.Length == 0) return;
                Config.config.SpyPath = dnspy[0];
            }
            var filename = System.IO.Path.GetFileName(Config.config.SpyPath);
            if (!filename.Equals("dnspy.exe", StringComparison.OrdinalIgnoreCase) && !filename.Equals("ilspy.exe", StringComparison.OrdinalIgnoreCase))
            {
                await MessageBoxUtil.ShowDialog(App.mainWindow, "DnSpy/ILSpy", "Invalid DnSpy or ILSpy path");
                Config.config.SpyPath = null;
                goto SELECT;
            }

            string arg = " \"" + GameFileHelper.FindGameFilePath(await GameFileHelper.FindHollowKnightPath(App.mainWindow),
                System.IO.Path.Combine("Managed", "Assembly-CSharp.dll")) + "\" ";
            if (filename.Equals("dnspy.exe", StringComparison.OrdinalIgnoreCase))
            {
                arg = arg + "--select T:" + FullName;
            }
            else
            {
                arg = arg + "/navigateTo:T:" + FullName;
            }
            System.Diagnostics.Process.Start(Config.config.SpyPath, arg);
        }
    }
}
