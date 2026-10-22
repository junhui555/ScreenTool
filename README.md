# ScreenTool — 屏幕工具箱

基于 C# / .NET 8 WinForms 的桌面小工具，集成三项功能：

- 截图：全屏截图、区域截图（拖拽选区），自动保存 PNG 并复制到剪贴板。
- 取色：屏幕取色（带放大镜与十字准星），显示 HEX / RGB，自动复制 HEX。
- 串口：串口连接，可配置端口 / 波特率 / 数据位 / 校验 / 停止位，支持文本与 HEX 发送、接收区显示。

## 运行

需要 .NET 8 SDK（已在本机确认可用）。

```powershell
cd ScreenTool
dotnet run
```

或发布为独立 exe：

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

截图默认保存到程序目录下的 `screenshots` 文件夹。

## 结构

- `Program.cs` — 入口
- `MainForm.cs` — 主界面与三个功能模块
- `ScreenshotForm.cs` — 区域截图遮罩窗体
- `ColorPickerForm.cs` — 取色放大镜窗体
- `ScreenCapture.cs` — 屏幕捕获工具类
