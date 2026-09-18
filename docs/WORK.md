# WORK.md — 작업 지시 (디렉터만 작성)
> 팀장은 각 AI에게 "docs/WORK.md 읽고 네 항목 처리해줘" 만 말한다.
> **이 파일은 디렉터만 수정한다.** 보고는 각자 파일에: 프로그래머 → docs/REPORT_PROGRAMMER.md, 기획자 → docs/REPORT_PLANNER.md
> 규칙 기준: GAME_RULES **v0.7.2**. 커밋 담당: docs/TEAM.md.

---

## 팀장 — **지금 병목. 이게 끝나야 첫 빌드**
- [ ] REPORT_PROGRAMMER 맨 위 "씬에 붙일 것" (W-011·W-017·W-020·W-010): Game 씬 Ads/LivesView/Tutorial/팝업 4종, Main 씬 MainMenu/LevelSelect/Settings/Raffle, TapInput.config, GameController.catalog·boardCamera, Main Camera 의 BoardCameraController
- [ ] LevelCatalog.asset 에 21~50 추가 (드래그), 한글 TMP 폰트 (UI_FLOW §12-1)
- [ ] Play 확인: 하트 0 → 이어하기 → 다시하기 / 핀치 줌·드래그 / Level Jump 26·40·50
- [ ] 끝나면 디렉터에게 "씬 커밋해줘" + 스크린샷
- [ ] (출시 전) AdMob 앱·광고 단위, Unity Ads 미디에이션, Firebase 새 프로젝트 + google-services.json, Android keystore

## 프로그래머

### W-028 정리 작업 (브랜치 `chore/assets-doc`)
- 디렉터 답변: (1) LEVEL_FORMAT v0.7 갱신 완료 (2) **팔레트: GAME_RULES §9 남색+연보라가 목표. 금색 FreeButtonSet 은 임시 에셋**. 코드는 색을 하드코딩하지 말고 ArrowViewStyle/UI 테마 SO 로. 정식 아트는 팀장이 교체 (3) 에셋 출처 → `docs/ASSETS.md` 정식 문서로 (프로그래머가 작성, docs 예외 허용): 파일/폴더, 출처, 라이선스, 상업 이용 가능 여부, 교체 필요 여부
- `.gitignore` 에 `Assets/Plugins/Android/FirebaseCrashlytics.androidlib/res/values/crashlytics_build_id.xml` 추가 (빌드마다 바뀌는 생성 파일)
- mask 밖 레인 통과: **현행 유지** (레퍼런스와 동일 추정). 기획자에게 "마스크 파인 곳을 정면으로 관통하는 화살표는 어색하니 피하라" 만 전달됨


### W-025 팀장 플레이 테스트 버그 (최우선, 브랜치 `fix/playtest-1`) — 09-18
UI 는 당분간 신경 쓰지 않음. 아래 순서대로. 항목마다 GameConfig/ArrowViewStyle 값으로 빼서 인스펙터 조정 가능하게.
1. **Fire 연출: 화살표가 보드 밖으로 완전히 나가야 함.** 지금은 보드 가장자리(또는 중간)에서 사라짐. 머리가 **화면 밖**(카메라 뷰 밖 + 여유 `exitMarginCells` 2)까지 직진하고 꼬리까지 화면 밖으로 나간 뒤 제거. 논리 Exit 는 즉시, 연출만 연장. 줌 상태에서도 화면 밖 기준
2. **길게 누르기 Lane 미리보기도 화면 끝까지.** 보드 가장자리에서 끊지 말고 화면 경계까지 연장해서 그림 (레퍼런스 동일)
3. **60fps**: `Application.targetFrameRate` 가 -1 이라 모바일에서 30 으로 잡힘. BootLoader 에서 `GameConfig.targetFrameRate`(기본 60) 적용, `vSyncCount` 0
4. **씬 시작 시 팝업이 전부 겹쳐 보임** (클리어·실패·응모 팝업 + HUD 동시 표시, 사진 확인). 원인: 팝업 오브젝트가 씬에 활성 상태로 저장됨. 수정: `PopupBase.Awake` 에서 무조건 `SetActive(false)`(자기 루트) — 씬 저장 상태와 무관하게. `GameController.Start` 에서도 모든 팝업 닫기 호출. 팀장에게 '씬에서 팝업 4종 비활성 상태로 저장' 도 요청
5. **레벨 선택 화면에서 뒤로가기 안 됨** (안드로이드 뒤로가기·화면 뒤로가기 버튼 둘 다 확인). `PopupBase.ConsumedBack`/`LevelSelectView` 경로 점검
6. **난이도 라벨(어려움/쉬움) 가운데 정렬 안 됨** — 텍스트 anchor·alignment 를 코드에서 강제하지 말고, 프리팹 쪽 문제면 "팀장이 고칠 것" 으로 보고에 적기. TMP `alignment=Center` + RectTransform 중앙 앵커
7. **`#` 토글 = 레인 가이드 (표 격자 아님).** 레퍼런스(docs/reference/ref_video 및 팀장 사진) 확인: 균일한 표가 아니라 **각 Arrow 의 머리가 있는 행(가로 방향 Arrow) 또는 열(세로 방향 Arrow)에 화면 끝에서 끝까지** 옅은 연보라 선을 긋는다. Arrow 가 없는 행·열엔 선 없음. 즉 "모든 Arrow 의 레인을 한꺼번에 보여주는" 기능. 현재의 균일 격자 `GridOverlay` 를 `LaneGuideOverlay` 로 교체: Arrow 마다 머리 셀의 행/열 중심선을 카메라 뷰 폭·높이 전체로 그림, Arrow 가 Exit 되면 그 선도 제거, 줌·팬 시 보드와 함께. 색·굵기는 ArrowViewStyle (`laneGuideColor` 연보라, `laneGuideWidthCellRatio` 0.06)
8. **빈 칸 점(dot) 추가.** 레퍼런스는 격자 토글과 무관하게 **보드 사각형 안의 빈 칸마다 옅은 점을 항상** 찍는다 (셀 중앙, 연회색, 지름 ≈ 셀의 0.12). Arrow 가 나가면 그 칸에도 점이 생김. `ArrowViewStyle.emptyCellDotColor / emptyCellDotRadiusCellRatio / showEmptyCellDots(기본 켜짐)`. 보드 바깥엔 없음
- 각 항목 수정 후 Play 로 확인, 보고에 항목별 "수정됨 / 팀장 확인 필요" 표시


