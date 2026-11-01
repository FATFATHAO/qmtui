using System.Collections.ObjectModel;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace QmTui.UI;

/// <summary>
/// 快捷键与鼠标操作总览（只读），由全局 ? / F1 键唤起
/// </summary>
/// <remarks>
/// 文案只描述"实际生效"的按键：HandleGlobalKeyDownAsync 先于各视图 handler 执行并吞掉按键，
/// 因此部分视图级键位分支实际不会触发，此处一律按全局分发的真实行为书写。
/// shortcut: 静态文案与代码无绑定关系, 新增或修改快捷键时必须同步下面的 KeysHelpLines;
/// 若键位继续增长再考虑抽成集中式键位表。
/// </remarks>
public sealed class HelpDialog : Dialog
{
    private const int KeyColumnWidth = 13;

    internal static readonly (string Group, (string Key, string Desc)[] Rows)[] KeysHelpLines =
    [
        ("全局（任何界面均可用）",
        [
            ("Space", "播放 / 暂停"),
            ("Tab / S-Tab", "切焦点：侧栏→列表→歌词→控制栏"),
            ("V", "全屏播放界面"),
            ("P", "沉浸模式"),
            ("O", "播放模式：列表循环 / 单曲 / 随机"),
            ("M", "静音"),
            ("- / +", "音量 ±5"),
            ("S", "收藏 / 取消收藏当前曲"),
            ("A", "把当前曲加入歌单"),
            ("C", "评论面板"),
            ("T", "歌词译文开关（有译文时才生效）"),
            ("Y", "匹配歌词 / 再按撤销"),
            ("G", "当前列表内即时查找"),
            ("J / L", "上一首 / 下一首"),
            ("K", "一起听房间"),
            ("U", "登录"),
            ("R", "听歌识曲"),
            ("W", "Web 遥控"),
            ("H", "主题：自动→亮→暗"),
            ("B", "桌面切歌通知开关"),
            ("/  F3  C-V", "唤起搜索（Ctrl+V 顺带粘贴剪贴板）"),
            ("Q", "退出（二次确认）"),
            ("Esc", "逐级返回：弹窗→评论→沉浸→详情→AOD"),
        ]),
        ("主列表",
        [
            ("Enter", "播放选中曲；焦点在歌手/专辑列时进其详情页"),
            ("← / →", "在 歌名 / 歌手 / 专辑 三列间移动"),
            ("↑ / ↓", "移动光标，到底自动加载下一页"),
            ("PgUp / PgDn", "列表翻页"),
            ("Home", "滚回歌曲列表顶部"),
            ("N", "把选中曲插队为「下一首」播放"),
            ("D / Del", "从当前列表移除（歌单=移出歌单，最近播放=删历史）"),
            ("1 / 2 / 3", "等同右上角第 1/2/3 个上下文按钮"),
        ]),
        ("全屏播放界面 (V)",
        [
            ("Esc / V", "退出播放界面"),
            ("E", "播放队列抽屉"),
            ("C", "评论"),
            ("T", "歌词译文"),
            ("A", "当前曲加入歌单"),
            ("Y", "匹配歌词"),
            ("U", "登录"),
            ("P", "沉浸模式（评论打开时不生效）"),
            ("↑↓←→ / Pg / Home / End", "评论打开时用于翻页"),
            ("Enter", "选中评论生效（艺人链接需双击）"),
        ]),
        ("播放队列抽屉 (E)",
        [
            ("Enter / 双击", "播放选中项"),
            ("↑ / ↓", "选择"),
            ("D / Del", "移除选中项"),
            ("C", "清空待播（保留当前曲）"),
            ("Esc / E", "关闭"),
        ]),
        ("听歌足迹",
        [
            ("1 / 2 / 3 / 4", "日 / 周 / 月 / 年"),
            ("← / →", "切换周期"),
            ("< > 或 , . 或 [ ]", "切换日期"),
            ("↓", "到底后再按 = 榜单→喜欢"),
            ("Shift+↑", "年视图：榜单回到喜欢"),
        ]),
        ("本地音乐 / WebDAV",
        [
            ("A", "本地音乐=添加目录；WebDAV=导入当前目录"),
            ("R", "本地音乐=重新扫描；WebDAV=刷新"),
            ("F", "管理扫描目录 / 管理 WebDAV 站点"),
            ("D", "本地音乐=删目录；WebDAV=目录树/平铺切换"),
            ("S", "WebDAV 嗅探元数据"),
            ("Backspace", "WebDAV 返回上级目录"),
        ]),
        ("各弹窗通用",
        [
            ("Esc", "关闭（音质/登录/艺人弹窗按 Q 同样关闭）"),
            ("N / D", "加入歌单弹窗：新建歌单 / 删除歌单"),
            ("Enter", "加入歌单弹窗：添加到选中歌单"),
            ("A", "再次按下收起加入歌单弹窗"),
            ("1 / 2 / 3", "登录弹窗：QQ / 微信 / 服务号"),
            ("T / W / R / B", "登录弹窗：扫码 / 网页 / 刷新二维码 / 浏览器打开"),
            ("K", "关闭一起听弹窗（输入框内除外）"),
            ("1 … 9", "选择艺人弹窗：按序号直达"),
        ]),
        ("鼠标（部分操作只有鼠标，无按键）",
        [
            ("单击 / 双击", "选中 / 播放（列表、队列、加入歌单同理）"),
            ("点击进度条", "直接 seek 到点击位置"),
            ("Enter 后 ← / →", "控制栏聚焦时进入进度调节，每次 ±10s"),
            ("音量按钮", "单击或中键静音，滚轮 ±5"),
            ("双击歌词行", "跳到该句时间；滚轮翻歌词"),
            ("点击歌曲信息行", "打开播放界面（迷你封面同理）"),
            ("点击面板标题", "侧栏/列表/歌词 之间移焦；点歌词标题=开评论"),
            ("点击底部 [C]", "开关评论（该位置右侧整段都是点击区）"),
            ("转存 / 分享 / 音质", "只能鼠标点击，没有对应按键"),
            ("任意键", "退出 AOD 后台模式（Q 为退出确认）"),
        ]),
    ];

