# WORK.md — 작업 지시 (디렉터만 작성)
> 팀장은 각 AI에게 "docs/WORK.md 읽고 네 항목 처리해줘" 만 말한다.
> **이 파일은 디렉터만 수정한다.** 보고는 각자 파일에: 프로그래머 → docs/REPORT_PROGRAMMER.md, 기획자 → docs/REPORT_PLANNER.md
> 항목이 없으면 그 역할은 할 일 없음. W-### 은 GitHub 이슈 번호와 별개.

---

## ⚠ 2026-09-17 코어 전환 공지 (전원 필독)
레퍼런스가 **Arrows – Puzzle Escape (Lessmore)** 로 확정됨. 화살표는 직선 막대가 아니라 **그리드를 따라 꺾이는 경로**다. GAME_RULES **v0.6 §0** 과 LEVEL_FORMAT **v0.5** 를 먼저 읽을 것. Block 판정·목숨·Marked·Frozen/Locked·GameSession 은 그대로. 바뀌는 건 Arrow 데이터(순서 있는 경로), 검증 규칙 2, 뷰/연출, 레벨 데이터.
- 프로그래머: **W-015 가 W-010 보다 우선.** W-010/W-011 은 영향 없으니 그 다음.
- 기획자: **W-016 이 최우선.** W-012(UI_FLOW) 는 영향 적으니 그 다음, W-013(레벨 21~30) 은 W-016 뒤로.

## 팀장 (에디터·계정 작업 — AI 가 못 하는 것)
- [ ] **PR 머지**: GitHub 웹에서 PR #1 → #28 → #29 순서로 Merge. 그 다음 터미널 `git checkout main && git pull`
- [ ] Game 씬 조립: REPORT_PROGRAMMER.md W-005 "팀장 에디터 할 일" 1~7 (ArrowViewStyle 에셋 → Board/Input/GameController 오브젝트 → Play 테스트 → Level Jump 치트)
- [ ] (출시 전, 급하지 않음) AdMob 콘솔 앱 2개 + 광고 단위 4개 + Unity Ads 미디에이션 그룹, Firebase 새 프로젝트 `nana-arrow` + google-services.json / GoogleService-Info.plist

## 프로그래머

### W-015 코어 전환: 경로형 Arrow (최우선. 브랜치 `feat/path-arrows`, base main, 스택 PR 금지)
- `Arrow`: cells 를 순서 있는 경로로. `Head = cells[^1]`, `Direction` 은 길이≥2 면 마지막 두 셀에서 계산(JSON dir 과 불일치 시 로더 예외), 길이 1 이면 JSON dir. `Tail`, `Length` 추가
- `ArrowType.Long` 삭제, 관련 config(`longMin/Max`) → `maxArrowLength`(GameConfig, 기본 12)
- `LevelValidator` 규칙 2 교체: 인접 셀 상하좌우 연결 / 자기 겹침 없음 / dir 과 마지막 세그먼트 일치 / 길이 범위. 규칙 4(탐욕 시뮬)는 레인만 보므로 그대로
- `FireResolver`·`TapHandler`·`LivesTracker`·`GameSession`: 변경 없음 확인 (테스트로 증명)
- `TapResult` 에 `Lane`(머리 앞 직선 셀 목록) 추가 — 빨간 번쩍·미리보기용
- **뷰 재작성**: `ArrowView` 를 경로 폴리라인(LineRenderer 또는 스프라이트 세그먼트, 직각 코너) + 머리 화살촉으로. Fire 연출 = 머리가 레인을 직진, 몸통은 경로를 따라 뱀처럼 이동, 꼬리가 나가면 제거 (`fireSpeedCellsPerSec`). Block = 레인 빨간 번쩍 + 머리 살짝 튕김. Marked = 선 전체 빨강. Frozen/Locked 표시는 머리 위
- `TapInput`: 탭은 Arrow 의 **어느 셀이든** 히트. **길게 누르기** → `LanePreviewRequested(arrow)` / 떼면 해제 (`longPressSeconds`)
- `BoardView`: 격자 옅게, 셀은 정사각 보장 (현재 Game 뷰 16:9 에서 셀이 4:1 로 찌그러지는 문제 — 원인 확인해 수정), 세로 화면에서 보드가 폭을 거의 채우도록 `areaWidthFraction` 추가
- 테스트: Arrow 경로/Head/Direction, Validator 신규 규칙, 기존 전부 통과
- 보고에 "팀장 에디터 할 일" + 스크린샷 경로


### W-010 인프라 이식 (선행: W-015. 브랜치 `feat/services-infra`, base main)
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

### W-016 레벨 1~20 경로형으로 재설계 (최우선)
- GAME_RULES v0.6 §0, LEVEL_FORMAT v0.5 읽기. 레퍼런스 Arrows – Puzzle Escape 초반 레벨 구조 조사 (스크린샷·플레이 영상): 보드 크기, 화살표 수, 꺾임 수, 첫 튜토리얼 흐름
- LEVEL_DESIGN v1.0: 난이도 지표에 "총 경로 길이", "꺾임 수" 추가. Lv1~2 직선만, Lv3 첫 꺾임, Lv6 이후 긴 경로
- 레벨 1~20 JSON 재작성 → `Assets/_Project/Levels/` 덮어쓰기. 검증은 프로그래머 W-015 의 새 Validator 가 머지된 뒤 (그 전엔 손 검증 + 시뮬레이터)
- 생성기: 역방향(빈 보드에서 Arrow 를 하나씩 "되돌려 넣기") 로 경로형 지원


### W-012 UI_FLOW v0.2 + 프리팹 가이드
- GAME_RULES §11 의 디렉터 답변 5건을 UI_FLOW 에 반영 (v0.2)
- §12 캔버스·팝업 프리팹 구조를 **팀장이 에디터에서 따라 만들 수 있는 단계별 가이드** 로 확장 (오브젝트 이름, 앵커, 컴포넌트, 어떤 스크립트를 붙이는지). 프로그래머 W-010 의 PopupBase 를 전제
- `TutorialConfig` SO 필드 정의 (레벨·대상 arrow id·문구·손가락 위치)

### W-013 레벨 21~30 (7×7, Key/Locked 26 도입) — W-016·W-012 다음
- LEVEL_DESIGN v0.4, JSON 을 직접 `Assets/_Project/Levels/` 에 저장하고 프로젝트 LevelValidator 로 검증 (앞으로 레벨 파일 저장은 기획자 담당, 프로그래머는 손대지 않음)

### W-014 애널리틱스 이벤트 목록 (이슈 #26) — W-013 다음
- level_start / level_clear / level_fail / continue / retry / ad_shown 등 이벤트명·파라미터 표 → docs/ANALYTICS.md

---

## 결정 사항 (참고)
- **레퍼런스 게임: Arrows – Puzzle Escape (Lessmore GmbH)** — 팀장·대표 확정 09-17. 코어 = 경로형 화살표
- PR 은 스택 금지, 항상 main 기준 브랜치 하나씩 (오늘 스택 PR 역순 머지 사고)
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
