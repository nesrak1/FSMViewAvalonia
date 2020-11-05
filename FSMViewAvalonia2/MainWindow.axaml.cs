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
        private MenuItem openLast;
        private TextBlock tipText;
        private MatrixTransform mt;

        //variables
        private AssetsManager am;
        private FSMLoader fsmLoader;
        private FsmDataInstance fsmData;
        private string lastFilename;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
            //generated items
            graphCanvas = this.FindControl<Canvas>("graphCanvas");
            fileOpen = this.FindControl<MenuItem>("fileOpen");
            openLast = this.FindControl<MenuItem>("openLast");
            tipText = this.FindControl<TextBlock>("tipText");
            mt = graphCanvas.RenderTransform as MatrixTransform;
            //generated events
            PointerPressed += MouseDownCanvas;
            PointerReleased += MouseUpCanvas;
            PointerMoved += MouseMoveCanvas;
            PointerWheelChanged += MouseScrollCanvas;
            fileOpen.Click += FileOpen_Click;
        }

        //ui events
        private void Eee_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxStandardWindow = MessageBoxManager
                .GetMessageBoxStandardWindow("EEE", "You clicked EEE");
            messageBoxStandardWindow.Show();
        }

        public async void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            if (am == null)
                am = FSMAssetHelper.CreateAssetManager();

            if (fsmLoader == null)
                fsmLoader = new FSMLoader(this, am);

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
            lastFilename = fileName;
            openLast.IsEnabled = true;

            List<AssetInfo> assetInfos = fsmLoader.LoadAllFSMsFromFile(fileName);
            FSMSelectionDialog selector = new FSMSelectionDialog(assetInfos);
            await selector.ShowDialog(this);
            long selectedId = selector.selectedID;

            if (selectedId == 0)
                return;

            fsmData = fsmLoader.LoadFSM(selectedId);
                
            foreach (FsmStateData stateData in fsmData.states)
            {
                FsmNodeData node = stateData.node;
                UINode uiNode = new UINode(node);

                graphCanvas.Children.Add(uiNode.grid);

                PlaceTransitions(stateData, node);
            }
        }

        private async void PlaceTransitions(FsmStateData stateData, FsmNodeData node)
        {
            float yPos = 24;
            foreach (FsmTransition trans in node.transitions)
            {
                try
                {
                    FsmStateData endState = fsmData.states.FirstOrDefault(s => s.node.name == trans.toState);
                    if (endState != null)
                    {
                        FsmNodeData endNode = endState.node;

                        Point start, end, startMiddle, endMiddle;

                        if (stateData != null)
                        {
                            start = ArrowUtil.ComputeLocation(node, endNode, yPos, out bool isLeftStart);
                            end = ArrowUtil.ComputeLocation(endNode, node, 10, out bool isLeftEnd);

                            double dist = 30;

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
                                              node.transform.Y + node.transform.Height);
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
        }
        private void MouseScrollCanvas(object sender, PointerWheelEventArgs args)
        {
            var pos = args.GetPosition(this);
            var matrix = mt.Matrix;

            double scale = 1 + args.Delta.Y / 10;
            //matrix.ScaleAtPrepend(scale, scale, pos.X, pos.Y);
            mt.Matrix = ZoomToLocation(matrix, new Point(pos.X - graphCanvas.Bounds.Width / 2, pos.Y - graphCanvas.Bounds.Height / 2), scale);
            //mt.Matrix = CreateScaling(matrix, scale, scale, pos.X, pos.Y);
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
