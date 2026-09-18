# REPORT_PROGRAMMER.md — 프로그래머 보고
> **프로그래머(클로드 코드)만 쓴다.** 작업 하나 끝날 때마다 맨 위에 새 항목 추가. 디렉터는 읽기만.
> 형식: `### W-### 완료 (날짜) — 브랜치` / 변경 요약 / 가정·질문 / 팀장 에디터 할 일

### W-025 5번 재수정 (2026-09-18) — 레벨 선택 뒤로가기

**팀장 재확인 결과**: 1·2·4·6·7·8 확인됨, 3 은 빌드 후 확인 예정, **5 는 아직 안 됨**.

**내가 놓친 이유**: 지난번에 `BackButton.Pressed` 를 리플렉션으로 **직접 호출**해서 "닫힌다" 를 확인했습니다. 그건 **C# 로직만** 검증한 것이고, 실제 사용자 입력이 버튼에 **닿는지**는 검증하지 않았습니다.

**진짜 원인**: 레벨 선택 헤더의 **`Title`("레벨 선택" 글자)이 헤더 전체를 덮도록 stretch** 되어 있고, 계층상 `BackButton` **뒤에 그려져서** 클릭을 가로채고 있었습니다. TMP 의 `raycastTarget` 이 기본으로 켜져 있는 것이 원인입니다.

```
Header/BackButton   그리기 순서 48
Header/Title        그리기 순서 50  ← 버튼 위에 그려지고 버튼 영역을 덮음 (raycastTarget=true)
```

즉 **글자가 버튼을 먹고 있었습니다.** 안드로이드 뒤로가기(ESC) 경로는 원래부터 정상이었고, 화면의 `←` 버튼만 안 눌렸던 것입니다.

**수정**
- 세 씬 + `LevelCell` 프리팹의 **모든 TMP 텍스트 39개**의 `raycastTarget` 을 껐습니다. 글자는 클릭을 받을 일이 없고, 버튼은 자기 Image 로 받습니다
- 전체 버튼을 대상으로 **"버튼보다 나중에 그려지면서 버튼 영역을 덮는 raycastTarget"** 을 찾는 검사를 만들어 두 씬을 훑었습니다 → 수정 후 **가려진 버튼 0개**
- 런타임 재확인: 레벨 선택을 연 상태에서 `←` 버튼을 가리는 것 **없음**

**참고**: 팝업 내부 버튼이 서로 겹친다고 나온 건 **오탐**이었습니다. 닫힌 팝업은 `VerticalLayoutGroup` 이 돌지 않아 자식이 모두 (0,0) 에 쌓여 있어서 그렇게 보인 것이고, 열리면 정상 배치됩니다 (런타임 실측: Title 1318~1368 / Subtitle 1236~1286 / Primary 1104~1204 / Secondary 972~1072, 겹침 없음).

**교훈 (지난 SafeArea 건과 같은 계열)**: "메서드를 직접 호출해서 된다" 는 **입력이 닿는다는 증거가 아닙니다.** UI 는 (1) 로직 (2) 레이아웃 (3) 레이캐스트(무엇이 위에 있나) 세 층을 따로 확인해야 합니다.

---


### 에디터 화면 UI 뭉침 수정 (2026-09-18) — 팀장 보고

**증상**: 에디터 Game 뷰에서 HUD 버튼·하트·`#` 버튼·글자가 전부 화면 **가운데에 작게 뭉쳐** 보임. 그런데 **플레이하면 정상**.

**원인**: Game 씬 `SafeArea` 의 RectTransform 이 앵커만 stretch 이고 **오프셋이 0 이 아니었습니다.**

```
anchorMin (0,0)  anchorMax (1,1)   ← 앵커는 화면 전체
sizeDelta (-980, -2240)            ← 실제 크기는 100 x 100
```

캔버스가 1080×2340 인데 `SafeArea` 는 가운데 **100×100 상자**였고, 그 안의 자식이 전부 그 상자로 눌려 들어가 뭉친 것입니다.

**왜 플레이하면 정상으로 보였나**: `SafeAreaAdapter.Awake()` 가 런타임에 `offsetMin/offsetMax` 를 0 으로 덮어씁니다. 그래서 **에디터에서만** 깨져 보였고, 제가 플레이 모드로만 검증해서 놓쳤습니다.

**수정**
- 세 씬을 전부 훑어 "stretch 앵커인데 오프셋이 0 이 아닌" RectTransform 을 찾아 채움 처리
  - `Game/SafeArea` 100×100 → **1080×2340**
  - 의도적으로 여백을 둔 두 곳은 그대로 유지: `Main/Scroll`(상단 Header 200 자리), `Game/TutorialBubble/Text`(안쪽 여백 32)
- `Game/DifficultyLabel` 의 에디터 표시 문구가 TMP 기본값 `New Text` 였음 → **`보통`** 으로 (런타임엔 `meta.difficulty` 로 덮어씀). 스크린샷에 보이던 "New Text" 가 이것입니다
- 이 함정(앵커 프리셋은 **Alt+Shift** 로, Left/Top/Right/Bottom 이 0 인지 확인, **"플레이하면 되는데 에디터 화면이 이상하다" 면 의심**)은 `docs/HANDOFF_PROGRAMMER.md` §7 에 남겼습니다

**확인한 최종 배치 (Game 씬, 캔버스 1080×2340 기준)**

| 오브젝트 | 월드 y | 위치 |
|---|---|---|
| SafeArea | 0~2340 | 화면 전체 ✅ |
| HUD | 2140~2340 | 상단 |
| DifficultyLabel | 2260~2316 | 하트 위 (겹침 없음) |
| Hearts | 2176~2240 | 상단 중앙 |
| BackButton / RetryButton | 2180~2300 | 좌상단 |
| LaneGuideToggleButton | 40~160 (x 920~1040) | 우하단 |
| TutorialBubble | 240~390 | 하단 중앙 |

플레이로 재확인: 화살표 3개·레인 가이드 3선·빈 칸 점 12개·HUD 전부 정상, 콘솔 에러 0.

**교훈 (다음 세션용)**: 씬 레이아웃은 **플레이 모드에서만 검증하면 안 된다.** `SafeAreaAdapter` 같은 런타임 보정 컴포넌트가 저장된 잘못된 값을 가려 버린다. 에디터 상태의 RectTransform 값을 직접 읽어 확인할 것.

---


### W-025 완료 (2026-09-18) — 브랜치 `fix/playtest-1`

- 테스트 **302/302** (+14), 컴파일 에러 0, 콘솔 에러 0 (경고 2건은 에디터 Firebase 미초기화 — 알려진 정상)
- 8건 전부 **플레이 모드에서 수치로 확인**했습니다. 아래 표의 "확인" 은 추측이 아니라 실측값입니다

| # | 항목 | 상태 | 확인한 값 |
|---|---|---|---|
| 1 | Fire 가 화면 밖까지 | ✅ 수정됨 | 레벨 1 a1: 머리→보드 끝 **0칸**(예전엔 여기서 사라짐) → 머리→화면 끝 **18.3칸** + 여유 2 + 길이 6 |
| 2 | Lane 미리보기 화면 끝까지 | ✅ 수정됨 | 선 y 0.96→**8.19**, 화면 위 끝 8.00 (보드 끝은 1.15) |
| 3 | 60fps | ✅ 수정됨 | Boot 경유 시 `targetFrameRate=60`, `vSyncCount=0` |
| 4 | 팝업 겹침 | ✅ 수정됨 | Game 4개·Main 3개 전부 진입 시 `활성=False, 열림=False` |
| 5 | 레벨 선택 뒤로가기 | ✅ 수정됨 | 열림=True → 뒤로가기 → 열림=False |
| 6 | 난이도 라벨 정렬 | ✅ 수정됨 | 원인은 **`alignment=TopLeft`** (아래 설명) |
| 7 | `#` = 레인 가이드 | ✅ 교체됨 | 세로 Arrow 3개 → 선 3개, 1개 Exit → **선 2개** |
| 8 | 빈 칸 점 | ✅ 추가됨 | 5×6 보드 Arrow 3개(18칸) → 점 **12개**, 1개 Exit 후 **18개** |

**변경 요약 (PR)**

