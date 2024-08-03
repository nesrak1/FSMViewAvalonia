using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;


namespace FSMViewAvalonia2;
public partial class MainWindow : Window
{
    //controls
    private readonly MatrixTransform mt;

    public static MainWindow instance;
    //variables

    public FSMLoader fsmLoader;
    private FsmDataInstanceUI currentFSMData;
    private string lastFileName;
    private bool lastIsBundle;
    private readonly List<FsmDataInstanceUI> loadedFsmDatas = [];
    private bool addingTabs;

    //fsm info
    private readonly ObservableCollection<TabItem> tabItems = [];
    public static readonly FontFamily font = new("Segoe UI Bold");

    public MainWindow()
    {
#if DEBUG
        this.AttachDevTools();
#endif
        instance = this;
        App.mainWindow = this;
        InitializeComponent();

        mt = graphCanvas.RenderTransform as MatrixTransform;

        InitView();

        InitFSMLoader();

        InitFind();

        InitOption();
        
    }
    private void InitOption()
    {
        option_includeSharedassets.IsChecked = Config.config.option_includeSharedassets;
        option_includeSharedassets.IsCheckedChanged += (_, ev) => Config.config.option_includeSharedassets =
            option_includeSharedassets.IsChecked ?? false;
        option_currentGame.Items.Clear();
        foreach (var v in GameFileHelper.allGameInfos)
        {
            _ = option_currentGame.Items.Add(v);
        }
        option_currentGame.SelectedIndex = GameFileHelper.CurrentGameInfo.Index;
        option_currentGame.SelectionChanged += (_, ev) =>
        {
            Config.config.currentGame = option_currentGame.SelectedIndex;
            UpdateCurrentGame();
            Config.Save();
        };
        UpdateCurrentGame();
    }
    private void InitFSMLoader()
    {
        openJson.Click += OpenJson_Click;
        fileOpen.Click += FileOpen_Click;
        openLast.Click += OpenLast_Click;
        closeTab.Click += CloseTab_Click;
        closeAllTab.Click += CloseAllTab_Click;
        openResources.Click += OpenResources_Click;
        openSceneList.Click += OpenSceneList_Click;
        openBundle.Click += OpenBundle_Click;
        fsmTabs.SelectionChanged += FsmTabs_SelectionChanged;
        fsmTabs.ItemsSource = tabItems;
    }

    private async void OpenBundle_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new();
        string[] result = await openFileDialog.ShowAsync(this);

        if (result == null || result.Length == 0)
        {
            return;
        }

        if (tipText != null)
        {
            _ = graphCanvas.Children.Remove(tipText);
            tipText = null;
        }

        string fileName = result[0];
        lastFileName = fileName;
        lastIsBundle = true;
        openLast.IsEnabled = true;

