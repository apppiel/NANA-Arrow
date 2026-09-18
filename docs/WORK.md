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

### W-021 v0.7.2 반영 + 빌드 준비 (브랜치 `feat/build-prep`)
- 디렉터 승인: W-011 가정 5건 전부 (실패 횟수 정의, 이어하기 버튼 숨김, 광고 경합 처리, Firestore 컬렉션명, ID 교체 시점). `raffle.status.*` 키 6개 승인
- v0.7.2: `GridOverlay` 토글이 W-020 에 들어갔는지 확인, 없으면 추가. HUD `DifficultyLabel`(meta.difficulty 1~3 → `hud.difficulty.easy/normal/hard`) + `GridToggleButton` — W-017 에 없으면 추가. `zoomMin` 기본 0.5
- **Android 빌드**: Build Profile(Android, IL2CPP, ARM64+ARMv7, minSdk 25), `Editor/BuildScript`(메뉴 한 번에 개발 빌드 APK → `Builds/`), keystore 는 팀장 (경로·비번은 로컬 파일, git 제외). EDM Force Resolve 자동화
- `docs/BUILD.md`: 팀장이 따라 할 개발 빌드 → 폰 설치 순서 (5줄 이내)
- 실기 체크리스트 `docs/QA_DEVICE.md`: 첫 빌드에서 확인할 20항목 (터치·줌·광고 테스트 ID·저장·회전 잠금·Safe Area·뒤로가기)

### W-022 팀장 조립 결과 검토 (선행: 팀장이 씬 커밋)
- 씬을 열어 연결 누락(빈 SerializeField) 자동 검사 `Editor/SceneLintWindow` + 결과 보고. 누락은 고치지 말고 목록만

## 기획자

### W-023 레벨 51~80
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
- W-001~009 (09-17) 초기화·코어·로더·검증기·보드뷰·이전 프로젝트 조사
- W-012·014·016 (09-17) UI_FLOW v0.2, ANALYTICS, 경로형 레벨 1~20 — 기획자
- W-015 (09-17) 경로형 Arrow 전환 PR #30 — 프로그래머
- W-010 인프라 이식 / W-020 줌·팬 / W-017 UI 스크립트 PR #33 / W-011 광고·응모 PR #34 / 인수인계 PR #35, 테스트 267 — 프로그래머 (09-17~18)
- W-013·018·019 (09-18) 레벨 21~50, LEVEL_DESIGN v1.1, LEVEL_FORMAT v0.6, v0.7 검토 — 기획자
- GAME_RULES v0.7 → v0.7.2 — 디렉터
