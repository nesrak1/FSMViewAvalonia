using Path = Avalonia.Controls.Shapes.Path;

namespace FSMViewAvalonia2
{
    internal static class ArrowUtil
    {
        public static Point ComputeLocation(FsmNodeData node1, FsmNodeData node2, float yPos, out bool isLeft)
        {
            Rect nodetfm1 = node1.transform;
            Rect nodetfm2 = node2.transform;
            double midx1 = nodetfm1.X + nodetfm1.Width / 2;
            double midx2 = nodetfm2.X + nodetfm2.Width / 2;
            double midy1 = nodetfm1.Y + nodetfm1.Height / 2;
            double midy2 = nodetfm2.Y + nodetfm2.Height / 2;

            Point loc = new(
                nodetfm1.X + nodetfm1.Width / 2,
                nodetfm1.Y + yPos
            );

            if (midx1 == midx2)
            {
                isLeft = true;
            }
            else
            {
                if (Math.Abs(midx1 - midx2) * 2 < nodetfm1.Width + nodetfm2.Width)
                {
                    if (midy2 > midy1)
                        isLeft = midx1 < midx2;
                    else
                        isLeft = midx1 > midx2;
                }
                else
                {
                    isLeft = midx1 < midx2;
                }
            }

            if (isLeft)
                loc = new Point(loc.X + nodetfm1.Width / 2, loc.Y);
            else
                loc = new Point(loc.X - nodetfm1.Width / 2, loc.Y);

            return loc;
        }

        private static PathFigure BezierFromIntersection(Point startPt, Point int1, Point int2, Point endPt)
        {
            PathFigure p = new() { StartPoint = startPt };
            p.Segments.Add(new BezierSegment { Point1 = int1, Point2 = int2, Point3 = endPt });
            p.IsClosed = false;
            return p;
        }

        public static Path CreateLine(Point startPt, Point int1, Point int2, Point endPt, SolidColorBrush brush)
        {
            PathGeometry pathGeometry = new()
            {
                Figures = new PathFigures { BezierFromIntersection(startPt, int1, int2, endPt) }
            };

            Path path = new()
            {
                Data = pathGeometry,
                Stroke = brush,
                StrokeThickness = 3
            };

            return path;
        }
    }
}
