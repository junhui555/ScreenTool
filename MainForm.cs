using System.Drawing.Imaging;
using System.IO.Ports;
using System.Text;

namespace ScreenTool;

public sealed class MainForm : Form
{
    private readonly TabControl _tabs = new();

    private readonly PictureBox _preview = new();
    private readonly Label _captureStatus = new();

    private readonly Panel _swatch = new();
    private readonly Label _hexLabel = new();
    private readonly Label _rgbLabel = new();

    private readonly ComboBox _portCombo = new();
    private readonly Button _refreshPortsButton = new();
    private readonly ComboBox _baudCombo = new();
    private readonly ComboBox _dataBitsCombo = new();
    private readonly ComboBox _parityCombo = new();
    private readonly ComboBox _stopBitsCombo = new();
    private readonly Button _openPortButton = new();
    private readonly TextBox _receiveBox = new();
    private readonly TextBox _sendBox = new();
    private readonly CheckBox _hexSendCheck = new();
    private readonly Button _sendButton = new();
    private readonly Button _clearReceiveButton = new();
    private readonly Label _serialStatus = new();

    private SerialPort? _port;

    public MainForm()
    {
        Text = "屏幕工具箱 — 截图 / 取色 / 串口";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(980, 640);
        MinimumSize = new Size(860, 560);
        Font = new Font("Microsoft YaHei UI", 9f);

        BuildUi();
        RefreshPorts();
    }

    private void BuildUi()
    {
        _tabs.Dock = DockStyle.Fill;
        Controls.Add(_tabs);

        _tabs.TabPages.Add(BuildScreenshotTab());
        _tabs.TabPages.Add(BuildColorTab());
        _tabs.TabPages.Add(BuildSerialTab());
    }

    private TabPage BuildScreenshotTab()
    {
        TabPage page = new TabPage("截图");

        Panel top = new Panel { Dock = DockStyle.Top, Height = 46 };
        Button fullButton = new Button { Text = "全屏截图", Location = new Point(12, 10), Size = new Size(100, 28) };
        Button regionButton = new Button { Text = "区域截图", Location = new Point(122, 10), Size = new Size(100, 28) };
        _captureStatus.AutoSize = true;
        _captureStatus.Location = new Point(240, 16);
        _captureStatus.Text = "点击按钮开始截图，结果会自动保存为 PNG 并复制到剪贴板。";

        fullButton.Click += (_, _) => FullScreenCapture();
        regionButton.Click += (_, _) => RegionCapture();
        top.Controls.Add(fullButton);
        top.Controls.Add(regionButton);
        top.Controls.Add(_captureStatus);

        _preview.Dock = DockStyle.Fill;
        _preview.BorderStyle = BorderStyle.FixedSingle;
        _preview.BackColor = Color.White;
        _preview.SizeMode = PictureBoxSizeMode.Zoom;

        page.Controls.Add(_preview);
        page.Controls.Add(top);
        return page;
    }

    private TabPage BuildColorTab()
    {
        TabPage page = new TabPage("取色");

        GroupBox group = new GroupBox { Text = "屏幕取色", Location = new Point(12, 12), Size = new Size(460, 170) };

        Button pickButton = new Button { Text = "屏幕取色", Location = new Point(14, 30), Size = new Size(110, 32) };
        pickButton.Click += (_, _) => PickColor();

        _swatch.Location = new Point(150, 26);
        _swatch.Size = new Size(96, 96);
        _swatch.BorderStyle = BorderStyle.FixedSingle;
        _swatch.BackColor = Color.Black;

        _hexLabel.AutoSize = true;
        _hexLabel.Location = new Point(266, 34);
        _hexLabel.Text = "HEX: #000000";
        _hexLabel.Font = new Font("Consolas", 12f, FontStyle.Bold);

        _rgbLabel.AutoSize = true;
        _rgbLabel.Location = new Point(266, 66);
        _rgbLabel.Text = "RGB: 0, 0, 0";

        Label hint = new Label
        {
            AutoSize = true,
            Location = new Point(14, 88),
            Text = "点击取色后在屏幕上移动，单击拾取颜色；\r\n拾取后自动将 HEX 值复制到剪贴板，按 Esc 取消。"
        };

        group.Controls.Add(pickButton);
        group.Controls.Add(_swatch);
        group.Controls.Add(_hexLabel);
        group.Controls.Add(_rgbLabel);
        group.Controls.Add(hint);
        page.Controls.Add(group);
        return page;
    }

