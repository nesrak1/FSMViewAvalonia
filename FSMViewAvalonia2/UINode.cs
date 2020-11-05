using AssetsTools.NET;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSMViewAvalonia2
{
    public class UINode
    {
        public Grid grid;
        public RectangleGeometry rectGeom;
        public Rectangle rect;
        public Path rectPath;
        public TextBlock label;
        public Brush stroke;
        public FsmNodeData nodeData;
        public FsmTransition[] transitions;
        public string name;
        private bool selected;
    
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
    
                rect.Stroke = selected
                    ? new SolidColorBrush(Colors.LightBlue)
                    : stroke;
    
                rect.StrokeThickness = selected
                    ? 8
                    : 2;

                Rect transform = nodeData.transform;

                //add border and fix offset
                if (selected)
                {
                    rect.Margin = new Thickness(0, -2, 0, 0);
                    Transform = new Rect(transform.X - 1, transform.Y, transform.Width + 4, transform.Height + 5);
                }
                else
                {
                    rect.Margin = new Thickness(0, -1, 0, 0);
                    Transform = new Rect(transform.X, transform.Y, transform.Width + 2, transform.Height + 3);
                }
            }
        }
    
        public Rect Transform
        {
            get
            {
                return new Rect((double)grid.GetValue(Canvas.LeftProperty),
                                (double)grid.GetValue(Canvas.TopProperty),
                                        rectGeom.Rect.Width,
                                        rectGeom.Rect.Height);
            }
            set
            {
                grid.SetValue(Canvas.LeftProperty, value.X);
                grid.SetValue(Canvas.TopProperty, value.Y);
                rect.Width = value.Width;
                rect.Height = value.Height;
            }
        }
    
        public UINode(FsmNodeData nodeData) :
                    this(nodeData, new SolidColorBrush(Colors.LightGray), new SolidColorBrush(Colors.Black))
        { }
    
        public UINode(FsmNodeData nodeData, Brush fill, Brush stroke)
        {
            this.nodeData = nodeData;
            this.transitions = nodeData.transitions;
            this.name = nodeData.name;
    
            this.stroke = stroke;
    
            bool isGlobal = nodeData.isGlobal;

            Rect transform = nodeData.transform;
    
            grid = new Grid();
            grid.SetValue(Canvas.LeftProperty, transform.X);
            grid.SetValue(Canvas.TopProperty, transform.Y);
    
            rectGeom = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, transform.Width, transform.Height)
            };
    
            rect = new Rectangle()
            {
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 2,
                Opacity = 0.75,
                Width = transform.Width + 2,
                Height = transform.Height + 3,
                Margin = new Thickness(0, -1, 0, 0)
            };
    
            rectPath = new Path
            {
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1,
                Opacity = 0.75,
                Data = rectGeom
            };
    
            FontFamily font = new FontFamily("Segoe UI Bold");
    
            StackPanel stack = new StackPanel();
    
            label = new TextBlock
            {
                Foreground = Brushes.White,
                Text = name,
                ////Padding = new Thickness(1),
                FontFamily = font,
                //BorderBrush = Brushes.Black,
                //BorderThickness = new Thickness(1, 1, 1, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                //HorizontalContentAlignment = HorizontalAlignment.Center,
                //VerticalContentAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(nodeData.stateColor),
                MaxWidth = transform.Width,
                MinWidth = transform.Width
            };
    
            if (isGlobal)
                label.Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20));
    
            stack.Children.Add(label);
    
            if (!isGlobal)
            {
                foreach (FsmTransition transition in transitions)
                {
                    stack.Children.Add(new TextBlock
                    {
                        Background = new SolidColorBrush(nodeData.transitionColor),
                        Foreground = Brushes.DimGray,
                        Text = transition.fsmEvent.name,
                        ////Padding = new Thickness(1),
                        //BorderBrush = Brushes.Black,
                        //BorderThickness = new Thickness(1, .5, 1, .25),
                        FontFamily = font,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        //HorizontalContentAlignment = HorizontalAlignment.Center,
                        //VerticalContentAlignment = VerticalAlignment.Stretch,
                        MaxWidth = transform.Width,
                        MinWidth = transform.Width
                    });
                }
    
                List<TextBlock> list = stack.Children.OfType<TextBlock>().ToList();
                for (int index = 0; index < list.Count; index++)
                {
                    TextBlock i = list[index];
                    Grid.SetRow(i, index);
    
                    //stops lowercase descenders in the state titles
                    //from getting cut-off
                    i.MaxHeight = index == 0
                        ? (i.MinHeight = transform.Height / list.Count + 1.4)
                        : (i.MinHeight = (transform.Height - 1.4) / list.Count);
                }
            }
    
            grid.Children.Add(rect);
            grid.Children.Add(stack);
        }
    }
}
