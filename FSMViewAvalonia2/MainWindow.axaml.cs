

using Mono.Cecil.Rocks;
using System.Linq;

namespace FSMViewAvalonia2
{
    public class MainWindow : Window
    {
        //controls
        private readonly Canvas graphCanvas;
        private readonly MenuItem fileOpen, openJson;
        private readonly MenuItem openSceneList;
        private readonly MenuItem openResources;
        private readonly MenuItem openLast;
        private readonly MenuItem closeTab;
        private readonly MenuItem closeAllTab;
        private TextBlock tipText;
        private readonly StackPanel stateList;
        private readonly StackPanel eventList;
        private readonly StackPanel variableList;
        private readonly TabControl fsmTabs;
        private readonly MatrixTransform mt;

        //variables
        private AssetsManager am;
        private FSMLoader fsmLoader;
        private FsmDataInstance fsmData;
        private string lastFileName;
        private readonly List<FsmDataInstance> loadedFsmDatas;
        private bool addingTabs;

        //fsm info
        private readonly ObservableCollection<TabItem> tabItems;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            //generated items
            graphCanvas = this.FindControl<Canvas>("graphCanvas");
            fileOpen = this.FindControl<MenuItem>("fileOpen");
            openJson = this.FindControl<MenuItem>("openJson");
            openSceneList = this.FindControl<MenuItem>("openSceneList");
            openResources = this.FindControl<MenuItem>("openResources");
            openLast = this.FindControl<MenuItem>("openLast");
            closeTab = this.FindControl<MenuItem>("closeTab");
            closeAllTab = this.FindControl<MenuItem>("closeAllTab");
            tipText = this.FindControl<TextBlock>("tipText");
            stateList = this.FindControl<StackPanel>("stateList");
            eventList = this.FindControl<StackPanel>("eventList");
            variableList = this.FindControl<StackPanel>("variableList");
            fsmTabs = this.FindControl<TabControl>("fsmTabs");
            mt = graphCanvas.RenderTransform as MatrixTransform;
            //generated events
            PointerPressed += MouseDownCanvas;
            PointerReleased += MouseUpCanvas;
            PointerMoved += MouseMoveCanvas;
            PointerWheelChanged += MouseScrollCanvas;
            openJson.Click += OpenJson_Click;
            fileOpen.Click += FileOpen_Click;
            openLast.Click += OpenLast_Click;
            closeTab.Click += CloseTab_Click;
            closeAllTab.Click += CloseAllTab_Click;
            openResources.Click += OpenResources_Click;
            openSceneList.Click += OpenSceneList_Click;
            fsmTabs.SelectionChanged += FsmTabs_SelectionChanged;

            loadedFsmDatas = new List<FsmDataInstance>();
            tabItems = new ObservableCollection<TabItem>();
            fsmTabs.Items = tabItems;

            App.mainWindow = this;
        }

        

        private async void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            string[] result = await openFileDialog.ShowAsync(this);
            
            if (result == null || result.Length == 0)
                return;

            if (tipText != null)
            {
                graphCanvas.Children.Remove(tipText);
                tipText = null;
            }

            string fileName = result[0];
            lastFileName = fileName;
            openLast.IsEnabled = true;

