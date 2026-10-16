using System.Linq;

namespace VamTimeline
{
    public class GroupingScreen : ScreenBase
    {
        public const string ScreenName = "Grouping";

        private JSONStorableString _assignGroupJSON;

        public override string screenId => ScreenName;

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            CreateChangeScreenButton("<b><</b> <i>返回</i>", MoreScreen.ScreenName);

            InitAssignToGroupUI();

            prefabFactory.CreateButton("分配给已选目标").button.onClick.AddListener(Assign);

            animation.animatables.onTargetsSelectionChanged.AddListener(OnTargetsSelectionChanged);
            OnTargetsSelectionChanged();
        }

        private void InitAssignToGroupUI()
        {
            _assignGroupJSON = new JSONStorableString("分组名称(留空自动)", "");
            prefabFactory.CreateTextInput(_assignGroupJSON);
        }

        private void OnTargetsSelectionChanged()
        {
            var groups = animationEditContext.GetSelectedTargets().Select(t => t.group).Distinct().ToList();
            if (groups.Count == 1) _assignGroupJSON.val = groups[0];
        }

        private void Assign()
        {
            var group = _assignGroupJSON.val;
            if (string.IsNullOrEmpty(group)) group = null;
            foreach (var target in animationEditContext.GetSelectedTargets())
            {
                foreach (var c in animationEditContext.currentLayer)
                {
                    var t = c.GetAllTargets().FirstOrDefault(x => x.TargetsSameAs(target));
                    if (t == null) continue;
                    t.group = group;
                }
            }
            current.onTargetsListChanged.Invoke();
        }

        public override void OnDestroy()
        {
            animation.animatables.onTargetsSelectionChanged.RemoveListener(OnTargetsSelectionChanged);
            base.OnDestroy();
        }
    }
}

