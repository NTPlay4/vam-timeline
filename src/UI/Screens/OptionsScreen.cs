using System.Collections.Generic;

namespace VamTimeline
{
    public class OptionsScreen : ScreenBase
    {
        public const string ScreenName = "Settings";
        private JSONStorableBool _lockedJSON;
        private JSONStorableBool _syncWithPeersJSON;
        private JSONStorableBool _syncSubsceneOnlyJSON;
        private JSONStorableFloat _snapJSON;
        private JSONStorableBool _autoKeyframeAllControllersJSON;
        private JSONStorableBool _showPaths;
        private JSONStorableBool _zoomDraggableJSON;
        private JSONStorableBool _ignoreSequencingJSON;
        private JSONStorableStringChooser _serializationModeJSON;

        public override string screenId => ScreenName;

        #region Init

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            // Right side

            CreateChangeScreenButton("<b><</b> <i>返回</i>", MoreScreen.ScreenName);

            InitLockedUI();

            prefabFactory.CreateHeader("播放选项", 1);

            InitUseRealTimeUI();
            InitLiveParenting();
            InitForceBlendInTime();

            InitDisableSync();
            InitIgnoreSequencing();
#if (VAM_GT_1_20)
            if (!ReferenceEquals(plugin.containingAtom.containingSubScene, null))
                InitSyncSubsceneOnly();
#endif

            prefabFactory.CreateHeader("编辑选项", 1);

            InitSnapUI();

            InitAutoKeyframeUI();

            prefabFactory.CreateHeader("界面选项", 1);

            InitShowPathsUI();
            InitZoomDraggableUI();

            prefabFactory.CreateHeader("序列化选项", 1);

            InitSerializationModeUI();

            animationEditContext.onEditorSettingsChanged.AddListener(OnEditorSettingsChanged);
        }

        private void InitLockedUI()
        {
            _lockedJSON = new JSONStorableBool("锁定(忽略游戏内编辑)", animationEditContext.locked, val => animationEditContext.locked = val);
            prefabFactory.CreateToggle(_lockedJSON);
        }

        private void InitUseRealTimeUI()
        {
            var timeModes = new List<string> {TimeModes.UnityTime.ToString(), TimeModes.RealTime.ToString()};
            var timeModeDisplay = new List<string>
            {
                "游戏时间(低帧率时变慢)",
                "实时(音频同步更好)"
            };
            if (animation.timeMode == TimeModes.RealTimeLegacy)
            {
              timeModes.Add(TimeModes.RealTimeLegacy.ToString());
              timeModeDisplay.Add("实时(旧版,仅用于老场景)");
            }
            var timeTypeJSON = new JSONStorableStringChooser("时间模式", timeModes, TimeModes.UnityTime.ToString(), "时间模式")
            {
                displayChoices = timeModeDisplay,
                valNoCallback = animation.timeMode.ToString(),
                setCallbackFunction = val => animation.timeMode = int.Parse(val)
            };
            prefabFactory.CreatePopup(timeTypeJSON, true, true);
        }

        private void InitForceBlendInTime()
        {
            var forceBlendTimeJSON = new JSONStorableBool("忽略混合速度", animation.forceBlendTime, val => animation.forceBlendTime = val);
            prefabFactory.CreateToggle(forceBlendTimeJSON);
        }

        private void InitLiveParenting()
        {
            var liveParentingJSON = new JSONStorableBool("常驻父子关系", animation.liveParenting, val => animation.liveParenting = val);
            prefabFactory.CreateToggle(liveParentingJSON);
        }

        private void InitDisableSync()
        {
            _syncWithPeersJSON = new JSONStorableBool("与其他人物同步", animation.syncWithPeers, val => animation.syncWithPeers = val);
            prefabFactory.CreateToggle(_syncWithPeersJSON);
        }

        private void InitSyncSubsceneOnly()
        {
            _syncSubsceneOnlyJSON = new JSONStorableBool("仅在子场景中发送同步", animation.syncSubsceneOnly, val => animation.syncSubsceneOnly = val);
            prefabFactory.CreateToggle(_syncSubsceneOnlyJSON);
        }

        private void InitSnapUI()
        {
            _snapJSON = new JSONStorableFloat("吸附", 0.001f, val => animationEditContext.snap = val.Snap(), 0.1f, 1f)
            {
                valNoCallback = animationEditContext.snap
            };
            var snapUI = prefabFactory.CreateSlider(_snapJSON);
            snapUI.valueFormat = "F3";
        }

        private void InitAutoKeyframeUI()
        {
            _autoKeyframeAllControllersJSON = new JSONStorableBool("一次性给所有控制器打关键帧", animationEditContext.autoKeyframeAllControllers, val => animationEditContext.autoKeyframeAllControllers = val);
            prefabFactory.CreateToggle(_autoKeyframeAllControllersJSON);
        }

        private void InitShowPathsUI()
        {
            _showPaths = new JSONStorableBool(
                "显示所选控制器的路径", animationEditContext.showPaths,
                val => animationEditContext.showPaths = val);
            prefabFactory.CreateToggle(_showPaths);
        }

        private void InitZoomDraggableUI()
        {
            _zoomDraggableJSON = new JSONStorableBool(
                "拖动时间轴两端(缩放)", animationEditContext.zoomDraggable,
                val => animationEditContext.zoomDraggable = val);
            prefabFactory.CreateToggle(_zoomDraggableJSON);
        }

        private void InitIgnoreSequencing()
        {
            _ignoreSequencingJSON = new JSONStorableBool("禁用序列", animation.pauseSequencing, val => animation.pauseSequencing = val);
            prefabFactory.CreateToggle(_ignoreSequencingJSON);
        }

        private void InitSerializationModeUI()
        {
            _serializationModeJSON = new JSONStorableStringChooser("SerializationMode",new List<string>
            {
                AtomAnimationSerializer.Modes.Full.ToString(),
                AtomAnimationSerializer.Modes.Readable.ToString(),
                AtomAnimationSerializer.Modes.Optimized.ToString(),
            }, "Optimized", "序列化模式")
            {
                displayChoices = new List<string>
                {
                    "完整(旧版 <7.*)",
                    "可读(便于编辑)",
                    "优化(更小文件)",
                },
                setCallbackFunction = val => animation.serializeMode = int.Parse(val),
                valNoCallback = animation.serializeMode.ToString()
            };
            prefabFactory.CreatePopup(_serializationModeJSON, false, false, upwards: true, popupPanelHeight: 180f);
        }

        #endregion

        private void OnEditorSettingsChanged(string _)
        {
            _lockedJSON.valNoCallback = animationEditContext.locked;
            _snapJSON.valNoCallback = animationEditContext.snap;
            _autoKeyframeAllControllersJSON.valNoCallback = animationEditContext.autoKeyframeAllControllers;
            _zoomDraggableJSON.valNoCallback = animationEditContext.zoomDraggable;
        }
    }
}

