# HANDOFF_PROGRAMMER.md — 클라이언트 프로그래머 인수인계 (2026-09-18 세션 종료 시점)
> **프로그래머(클로드 코드)만 쓴다.** 새 세션은 이 파일 → `CLAUDE.md` → `docs/WORK.md` 순서로 읽고 시작. 매 세션 끝에 이 파일을 갱신한다.
> 상세 변경 이력은 `docs/REPORT_PROGRAMMER.md` (W-### 별 보고, 최신이 맨 위). 이 파일은 "지금 상태 + 다음에 알아야 할 것" 만.

## 0. 세션 시작 체크리스트
1. `git checkout main && git pull` → `git status` / `git log --oneline -5`. 브랜치는 항상 **main 기준 하나씩** (스택 금지)
2. `docs/WORK.md` 의 **프로그래머** 항목만 처리 (WORK.md 는 디렉터만 수정 — **절대 편집 금지**). 기획자·팀장 항목은 건드리지 않음
3. 규칙 문서 (**세션 중에도 디렉터가 자주 바꾼다 — 의존하기 전에 다시 읽고, 중간에 `git pull` 하면 WORK.md 를 다시 확인**):
   `docs/GAME_RULES.md` **v0.8** / `docs/LEVEL_FORMAT.md` **v0.7** / `docs/UI_FLOW.md` v0.2 / `docs/ANALYTICS.md` v0.1 / `docs/TEAM.md`(커밋 담당) / `docs/ASSETS.md`(에셋 출처)
4. 에디터가 열려 있으면 Unity MCP: `recompile` → `console(levels=[error,warning])` → `run_tests(mode=editor, filter_type=assembly, filter=NanaArrow.Tests.EditMode)`. 현재 **321/321** (테스트 파일 27개)
5. 작업 끝: `docs/REPORT_PROGRAMMER.md` 맨 위에 보고 추가 → 커밋 → push → `gh pr create` → `gh pr merge --merge --delete-branch` → `git checkout main && git pull` → **이 파일 갱신**

---

## 1. ★ 다음에 할 일

### W-029 레벨 자동 생성기 (최우선, 브랜치 `feat/level-generator`)
GAME_RULES **v0.8** 에서 후반 레벨이 Arrow 100~200개로 커져 손 설계가 불가능해졌다. 핵심만:
- **`GameConfig` 상한을 먼저 올려야 한다**: `maxBoardWidth` 10 → **24**, `maxBoardHeight` 14 → **32**, **`maxArrowsPerLevel` 200 신설**. `LevelValidator` 규칙 0(`CheckSchema`)도 같이 갱신. **에셋 `GameConfig.asset` 값도 바꿔야 한다** (C# 기본값만 바꾸면 직렬화된 에셋은 안 바뀐다 — 여러 번 겪음)
- `Editor/LevelGenerator` **순수 C# + 테스트**: 역방향 생성(빈 보드+mask 에서 Arrow 를 "나갔던 방향의 역으로 밀어 넣기") → 정의상 해결 가능. **시드 고정 시 재현 가능**해야 함
- `GeneratorParams` SO, `Editor/LevelGeneratorWindow`(파라미터 → 생성 → 아스키 미리보기 → Validator 통과 시 저장, "다시 뽑기")
- `meta.generated=true`, `seed`, params 기록. 성능 목표: 생성 <2초, Validator <1초
- 보고에 생성 예시 3개(30/100/200개) 아스키 첨부. `QA_DEVICE.md` 에 "LineRenderer 200개 + 점 + 레인 가이드에서 60fps" 항목 추가

### W-022 씬 조립 결과 검토 (선행 조건 **이미 충족** — 씬이 커밋됨 `39968de`)
- `Editor/SceneLintWindow`: 씬을 열어 **빈 SerializeField 자동 검사**. 고치지 말고 **목록만** 보고
- 이미 이 세션에서 같은 검사를 eval 로 여러 번 했다 — 그 로직을 창으로 옮기면 된다. 아래 "의도적으로 비어 있는 필드" 는 오탐이므로 화이트리스트에 넣을 것:
  `BoardView.targetCamera`, `TapInput.targetCamera` (비우면 `Camera.main`) / `LivesView.emptySprite` (없으면 알파 30%) / `DifficultyLabel.label`, `LaneGuideToggleButton.background` (Awake 에서 자기 오브젝트에서 찾음) / UnityEvent 의 `m_ObjectArgument` (무인자 메서드라 항상 빔)

### ⚠️ 디렉터에게 답을 기다리는 것
- **`docs/LEVEL_FORMAT.md` 필드 표(65줄 근처)가 머리말과 어긋난다.** 머리말은 v0.7 `string[]` 인데 표는 아직 `mask | int[][] | 예약(v1.1) … v1 로더는 무시`. 기획자가 표를 보면 틀리게 쓴다. W-028 PR #47 에 수정안을 적어 뒀다 (내 docs 예외는 `ASSETS.md` 뿐이라 직접 못 고침)
- **`Art/FreeButtonSet` 라이선스 불명** — 폴더에 LICENSE·README 가 없다. `docs/ASSETS.md` §2 에 "확인 필요 / 교체 필요(임시)" 로 적어 뒀다. 정식 아트 교체 시 폴더 삭제 권고

---

## 2. 완료된 작업 (전부 main 머지됨)
| W | 내용 | PR |
|---|---|---|
| W-001~008 | 순수 C# 코어(Board/Arrow/FireResolver/TapHandler/LivesTracker), Data(LevelLoader), Editor(LevelValidator·치트 창), View, TapInput | #1~#29 |
| W-015 후속 + W-010 | 셀 크기 규칙, 규칙 2-e, SDK 이식(AdMob·Firebase·EDM), Save/Progress, SceneLoader, AudioManager, PopupBase, AnalyticsReporter | #30·#31 |
| W-020 | 보드 줌·팬 (`BoardCameraModel` 순수 + `BoardCameraController`) | #32 |
| W-017 | UI 스크립트 8건 + 접착 | #33 |
| W-011 | 광고(`AdsManager`+`AdsController`+`AdMobService`), 응모 코드, `FailPopup` | #34 |
| W-021 | v0.7.2(줌 3단 분리·난이도 라벨), Android `Editor/BuildScript`, `docs/BUILD.md`·`QA_DEVICE.md` | #37 |
| — | **LevelCatalog.asset 생성**(50개 연결) + 조립 가이드 실측 정정 | #38 |
| — | **씬 3개 + LevelCell 프리팹 조립 대행** (팀장 요청, CLAUDE.md 예외) | #41 |
| W-025 | 플레이 테스트 버그 **8건** (아래 §3) | #42 |
| — | 에디터 UI 뭉침(SafeArea) / 레벨 선택 뒤로가기(TMP raycast) 재수정 | #43·#44 |
| — | REPORT 에서 조립 가이드 364줄 제거, UI 함정을 이 파일 §7 로 이관 | #45 |
| W-027 | 비직사각 보드 `mask` (`BoardMask` 순수 + 로더·검증기·점·아스키 미리보기) | #46 |
| W-028 | `docs/ASSETS.md`, `UITheme` SO(코드에서 팔레트 제거), crashlytics 생성 파일 제외 | #47 |

### W-025 8건 (전부 팀장 확인 완료, 3번은 실기 빌드로)
1 Fire 가 화면 밖까지 / 2 미리보기 화면 끝까지 / 3 60fps / 4 팝업 겹침 / 5 레벨 선택 뒤로가기 / 6 난이도 라벨 정렬 / 7 `#` = **레인 가이드**(균일 격자 아님) / 8 빈 칸 점

---

## 3. 코드 지도 (어셈블리 → 폴더)
참조 방향: **Core → Gameplay/Data**, **UI·Services → Core**. **Gameplay 는 Core 를 모른다** (그래서 `SettingsStore`(Core) ↔ `BoardView`(Gameplay) 는 `GameController` 가 중계한다)

- `NanaArrow.Gameplay`: 순수 규칙 — `Board`(+**`BoardMask`**, `IsInMask`, `UsableCellCount`), `Arrow`(경로형: cells 꼬리→머리), `FireResolver`, `TapHandler`, `LivesTracker`, `GameConfig`/`ArrowTypeConfig` SO
  - `View/`: `BoardView`, `ArrowView`, `LaneView`, `BoardLayout`, `ArrowViewStyle` SO, **`LaneGuides`(순수)·`LaneGuideOverlay`·`EmptyCellDots`**
  - `Input/`: `TapInput`, `BoardCameraModel`(순수), `BoardCameraController`
- `NanaArrow.Data`: `LevelData`(+`Mask`, `Difficulty`)/`ArrowData`/`LevelLoader`/`LevelCatalog`
- `NanaArrow.Core`: `GameSession` → `GameController`(씬 조립, `CurrentLevelData`), `GameEvents`, `LevelStats`, `App`(로케이터), `PlayerProgress`+`SaveService`+`SaveCodec`, `SettingsStore`(사운드·진동·**`LaneGuideOn`**), `Haptics`, `SceneLoader`/`SceneId`/`BootLoader`(**targetFrameRate 적용**), `AudioManager`/`SoundId`, `BackButton`, `AdsManager`(순수)+`AdsController`, `IInterstitialAd`/`IRewardedAd`, `RewardCode`·`RewardConfig`·`IRewardCodeService`, `AdsConfig` SO
- `NanaArrow.Services`: `AdMobService`, `RewardCodeService`, `AnalyticsReporter`(+`AnalyticsEvents`·`SafeAnalytics`), `ScreenCaptureProtection` — 전부 `RuntimeInitializeOnLoadMethod` 자가 생성
- `NanaArrow.UI`: `PopupBase`(`Current`·`ConsumedBack`·**`CloseAll()`**), `SafeAreaAdapter`, `Strings`/`LocalizedText`, **`UITheme` SO**, `RewardCodePanel`, `Tutorial/`, `Game/`(`LivesView`, `ClearPopup`, `FailPopup`, `GameScreen`, **`DifficultyLabel`**, **`LaneGuideToggleButton`**), `Main/`(`MainMenu`, `LevelSelectView`, `LevelCell`, `SettingsPopup`)
- `NanaArrow.Editor`: `LevelValidator`(규칙 0~5, 2-e·**mask** 포함), `LevelFiles`, `LevelValidatorWindow`(**mask 아스키 미리보기**), `LevelCheatWindow`, **`BuildScript`**(NanaArrow/Build)
- `NanaArrow.Tests.EditMode`: **321개 / 27파일**. 순수 로직은 전부 테스트가 있다 — 새 로직도 같은 방식으로
- 설정 에셋 `Assets/_Project/Settings/`: `GameConfig`, `ArrowTypeConfig`, `AdsConfig`, `ArrowViewStyle`, `LevelCatalog`(50개), `Strings_ko`(45키), `TutorialConfig`, `RewardConfig`, **`UITheme`**
- 아트 `Assets/_Project/Art/`: `Fonts/NanumGothic(.otf + SDF)`, `Sprites/Heart.png`, `FreeButtonSet/`(임시). 프리팹 `Prefabs/UI/LevelCell.prefab`
- 레벨 `Assets/_Project/Levels/level_001~050.json` (기획자 소유)

---

## 4. 규칙·관례
- **커밋 담당 (docs/TEAM.md, 09-18 확정)**: 나는 **코드·테스트·패키지·SDK** 만 커밋. `docs/` 는 add 하지 않는다 — **`REPORT_PROGRAMMER.md`·`HANDOFF_PROGRAMMER.md`·`ASSETS.md`(W-028 예외) 만 허용**. docs·레벨·씬·프리팹·아트·`ProjectSettings` 는 **디렉터가 main 에 직접** 커밋
  - WORK.md 가 나에게 doc 산출물을 시키면(예: `BUILD.md`) **파일은 만들되 untracked 로 두고 REPORT 에 커밋 요청**을 적는다
- **씬·프리팹 직접 수정 금지** (CLAUDE.md). 단 **2026-09-18 에 팀장이 직접 요청해서 씬 3개+프리팹을 내가 조립했다** — 이건 1회 예외였고 REPORT 에 명시했다. 앞으로도 할지는 디렉터 판단 대기
- 숫자는 전부 SO 또는 `[SerializeField] private` (+ getter). **색은 `ArrowViewStyle`/`UITheme` 에서만** (W-028). 문구는 `Strings` 키. 파일당 클래스 하나, 네임스페이스 `NanaArrow.<폴더>`
- v1 미포함(구현 금지): Undo·힌트·Shuffle, 코인, 별점, 인앱결제, Bomb
- **복사 금지**: `google-services.json`, `GoogleService-Info.plist`, `google-services.xml`, 실제 AdMob ID, `*.keystore`/`*.jks`/`keystore.local.json`. 테스트 ID·빈 값만
- 커밋 접두어 feat/fix/test/chore, 트레일러 `Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>`, PR 끝 `🤖 Generated with [Claude Code](https://claude.com/claude-code)`
- 참고 프로젝트: `../NANA-SpotTheDifference`(NO.3, 이식 원본), `../NANA_puzzle`, `../WaterSortPuzzle`

---

## 5. Unity MCP 요령 (실측)
- `eval` 은 `using` 불가 → **완전 한정 이름** (`UnityEngine.…`). LINQ(`Select`/`Where`)와 로컬 함수는 쓸 수 있다. `Direction.ToOffset()` 같은 확장 메서드는 `NanaArrow.Gameplay.DirectionExtensions.ToOffset(dir)` 로
- private 필드는 `new UnityEditor.SerializedObject(obj).FindProperty("field")` → `ApplyModifiedPropertiesWithoutUndo()`
- 버튼 onClick·UnityEvent 연결은 `UnityEditor.Events.UnityEventTools.AddPersistentListener(evt, target.Method)`. private UnityEvent 필드는 리플렉션으로 꺼낸다
- **`Destroy()` 는 지연된다** — 같은 프레임에 `transform.Find()` 하면 **삭제 예정인 옛 오브젝트를 잡는다**. 재생성 직후 개수를 세면 틀린 값이 나온다 (실제로 한 번 속았다). `GetComponentsInChildren<T>(true)` 로 전부 세거나 다음 프레임에 확인
- **`Screen.width/height` 는 eval 안에서 부정확하다** (에디터 컨텍스트 값이 온다). 화면 기준 판정·레이캐스트를 eval 로 하면 헛짚는다 → RectTransform 의 **월드 좌표**로 비교할 것
- `capture_game_view(source=screen)` 는 **플레이 모드에서만** 된다 (Overlay UI 는 런타임에만 백버퍼에 합성됨). 에디터 상태 확인은 캡처 말고 **수치를 읽어라**
- 캡처는 등장 연출(PopIn) 중이면 비어 보인다 → 한 번 더 찍기. `Assets/Temp` 에 저장하고 끝나면 `AssetDatabase.DeleteAsset("Assets/Temp")`
- 플레이 루프는 에디터 포커스를 잃으면 멈춤 → `editor_focus`
- 도메인 리로드 중 "Network error" → 20초 기다렸다 재시도. `editor_status` 로 확인
- **코드를 고친 직후 `run_tests` 를 바로 돌리면 옛 어셈블리로 돈다** → `recompile(wait=true)` 먼저
- `run_tests` 결과는 커서 파일로 떨어짐 → `sed -n '1,9p'` 로 Summary 만
- 에디터가 닫혀 있으면 CLI: `/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults <xml> -logFile <log>`
- **C# 기본값을 바꿔도 이미 직렬화된 `.asset` 은 안 바뀐다** — SO 값을 바꾸면 에셋도 같이 고칠 것
- 비활성 오브젝트는 `GameObject.Find` 로 안 잡힌다 → `GetComponentsInChildren<T>(true)` 나 루트 순회

---

## 6. 지금 상태 / 걸려 있는 것
- **커밋 안 된 것 (디렉터 몫)**: `Assets/_Project/Scenes/Game.unity`(`UITheme` 연결), `ProjectSettings/ProjectSettings.asset`(Unity 자동 생성 배칭값)
- **팀장 에디터 할 일: 사실상 없음.** 씬 3개·프리팹·`UITheme` 연결까지 끝났다. 남은 건 아트 교체(빈 하트, 손가락, 자물쇠, 로고, 사운드 13종)와 출시 전 콘솔 작업
- **출시 전 필수**: AdMob 실제 앱·광고 단위 ID(`AdMobService` 상수 + `GoogleMobileAdsSettings.asset`), Firebase `google-services.json`, EDM Force Resolve, 서명 키(`ProjectSettings/keystore.local.json`) + **키 백업**, `FreeButtonSet` 처리, 망고보드 플랜 확인
- 팀장 기기 저장 파일: 최고 레벨 4 + **응모 코드 발급됨**(내 테스트로 발급됨 — Main 에 응모 버튼이 보이는 이유). 치트 창 "저장 초기화" 로 되돌릴 수 있음
- 현재 UI 팔레트는 `FreeButtonSet` 의 **금색**이라 GAME_RULES §9(남색+연보라)와 다르다 → **임시**이고 정식 아트 교체 시 맞춘다 (디렉터 확인 완료)

## 7. 디렉터 확인 대기 중인 가정 (REPORT 에 적어 둠)
- 튜토리얼 `hideDelay` 는 `hideOn == None` 항목에만 적용
- 실패 횟수 = 앱 실행 후 그 레벨의 실패 팝업 횟수 (재시작하면 0)
- W-025 4번을 지시("`Awake` 에서 무조건 `SetActive(false)`")대로가 아니라 **`Open/Close` 가 활성 상태를 관리**하도록 구현했다 — 지시대로 하면 `Open()` 이 오브젝트를 다시 켜지 않아 팝업이 영원히 안 뜬다
- 레인 가이드 선은 보드 좌표계라 줌·팬에 보드와 함께 움직인다
- iOS 는 v1 아님 → ATT 브릿지·Plugins/iOS 미이식

---

## 8. 알려진 함정
- `PopupBase.Open()` 은 다른 팝업을 닫고 연다 → 연속 팝업(응모 → 클리어)은 `RewardCodePanel.OpenThen(next)` 패턴
- `BackButton.Pressed` 는 팝업과 화면이 같은 프레임에 받으므로 화면 핸들러는 `PopupBase.ConsumedBack` 을 먼저 본다
- `AdMobService`/`RewardCodeService`/`AnalyticsReporter`/`BackButton` 은 씬 배선 없이 **자가 생성**
- 에디터에서 Firebase 네이티브가 없어 초기화가 항상 실패 → 응모 코드는 로컬 발급(정상), 경고 2건이 뜨는 게 정상. Firestore 저장은 실기에서만
- `AnalyticsReporter.sendInEditor` 기본 끔
- Game 씬을 직접 Play 하면 `SceneLoader.PendingLevel` 이 없어 `startLevel`(=1) 이 로드된다

### UI 함정 (2026-09-18 씬 조립하며 비싸게 배운 것)
- **팝업은 닫힘 = 비활성 + CanvasGroup 숨김 둘 다.** 예전엔 비활성으로 저장하면 `Awake` 가 안 돌아 `Open()` 이 NRE 였다. W-025 에서 `Open/Close` 가 활성 상태를 관리하게 고쳐 지금은 어느 상태로 저장돼도 동작한다. **씬에는 비활성으로 저장**(에디터에서 안 겹쳐 보이게)
- **닫혀 있는 동안 비활성인 오브젝트는 `Awake` 에서 이벤트를 구독하면 안 된다** → `OnEnable/OnDisable` 로. `PopupBase`·`LevelSelectView` 가 이것 때문에 뒤로가기가 안 됐다
- **색이 칠해진 스프라이트에 `Image.color` 를 곱하지 말 것.** Image 는 스프라이트 색 × Color 다. 금색 버튼·빨간 하트에 남색을 곱해 검은 덩어리가 되는 일을 **두 번** 겪었다. 그림이 있는 스프라이트는 Color **흰색**
- **RectTransform 앵커 프리셋은 Alt+Shift 로.** 그냥 누르면 Left/Top/Right/Bottom 이 0 이 안 돼 겉보기 stretch 인데 실제 100×100 상자가 된다 → 자식 UI 가 화면 가운데로 뭉친다. **`SafeAreaAdapter` 가 런타임에 오프셋을 0 으로 덮어써서 플레이하면 정상, 에디터에서만 깨진다**
- **TMP 의 `raycastTarget` 이 버튼 클릭을 먹는다.** 버튼을 덮도록 stretch 된 라벨이 버튼보다 나중에 그려지면 클릭이 라벨에 먹힌다(레벨 선택 `←` 버튼). 세 씬 + 프리팹의 TMP 39개는 전부 꺼 둔 상태
- **UI 는 세 층을 따로 확인한다**: (1) 로직 — 메서드 직접 호출 (2) 레이아웃 — RectTransform 값을 **에디터 상태에서** 읽기 (3) 레이캐스트 — 버튼보다 나중에 그려지며 버튼을 덮는 `raycastTarget` 이 있는지. **"메서드를 호출하니 되더라" 는 입력이 닿는다는 증거가 아니다** (W-025 5번을 이것 때문에 두 번 고쳤다)
- **씬 상태를 글(인수인계·이전 보고)로 믿지 말고 `SerializedObject` 로 직접 읽어라.** 세션 사이에 팀장이 바꾸고, 이전 세션의 기술이 실제로 여러 군데 틀렸었다
