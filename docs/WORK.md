# WORK.md — 작업 지시 (디렉터만 작성)
> 팀장은 각 AI에게 "docs/WORK.md 읽고 네 항목 처리해줘" 만 말한다.
> **이 파일은 디렉터만 수정한다.** 보고는 각자 파일에: 프로그래머 → docs/REPORT_PROGRAMMER.md, 기획자 → docs/REPORT_PLANNER.md
> 항목이 없으면 그 역할은 할 일 없음. W-### 은 GitHub 이슈 번호와 별개.

---

## 팀장 (에디터·계정 작업 — AI 가 못 하는 것)
- [ ] **PR 머지**: GitHub 웹에서 PR #1 → #28 → #29 순서로 Merge. 그 다음 터미널 `git checkout main && git pull`
- [ ] Game 씬 조립: REPORT_PROGRAMMER.md W-005 "팀장 에디터 할 일" 1~7 (ArrowViewStyle 에셋 → Board/Input/GameController 오브젝트 → Play 테스트 → Level Jump 치트)
- [ ] (출시 전, 급하지 않음) AdMob 콘솔 앱 2개 + 광고 단위 4개 + Unity Ads 미디에이션 그룹, Firebase 새 프로젝트 `nana-arrow` + google-services.json / GoogleService-Info.plist

## 프로그래머

### W-010 인프라 이식 (선행: 팀장 PR 머지 완료. 브랜치 `feat/services-infra`)
디렉터 승인: W-009 §4 어셈블리 제안 그대로. `NanaArrow.Services` asmdef 신설, Core 는 Services 미참조.
- SDK 폴더 복사 (NO.3 SpotTheDifference 에서): `Assets/GoogleMobileAds`, `Assets/Firebase`, `Assets/ExternalDependencyManager`, `Plugins/Android/*.androidlib`, gradle 템플릿 3종. **CLAUDE.md "절대 복사 금지" 항목은 테스트 ID / 빈 값으로**
- 그대로 이식: `SaveService`+`SaveCodec`+`SaveData`(필드는 최고 레벨·응모 코드 발급 여부·설정 3종: 사운드·진동, 테스트 포함), `ScreenCaptureProtection`, `SafeAnalytics`, NO.2 `SafeAreaAdapter`
- 수정 이식: `BootLoader`(Addressables 제거), NO.2 `SceneLoader` → `Core.SceneLoader`(씬 이름 enum), NO.2 `AudioManager` 뼈대(클립 목록은 UI_FLOW 사운드 13종 이름으로 SerializeField), 뒤로가기 `Keyboard.escapeKey` 부분만
- 새로: `Data/LevelCatalog` SO (TextAsset 리스트, 인스펙터 순서 편집) + `GameController` 가 카탈로그+레벨 번호로 로드. `PopupBase`(열기/닫기/버튼 이벤트만, 문구는 UI_FLOW 키). 치트에 즉시 클리어·목숨 채우기 추가. `TapInput` 에 `EventSystem.IsPointerOverGameObject` 필터
- 설정(사운드·진동)은 PlayerPrefs 유지 (재화 아님). 진동은 하트 감소 시에만 (`vibrateOnLifeLost` SerializeField)
- 보고에 팀장이 씬에 붙일 것 목록

### W-011 광고·응모 코드 (선행: W-010. 브랜치 `feat/ads-reward`)
- `Core/AdsManager` 순수 C#: NO.3 `InterstitialGate` 를 AdsConfig 값으로 확장. 입력 = `LevelCleared(level, alreadyCleared)` / `RetryPressed(failCountThisLevel)` / `ContinueRequested(continuesUsed)` → 출력 = 광고 종류 or 없음. 이미 깬 레벨 재클리어도 카운트 포함. 설정 팝업의 다시하기는 실패로 안 셈 (GAME_RULES §11). 테스트 필수
- `Core/IInterstitialAd`, `IRewardedAd` 인터페이스 + `Services/AdMobService`(NO.3 거의 그대로, 클래스명·네임스페이스·ID 만 교체, 테스트 ID). 에디터에서는 광고 스킵
- `Core/RewardCode`(NO.3 그대로 + 테스트) + `Services/RewardCodeService`(Firestore, NO.3 그대로). 홈페이지 URL `https://nanabox.co.kr/reward-claim.html` 은 AdsConfig 옆 `RewardConfig` SO 에 (인스펙터)
- `RewardCodePanel` 은 UI_FLOW §7 문구로 뼈대만 (팀장이 프리팹 조립)

