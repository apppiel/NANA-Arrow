namespace NanaArrow.Services
{
    /// <summary>docs/ANALYTICS.md v0.1 이벤트·파라미터·속성 이름. 문자열은 여기에만 둔다.</summary>
    public static class AnalyticsEvents
    {
        // §3-1 레벨 진행
        public const string LevelStart = "level_start";
        public const string LevelClear = "level_clear";
        public const string LevelFail = "level_fail";
        public const string LevelQuit = "level_quit";
        public const string Retry = "retry";
        // §3-2 광고
        public const string ContinueOffer = "continue_offer";
        public const string ContinueRequest = "continue_request";
        public const string ContinueGranted = "continue_granted";
        public const string AdInterstitial = "ad_interstitial";
        public const string AdRewardedResult = "ad_rewarded_result";
        // §3-3 튜토리얼·기능
        public const string TutorialStep = "tutorial_step";
        public const string TutorialDone = "tutorial_done";
        public const string LanePreviewFirst = "lane_preview_first";
        // §3-4 메뉴·보상
        public const string LevelSelectOpen = "level_select_open";
        public const string SettingsChange = "settings_change";
        public const string RewardCodeIssued = "reward_code_issued";
        public const string RewardCodeCopy = "reward_code_copy";
        public const string RewardLinkOpen = "reward_link_open";

        // §2 공통 파라미터
        public const string ParamLevel = "level";
        public const string ParamLevelName = "level_name";
        public const string ParamBoard = "board";
        public const string ParamArrows = "arrows";
        public const string ParamIsReplay = "is_replay";
        public const string ParamAttempt = "attempt";
        // 이벤트별
        public const string ParamStartReason = "start_reason";
        public const string ParamDurationSec = "duration_sec";
        public const string ParamTaps = "taps";
        public const string ParamBlocks = "blocks";
        public const string ParamLivesLeft = "lives_left";
        public const string ParamArrowsLeft = "arrows_left";
        public const string ParamContinuesUsed = "continues_used";
        public const string ParamLanePreviews = "lane_previews";
        public const string ParamQuitReason = "quit_reason";
        public const string ParamRetrySource = "retry_source";
        public const string ParamAdReady = "ad_ready";
        public const string ParamLivesAfter = "lives_after";
        public const string ParamTrigger = "trigger";
        public const string ParamResult = "result";
        public const string ParamTextKey = "text_key";
        public const string ParamElapsedSec = "elapsed_sec";
        public const string ParamHighestLevel = "highest_level";
        public const string ParamSetting = "setting";
        public const string ParamValue = "value";
        public const string ParamSynced = "synced";

        // §4 사용자 속성
        public const string PropHighestLevel = "highest_level";
        public const string PropSoundOn = "sound_on";
        public const string PropVibrationOn = "vibration_on";
        public const string PropRewardIssued = "reward_issued";

        /// <summary>`level_012` (ANALYTICS §1).</summary>
        public static string LevelName(int level) => $"level_{level:000}";

        /// <summary>`8x10`.</summary>
        public static string Board(int width, int height) => $"{width}x{height}";

        /// <summary>enum → snake_case (First → first, RetryHud → retry_hud).</summary>
        public static string SnakeCase(System.Enum value)
        {
            var name = value.ToString();
            var sb = new System.Text.StringBuilder(name.Length + 4);
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsUpper(c) && i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }
    }
}
