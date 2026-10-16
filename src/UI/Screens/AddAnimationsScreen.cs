namespace VamTimeline
{
    public class AddAnimationsScreen : ScreenBase
    {
        public const string ScreenName = "添加动画";

        public override string screenId => ScreenName;

        #region Init

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            // Right side

            CreateChangeScreenButton("<b><</b> <i>返回动画列表</i>", AnimationsScreen.ScreenName);

            prefabFactory.CreateSpacer();
            prefabFactory.CreateHeader("Create", 1);

            CreateChangeScreenButton("<i>创建<b>动画</b>...</i>", AddClipScreen.ScreenName);
            CreateChangeScreenButton("<i>创建<b>层</b>...</i>", AddLayerScreen.ScreenName);
            if (animation.index.useSegment)
                CreateChangeScreenButton("<i>创建<b>片段</b>...</i>", AddSegmentScreen.ScreenName);
            else
                CreateChangeScreenButton("<i>使用<b>片段</b>...</i>", AddSegmentScreen.ScreenName);

            prefabFactory.CreateSpacer();
            prefabFactory.CreateHeader("More", 1);

            CreateChangeScreenButton("<i>从文件<b>导入</b>...</i>", ImportExportScreen.ScreenName);
            CreateChangeScreenButton("<i><b>管理 / 排序</b>动画...</i>", ManageAnimationsScreen.ScreenName);
        }

        #endregion
    }
}