### W-021 v0.7.2 반영 + 빌드 준비 (브랜치 `feat/build-prep`)
- 디렉터 승인: W-011 가정 5건 전부 (실패 횟수 정의, 이어하기 버튼 숨김, 광고 경합 처리, Firestore 컬렉션명, ID 교체 시점). `raffle.status.*` 키 6개 승인
- (W-025 7 로 대체됨: GridOverlay → LaneGuideOverlay) HUD `DifficultyLabel`(meta.difficulty 1~3 → `hud.difficulty.easy/normal/hard`) + `GridToggleButton` — W-017 에 없으면 추가. `zoomMin` 기본 0.5
- **Android 빌드**: Build Profile(Android, IL2CPP, ARM64+ARMv7, minSdk 25), `Editor/BuildScript`(메뉴 한 번에 개발 빌드 APK → `Builds/`), keystore 는 팀장 (경로·비번은 로컬 파일, git 제외). EDM Force Resolve 자동화
- `docs/BUILD.md`: 팀장이 따라 할 개발 빌드 → 폰 설치 순서 (5줄 이내)
- 실기 체크리스트 `docs/QA_DEVICE.md`: 첫 빌드에서 확인할 20항목 (터치·줌·광고 테스트 ID·저장·회전 잠금·Safe Area·뒤로가기)

### W-027 비직사각 보드 (mask) — 팀장·대표 요청 09-18 (선행: W-025. 브랜치 `feat/board-mask`)
- LEVEL_FORMAT v0.7: `mask` 필드 = 문자열 배열, 위→아래 행, `#` 포함 칸 / `.` 제외 칸. 없으면 전체 사각형. 예: 하트
```
"mask": [ ".##.##.", "#######", "#######", ".#####.", "..###..", "...#..." ]
```
- 로더: mask 파싱 → `Board.IsInMask(x,y)`. 마스크 밖 칸은 **빈 칸 취급 — 레인 판정·Fire 로직 변경 없음**(사각형 경계까지 그대로)
- 검증기 규칙 1 확장: 모든 Arrow 셀이 mask 안. mask 행 수=height, 열 수=width
- `EmptyCellDots`: mask 안 빈 칸에만 점. `LaneGuides`·`BoardLayout` 은 변경 없음 (바운딩 박스 = width×height)
- 테스트: 로더·검증기·점 개수
- 기획자용: `LevelValidatorWindow` 에 mask 를 아스키로 미리보기

### W-022 팀장 조립 결과 검토 (선행: 팀장이 씬 커밋)
- 씬을 열어 연결 누락(빈 SerializeField) 자동 검사 `Editor/SceneLintWindow` + 결과 보고. 누락은 고치지 말고 목록만

## 기획자

### W-026 레벨 다양성 — 추가: mask 레벨에서 마스크 파인 곳(`.`)을 정면으로 관통하는 레인은 보기에 어색하니 피할 것 (로직상은 통과함)

