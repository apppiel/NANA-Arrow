using UnityEngine;

namespace NanaArrow.Core
{
    /// <summary>응모 코드 (GAME_RULES §7, UI_FLOW §6-5). 인스턴스는 Assets/_Project/Settings/RewardConfig.asset.</summary>
    [CreateAssetMenu(fileName = "RewardConfig", menuName = "NanaArrow/Reward Config")]
    public sealed class RewardConfig : ScriptableObject
    {
        [SerializeField, Min(1), Tooltip("이 레벨을 클리어하면 응모 코드 발급")] private int rewardLevel = 100;
        [SerializeField, Tooltip("상품 받으러 가기 URL")] private string claimUrl = "https://nanabox.co.kr/reward-claim.html";

        public int RewardLevel => rewardLevel;
        public string ClaimUrl => claimUrl;
    }
}
