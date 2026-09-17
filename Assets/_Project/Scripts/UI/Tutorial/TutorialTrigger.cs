namespace NanaArrow.UI.Tutorial
{
    /// <summary>튜토리얼 표시·숨김 조건 (UI_FLOW §7-1). First* 는 그 판에서 처음 일어난 순간.</summary>
    public enum TutorialTrigger
    {
        None,
        LevelStart,
        FirstTap,
        FirstExit,
        FirstBlock,
        FirstIceBreak,
        FirstLongPress
    }
}
