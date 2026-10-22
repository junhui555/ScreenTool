namespace ScreenTool;

public sealed class ColorPickerForm : Form
{
    private static readonly Color TransparentKey = Color.FromArgb(255, 1, 2, 3);

    private readonly Bitmap _screen;
    private readonly Rectangle _screenBounds;
    private readonly PictureBox _magnifier = new();
    private readonly System.Windows.Forms.Timer _timer = new();
    private Point _lastPoint;

    public Color PickedColor { get; private set; }

    public ColorPickerForm()
    {
        _screenBounds = ScreenCapture.GetVirtualScreenBounds();
        _screen = ScreenCapture.CaptureAllScreens();

        Bounds = _screenBounds;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        ShowInTaskbar = false;
        Cursor = Cursors.Cross;
        DoubleBuffered = true;
        BackColor = TransparentKey;
        TransparencyKey = TransparentKey;

        _magnifier.Size = new Size(121, 121);
        _magnifier.SizeMode = PictureBoxSizeMode.StretchImage;
        _magnifier.BackColor = Color.White;
        Controls.Add(_magnifier);

        _timer.Interval = 30;
        _timer.Tick += (_, _) => UpdateMagnifier();
        _timer.Start();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Activate();
        _lastPoint = PointToClient(Cursor.Position);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        _lastPoint = e.Location;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            PickedColor = _screen.GetPixel(e.X, e.Y);
            DialogResult = DialogResult.OK;
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
        using Pen pen = new Pen(Color.Red, 1f);
        const int radius = 10;
        int x = _lastPoint.X;
        int y = _lastPoint.Y;
        e.Graphics.DrawLine(pen, x - radius - 6, y, x - 6, y);
        e.Graphics.DrawLine(pen, x + 6, y, x + radius + 6, y);
        e.Graphics.DrawLine(pen, x, y - radius - 6, x, y - 6);
        e.Graphics.DrawLine(pen, x, y + 6, x, y + radius + 6);
        e.Graphics.DrawEllipse(pen, x - radius, y - radius, radius * 2, radius * 2);
    }

    private void UpdateMagnifier()
    {
        Point pos = Cursor.Position;
        const int size = 11;
        const int zoom = 11;

        int sx = Math.Clamp(pos.X - size / 2, _screenBounds.Left, _screenBounds.Right - size);
        int sy = Math.Clamp(pos.Y - size / 2, _screenBounds.Top, _screenBounds.Bottom - size);

        using Bitmap sample = new Bitmap(size, size);
        using (Graphics g = Graphics.FromImage(sample))
            g.CopyFromScreen(sx, sy, 0, 0, new Size(size, size));

        Bitmap zoomed = new Bitmap(size * zoom, size * zoom);
        using (Graphics g = Graphics.FromImage(zoomed))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            g.DrawImage(sample, 0, 0, size * zoom, size * zoom);
            int center = size * zoom / 2;
            using Pen pen = new Pen(Color.Red, 1f);
            g.DrawLine(pen, center, 0, center, size * zoom);
            g.DrawLine(pen, 0, center, size * zoom, center);
        }

        Image? old = _magnifier.Image;
        _magnifier.Image = zoomed;
        old?.Dispose();

        Point local = PointToClient(pos);
        const int offset = 26;
        int px = local.X + offset;
        int py = local.Y + offset;
        if (px + _magnifier.Width > ClientSize.Width) px = local.X - offset - _magnifier.Width;
        if (py + _magnifier.Height > ClientSize.Height) py = local.Y - offset - _magnifier.Height;
        _magnifier.Location = new Point(Math.Max(0, px), Math.Max(0, py));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            _screen.Dispose();
        }
        base.Dispose(disposing);
    }
}
