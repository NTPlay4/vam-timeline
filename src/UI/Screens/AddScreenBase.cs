namespace VamTimeline
{
    public abstract class AddScreenBase : ScreenBase
    {
        private static bool _previousCreateInOtherAtoms;
        private static bool _previousAddAnother;

        protected JSONStorableString clipNameJSON;
        protected JSONStorableString layerNameJSON;
        protected JSONStorableString segmentNameJSON;
        protected JSONStorableBool createInOtherAtomsJSON;
        protected JSONStorableBool addAnotherJSON;
        protected JSONStorableStringChooser createPositionJSON;
        protected UIDynamicTextField clipNameUI;

        #region Init

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            // Right side

            CreateChangeScreenButton("<b><</b> <i>返回</i>", AddAnimationsScreen.ScreenName);
        }

        protected void InitNewClipNameUI()
        {
            clipNameJSON = new JSONStorableString("新动画名称", "", (string _) => OptionsUpdated());
            clipNameUI = prefabFactory.CreateTextInput(clipNameJSON);
        }

        protected void InitNewLayerNameUI(string label = "新层名称")
        {
            layerNameJSON = new JSONStorableString(label, "", (string _) => OptionsUpdated());
            prefabFactory.CreateTextInput(layerNameJSON);
        }

        protected void InitNewSegmentNameUI(string label = "新片段名称")
        {
            segmentNameJSON = new JSONStorableString(label, "", (string _) => OptionsUpdated());
            prefabFactory.CreateTextInput(segmentNameJSON);
        }

        protected void InitCreateInOtherAtomsUI(string label = "在其他人物上创建")
        {
            createInOtherAtomsJSON = new JSONStorableBool(label, _previousCreateInOtherAtoms, val => _previousCreateInOtherAtoms = val);
            prefabFactory.CreateToggle(createInOtherAtomsJSON);
        }

        protected void InitAddAnotherUI()
        {
            addAnotherJSON = new JSONStorableBool("停留在本界面", _previousAddAnother, val => _previousAddAnother = val);
            prefabFactory.CreateToggle(addAnotherJSON);
        }

        protected void InitNewPositionUI()
        {
            createPositionJSON = new JSONStorableStringChooser(
                "在位置添加",
                AddAnimationOperations.Positions.all,
                AddAnimationOperations.Positions.PositionNext,
                "在位置添加");
            prefabFactory.CreatePopup(createPositionJSON, false, true);
        }

        #endregion

        protected override void OnCurrentAnimationChanged(AtomAnimationEditContext.CurrentAnimationChangedEventArgs args)
        {
            base.OnCurrentAnimationChanged(args);

            RefreshUI();
        }

        protected virtual void RefreshUI()
        {
            OptionsUpdated();
        }

        protected abstract void OptionsUpdated();
    }
}
