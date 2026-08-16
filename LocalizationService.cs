using System.Windows;

namespace StarLead;

public static class LocalizationService
{
    private static readonly Dictionary<string, (string Zh, string En)> Strings = new()
    {
        ["AppName"] = ("星引", "StarLead"),
        ["MainSubtitle"] = ("单击启动 · 拖动整理 · 随时呼出", "Click to launch · Drag to arrange · Open anytime"),
        ["Settings"] = ("设置", "Settings"),
        ["NameOn"] = ("名称：开", "Names: On"), ["NameOff"] = ("名称：关", "Names: Off"),
        ["PinOn"] = ("置顶：开", "Pin: On"), ["PinOff"] = ("置顶：关", "Pin: Off"),
        ["MainHint"] = ("单击图标启动 · 拖动键位不会误开 · 按对应键直接启动 · Esc 关闭", "Click an icon to launch · Drag to arrange · Press its key to launch · Esc closes"),
        ["Open"] = ("打开", "Open"), ["Delete"] = ("删除", "Delete"),
        ["BindProgram"] = ("绑定程序", "Bind app"), ["BindFile"] = ("绑定文件", "Bind file"), ["BindFolder"] = ("绑定文件夹", "Bind folder"), ["BindNotebook"] = ("绑定笔记本", "Bind notebook"),
        ["RebindProgram"] = ("重新绑定为程序", "Rebind as app"), ["RebindFile"] = ("重新绑定为文件", "Rebind as file"), ["RebindFolder"] = ("重新绑定为文件夹", "Rebind as folder"), ["RebindNotebook"] = ("重新绑定为笔记本", "Rebind as notebook"), ["DeleteBinding"] = ("删除绑定", "Remove binding"),
        ["Notebook"] = ("笔记本", "Notebook"), ["CannotOpen"] = ("无法打开：", "Could not open: "),
        ["SettingsTitle"] = ("星引设置", "StarLead Settings"), ["SettingsSubtitle"] = ("调整星引的外观、唤起方式与键位显示", "Customize appearance, activation, and key visibility"),
        ["Language"] = ("语言", "Language"), ["Chinese"] = ("中文", "Chinese"), ["English"] = ("English", "English"), ["InstantApply"] = ("点击后立即生效，无需保存。", "Applies immediately; no save required."),
        ["Appearance"] = ("外观", "Appearance"), ["Light"] = ("浅色", "Light"), ["Dark"] = ("深色", "Dark"),
        ["PanelStyle"] = ("面板样式", "Panel style"), ["KeyboardPanel"] = ("键盘型", "Keyboard"), ["LinearPanel"] = ("一字型", "Linear"),
        ["BackgroundOpacity"] = ("背景透明度", "Background opacity"), ["OpaqueIcons"] = ("图标保持不透明", "icons stay opaque"),
        ["LinearLength"] = ("一字型长度", "Linear panel length"), ["LinearLengthHelp"] = ("图标较少时仍会按实际数量自动收紧。", "When there are fewer icons, the panel still fits the content."),
        ["MaxWidthPreview"] = ("最大 {0:0} px（拖动实时预览）", "Max {0:0} px (live preview)"),
        ["SystemComponents"] = ("系统组件", "System components"), ["SystemComponentsHelp"] = ("分别控制键盘型顶部的 Windows 系统图标。", "Control the Windows system icons shown above the keyboard."),
        ["MyComputer"] = ("我的电脑", "This PC"), ["Downloads"] = ("下载", "Downloads"), ["RecycleBin"] = ("回收站", "Recycle Bin"), ["NotebookBoth"] = ("笔记本（两个界面）", "Notebook (both panels)"), ["EmptyRecycleBin"] = ("清空回收站", "Empty Recycle Bin"), ["EmptyRecycleBinFailed"] = ("无法清空回收站：", "Could not empty Recycle Bin: "),
        ["IconContent"] = ("图标内容", "Icon content"), ["ShowIconNames"] = ("显示图标名称", "Show icon names"), ["ShowNamesHelp"] = ("同时控制键盘型和一字型；关闭后键位或顺序数字仍会保留。", "Applies to both panels; key and order badges remain visible."),
        ["ActivationHotkey"] = ("呼出快捷键", "Activation hotkey"), ["HotkeyHelp"] = ("修饰键可选择“无”，支持 `、-、=、[、] 等符号键单独呼出。", "Choose no modifier to use symbol keys such as `, -, =, [, or ] alone."), ["None"] = ("无", "None"),
        ["System"] = ("系统", "System"), ["Startup"] = ("登录 Windows 后自动启动星引", "Start StarLead when signing in to Windows"),
        ["PanelPosition"] = ("面板位置", "Panel position"), ["PanelPositionHelp"] = ("也可以直接拖动动作面板顶部改变位置。", "You can also drag any empty panel area to move it."), ["RememberPosition"] = ("记住上次位置", "Remember last position"), ["AlwaysCenter"] = ("每次居中", "Always center"),
        ["VisibleKeys"] = ("显示的键位", "Visible keys"), ["VisibleKeysHelp"] = ("取消勾选后，该键位会从动作面板隐藏，已有绑定不会丢失。", "Hidden keys keep their existing bindings."),
        ["Cancel"] = ("取消", "Cancel"), ["SaveSettings"] = ("保存设置", "Save settings"),
        ["NotebookTitle"] = ("星引笔记本", "StarLead Notebook"), ["NotebookSubtitle"] = ("双击空白处新建 · 拖动后自动吸附网格", "Double-click empty space to create · Cards snap to the grid"),
        ["FreeLayout"] = ("自由排列", "Free layout"), ["AutoLayout"] = ("自动排列", "Auto layout"), ["Arrange"] = ("一键整理", "Arrange"), ["New"] = ("＋ 新建", "+ New"), ["EmptyNotebook"] = ("双击这里，创建第一张笔记卡片", "Double-click here to create your first note"),
        ["SelectedCount"] = ("已选择 {0} 张", "{0} selected"), ["MultiSelectHint"] = ("Ctrl + 单击多选", "Ctrl + click to multi-select"),
        ["Edit"] = ("编辑", "Edit"), ["CopyText"] = ("复制文字", "Copy text"), ["Pin"] = ("置顶", "Pin"), ["Unpin"] = ("取消置顶", "Unpin"), ["SetColor"] = ("设置颜色", "Set color"),
        ["DefaultColor"] = ("默认", "Default"), ["Yellow"] = ("黄色", "Yellow"), ["Blue"] = ("蓝色", "Blue"), ["Green"] = ("绿色", "Green"), ["Pink"] = ("粉色", "Pink"), ["Purple"] = ("紫色", "Purple"),
        ["DeleteNoteQuestion"] = ("删除笔记“{0}”？", "Delete note \"{0}\"?"), ["NewNote"] = ("新笔记", "New note"), ["Untitled"] = ("无标题", "Untitled"),
        ["EditNote"] = ("编辑笔记", "Edit note"), ["AutosaveLocal"] = ("内容自动保存到本机", "Saved automatically on this device"), ["Done"] = ("完成", "Done"),
        ["TrayOpen"] = ("打开动作面板", "Open action panel"), ["TrayExit"] = ("退出星引", "Exit StarLead")
        , ["WidthAuto"] = ("宽度：自适应", "Width: adaptive"), ["WidthCustom"] = ("宽度：自定义（拖动左右边缘）", "Width: custom (drag either edge)"),
        ["IconSpacing"] = ("图标间距：{0:0} px", "Icon spacing: {0:0} px"), ["CannotOpenDownloads"] = ("无法打开下载：", "Could not open Downloads: "),
        ["VisualStyle"] = ("界面风格", "Interface style"), ["LiquidGlass"] = ("液态玻璃", "Liquid Glass"), ["Ocean"] = ("海湾蓝", "Ocean Blue"), ["Aurora"] = ("极光", "Aurora"), ["Graphite"] = ("石墨", "Graphite"),
        ["VisualStyleHelp"] = ("四套配色均支持浅色和深色，点击即时预览。", "All four styles support light and dark modes with instant preview.")
    };

    public static bool IsEnglish => App.Data?.Settings.Language == "en-US";
    public static string Get(string key) => Strings.TryGetValue(key, out var value) ? (IsEnglish ? value.En : value.Zh) : key;
    public static string Format(string key, params object[] args) => string.Format(Get(key), args);

    public static void Apply(string language)
    {
        bool english = language == "en-US";
        foreach (var pair in Strings)
            Application.Current.Resources[pair.Key] = english ? pair.Value.En : pair.Value.Zh;
    }
}
