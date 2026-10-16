using System;
using System.Collections.Generic;
using System.Linq;
using MVR.FileManagementSecure;
using SimpleJSON;

namespace VamTimeline
{
    public class ImportExportScreen : ScreenBase
    {
        private const string _saveExt = "json";
        private const string _saveFolder = "Saves\\PluginData\\animations";
        private const string _exportCurrentAnimation = "当前动画";
        private const string _exportAllAnimations = "全部动画";
        private const string _exportCurrentAnimationAllLayers = "当前动画(所有层)";
        private const string _exportCurrentLayer = "当前层";
        private const string _exportCurrentSegment = "当前片段";
        private const string _exportAllSegments = "除共享片段外的所有片段";
        private const string _exportEverything = "包含共享片段的所有片段";

        public const string ScreenName = "Import / Export";

        public override string screenId => ScreenName;

        private JSONStorableStringChooser _exportIncludeJSON;
        private JSONStorableBool _exportPoseJSON;

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            CreateChangeScreenButton("<b><</b> <i>返回</i>", MoreScreen.ScreenName);

            prefabFactory.CreateSpacer();

            prefabFactory.CreateHeader("导入", 1);

            InitImportUI();

            prefabFactory.CreateSpacer();

            prefabFactory.CreateHeader("导出", 1);

            InitExportUI();
        }

        private void InitImportUI()
        {
            var importUI = prefabFactory.CreateButton("导入动画");
            importUI.button.onClick.AddListener(Import);
        }

        private void InitExportUI()
        {
            _exportIncludeJSON = new JSONStorableStringChooser(
                "包含",
                new List<string>(),
                "",
                "包含", (string _) => SyncExportPose()
            )
            {
                isStorable = false
            };
            SyncExportInclude();
            prefabFactory.CreatePopup(_exportIncludeJSON, true, true, 800f);

            _exportPoseJSON = new JSONStorableBool("包含姿势", true);
            prefabFactory.CreateToggle(_exportPoseJSON);

            var exportUI = prefabFactory.CreateButton("导出");
            exportUI.button.onClick.AddListener(Export);
        }

        private void SyncExportInclude()
        {
            var choices = new List<string> { _exportCurrentAnimation };
            var defaultChoice = _exportCurrentAnimation;
            if (animationEditContext.currentSegment.layers.Count > 1)
                choices.AddRange(new[] { defaultChoice = _exportCurrentLayer, _exportCurrentAnimationAllLayers });
            if (animation.index.segmentNames.Count > 1)
                choices.AddRange(new[] { defaultChoice = _exportCurrentSegment, _exportAllSegments });
            else if(animationEditContext.currentSegment.allClips.Count() > 1)
                choices.AddRange(new[] { defaultChoice = _exportAllAnimations });
            if (animation.index.segmentNames.Any(s => s == AtomAnimationClip.SharedAnimationSegment))
                choices.Add(_exportEverything);
            _exportIncludeJSON.choices = choices;
            if (!choices.Contains(_exportIncludeJSON.val))
                _exportIncludeJSON.val = defaultChoice;
        }

        private void Export()
        {
            try
            {
                #if (VAM_GT_1_20)
                FileManagerSecure.CreateDirectory(_saveFolder);
                #endif
                var fileBrowserUI = SuperController.singleton.fileBrowserUI;
                fileBrowserUI.SetTitle("导出动画");
                fileBrowserUI.fileRemovePrefix = null;
                fileBrowserUI.hideExtension = false;
                fileBrowserUI.keepOpen = false;
                fileBrowserUI.fileFormat = _saveExt;
                fileBrowserUI.defaultPath = _saveFolder;
                fileBrowserUI.showDirs = true;
                fileBrowserUI.shortCuts = null;
                fileBrowserUI.browseVarFilesAsDirectories = false;
                fileBrowserUI.SetTextEntry(true);
                fileBrowserUI.Show(ExportFileSelected);
                fileBrowserUI.ActivateFileNameField();
            }
            catch (Exception exc)
            {
                SuperController.LogError($"Timeline: Failed to save file dialog: {exc}");
            }
        }

        private void ExportFileSelected(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!path.ToLower().EndsWith($".{_saveExt}")) path += $".{_saveExt}";