            await LoadFsm(fileName);
        }

        private async void OpenJson_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            string[] result = await openFileDialog.ShowAsync(this);

            if (result == null || result.Length == 0)
                return;

            if (tipText != null)
            {
                graphCanvas.Children.Remove(tipText);
                tipText = null;
            }

            string fileName = result[0];
            lastFileName = fileName;
            openLast.IsEnabled = true;
            string data = File.ReadAllText(fileName);
            LoadJsonFSM(data, fileName);
            //System.Diagnostics.Process.Start(Environment.ProcessPath, "-Json \"" + fileName + "\"");
        }

        public void LoadJsonFSM(string data, string fileName = null)
        {
            IDataProvider jsonProvider = new JsonDataProvider(JToken.Parse(data));
            var assetInfo = new AssetInfo()
            {
                id = jsonProvider.Get<int>("fsmId"),
                providerType = AssetInfo.DataProviderType.Json,
                size = 999,
                name = jsonProvider.Get<string>("goName"),
                nameBase = jsonProvider.Get<string>("goName"),
                assetFile = fileName ?? Guid.NewGuid().ToString(),
                path = jsonProvider.Get<string>("goPath")
            };
            LoadFsm(assetInfo, jsonProvider);
        }

        private async void OpenLast_Click(object sender, RoutedEventArgs e)
        {
            await LoadFsm(lastFileName);
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            TabItem tabItem = (TabItem)fsmTabs.SelectedItem;
            if (tabItem != null)
            {
                FsmDataInstance fsmInst = (FsmDataInstance)tabItem.Tag;
                tabItems.Remove(tabItem);
                loadedFsmDatas.Remove(fsmInst);
                fsmInst.canvasControls.Clear();
            }
        }
        private void CloseAllTab_Click(object sender, RoutedEventArgs e)
        {
            TabItem tabItem = (TabItem)fsmTabs.SelectedItem;
            if (tabItem != null)
            {
                FsmDataInstance fsmInst = (FsmDataInstance)tabItem.Tag;
                fsmInst.canvasControls.Clear();
            }
            tabItems.Clear();
            loadedFsmDatas.Clear();
        }

        private async void OpenResources_Click(object sender, RoutedEventArgs e)
        {
            await CreateAssetsManagerAndLoader();

            string gamePath = await GameFileHelper.FindHollowKnightPath(this);
            if (gamePath == null)
                return;

            string resourcesPath = GameFileHelper.FindGameFilePath(gamePath, "resources.assets");

            await LoadFsm(resourcesPath);
        }

        private async void OpenSceneList_Click(object sender, RoutedEventArgs e)
        {
            await CreateAssetsManagerAndLoader();

            string gamePath = await GameFileHelper.FindHollowKnightPath(this);
            if (gamePath == null)
                return;

            //gog and mac could have multiple folders that match, so find the one with a valid assets file (?)
            string resourcesPath = GameFileHelper.FindGameFilePath(gamePath, "resources.assets");
            string dataPath = System.IO.Path.GetDirectoryName(resourcesPath);

            List<SceneInfo> sceneList = fsmLoader.LoadSceneList(dataPath);
            SceneSelectionDialog selector = new(sceneList);
            await selector.ShowDialog(this);

            long selectedId = selector.selectedID;
            bool selectedLevelFile = selector.selectedLevel;

            if (selectedId == -1)
                return;

            string format;
            if (selectedLevelFile)
                format = "level{0}";
            else
                format = "sharedassets{0}.assets";

            string assetsName = string.Format(format, selectedId);
            string fullAssetsPath = System.IO.Path.Combine(dataPath, assetsName);

            lastFileName = fullAssetsPath;
            openLast.IsEnabled = true;

            await LoadFsm(fullAssetsPath);
        }

        private void FsmTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!addingTabs)
            {
                graphCanvas.Children.Clear();
                stateList.Children.Clear();
                eventList.Children.Clear();
                variableList.Children.Clear();

                if (fsmTabs.SelectedItem != null)
                {
                    var fsmDataInst = (FsmDataInstance)((TabItem)fsmTabs.SelectedItem).Tag;

                    fsmData = fsmDataInst;
                    mt.Matrix = fsmData.matrix;

                    foreach (UINode uiNode in fsmData.nodes)
                    {
                        if (uiNode.Selected)
                        {
                            uiNode.Selected = false;
                            uiNode.Selected = true;
                            if (uiNode.stateData != null)
                            {
                                StateSidebarData(uiNode.stateData);
                            }
                            break;
                        }
                    }

                    LoadStates();
                    LoadEvents();
                    LoadVariables();
                }
            }
        }
        public async Task<bool> LoadFsm(string fileName, string defaultSearch = "")
        {
            await CreateAssetsManagerAndLoader();

            List<AssetInfo> assetInfos = fsmLoader.LoadAllFSMsFromFile(fileName);
            FSMSelectionDialog selector = new(assetInfos, System.IO.Path.GetFileName(fileName));
            if (!string.IsNullOrEmpty(defaultSearch))
            {
                var tex = selector.FindControl<AutoCompleteBox>("searchBox");
                tex.Text = defaultSearch;
                selector.RefreshFilter(defaultSearch);
            }
            await selector.ShowDialog(this);

            var assetInfo = selector.selectedAssetInfo;
            return LoadFsm(assetInfo);
        }
        public async Task<bool> LoadFsm(string fileName, string fullname, bool fallback)
        {
            await CreateAssetsManagerAndLoader();

            List<AssetInfo> assetInfos = fsmLoader.LoadAllFSMsFromFile(fileName);
            var assetInfo = assetInfos.FirstOrDefault(x => x.assetFile == fileName && x.Name == fullname);
            if(assetInfo is null)
            {
                if (fallback) return await LoadFsm(fileName, "");
                else return false;
            }
            return LoadFsm(assetInfo);
        }
        public bool LoadFsm(AssetInfo assetInfo, IDataProvider dataProvider = null)
        {
            if(assetInfo == null) return false;
            long selectedId = assetInfo.id;
            if (dataProvider == null)
            {
                if (selectedId == 0)
                    return false;
            }
            fsmData = loadedFsmDatas.FirstOrDefault(x => x.info.assetFile == assetInfo.assetFile && x.info.Name == assetInfo.Name);
            if (fsmData == null)
            {
                fsmData = dataProvider == null ? fsmLoader.LoadFSMWithAssets(selectedId, assetInfo) : fsmLoader.LoadFSM(assetInfo, dataProvider);
                loadedFsmDatas.Add(fsmData);
                fsmData.tabIndex = tabItems.Count;

                TabItem newTabItem = new()
                {
                    Header = $"{fsmData.goName}-{fsmData.fsmName}",
                    Tag = fsmData
                };

                addingTabs = true;
                tabItems.Add(newTabItem);
            }

            fsmTabs.SelectedIndex = fsmData.tabIndex;
            addingTabs = false;

            graphCanvas.Children.Clear();
            fsmData.matrix = mt.Matrix;

            stateList.Children.Clear();
            eventList.Children.Clear();
            variableList.Children.Clear();

            LoadStates();
            LoadEvents();
            LoadVariables();
            return true;
        }

        private void LoadStates()
        {
            if (fsmData.canvasControls == null)
            {
                fsmData.nodes = new List<UINode>();
                fsmData.canvasControls = new Controls();
                foreach (FsmStateData stateData in fsmData.states)
                {
                    FsmNodeData node = stateData.node;
                    UINode uiNode = new(stateData, node);

                    uiNode.grid.PointerPressed += (object sender, PointerPressedEventArgs e) =>
                    {
                        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                            return;

                        foreach (UINode uiNode2 in fsmData.nodes)
                        {
                            uiNode2.Selected = false;
                        }
                        uiNode.Selected = true;
                        StateSidebarData(stateData);
                    };

                    graphCanvas.Children.Add(uiNode.grid);
                    fsmData.nodes.Add(uiNode);

                    PlaceTransitions(node, false);
                }
                foreach (FsmNodeData globalTransition in fsmData.globalTransitions)
                {
                    FsmNodeData node = globalTransition;
                    UINode uiNode = new(null, node);

                    graphCanvas.Children.Add(uiNode.grid);
                    fsmData.nodes.Add(uiNode);

                    PlaceTransitions(node, true);
                }
                fsmData.canvasControls.AddRange(graphCanvas.Children);
            }
            else
            {
                graphCanvas.Children.Clear();
                graphCanvas.Children.AddRange(fsmData.canvasControls);
            }
        }

        private void LoadEvents()
        {
            foreach (FsmEventData eventData in fsmData.events)
            {
                eventList.Children.Add(CreateSidebarRowEvent(eventData.Name, eventData.Global));
            }
        }

        private async void LoadVariables()
        {
            fsmData.variables.Sort((a,b) => a.Type.CompareTo(b.Type));
            foreach (FsmVariableData varData in fsmData.variables)
            {
                if (varData.Values.Count == 0) continue;
                string variableType = varData.Type;

                variableList.Children.Add(CreateSidebarHeader(variableType));
                foreach (Tuple<string, object> value in varData.Values)
                {
                    await CreateSidebarRow(value.Item1, value.Item2, variableList);
                }
            }
        }

        private void StateSidebarData(FsmStateData stateData)
        {
            stateList.Children.Clear();
            var entries = stateData.ActionData;
            for(int i = 0; i < entries.Count;i++)
            {
                var entry = entries[i];
                entry.BuildView(stateList, i);
            }
        }

        public TextBlock CreateSidebarHeader(string text,  int index, bool enabled)
        {
            var header = CreateSidebarHeader($"{index}) {text}");
            if (!enabled)
            {
                header.Background = Brushes.Red;
                header.Text += " (disabled)";
            }
            return header;
        }

        public static TextBlock CreateSidebarHeader(string text)
        {
            TextBlock header = new()
            {
                Text = text,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Padding = new Thickness(5),
                Height = 28,
                FontWeight = FontWeight.Bold
            };
            return header;
        }

        public static string GetFsmEnumString(TypeDefinition enumType, int val)
        {
            var fn = enumType.FullName;
            if (enumType.IsEnum)
            {
                var isFlag = enumType.CustomAttributes.Any(x => x.AttributeType.FullName == "System.FlagAttribute");
                StringBuilder sb = isFlag ? new() : null;
                foreach (var v in enumType.Fields.Where(x => x.IsLiteral && x.Constant is int))
                {
                    var fv = (int)v.Constant;
                    if (isFlag)
                    {
                        if((fv & val) == val)
                        {
                            if(sb.Length != 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append(v.Name);
                        }
                    }
                    else
                    {
                        if (fv == val)
                        {
                            return $"{fn}::{v.Name}";
                        }
                    }
                }
                if(sb?.Length != 0)
                {
                    return $"{fn}::{sb}";
                }
            }
            return $"({fn}) {val}";
        }

        public async Task<Grid> CreateSidebarRow(string key, object rawvalue, StackPanel panel)
        {
            var value = rawvalue.ToString();
            if (rawvalue is bool)
            {
                value = value.ToLower();
            }
            Grid valueContainer = new()
            {
                Height = 28,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Background = Brushes.LightGray
            };
            panel.Children.Add(valueContainer);
            int marginRight = 0;
            INamedAssetProvider pptr = null;
            if (rawvalue is GameObjectPPtrHolder ptr) pptr = ptr.pptr;
            if (rawvalue is FsmGameObject go) pptr = go.value;
            if(rawvalue is FsmOwnerDefault fsmOwnerDefault)
            {
                if(fsmOwnerDefault.ownerOption == OwnerDefaultOption.SpecifyGameObject)
                {
                    pptr = fsmOwnerDefault.gameObject?.value;
                }
            }
            if (rawvalue is FsmEventTarget eventTarget)
            {
                if (eventTarget.gameObject?.ownerOption == OwnerDefaultOption.SpecifyGameObject)
                {
                    pptr = eventTarget.gameObject.gameObject?.value;
                }
            }
            if(rawvalue is FsmArray array)
            {
                int id = 0;
                foreach(var v in array)
                {
                    await CreateSidebarRow($"[{id++}]", v, panel);
                }
            }
            if(rawvalue is FsmArray2 array2)
            {
                value = $"[Array {array2.type}] {array2.array?.Length}";
                int id = 0;
                foreach (var v in array2.array)
                {
                    await CreateSidebarRow($"[{id++}]", v, panel);
                }
            }
            if(FSMLoader.mainAssembly != null)
            {
                if(rawvalue is FsmEnum @enum)
                {
                    if(!string.IsNullOrEmpty(@enum.enumName))
                    {
                        var enumType = FSMLoader.mainAssembly.MainModule.AssemblyReferences
                            .Select(x => FSMLoader.mainAssembly.MainModule.AssemblyResolver.Resolve(x).MainModule)
                            .Append(FSMLoader.mainAssembly.MainModule)
                            .Select(
                                x => x.GetType(@enum.enumName.Replace('+', '/'))
                                ).FirstOrDefault(x => x != null);
                        if(enumType != null)
                        {
                            value = GetFsmEnumString(enumType, @enum.intValue);
                        }
                    }
                } 
            }
            if (pptr != null)
            {
                string assetPath = pptr.file;
                if (!string.IsNullOrEmpty(pptr.file) && (File.Exists(pptr.file) || 
                    !string.IsNullOrEmpty(assetPath = GameFileHelper.FindGameFilePath(await GameFileHelper.FindHollowKnightPath(this), pptr.file))))
                {
                    Button btn = new()
                    {
                        Padding = new Thickness(5),
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    marginRight = 55;
                    btn.Content = "Search";
                    btn.Click += async (sender, ev) =>
                    {
                        await LoadFsm(assetPath, pptr.name);
                    };
                    valueContainer.Children.Add(btn);
                }
            }
            if (UEPConnect.UEPConnected)
            {
                JsonNamedAssetProvider provider = (pptr ?? (rawvalue as JsonNamedAssetProvider)) as JsonNamedAssetProvider;
                if (provider is not null && provider.instanceId is not null)
                {
                    Button btn = new()
                    {
                        Padding = new Thickness(5),
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    marginRight = 55;
                    btn.Content = "Inspect";
                    btn.Click += (sender, ev) =>
                    {
                        UEPConnect.Send("INSPECT-UOBJ\n" + provider.instanceId);
                    };
                    valueContainer.Children.Add(btn);
                }
            }
            TextBlock valueLabel = new()
            {
                Text = key,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 0, 0, 0),
                Width = 120
            };
            TextBox valueBox = new()
            {
                Margin = new Thickness(125, 0, marginRight, 0),
                IsReadOnly = true,
                Text = value
            };
            valueContainer.Children.Add(valueLabel);
            valueContainer.Children.Add(valueBox);
            return valueContainer;
        }

        public static Grid CreateSidebarRowEvent(string key, bool value)
        {
            Grid valueContainer = new()
            {
                Height = 28,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Background = Brushes.LightGray
            };
            TextBlock valueLabel = new()
            {
                Text = key,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Padding = new Thickness(5),
                Width = 120
            };
            CheckBox valueBox = new()
            {
                Margin = new Thickness(125, 0, 0, 0),
                IsEnabled = false,
                IsChecked = value,
                Content = "Global"
            };
            valueContainer.Children.Add(valueLabel);
            valueContainer.Children.Add(valueBox);
            return valueContainer;
        }

        private async void PlaceTransitions(FsmNodeData node, bool global)
        {
            float yPos = 27;
            foreach (FsmTransition trans in node.transitions)
            {
                try
                {
                    FsmStateData endState = fsmData.states.FirstOrDefault(s => s.node.name == trans.toState);
                    if (endState != null)
                    {
                        FsmNodeData endNode = endState.node;

                        Point start, end, startMiddle, endMiddle;

                        if (!global)
                        {
                            start = ArrowUtil.ComputeLocation(node, endNode, yPos, out bool isLeftStart);
                            end = ArrowUtil.ComputeLocation(endNode, node, 10, out bool isLeftEnd);

                            double dist = 40;

                            if (isLeftStart == isLeftEnd)
                                dist *= 0.5;

                            if (!isLeftStart)
                                startMiddle = new Point(start.X - dist, start.Y);
                            else
                                startMiddle = new Point(start.X + dist, start.Y);

                            if (!isLeftEnd)
                                endMiddle = new Point(end.X - dist, end.Y);
                            else
                                endMiddle = new Point(end.X + dist, end.Y);
                        }
                        else
                        {
                            start = new Point(node.transform.X + node.transform.Width / 2,
                                              node.transform.Y + node.transform.Height / 2);
                            end = new Point(endNode.transform.X + endNode.transform.Width / 2,
                                            endNode.transform.Y);
                            startMiddle = new Point(start.X, start.Y + 1);
                            endMiddle = new Point(end.X, end.Y - 1);
                        }

                        Color color = Constants.TRANSITION_COLORS[trans.colorIndex];
                        SolidColorBrush brush = new(color);

                        Avalonia.Controls.Shapes.Path line = ArrowUtil.CreateLine(start, startMiddle, endMiddle, end, brush);

                        line.PointerMoved += (object sender, PointerEventArgs e) =>
                        {
                            line.Stroke = Brushes.Black;
                        };

                        line.PointerLeave += (object sender, PointerEventArgs e) =>
                        {
                            line.Stroke = brush;
                        };

                        line.ZIndex = -1;

                        graphCanvas.Children.Add(line);
                    }
                    yPos += 16;
                }
                catch (Exception ex)
                {
                    var messageBoxStandardWindow = MessageBoxManager
                        .GetMessageBoxStandardWindow("Exception", ex.ToString());
                    await messageBoxStandardWindow.Show();
                }
            }
        }

        private async Task CreateAssetsManagerAndLoader()
        {
            if (am == null)
            {
                am = FSMAssetHelper.CreateAssetManager();
                if (am == null)
                {
                    await MessageBoxManager
                        .GetMessageBoxStandardWindow("No classdata",
                        "You're missing classdata.tpk next to the executable. Please make sure it exists.")
                        .Show();
                    Environment.Exit(0);
                }
            }

            fsmLoader ??= new FSMLoader(this, am);
        }

        #region Drag
        private Point _last;
        private bool isDragged = false;
        private void MouseDownCanvas(object sender, PointerPressedEventArgs args)
        {
            if (!args.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                return;

            _last = args.GetPosition(this);
            Cursor = new Cursor(StandardCursorType.Hand);
            isDragged = true;
        }
        private void MouseUpCanvas(object sender, PointerReleasedEventArgs args)
        {
            if (args.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                return;

            Cursor = new Cursor(StandardCursorType.Arrow);
            isDragged = false;
        }
        private void MouseMoveCanvas(object sender, PointerEventArgs args)
        {
            if (!isDragged)
                return;

            var pos = args.GetPosition(this);
            var matrix = mt.Matrix;
            matrix = new Matrix(matrix.M11, matrix.M12, matrix.M21, matrix.M22, pos.X - _last.X + matrix.M31, pos.Y - _last.Y + matrix.M32);
            mt.Matrix = matrix;
            _last = pos;

            if (fsmData != null)
                fsmData.matrix = mt.Matrix;
        }
        private void MouseScrollCanvas(object sender, PointerWheelEventArgs args)
        {
            var pos = args.GetPosition(this);
            var matrix = mt.Matrix;

            double scale = 1 + args.Delta.Y / 10;
            mt.Matrix = ZoomToLocation(matrix, new Point(pos.X - graphCanvas.Bounds.Width / 2, pos.Y - graphCanvas.Bounds.Height / 2), scale);

            if (fsmData != null)
                fsmData.matrix = mt.Matrix;
        }

        private static Matrix ZoomToLocation(Matrix mat, Point pos, double scale)
        {
            Matrix matrix = mat;

            Matrix step1 = new(1, 0, 0, 1, -pos.X, -pos.Y);
            Matrix step2 = new(scale, 0, 0, scale, 0, 0);
            Matrix step3 = new(1, 0, 0, 1, pos.X, pos.Y);

            matrix *= step1;
            matrix *= step2;
            matrix *= step3;

            //matrix = Matrix.CreateTranslation(pos.X, pos.Y) * matrix;
            //Matrix matrix = new Matrix(mat.M11, mat.M12, mat.M21, mat.M22, pos.X - mat.M31, pos.Y - mat.M32);
            //matrix = Matrix.CreateScale(scale, scale) * matrix;
            //matrix = Matrix.CreateTranslation(-pos.X, -pos.Y) * matrix;
            return matrix;
        }
        private static Matrix CreateScaling(Matrix mat, double scaleX, double scaleY, double centerX, double centerY)
        {
            return new Matrix(scaleX, 0.0, 0.0, scaleY, centerX - scaleX * centerX, centerY - scaleY * centerY) * mat;
        }
        #endregion

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
