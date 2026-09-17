namespace NanaArrow.Core
{
    /// <summary>응모 코드 발급 진행 상태 (문구는 UI 가 Strings 키로 바꾼다).</summary>
    public enum RewardCodeStatus
    {
        /// <summary>로컬·서버 확인 중 (코드 아직 없음).</summary>
        Checking,
        /// <summary>새로 발급돼 서버에 저장 중.</summary>
        Saving,
        /// <summary>새로 발급 + 서버 저장 완료.</summary>
        Issued,
        /// <summary>이미 발급된 코드 (서버 저장 완료).</summary>
        Reissued,
        /// <summary>서버 연결 불가 — 로컬 발급, 다음 실행 때 재시도.</summary>
        Offline,
        /// <summary>서버 저장 실패 — 로컬엔 남음, 다음 실행 때 재시도.</summary>
        SaveFailed
    }
}
