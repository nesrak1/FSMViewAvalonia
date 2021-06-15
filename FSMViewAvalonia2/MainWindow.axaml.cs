using AssetsTools.NET.Extra;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MessageBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FSMViewAvalonia2
{
    public class MainWindow : Window
    {
        //controls
        private Canvas graphCanvas;
        private MenuItem fileOpen;
        private MenuItem openSceneList;
        private MenuItem openResources;
        private MenuItem openLast;
        private MenuItem closeTab;
        private TextBlock tipText;
        private StackPanel stateList;
        private StackPanel eventList;
        private StackPanel variableList;
        private TabControl fsmTabs;
        private MatrixTransform mt;

        //variables
        private AssetsManager am;
        private FSMLoader fsmLoader;
        private FsmDataInstance fsmData;
        private string lastFileName;
        private List<FsmDataInstance> loadedFsmDatas;
        private bool addingTabs;

        //fsm info
        private ObservableCollection<TabItem> tabItems;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            //generated items
            graphCanvas = this.FindControl<Canvas>("graphCanvas");
            fileOpen = this.FindControl<MenuItem>("fileOpen");
            openSceneList = this.FindControl<MenuItem>("openSceneList");
            openResources = this.FindControl<MenuItem>("openResources");
            openLast = this.FindControl<MenuItem>("openLast");
            closeTab = this.FindControl<MenuItem>("closeTab");
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
            fileOpen.Click += FileOpen_Click;
            openLast.Click += OpenLast_Click;
            closeTab.Click += CloseTab_Click;
            openResources.Click += OpenResources_Click;
            openSceneList.Click += OpenSceneList_Click;
            fsmTabs.SelectionChanged += FsmTabs_SelectionChanged;

            loadedFsmDatas = new List<FsmDataInstance>();
            tabItems = new ObservableCollection<TabItem>();
            fsmTabs.Items = tabItems;
        }

        private async void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
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

            LoadFsm(fileName);
        }

        private void OpenLast_Click(object sender, RoutedEventArgs e)
        {
            LoadFsm(lastFileName);
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

        private async void OpenResources_Click(object sender, RoutedEventArgs e)
        {
            await CreateAssetsManagerAndLoader();

            string gamePath = await SteamHelper.FindHollowKnightPath(this);
            if (gamePath == null)
                return;

            string dataPath = System.IO.Path.Combine(gamePath, "hollow_knight_Data");
            string resourcesPath = System.IO.Path.Combine(dataPath, "resources.assets");

            LoadFsm(resourcesPath);
        }

        private async void OpenSceneList_Click(object sender, RoutedEventArgs e)
        {
            await CreateAssetsManagerAndLoader();

            string gamePath = await SteamHelper.FindHollowKnightPath(this);
            if (gamePath == null)
                return;

            string dataPath = System.IO.Path.Combine(gamePath, "hollow_knight_Data");

            List<SceneInfo> sceneList = fsmLoader.LoadSceneList(dataPath);
            SceneSelectionDialog selector = new SceneSelectionDialog(sceneList);
            await selector.ShowDialog(this);

            long selectedId = selector.selectedID;

            if (selectedId == -1)
                return;

            string levelName = "level" + selectedId;
            string fullLevelPath = System.IO.Path.Combine(dataPath, levelName);

            lastFileName = fullLevelPath;
            openLast.IsEnabled = true;

            LoadFsm(fullLevelPath);
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

        private async void LoadFsm(string fileName)
        {
            await CreateAssetsManagerAndLoader();

            List<AssetInfo> assetInfos = fsmLoader.LoadAllFSMsFromFile(fileName);
            FSMSelectionDialog selector = new FSMSelectionDialog(assetInfos);
            await selector.ShowDialog(this);
            long selectedId = selector.selectedID;

            if (selectedId == 0)
                return;

            fsmData = fsmLoader.LoadFSM(selectedId);
            loadedFsmDatas.Add(fsmData);

            TabItem newTabItem = new TabItem
            {
                Header = $"{fsmData.goName}-{fsmData.fsmName}",
                Tag = fsmData
            };

            addingTabs = true;
            tabItems.Add(newTabItem);
            fsmTabs.SelectedIndex = tabItems.Count - 1;
            addingTabs = false;

            graphCanvas.Children.Clear();
            fsmData.matrix = mt.Matrix;

            stateList.Children.Clear();
            eventList.Children.Clear();
            variableList.Children.Clear();

            LoadStates();
            LoadEvents();
            LoadVariables();
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
                    UINode uiNode = new UINode(stateData, node);

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
                    UINode uiNode = new UINode(null, node);

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

        private void LoadVariables()
        {
            foreach (FsmVariableData varData in fsmData.variables)
            {
                string variableType = varData.Type;

                variableList.Children.Add(CreateSidebarHeader(variableType));
                foreach (Tuple<string, object> value in varData.Values)
                {
                    variableList.Children.Add(CreateSidebarRow(value.Item1, value.Item2.ToString()));
                }
            }
        }

        private void StateSidebarData(FsmStateData stateData)
        {
            stateList.Children.Clear();
            var entries = stateData.ActionData;
            foreach (var entry in entries)
            {
                string actionName = entry.Name;
                var fields = entry.Values;

                stateList.Children.Add(CreateSidebarHeader(actionName));

                foreach (var field in fields)
                {
                    string key = field.Item1;
                    object value = field.Item2;
                    string valueString = value.ToString();

                    if (value is bool)
                    {
                        valueString = valueString.ToLower();
                    }

                    stateList.Children.Add(CreateSidebarRow(key, valueString));
                }
            }
        }

        private TextBlock CreateSidebarHeader(string text)
        {
            TextBlock header = new TextBlock()
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

        private Grid CreateSidebarRow(string key, string value)
        {
            Grid valueContainer = new Grid()
            {
                Height = 28,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Background = Brushes.LightGray
            };
            TextBlock valueLabel = new TextBlock()
            {
                Text = key,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Padding = new Thickness(5),
                Width = 120
            };
            TextBox valueBox = new TextBox()
            {
                Margin = new Thickness(125, 0, 0, 0),
                IsReadOnly = true,
                Text = value
            };
            valueContainer.Children.Add(valueLabel);
            valueContainer.Children.Add(valueBox);
            return valueContainer;
        }

        private Grid CreateSidebarRowEvent(string key, bool value)
        {
            Grid valueContainer = new Grid()
            {
                Height = 28,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Background = Brushes.LightGray
            };
            TextBlock valueLabel = new TextBlock()
            {
                Text = key,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Padding = new Thickness(5),
                Width = 120
            };
            CheckBox valueBox = new CheckBox()
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
                        SolidColorBrush brush = new SolidColorBrush(color);

                        Path line = ArrowUtil.CreateLine(start, startMiddle, endMiddle, end, brush);

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

            if (fsmLoader == null)
            {
                fsmLoader = new FSMLoader(this, am);
            }
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

        private Matrix ZoomToLocation(Matrix mat, Point pos, double scale)
        {
            Matrix matrix = mat;

            Matrix step1 = new Matrix(1, 0, 0, 1, -pos.X, -pos.Y);
            Matrix step2 = new Matrix(scale, 0, 0, scale, 0, 0);
            Matrix step3 = new Matrix(1, 0, 0, 1, pos.X, pos.Y);

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
