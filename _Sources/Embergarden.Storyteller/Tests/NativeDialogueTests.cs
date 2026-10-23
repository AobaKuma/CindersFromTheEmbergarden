using System.Reflection;
using Embergarden.Storytelling;
using Verse;
using UnityEngine;

internal static class NativeDialogueTests
{
    private static readonly FieldInfo PlaybackField = typeof(EmberCycleGame)
        .GetField("dialogue", BindingFlags.NonPublic | BindingFlags.Instance);

    public static void Run()
    {
        CheckSingleConfirmation();
        CheckStaleWindowClose();
        CheckWindowFramework();
        Console.WriteLine("[CinderStory] 单确认窗口快照与关闭归属测试通过");
    }

    private static DialoguePlayback CreatePlayback()
    {
        EmberDialogueDef next = new EmberDialogueDef { text = "后续波次 {WAVES}" };
        return new DialoguePlayback
        {
            Text = "开场", ConfirmationText = "确认", Context = new DialogueContext { Waves = 8 },
            DefaultNextDialogue = next,
            Replies = { new DialogueReplySnapshot { Text = "继续", NextDialogue = next },
                new DialogueReplySnapshot { Text = "结束" } }
        };
    }

    private static void CheckSingleConfirmation()
    {
        EmberCycleGame owner = new EmberCycleGame(null);
        DialoguePlayback playback = CreatePlayback();
        PlaybackField.SetValue(owner, playback);
        EmberDialogueSession session = new EmberDialogueSession(owner, playback);
        Program.Check(session.Text == "开场" && session.ConfirmationText == "确认", "单确认文本未使用快照");
        Program.Check(ReferenceEquals(owner.Dialogue, playback), "旧分支配置触发了额外对话");
        session.Close();
        Program.Check(owner.Dialogue == null, "关闭未确认当前对话");
        Program.Check(session.Text == "开场" && session.ConfirmationText == "确认", "关闭后旧窗口丢失绘制快照");
    }

    private static void CheckStaleWindowClose()
    {
        EmberCycleGame owner = new EmberCycleGame(null);
        DialoguePlayback old = CreatePlayback();
        DialoguePlayback next = CreatePlayback();
        PlaybackField.SetValue(owner, old);
        EmberDialogueSession session = new EmberDialogueSession(owner, old);
        PlaybackField.SetValue(owner, next);
        session.Close();
        Program.Check(ReferenceEquals(owner.Dialogue, next), "旧窗口关闭清空了新对话");
        session.Close();
    }

    private static void CheckWindowFramework()
    {
        Program.Check(typeof(Dialog_EmberPhase).BaseType == typeof(Dialog_MessageBox), "未使用原版单确认窗口");
        CheckConstructedWindows();
        Program.Check(typeof(Dialog_EmberPhase).GetMethod("WindowUpdate").DeclaringType != typeof(Dialog_EmberPhase),
            "残留自建逐字窗口更新");
        Program.Check(!EmberDialogueTheme.Active, "无对话窗口时误启用主题");
        Program.Check(typeof(Widgets).GetMethod(nameof(Widgets.DrawWindowBackground),
            new[] { typeof(Rect) }) != null, "原版底板配色入口不存在");
        Program.Check(typeof(Widgets).GetMethod(nameof(Widgets.ButtonText), new[] { typeof(Rect),
            typeof(string), typeof(bool), typeof(bool), typeof(Color), typeof(bool), typeof(TextAnchor?) }) != null,
            "原版选项配色入口不存在");
    }

    private static void CheckConstructedWindows()
    {
        using DialogueConstructionScope scope = new DialogueConstructionScope();
        EmberCycleGame owner = new EmberCycleGame(null);
        PlaybackField.SetValue(owner, CreatePlayback());
        foreach (bool battle in new[] { false, true })
        {
            Dialog_EmberPhase window = new Dialog_EmberPhase(owner, battle);
            Program.Check(window.forcePause, "真实周期弹窗未强制暂停");
            Program.Check(window.absorbInputAroundWindow && window.preventCameraMotion,
                "真实周期弹窗未阻止外部输入");
            Program.Check(window.text.ToString() == "开场" && window.buttonAText == "确认",
                "真实周期弹窗未使用对话快照");
        }
    }
}
