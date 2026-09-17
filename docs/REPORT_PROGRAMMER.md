# REPORT_PROGRAMMER.md — 프로그래머 보고
> **프로그래머(클로드 코드)만 쓴다.** 작업 하나 끝날 때마다 맨 위에 새 항목 추가. 디렉터는 읽기만.
> 형식: `### W-### 완료 (날짜) — 브랜치` / 변경 요약 / 가정·질문 / 팀장 에디터 할 일

### W-015 완료 (2026-09-17) — 브랜치 `feat/path-arrows` → **PR #30** (base main, 단독 PR)
- 테스트 **155/155 통과**, 컴파일 에러 0. `FireResolver`·`TapHandler`·`LivesTracker`·`GameSession` 로직은 바꾸지 않았고 기존 테스트가 그대로 통과함 (Lane 추가만)
- **W-010 / W-011 은 착수하지 않음**: 선행 W-015 + "스택 PR 금지" 규칙 → 이 PR 이 main 에 머지된 뒤 다음 지시에서 진행

**변경 요약 (PR)**
- **Gameplay**: `Arrow` = 꼬리→머리 순서 경로. `Head = cells[^1]`, `Tail`, `Length`. `Direction` 은 길이≥2 면 마지막 두 칸에서 계산(인자 dir 과 불일치·비인접이면 `ArgumentException` → 로더가 `FormatException` 으로 감쌈), 길이 1 이면 인자. `ArrowType.Long` 삭제, `ArrowTypeConfig` 의 long* 필드 삭제, `GameConfig.maxArrowLength`(12) 추가
- `FireResolver`: 레인 판정 동일 + **자기 몸통은 레인을 막지 않음**(GAME_RULES §0 "다른 Arrow"). `FireResult.Lane` / `TapResult.Lane`(머리 앞 빈 칸 목록, `FreeCells = Lane.Count`). `GameSession.Preview(arrow)` (길게 누르기용, 상태 변화 없음)
- **Validator 규칙 2 교체**: (a) 이웃 셀 상하좌우 인접 (b) 자기 겹침 없음 (c) 길이≥2 면 dir = 마지막 세그먼트 (d) 길이 ≤ maxArrowLength. 규칙 1 은 서로 다른 Arrow 끼리만 겹침 검사. `LevelRule.LongShape` → `PathShape`
- **뷰 재작성**: `ArrowView` = `LineRenderer` 폴리라인(둥근 꺾임 `cornerVertices`·둥근 꼬리 `capVertices`) + 머리 삼각 화살촉 + 머리 위 타입 아이콘(Frozen 얼음/Locked 자물쇠/Key 원, 임시 도형). Fire = 경로+레인 연장 폴리라인 위의 창(window)을 `fireSpeedCellsPerSec` 로 밀어 머리는 직진·몸통은 경로를 따라 이동, 꼬리가 나가면 파괴. Block = `LaneView` 빨간 번쩍(`laneFlashDuration`) + 머리 살짝 튕김. Marked = 선·머리 전체 빨강. 미리보기 = 선 파란 강조 + 레인 하이라이트
- `BoardView`: **격자·셀 배경 제거**, 보드 폭 = 카메라 폭 × `areaWidthFraction`(기본 0.5), 세로 중앙. `Build()` 에서 `camera.ResetAspect()` 후 계산
- `TapInput`: 탭은 **놓을 때** 확정, `longPressSeconds` 이상 누르면 `LanePreviewRequested(cell)` → 놓으면 `LanePreviewReleased` (미리보기로 시작된 누름은 탭으로 세지 않음 = 목숨 차감 없음). Arrow 의 어느 셀을 눌러도 히트 (Board 가 모든 셀을 매핑)
- `ArrowViewStyle` 전면 개편: `lineColor`(#141A33), `lineWidthCellRatio` 0.15, `cornerVertices`/`capVertices` 8, `arrowHeadLength/WidthCellRatio` 0.45, `markedColor`, `laneFlashColor`, `lanePreviewColor`, `previewLineColor`, `laneWidthCellRatio`, 아이콘 크기·색·스프라이트, 정렬 순서. 스프라이트/머티리얼 비우면 임시 도형 / Sprites-Default
- `GameConfig`: `fireDuration` → **`fireSpeedCellsPerSec`(24)** 로 교체(경로 길이가 달라 시간보다 속도가 맞음 — §8 문서 갱신 요청), `laneFlashDuration` 0.35, `longPressSeconds` 0.35, `cellGap` 기본 0 (선이 이어져야 하므로). `GameConfig.asset` 도 같이 갱신함

**스크린샷 (Game 뷰 1080×1920, 레퍼런스 `docs/reference/ref _lv4.jpeg` 와 비교)**
- `docs/screenshots/W-015_path_arrows_9x16.png` — 6×6 테스트 레벨 정지 상태 (남색 선·둥근 꺾임·화살촉·격자 없음·보드 폭 ≈ 화면 절반)
- `docs/screenshots/W-015_fire_midflight.png` — Fire 중간 포즈: 머리는 오른쪽 레인으로 직진(보드 밖까지), 몸통은 꺾인 경로를 따라 올라옴
- `docs/screenshots/W-015_lane_preview.png` — 길게 누르기: 선 파란 강조 + 레인 하이라이트
- 배경이 순백이 아닌 건 씬 카메라 배경색(팀장 설정) 때문 — 아래 할 일 3
- 레퍼런스 대비 차이: 머리 삼각형이 조금 작고 선이 약간 굵어 보임 → `ArrowViewStyle` 값으로 조정 가능 (화살촉 0.45→0.55, 선 0.15→0.13 정도 제안)

**"셀 4:1 찌그러짐" 조사**
- 지금 에디터에서는 재현 안 됨 (Game 뷰 1080×2340, `camera.aspect` 0.462 정상). `BoardLayout` 은 가로·세로에 같은 Scale 을 쓰므로 레이아웃 자체로는 비정사각이 나올 수 없음
- 유력 원인: MCP `capture_game_view`(기획자/제 캡처)가 다른 해상도로 렌더하면서 `Camera.aspect` 를 덮어쓴 채 남김 → 이후 Game 뷰 비율과 달라져 화면 전체가 늘어남. `BoardView.Build()` 에서 `ResetAspect()` 로 방어해 둠. 재발하면 인스펙터에서 Main Camera 를 잠깐 껐다 켜거나 Game 뷰 비율을 바꿔 보면 풀림

**기획자 레벨 확인**: W-016 으로 다시 쓴 `Levels/level_001~020.json` 20개를 새 검증기로 돌림 → **20/20 통과, solution·minTaps 모두 파일 값과 일치**. 파일은 기획자 작성본 그대로이며 이 브랜치에 `chore: sync levels` 커밋으로만 실었음 (내용 수정 없음)

**가정 (디렉터 확인)**
- 자기 몸통이 레인 위에 있어도 막지 않음 (§0 문구대로). 연출상 머리와 꼬리가 같은 칸을 스치는 순간이 생길 수 있음 — 레퍼런스와 다르면 규칙 4 에 "자기 레인 위에 몸통 금지" 를 추가하면 됨
- 길이 1 Arrow 는 머리 뒤로 반 칸짜리 짧은 선으로 표시
- Frozen/Locked/Key 표시는 머리 위 임시 아이콘 (원/자물쇠/원). 아트 오면 `ArrowViewStyle` 스프라이트 교체
- HUD(하트·뒤로가기·다시하기)는 W-015 범위 밖 (이슈 #13)

**팀장 에디터 할 일**
1. `Input` 오브젝트의 **TapInput → Config 에 `GameConfig` 연결** (새 필드, 비어 있으면 탭 시 NullReference)
2. `Board` 오브젝트의 **BoardView → Area Width Fraction 을 0.5 로** (씬에 이전 기본값 0.9 가 저장돼 있어 보드가 너무 큼)
3. Main Camera → Background 를 **순백 #FFFFFF** 로 (GAME_RULES §0)
4. `Settings/ArrowViewStyle.asset` 인스펙터 확인: 필드가 새로 바뀌었음 (Line Color, Line Width Cell Ratio 등). 기본값 그대로 두면 레퍼런스와 비슷함. 재생성 불필요
5. `Settings/GameConfig.asset` 은 제가 갱신함 (Fire Speed Cells Per Sec, Lane Flash Duration, Long Press Seconds, Cell Gap 0). 인스펙터에서 확인만
6. Play → 탭으로 발사, **화살표를 0.35초 이상 누르고 있으면** 레인 미리보기, 놓으면 해제. Level Jump 치트로 기획자의 새 1~20 레벨 확인
7. Game 뷰를 9:16 으로 두고 레퍼런스와 나란히 보며 `ArrowViewStyle` 값 조정 (선 굵기·화살촉 비율·색)

### W-005 + W-006 완료 · W-009 조사 보고 (2026-09-17) — 브랜치 `feat/board-view`(PR #28) → `feat/level-editor-tools`(PR #29, 스택)

**⚠ 팀장 조치 필요 — PR #1 머지가 안 됐습니다.** 자동 모드 권한 분류기가 "리뷰 없는 머지"로 `gh pr merge` 를 차단했습니다. 아래 중 하나로 처리해 주세요:
- 터미널에서 `! gh pr merge 1 --merge --delete-branch` (또는 GitHub 웹에서 Merge), 이후 `git checkout main && git pull`
- 다음 지시에 "PR 머지 허용" 을 명시하면 제가 시도합니다 (권한 프롬프트가 뜰 수 있음)
- 이번 브랜치들은 PR #1 위에 스택으로 올렸으므로 PR #1 → PR #28 → PR #29 순서로 머지하면 됩니다. GitHub 이 앞 PR 머지 시 base 를 자동으로 main 으로 옮깁니다

**GitHub 이슈 등록 (W-005)** — ISSUES.md 21+5건 등록 완료. PR #1 이 번호 1을 써서 **실제 번호 = 초안 번호 + 1** (초안 #1→#2 … #21→#22, P1~P5 → #23~#27). 라벨 `logic/view/editor/data/ads/release/planning` + `P0/P1/P2`, 마일스톤 M1/M2/M3 생성.
- 바로 close: #2(기본 구성), #3(Gameplay 코어). PR #1 본문에 `Closes #4, #5` 연결 (머지 시 자동 close), #6(GameSession) 은 `feat/board-view` 에서 마무리
- 광고 SDK 이슈(#18)는 "AdMob + Unity Ads" 결정을 본문에 반영해 등록

---
#### W-005 보드 뷰 (Game 씬) — `feat/board-view`, 테스트 129
- `Gameplay/View/BoardLayout` (순수 C#): 카메라 영역 × 비율 안에 보드를 중앙 배치. `cellSize` 는 **최대** 크기(큰 보드는 축소, 작은 보드는 확대하지 않음). 셀↔월드 변환
- `Gameplay/View/BoardView`: `Build(Board)` 로 셀·ArrowView 생성(좌하단부터 대각선 등장 스태거), `Play(arrow, TapResult)` 로 Fire/Bounce/IceBreak/Shake 재생, `Refresh(board, lives)` 로 Marked/Locked 표시 동기화. `IsFiring` 으로 `allowInputDuringFire=false` 지원
- `Gameplay/View/ArrowView`: 몸통 + 화살촉 삼각형(방향 회전), Long 은 칸 수만큼 늘림, Frozen 얼음 레이어(남은 얼음만큼 알파), Locked 자물쇠, Key/Locked 는 keyGroup 등장 순서별 색. 연출 시간은 전부 GameConfig
- `Gameplay/View/ArrowViewStyle` (SO): 색·스프라이트·비율·정렬. **스프라이트 필드를 비우면 코드 생성 임시 도형**(`PlaceholderSprites`: 사각형·삼각형·자물쇠) — 아트 오면 SO 에서 교체
- `Gameplay/Input/TapInput`: Input System `Pointer.current` 눌림 → 월드 → 셀 → `CellTapped` 이벤트 (마우스·터치 공용)
- `Core/GameSession` (순수 C#): LevelData → Board/LivesTracker/TapHandler. `Tapped/Cleared/Failed` 이벤트, `TapAt(cell)`. 목숨은 `level.lives ?? GameConfig.maxLives`
- `Core/GameController` (MonoBehaviour, 조립 지점): TextAsset 레벨 로드 → GameSession → BoardView/TapInput 연결. `levelCleared`(clearPopupDelay 뒤)·`levelFailed` UnityEvent 로 팝업 연결. 광고 판단 없음
- GameConfig 추가값 (§8 미기재 → 디렉터 확인): `cellSpawnDuration` 0.15, `iceBreakDuration` 0.15, `lockShakeDuration` 0.2, `lockShakeDistance` 0.08셀. Block 튕김은 "막은 Arrow 직전까지(FreeCells) + blockBounceDistance" 전진 후 복귀, 한 방향에 blockBounceDuration
- 플레이 모드 스모크 테스트(씬 저장 없이 임시 오브젝트)로 LEVEL_FORMAT 예시 렌더링·Block 빨간 표시·Exit 후 Marked 해제·Key Exit 후 자물쇠 해제·얼음 깨짐 확인함 (스크린샷은 커밋하지 않음)

#### W-006 에디터 툴 — `feat/level-editor-tools` (base: feat/board-view), 테스트 132
- `LevelLoader.ToJson`: 저장용 직렬화. cells/solution 은 한 줄, 생략 필드는 안 씀, enum 은 이름 (`InlineArrayConverter`)
- **NanaArrow → Level Validator**: `Assets/_Project/Levels/*.json` 전체 검증, 규칙별 오류 표시, "통과한 N개에 solution·minTaps 기록 후 저장" (실패 레벨은 저장 안 함)
- **NanaArrow → Cheat → Level Jump**: 플레이 모드에서 번호 입력/이전/다음/다시하기 → `GameController.LoadLevel`
- **레벨 1~20 파일 반영 (이슈 #8)**: LEVEL_DESIGN v0.3 §6·§7 의 JSON 을 `Levels/level_001~020.json` 으로 저장 후 검증기로 정규화 저장. **20개 전부 통과, 검증기 solution·minTaps 가 문서 값과 전부 일치** (기획 검증 재현). W-004 항목이라 범위를 넘었으면 이 커밋(`0d5a6f9`)만 빼면 됩니다

**가정 (디렉터 확인)**
- 이슈 #7 의 "즉시 클리어 · 목숨 채우기" 치트는 W-006 지시에 없어 미구현 (레벨 점프만)
- 레벨 로드는 당장 `GameController.levelJson`(TextAsset) 한 개. 레벨 선택·진행(#15·#16) 때 `Resources` 또는 레벨 카탈로그 SO 로 바꿔야 함 — Android 런타임은 `Assets/_Project/Levels/` 를 직접 못 읽음
- `TapInput` 은 UI 위 탭을 아직 거르지 않음 (HUD #13 붙일 때 `EventSystem.IsPointerOverGameObject` 추가)

**팀장 에디터 할 일 (Game 씬)** — 씬은 제가 안 건드렸습니다. 스모크 테스트 중 URP 가 카메라에 `UniversalAdditionalCameraData` 를 붙여 저장했길래 되돌렸습니다(팀장님이 씬 저장하면 자연히 다시 생김, 정상)
1. **Settings 에 `ArrowViewStyle` 에셋 생성**: Project 창 `Assets/_Project/Settings` 우클릭 → Create → NanaArrow → Arrow View Style (기본값 그대로 OK)
2. `GameConfig` 인스펙터에 새 값 4개(Cell Spawn Duration, Ice Break Duration, Lock Shake Duration/Distance) 표시 확인
3. Game 씬에 빈 오브젝트 **`Board`** 생성 → `BoardView` 추가 → Config=GameConfig, Style=ArrowViewStyle, Target Camera=Main Camera(비우면 Camera.main). 위치는 보드 중심(0,0,0 권장, HUD 자리만큼 아래로 내려도 됨). Area Height Fraction(0.6)이 HUD 공간을 뺀 세로 비율
4. 빈 오브젝트 **`Input`** → `TapInput` 추가 → Board View=Board
5. 빈 오브젝트 **`GameController`** → `GameController` 추가 → Game Config, Arrow Type Config, Board View, Tap Input 연결, **Level Json 에 `Levels/level_001.json` 드래그**
6. Main Camera 는 Orthographic 유지 (Size 8 기준으로 5×5 가 셀 1유닛으로 딱 맞음). 배경색 자유
7. Play → 탭으로 화살표 발사 확인 → 메뉴 **NanaArrow → Cheat → Level Jump** 로 1~20 이동해 보기. **NanaArrow → Level Validator** 로 검증 창 확인
8. `levelCleared` / `levelFailed` UnityEvent 는 팝업(W-007/#14) 나오면 연결. 지금은 비워 둬도 됨

---
#### W-009 이전 프로젝트 조사 (읽기만, 코드 이식 없음)

**0. 세 프로젝트 한눈에**
| | NANA_puzzle (Block Fill, NO.1) | WaterSortPuzzle (NO.2) | **NANA-SpotTheDifference (NO.3, 미완)** |
|---|---|---|---|
| Unity | 6000.0.77f1 | 6000.3.13f1 | **6000.3.20f1 (우리와 동일)** |
| 마지막 커밋 | 2026-08-24 | 2026-09-01 | **2026-09-14 (최신)** |
| 구조 | Assets/Scripts 단일, asmdef 없음 | Core/Game/Data/UI asmdef, EditMode 테스트 | **_Project/Scripts/{Core,Data,Gameplay,Services,UI,Editor} asmdef + 테스트 14파일 (우리 규칙과 같음)** |
| GMA 플러그인 | 11.2.0 | 11.2.0 | 11.2.0 |
| Unity Ads 어댑터 | 4.19.0.0 (unity-ads 4.19.0) | 동일 | 동일 |
| Firebase | 13.13.0 (App·Analytics·Auth·Firestore) | 13.13.0 (+Crashlytics) | 13.13.0 (+Crashlytics) |
| Firebase 프로젝트 | `nana-53f2e` / com.nanaBox.NANApuzzle | `nana-no2` / com.nanabox.watersortpuzzle | `nana-std` / com.nanabox.nanaspot |
| AdMob 앱 ID | 실제 (`~4278956669` / `~9230533462`) | 실제 (`~8454290804` / `~6304172559`) | **테스트 ID 그대로**, 광고 단위 실제 ID 빈 문자열 |

→ **최신 = SpotTheDifference.** 광고·응모 코드는 WaterSort 를 원본으로 NO.3 에서 한 번 더 정리한 상태라 NO.3 를 1순위로 보면 됨. NO.3 `_handoff/reference_prev_projects.md` 에 NO.1·NO.2 비교와 이식 순서가 이미 정리돼 있음 (아래 내용과 일치).

**1. 광고 — AdMob 단독 호출 + Unity Ads 는 AdMob 미디에이션 (세 프로젝트 공통)**
- 코드는 `GoogleMobileAds.Api` 만 씀. `UnityEngine.Advertisements` 직접 호출 없음, `com.unity.ads` 패키지 없음. Unity Ads 는 `Assets/GoogleMobileAds/Mediation/UnityAds/` 어댑터 폴더 + `mainTemplate.gradle` 의존성(`com.google.ads.mediation:unity:4.19.0.0`, `com.unity3d.ads:unity-ads:4.19.0`)으로만 존재. Game ID/Placement 는 코드에 없고 **AdMob 콘솔 미디에이션 그룹**에서 매핑 (WaterSort 기준 Game ID Android 800195956 / iOS 800195957, Placement `Interstitial_Android` 등 — 새 게임은 새로 발급)
- 호출 코드 위치
  - NO.2: `Assets/Scripts/Game/AdManager.cs` (287줄, 싱글턴·DontDestroyOnLoad). `ShowInterstitialGated(onProceed)`(3회마다 1회, 상수), `ShowRewarded(onRewarded)`. **콜백을 광고보다 먼저 실행**하는 구조(광고 close 이벤트가 실기에서 안 오는 문제 회피) — 리셋처럼 "잃을 게 없는 보상" 전용
  - NO.3: `Assets/_Project/Scripts/Services/AdMobService.cs` (390줄) + `IStageInterstitial`/`IRewardedAdSource` 인터페이스 + `Core/InterstitialGate.cs`(순수 C#, 테스트 있음). `RuntimeInitializeOnLoadMethod` 로 자가 생성(씬 배선 불필요), `Awaitable<bool> ShowRewardedAsync()` 는 **`OnUserEarnedReward` 가 왔을 때만 true** (보상이 재화라 순서를 안 뒤집음), 콜백은 volatile 플래그만 세우고 처리는 Update(메인 스레드), 에디터는 `#if UNITY_EDITOR` 로 광고 스킵(전면=즉시 진행, 보상형=지급 처리)
- ID 위치: 앱 ID = `Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset` (`adMobAndroidAppId`/`adMobIOSAppId`) + `Plugins/Android/GoogleMobileAdsPlugin.androidlib` 매니페스트. 광고 단위 ID = 각 AdManager/AdMobService 상단 `const` (`#if UNITY_ANDROID/IOS`, `Debug.isDebugBuild` 로 테스트/실제 자동 분기). 게시자 계정 `pub-3079888946602647` 공통
- 실기 함정 (NO.2 CLAUDE.md 에 기록): AdMob 콜백은 메인 스레드가 아님 → 콜백 안 Unity API 예외가 조용히 삼켜짐. `Debug.isDebugBuild` 도 메인 스레드 전용. Custom Main Manifest 켜지 말 것. 새 앱은 등록 후 24~72시간 no-fill

**2. 응모 코드 (100단계 클리어)**
- NO.3: `Core/RewardCode.cs`(순수 C#: `Generate(rng)`, `IsWellFormed`, 테스트 있음) + `Services/RewardCodeService.cs`(Firestore) + `UI/RewardCodePanel.cs`. NO.2: `Game/RewardManager.cs`(385줄) + `Game/RewardPopup.cs`. NO.1: `Core/RewardManager.cs`
- 생성 규칙 (세 프로젝트 동일): charset `ABCDEFGHJKLMNPQRSTUVWXYZ23456789` (0/O/1/I 제외 32자), `XXXX-XXXX` 8자 + 하이픈, `System.Random`. 시드 없음(순수 난수) — 기기별 중복은 Firestore `rewards/{deviceId}` 문서로 막음
- 저장: 로컬 `PlayerPrefs`(`rewardCode`, `rewardCodeSynced`) 가 원본, Firestore 는 웹 검증용 사본. `rewards/{deviceId}` + `code_index/{code}` 를 WriteBatch 로 원자 커밋. 초기화·조회·저장 각각 타임아웃(12/10/10초) 후 로컬 폴백, 미동기화면 앱 시작마다 백그라운드 재시도. `CheckAndFixDependenciesAsync` 중복 호출 예외를 30회 재시도로 우회
- 홈페이지: **`https://nanabox.co.kr/reward-claim.html`** (팝업 "상품 받으러 가기" → `Application.OpenURL`, 코드 복사 버튼). 웹 검증 쪽 코드(Firestore rules·Cloud Functions)는 Unity 저장소 어디에도 없음 → 홈페이지 저장소는 별도 확인 필요
- Firebase: Analytics 는 `SafeAnalytics.LogEvent` 래퍼(에디터 네이티브 누락 시 예외 삼킴), Crashlytics 는 `AppBootstrap` 에서 `ReportUncaughtExceptionsAsFatal`. 프로젝트는 게임마다 분리 → **NANA-Arrow 용 새 Firebase 프로젝트 + google-services.json/GoogleService-Info.plist 필요** (NO.2 사고: 폴더 복사 시 `FirebaseApp.androidlib/res/values/google-services.xml` 이 옛 프로젝트 값으로 남아 실기 데이터가 NO.1 DB 로 흘러감 → 이식 시 이 XML 반드시 확인)

**3. 최신 순서**: NO.3 > NO.2 > NO.1. SDK 버전은 셋 다 같아서 `Assets/GoogleMobileAds`, `Assets/Firebase`, `Assets/ExternalDependencyManager`, `Plugins/Android/*.androidlib`, gradle 템플릿 3종은 NO.3 것을 그대로 복사하면 됨 (ID·google-services 파일은 제외)

**4. NANA-Arrow 이식 시 어셈블리·네임스페이스 제안**
- 새 asmdef **`NanaArrow.Services`** (Core ↔ UI 사이, `Scripts/Services/`): AdMob·Firebase·Analytics 같은 외부 SDK 의존은 전부 여기. 참조: `GoogleMobileAds`(precompiled dll 들), `Firebase.*` dll, `NanaArrow.Gameplay`(AdsConfig 값), `NanaArrow.Core` 는 Services 를 참조하지 않음 → Core 의 GameController 가 인터페이스만 보게 하려면 인터페이스는 Core 에 두고 구현만 Services 에
  - `NanaArrow.Core`: `AdsManager` 판단 로직(순수 C#, 이슈 #17) — NO.3 `InterstitialGate` 를 AdsConfig 값(adFreeLevels·interstitialEveryNLevels·interstitialAfterFails·maxContinues) 으로 확장. `IInterstitialAd`/`IRewardedAd` 인터페이스
  - `NanaArrow.Services`: `AdMobService`(NO.3 `AdMobService.cs` 거의 그대로 — 클래스명·네임스페이스·ID 상수만 교체), `RewardCodeService`(NO.3 그대로), `SafeAnalytics`(그대로), `ScreenCaptureProtection`(그대로)
  - `NanaArrow.Gameplay`: `RewardCode` 순수 로직은 Gameplay 보다 Core 가 맞음 (SDK 무관, 테스트 이식)
  - `NanaArrow.UI`: `RewardCodePanel`(NO.3) 은 uGUI 라 UI_FLOW 확정 후 스타일만 맞춰 이식
- CLAUDE.md 폴더 규칙에 `Scripts/Services : 외부 SDK 연동(광고·Firebase·애널리틱스)` 한 줄 추가가 필요 (문서는 디렉터 몫)

**5. SpotTheDifference 공용 코드 이식 판정**
| 항목 | 파일 (NO.3 `Assets/_Project/Scripts/`) | 판정 | 비고 |
|---|---|---|---|
| 씬 전환 Boot | `Gameplay/BootLoader.cs` (46줄) | 수정 필요 | Addressables 초기화 대기 포함 → 우리는 Addressables 안 쓰므로 그 부분 제거하면 10줄. 씬 이름은 SerializeField |
| 씬 전환 Main→Game / Game→Main | `UI/MainMenuController.cs`, `Gameplay/GameFlowController.cs` 안의 `SceneManager.LoadScene` | 새로 짜는 게 나음 | 코인 투입구·스테이지 팩 등 NO.3 전용 로직과 섞여 있음. NO.2 `UI/SceneLoader.cs`(32줄, static) 가 더 단순 — 이걸 `NanaArrow.Core.SceneLoader` 로 (씬 이름 enum 화) |
| 저장 시스템 | `Services/SaveService.cs`(80) + `SaveCodec.cs`(104, HMAC-SHA256) + `SaveData.cs`(68) | **그대로** (SaveData 필드만 교체) | JsonUtility + persistentDataPath + HMAC 무결성. 우리 저장 항목(최고 레벨·사운드·응모 코드 발급 여부)은 재화가 아니라 HMAC 이 과할 수 있으나 테스트 포함이라 그대로 가져오는 게 싸다. `SaveServiceTests`/`SaveCodecTests` 같이 |
| 설정(사운드 토글) | NO.3 에 없음. NO.2 `UI/SettingsManager.cs`(36줄, PlayerPrefs static) | 수정 필요 | PlayerPrefs 대신 위 SaveData 에 넣거나, 그대로 PlayerPrefs 로 두는 결정 필요 (설정은 재화 아님 → PlayerPrefs 유지 추천) |
| Safe Area | NO.3 에 없음(HUD 스트립을 고정 px 로 계산). NO.2 `UI/SafeAreaAdapter.cs`(42줄), NO.1 `UI/SafeAreaTopOffset.cs` | 그대로 (NO.2 것) | RectTransform 앵커를 `Screen.safeArea` 로 맞추는 표준 컴포넌트 |
| 오디오 매니저 | NO.3 에 없음. NO.2 `Game/AudioManager.cs`(63줄) | 수정 필요 | 튜브 게임 전용 클립 이름들 → 우리 사운드 리스트(UI_FLOW 사운드 항목) 로 교체. 구조(AudioSource 1개 + 클립 SerializeField + SettingsManager 체크) 는 유지 |
| 팝업 베이스 | NO.3 에 없음(오버레이를 HUD 에 직접). NO.2 `Game/PopupHelpers.cs`(211줄) + `MenuPopup.cs`, `LegacyUIBuilder.cs` | 새로 짜는 게 나음 | NO.2 는 UI 를 코드로 생성하는 방식(`LegacyUIBuilder`). 우리는 팀장이 에디터에서 프리팹/씬 UI 를 만드는 흐름이라 코드 생성 팝업은 안 맞음 → `PopupBase`(열기/닫기/버튼 이벤트) 만 새로, 문구는 UI_FLOW |
| 뒤로가기 | NO.2 `Game/BackButtonHandler.cs`(151줄) | 수정 필요 | Input System `Keyboard.escapeKey` 처리 부분(상단 30줄)만. 종료 확인 팝업은 위 PopupBase 로 |
| 업데이트 체크 | NO.2 `Game/UpdateChecker.cs`(182줄) | 보류 | `https://nana-no2.web.app/version.json` 정적 JSON 조회. 출시 단계에서 결정 |
| 스크린 캡처 방지 | NO.3 `Services/ScreenCaptureProtection.cs` | 그대로 | Android FLAG_SECURE / iOS isCaptured 폴링 |
| 애널리틱스 래퍼 | NO.3 `Services/SafeAnalytics.cs` | 그대로 | 이벤트 목록은 기획 P4(#26) 뒤 |

**절대 복사 금지**: `google-services.json`, `GoogleService-Info.plist`, `GoogleMobileAdsSettings.asset` 의 앱 ID, `FirebaseApp.androidlib/res/values/google-services.xml`, 각 AdManager 의 실제 광고 단위 ID — 전부 NANA-Arrow 용으로 새로 발급 (팀장: AdMob 콘솔 앱 2개·광고 단위 4개·미디에이션 그룹, Unity Ads Game ID, Firebase 새 프로젝트)

### W-002 + W-003 완료 보고 (2026-09-17) — 브랜치 `feat/level-data-validator`
- `feat/gameplay-core` 는 main 에 fast-forward 머지 후 origin 푸시 완료 (`d22312f`)
- 테스트 **110/110 통과** (Unity MCP EditMode), 컴파일 에러 0

**변경 요약 (PR)**
- **Data**: `LevelData` / `ArrowData` — LEVEL_FORMAT v0.3 과 1:1, 필수 필드 누락·미지 enum(`Bomb`)·version≠1 은 파싱 단계에서 예외. `LevelLoader.Parse(json)` → `CreateBoard(level, frozenDefaultHits)` (`CreateArrow`, `ToCells` 도 public). 파일 I/O 는 호출자 몫
  - `Newtonsoft.Json 3.2.2` 를 manifest 직접 의존성으로 추가 (`JsonUtility` 는 `[[x,y]]` 중첩 배열을 못 읽음)
- **Editor**: `LevelValidator.Validate(level, GameConfig, ArrowTypeConfig)` → `LevelValidationResult { Errors[(LevelRule, Message)], Solution, MinTaps }`
  - 규칙 0 스키마(보드 `minBoardSize`~`maxBoardSize`, id 고유, 셀 `[x,y]` 형식, Frozen hits ≥ 2) → 1 범위·겹침 → 2 Long 직선·연속·축 일치·길이 `longMin~Max` (Long 외 타입은 1칸) → 3 Locked/Key keyGroup 및 Key 존재 → 4 탐욕 시뮬레이션(경로 비고 잠기지 않은 Arrow 를 레벨 순서대로 반복 Exit) → 5 `Solution` + `MinTaps = Σhits`
  - `LevelValidator.Record(level, result)` 가 규칙 5·6: `solution` 과 `meta.minTaps` 덮어쓰기, `meta` 의 다른 키는 보존
  - LEVEL_FORMAT v0.3 예시 → solution `[a1,a5,a4,a2,a3]`, minTaps 6 재현. v0.1 예시 → Solvable 실패 재현
- **Gameplay (v0.5.1)**: `Arrow.Hits` / `Arrow.KeyGroup` 추가(기존 생성자 유지), `Board.IsLocked(arrow)` (같은 keyGroup 의 Key 가 보드에 남아 있으면 잠김 — Validator·TapHandler 공용)
  - `TapHandler(board, lives).Tap(arrow)` → `TapResult { Outcome, FreeCells, BlockedBy, LifeLost, RemainingHits }`
  - `Outcome`: `Exit` / `Blocked`(목숨·Marked 는 LivesTracker 규칙) / `IceBroken`(Frozen 얼음 탭: 경로 무관·목숨 차감 없음, 남은 탭 수 반환) / `Locked`(잠긴 Locked 탭: 목숨 차감 없음, 흔들림용) / `Ignored`(보드에 없는 Arrow 또는 목숨 0)
  - Frozen 은 마지막 탭만 Block 판정. Block 돼도 얼음은 다시 얼지 않음
- **Config**: `GameConfig.coinPerClear` 제거, `minBoardSize`(3) 추가. `ArrowTypeConfig.frozenDefaultHits` 최소 2. `ArrowType.Key` 는 W-001 에 이미 포함
- **테스트**: `LevelLoaderTests` 12, `LevelValidatorTests` 27, `TapHandlerTests` 17, `ArrowTests`/`BoardTests` 보강 → 총 110

**가정 (기획자 확인 요청)**
- Long 이 아닌 타입(Basic/Frozen/Locked/Key)은 정확히 1칸으로 검증함 (LEVEL_FORMAT 은 Basic·Long 만 명시)
- Frozen 의 얼음 탭은 잠금과 무관하게 항상 가능 (Locked+Frozen 조합은 타입이 하나라 존재하지 않음)
- 검증기 파일 저장은 레벨 에디터/생성기 작업에서 (`JsonConvert.SerializeObject(level, Formatting.Indented)` 로 가능)

**팀장 에디터 할 일**
1. Package Manager 에 `Newtonsoft Json 3.2.2` 가 직접 의존성으로 보이는지 확인 (manifest 는 이미 수정됨, 별도 조작 없음)
2. `Assets/_Project/Settings/GameConfig.asset` 인스펙터: `Min Board Size = 3` 가 보이고 `Coin Per Clear` 가 사라졌는지 확인 (stale 값은 제거해 둠)
3. Window → General → Test Runner → EditMode → Run All → 110 통과 확인
4. 씬·프리팹 연결은 아직 없음. `TapHandler` 를 쓰는 MonoBehaviour(보드 뷰·입력)는 다음 작업
