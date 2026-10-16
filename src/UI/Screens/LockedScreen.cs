namespace VamTimeline
{
    public class LockedScreen : ScreenBase
    {
        public const string ScreenName = "Locked";

        public override string screenId => ScreenName;

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            InitExplanation();
        }

        private void InitExplanation()
        {
            string text;
            if(animationEditContext.locked)
                text = @"请启用「编辑模式」以修改此动画";
            else if (!plugin.isActiveAndEnabled)
                text = "请同时启用 Timeline 插件及其所在人物,才能修改此动画";
            else
                text = "无法识别锁定原因";

            var textJSON = new JSONStorableString("帮助", $@"
<b>已锁定</b>

{text}
");
            var textUI = prefabFactory.CreateTextField(textJSON);
            textUI.height = 350f;
        }
    }
}