        _ = await LoadFsm(fileName, true);
    }

    private async void FileOpen_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new();
        string[] result = await openFileDialog.ShowAsync(this);

        if (result == null || result.Length == 0)
        {
            return;
        }

        if (tipText != null)
        {
            _ = graphCanvas.Children.Remove(tipText);
            tipText = null;
        }

        string fileName = result[0];
        lastFileName = fileName;
        lastIsBundle = false;
        openLast.IsEnabled = true;

        _ = await LoadFsm(fileName, false);
    }

    private async void OpenJson_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new();
        string[] result = await openFileDialog.ShowAsync(this);

        if (result == null || result.Length == 0)
        {
            return;
        }

        if (tipText != null)
        {
            _ = graphCanvas.Children.Remove(tipText);
            tipText = null;
        }

        string fileName = result[0];
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
            name = jsonProvider.Get<string>("goName"),
            nameBase = jsonProvider.Get<string>("goName"),
            assetFile = fileName ?? Guid.NewGuid().ToString(),
            path = jsonProvider.Get<string>("goPath")
        };
        _ = LoadFsm(assetInfo, jsonProvider);
    }

    private async void OpenLast_Click(object sender, RoutedEventArgs e) => await LoadFsm(lastFileName, lastIsBundle);

    private void CloseTab_Click(object sender, RoutedEventArgs e)
    {
        var tabItem = (TabItem) fsmTabs.SelectedItem;
        if (tabItem != null)
        {
            var fsmInst = (FsmDataInstanceUI) tabItem.Tag;
            _ = tabItems.Remove(tabItem);
            _ = loadedFsmDatas.Remove(fsmInst);
            fsmInst.canvasControls.Clear();
        }
    }
    private void CloseAllTab_Click(object sender, RoutedEventArgs e)
    {
        var tabItem = (TabItem) fsmTabs.SelectedItem;
        if (tabItem != null)
        {
            var fsmInst = (FsmDataInstanceUI) tabItem.Tag;
            fsmInst.canvasControls.Clear();
        }

        tabItems.Clear();
        loadedFsmDatas.Clear();
    }

    private async void OpenResources_Click(object sender, RoutedEventArgs e)
    {
        CreateAssetsManagerAndLoader();

        string gamePath = await GameFileHelper.FindGamePath(this);
        if (gamePath == null)
        {
            return;
        }

        string resourcesPath = GameFileHelper.FindGameFilePath(gamePath, "resources.assets");

        _ = await LoadFsm(resourcesPath, false);
    }

    private async void OpenSceneList_Click(object sender, RoutedEventArgs e)
    {
        CreateAssetsManagerAndLoader();

        string gamePath = await GameFileHelper.FindGamePath(this);
        if (gamePath == null)
        {
            return;
        }

        //gog and mac could have multiple folders that match, so find the one with a valid assets file (?)
        string resourcesPath = GameFileHelper.FindGameFilePath(gamePath, "resources.assets");
        string dataPath = System.IO.Path.GetDirectoryName(resourcesPath);

        List<SceneInfo> sceneList = fsmLoader.LoadSceneList();
        SceneSelectionDialog selector = new(sceneList);
        await selector.ShowDialog(this);

        long selectedId = selector.selectedID;
        bool selectedLevelFile = selector.selectedLevel;

        if (selectedId == -1)
        {
            return;
        }

        string format = selectedLevelFile ? "level{0}" : "sharedassets{0}.assets";

        string assetsName = string.Format(format, selectedId);
        string fullAssetsPath = System.IO.Path.Combine(dataPath, assetsName);

        lastIsBundle = false;
        lastFileName = fullAssetsPath;
        openLast.IsEnabled = true;

        _ = await LoadFsm(fullAssetsPath, false);
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
                var fsmDataInst = (FsmDataInstanceUI) ((TabItem) fsmTabs.SelectedItem).Tag;

                currentFSMData = fsmDataInst;
                mt.Matrix = currentFSMData.matrix;

                foreach (UINode uiNode in currentFSMData.nodes)
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

    public async Task<bool> LoadFsm(string fileName, bool isBundle, string defaultSearch = "")
    {
        CreateAssetsManagerAndLoader();

        List<AssetInfo> assetInfos = isBundle ? fsmLoader.LoadAllFSMsFromBundle(fileName) :
            fsmLoader.LoadAllFSMsFromFile(fileName);
        FSMSelectionDialog selector = new(assetInfos, fileName);
        if (!string.IsNullOrEmpty(defaultSearch))
        {
            AutoCompleteBox tex = selector.FindControl<AutoCompleteBox>("searchBox");
            tex.Text = defaultSearch;
            selector.RefreshFilter(defaultSearch);
        }

        await selector.ShowDialog(this);

        AssetInfo assetInfo = selector.selectedAssetInfo;
        return LoadFsm(assetInfo);
    }

    public async Task<bool> LoadFsm(string fileName, string fullname, bool fallback)
    {
        CreateAssetsManagerAndLoader();

        List<AssetInfo> assetInfos = fsmLoader.LoadAllFSMsFromFile(fileName);
        AssetInfo assetInfo = assetInfos.FirstOrDefault(x => x.assetFile == fileName && x.Name == fullname);
        return assetInfo is null ? fallback && await LoadFsm(fileName, false, fullname) : LoadFsm(assetInfo);
    }

    public bool LoadFsm(AssetInfo assetInfo, IDataProvider dataProvider = null)
    {
        if (assetInfo == null)
        {
            return false;
        }

        long selectedId = assetInfo.id;
        if (dataProvider == null)
        {
            if (selectedId == 0)
            {
                return false;
            }
        }

        currentFSMData = loadedFsmDatas.FirstOrDefault(x => x.fsm.info.assetFile == assetInfo.assetFile &&
                                                        x.fsm.info.Name == assetInfo.Name &&
                                                        x.fsm.info.ProviderType == assetInfo.ProviderType &&
                                                        (
                                                        x.fsm.info is not AssetInfoUnity xaiu ||
                                                        assetInfo is not AssetInfoUnity aiu ||
                                                        (xaiu.goId == aiu.goId &&
                                                        xaiu.fsmId == aiu.fsmId)
                                                        )
                                                        );
        if (currentFSMData == null)
        {
            currentFSMData = new(
                dataProvider == null ?
                (assetInfo is AssetInfoUnity uinfo ?
                    fsmLoader.LoadFSMWithAssets(selectedId, uinfo) :
                    throw new NotSupportedException())
                : new(assetInfo, dataProvider)
                );
            loadedFsmDatas.Add(currentFSMData);
            currentFSMData.tabIndex = tabItems.Count;

            TabItem newTabItem = new()
            {
                Header = $"{currentFSMData.fsm.goName}-{currentFSMData.fsm.fsmName}",
                Tag = currentFSMData
            };

            addingTabs = true;
            tabItems.Add(newTabItem);
        }

        fsmTabs.SelectedIndex = currentFSMData.tabIndex;
        addingTabs = false;

        graphCanvas.Children.Clear();
        currentFSMData.matrix = mt.Matrix = Matrix.Identity;

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
        if (currentFSMData.canvasControls == null)
        {
            currentFSMData.nodes = [];
            currentFSMData.canvasControls = [];
            foreach (FsmStateData stateData in currentFSMData.states)
            {
                FsmNodeData node = stateData.node;
                UINode uiNode = new(stateData, node);

                uiNode.grid.PointerPressed += (object sender, PointerPressedEventArgs e) =>
                {
                    if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    {
                        return;
                    }

                    foreach (UINode uiNode2 in currentFSMData.nodes)
                    {
                        uiNode2.Selected = false;
                    }

                    uiNode.Selected = true;
                    StateSidebarData(stateData);
                };

                graphCanvas.Children.Add(uiNode.grid);
                currentFSMData.nodes.Add(uiNode);

                PlaceTransitions(node, false);
            }

            foreach (FsmGlobalTransition globalTransition in currentFSMData.fsm.globalTransitions)
            {
                FsmNodeData node = new(currentFSMData, globalTransition);
                UINode uiNode = new(null, node);

                graphCanvas.Children.Add(uiNode.grid);
                currentFSMData.nodes.Add(uiNode);

                PlaceTransitions(node, true);
            }

            currentFSMData.canvasControls.AddRange(graphCanvas.Children);
            (stateList.Parent as ScrollViewer)!.ScrollToHome();
        }
        else
        {
            graphCanvas.Children.Clear();
            graphCanvas.Children.AddRange(currentFSMData.canvasControls);
        }
    }

    private void LoadEvents()
    {
        foreach (FsmEventData eventData in currentFSMData.fsm.events)
        {
            eventList.Children.Add(CreateSidebarRowEvent(eventData.Name, eventData.Global));
        }

        (eventList.Parent as ScrollViewer)!.ScrollToHome();
    }

    private async void LoadVariables()
    {
        currentFSMData.fsm.variables.Sort((a, b) => a.Type.CompareTo(b.Type));
        foreach (FsmVariableData varData in currentFSMData.fsm.variables)
        {
            if (varData.Values.Count == 0)
            {
                continue;
            }

            string variableType = varData.Type;

            variableList.Children.Add(CreateSidebarHeader(variableType));
            foreach (FsmVariableData.ValueTuple value in varData.Values)
            {
                _ = await CreateSidebarRow(currentFSMData.fsm.info.assemblyProvider,
                    new(value.Name, value.RawValue, value.Value, null), variableList);
            }
        }

    (variableList.Parent as ScrollViewer)!.ScrollToHome();
    }

    private void StateSidebarData(FsmStateData stateData)
    {
        stateList.Children.Clear();
        IReadOnlyList<IActionScriptEntry> entries = stateData.ActionData;
        for (int i = 0; i < entries.Count; i++)
        {
            IActionScriptEntry entry = entries[i];
            var ui = new FsmStateActionUI((FsmStateAction) entry);
            ui.BuildView(stateList, i);
        }
    }

    public TextBlock CreateSidebarHeader(string text, int index, bool enabled)
    {

        TextBlock header = CreateSidebarHeader($"{index}) {text}");

        if (!enabled)
        {
            header.Background = Brushes.Red;
            header.Text += " (disabled)";
        }

        return header;
    }

    public TextBlock CreateSidebarHeader(string text)
    {
        _ = this.TryFindResource("ThemeControlLowBrush", out object background);
        TextBlock header = new()
        {
            Text = text,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            TextAlignment = TextAlignment.Left,
            Padding = new Thickness(5),
            Height = 28,
            FontWeight = FontWeight.Bold,
            Background = (IBrush) background,
            FontFamily = font
        };
        return header;
    }



    public async Task<Grid> CreateSidebarRow(IAssemblyProvider assemblyProvider,
        IActionScriptEntry.PropertyInfo prop, StackPanel panel)
    {
        _ = this.TryGetResource("ThemeBackgroundBrush", out object background);
        object rawvalue = prop.RawValue;
        string value = rawvalue.ToString();
        if (rawvalue is bool)
        {
            value = value.ToLower();
        }

        Grid valueContainer = new()
        {
            Height = 28,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Background = (IBrush) background,
        };
        panel.Children.Add(valueContainer);
        int marginRight = 0;
        INamedAssetProvider pptr = null;
        if (rawvalue is GameObjectPPtrHolder ptr)
        {
            pptr = ptr.pptr;
        }

        if (rawvalue is FsmGameObject go)
        {
            pptr = go.value;
        }

        if (rawvalue is FsmOwnerDefault fsmOwnerDefault)
        {
            if (fsmOwnerDefault.ownerOption == OwnerDefaultOption.SpecifyGameObject)
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

        if (rawvalue is FsmArray array)
        {
            int id = 0;
            foreach (object v in array)
            {
                _ = await CreateSidebarRow(assemblyProvider, new($"[{id++}]", v, v, null), panel);
            }
        }

        if (rawvalue is FsmArray2 array2)
        {
            value = $"[Array {array2.type}] {array2.array?.Length}";
            int id = 0;
            foreach (object v in array2.array)
            {
                _ = await CreateSidebarRow(assemblyProvider, new($"[{id++}]", v, v, null), panel);
            }
        }

        if (assemblyProvider != null)
        {
            if (rawvalue is FsmEnum @enum)
            {
                if (!string.IsNullOrEmpty(@enum.enumName))
                {
                    TypeDefinition enumType = assemblyProvider.GetType(@enum.enumName.Replace('+', '/'));
                    if (enumType != null)
                    {
                        value = Utils.GetFsmEnumString(enumType, @enum.intValue);
                    }
                }
            }
        }

        if (pptr != null)
        {
            string assetPath = pptr.file;
            if (!string.IsNullOrEmpty(pptr.file) && (File.Exists(pptr.file) ||
                !string.IsNullOrEmpty(assetPath = GameFileHelper.FindGameFilePath(await GameFileHelper.FindGamePath(this), pptr.file))))
            {
                Button btn = new()
                {
                    Padding = new Thickness(5),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                marginRight = 55;
                btn.Content = "Search";
                btn.Click += async (sender, ev) => await LoadFsm(assetPath, false, pptr.name);
                valueContainer.Children.Add(btn);
            }
        }

        string name = prop.Name;
        if (prop.UIHint is not null)
        {
            UIHint uihint = prop.UIHint.Value;
            name += $" [{uihint}]";
        }

        TextBlock valueLabel = new()
        {
            Text = name,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Padding = new Thickness(5),
            Margin = new Thickness(0, 0, 0, 0),
            Width = 120,
            FontFamily = font
        };
        ToolTip.SetTip(valueLabel, name);
        TextBox valueBox = new()
        {
            Margin = new Thickness(125, 0, marginRight, 0),
            IsReadOnly = true,
            Text = value,
            FontFamily = font
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
        };
        TextBlock valueLabel = new()
        {
            Text = key,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Padding = new Thickness(5),
            Width = 120,
            FontFamily = font
        };
        CheckBox valueBox = new()
        {
            Margin = new Thickness(125, 0, 0, 0),
            IsEnabled = false,
            IsChecked = value,
            Content = "Global",
            FontFamily = font
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
                FsmStateData endState = currentFSMData.states.FirstOrDefault(s => s.node.name == trans.toState);
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
                        {
                            dist *= 0.5;
                        }

                        startMiddle = !isLeftStart ? new Point(start.X - dist, start.Y) : new Point(start.X + dist, start.Y);

                        endMiddle = !isLeftEnd ? new Point(end.X - dist, end.Y) : new Point(end.X + dist, end.Y);
                    }
                    else
                    {
                        start = new Point(node.transform.X + (node.transform.Width / 2),
                                          node.transform.Y + (node.transform.Height / 2));
                        end = new Point(endNode.transform.X + (endNode.transform.Width / 2),
                                        endNode.transform.Y);
                        startMiddle = new Point(start.X, start.Y + 1);
                        endMiddle = new Point(end.X, end.Y - 1);
                    }

                    Color color = Constants.TRANSITION_COLORS[trans.colorIndex];
                    SolidColorBrush brush = new(color);

                    Avalonia.Controls.Shapes.Path line = ArrowUtil.CreateLine(start, startMiddle, endMiddle, end, brush);

                    line.PointerMoved += (object sender, PointerEventArgs e) => line.Stroke = Brushes.Black;

                    line.PointerExited += (object sender, PointerEventArgs e) => line.Stroke = brush;

                    line.ZIndex = -1;

                    graphCanvas.Children.Add(line);
                }

                yPos += 16;
            }
            catch (Exception ex)
            {
                IMsBox<ButtonResult> messageBoxStandardWindow = MessageBoxManager
                        .GetMessageBoxStandard("Exception", ex.ToString());
                _ = await messageBoxStandardWindow.ShowAsync();
            }
        }
    }

    private void CreateAssetsManagerAndLoader()
    {
        if (fsmLoader == null)
        {
            FSMAssetHelper.Init();
            fsmLoader = new FSMLoader(this);
        }


    }

    public void UpdateCurrentGame()
    {
        var isNone = GameFileHelper.CurrentGameInfo.IsNone;
        findMenuRoot.IsEnabled = !isNone;
        findInAllScenes.IsEnabled = !isNone;
        openResources.IsEnabled = !isNone;
        openSceneList.IsEnabled = !isNone;
        generateFsmList.IsEnabled = !isNone;

        if (isNone)
        {
            Title = "FSMView Avalonia";
        }
        else
        {
            Title = "FSMView Avalonia for " + GameFileHelper.CurrentGameInfo.Name;
        }
    }
}