### W-006 후속 — 보류
즉시 클리어·목숨 채우기 치트는 W-010 에 포함시킴.

## 기획자

### W-012 UI_FLOW v0.2 + 프리팹 가이드
- GAME_RULES §11 의 디렉터 답변 5건을 UI_FLOW 에 반영 (v0.2)
- §12 캔버스·팝업 프리팹 구조를 **팀장이 에디터에서 따라 만들 수 있는 단계별 가이드** 로 확장 (오브젝트 이름, 앵커, 컴포넌트, 어떤 스크립트를 붙이는지). 프로그래머 W-010 의 PopupBase 를 전제
- `TutorialConfig` SO 필드 정의 (레벨·대상 arrow id·문구·손가락 위치)

### W-013 레벨 21~30 (7×7, Key/Locked 26 도입) — W-012 다음
- LEVEL_DESIGN v0.4, JSON 을 직접 `Assets/_Project/Levels/` 에 저장하고 프로젝트 LevelValidator 로 검증 (앞으로 레벨 파일 저장은 기획자 담당, 프로그래머는 손대지 않음)

### W-014 애널리틱스 이벤트 목록 (이슈 #26) — W-013 다음
- level_start / level_clear / level_fail / continue / retry / ad_shown 등 이벤트명·파라미터 표 → docs/ANALYTICS.md

---

## 결정 사항 (참고)
- 광고 SDK: **AdMob 단독 호출 + Unity Ads 는 AdMob 미디에이션** (세 기존 게임과 동일). ID 는 전부 신규 발급
- 응모 코드: NO.3 SpotTheDifference 의 RewardCode/RewardCodeService 이식. 규칙 XXXX-XXXX (0/O/1/I 제외 32자)
- Firebase: NANA-Arrow 전용 새 프로젝트 필요 (기존 게임과 분리)
- 레벨 파일 저장 담당: 기획자 (프로그래머는 로더·검증기만)
- 어셈블리: Core / Gameplay / Data / Services / UI / Editor / Tests

## 완료
- W-001 초기화·asmdef·Gameplay 코어·테스트 49 — 프로그래머 (09-17)
- W-002 LevelLoader/Validator + W-003 규칙 v0.5.1, 테스트 110 — 프로그래머 (09-17)
- W-004 ISSUES.md 26건, LEVEL_FORMAT v0.4, LEVEL_DESIGN 1~10 — 기획자 (09-17)
- W-005 보드 뷰·GameSession·GameController·TapInput, 이슈 등록 27건, 테스트 129 — 프로그래머 (09-17)
- W-006 Level Validator 창, Level Jump 치트, 레벨 1~20 파일 저장, 테스트 132 — 프로그래머 (09-17)
- W-007 UI_FLOW v0.1 (흐름·팝업 6종·문구 키·사운드 13종·광고 연결 지점) — 기획자 (09-17)
- W-008 LEVEL_DESIGN 11~20 (6×6, Frozen 16) — 기획자 (09-17)
- W-009 이전 프로젝트 3종 조사·이식 판정 — 프로그래머 (09-17)
- 규칙 v0.4 검토 7건 → v0.5.1, 보드 크기 → v0.5.2, UI 결정 5건 → v0.5.3 — 기획자/디렉터 (09-17)