### (원문) W-026 레벨 다양성 (최우선 — 팀장 플레이 피드백 09-18: "10단계쯤부터 전부 똑같다. 가로세로도 똑같다. 얼음·자물쇠는 좋다")
원인: 보드 크기가 구간별로 한 칸씩만 커지고, 매 레벨 점유율 90~100% 로 꽉 채우는 같은 공식. 레퍼런스는 레벨마다 **보드 모양·밀도·리듬**이 다르다 (reference/ 사진·영상 참고).
- **다양성 규칙을 LEVEL_DESIGN v1.2 에 추가하고 10~50 을 재설계** (1~9 는 유지). 51~80 은 이 규칙으로 처음부터
  1. **연속 3레벨이 같은 가로×세로이면 안 된다.** 구간 상한 안에서 매 레벨 다르게: 세로 긴 것(6×12), 가로 넓은 것(10×7), 정사각(8×8), 작은 것(5×6) 을 섞는다. 크기가 "단조 증가" 하지 않게
  2. **점유율 40~100% 를 오간다.** 꽉 찬 판(95%) 다음엔 성긴 판(50%, 화살표 적고 길게 꼬임) 식으로. 5레벨 안에 최소 1개는 60% 이하
  3. **화살표 수 × 길이 조합**을 바꾼다: (많고 짧게) / (적고 아주 길게, 30칸+) / (중간) — 같은 조합 2연속 금지
  4. **모양 테마**: 나선형, 테두리 한 바퀴 도는 긴 화살표, 두 덩어리로 갈라진 판, 중앙이 빈 판(도넛), 대각선 계단 — 10레벨마다 각 테마 1회 이상
  5. **기믹 배치 리듬**: Frozen·Locked 는 "이 레벨의 핵심 한 곳" 으로 — 여기저기 뿌리지 말고 한 판에 1~3개, 그것 때문에 순서가 갈리게. 팀장이 이 기믹을 좋아하니 16 이후 3레벨에 1번 이상 등장
  6. **레벨 리듬**: 5레벨 단위로 "쉬움-보통-어려움-어려움-숨돌리기". **숨돌리기 레벨 = 모양 판** (W-027 mask): 하트·별·돛단배·나비·집·물고기·음표 등. 첫 등장 레벨 10. LEVEL_FORMAT v0.7 의 `mask` 를 아스키로 그려서 넣는다 (프로그래머 W-027 머지 전엔 JSON 만 준비)
  7. 첫 탭 후보(free0) 가 3개 이상인 판과 1개뿐인 판을 섞는다 (후자는 "찾는 재미")
- 각 레벨에 `meta.theme` 문자열(위 4 의 테마명 또는 "dense"/"sparse") 과 `meta.difficulty` 기록
- 재설계 후 10~50 을 보드 그림 한 장(ASCII 또는 표)으로 REPORT 에 첨부 — 디렉터·팀장이 한눈에 다양성 확인
- Validator 통과 후 저장. 커밋은 디렉터


### W-023 레벨 51~80 (W-026 규칙 적용 후)
- 디렉터 승인: 51 → 9×13, 61 → 10×13, 81 → 10×14. 45~50 방식(화살표 수 대신 경로 길이) 유지
- 각 레벨 `meta.difficulty` 1~3 부여 기준을 LEVEL_DESIGN 에 추가하고 **1~50 에도 값 채우기** (HUD 난이도 라벨용)
- 프로젝트 Validator 통과 후 저장. 커밋은 디렉터

### W-024 UI_FLOW v0.3 + 스토어 초안 (W-023 과 병행)
- UI_FLOW: 난이도 라벨, `#` 격자 토글(우하단), HUD 구분선, 우상단 예약, 줌·팬 제스처 vs 탭 규칙(GAME_RULES §10), `hud.difficulty.*`·`raffle.status.*` 키
- `docs/STORE.md` 초안: 앱 이름 후보 3개(한/영), 짧은 설명·긴 설명(한국어), 스크린샷 6장 구성안, 개인정보처리방침 필요 항목(AdMob·Firebase)

---

## 결정 사항 (참고)
- 레퍼런스: Arrows – Puzzle Escape (Lessmore). 코어 = 경로형 화살표. reference/ 에 스크린샷 4장 + 영상
- 광고: AdMob 단독 + Unity Ads 미디에이션. ID 신규 발급. Firebase 새 프로젝트
- 커밋 담당: TEAM.md. 디렉터가 docs·레벨·에셋, 프로그래머가 코드
- 어셈블리: Core / Gameplay / Data / Services / UI / Editor / Tests

## 완료
- W-025 플레이 테스트 버그 8건 + 5번 재수정(TMP raycastTarget) + 에디터 UI 뭉침(SafeArea) — 프로그래머 (09-18)
- W-027 비직사각 보드 mask, PR #46, 테스트 321 — 프로그래머 (09-18)
- W-001~009 (09-17) 초기화·코어·로더·검증기·보드뷰·이전 프로젝트 조사
- W-012·014·016 (09-17) UI_FLOW v0.2, ANALYTICS, 경로형 레벨 1~20 — 기획자
- W-015 (09-17) 경로형 Arrow 전환 PR #30 — 프로그래머
- W-010 인프라 이식 / W-020 줌·팬 / W-017 UI 스크립트 PR #33 / W-011 광고·응모 PR #34 / 인수인계 PR #35, 테스트 267 — 프로그래머 (09-17~18)
- W-013·018·019 (09-18) 레벨 21~50, LEVEL_DESIGN v1.1, LEVEL_FORMAT v0.6, v0.7 검토 — 기획자
- GAME_RULES v0.7 → v0.7.2 — 디렉터
