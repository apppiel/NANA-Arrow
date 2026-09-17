using System;

namespace NanaArrow.Core
{
    /// <summary>응모 코드 발급 서비스 (Services 의 RewardCodeService 가 구현, App.RewardCodes 로 등록). UI 는 이것만 본다.</summary>
    public interface IRewardCodeService
    {
        /// <summary>팝업이 열릴 때. 결과는 <see cref="App.RewardCodeIssued"/> 로 온다 (code 가 비어 있으면 "확인 중").</summary>
        void IssueCode();
    }
}
