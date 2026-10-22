namespace ScreenTool;

internal static class ScreenCapture
{
    public static Rectangle GetVirtualScreenBounds()
    {
        Rectangle bounds = Rectangle.Empty;
        foreach (Screen screen in Screen.AllScreens)
            bounds = Rectangle.Union(bounds, screen.Bounds);
        return bounds;
    }

    public static Bitmap CaptureAllScreens()
    {
        Rectangle bounds = GetVirtualScreenBounds();
        Bitmap bmp = new Bitmap(bounds.Width, bounds.Height);
        using Graphics g = Graphics.FromImage(bmp);
        g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
        return bmp;
    }

    public static Bitmap CaptureRegion(Rectangle region)
    {
        Bitmap bmp = new Bitmap(region.Width, region.Height);
        using Graphics g = Graphics.FromImage(bmp);
        g.CopyFromScreen(region.Left, region.Top, 0, 0, region.Size);
        return bmp;
    }
}
