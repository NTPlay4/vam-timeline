using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace VamTimeline
{
    public class RecordScreenSettings : TimelineSettings
    {
        public static readonly RecordScreenSettings singleton = new RecordScreenSettings();

        public readonly TimelineSetting<int> delayTimer = new TimelineSetting<int>("DelayTimer", 5);
        public readonly TimelineSetting<bool> hideMenu = new TimelineSetting<bool>("HideMenu", false);
        public readonly TimelineSetting<bool> showStartMarkers = new TimelineSetting<bool>("ShowStartMarkers", true);
        public readonly TimelineSetting<int> timeMode = new TimelineSetting<int>("TimeMode", TimeModes.RealTime);

        public override void Load(JSONClass json)
        {
            delayTimer.Load(json);
            hideMenu.Load(json);
            showStartMarkers.Load(json);
            timeMode.Load(json);
        }

        public override void Save(JSONClass json)
        {
            delayTimer.Save(json);
            hideMenu.Save(json);
            showStartMarkers.Save(json);
            timeMode.Save(json);
        }
    }

    public class RecordScreen : ScreenBase
    {
        public const string ScreenName = "Record";

        private static UIDynamicButton _recordButton;
        private static string _previousUseCameraRaycast;

        private JSONStorableStringChooser _useCameraRaycast;
        private JSONStorableBool _recordExtendsLength;

        public override string screenId => ScreenName;

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            CreateChangeScreenButton("<b><</b> <i>返回</i>", MoreScreen.ScreenName);

            prefabFactory.CreateSpacer();

            prefabFactory.CreateHeader("录制选项", 1);

            _recordExtendsLength = new JSONStorableBool("录制延长长度", false);
            prefabFactory.CreateToggle(_recordExtendsLength);

            var recordTimeModeJSON = new JSONStorableStringChooser(
                "时间模式",
                new List<string> { TimeModes.RealTime.ToString(), TimeModes.UnityTime.ToString() },
                RecordScreenSettings.singleton.timeMode.defaultValue.ToString(),
                "时间模式"
            )
            {
                displayChoices = new List<string> { "实时(计时更好)", "游戏时间(与动画模式同步更好)" },
                valNoCallback = RecordScreenSettings.singleton.timeMode.value.ToString(),
                setCallbackFunction = val => RecordScreenSettings.singleton.timeMode.value = int.Parse(val)
            };
            prefabFactory.CreatePopup(recordTimeModeJSON, false, false);

            var recordInJSON = new JSONStorableFloat("录制延迟计时", RecordScreenSettings.singleton.delayTimer.defaultValue, 0f, 30f, false)
            {
                valNoCallback = RecordScreenSettings.singleton.delayTimer.value
            };
            recordInJSON.setCallbackFunction = val =>
            {
                RecordScreenSettings.singleton.delayTimer.value = (int) Mathf.Round(val);
                recordInJSON.valNoCallback = RecordScreenSettings.singleton.delayTimer.value;
                _recordButton.label = $"Start recording in {RecordScreenSettings.singleton.delayTimer.value}...";
            };
            prefabFactory.CreateSlider(recordInJSON);

            var hideMenuDuringRecording = new JSONStorableBool("录制时隐藏菜单", RecordScreenSettings.singleton.hideMenu.defaultValue)
            {
                valNoCallback = RecordScreenSettings.singleton.hideMenu.value,
                setCallbackFunction = val => RecordScreenSettings.singleton.hideMenu.value = val
            };
            prefabFactory.CreateToggle(hideMenuDuringRecording);

            var showStartMarkers = new JSONStorableBool("显示开始 / 下一个标记", RecordScreenSettings.singleton.showStartMarkers.defaultValue)
            {
                valNoCallback = RecordScreenSettings.singleton.showStartMarkers.value,
                setCallbackFunction = val => RecordScreenSettings.singleton.showStartMarkers.value = val
            };
            prefabFactory.CreateToggle(showStartMarkers);

            FreeControllerV3AnimationTarget raycastTarget = null;
            _useCameraRaycast = new JSONStorableStringChooser("摄像机射线检测目标", new List<string>(), "", "摄像机射线检测目标")
            {
                valNoCallback = _previousUseCameraRaycast
            };
            _useCameraRaycast.setCallbackFunction = val =>
            {
                _previousUseCameraRaycast = val;
                if (string.IsNullOrEmpty(val))
                {
                    raycastTarget = null;
                    return;
                }
                raycastTarget = current.targetControllers.FirstOrDefault(t => t.animatableRef.name == val);
                if (raycastTarget == null)
                {
                    SuperController.LogError($"Timeline: Target '{val}' was not in the list of targets");
                    _useCameraRaycast.valNoCallback = "";
                    return;
                }
                raycastTarget.selected = true;
            };
            prefabFactory.CreatePopup(_useCameraRaycast, true, false);

            prefabFactory.CreateSpacer();
            prefabFactory.CreateHeader("Record", 1);
            prefabFactory.CreateHeader("注意:请选择要录制的目标", 2);
            _recordButton = prefabFactory.CreateButton($"Start recording in {RecordScreenSettings.singleton.delayTimer.value}...");
            _recordButton.button.onClick.AddListener(() => SuperController.singleton.StartCoroutine(OnRecordCo(
                int.Parse(recordTimeModeJSON.val),
                _recordExtendsLength.val,
                hideMenuDuringRecording.val,
                showStartMarkers.val,
                raycastTarget)));

            prefabFactory.CreateSpacer();

            CreateChangeScreenButton("<i>前往<b>精简</b>界面...</i>", ReduceScreen.ScreenName);

            animationEditContext.animation.animatables.onTargetsSelectionChanged.AddListener(OnTargetsSelectionChanged);
            SyncAnimationFields();
            SyncRaycastTargets();
        }

        private void OnTargetsSelectionChanged()
        {
            SyncAnimationFields();
        }

        protected override void OnCurrentAnimationChanged(AtomAnimationEditContext.CurrentAnimationChangedEventArgs args)
        {
            base.OnCurrentAnimationChanged(args);
            SyncAnimationFields();
            SyncRaycastTargets();
        }

        private void SyncRaycastTargets()
        {
            _useCameraRaycast.choices = new[] {""}.Concat(current.targetControllers.Select(t => t.name)).ToList();
            _useCameraRaycast.valNoCallback = _useCameraRaycast.choices.Contains(_previousUseCameraRaycast) ? _previousUseCameraRaycast : "";
        }

        private void SyncAnimationFields()
        {
            _recordButton.button.interactable = !current.recording && animationEditContext.GetSelectedTargets().Any();
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            _recordExtendsLength.valNoCallback = current.animationLength == AtomAnimationClip.DefaultAnimationLength && current.GetAllCurveTargets().All(t => t.GetLeadCurve().length == 2);
        }

        public override void OnDestroy()
        {
            animationEditContext.animation.animatables.onTargetsSelectionChanged.RemoveListener(OnTargetsSelectionChanged);
            base.OnDestroy();
        }

        private IEnumerator OnRecordCo(int timeMode, bool recordExtendsLength, bool hideMenuDuringRecording, bool showStartMarkers, FreeControllerV3AnimationTarget raycastTarget)
        {
            if (raycastTarget != null && !raycastTarget.selected)
            {
                SuperController.LogError($"Cannot record raycast target {raycastTarget.name} because it is not selected. Either select it or remove it from the raycast target list.");
                yield break;
            }

            _recordButton.button.interactable = false;

            if (hideMenuDuringRecording)
            {
                SuperController.singleton.HideMainHUD();
            }

            var targets = animationEditContext.GetSelectedTargets().OfType<ICurveAnimationTarget>().ToList();

            var enumerator = operations.Record().StartRecording(
                plugin,
                timeMode,
                recordExtendsLength,
                RecordScreenSettings.singleton.delayTimer.value,
                targets.ToList(),
                raycastTarget,
                hideMenuDuringRecording,
                showStartMarkers
            );

            var changedScreen = false;
            if (!targets.OfType<FreeControllerV3AnimationTarget>().Any() && targets.OfType<JSONStorableFloatAnimationTarget>().Any())
            {
                ChangeScreen(TargetsScreen.ScreenName);
                changedScreen = true;
            }

            while (enumerator.MoveNext())
                yield return enumerator.Current;

            if (_recordButton != null)
                _recordButton.button.interactable = true;

            // This is a hack, not sure why it's necessary to update the keyframes
            yield return 0;
            current.DirtyAll();
            yield return 0;
            animationEditContext.ResetScrubberRange();
            if (hideMenuDuringRecording)
                SuperController.singleton.ShowMainHUDAuto();
            if (changedScreen)
            {
                yield return 0;
                // Note: The listener has been disposed, we have to ask the plugin
                plugin.ChangeScreen(ScreenName, null);
            }
        }
    }
}

