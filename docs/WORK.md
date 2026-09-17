# WORK.md — 작업 지시 (디렉터만 작성)
> 팀장은 각 AI에게 "docs/WORK.md 읽고 네 항목 처리해줘" 만 말한다.
> **이 파일은 디렉터만 수정한다.** 보고는 각자 파일에: 프로그래머 → docs/REPORT_PROGRAMMER.md, 기획자 → docs/REPORT_PLANNER.md
> 항목이 없으면 그 역할은 할 일 없음. W-### 은 GitHub 이슈 번호와 별개.
> **규칙 문서는 GAME_RULES v0.7 (2026-09-17 전면 재작성) 이 유일한 기준.** 이전 버전 기억은 버릴 것.

---

## 팀장
- [ ] 문서 커밋: `git add docs .claude && git commit -m "docs: GAME_RULES v0.7 rewrite, WORK next tasks" && git push`
- [ ] REPORT_PROGRAMMER W-015 에디터 할 일 4~7 (ArrowViewStyle 값 확인, Play 로 탭·길게 누르기 확인, Level Jump 로 새 1~20 확인, 9:16 에서 선 굵기·화살촉 조정)
- [ ] UI_FLOW §12-1 **한글 TMP 폰트** 준비 (W-017 전에 필요)
- [ ] (출시 전) AdMob 앱·광고 단위, Unity Ads 미디에이션, Firebase 새 프로젝트

## 프로그래머
PR 은 이제 프로그래머가 직접 머지 (`.claude/settings.json` 허용됨). main 기준 브랜치 하나씩, 스택 금지.

### W-015 후속 (작은 것, `feat/services-infra` 에 같이 실어도 됨)
- GAME_RULES v0.7 반영: `maxArrowLength` 기본 40, `maxBoardSize` → `maxBoardWidth`(10)·`maxBoardHeight`(14) 분리 (Validator 규칙 0 포함)
- **셀 크기 규칙 교체** (§9): `cell = min(화면폭 × cellWidthFraction(0.052), 화면폭 × maxAreaFraction(0.9) ÷ 가로칸수)`. `areaWidthFraction` 삭제. GameConfig 에 두 값 추가
- Validator 규칙 **2-e**: 자기 Lane 위에 자기 몸통 셀이 있으면 오류 (프로그래머 가정 "자기 몸통은 안 막음" 은 승인하되 레벨에서 금지)
- `.claude/settings.json` 커밋

### W-010 인프라 이식 (브랜치 `feat/services-infra`)
디렉터 승인: W-009 §4 어셈블리 제안 그대로. `NanaArrow.Services` asmdef 신설, Core 는 Services 미참조.
- SDK 폴더 복사 (NO.3 SpotTheDifference 에서): `Assets/GoogleMobileAds`, `Assets/Firebase`, `Assets/ExternalDependencyManager`, `Plugins/Android/*.androidlib`, gradle 템플릿 3종. **CLAUDE.md "절대 복사 금지" 항목은 테스트 ID / 빈 값으로**
- 그대로 이식: `SaveService`+`SaveCodec`+`SaveData`(필드: 최고 레벨·응모 코드 발급 여부·사운드·진동, 테스트 포함), `ScreenCaptureProtection`, `SafeAnalytics`, NO.2 `SafeAreaAdapter`
- 수정 이식: `BootLoader`(Addressables 제거), NO.2 `SceneLoader` → `Core.SceneLoader`(씬 enum), NO.2 `AudioManager` 뼈대(클립 = UI_FLOW 사운드 13종 SerializeField), 뒤로가기 `Keyboard.escapeKey` 부분만
- 새로: `Data/LevelCatalog` SO (TextAsset 리스트) + `GameController` 카탈로그 로드. `PopupBase`. 치트 즉시 클리어·하트 채우기. `TapInput` 에 `EventSystem.IsPointerOverGameObject` 필터
- 설정(사운드·진동) PlayerPrefs. 진동은 하트 감소 시만
- `AnalyticsReporter` 뼈대: docs/ANALYTICS.md v0.1 의 이벤트 18종을 상수로, 전송은 한 곳에서만 (SafeAnalytics 경유)

### W-017 UI 스크립트 (선행: W-020. 브랜치 `feat/ui-scripts`)
UI_FLOW v0.2 §12-8 의 8건: `LivesView`(하트 3개), `TutorialPresenter`(TutorialConfig SO + 말풍선·손가락), `MainMenu`, `LevelSelectView`, `SettingsPopup`(사운드·진동), `GameController` 재시작/메인 이동, `PopupBase` 닫기 옵션, `Strings` SO(문구 키). 프리팹·씬 조립은 팀장 (UI_FLOW §12 가이드). 보고에 "씬에 붙일 것" 목록.

