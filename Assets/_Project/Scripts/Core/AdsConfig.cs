using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>
    /// 광고 정책 (GAME_RULES §7, 확정). 판단은 AdsManager 한 곳에서만 한다.
    /// 인스턴스는 Assets/_Project/Settings/AdsConfig.asset.
    /// </summary>
    [CreateAssetMenu(fileName = "AdsConfig", menuName = "NanaArrow/Ads Config")]
    public sealed class AdsConfig : ScriptableObject
    {
        [Header("보상형: 목숨 0 → 이어하기")]
        [SerializeField, Min(1), Tooltip("광고 시청 후 회복되는 목숨")]
        private int continueLives = 1;

        [SerializeField, Min(0), Tooltip("레벨당 이어하기 허용 횟수")]
        private int maxContinues = 1;

        [Header("전면: 레벨 클리어 후")]
        [SerializeField, Min(1), Tooltip("N 레벨 클리어마다 전면 광고")]
        private int interstitialEveryNLevels = 3;

        [SerializeField, Min(0), Tooltip("이 레벨까지는 광고 없음")]
        private int adFreeLevels = 5;

        [Header("전면: 다시하기 반복")]
        [SerializeField, Min(1), Tooltip("같은 레벨 N번 실패마다 전면 광고")]
        private int interstitialAfterFails = 2;

        public int ContinueLives => continueLives;
        public int MaxContinues => maxContinues;
        public int InterstitialEveryNLevels => interstitialEveryNLevels;
        public int AdFreeLevels => adFreeLevels;
        public int InterstitialAfterFails => interstitialAfterFails;
    }
}
