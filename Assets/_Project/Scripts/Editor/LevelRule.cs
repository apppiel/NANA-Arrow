namespace NanaArrow.Editor
{
    /// <summary>LEVEL_FORMAT 검증 규칙 번호. Schema 는 규칙 이전의 형식 검사.</summary>
    public enum LevelRule
    {
        Schema = 0,
        CellsInBoundsAndDisjoint = 1,
        PathShape = 2,
        LockedHasKey = 3,
        Solvable = 4,
    }
}
