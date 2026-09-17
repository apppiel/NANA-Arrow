namespace NanaArrow.Core
{
    /// <summary>전면 광고 판정 (ANALYTICS.md ad_interstitial.result 와 1:1 — 실제 표시 여부는 SDK 재고에 따라 Shown/NotReady 로 갈린다).</summary>
    public enum InterstitialVerdict
    {
        /// <summary>광고 차례 아님 (카운터 진행 중).</summary>
        NotDue,
        /// <summary>광고 없는 초반 레벨 (adFreeLevels 이하).</summary>
        SkippedFreeLevel,
        /// <summary>광고 차례. 준비돼 있으면 띄운다.</summary>
        Due
    }
}
