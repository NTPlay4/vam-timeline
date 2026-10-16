using System.Text.RegularExpressions;

namespace VamTimeline
{
    public class LoggingScreen : ScreenBase
    {
        public const string ScreenName = "Logging";

        public override string screenId => ScreenName;

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            CreateChangeScreenButton("<b><</b> <i>返回</i>", MoreScreen.ScreenName);

            InitLoggingUI();
        }

        private void InitLoggingUI()
        {
            prefabFactory.CreateHeader("快捷链接", 1);

            var toggleAllJSON = new JSONStorableBool("全部切换", false)
            {
                valNoCallback = plugin.logger.clearOnPlay || plugin.logger.general || plugin.logger.triggersReceived || plugin.logger.sequencing || plugin.logger.peersSync || plugin.logger.triggersInvoked
            };
            prefabFactory.CreateToggle(toggleAllJSON);

            prefabFactory.CreateHeader("日志包含项", 1);

            var generalJSON = new JSONStorableBool("常规", false, val => plugin.logger.general = val) { valNoCallback = plugin.logger.general };
            prefabFactory.CreateToggle(generalJSON);

            var triggersReceivedJSON = new JSONStorableBool("触发器(接收)", false, val => plugin.logger.triggersReceived = val) { valNoCallback = plugin.logger.triggersReceived };
            prefabFactory.CreateToggle(triggersReceivedJSON);

            var triggersInvokedJSON = new JSONStorableBool("触发器(发出)", false, val => plugin.logger.triggersInvoked = val) { valNoCallback = plugin.logger.triggersInvoked };
            prefabFactory.CreateToggle(triggersInvokedJSON);

            var sequencingJSON = new JSONStorableBool("序列", false, val => plugin.logger.sequencing= val) { valNoCallback = plugin.logger.sequencing};
            prefabFactory.CreateToggle(sequencingJSON);

            var peerSyncJSON = new JSONStorableBool("对等同步", false, val => plugin.logger.peersSync = val) { valNoCallback = plugin.logger.peersSync };
            prefabFactory.CreateToggle(peerSyncJSON);

            var filterJSON = new JSONStorableString("过滤", "", val =>
            {
                if (string.IsNullOrEmpty(val))
                {
                    plugin.logger.filter = null;
                    return;
                }
                var regex = new Regex(val, RegexOptions.Compiled);
                plugin.logger.filter = regex;
            }){valNoCallback = plugin.logger.filter?.ToString()};
            prefabFactory.CreateTextInput(filterJSON);

            prefabFactory.CreateSpacer();
            prefabFactory.CreateHeader("日志选项", 1);

            var clearOnPlayJSON = new JSONStorableBool("播放时清除", false, val => plugin.logger.clearOnPlay = val) { valNoCallback = plugin.logger.clearOnPlay };
            prefabFactory.CreateToggle(clearOnPlayJSON);

            var showCurrentAnimationJSON = new JSONStorableBool("在帮助文本中显示当前播放", false, val => plugin.logger.showPlayInfoInHelpText = val) { valNoCallback = plugin.logger.showPlayInfoInHelpText };
            prefabFactory.CreateToggle(showCurrentAnimationJSON);

            prefabFactory.CreateSpacer();
            prefabFactory.CreateHeader("同步到其他人物", 1);

            var syncOtherAtoms = prefabFactory.CreateButton("在所有人物上同步日志设置");
            syncOtherAtoms.button.onClick.AddListener(() => plugin.peers.SendLoggingSettings());

            toggleAllJSON.setCallbackFunction = val =>
            {
                clearOnPlayJSON.val = val;
                generalJSON.val = val;
                triggersInvokedJSON.val = val;
                triggersReceivedJSON.val = val;
                sequencingJSON.val = val;
                peerSyncJSON.val = val;
            };
        }
    }
}