**1·2. 화면 밖까지 (`BoardView` + `ArrowView` + `LaneView`)**
- 원인: 이동량을 **보드 레인 길이**로 계산해서, 머리가 이미 보드 가장자리면 이동량이 0이 됐습니다 (레벨 1 이 정확히 그 경우 — 그래서 "가장자리에서 사라짐")
- `BoardView.CellsToViewEdge(cell, dir)` 신설: **현재 카메라**의 orthographicSize·aspect·position 으로 화면 경계까지의 칸 수를 계산 → 줌·팬 상태에서도 화면 기준
- Exit = `CellsToViewEdge(머리) + exitMarginCells` + (꼬리까지 길이+1). 미리보기 = 레인 끝에서 화면까지 추가 연장
- `ArrowView.Sample()` 이 배열 끝에서 **직선 외삽**하도록 변경 (줌 아웃 시 미리 만든 배열보다 멀리 나가므로)
- `GameConfig.exitMarginCells` (기본 2) 추가

**3. 60fps (`BootLoader` + `GameConfig`)**
- `GameConfig.targetFrameRate` (기본 60) + `BootLoader.Awake` 에서 `Application.targetFrameRate` 적용, `QualitySettings.vSyncCount = 0` (vSync 가 켜져 있으면 targetFrameRate 가 무시됨)

**4·5. 팝업 겹침과 뒤로가기 — 같은 원인이었습니다 (`PopupBase`)**
- 원인: `PopupBase.Awake` 가 `CanvasGroup` 을 잡고 `BackButton.Pressed` 를 구독하는데,
  - 팝업을 **활성**으로 저장하면 → 에디터·시작 순간 전부 겹쳐 보임 (팀장 사진)
  - 팝업을 **비활성**으로 저장하면 → Awake 가 안 돌아 `Open()` 이 NRE, 뒤로가기도 구독 안 됨
- 수정: 닫힘 = **비활성 + CanvasGroup 숨김** 둘 다. `Open()` 이 `SetActive(true)` 로 스스로 살아나고 `Close()` 가 다시 비활성으로. `_group` 은 지연 초기화라 어느 상태로 저장돼 있든 동작합니다
- 뒤로가기 구독을 `Awake/OnDestroy` → **`OnEnable/OnDisable`** 로 이동 (열려 있는 동안만 듣는다). `LevelSelectView` 도 같은 이유로 동일하게 수정 — 이게 5번의 직접 원인이었습니다
- `PopupBase.CloseAll()` 추가 → `GameScreen.Start` / `MainMenu.Start` 에서 호출. 씬 저장 상태와 무관하게 진입 시 정리
- 씬 3개의 팝업을 **비활성으로 저장**해 뒀습니다 (이제 안전합니다)

> 디렉터 지시는 "`PopupBase.Awake` 에서 무조건 `SetActive(false)`" 였는데, 그러면 `Open()` 이 오브젝트를 다시 켜지 않아 팝업이 **영원히 안 뜹니다**. 의도(진입 시 안 겹치기)는 그대로 지키면서 동작하도록 `Open/Close` 가 활성 상태를 관리하는 형태로 구현했습니다.

**6. 난이도 라벨 (씬 값, 코드 강제 안 함)**
- 원인: TMP 의 **`alignment` 이 `TopLeft`** 였습니다 (TMP 기본값). 코드에서 강제하지 않고 **씬 값만** 고쳤습니다: `alignment=Center`, 앵커·피벗 top-center, Pos (0,-24), 400×56
- 겸사겸사: 라벨(y 2260~2316)과 하트(2208~2272)가 **12만큼 겹쳐** 있었습니다 → 하트를 Pos Y −100 → **−132** 로 내려 겹침 제거 (GAME_RULES §10 "난이도 라벨 아래 하트")

