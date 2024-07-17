namespace FSMViewAvalonia2;
public partial class MainWindow
{
    private void InitView()
    {
        PointerPressed += MouseDownCanvas;
        PointerReleased += MouseUpCanvas;
        PointerMoved += MouseMoveCanvas;
        PointerWheelChanged += MouseScrollCanvas;
    }
    #region Drag
    private Point _last;
    private bool isDragged = false;
    private void MouseDownCanvas(object sender, PointerPressedEventArgs args)
    {
        if (!args.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            return;
        }

        _last = args.GetPosition(this);
        Cursor = new Cursor(StandardCursorType.Hand);
        isDragged = true;
    }
    private void MouseUpCanvas(object sender, PointerReleasedEventArgs args)
    {
        if (args.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            return;
        }

        Cursor = new Cursor(StandardCursorType.Arrow);
        isDragged = false;
    }
    private void MouseMoveCanvas(object sender, PointerEventArgs args)
    {
        if (!isDragged)
        {
            return;
        }

        Point pos = args.GetPosition(this);
        Matrix matrix = mt.Matrix;
        matrix = new Matrix(matrix.M11, matrix.M12, matrix.M21, matrix.M22, pos.X - _last.X + matrix.M31, pos.Y - _last.Y + matrix.M32);
        mt.Matrix = matrix;
        _last = pos;

        if (currentFSMData != null)
        {
            currentFSMData.matrix = mt.Matrix;
        }
    }
    private void MouseScrollCanvas(object sender, PointerWheelEventArgs args)
    {
        Point pos = args.GetPosition(this);
        Matrix matrix = mt.Matrix;

        double scale = 1 + (args.Delta.Y / 10);
        mt.Matrix = ZoomToLocation(matrix, new Point(pos.X - (graphCanvas.Bounds.Width / 2), pos.Y - (graphCanvas.Bounds.Height / 2)), scale);

        if (currentFSMData != null)
        {
            currentFSMData.matrix = mt.Matrix;
        }
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
    private static Matrix CreateScaling(Matrix mat, double scaleX, double scaleY, double centerX, double centerY) => new Matrix(scaleX, 0.0, 0.0, scaleY, centerX - (scaleX * centerX), centerY - (scaleY * centerY)) * mat;
    #endregion
}
