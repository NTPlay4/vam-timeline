using UnityEngine;

namespace VamTimeline
{
    public class MoreScreen : ScreenBase
    {
        public const string ScreenName = "More...";

        public override string screenId => ScreenName;

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            prefabFactory.CreateHeader("更多选项", 1);

            CreateChangeScreenButton("<b>导入 / 导出</b>动画...", ImportExportScreen.ScreenName);
            CreateChangeScreenButton("<b>转换</b>VaM 原生场景动画...", MocapScreen.ScreenName);
            CreateChangeScreenButton("<b>录制</b>动画...", RecordScreen.ScreenName);
            CreateChangeScreenButton("<b>精简</b>关键帧...", ReduceScreen.ScreenName);
            CreateChangeScreenButton("<b>平滑</b>关键帧...", SmoothScreen.ScreenName);

            prefabFactory.CreateSpacer();

            CreateChangeScreenButton("<b>批量</b>更改...", BulkScreen.ScreenName);
            CreateChangeScreenButton("<b>高级</b>关键帧工具...", AdvancedKeyframeToolsScreen.ScreenName);
            CreateChangeScreenButton("<b>分组</b>...", GroupingScreen.ScreenName);
            CreateChangeScreenButton("<b>全局触发器</b>...", GlobalTriggersScreen.ScreenName);

            prefabFactory.CreateSpacer();

            CreateChangeScreenButton("<b>诊断</b>与场景分析...", DiagnosticsScreen.ScreenName);
            CreateChangeScreenButton("<b>选项</b>...", OptionsScreen.ScreenName);
            CreateChangeScreenButton("<b>日志</b>...", LoggingScreen.ScreenName);
            CreateChangeScreenButton("<b>默认值</b>...", DefaultsScreen.ScreenName);

            prefabFactory.CreateSpacer();

            CreateChangeScreenButton("内置帮助", HelpScreen.ScreenName);
            var helpButton = prefabFactory.CreateButton("[浏览器] 在线帮助");
            helpButton.button.onClick.AddListener(() => Application.OpenURL("https://github.com/acidbubbles/vam-timeline/wiki"));

            var patreonBtn = prefabFactory.CreateButton("[浏览器] 在 Patreon 支持我!");
            patreonBtn.textColor = new Color(0.97647f, 0.40784f, 0.32941f);
            patreonBtn.buttonColor = Color.white;
            patreonBtn.button.onClick.AddListener(() => Application.OpenURL("https://www.patreon.com/acidbubbles"));
        }
    }
}

