using System;
using System.Text;

namespace VamTimeline
{
    public class DefaultsScreen : ScreenBase
    {
        public const string ScreenName = "Defaults";

        public override string screenId => ScreenName;

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            CreateChangeScreenButton("<b><</b> <i>返回</i>", MoreScreen.ScreenName);

            var createDefaultsBtn = prefabFactory.CreateButton("保存为默认");
            createDefaultsBtn.button.onClick.AddListener(() =>
            {
                TimelineDefaults.singleton.Save();
                SuperController.LogMessage($"Timeline: Settings saved to '{TimelineDefaults.DefaultsPath}'.");
            });

            var deleteDefaultsBtn = prefabFactory.CreateButton("删除当前默认值");
            deleteDefaultsBtn.button.onClick.AddListener(() =>
            {
                TimelineDefaults.singleton.Delete();
                SuperController.LogMessage("Timeline: Settings deleted. Reload to revert to defaults options.");
            });

            prefabFactory.CreateHeader("Current", 2);
            {
                var sb = new StringBuilder();
                TimelineDefaults.singleton.GetJSON().ToString("", sb);
                var currentDefaultsJSON = new JSONStorableString("Live", sb.ToString());
                var currentDefaultsUI = prefabFactory.CreateTextField(currentDefaultsJSON);
                currentDefaultsUI.height = 400f;
            }

            prefabFactory.CreateHeader("磁盘上的内容", 2);
            {
                if (TimelineDefaults.singleton.Exists())
                {
                    var sb = new StringBuilder();
                    try
                    {
                        var json = SuperController.singleton.LoadJSON(TimelineDefaults.DefaultsPath);

                        json.ToString("", sb);
                    }
                    catch (Exception exc)
                    {
                        sb.AppendLine($"An error occured while trying to open the file: {exc}");
                    }
                    var currentDefaultsJSON = new JSONStorableString("OnDisk", sb.ToString());
                    var currentDefaultsUI = prefabFactory.CreateTextField(currentDefaultsJSON);
                    currentDefaultsUI.height = 400f;
                }
                else
                {
                    var currentDefaultsJSON = new JSONStorableString("OnDisk", "[文件未找到]");
                    var currentDefaultsUI = prefabFactory.CreateTextField(currentDefaultsJSON);
                    currentDefaultsUI.height = 60f;
                }
            }
        }
    }
}