### W-020 보드 줌·팬 (선행: W-010. 브랜치 `feat/board-zoom`) — **대표 요구사항, W-017 보다 먼저**
- GAME_RULES v0.7.1 §10 "보드 확대·축소·이동" 그대로. `Gameplay/Input/BoardCameraController` (Main Camera orthographicSize + position)
- `TapInput` 개편: 터치 시작 → (a) `dragThresholdCells` 이상 이동 → 드래그(팬) 확정, 탭 아님 (b) `longPressSeconds` 경과 → 미리보기 (c) 그 전에 놓음 → 탭. 두 손가락 감지 시 즉시 핀치 모드, 진행 중이던 탭·미리보기 취소
- 에디터: 마우스 휠 줌, 마우스 드래그 팬 (테스트용)
- 클램프·리셋·더블탭 로직은 순수 C# (`BoardCameraModel`) 로 분리해 테스트
- 팀장 에디터 할 일: Main Camera 에 컴포넌트 붙이고 Config 연결

### W-011 광고·응모 코드 (선행: W-017. 브랜치 `feat/ads-reward`)
- `Core/AdsManager` 순수 C#: NO.3 `InterstitialGate` 를 AdsConfig 값으로 확장. 입력 `LevelCleared(level, alreadyCleared)` / `RetryPressed(failCount)` / `ContinueRequested(continuesUsed)` → 광고 종류. 테스트 필수
- `Core/IInterstitialAd`, `IRewardedAd` + `Services/AdMobService`(NO.3, 테스트 ID). 에디터는 광고 스킵
- `Core/RewardCode`(+테스트) + `Services/RewardCodeService`. `RewardConfig` SO (홈페이지 URL)
- `RewardCodePanel` 뼈대

## 기획자

### W-013 레벨 21~30 (7×7 아님 — LEVEL_DESIGN v1.0 구간대로 8×10~8×11, Key/Locked 26 도입)
- 디렉터 답변: `maxArrowLength` 40 승인, 보드 가로·세로 분리 승인(10×14 상한), 셀 크기 규칙 승인, Lv4 튜토리얼 승인, 게임 중 설정 없음 승인. W-016 확인 요청 전부 처리됨
- 프로그래머 W-015 후속(규칙 2-e, 보드 상한) 머지 후 프로젝트 Level Validator 로 검증. 그 전엔 기획 측 검사기 + 2-e 수동 확인
- JSON 저장 → `Assets/_Project/Levels/level_021~030.json`

### W-018 레벨 31~50 (선행: W-013)
- 8×11~9×12, Frozen·Key/Locked 혼합. 40 부근 보드 확대

### W-019 GAME_RULES v0.7 검토 (W-013 과 병행, 30분 이내)
- 이번 재작성이 v0.5~0.6 결정과 어긋난 곳이 있으면 REPORT_PLANNER 에 목록. 없으면 "이상 없음"

---

## 결정 사항 (참고)
- 레퍼런스: **Arrows – Puzzle Escape (Lessmore GmbH)** — 코어 = 경로형 화살표
- 광고 SDK: AdMob 단독 + Unity Ads 미디에이션. ID 신규 발급. Firebase 새 프로젝트
- 응모 코드: NO.3 RewardCode 이식
- 레벨 파일 저장 담당: 기획자. 프로그래머는 로더·검증기만
- PR 스택 금지. 프로그래머가 직접 머지
- 어셈블리: Core / Gameplay / Data / Services / UI / Editor / Tests

## 완료
- W-001 초기화·asmdef·Gameplay 코어 — 프로그래머 (09-17)
- W-002·003 LevelLoader/Validator/TapHandler, 규칙 v0.5.1 — 프로그래머
- W-004 ISSUES 26건, LEVEL_FORMAT v0.4, LEVEL_DESIGN 1~10 — 기획자
- W-005·006 보드 뷰·GameSession·GameController·TapInput, 검증기 창·치트, 이슈 등록 — 프로그래머
- W-007 UI_FLOW v0.1 / W-008 LEVEL_DESIGN 11~20 (직선형, 폐기) — 기획자
- W-009 이전 프로젝트 3종 조사 — 프로그래머
- **W-015 경로형 Arrow 전환 (PR #30, 테스트 155)** — 프로그래머 (09-17)
- **W-016 경로형 레벨 1~20, LEVEL_FORMAT v0.5, LEVEL_DESIGN v1.0** — 기획자 (09-17)
- **W-012 UI_FLOW v0.2 + 팀장 조립 가이드** — 기획자 (09-17)
- **W-014 ANALYTICS.md v0.1 (이벤트 18종)** — 기획자 (09-17)
- 규칙 → GAME_RULES v0.7 전면 재작성 — 디렉터 (09-17)