            try
            {
                var jc = new JSONClass
                {
                    ["SerializeVersion"] = new JSONData(AtomAnimationSerializer.SerializeVersion),
                    ["AtomType"] = plugin.containingAtom.type,
                    ["Clips"] = GetExportClipsJson(),
                };
                SuperController.singleton.SaveJSON(jc, path);
                SuperController.singleton.DoSaveScreenshot(path);
            }
            catch (Exception exc)
            {
                SuperController.LogError($"Timeline: Failed to export animation: {exc}");
            }
        }

        private JSONArray GetExportClipsJson()
        {
            var clips = GetExportClips();

            var temporaryPose = _exportPoseJSON.val && clips[0].pose == null;
            if (temporaryPose)
                clips[0].pose = AtomPose.FromAtom(plugin.containingAtom, false, true, false, false);

            var clipsJSON = new JSONArray();
            foreach (var clip in clips)
            {
                clipsJSON.Add(plugin.serializer.SerializeClip(clip, animation.serializeMode));
            }

            if (temporaryPose)
                clips[0].pose = null;

            return clipsJSON;
        }

        private List<AtomAnimationClip> GetExportClips()
        {
            List<AtomAnimationClip> clips;
            switch (_exportIncludeJSON.val)
            {
                case "":
                    return new List<AtomAnimationClip>();
                case _exportCurrentAnimation:
                    clips = new List<AtomAnimationClip>(new[] { animationEditContext.current });
                    break;
                case _exportCurrentLayer:
                    clips = animationEditContext.currentLayer.ToList();
                    break;
                case _exportCurrentAnimationAllLayers:
                    clips = animation.index.ByName(current.animationSegmentId, current.animationNameId).ToList();
                    break;
                case _exportCurrentSegment:
                case _exportAllAnimations:
                    clips = animationEditContext.currentSegment.allClips.ToList();
                    break;
                case _exportAllSegments:
                    clips = animation.clips.Where(c => !c.isOnSharedSegment && !c.isOnNoneSegment).ToList();
                    break;
                case _exportEverything:
                    clips = animation.clips;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown export mode: {_exportIncludeJSON.val}");
            }

            return clips;
        }

        private void Import()
        {
            try
            {
                #if (VAM_GT_1_20)
                FileManagerSecure.CreateDirectory(_saveFolder);
                var shortcuts = FileManagerSecure.GetShortCutsForDirectory(_saveFolder);
                SuperController.singleton.GetMediaPathDialog(ImportFileSelected, _saveExt, _saveFolder, false, true, false, null, false, shortcuts);
                #else
                SuperController.singleton.GetMediaPathDialog(ImportFileSelected, _saveExt, _saveFolder);
                #endif
            }
            catch (Exception exc)
            {
                SuperController.LogError($"Timeline: Failed to open file dialog: {exc}");
            }
        }

        private void ImportFileSelected(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            path = FileManagerSecure.NormalizePath(path);

            try
            {
                var json = SuperController.singleton.LoadJSON(path);
                if (json["AtomType"]?.Value != plugin.containingAtom.type)
                {
                    SuperController.LogError($"Timeline: Loaded animation for {json["AtomType"]} but current atom type is {plugin.containingAtom.type}");
                    return;
                }

                var jc = json.AsObject;
                var serializationVersion = jc.HasKey("SerializeVersion") ? jc["SerializeVersion"].AsInt : 0;

                if (serializationVersion == 0)
                    ImportControllerStatesLegacy(jc);

                var clipsJSON = jc["Clips"].AsArray;
                if (clipsJSON == null || clipsJSON.Count == 0)
                {
                    SuperController.LogError($"Timeline: No animations were found in {path}");
                    return;
                }

                if (animation.clips.Count == 1 && animation.clips[0].IsEmpty())
                    animation.RemoveClip(animation.clips[0]);

                animation.animatables.locked = true;
                var imported = new List<AtomAnimationClip>();
                foreach (JSONClass clipJSON in clipsJSON)
                {
                    imported.Add(plugin.serializer.DeserializeClip(clipJSON, animation.animatables, animation.logger, serializationVersion));
                }

                ChangeScreen(ImportAssignScreen.ScreenName, imported);
            }
            catch (Exception exc)
            {
                SuperController.LogError($"Timeline.{nameof(AdvancedKeyframeToolsScreen)}.{nameof(ImportFileSelected)}: Failed to import animation: {exc}");
            }
        }

        private void ImportControllerStatesLegacy(JSONClass jc)
        {
            if (!jc.HasKey("ControllersState")) return;
            var controllersState = jc["ControllersState"].AsObject;
            foreach (var k in controllersState.Keys)
            {
                var fc = plugin.containingAtom.freeControllers.FirstOrDefault(x => x.name == k);
                if (fc == null)
                {
                    SuperController.LogError($"Timeline: Loaded animation had state for controller {k} but no such controller were found on this atom.");
                    continue;
                }
                var state = controllersState[k];
                fc.currentPositionState = (FreeControllerV3.PositionState)state["currentPositionState"].AsInt;
                fc.transform.localPosition = AtomAnimationSerializer.DeserializeVector3(state["localPosition"].AsObject);
                fc.currentRotationState = (FreeControllerV3.RotationState)state["currentRotationState"].AsInt;
                fc.transform.localRotation = AtomAnimationSerializer.DeserializeQuaternion(state["localRotation"].AsObject);
            }
            SuperController.LogMessage("Timeline: The imported animation contains legacy controllers state, your pose has been modified.");
        }

        protected override void OnCurrentAnimationChanged(AtomAnimationEditContext.CurrentAnimationChangedEventArgs args)
        {
            base.OnCurrentAnimationChanged(args);

            SyncExportInclude();
            SyncExportPose();
        }

        private void SyncExportPose()
        {
            if (_exportPoseJSON == null) return;
            if (_exportIncludeJSON.val == "") return;
            _exportPoseJSON.toggle.interactable = GetExportClips().GroupBy(c => c.animationLayer).Select(l => l.First()).All(c => c.pose == null);
            if (!_exportPoseJSON.toggle.interactable) _exportPoseJSON.valNoCallback = true;
        }
    }
}