    private static Scheme TransparentDialogScheme => MikuTheme.TransparentDialog;

    public HelpDialog()
    {
        Title = "快捷键与鼠标操作帮助";
        Width = 76;
        Height = 22;
        SetScheme(TransparentDialogScheme);

        var listView = new ListView
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(1),
            Height = Dim.Fill(2)
        };
        listView.SetScheme(TransparentDialogScheme);
        listView.KeyBindings.Remove(Key.Space);
        listView.SetSource(new ObservableCollection<string>(BuildDisplayLines()));
        Add(listView);

        var hintLabel = new Label
        {
            Text = "[↑↓/PgUp] 滚动   [Esc / ? / F1] 关闭",
            X = 2,
            Y = Pos.AnchorEnd(1)
        };
        hintLabel.SetScheme(new Scheme
        {
            Normal = new Terminal.Gui.Drawing.Attribute(MikuTheme.MikuTextMuted, Color.None)
        });
        Add(hintLabel);

        void HandleKeyAction(Key k)
        {
            if (!MainWindow.IsTerminalWindowFocused)
            {
                k.Handled = true;
                return;
            }

            bool isClose = k == Key.Esc || k == Key.F1 || k.AsRune.Value == '?';
            if (isClose)
            {
                k.Handled = true;
                Application.RequestStop(this);
            }
        }

        listView.KeyDown += (s, k) => HandleKeyAction(k);
        KeyDown += (s, k) =>
        {
            if (k.Handled) return;
            HandleKeyAction(k);
        };
    }

    internal static List<string> BuildDisplayLines()
    {
        var lines = new List<string>();
        foreach (var (group, rows) in KeysHelpLines)
        {
            lines.Add($"── {group} ──");
            foreach (var (key, desc) in rows)
            {
                lines.Add($" {key.PadRight(KeyColumnWidth)}{desc}");
            }
            lines.Add("");
        }

        return lines;
    }
}
