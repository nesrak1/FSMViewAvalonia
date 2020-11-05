using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

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

            Point loc = new Point(
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

        // linear equation solver utility for ai + bj = c and di + ej = f
        private static void solvexy(double a, double b, double c, double d, double e, double f, out double i, out double j)
        {
            j = (c - a / d * f) / (b - a * e / d);
            i = (c - (b * j)) / a;
        }

        // basis functions
        private static double b0(double t) { return Math.Pow(1 - t, 3); }
        private static double b1(double t) { return t * (1 - t) * (1 - t) * 3; }
        private static double b2(double t) { return (1 - t) * t * t * 3; }
        private static double b3(double t) { return Math.Pow(t, 3); }

        private static void bez4pts1(double x0, double y0, double x4, double y4, double x5, double y5, double x3, double y3, out double x1, out double y1, out double x2, out double y2)
        {
            // find chord lengths
            double c1 = Math.Sqrt((x4 - x0) * (x4 - x0) + (y4 - y0) * (y4 - y0));
            double c2 = Math.Sqrt((x5 - x4) * (x5 - x4) + (y5 - y4) * (y5 - y4));
            double c3 = Math.Sqrt((x3 - x5) * (x3 - x5) + (y3 - y5) * (y3 - y5));
            // guess "best" t
            double t1 = c1 / (c1 + c2 + c3);
            double t2 = (c1 + c2) / (c1 + c2 + c3);
            // transform x1 and x2
            solvexy(b1(t1), b2(t1), x4 - (x0 * b0(t1)) - (x3 * b3(t1)), b1(t2), b2(t2), x5 - (x0 * b0(t2)) - (x3 * b3(t2)), out x1, out x2);
            // transform y1 and y2
            solvexy(b1(t1), b2(t1), y4 - (y0 * b0(t1)) - (y3 * b3(t1)), b1(t2), b2(t2), y5 - (y0 * b0(t2)) - (y3 * b3(t2)), out y1, out y2);
        }

        private static PathFigure BezierFromIntersection(Point startPt, Point int1, Point int2, Point endPt)
        {
            bez4pts1(startPt.X, startPt.Y, int1.X, int1.Y, int2.X, int2.Y, endPt.X, endPt.Y, out double x1, out double y1, out double x2, out double y2);
            PathFigure p = new PathFigure { StartPoint = startPt };
            p.Segments.Add(new BezierSegment { Point1 = new Point(x1, y1), Point2 = new Point(x2, y2), Point3 = endPt });
            p.IsClosed = false;
            return p;
        }

        public static Path CreateLine(Point startPt, Point int1, Point int2, Point endPt, SolidColorBrush brush)
        {
            PathGeometry pathGeometry = new PathGeometry
            {
                Figures = new PathFigures { BezierFromIntersection(startPt, int1, int2, endPt) }
            };

            Path path = new Path();
            path.Data = pathGeometry;
            path.Stroke = brush;
            path.StrokeThickness = 2;

            return path;
        }
    }
}