**7. `GridOverlay` → `LaneGuideOverlay` (교체)**
- `GridOverlay` **삭제**. 균일한 표가 아니라 **Arrow 머리가 있는 행(가로 Arrow)·열(세로 Arrow)** 에만 화면 끝까지 선을 긋습니다. Arrow 가 없는 행·열엔 선 없음, 같은 행·열에 여럿이면 선 하나
- **`LaneGuides`** (순수 C#, 테스트 11): `IsHorizontal` / `LineFor(arrow)` / `For(arrows)` 중복 제거 / `EmptyCells(w,h,arrows)`
- `Arrow` 가 Exit 하면 `BoardView.RebuildGuides()` 가 그 선을 제거
- 선 길이는 `zoomMin` 까지 축소 + 팬 해도 화면을 덮도록 계산 (`GuideHalfSpan`)
- `ArrowViewStyle.laneGuideColor`(연보라) / `laneGuideWidthCellRatio`(0.06) / `guideOrder`
- 이름 정리: `GridToggleButton` → **`LaneGuideToggleButton`**, `SettingsStore.GridOn` → **`LaneGuideOn`**(PlayerPrefs 키 `lane_guide_on`), `BoardView.SetGridVisible` → `SetLaneGuideVisible`

**8. 빈 칸 점 (`EmptyCellDots`)**
- 토글과 **무관하게 항상** 표시. 보드 사각형 안에서 Arrow 가 덮지 않은 칸마다 점. Arrow 가 나가면 그 칸에도 생김
- `ArrowViewStyle.showEmptyCellDots`(기본 켜짐) / `emptyCellDotColor`(연회색) / `emptyCellDotRadiusCellRatio`(0.12)

**에셋 값** — `GameConfig.asset`: `targetFrameRate 60`, `exitMarginCells 2`. `ArrowViewStyle.asset`: 레인 가이드·점 값 6개

**가정 (디렉터 확인)**
- 4번을 지시대로가 아니라 `Open/Close` 가 활성 상태를 관리하는 방식으로 구현했습니다 (위 인용 참고). 지시대로가 맞다면 알려 주세요
- 레인 가이드 선은 **보드 좌표계**라 줌·팬에 보드와 함께 움직입니다 (지시의 "줌·팬 시 보드와 함께" 해석)
- 빈 칸 점 색·크기는 §9 수치 그대로. 레퍼런스보다 흐리면 `ArrowViewStyle` 에서 조정하시면 됩니다
- 6번 하트 위치 조정(−100→−132)은 제가 씬에서 했습니다. 디자인상 다른 위치가 좋으면 팀장이 옮기시면 됩니다

**커밋 범위**
- 커밋함: 코드·테스트 + `GameConfig.asset`·`ArrowViewStyle.asset`
- **커밋 안 함(디렉터 몫)**: 씬 3개, `Prefabs/UI/LevelCell.prefab`, `Art/*`, `docs/BUILD.md`·`QA_DEVICE.md`, `docs/WORK.md`
- Unity 가 자동으로 건드린 `ProjectSettings.asset`(Android 기본 배칭값)·`UniversalRP.asset` 등도 손대지 않았습니다

**팀장 에디터 할 일**
1. **없습니다.** 씬 값(팝업 비활성, 난이도 라벨 정렬, 하트 위치, 토글 컴포넌트 교체)까지 제가 반영했습니다
2. 디렉터께 **"씬 커밋해줘"**
3. `#` 버튼을 눌러 레인 가이드가 레퍼런스와 같은지, 빈 칸 점 농도가 적당한지 봐 주세요 — 다르면 `ArrowViewStyle` 에서 색·굵기만 조정하면 됩니다

---


### 씬 조립 대행 (2026-09-18) — 팀장 요청

> **CLAUDE.md 예외**: "씬·프리팹 직접 수정 금지, 컴포넌트 연결은 사용자가 에디터에서 함" 규칙이 있으나, **팀장이 직접 "나머지는 해줄 수 있습니까" 라고 요청**해서 이번 건에 한해 프로그래머가 조립했습니다. 디렉터께서 아셔야 씬 커밋이 가능하므로 여기 남깁니다. 앞으로도 프로그래머가 조립해도 되는지는 디렉터 판단으로 정해 주세요.
>
> **씬·프리팹·에셋은 커밋하지 않았습니다** (TEAM.md: 디렉터 담당). 작업 트리에 변경분이 있습니다.

**팀장이 직접 하신 것**: `GameController.catalog`, UICanvas/SafeArea/Popups, EventSystem(InputSystemUIInputModule), HUD(하트 3·BackButton·RetryButton·DifficultyLabel·GridToggleButton), `Popup_Clear` 의 뼈대, `Art/Sprites/Heart.png`, `Art/FreeButtonSet` 임포트

**프로그래머가 이어서 한 것**

| 씬/에셋 | 내용 |
|---|---|
| Game | `Popup_Clear` 완성(Dim/Panel 이름·Panel 구성·Title/Subtitle/버튼 2종), `Popup_Fail`·`Popup_ConfirmMain`·`Popup_Raffle` 신규, `Ads`(AdsController), `TutorialBubble`+`Finger`+`Tutorial`(TutorialPresenter), `GameScreen`, `BoardView.boardCamera`, 모든 필드·onClick 배선, `GameController.levelCleared/levelFailed` 이벤트 |
| Main | 전체 신규 — UICanvas/EventSystem/SafeArea/Popups, Logo, StartButton(+LevelLabel), LevelSelectButton, SettingsButton, RaffleButton, LevelSelectPanel(Header+ScrollRect+GridLayout), `Popup_Settings`·`Popup_Quit`·`Popup_Raffle`, `MainMenu` 배선 |
| Boot | 전체 신규 — UICanvas/SafeArea/Logo/Loading, `Boot`(BootLoader → Main), `Audio`(AudioManager) |
| 프리팹 | `Assets/_Project/Prefabs/UI/LevelCell.prefab` 신규 |

**실제로 돌려서 확인한 것 (플레이 모드)**

- Boot → Main 자동 전환. Main 이 `레벨 5` 표시 (저장된 최고 레벨 4 기준)
- 레벨 선택: 칸 **50개** 생성, 1~4 체크 / 5 강조 테두리 / 6+ 잠김
- 설정 팝업: 사운드·진동 토글이 `SettingsStore`(PlayerPrefs)와 양방향 연동
- Game: 레벨 1 로드(화살표 3), HUD 하트 3, 난이도 "쉬움", `#` 격자 버튼
- 팝업 4종 전부 `Open()`/`Close()` 동작. 클리어 팝업은 `Strings` 로 "레벨 1"·"다음 레벨" 자동 채움
- **콘솔 에러 0**. 경고 2건은 에디터에서 Firebase 네이티브가 없어 나는 알려진 정상 건
- EditMode 테스트 **288/288**

**조립 중 발견한 내 가이드의 오류 2건 (가이드 수정함)**

1. **팝업을 비활성으로 두라고 쓴 것 — 틀렸습니다.** `PopupBase.Awake()` 가 `CanvasGroup` 을 잡아 `SetVisible(false)` 로 숨기는 구조라, 비활성이면 Awake 가 돌지 않아 `_group` 이 null → `Open()` 에서 **NRE** 로 죽습니다. 팝업은 **활성**으로 두고 숨김은 CanvasGroup 이 담당합니다. 실제로 6개(Game 3 + Main 3)를 활성으로 되돌렸습니다
   - 비활성이 맞는 건 PopupBase 가 아닌 것들뿐: `AdUnavailableText`, `TutorialBubble`, `Finger`, `RaffleButton`, `LevelSelectPanel`
2. **버튼 색을 남색(`141A33`)으로 칠하라고 쓴 것 — 스프라이트가 생겼으니 틀렸습니다.** `FreeButtonSet` 버튼은 이미 금색으로 그려진 스프라이트입니다. Image 는 스프라이트 색에 Color 를 **곱하므로** 남색을 곱하면 "시작하기" 가 검은 덩어리가 됩니다 (하트 때와 같은 문제). **버튼 Image = 흰색, 라벨 = 남색**으로 바꿨습니다 (14개)

**만들면서 고친 것**

- `LevelCell` 프리팹: 잠금 아이콘이 가운데라 **숫자와 겹쳤음** → 우하단 48px 반투명으로 이동 (`Bind()` 는 숫자를 항상 켜 두므로 가운데는 쓸 수 없습니다). `Highlight` 는 루트 Image 보다 뒤로 갈 수 없어 칸을 통째로 덮었음 → **9-slice `fillCenter = false`** 로 테두리만 그리게 변경
- `GridToggleButton` 의 `onColor`/`offColor` 를 금색 스프라이트에 맞게 흰색/흐린 회색으로 (남색을 곱하면 탁해짐)

**가정 (디렉터 확인)**

- **팔레트가 GAME_RULES §9 와 달라졌습니다.** 규칙은 남색 `141A33` + 연보라 `E9E4FF` 인데, 팀장이 넣으신 `FreeButtonSet` 이 금색 계열이라 버튼이 전부 금색입니다. 색을 규칙대로 맞추려면 **색이 칠해지지 않은(흰색/회색) 버튼 스프라이트**가 필요합니다. 지금은 팀장 선택을 존중해 금색 그대로 뒀습니다
- Main 의 Logo 는 스프라이트가 없어 **TMP 글자("NANA Arrow")**로 뒀습니다. 로고 이미지가 나오면 교체
- `AudioManager` 의 클립 목록은 **비워** 뒀습니다 (무음으로 정상 동작). 사운드 13종이 준비되면 채우면 됩니다
- 팝업 문구를 코드가 덮어쓰지 않는 자리(제목 등)는 **한국어를 직접 넣었습니다**. `Strings` 키로 빼야 하면 `LocalizedText` 를 붙이면 됩니다
- 잠금 아이콘이 `x` 입니다 — `FreeButtonSet` 에 **자물쇠가 없습니다**. 자물쇠 스프라이트가 생기면 교체 권장

**주의 — 팀장님 저장 파일이 바뀌었습니다**

플레이 검증 중 응모 코드 팝업을 열어서 **로컬 응모 코드가 발급됐습니다** (`App.Progress.RewardCodeIssued = true`). 그래서 Main 에 "응모 코드 확인" 버튼이 보입니다. 원래 상태로 돌리려면 **NanaArrow → Cheat → Level Jump** 창의 "저장 초기화" 를 쓰시면 됩니다 (최고 레벨 4도 함께 초기화됩니다).

**남은 팀장 에디터 할 일**

1. **없습니다 — 씬 조립은 끝났습니다.** 열어서 눈으로 확인만 해 주세요
2. 디렉터께 **"씬 커밋해줘"** (Game/Main/Boot `.unity` + `Prefabs/UI/LevelCell.prefab` + `Art/FreeButtonSet` + `Art/Sprites` + `NanumGothic SDF.asset`)
3. 첫 개발 빌드: **NanaArrow → Build → 개발 빌드 APK** (`docs/BUILD.md`) → 폰에서 `docs/QA_DEVICE.md` 20항목
4. (선택) 자물쇠·빈 하트·로고 스프라이트, 사운드 13종

---


### W-021 완료 (2026-09-18) — 브랜치 `feat/build-prep`

- 테스트 **288/288** (+21), 컴파일 에러 0, 콘솔 경고 0. 씬·프리팹은 안 건드림
- W-011 가정 5건이 전부 승인돼서 코드 조정은 없음 (이미 그 전제로 구현돼 있었음)

**변경 요약 (PR)**

**1. GAME_RULES v0.7.2 §10 — 줌**
- `zoomMin` 이 "최소 = 기본" 이었던 걸 분리: **`zoomMin` 0.5 / `zoomDefault` 1.0 / `zoomMax` 3.0**. `BoardCameraModel` 은 시작·리셋을 `zoomDefault` 로 하고 `[zoomMin, zoomMax]` 로 클램프. `IsDefault` 도 기본 줌 기준
- **`Clamp()` 판정 교체**: "줌이 최소면 항상 중앙" → **"보드(+여백)가 화면에 다 들어오면 중앙"**. v0.7.2 가 "후반 레벨은 보드가 화면 폭을 넘는다" 를 허용하므로, 기본 줌에서도 보드가 화면보다 크면 이동이 돼야 합니다. 예전 코드는 이 경우 이동을 막았습니다
- `GameConfig.asset` 값도 갱신 (`zoomMin: 1` → `0.5`, `zoomDefault: 1` 추가). **C# 기본값만 바꾸면 이미 직렬화된 에셋은 안 바뀌어서 에셋을 직접 고쳤습니다**

**2. §9 — 격자 토글 (W-020 에 없었음 → 새로 추가)**
- **`Gameplay/View/GridOverlay`**: 셀 경계를 옅은 선으로. `BoardLayout.GridMin`·`GridMax` 로 영역 계산 (셀 중심이 격자 칸 한가운데 오도록)
- `ArrowViewStyle` 에 `gridColor`(연회색) / `gridLineWidthCellRatio`(0.03) / `gridOrder`(0, 모든 것 뒤) — §9 수치 그대로. 에셋에도 기록
- **`SettingsStore.GridOn`** (PlayerPrefs, **기본 꺼짐**) + `GridChanged` 이벤트
- **`UI/Game/GridToggleButton`**: 우하단 `#` 버튼. onClick 을 스스로 등록하므로 인스펙터 연결 불필요
- 배선 주의: `BoardView`(Gameplay)는 `SettingsStore`(Core)를 **참조할 수 없어서**(Core → Gameplay 단방향) `GameController` 가 중계합니다

**3. §10 — 난이도 라벨 (W-017 에 없었음 → 새로 추가)**
- `LevelData.Difficulty`: `meta.difficulty` 1~3, 없거나 범위 밖·타입 불일치면 **0 = 숨김**
- **`UI/Game/DifficultyLabel`**: 1/2/3 → `hud.difficulty.easy/normal/hard`. `KeyFor` 는 static 이라 테스트 가능
- `GameController.CurrentLevelData` 노출 (기존엔 `LevelData` 를 안 들고 있었음)
- `Strings_ko.asset` 에 **`hud.difficulty.easy/normal/hard`** 3키 추가 (`쉬움`/`보통`/`어려움`, 총 45키)

**4. Android 빌드**
- **`Editor/BuildScript`**: 메뉴 `NanaArrow/Build/` 에 **개발 빌드 APK (`Cmd+Shift+B`)** / 릴리스 빌드 APK / EDM Force Resolve. 결과 = `Builds/NANA-Arrow_<버전>_<dev|release>_<날짜>.apk`, 끝나면 폴더를 열어 줌
- 빌드 전에 Scene List 비었는지·플랫폼이 Android 인지 확인하고 **EDM Force Resolve 를 먼저 실행**. EDM 은 asmdef 가 없어 `Assembly-CSharp-Editor` 에 들어가므로 **리플렉션**으로 `GooglePlayServices.PlayServicesResolver.MenuResolve` 를 호출합니다 (에디터에서 조회 성공 확인)
- 릴리스 서명은 **`ProjectSettings/keystore.local.json`** 에서 읽음 (`keystorePath`/`keystorePass`/`keyaliasName`/`keyaliasPass`). 없으면 릴리스만 막히고 개발 빌드는 정상
- `.gitignore` 에 `/Builds/`, `keystore.local.json`, `*.keystore`, `*.jks` 추가
- **Player Settings 는 손대지 않았습니다** — 이미 요구사항을 만족합니다: Android / IL2CPP / ARM64+ARMv7(`AndroidTargetArchitectures: 3`) / `AndroidMinSdkVersion: 25` / Portrait 고정 / 씬 Boot·Main·Game 순서 / `com.nanabox.arrow`


**디렉터께 — 커밋 부탁드립니다**
`docs/BUILD.md` 와 `docs/QA_DEVICE.md` 를 만들어 두었지만 **커밋하지 않았습니다**. TEAM.md "커밋 담당" 규칙 (2) 가 *프로그래머는 `docs/` 를 add 하지 않는다 — `REPORT_PROGRAMMER.md`, `HANDOFF_PROGRAMMER.md` 만 예외* 라서입니다. WORK.md W-021 은 두 문서를 프로그래머 항목으로 적어 두셨는데, 두 규칙이 같은 날짜라 충돌합니다. **작업 트리에 untracked 로 있으니 main 에서 커밋해 주시면 됩니다.** (앞으로 이런 경우 제가 커밋해도 되는지도 알려 주시면 그대로 따르겠습니다)

**가정 (디렉터 확인)**
- 격자 토글 상태를 **PlayerPrefs 에 저장**했습니다 (§9 "상태는 PlayerPrefs 저장" 그대로). 레벨마다 초기화하지 않습니다
- 격자 기본값은 **꺼짐** (§9 "기본은 격자·셀 배경 없음")
- 난이도 라벨은 `meta.difficulty` 가 없는 레벨에서 **숨김**. 현재 레벨 1~50 에는 전부 들어 있어 항상 보입니다
- 릴리스 빌드는 APK 로 만듭니다. 스토어 업로드용 **AAB** 가 필요하면 말씀해 주시면 메뉴를 추가하겠습니다
- `BuildScript` 의 출력 폴더·키스토어 경로는 Editor 전용 도구라 인스펙터가 없어 `const` 로 뒀습니다 (게임 밸런스 값이 아니라 SO 로 빼지 않았습니다)

**팀장 에디터 할 일 — "씬에 붙일 것"**
1. **Game 씬 HUD**: `DifficultyLabel`(TMP + DifficultyLabel), `GridToggleButton`(Button + GridToggleButton) — *이후 "씬 조립 대행" 에서 프로그래머가 붙였습니다*
2. 첫 개발 빌드: **NanaArrow → Build → 개발 빌드 APK** (`docs/BUILD.md`), 폰에서 `docs/QA_DEVICE.md` 20항목 확인
3. (출시 전) 서명 키 만들고 `ProjectSettings/keystore.local.json` 작성 + **키 백업** (분실 시 스토어 업데이트 불가)

---


### 남은 에셋·출처 기록 (2026-09-18)

> 씬 조립이 끝나 **STEP 0~8 조립 가이드는 제거**했습니다 (git 이력 `docs/REPORT_PROGRAMMER.md`, 2026-09-18 이전 커밋에 남아 있습니다).
> 조립하며 배운 UI 함정 4개는 `docs/HANDOFF_PROGRAMMER.md` §7 로 옮겼습니다. 아래 두 표는 아직 유효해서 남깁니다.

#### 아직 없는 것 (나중에, 지금은 없어도 진행 가능)

| 항목 | 담당 | 없으면 |
|---|---|---|
| ~~하트(가득)~~ | ✅ 완료 | `Art/Sprites/Heart.png` (2026-09-18 팀장 추가) |
| 하트(빈), 손가락, ←·↻·톱니·체크·자물쇠 아이콘 스프라이트 | 팀장(아트) | `Knob`/`UISprite` 로 임시 진행. 빈 하트는 없으면 가득 하트를 알파 30% 로 씀 |
| 사운드 13종 클립 | 팀장 | 무음 (정상 동작) |
| AdMob 실제 앱 ID·광고 단위 ID | 팀장(콘솔) | 테스트 광고로 동작. 출시 전 필수 |
| Firebase `google-services.json` | 팀장(콘솔) | 응모 코드가 로컬 발급만 됨. 출시 전 필수 |

#### 에셋 출처 기록 (디렉터께 — 정식 문서로 옮기실지 판단 부탁드립니다)

스토어 심사·분쟁 대비로 아트 에셋 출처를 남겨 둡니다. 저는 `docs/` 에 새 파일을 만들 수 없어 여기 적습니다.

| 에셋 | 출처 | 상업적 이용 | 확인 |
|---|---|---|---|
| `Art/Sprites/Heart.png` | **망고보드** | 팀장 확인 "사용해도 괜찮음" | 2026-09-18 |
| `Art/Fonts/NanumGothic.otf` (+ SDF) | 나눔고딕 | OFL (자유 이용) | — |

> 망고보드는 **플랜·용도에 따라 이용 범위가 다릅니다** (앱 스토어 배포·재판매 포함 여부). 출시 전에 현재 플랜이 모바일 앱 배포를 포함하는지 한 번만 확인해 두시면 안전합니다. 개발·테스트 단계에서는 문제없습니다.

---


### W-011 완료 (2026-09-17) — 브랜치 `feat/ads-reward` (base main)
- 테스트 **267/267** (+34), 컴파일 에러 0. 플레이 모드(에디터 광고 스킵 경로)에서 하트 0 → 실패 팝업 → 이어하기(보상형 = 에디터는 시청 완료 처리) → 하트 +1·팝업 닫힘·보드 유지 → 다시 하트 0 → 이어하기 버튼 숨김(레벨당 1회) → 다시하기 → 재시작(RetryFail) 확인. 클리어 팝업 → `AfterLevelCleared` → 이동 콜백 확인. 씬·프리팹은 안 건드림
- 겸사겸사: docs 스냅샷 커밋 (LEVEL_DESIGN 21~50, LEVEL_FORMAT v0.6, UI_FLOW L26 a12, REPORT_PLANNER)

**변경 요약 (PR)**
- **`Core/AdsManager`** 순수 C# (테스트 11): NO.3 InterstitialGate 확장. `LevelCleared(level, alreadyCleared)` → `NotDue / SkippedFreeLevel / Due` (adFreeLevels 이하는 세지도 않음, 그 위 N번째 클리어마다 Due + 카운터 0, 재클리어도 카운트) · `RetryPressed(failCount)` → 같은 레벨 실패 횟수가 interstitialAfterFails 의 배수면 Due · `ContinueRequested(continuesUsed)` → maxContinues 미만이면 허용. 값은 `AdsManager.FromConfig(AdsConfig)`. 앱 세션 안에서만 (저장 안 함)
- **`Core/AdsController`** (Game 씬 MonoBehaviour, UI_FLOW §8 연결점): `AfterLevelCleared(then)` (클리어 팝업 두 버튼) · `RetryFromFailPopup()` (실패 횟수 세서 판정 → 재시작) · `RequestContinue(done)` (보상형 → **시청 완료(OnUserEarnedReward)일 때만** `GameController.Continue(continueLives)`) · `CanOfferContinue` / `IsRewardedReady`. 전면 광고가 준비 안 됐으면 기다리지 않고 바로 이동. 광고 중 `AudioManager.PauseBgm`. 레벨별 실패 횟수·판당 이어하기 횟수는 세션 안에서만
- **`Core/IInterstitialAd` / `IRewardedAd`** (`IsReady`, `Awaitable<bool> ShowAsync()`) + `App.Interstitial / App.Rewarded` (Services 가 등록. Core 는 SDK 를 모름)
- **`Services/AdMobService`** (NO.3 이식): 첫 씬 로드 전 자가 생성 → `App.SetAds`. Google **테스트 광고 단위 ID** (실제 ID 상수는 비어 있음 → 테스트 ID 폴백 + 경고). 콜백은 volatile 플래그 → Update 에서 처리, 전면 90초 안전장치, 닫히면 재로드, 포커스 복귀 시 재시도. **에디터**: 전면 즉시 스킵(false) / 보상형 시청 완료(true) — `#if UNITY_EDITOR` 격리. iOS ATT 브릿지는 미이식 (v1 Android)
- `GameController.Continue(lives)`: 보드·Marked 그대로 목숨만 회복 + `LevelStats.ContinuesUsed`
- **`Core/RewardCode`** (NO.3 이식, 테스트 14): XXXX-XXXX, 0/O/1/I 제외 32자, `Generate(nextIndex)` 결정적, `IsWellFormed`. **`RewardConfig` SO** (`rewardLevel` 100, `claimUrl` nanabox.co.kr/reward-claim.html) → `Settings/RewardConfig.asset` 생성됨. `RewardCodeStatus` enum, `IRewardCodeService` + `App.RewardCodes` / `App.RewardCodeIssued` 통지
- **`Services/RewardCodeService`** (NO.3 이식): 자가 생성. 로컬(PlayerPrefs `nanaarrow.reward.*`) 먼저 → Firestore `rewards`(기기당 1) + `code_index` WriteBatch. 서버 실패해도 유저는 코드를 받고, 미동기화면 앱 실행마다 조용히 재시도. Firebase 대기 전부 타임아웃. 발급되면 `PlayerProgress.MarkRewardCodeIssued()` (Main 의 응모 버튼 표시). 에디터 = 네이티브 없음 → 즉시 로컬 발급
- **UI**: `Game/FailPopup`(PopupBase 파생): 이어하기 버튼은 남은 횟수 있을 때만, 광고 준비 안 됐으면 비활성 + `fail.ad_unavailable` 표시, 끝까지 안 보면 팝업 유지. `RewardCodePanel`(PopupBase 파생, Game·Main 공용): 열리면 발급 요청, 코드·상태 문구, 복사(클립보드 + `raffle.copied`), 상품 받으러 가기(URL). `ClearPopup`: 두 버튼이 `AdsController.AfterLevelCleared` 를 거쳐 이동, **응모 레벨 클리어면 응모 팝업이 먼저** 뜨고 닫히면 클리어 팝업이 다시 열림 (§6-5)
- 애널리틱스 (ANALYTICS.md §3-2, §3-4 전부): `continue_offer(ad_ready)` / `continue_request` / `continue_granted(lives_after)` / `ad_interstitial(trigger, result, level)` / `ad_rewarded_result(result, level)` / `reward_code_issued(synced)` / `reward_code_copy` / `reward_link_open` + 사용자 속성 `reward_issued`
- `Strings_ko.asset` 에 **`raffle.status.checking / saving / issued / reissued / offline / save_failed`** 6키 추가 (UI_FLOW §9 에 없는 키 — 디렉터가 §9 에 반영해 주시면 됨. 문구는 NO.3 것)

**가정 (디렉터 확인)**
- "같은 레벨 N번 실패마다" 의 실패 횟수 = 앱 실행 후 그 레벨의 실패 팝업 횟수 (HUD 다시하기·이어하기는 안 셈). 앱 재시작하면 0
- 이어하기 뒤 다시 하트 0 → 이어하기 버튼 **숨김** (§6-2 "주 버튼 숨김"). 광고 준비 안 됨은 **비활성 + 문구**
- 보상형 광고가 준비 안 된 상태로 이어하기를 눌렀을 때(경합) `ad_rewarded_result(failed_to_show)` 만 남기고 팝업 유지
- 응모 코드 Firestore 컬렉션 이름은 NO.3 과 같음 (`rewards`, `code_index`) — Firebase 프로젝트가 새로 생기니 충돌 없음. 홈페이지 검증 규칙 동일
- 실제 AdMob 앱 ID·광고 단위 ID 는 팀장 콘솔 작업 뒤 `AdMobService` 상수 + `GoogleMobileAdsSettings.asset` 을 한 커밋으로 바꿈 (출시 전 필수)

**팀장 에디터 할 일 — "씬에 붙일 것"**
1. **Game 씬** 빈 오브젝트 `Ads` → **AdsController** → Ads Config = `Settings/AdsConfig`, Game Controller
2. `Popup_Fail` 은 PopupBase 대신 **FailPopup** (Closable By Back **끔**) → Ads = Ads 오브젝트, Continue Button = PrimaryButton, Ad Unavailable Text. PrimaryButton.OnClick → FailPopup.**OnContinue**, SecondaryButton → FailPopup.**OnRetry** (W-017 에서 GameController.RestartFromFailPopup 에 직접 연결했다면 이걸로 교체)
3. `Popup_Clear` 의 ClearPopup → **Ads** = Ads 오브젝트, **Reward Panel** = Popup_Raffle
4. `Popup_Raffle` (Game·Main 각 1개, §12-5) 는 PopupBase 대신 **RewardCodePanel** (Closable By Back 끔) → Strings, Config = `Settings/RewardConfig`, Code Text, Status Text(선택, 작은 TMP 하나 추가 권장), Copy Button, Go Button. CopyButton.OnClick → **CopyCode**, GoButton → **OpenClaimPage**, CloseButton → PopupBase.**Close**
5. Main 씬 `MainMenu` → **Raffle Popup** = Popup_Raffle
6. 확인: Play(Game) → 하트 0 → 이어하기(에디터는 광고 없이 바로 +1) → 다시 0 → 버튼 없음 → 다시하기. 응모 팝업은 치트 창 레벨 점프로 100 클리어해야 보임 (카탈로그 50개라 당장은 `RewardConfig.rewardLevel` 을 임시로 낮춰 확인 가능)
7. Android 빌드 전: EDM Force Resolve, Firebase 프로젝트의 `google-services.json` 넣기 (없으면 Firestore 저장은 실패하고 로컬 발급으로 감 — 정상)

### W-017 완료 (2026-09-17) — 브랜치 `feat/ui-scripts` (base main)
- 테스트 **233/233** (+28), 컴파일 에러 0. 플레이 모드에서 임시 캔버스로 튜토리얼(레벨 16: 말풍선 + 손가락이 a2 머리 위) → 얼음 깨기로 사라짐, 하트 감소(흔들림 → 빈 하트) · 회복 연출 확인. 씬·프리팹은 안 건드림
- 겸사겸사: 기획자가 올린 **레벨 21~50** 을 Level Validator 로 검증 **30/30 통과**(minTaps 전부 일치) 후 커밋. UI_FLOW §7-2 의 L26 손가락 대상 "(W-013 에서 확정)" → 레벨 26 의 Key 화살표 **a12** 로 채움

**변경 요약 (PR) — UI_FLOW v0.2 §12-8 8건 + 붙이는 데 필요한 접착 스크립트**
- `UI/Strings` SO + `StringEntry` (§9 키 → 문구, `Get`/`Format("{0}")`, 없는 키는 키 그대로 표시) → **`Settings/Strings_ko.asset`** 생성됨(36키 전부). `UI/LocalizedText`(TMP 에 붙여 키만 지정하는 고정 문구용)
- `UI/Tutorial/`: `TutorialConfig` SO(`steps`, `hideDelay` 3, `showOnReplay` false) · `TutorialStep` · `TutorialTrigger` · `FingerAnchor` (§7-1 그대로, 필드는 CLAUDE.md 규칙대로 `[SerializeField] private` + getter). **`TutorialFlow`** 순수 C#(테스트 9): 트리거는 판마다 처음 1회만 인정, 항목은 한 번에 하나, `hideOn` 트리거 또는 `hideDelay` 로 닫힘. `TutorialPresenter`: GameSession 사건 → 트리거 (탭=FirstTap, Exit/Blocked/IceBroken, 길게 누르기=FirstLongPress), 말풍선 문구 = Strings, 손가락은 화살표 셀(Head/Middle/Tail + 오프셋) → 화면 좌표로 매 프레임 추적(줌·팬 대응) + 위아래 흔들림. `showOnReplay` 끄면 클리어한 레벨은 표시 안 함. 애널리틱스 `tutorial_step`/`tutorial_done`(elapsed) 발행 → **`Settings/TutorialConfig.asset`** 생성됨(§7-2 6행)
- `UI/Game/LivesView`: `SessionStarted` 마다 세션의 `LivesTracker.LivesChanged` 구독. 감소 → 해당 하트 흔들림(`shakeDuration` 0.3 / `shakeDistance` 10px) → 빈 하트 스프라이트 + `Haptics.LifeLost()`(`vibrateOnLifeLost` 켜짐 + 설정 진동 켜짐일 때) + SfxLifeLost. 회복 → 채워지며 팝(`refillDuration` 0.25 / `refillScale` 1.3) + SfxHeartRestore. 빈 하트 스프라이트가 없으면 알파만 낮춤
- `UI/Game/ClearPopup`(PopupBase 파생): 열릴 때 부제 `레벨 N` / 마지막 레벨이면 `clear.all_done` + 주 버튼 `메인으로` + 보조 버튼 숨김. `OnPrimary()`(다음 레벨 또는 메인) / `OnSecondary()`(메인). 광고 판단은 W-011 이 끼어듦
- `UI/Game/GameScreen`: Android 뒤로가기 → 팝업 없을 때 `Popup_ConfirmMain` 열기
- `UI/Main/MainMenu`: `StartLevel` = `App.Progress.NextLevel` 을 1~카탈로그 수로 클램프(전부 깼으면 마지막), `레벨 N` 라벨, `RaffleButton` 은 응모 코드 발급 뒤만, `StartGame()`/`OpenLevelSelect()`/`OpenSettings()`/`OpenRaffle()`/`Quit()`, 뒤로가기 → 종료 확인 팝업(레벨 선택이 열려 있으면 그쪽이 닫힘). Main 진입 시 BgmMain
- `UI/Main/LevelSelectView` + `LevelCell` + `LevelCellState`: 카탈로그 개수만큼 셀 생성(재사용), 열 때마다 상태 갱신 — 클리어(체크) / 다음 도전(강조) / 잠김(자물쇠, 탭하면 흔들림만). 열면 '다음 도전' 행으로 스크롤, `level_select_open` 발행. 탭 → `SceneLoader.LoadGame`
- `UI/Main/SettingsPopup`(PopupBase 파생): 열릴 때 토글을 `SettingsStore` 값으로, 바꾸면 즉시 PlayerPrefs (변경 이벤트 → AudioManager·애널리틱스는 기존 경로)
- `GameController`: **`RestartFromHud()`**(실패 횟수 미포함) / **`RestartFromFailPopup()`**(포함) / 기존 `GoToMain()` `LoadNextLevel()` — UnityEvent 에서 바로 연결. `SessionStarted` 이벤트, `LanePreviewShown` 이벤트, `lane_preview_first`(설치 후 1회, PlayerPrefs)
- `PopupBase`: `closableByBack` 은 W-010 에 이미 있음. 추가로 `ConsumedBack`(팝업이 열려 있거나 이번 프레임 뒤로가기를 팝업이 받았으면 true) — 화면 쪽 핸들러가 같은 프레임에 이중 처리(팝업 닫힘 → 종료 확인 열림)하지 않게
- `LivesTracker.LivesChanged(int)`, `GameEvents` 4종(TutorialStepShown/TutorialDone/LanePreviewFirst/LevelSelectOpened) + `AnalyticsReporter` 전송, `BoardView.TargetCamera`, UI asmdef 에 `Unity.TextMeshPro`
- 테스트: TutorialFlow 9 · TutorialConfig 3 · Strings 4 · MainMenu/LevelSelect 규칙 9 · LivesChanged 3

**가정 (디렉터 확인)**
- `hideDelay` 해석: **`hideOn` 이 None 인 항목에만** 적용(3초 뒤 숨김). `hideOn` 이 있는 항목은 그 트리거가 올 때까지 유지 (L1 의 손가락이 3초 만에 사라지면 안 되니까). "0 이면 판이 끝날 때까지 유지"
- 튜토리얼 표시 중 새 항목이 열리면 앞 항목은 닫힘(한 번에 하나). L2 에서 첫 탭이 Block 이면 free_first 가 닫히고 block 이 열림
- FirstTap = Ignored 가 아닌 모든 탭(Locked 흔들림 포함). FirstLongPress = 레인 미리보기가 실제로 켜진 순간
- 설정 팝업은 Main 전용이라 Game 씬엔 넣지 않음 (§4). 실패 팝업은 W-011 (이어하기) 에서 `FailPopup` 스크립트로 — 지금은 PopupBase + 버튼을 `GameController.RestartFromFailPopup` 에 직접 연결하면 동작
- 응모 코드 팝업 스크립트(`RewardCodePanel`)는 W-011. `MainMenu.rafflePopup` 필드는 비워 둬도 됨
- 하트 연출 수치·손가락 흔들림·셀 흔들림은 각 컴포넌트의 SerializeField (UI_FLOW 의 `UIConfig` SO 제안은 안 만듦 — 값이 컴포넌트마다 2~4개라 SO 로 묶으면 연결만 늘어남. 원하면 다음에 묶음)

**팀장 에디터 할 일 — "씬에 붙일 것"** (UI_FLOW §12 순서 그대로, [스크립트] 이름만 확정)
0. §12-1 한글 TMP 폰트 먼저 (없으면 말풍선이 □□□ 로 보임 — 이번 스크린샷이 그 상태)
1. **Game 씬 HUD** (§12-3): `Hearts` 에 **LivesView** → Game Controller = GameController, Hearts = Heart_0~2 의 Image 3개, Full/Empty Sprite = 하트 스프라이트 2종 (없으면 비워도 됨: 빈 하트는 알파 0.3). `RetryButton.OnClick` → GameController.**RestartFromHud**, `BackButton.OnClick` → Popup_ConfirmMain 의 PopupBase.**Open**
2. **Game 씬 튜토리얼** (§12-4): `Tutorial` 오브젝트에 **TutorialPresenter** → Config = `Settings/TutorialConfig`, Strings = `Settings/Strings_ko`, Game Controller, Board View = Board, Bubble = TutorialBubble, Bubble Text = TutorialBubble/Text, Finger = UICanvas/Finger (SafeArea 밖, Raycast Target 끔)
3. **Game 씬 빈 오브젝트 `Screen`** → **GameScreen** → Confirm Main Popup = Popup_ConfirmMain (Android 뒤로가기용)
4. **팝업** (§12-5): `Popup_Clear` 는 PopupBase 대신 **ClearPopup** 을 붙임(파생이라 PopupBase 기능 포함) → Strings, Game Controller, Subtitle, Primary Label(PrimaryButton 안 TMP), Secondary Button. PrimaryButton.OnClick → ClearPopup.**OnPrimary**, SecondaryButton → **OnSecondary**. `GameController` 인스펙터의 **Level Cleared** 이벤트 → Popup_Clear.**Open**, **Level Failed** → Popup_Fail.**Open**
   - `Popup_Fail`: PopupBase, **Closable By Back 끔**, `다시하기` → GameController.**RestartFromFailPopup** (이어하기 버튼은 W-011 까지 비활성)
   - `Popup_ConfirmMain`: PopupBase, `계속하기` → PopupBase.**Close**, `나가기` → GameController.**GoToMain**
   - 문구는 TMP 에 직접 입력하거나 TMP 에 **LocalizedText** 붙여 Strings + 키 지정 (둘 다 됨. `레벨 N` 처럼 숫자가 들어가는 건 스크립트가 채움)
5. **Main 씬** (§12-6): 빈 오브젝트 `Main` → **MainMenu** → Strings, Catalog = LevelCatalog, Start Level Label = StartButton/LevelLabel, Level Select = LevelSelectPanel, Settings Popup = Popup_Settings, Quit Popup = Popup_Quit, Raffle Button = RaffleButton (Raffle Popup 은 비움). 버튼: StartButton → MainMenu.**StartGame**, LevelSelectButton → **OpenLevelSelect**, SettingsButton → **OpenSettings**, RaffleButton → **OpenRaffle**
   - `LevelSelectPanel` 에 **LevelSelectView** → Catalog, Cell Prefab = `Prefabs/UI/LevelCell`, Content = Scroll/Viewport/Content, Scroll Rect = Scroll, Columns 5. Header/BackButton → LevelSelectView.**Close**
   - `LevelCell` 프리팹에 **LevelCell** → Number, Check, Lock Icon, Highlight 연결 (Button 은 자동)
   - `Popup_Settings` 는 PopupBase 대신 **SettingsPopup** → Sound Toggle, Vibration Toggle. CloseButton → SettingsPopup.**Close**
   - `Popup_Quit`: PopupBase, `취소` → **Close**, `종료` → MainMenu.**Quit**
6. **LevelCatalog.asset** 에 `level_021~050.json` 추가 (30개, 순서대로) — 이제 50레벨
7. Play(Main 씬부터) → 시작하기 라벨 `레벨 5`(지금 저장 기준), 레벨 선택에서 1~4 체크 · 5 강조 · 6~ 자물쇠(탭하면 흔들림), 설정 토글, Esc 로 종료 확인 팝업. Game 씬: 하트 감소·재시작·클리어 팝업 → 다음 레벨. 레벨 1 튜토리얼은 저장 초기화(치트 창) 후에 보임

### W-020 완료 (2026-09-17) — 브랜치 `feat/board-zoom` (base main)
- 테스트 **205/205**, 컴파일 에러 0. 플레이 모드에서 핀치(×2.5, ×3)·팬 클램프·재시작 시 줌 리셋과 셀 크기 고정 확인 (`docs/screenshots/W-020_zoom3_pan.png`)

**변경 요약 (PR)**
- `Gameplay/Input/BoardCameraModel` (순수 C#, 테스트 11): `Zoom`(zoomMin~zoomMax) · `Offset`(보드 중심 기준). `SetZoom(zoom, focus)` 는 손가락 아래 점을 화면에서 고정, `Pan`, `Reset`, `RegisterEmptyTap(time)` 더블 탭 판정. **클램프**: 줌 1 이면 항상 중앙. 보드+여백이 화면보다 큰 축만 이동 가능하고, 화면이 보드+여백(`panMarginCells`) 밖으로 나가지 않게 (= 보드가 화면 밖으로 완전히 나갈 수 없음)
- `Gameplay/Input/BoardCameraController` (Main Camera): 모델을 `orthographicSize = 기본 ÷ Zoom`, position 으로 적용. `Attach(layout)` 레벨마다, `ResetZoom()` 은 GameController 가 레벨 시작·클리어·실패 때 호출. 줌 1.0 = 카메라의 시작 orthographicSize
- **`TapInput` 개편**: 누름 → (a) `dragThresholdCells` 이상 이동 → 드래그 확정(`Pan` 이벤트, 탭·미리보기 아님, 미리보기 중이었으면 해제) (b) `longPressSeconds` 경과 → 미리보기 (c) 그 전에 놓음 → `CellTapped`(Arrow 있는 셀) 또는 `EmptyTapped`(빈 셀·보드 밖). 두 손가락 감지 → 즉시 `Pinch`(배율, 중심점) 모드, 진행 중이던 탭·미리보기 취소, 손가락이 다 떨어질 때까지 새 탭 후보 없음. 에디터: 마우스 드래그 = Pan, 휠 = `Scroll`(눈금당 ±10%)
- `BoardView.HasArrowAt(cell)`, 셀 크기 계산은 `BoardCameraController.BaseOrthographicSize`(줌 1) 기준 고정
- `GameConfig`: `zoomMin` 1 / `zoomMax` 3 / `dragThresholdCells` 0.3 / `panMarginCells` 1 / `doubleTapSeconds` 0.3 (+ `GameConfig.asset`)
- HUD 는 Screen Space Overlay 라 줌 영향 없음 (W-017 캔버스 설정 그대로)

**가정 (디렉터 확인)**
- 작은 보드(초반 레벨)는 줌 2 정도까지는 보드+여백이 화면보다 작아 이동이 안 됨 (클램프 규칙의 자연스러운 결과). 줌 3 부터 이동 가능. 초반부터 이동을 원하면 `panMarginCells` 를 키우면 됨
- 더블 탭 "빈 곳" = Arrow 가 없는 셀 + 보드 밖 전부. Arrow 위 더블 탭은 탭 2회 (규칙대로)
- 핀치 중 HUD 위 손가락은 구분하지 않음 (두 손가락이면 무조건 핀치)

**팀장 에디터 할 일**
1. Game 씬 **Main Camera** → Add Component → **BoardCameraController** → Config = GameConfig, Tap Input = Input 오브젝트, Board View = Board 오브젝트
2. `GameController` → **Board Camera** = Main Camera (새 필드), `Board`(BoardView) → **Board Camera** = Main Camera (새 필드)
3. Play → 마우스 휠로 줌, 드래그로 이동, 빈 곳 더블 클릭으로 리셋 확인. 폰에서는 핀치·드래그

### W-015 후속 + W-010 완료 (2026-09-17) — 브랜치 `feat/services-infra` (base main)
- 테스트 **194/194 통과** (CLI 배치 + 에디터), 컴파일 에러·경고 0 (UI/Services 에 스크립트가 생겨 "빈 asmdef" 경고도 사라짐)
- 에디터가 SDK 임포트 중 종료돼 있어서 테스트는 `Unity -batchmode -runTests` 로 돌렸고, 팀장이 다시 연 뒤 플레이 모드로 카탈로그 로드·셀 크기·즉시 클리어→저장 파일 생성을 확인함 (`docs/screenshots/W-010_cell_rule_level5.png`)

**W-015 후속**
- `GameConfig`: `maxBoardWidth` 10 / `maxBoardHeight` 14 (검증기 규칙 0 분리 검사), `maxArrowLength` 40, **셀 크기 규칙** `cellWidthFraction` 0.052 / `maxAreaFraction` 0.9 / `cellGapRatio` 0. `cellSize`·`cellGap`·`areaWidthFraction` 삭제. `GameConfig.asset` 갱신
- `BoardLayout.CellSizeFor(화면폭, 가로칸수, …)` = min(폭×0.052, 폭×0.9÷칸수). 세로 중앙 = Board 오브젝트 위치
- 검증기 **규칙 2-e**: 머리 앞 Lane(가장자리까지) 위에 자기 몸통 셀이 있으면 오류. 기획자의 레벨 1~20 은 그대로 통과
- ⚠ 셀 크기 관찰: 0.052 면 6칸 보드가 화면 폭의 31% (레퍼런스 Lv4 는 약 45%). 레퍼런스에 맞추려면 `cellWidthFraction` 0.07 근처 — 인스펙터 값이니 팀장이 보면서 조정

**W-010 인프라**
- **SDK 이식 (NO.3 → `Assets/`)**: `GoogleMobileAds`(11.2.0, Unity Ads 미디에이션 4.19.0 포함), `Firebase`(13.13.0 App·Analytics·Auth·Firestore·Crashlytics + m2repository), `ExternalDependencyManager`(1.2.187), `Plugins/Android`(androidlib 3종, aar, gradle 템플릿 3종), `ProjectSettings/GvhProjectSettings.xml`·`AndroidResolverDependencies.xml`. **복사 안 함**: `google-services.json`, `GoogleService-Info.plist`, `FirebaseApp.androidlib/res/values/google-services.xml`(NO.3 프로젝트 값 — 실제 json 이 들어오면 EDM 이 재생성), `Plugins/iOS`, `StreamingAssets/google-services-desktop.json`. AdMob 앱 ID 는 Google 테스트 ID 그대로. `Assets/GeneratedLocalRepo/` 는 EDM 생성물이라 .gitignore (NO.3 와 동일)
- **어셈블리**: `NanaArrow.Services` 신설 (Core/Gameplay/Data 참조, SDK dll 은 auto-reference). Core 는 Services 를 모름. UI → Core. 결정대로 6+테스트
- **Core (순수 C# + MonoBehaviour)**
  - `SaveData`(v, 최고 클리어 레벨, 응모 코드 발급 여부) / `SaveCodec`(HMAC-SHA256, 새 키) / `SaveService`(원자적 쓰기) — NO.3 그대로, 필드만 교체. `PlayerProgress`(메모리 사본 + 즉시 저장, `NextLevel`, `IsCleared`, 레벨별 `attempts` 는 PlayerPrefs) / `App.Progress` 로케이터 (파일 `persistentDataPath/save.json`)
  - `SettingsStore`(사운드·진동 PlayerPrefs, 변경 이벤트) / `Haptics.LifeLost()`(Android Vibrator 30ms, iOS 는 Handheld.Vibrate 대체)
  - `SceneId`(Boot/Main/Game) + `SceneLoader.Load / LoadGame(level)` (레벨 전달은 static, PlayerPrefs 아님) / `BootLoader`(저장 로드 → 설정 적용 → `minDuration` 1초 뒤 Main)
  - `AudioManager`(BGM 1채널 + SFX, `SoundId` 13종 ↔ 클립 인스펙터 리스트, 사운드 토글 연동, `PauseBgm`) / `BackButton.Pressed` static 이벤트 (Escape, 자가 생성)
  - **`GameEvents` 허브** (LevelStarted/Cleared/Failed/RetryPressed/LevelQuit) + `LevelStats`(taps·blocks·lane_previews·duration·attempt) — 게임플레이는 발행만
  - `GameController`: `LevelCatalog` + 레벨 번호로 로드 (`SceneLoader.PendingLevel` 우선, 없으면 인스펙터 `startLevel`), `Restart(reason)`, `LoadNextLevel()`, `GoToMain()`, `IsLastLevel`, 치트 `CheatClear()`/`CheatRefillLives()`, 클리어 시 `App.Progress.MarkCleared`, 앱 pause 시 level_quit
- `Data/LevelCatalog` SO (TextAsset 리스트, 1부터), `GameSession.ForceClear()`, `TapInput` 은 `EventSystem.IsPointerOverGameObject()` 로 UI 위 탭 무시
- **UI**: `PopupBase`(CanvasGroup, 한 번에 하나 `Current`, `closableByBack`, Dim 용 `CloseIfAllowed`, 열림 SFX), `SafeAreaAdapter`
- **Services**: `SafeAnalytics`(LogEvent/SetUserProperty 예외 삼킴), `AnalyticsEvents`(18 이벤트·파라미터·속성 상수, `LevelName`/`Board`/`SnakeCase`), `AnalyticsReporter`(자가 생성, GameEvents 구독 → 전송은 여기 한 곳, `sendInEditor` 기본 끔, 설정 변경도 전송), `ScreenCaptureProtection`(Android FLAG_SECURE)
- 치트 창: 레벨 점프(카탈로그 없으면 파일 직접) · 다시하기 · **즉시 클리어 · 하트 채우기 · 저장 초기화**
- 테스트 추가: SaveCodec 5 · SaveService 5 · PlayerProgress 5 · LevelCatalog 4 · AnalyticsEvents 4 + 규칙 2-e·보드 상한·셀 규칙

**가정 (디렉터 확인)**
- WORK 의 "SaveData 필드: …사운드·진동" 과 "설정은 PlayerPrefs" 가 겹쳐서 **설정은 PlayerPrefs, SaveData 는 최고 레벨·응모 여부만** 으로 함 (UI_FLOW §6-4 와 일치)
- iOS 는 v1 대상이 아니라 `Plugins/iOS`(ATT·캡처 감지·햅틱 브릿지) 미복사. iOS 빌드 시 W-011 에서 함께
- `google-services.json` 이 없으므로 실기에서 Firebase 초기화는 실패하고 폴백 경로를 탄다 (정상). 팀장 Firebase 프로젝트 생성 후 파일만 넣으면 됨
- Android 빌드 전 **Assets → External Dependency Manager → Android Resolver → Force Resolve** 필요 (GeneratedLocalRepo 생성)
- 이번 플레이 테스트로 팀장 기기에 `save.json`(최고 레벨 4) 이 생겼음 → 치트 창 "저장 초기화" 로 지우면 됨

**팀장 에디터 할 일**
1. `Settings` 우클릭 → Create → NanaArrow → **Level Catalog** → `LevelCatalog.asset`, Levels 리스트에 `level_001~020.json` 순서대로 드래그 (20개)
2. Game 씬 `GameController`: **Catalog = LevelCatalog**, Start Level = 1 (Level Json 필드는 사라짐). `Input` 의 TapInput → Config 가 비어 있으면 GameConfig 연결 (플레이 테스트 때 비어 있었음 — 씬 저장 확인)
3. Boot 씬: 빈 오브젝트 `Boot` → **BootLoader** (Next Scene = Main, Min Duration 1). 빈 오브젝트 `Audio` → **AudioManager** → Clips 리스트에 SoundId 13종 (클립은 있는 것만)
4. Build Settings 씬 순서 Boot / Main / Game 확인 (SceneLoader 는 이름으로 로드)
5. UI_FLOW §12-2 캔버스 준비 시 `SafeArea` 오브젝트에 **SafeAreaAdapter**, 팝업 루트에 **PopupBase**(+CanvasGroup) — 나머지 UI 스크립트는 W-017
6. Play(Boot) → 1초 뒤 Main(비어 있음) 로 넘어가는지, Game 씬 직접 Play → 레벨 1 로드 확인. `GameConfig.cellWidthFraction` 을 레퍼런스와 비교해 조정 (위 관찰)
7. Android 빌드 전: Force Resolve, `google-services.json` 은 Firebase 프로젝트 만든 뒤 `Assets/` 에

### W-015 완료 (2026-09-17) — 브랜치 `feat/path-arrows` → **PR #30 → main 머지 완료** (`2a9d2c4`, 브랜치 삭제)
- 테스트 **155/155 통과**, 컴파일 에러 0. `FireResolver`·`TapHandler`·`LivesTracker`·`GameSession` 로직은 바꾸지 않았고 기존 테스트가 그대로 통과함 (Lane 추가만)
- **상태 (디렉터용)**: W-015 는 main 에 들어갔으니 "완료" 로 옮겨도 됨. **W-010 착수 가능** — 다음 "WORK.md 읽고 처리" 지시 때 `feat/services-infra` 로 시작. W-011 은 W-010 뒤
- 팀장 에디터 할 일 1~3 완료 확인 (TapInput Config 연결, Area Width Fraction 0.5, 카메라 배경 흰색). 4(길게 누르기·Level Jump)는 설명해 드림. HUD(하트·뒤로가기·다시하기)는 W-015 범위 밖(#13) 이라 아직 화면에 없음
- **PR 머지 권한 해결**: 자동 모드가 `gh pr merge` 를 매번 차단하던 문제 → 팀장이 `.claude/settings.json` 에 `Bash(gh pr merge *)` 허용 규칙 추가. 다음 세션부터 프로그래머가 PR 을 직접 머지함 (WORK.md 팀장 항목 "PR 머지" 는 이제 불필요). 이 파일은 아직 미커밋 → W-010 브랜치에 `chore:` 로 포함 예정

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