    private TabPage BuildSerialTab()
    {
        TabPage page = new TabPage("串口");

        Panel top = new Panel { Dock = DockStyle.Top, Height = 84 };

        AddLabel(top, "串口", 12, 16);
        _portCombo.Location = new Point(48, 13);
        _portCombo.Width = 100;
        _portCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        top.Controls.Add(_portCombo);

        _refreshPortsButton.Text = "刷新";
        _refreshPortsButton.Location = new Point(154, 12);
        _refreshPortsButton.Size = new Size(54, 28);
        _refreshPortsButton.Click += (_, _) => RefreshPorts();
        top.Controls.Add(_refreshPortsButton);

        AddLabel(top, "波特率", 220, 16);
        _baudCombo.Location = new Point(268, 13);
        _baudCombo.Width = 96;
        _baudCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _baudCombo.Items.AddRange(new object[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 });
        _baudCombo.SelectedIndex = 7;
        top.Controls.Add(_baudCombo);

        AddLabel(top, "数据位", 374, 16);
        _dataBitsCombo.Location = new Point(422, 13);
        _dataBitsCombo.Width = 56;
        _dataBitsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _dataBitsCombo.Items.AddRange(new object[] { 5, 6, 7, 8 });
        _dataBitsCombo.SelectedIndex = 3;
        top.Controls.Add(_dataBitsCombo);

        AddLabel(top, "校验", 486, 16);
        _parityCombo.Location = new Point(522, 13);
        _parityCombo.Width = 72;
        _parityCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _parityCombo.Items.AddRange(new object[] { "None", "Even", "Odd", "Mark", "Space" });
        _parityCombo.SelectedIndex = 0;
        top.Controls.Add(_parityCombo);

        AddLabel(top, "停止位", 602, 16);
        _stopBitsCombo.Location = new Point(650, 13);
        _stopBitsCombo.Width = 60;
        _stopBitsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _stopBitsCombo.Items.AddRange(new object[] { "1", "1.5", "2" });
        _stopBitsCombo.SelectedIndex = 0;
        top.Controls.Add(_stopBitsCombo);

        _openPortButton.Text = "打开串口";
        _openPortButton.Location = new Point(722, 12);
        _openPortButton.Size = new Size(92, 30);
        _openPortButton.Click += (_, _) => TogglePort();
        top.Controls.Add(_openPortButton);

        _serialStatus.AutoSize = true;
        _serialStatus.Location = new Point(12, 54);
        _serialStatus.Text = "未连接";
        top.Controls.Add(_serialStatus);

        Panel sendPanel = new Panel { Dock = DockStyle.Bottom, Height = 92 };
        _sendBox.Location = new Point(12, 10);
        _sendBox.Size = new Size(650, 26);
        _sendBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendData();
            }
        };
        sendPanel.Controls.Add(_sendBox);

        _sendButton.Text = "发送";
        _sendButton.Location = new Point(672, 9);
        _sendButton.Size = new Size(72, 28);
        _sendButton.Click += (_, _) => SendData();
        sendPanel.Controls.Add(_sendButton);

        _hexSendCheck.Text = "HEX 发送";
        _hexSendCheck.AutoSize = true;
        _hexSendCheck.Location = new Point(12, 46);
        sendPanel.Controls.Add(_hexSendCheck);

        _clearReceiveButton.Text = "清空接收";
        _clearReceiveButton.Location = new Point(110, 42);
        _clearReceiveButton.Size = new Size(86, 28);
        _clearReceiveButton.Click += (_, _) => _receiveBox.Clear();
        sendPanel.Controls.Add(_clearReceiveButton);

        Label sendHint = new Label
        {
            AutoSize = true,
            Location = new Point(210, 48),
            Text = "接收区只读；文本按 UTF-8 发送，勾选 HEX 后按十六进制字节发送（如 01 03 00 00 00 01）。"
        };
        sendPanel.Controls.Add(sendHint);

        _receiveBox.Dock = DockStyle.Fill;
        _receiveBox.Multiline = true;
        _receiveBox.ReadOnly = true;
        _receiveBox.ScrollBars = ScrollBars.Vertical;
        _receiveBox.Font = new Font("Consolas", 10f);
        _receiveBox.BackColor = Color.White;

        page.Controls.Add(_receiveBox);
        page.Controls.Add(sendPanel);
        page.Controls.Add(top);
        return page;
    }

    private static Label AddLabel(Control parent, string text, int x, int y)
    {
        Label label = new Label { Text = text, Location = new Point(x, y), AutoSize = true };
        parent.Controls.Add(label);
        return label;
    }

    private void FullScreenCapture()
    {
        try
        {
            using Bitmap bmp = ScreenCapture.CaptureAllScreens();
            SaveAndShow(bmp);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "截图失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RegionCapture()
    {
        using ScreenshotForm form = new ScreenshotForm();
        if (form.ShowDialog(this) == DialogResult.OK && form.Captured != null)
            SaveAndShow(form.Captured);
    }

    private void SaveAndShow(Bitmap bmp)
    {
        string dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(dir);
        string file = Path.Combine(dir, "screenshot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png");
        bmp.Save(file, ImageFormat.Png);
        try { Clipboard.SetImage(bmp); } catch { }

        Image? old = _preview.Image;
        _preview.Image = new Bitmap(bmp);
        old?.Dispose();
        _captureStatus.Text = "已保存：" + file + "（已复制到剪贴板）";
    }

    private void PickColor()
    {
        using ColorPickerForm picker = new ColorPickerForm();
        if (picker.ShowDialog(this) == DialogResult.OK)
        {
            _swatch.BackColor = picker.PickedColor;
            _hexLabel.Text = "HEX: " + ToHex(picker.PickedColor);
            _rgbLabel.Text = "RGB: " + picker.PickedColor.R + ", " + picker.PickedColor.G + ", " + picker.PickedColor.B;
            try { Clipboard.SetText(ToHex(picker.PickedColor)); } catch { }
        }
    }

    private static string ToHex(Color c) => "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");

    private void RefreshPorts()
    {
        string? current = _portCombo.SelectedItem as string;
        _portCombo.Items.Clear();
        string[] ports;
        try { ports = SerialPort.GetPortNames(); }
        catch { ports = Array.Empty<string>(); }
        _portCombo.Items.AddRange(ports);

        if (current != null && _portCombo.Items.Contains(current))
            _portCombo.SelectedItem = current;
        else if (_portCombo.Items.Count > 0)
            _portCombo.SelectedIndex = 0;
    }

    private void TogglePort()
    {
        if (_port is { IsOpen: true })
        {
            ClosePort();
            return;
        }

        if (_portCombo.SelectedItem is not string portName || string.IsNullOrEmpty(portName))
        {
            MessageBox.Show(this, "请先选择串口。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int baud = Convert.ToInt32(_baudCombo.SelectedItem ?? 115200);
        int dataBits = Convert.ToInt32(_dataBitsCombo.SelectedItem ?? 8);
        Parity parity = _parityCombo.SelectedItem switch
        {
            "Even" => Parity.Even,
            "Odd" => Parity.Odd,
            "Mark" => Parity.Mark,
            "Space" => Parity.Space,
            _ => Parity.None
        };
        StopBits stopBits = _stopBitsCombo.SelectedItem switch
        {
            "1.5" => StopBits.OnePointFive,
            "2" => StopBits.Two,
            _ => StopBits.One
        };

        try
        {
            SerialPort port = new SerialPort(portName, baud, parity, dataBits, stopBits)
            {
                Encoding = Encoding.UTF8,
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            port.DataReceived += OnDataReceived;
            port.Open();

            _port = port;
            SetPortControlsEnabled(false);
            _openPortButton.Text = "关闭串口";
            _serialStatus.Text = "已连接 " + portName + " @ " + baud;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "打开串口失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClosePort()
    {
        if (_port == null)
            return;

        try
        {
            if (_port.IsOpen)
                _port.Close();
        }
        catch { }

        _port.DataReceived -= OnDataReceived;
        _port.Dispose();
        _port = null;

        SetPortControlsEnabled(true);
        _openPortButton.Text = "打开串口";
        _serialStatus.Text = "未连接";
    }

    private void SetPortControlsEnabled(bool enabled)
    {
        _portCombo.Enabled = enabled;
        _refreshPortsButton.Enabled = enabled;
        _baudCombo.Enabled = enabled;
        _dataBitsCombo.Enabled = enabled;
        _parityCombo.Enabled = enabled;
        _stopBitsCombo.Enabled = enabled;
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (sender is not SerialPort port)
            return;

        string data;
        try { data = port.ReadExisting(); }
        catch { return; }

        if (string.IsNullOrEmpty(data))
            return;

        AppendReceive(data);
    }

    private void AppendReceive(string text)
    {
        if (_receiveBox.IsDisposed)
            return;

        if (_receiveBox.InvokeRequired)
        {
            _receiveBox.BeginInvoke(() => AppendReceive(text));
            return;
        }

        _receiveBox.AppendText(text);
    }

    private void SendData()
    {
        if (_port is not { IsOpen: true })
        {
            MessageBox.Show(this, "串口未打开。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string text = _sendBox.Text;
        if (string.IsNullOrEmpty(text))
            return;

        try
        {
            if (_hexSendCheck.Checked)
            {
                byte[] bytes = ParseHex(text);
                _port.Write(bytes, 0, bytes.Length);
            }
            else
            {
                _port.Write(text);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "发送失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static byte[] ParseHex(string hex)
    {
        string cleaned = hex.Replace(" ", "").Replace("\r", "").Replace("\n", "");
        if (cleaned.Length == 0 || cleaned.Length % 2 != 0)
            throw new FormatException("HEX 字符串长度必须为非零偶数。");

        byte[] result = new byte[cleaned.Length / 2];
        for (int i = 0; i < result.Length; i++)
            result[i] = Convert.ToByte(cleaned.Substring(i * 2, 2), 16);
        return result;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        ClosePort();
        base.OnFormClosing(e);
    }
}
