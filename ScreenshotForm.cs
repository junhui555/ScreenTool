namespace ScreenTool;

public sealed class ScreenshotForm : Form
{
    private readonly Bitmap _screen;
    private Point _start;
    private Point _current;
    private bool _dragging;

    public Bitmap? Captured { get; private set; }

    public ScreenshotForm()
    {
        Rectangle bounds = ScreenCapture.GetVirtualScreenBounds();
        Bounds = bounds;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        ShowInTaskbar = false;
        Cursor = Cursors.Cross;
        DoubleBuffered = true;
        _screen = ScreenCapture.CaptureAllScreens();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Activate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _start = e.Location;
            _current = e.Location;
            _dragging = true;
            Invalidate();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging)
        {
            _current = e.Location;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left && _dragging)
        {
            _dragging = false;
            Rectangle rect = Normalize(_start, _current);
            if (rect.Width >= 3 && rect.Height >= 3)
            {
                Captured = ScreenCapture.CaptureRegion(rect);
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Escape)
            DialogResult = DialogResult.Cancel;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using Bitmap overlay = new Bitmap(Bounds.Width, Bounds.Height);
        using (Graphics g = Graphics.FromImage(overlay))
        {
            g.DrawImage(_screen, 0, 0);
            using (Brush dark = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                g.FillRectangle(dark, 0, 0, overlay.Width, overlay.Height);

            if (_dragging)
            {
                Rectangle rect = Normalize(_start, _current);
                g.DrawImage(_screen, rect, rect, GraphicsUnit.Pixel);
                using Pen pen = new Pen(Color.FromArgb(255, 46, 140, 255), 2f);
                g.DrawRectangle(pen, rect);
                string sizeText = rect.Width + " x " + rect.Height;
                using Font font = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold);
                g.DrawString(sizeText, font, Brushes.White, rect.Left, Math.Max(0, rect.Top - 24));
            }
        }
        e.Graphics.DrawImage(overlay, 0, 0);
    }

    private static Rectangle Normalize(Point a, Point b)
    {
        int x = Math.Min(a.X, b.X);
        int y = Math.Min(a.Y, b.Y);
        int w = Math.Abs(a.X - b.X);
        int h = Math.Abs(a.Y - b.Y);
        return new Rectangle(x, y, w, h);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _screen.Dispose();
        base.Dispose(disposing);
    }
}
