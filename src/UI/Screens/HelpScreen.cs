namespace VamTimeline
{
    public class HelpScreen : ScreenBase
    {
        public const string ScreenName = "Help";

        private const string _helpText = @"
<b>欢迎使用 Timeline!</b>

使用关键帧与触发器,创建高级且动态的基于曲线的动画。

wiki 中提供了文档(可从「更多」菜单进入),也有视频教程。

<b>界面</b>

顶部是<i>标签页</i>,用于在各界面之间切换。

左侧是动画列表。默认只有一个:'Anim 1'。如果你使用层(Layers)与片段(Segments),它们也会出现在这里(见下文)。

下方是关键帧导航控件。最左和最右的按钮跳到上一帧 / 下一帧,内侧按钮用于前后移动,中间按钮会「吸附」到最近的整秒。

有两个播放按钮:""全部""(All)会同时播放所有层并播放序列;另一个(以当前动画命名)只播放当前片段。

然后是<i>时间轴(scrubber)</i>,显示动画时间与当前位置。你可以前后拖动时间,也可以缩放。

再下面是<i>简易表(dope sheet)</i>,显示所有<i>目标</i>(例如手部控制器或微笑形变),以及它们在各个时间点是否有关键帧。可用左上角按钮切换到曲线视图。

最后是删除、复制、粘贴按钮;它们作用于简易表中当前选中的关键帧。

<b>你的第一个动画</b>

在「目标」面板中点击「添加 / 移除目标」按钮。

选择你想要动画化的对象。例如右手控制器:在「控制器」下拉框中选择 rHandControl,然后点击「添加」。你会看到该目标出现在简易表中。

把右手移动到一个新位置,将时间轴拖到 1 秒,再把右手移到另一个位置。

现在倒回(点击停止)并播放。你会看到右手在两个位置之间来回移动,这是因为动画在<i>循环</i>。你可以在「编辑」标签页中更改此行为。

<b>序列</b>

你可以创建多个动画并自动在它们之间混合。再创建一个动画(动画 → 创建 → 创建动画),然后进入「序列」标签页。在「播放下一个」下拉框中可以看到另一个动画,选中它便会自动播放。更多序列功能请查看 wiki。

你可以把动画命名为 'prefix/something' 来播放一组动画,它们会在「播放下一个」下拉框中显示为 'prefix/*'。

<b>层与片段</b>

你可以在「动画 → 创建」中创建层。层允许多个动画同时运行,各自影响自己的目标。例如一个层做呼吸动画,另一个层做手部动画。每层都有自己的动画与序列。

多个层之间也可以有同名动画;它们会始终一起播放与拖动。

如果你需要创建使用不同目标的动画,可以创建片段。片段就像完全独立的动画,拥有自己的层。除共享片段外,同一时间只能播放一个片段。

<b>学习</b>

更多文档与视频请查看 wiki(「更多」菜单中有链接)。你能做的事情非常多!祝你玩得开心!
";

        public override string screenId => ScreenName;

        public override void Init(IAtomPlugin plugin, object arg)
        {
            base.Init(plugin, arg);

            CreateChangeScreenButton("<b><</b> <i>返回</i>", MoreScreen.ScreenName);

            InitExplanation();
        }

        private void InitExplanation()
        {
            var textJSON = new JSONStorableString("帮助", _helpText);
            var textUI = prefabFactory.CreateTextField(textJSON);
            textUI.height = 1070f;
        }
    }
}

