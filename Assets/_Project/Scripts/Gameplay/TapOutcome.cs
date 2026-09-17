namespace NanaArrow.Gameplay
{
    public enum TapOutcome
    {
        /// <summary>보드에 없는 Arrow 이거나 목숨 0 — 상태 변화 없음.</summary>
        Ignored,
        /// <summary>보드 밖으로 나감 (GAME_RULES §2-2).</summary>
        Exit,
        /// <summary>경로가 막힘 (GAME_RULES §2-3, §2-6).</summary>
        Blocked,
        /// <summary>Frozen 얼음 깨기 탭 — 경로 무관, 목숨 차감 없음 (GAME_RULES §3).</summary>
        IceBroken,
        /// <summary>잠긴 Locked 탭 — 목숨 차감 없음, 자물쇠 흔들림 연출 (GAME_RULES §3).</summary>
        Locked,
    }
}
