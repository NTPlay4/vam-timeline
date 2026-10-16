using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace VamTimeline
{
    public class ReduceScreenSettings : TimelineSettings
    {
        public static readonly ReduceScreenSettings singleton = new ReduceScreenSettings();

        public readonly TimelineSetting<bool> removeFlatSections = new TimelineSetting<bool>("RemoveFlatSections", true);
        public readonly TimelineSetting<bool> simplifyKeyframes = new TimelineSetting<bool>("SimplifyKeyframes", true);
        public readonly TimelineSetting<float> minDistance = new TimelineSetting<float>("MinDistance", 0.008f);
        public readonly TimelineSetting<float> minRotation = new TimelineSetting<float>("MinRotation", 0.001f);
        public readonly TimelineSetting<float> minFloatRange = new TimelineSetting<float>("MinFloatRange", 0.01f);
        public readonly TimelineSetting<int> maxFPS = new TimelineSetting<int>("maxFPS", 10);
        public readonly TimelineSetting<bool> roundKeyTimeToFPS = new TimelineSetting<bool>("RoundKeyTimeToFPS", false);

        public override void Load(JSONClass json)
        {
            removeFlatSections.Load(json);
            simplifyKeyframes.Load(json);
            minDistance.Load(json);
            minRotation.Load(json);
            minFloatRange.Load(json);
            maxFPS.Load(json);
            roundKeyTimeToFPS.Load(json);
        }

        public override void Save(JSONClass json)
        {
            removeFlatSections.Save(json);
            simplifyKeyframes.Save(json);
            minDistance.Save(json);
            minRotation.Save(json);
            minFloatRange.Save(json);
            maxFPS.Save(json);
            roundKeyTimeToFPS.Save(json);
        }
    }

    public class ReduceScreen : ScreenBase
    {
        public const string ScreenName = "Reduce";

        private JSONStorableFloat _maxFramesPerSecondJSON;
        private JSONStorableBool _roundJSON;
        private JSONStorableBool _removeFlatSectionsKeyframes;
        private JSONStorableBool _simplifyKeyframes;
        private JSONStorableFloat _minDistanceJSON;
        private JSONStorableFloat _minRotationJSON;
        private JSONStorableFloat _minFloatParamRangeRatioJSON;

        public override string screenId => ScreenName;

        private UIDynamicButton _backupUI;
        private UIDynamicButton _restoreUI;
        private UIDynamicButton _reduceUI;

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            CreateChangeScreenButton("<b><</b> <i>返回</i>", MoreScreen.ScreenName);

            prefabFactory.CreateSpacer();

            _backupUI = prefabFactory.CreateButton("备份");
            _backupUI.button.onClick.AddListener(TakeBackup);
            _restoreUI = prefabFactory.CreateButton("恢复");
            _restoreUI.button.onClick.AddListener(RestoreBackup);

            _restoreUI.button.interactable = HasBackup(); if (HasBackup()) _restoreUI.label = $"Restore [{AtomAnimationBackup.singleton.backupTime}]";

            prefabFactory.CreateSpacer();

            CreateReduceSettingsUI();

            _reduceUI = prefabFactory.CreateButton("Reduce");
            _reduceUI.button.onClick.AddListener(Reduce);
            _reduceUI.buttonColor = Color.green;

            CreateChangeScreenButton("<i>前往<b>录制</b>界面...</i>", RecordScreen.ScreenName);

            animationEditContext.animation.animatables.onTargetsSelectionChanged.AddListener(OnTargetsSelectionChanged);
            OnTargetsSelectionChanged();
        }

        private void CreateReduceSettingsUI()
        {
            _roundJSON = new JSONStorableBool("关键帧时间取整到帧率", ReduceScreenSettings.singleton.roundKeyTimeToFPS.defaultValue)
            {
                valNoCallback = ReduceScreenSettings.singleton.roundKeyTimeToFPS.value,
                setCallbackFunction = val => ReduceScreenSettings.singleton.roundKeyTimeToFPS.value = val
            };

            _maxFramesPerSecondJSON = new JSONStorableFloat("每秒最大帧数", ReduceScreenSettings.singleton.maxFPS.defaultValue, 1f, 50f)
            {
                valNoCallback = ReduceScreenSettings.singleton.maxFPS.value,
                setCallbackFunction = val => _maxFramesPerSecondJSON.valNoCallback = ReduceScreenSettings.singleton.maxFPS.value = (int)Mathf.Round(val)
            };

            _removeFlatSectionsKeyframes = new JSONStorableBool("移除平坦区段", ReduceScreenSettings.singleton.removeFlatSections.defaultValue)
            {
                valNoCallback = ReduceScreenSettings.singleton.removeFlatSections.value,
                setCallbackFunction = val => ReduceScreenSettings.singleton.removeFlatSections.value = val
            };

            _simplifyKeyframes = new JSONStorableBool("简化关键帧", ReduceScreenSettings.singleton.removeFlatSections.defaultValue)
            {
                valNoCallback = ReduceScreenSettings.singleton.simplifyKeyframes.value,
                setCallbackFunction = val => ReduceScreenSettings.singleton.simplifyKeyframes.value = val
            };

            _minDistanceJSON = new JSONStorableFloat("最小有效距离", ReduceScreenSettings.singleton.minDistance.defaultValue, 0f, 1f, false)
            {
                valNoCallback = ReduceScreenSettings.singleton.minDistance.value,
                setCallbackFunction = val => ReduceScreenSettings.singleton.minDistance.value = val
            };

            _minRotationJSON = new JSONStorableFloat("最小有效旋转(点积)", ReduceScreenSettings.singleton.minRotation.defaultValue, 0f, 1f)
            {
                valNoCallback = ReduceScreenSettings.singleton.minRotation.value,
                setCallbackFunction = val => ReduceScreenSettings.singleton.minRotation.value = val
            };

            _minFloatParamRangeRatioJSON = new JSONStorableFloat("最小有效浮点范围比", ReduceScreenSettings.singleton.minFloatRange.defaultValue, 0f, 1f)
            {
                valNoCallback = ReduceScreenSettings.singleton.minFloatRange.value,
                setCallbackFunction = val => ReduceScreenSettings.singleton.minFloatRange.value = val
            };

            prefabFactory.CreateToggle(_removeFlatSectionsKeyframes);
            prefabFactory.CreateSpacer();
            prefabFactory.CreateToggle(_simplifyKeyframes);
            prefabFactory.CreateSlider(_minDistanceJSON).valueFormat = "F4";
            prefabFactory.CreateSlider(_minRotationJSON).valueFormat = "F4";
            prefabFactory.CreateSlider(_minFloatParamRangeRatioJSON).valueFormat = "F4";
            prefabFactory.CreateSpacer();
            prefabFactory.CreateSlider(_maxFramesPerSecondJSON).valueFormat = "F1";
            prefabFactory.CreateToggle(_roundJSON);
            prefabFactory.CreateSpacer();
        }

        protected override void OnCurrentAnimationChanged(AtomAnimationEditContext.CurrentAnimationChangedEventArgs args)
        {
            base.OnCurrentAnimationChanged(args);
            _restoreUI.button.interactable = false;
        }

        private void Reduce()
        {
            if (!HasBackup())
                TakeBackup();
            else
                RestoreBackup();

            _reduceUI.button.interactable = false;
            _reduceUI.label = "请耐心等待...";
            var settings = new ReduceSettings
            {
                fps = (int)_maxFramesPerSecondJSON.val,
                round = _roundJSON.val,
                removeFlats = _removeFlatSectionsKeyframes.val,
                simplify = _simplifyKeyframes.val,
                minMeaningfulDistance = _minDistanceJSON.val,
                minMeaningfulRotation = _minRotationJSON.val,
                minMeaningfulFloatParamRangeRatio = _minFloatParamRangeRatioJSON.val,
            };
            StartCoroutine(operations.Reduce(settings).ReduceKeyframes(
                animationEditContext.current.GetAllCurveTargets().Where(t => t.selected).ToList(),
                progress =>
                {
                    _reduceUI.label = $"{progress.stepsDone} of {progress.stepsTotal} ({progress.timeLeft:0}s left)";
                },
                () =>
                {
                    _reduceUI.button.interactable = true;
                    _reduceUI.label = "Reduce";

                    animationEditContext.onKeyframesReduced.Invoke();
                }));
        }

        private void OnTargetsSelectionChanged()
        {
            _reduceUI.button.interactable = current.GetAllCurveTargets().Any(t => t.selected);
        }

        private bool HasBackup()
        {
            return AtomAnimationBackup.singleton.HasBackup(current);
        }

        private void TakeBackup()
        {
            AtomAnimationBackup.singleton.TakeBackup(current);
            _restoreUI.label = $"Restore [{AtomAnimationBackup.singleton.backupTime}]";
            _restoreUI.button.interactable = true;
        }

        private void RestoreBackup()
        {
            AtomAnimationBackup.singleton.RestoreBackup(current);
        }

        public override void OnDestroy()
        {
            animationEditContext.animation.animatables.onTargetsSelectionChanged.RemoveListener(OnTargetsSelectionChanged);
            base.OnDestroy();
        }
    }
}

