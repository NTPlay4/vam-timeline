using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace VamTimeline
{
    public class Editor : MonoBehaviour
    {
        private const float _rightPanelExpandedWidth = 500f;
        private const float _rightPanelCollapsedWidth = 0f;

        public static Editor AddTo(Transform transform)
        {
            var go = new GameObject();
            go.transform.SetParent(transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.StretchTop();
            rect.sizeDelta = new Vector2(0, 750);
            rect.pivot = new Vector2(0, 1);

            return Configure(go);
        }

        public static Editor Configure(GameObject go)
        {
            go.AddComponent<VerticalLayoutGroup>();

            var tabs = ScreenTabs.Create(go.transform, VamPrefabFactory.buttonPrefab);

            var panels = new GameObject();
            panels.transform.SetParent(go.transform, false);

            var panelsGroup = panels.AddComponent<HorizontalLayoutGroup>();
            panelsGroup.spacing = 10f;
            panelsGroup.childControlWidth = true;
            panelsGroup.childForceExpandWidth = false;
            panelsGroup.childControlHeight = true;
            panelsGroup.childForceExpandHeight = false;

            var leftPanel = CreatePanel(panels.transform, 0f, 1f);
            var rightPanel = CreatePanel(panels.transform, _rightPanelExpandedWidth, 0f);

            var editor = go.AddComponent<Editor>();
            editor.tabs = tabs;
            editor.leftPanel = leftPanel;
            editor.rightPanel = rightPanel;

            return editor;
        }

        private static GameObject CreatePanel(Transform transform, float preferredWidth, float flexibleWidth)
        {
            var go = new GameObject();
            go.transform.SetParent(transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.pivot = new Vector2(0, 1f);

            var layout = go.AddComponent<LayoutElement>();
            layout.minWidth = 0;
            layout.preferredWidth = preferredWidth;
            layout.flexibleWidth = flexibleWidth;
            layout.minHeight = 100;

            var verticalLayoutGroup = go.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childForceExpandWidth = false;
            verticalLayoutGroup.childControlHeight = true;
            verticalLayoutGroup.childForceExpandHeight = false;
            verticalLayoutGroup.spacing = 10f;

            return go;
        }

        public AtomAnimationEditContext animationEditContext;
        public ScreenTabs tabs;
        public GameObject leftPanel;
        public GameObject rightPanel;
        public ScreensManager screensManager;
        public Transform popupParent;
        private AnimationControlPanel _controlPanel;
        private IAtomPlugin _plugin;
        private bool _bound;
        private VamPrefabFactory _leftPanelPrefabFactory;
        private CurveTypePopup _curveType;
        private bool _expanded = true;
        private UIDynamicButton _expandButton;

        private JSONStorableBool _batchJSON;
        private UIDynamicToggle _batchToggle;
        private GameObject _singleModeContainer;
        private GameObject _batchModeContainer;
        private JSONStorableFloat _selectionStartJSON;
        private JSONStorableFloat _selectionEndJSON;

        public void Bind(IAtomPlugin plugin, string defaultScreen = null)
        {
            _plugin = plugin;

            _leftPanelPrefabFactory = leftPanel.AddComponent<VamPrefabFactory>();
            _leftPanelPrefabFactory.plugin = _plugin;

            tabs.Add(AnimationsScreen.ScreenName, new Color(0.780f, 0.780f, 0.899f), "动画");
            tabs.Add(AddAnimationsScreen.ScreenName, new Color(0.780f, 0.780f, 0.899f), "+", 60f);
            tabs.Add(TargetsScreen.ScreenName, new Color(0.780f, 0.899f, 0.780f), "目标");
            tabs.Add(AddRemoveTargetsScreen.ScreenName, new Color(0.780f, 0.899f, 0.780f), "+", 60f);
            tabs.Add(EditAnimationScreen.ScreenName, new Color(0.899f, 0.780f, 0.780f), "编辑");
            tabs.Add(SequencingScreen.ScreenName, new Color(0.899f, 0.780f, 0.780f), "序列");
            tabs.Add(PoseScreen.ScreenName, new Color(0.799f, 0.880f, 0.880f), "姿势");
            tabs.Add(MoreScreen.ScreenName, new Color(0.889f, 0.780f, 0.889f), "更多");
            _expandButton = tabs.Add(">", new Color(0.839f, 0.839f, 0.839f),null, preferredWidth: 60f);
            InitToggleRightPanelButton(_expandButton);
            tabs.onTabSelected.AddListener(screenName =>
            {
                screensManager.ChangeScreen(screenName, null);
                Expand(true);
            });

            screensManager = InitScreensManager(rightPanel);
            screensManager.onScreenChanged.AddListener(args => tabs.Select(args.screenName));
            screensManager.Bind(plugin, defaultScreen);
        }

        public void Bind(AtomAnimationEditContext ctx)
        {
            animationEditContext = ctx;

            if (gameObject.activeInHierarchy)
                OnEnable();
        }

        private void OnEnable()
        {
            if (_bound || animationEditContext == null || _plugin == null) return;
            _bound = true;

            _controlPanel = CreateControlPanel(leftPanel);

            _controlPanel.Bind(_plugin);
            _controlPanel.Bind(animationEditContext);

            _batchJSON = new JSONStorableBool("批量", false, ApplyBatchMode);

            InitModeContainers();
            InitClipboardUI();

            InitChangeCurveTypeUI();
            InitBatchSelectionUI();

            _curveType.Bind(animationEditContext);

            animationEditContext.onCurrentAnimationChanged.AddListener(OnCurrentAnimationChanged);

            _singleModeContainer.transform.SetAsLastSibling();
            _batchModeContainer.transform.SetAsLastSibling();
            ApplyBatchMode(false);
        }

        private static AnimationControlPanel CreateControlPanel(GameObject panel)
        {
            var go = new GameObject();
            go.transform.SetParent(panel.transform, false);

            var layout = go.AddComponent<LayoutElement>();
            layout.flexibleHeight = 100f;

            return AnimationControlPanel.Configure(go);
        }

        private void InitChangeCurveTypeUI()
        {
            _curveType = CurveTypePopup.Create(_leftPanelPrefabFactory);
            _curveType.curveTypeUI.gameObject.transform.SetParent(_singleModeContainer.transform, false);
        }

        private void InitClipboardUI()
        {
            var container = _leftPanelPrefabFactory.CreateSpacer();
            container.height = 60f;

            var group = container.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.spacing = 4f;
            group.childControlWidth = true;
            group.childForceExpandWidth = false;
            group.childControlHeight = false;

            _batchToggle = _leftPanelPrefabFactory.CreateToggle(_batchJSON);
            _batchToggle.gameObject.transform.SetParent(group.transform, false);
            _batchToggle.gameObject.transform.SetAsFirstSibling();
            StretchAcrossRow(_batchToggle.gameObject, 80f);

            {
                var btn = Instantiate(_plugin.manager.configurableButtonPrefab).GetComponent<UIDynamicButton>();
                btn.gameObject.transform.SetParent(group.transform, false);
                btn.label = "删除";
                btn.button.onClick.AddListener(OnDeleteClicked);
                StretchAcrossRow(btn.gameObject, 100f);
            }

            {
                var btn = Instantiate(_plugin.manager.configurableButtonPrefab).GetComponent<UIDynamicButton>();
                btn.gameObject.transform.SetParent(group.transform, false);
                btn.label = "剪切";
                btn.button.onClick.AddListener(OnCutClicked);
                StretchAcrossRow(btn.gameObject, 100f);
            }

            {
                var btn = Instantiate(_plugin.manager.configurableButtonPrefab).GetComponent<UIDynamicButton>();
                btn.gameObject.transform.SetParent(group.transform, false);
                btn.label = "复制";
                btn.button.onClick.AddListener(OnCopyClicked);
                StretchAcrossRow(btn.gameObject, 100f);
            }

            {
                var btn = Instantiate(_plugin.manager.configurableButtonPrefab).GetComponent<UIDynamicButton>();
                btn.gameObject.transform.SetParent(group.transform, false);
                btn.label = "粘贴";
                btn.button.onClick.AddListener(OnPasteClicked);
                StretchAcrossRow(btn.gameObject, 100f);
            }
        }

        // Let the layout share the available width instead of using the prefab's fixed width,
        // so the row shrinks with the panel instead of overflowing it.
        private static void StretchAcrossRow(GameObject go, float flexibleWidth)
        {
            var layout = go.GetComponent<LayoutElement>();
            if (layout == null) layout = go.AddComponent<LayoutElement>();
            layout.minWidth = 0;
            layout.preferredWidth = 0;
            layout.flexibleWidth = flexibleWidth;
        }

        private void InitModeContainers()
        {
            _singleModeContainer = CreateModeContainer("TimelineSingleMode");
            _batchModeContainer = CreateModeContainer("TimelineBatchMode");
        }

        private GameObject CreateModeContainer(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(leftPanel.transform, false);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            return go;
        }

        private void InitBatchSelectionUI()
        {
            var length = animationEditContext.current.animationLength;

            _selectionStartJSON = new JSONStorableFloat("选区开始于", 0f, val =>
            {
                var closest = animationEditContext.GetAllOrSelectedTargets().Select(t => t.GetTimeClosestTo(val)).OrderBy(t => Mathf.Abs(val - t)).FirstOrDefault();
                _selectionStartJSON.valNoCallback = closest;
                if (_selectionStartJSON.val > _selectionEndJSON.val) _selectionEndJSON.valNoCallback = _selectionStartJSON.val;
            }, 0f, length);
            var startUI = _leftPanelPrefabFactory.CreateSlider(_selectionStartJSON);
            startUI.gameObject.transform.SetParent(_batchModeContainer.transform, false);

            _selectionEndJSON = new JSONStorableFloat("选区结束于", 0f, val =>
            {
                var closest = animationEditContext.GetAllOrSelectedTargets().Select(t => t.GetTimeClosestTo(val)).OrderBy(t => Mathf.Abs(val - t)).FirstOrDefault();
                _selectionEndJSON.valNoCallback = closest == 0 ? animationEditContext.current.animationLength : closest;
                if (_selectionEndJSON.val < _selectionStartJSON.val) _selectionStartJSON.valNoCallback = _selectionEndJSON.val;
            }, 0f, length);
            var endUI = _leftPanelPrefabFactory.CreateSlider(_selectionEndJSON);
            endUI.gameObject.transform.SetParent(_batchModeContainer.transform, false);

            var row = new GameObject("TimelineBatchRangeRow");
            row.transform.SetParent(_batchModeContainer.transform, false);
            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 4f;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childControlHeight = false;

            var visibleRangeUI = _leftPanelPrefabFactory.CreateButton("选择可见范围");
            visibleRangeUI.gameObject.transform.SetParent(row.transform, false);
            visibleRangeUI.button.onClick.AddListener(() =>
            {
                _selectionStartJSON.val = animationEditContext.scrubberRange.rangeBegin;
                _selectionEndJSON.val = animationEditContext.scrubberRange.rangeBegin + animationEditContext.scrubberRange.rangeDuration;
            });
            StretchAcrossRow(visibleRangeUI.gameObject, 100f);

            var startAtCurrentUI = _leftPanelPrefabFactory.CreateButton("在当前时间开始");
            startAtCurrentUI.gameObject.transform.SetParent(row.transform, false);
            startAtCurrentUI.button.onClick.AddListener(() => _selectionStartJSON.val = animationEditContext.clipTime);
            StretchAcrossRow(startAtCurrentUI.gameObject, 100f);

            var endAtCurrentUI = _leftPanelPrefabFactory.CreateButton("在当前时间结束");
            endAtCurrentUI.gameObject.transform.SetParent(row.transform, false);
            endAtCurrentUI.button.onClick.AddListener(() => _selectionEndJSON.val = animationEditContext.clipTime);
            StretchAcrossRow(endAtCurrentUI.gameObject, 100f);

            _selectionStartJSON.valNoCallback = 0f;
            _selectionEndJSON.valNoCallback = length;
        }

        private void ApplyBatchMode(bool batch)
        {
            if (_singleModeContainer != null) _singleModeContainer.SetActive(!batch);
            if (_batchModeContainer != null) _batchModeContainer.SetActive(batch);
        }

        private void OnDeleteClicked()
        {
            if (_batchJSON.val)
                animationEditContext.DeleteRange(_selectionStartJSON.val, _selectionEndJSON.val);
            else
                _plugin.deleteJSON.actionCallback();
        }

        private void OnCutClicked()
        {
            if (_batchJSON.val)
                animationEditContext.CutRange(_selectionStartJSON.val, _selectionEndJSON.val);
            else
                _plugin.cutJSON.actionCallback();
        }

        private void OnCopyClicked()
        {
            if (_batchJSON.val)
                animationEditContext.CopyRange(_selectionStartJSON.val, _selectionEndJSON.val);
            else
                _plugin.copyJSON.actionCallback();
        }

        private void OnPasteClicked()
        {
            _plugin.pasteJSON.actionCallback();
        }

        private void OnCurrentAnimationChanged(AtomAnimationEditContext.CurrentAnimationChangedEventArgs args)
        {
            if (_selectionStartJSON == null || _selectionEndJSON == null) return;

            var length = args.after.animationLength;
            _selectionStartJSON.max = length;
            _selectionEndJSON.max = length;

            if (length < _selectionStartJSON.valNoCallback) _selectionStartJSON.valNoCallback = 0f;
            if (length < _selectionEndJSON.valNoCallback) _selectionEndJSON.valNoCallback = length;
            if (_selectionStartJSON.valNoCallback > _selectionEndJSON.valNoCallback) _selectionStartJSON.valNoCallback = _selectionEndJSON.valNoCallback;
        }

        public void OnDestroy()
        {
            if (animationEditContext != null)
                animationEditContext.onCurrentAnimationChanged.RemoveListener(OnCurrentAnimationChanged);
        }

        private void InitToggleRightPanelButton(UIDynamicButton btn)
        {
            btn.button.onClick.RemoveAllListeners();
            btn.button.onClick.AddListener(() => Expand(!_expanded));
        }

        private void Expand(bool open)
        {
            if (!open && _expanded)
            {
                _expanded = false;
                screensManager.enabled = false;
                rightPanel.GetComponent<LayoutElement>().preferredWidth = _rightPanelCollapsedWidth;
                _expandButton.label = "<";
            }
            else if (open && !_expanded)
            {
                _expanded = true;
                screensManager.enabled = true;
                rightPanel.GetComponent<LayoutElement>().preferredWidth = _rightPanelExpandedWidth;
                _expandButton.label = ">";
            }
        }

        private ScreensManager InitScreensManager(GameObject panel)
        {
            var go = new GameObject();
            go.transform.SetParent(panel.transform, false);

            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = 1138f;
            layout.flexibleWidth = 1;

            var sm = ScreensManager.Configure(go);
            sm.popupParent = popupParent;
            return sm;
        }

        public void ToggleExpandCollapse()
        {
            Expand(!_expanded);
        }

        public void ToggleDopeSheetMode()
        {
            _controlPanel.ToggleDopeSheetMode();
        }
    }
}
