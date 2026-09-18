# HANDOFF_PROGRAMMER.md — 클라이언트 프로그래머 인수인계 (2026-09-17 세션 종료 시점)
> **프로그래머(클로드 코드)만 쓴다.** 새 세션은 이 파일 → `CLAUDE.md` → `docs/WORK.md` 순서로 읽고 시작. 매 세션 끝에 이 파일을 갱신한다.
> 상세 변경 이력은 `docs/REPORT_PROGRAMMER.md` (W-### 별 보고, 최신이 맨 위). 이 파일은 "지금 상태 + 다음에 알아야 할 것" 만.

## 0. 세션 시작 체크리스트
1. `git status` / `git log --oneline -5` — 브랜치는 항상 **main 기준 하나씩** (스택 금지). 남은 작업 브랜치가 있으면 WORK.md 와 대조
2. `docs/WORK.md` 의 **프로그래머** 항목만 처리 (WORK.md 는 디렉터만 수정 — 절대 편집 금지). 기획자·팀장 항목은 건드리지 않음
3. 규칙 문서: `docs/GAME_RULES.md` (v0.7.1 이 유일 기준), `docs/LEVEL_FORMAT.md` v0.6, `docs/UI_FLOW.md` v0.2, `docs/ANALYTICS.md` v0.1. 세션 중에도 PM(클로드 데스크탑)·디렉터가 docs 를 바꾸므로, 의존하기 전에 다시 읽는다
4. 에디터가 열려 있으면 Unity MCP: `recompile` → `console(level=warn)` → `run_tests(mode=editor, filter_type=assembly, filter=NanaArrow.Tests.EditMode)`. 현재 **267/267**
5. 작업 끝: `docs/REPORT_PROGRAMMER.md` 맨 위에 보고 추가 → 커밋 → push → `gh pr create` → `gh pr merge --merge --delete-branch` (허용됨) → `git checkout main && git pull` → 이 파일 갱신

## 1. 완료된 작업 (전부 main 에 머지됨)
| W | 내용 | PR |
|---|---|---|
| W-001~008 | 순수 C# 코어(Board/Arrow/FireResolver/TapHandler/LivesTracker), Data(LevelLoader), Editor(LevelValidator·치트 창), View(BoardView·ArrowView), TapInput | #1~#29 |
| W-015 후속 + W-010 | 셀 크기 규칙, 규칙 2-e, SDK 이식(AdMob·Firebase·EDM), Save/Progress, SceneLoader, AudioManager, PopupBase, AnalyticsReporter | #30, #31 |
| W-020 | 보드 줌·팬 (`BoardCameraModel` 순수 + `BoardCameraController`), TapInput 드래그/핀치 | #32 |
| W-017 | UI 스크립트 8건 + 접착 (`LivesView`, `TutorialPresenter`+`TutorialFlow`, `MainMenu`, `LevelSelectView`/`LevelCell`, `SettingsPopup`, `ClearPopup`, `GameScreen`, `Strings`/`LocalizedText`) + 레벨 21~50 커밋 | #33 |
| W-011 | 광고(`AdsManager` 순수 + `AdsController` + `AdMobService`), 응모 코드(`RewardCode`/`RewardCodeService`/`RewardConfig`/`RewardCodePanel`), `FailPopup` | #34 |
| W-021 | v0.7.2(줌 3단 분리 + 격자 `GridOverlay`/`GridToggleButton` + `DifficultyLabel`), Android `Editor/BuildScript`, `docs/BUILD.md`·`QA_DEVICE.md`(미커밋) | #37 |

**2026-09-18 기준 남은 프로그래머 항목: W-022 (팀장이 씬을 커밋한 뒤 `Editor/SceneLintWindow` 로 빈 SerializeField 검사).** 팀장 조립이 끝나야 시작할 수 있다 — 그 전까지는 WORK.md 에 새 항목이 있는지부터 본다.

**커밋 담당이 2026-09-18 에 확정됨 (docs/TEAM.md)**: 나는 코드·테스트·패키지·SDK 만 커밋하고 `docs/` 는 add 하지 않는다 (`REPORT_PROGRAMMER.md`, `HANDOFF_PROGRAMMER.md` 만 예외). docs·레벨·씬·에셋은 디렉터가 main 에 직접 커밋한다. W-021 에서 만든 `docs/BUILD.md`·`docs/QA_DEVICE.md` 는 그래서 **untracked 로 남겨 두고 REPORT 에 커밋 요청**을 적었다.

## 2. 코드 지도 (어셈블리 → 폴더)
- `NanaArrow.Gameplay` (`Scripts/Gameplay`): 순수 규칙. `Board`, `Arrow`(경로형: cells 꼬리→머리), `FireResolver`, `TapHandler`, `LivesTracker`(`LivesChanged` 이벤트), `GameConfig`/`ArrowTypeConfig`/`ArrowViewStyle` SO. `View/` (`BoardView`, `ArrowView`, `LaneView`, `BoardLayout`), `Input/` (`TapInput`, `BoardCameraModel`, `BoardCameraController`)
- `NanaArrow.Data`: `LevelData`/`ArrowData`/`LevelLoader`/`LevelCatalog` (Newtonsoft)
- `NanaArrow.Core`: `GameSession`(순수 흐름) → `GameController`(씬 조립, `SessionStarted`/`LanePreviewShown` 이벤트, `RestartFromHud`/`RestartFromFailPopup`/`LoadNextLevel`/`GoToMain`/`Continue`), `GameEvents`(static 허브 — 애널리틱스·UI 가 구독), `LevelStats`, `App`(로케이터: `Progress`, `Interstitial`/`Rewarded`, `RewardCodes`), `PlayerProgress`+`SaveService`+`SaveCodec`(HMAC), `SettingsStore`(PlayerPrefs), `Haptics`, `SceneLoader`/`SceneId`/`BootLoader`, `AudioManager`/`SoundId`, `BackButton`, **`AdsManager`(순수)+`AdsController`**, `IInterstitialAd`/`IRewardedAd`, `RewardCode`/`RewardConfig`/`RewardCodeStatus`/`IRewardCodeService`, `AdsConfig` SO
- `NanaArrow.Services` (Core 를 참조, Core 는 Services 를 모름): `AdMobService`, `RewardCodeService`, `AnalyticsReporter`(+`AnalyticsEvents` 상수, `SafeAnalytics`), `ScreenCaptureProtection`. 전부 `RuntimeInitializeOnLoadMethod` 로 자가 생성 → `App.Set*` 로 등록
- `NanaArrow.UI`: `PopupBase`(`Current`, `closableByBack`, `ConsumedBack`), `SafeAreaAdapter`, `Strings`/`StringEntry`/`LocalizedText`, `RewardCodePanel`, `Tutorial/`(`TutorialConfig`·`TutorialStep`·`TutorialTrigger`·`FingerAnchor`·`TutorialFlow`(순수)·`TutorialPresenter`), `Game/`(`LivesView`, `ClearPopup`, `FailPopup`, `GameScreen`), `Main/`(`MainMenu`, `LevelSelectView`, `LevelCell`, `LevelCellState`, `SettingsPopup`)
- `NanaArrow.Editor`: `LevelValidator`(규칙 0~5, 2-e 포함), `LevelFiles`, `LevelValidatorWindow`, `LevelCheatWindow`(NanaArrow/Cheat/Level Jump)
- `NanaArrow.Tests.EditMode` (`Assets/_Project/Tests/EditMode`): 267개. 순수 로직은 전부 테스트가 있다 — 새 로직도 같은 방식으로
- 설정 에셋 (`Assets/_Project/Settings`): `GameConfig`, `ArrowTypeConfig`, `AdsConfig`, `ArrowViewStyle`, `LevelCatalog`(팀장이 만듦, 1~20 만 연결됨), `Strings_ko`(42키), `TutorialConfig`(6행), `RewardConfig`
- 레벨: `Assets/_Project/Levels/level_001~050.json` (기획자 소유, 나는 검증·커밋만)

## 3. 규칙·관례 (CLAUDE.md 요약 + 세션에서 굳어진 것)
- 씬(.unity)·프리팹 직접 수정 금지 → 보고에 "씬에 붙일 것" 목록. `editor_play` 는 씬을 저장할 수 있으니 플레이 후 `git status` 확인, 바뀌었으면 `git checkout -- Assets/_Project/Scenes/*.unity`
- docs/ 는 읽기만. 내가 쓰는 건 `REPORT_PROGRAMMER.md` 와 이 파일뿐. 다른 사람이 바꾼 docs 는 `chore: sync docs (...)` 로 내 브랜치에 스냅샷 커밋 (나만 git 을 씀)
- 숫자는 전부 SO 또는 `[SerializeField] private` (+ public getter). 문구는 `Strings` 키. 파일당 클래스 하나, 네임스페이스 `NanaArrow.<폴더>`
- v1 에 없는 것(구현 금지): Undo·힌트·Shuffle 부스터, 코인, 별점, 인앱결제, Bomb
- **복사 금지**(NO.1/2/3 프로젝트에서): `google-services.json`, `GoogleService-Info.plist`, `google-services.xml`, 실제 AdMob ID. 테스트 ID / 빈 값만
- 커밋 접두어 feat/fix/test/chore, 트레일러 `Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>`, PR 본문 끝 `🤖 Generated with [Claude Code](https://claude.com/claude-code)`
- 참고 프로젝트 경로: `../NANA-SpotTheDifference` (NO.3, 이식 원본), `../NANA_puzzle`, `../WaterSortPuzzle`

## 4. Unity MCP 요령 (실측)
- `eval` 은 `using` 불가 → 완전 한정 이름. private 필드는 `new UnityEditor.SerializedObject(obj).FindProperty("field")`. Awake 전에 필드를 넣으려면 GO 를 `SetActive(false)` 로 만들고 AddComponent → 필드 세팅 → `SetActive(true)`
- 플레이 루프는 에디터가 포커스를 잃으면 멈춤 → `editor_focus` 후 `wait_for(UnityEngine.Time.frameCount greaterThan N)`
- `capture_game_view(source=screen, save_path=Assets/Temp/x.png)` 로 캡처 후 `Assets/Temp` 삭제. 보고용은 `docs/screenshots/`
- 도메인 리로드 중 MCP 가 "Network error" 를 내면 20초 기다렸다 재시도
- `run_tests` 결과가 커서 파일로 떨어짐 → `sed -n 2,8p` 로 Summary 만 읽기
- 에디터가 닫혀 있으면 CLI: `/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults <xml> -logFile <log>`
- 에셋 생성은 eval 에서 `ScriptableObject.CreateInstance` + `AssetDatabase.CreateAsset`. 메모리 인스턴스가 꼬이면 `Resources.UnloadAsset` 후 다시 Load
- 씬의 `GameController.catalog` 가 비어 있는 상태로 팀장이 저장해 둔 적이 있음 → 플레이 스모크는 `gc.LoadLevel(LevelLoader.Parse(json))` 로 직접 로드 (이때 `CurrentLevel` 은 0, `Stats.Level` 이 진짜 번호)

## 5. 팀장(에디터) 쪽에 걸려 있는 것 — 코드는 준비됐고 연결만 남음
REPORT_PROGRAMMER 의 W-010·W-020·W-017·W-011 "팀장 에디터 할 일" 참조. 요약:
- Game 씬: `TapInput.config` 비어 있음(플레이 시 NRE), `GameController.catalog/boardCamera`, Main Camera 의 `BoardCameraController`, `Ads`(AdsController), HUD `LivesView`, `Tutorial`(TutorialPresenter), `Screen`(GameScreen), 팝업 4종(`ClearPopup`/`FailPopup`/ConfirmMain/`RewardCodePanel`)
- Main 씬: `MainMenu`, `LevelSelectView`+`LevelCell` 프리팹, `SettingsPopup`, Quit/Raffle 팝업
- `LevelCatalog.asset` 에 21~50 추가 (현재 20개만)
- 한글 TMP 폰트: **완료** (`Art/Fonts/NanumGothic SDF.asset`). 하트·손가락·아이콘 스프라이트는 아직 없음 (`Knob`/`UISprite` 로 임시 진행 가능)
- **2026-09-18: 위 전부를 STEP 0~8 단계별 가이드로 풀어 `REPORT_PROGRAMMER.md` 맨 위에 적어 둠** (팀장이 "어떻게 하는지 모르겠다" 고 해서). Main·Boot 씬은 빈 씬이고 프리팹은 0개 — 참조 연결이 아니라 UI 를 통째로 만드는 작업임
- 출시 전: AdMob 실제 앱 ID·광고 단위 ID (`AdMobService` 상수 + `GoogleMobileAdsSettings.asset`), Firebase 새 프로젝트의 `google-services.json`, EDM Force Resolve, `cellWidthFraction` 0.052→0.07 검토(레퍼런스 대비 보드가 작음)

## 6. 디렉터 확인 대기 중인 가정 (REPORT 에 적어 둠, 답이 오면 코드 조정)
- 튜토리얼 `hideDelay` 는 `hideOn == None` 항목에만 적용 (hideOn 있는 항목은 트리거까지 유지)
- 실패 횟수 = 앱 실행 후 그 레벨의 실패 팝업 횟수 (재시작하면 0). 이어하기 소진 후 재실패 → 이어하기 버튼 숨김
- `raffle.status.*` 6키를 UI_FLOW §9 에 반영 요청
- `UIConfig` SO 는 안 만들고 컴포넌트별 SerializeField 로 둠
- iOS 는 v1 아님 → ATT 브릿지·Plugins/iOS 미이식

## 7. 알려진 함정
- `PopupBase.Open()` 은 다른 팝업을 닫고 연다 → 연속 팝업(응모 → 클리어)은 `RewardCodePanel.OpenThen(next)` 패턴
- `BackButton.Pressed` 는 팝업과 화면이 같은 프레임에 받으므로 화면 핸들러는 `PopupBase.ConsumedBack` 을 먼저 본다
- `AdMobService`/`RewardCodeService`/`AnalyticsReporter`/`BackButton` 은 씬 배선 없이 자가 생성 — 테스트 씬을 직접 Play 해도 존재한다
- 에디터에서 Firebase 네이티브가 없어 초기화가 항상 실패 → 응모 코드는 로컬 발급(정상). Firestore 저장은 실기에서만
- `AnalyticsReporter.sendInEditor` 기본 끔 — 에디터에서 이벤트가 안 나가는 게 정상
- 저장 파일 `persistentDataPath/save.json` 이 팀장 기기에 있음(최고 레벨 4) → 레벨 1 튜토리얼을 보려면 치트 창 "저장 초기화"
