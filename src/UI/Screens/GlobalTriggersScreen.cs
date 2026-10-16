namespace VamTimeline
{
    public class GlobalTriggersScreen : ScreenBase
    {
        public const string ScreenName = "Global Triggers";

        public override string screenId => ScreenName;

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            CreateChangeScreenButton("<b><</b> <i>返回</i>", MoreScreen.ScreenName);

            prefabFactory.CreateButton("当片段列表变化时").button.onClick.AddListener(()=>
            {
                animation.clipListChangedTrigger.trigger.triggerActionsParent = popupParent;
                animation.clipListChangedTrigger.trigger.OpenTriggerActionsPanel();
            });
            prefabFactory.CreateButton("当播放状态变化时").button.onClick.AddListener(()=>
            {
                animation.isPlayingChangedTrigger.trigger.triggerActionsParent = popupParent;
                animation.isPlayingChangedTrigger.trigger.OpenTriggerActionsPanel();
            });
            prefabFactory.CreateButton("当当前动画变化时").button.onClick.AddListener(()=>
            {
                animation.currentAnimationChangedTrigger.trigger.triggerActionsParent = popupParent;
                animation.currentAnimationChangedTrigger.trigger.OpenTriggerActionsPanel();
            });
        }
    }
}

