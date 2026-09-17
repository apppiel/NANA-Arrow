# NANA-Arrow — ISSUES.md (GitHub 이슈 초안, 2026-09-17)
> 작성: 기획자/PM. 기준: GAME_RULES v0.5.1, LEVEL_FORMAT v0.3, LEVEL_DESIGN v0.1
> **아직 GitHub에 등록되지 않음.** 팀장이 등록하거나 클로드 코드에게 `gh issue create` 로 옮기도록 지시. 등록 후 실제 번호를 여기 `#` 옆에 적고, 브랜치는 CLAUDE.md 규칙대로 `feat/<번호>-<설명>`
> 상태 기준: 2026-09-17 에디터 점검 (현재 브랜치 `feat/level-data-validator`, 컴파일 에러 0)

라벨: `logic` `view` `editor` `data` `ads` `release` `planning` / 우선순위: P0(지금) P1(다음) P2(출시 전)

## M1 — 코어 로직 (순수 C#, EditMode 테스트 필수)

### #1 [chore] 프로젝트 기본 구성 · `release` P0 · 상태: **코드 있음, 확인만**
- asmdef 5종(Core/Data/Gameplay/UI/Editor) + 테스트 asmdef, GameConfig·AdsConfig·ArrowTypeConfig 에셋
- 완료 조건: main 에 병합, GameConfig 에 코인 관련 값이 없음(GAME_RULES §7) — 현재 코드 기준 충족

### #2 [feat] Board · Arrow · FireResolver · LivesTracker · `logic` P0 · 상태: **`feat/gameplay-core` 브랜치, PR 리뷰 대기**
- GAME_RULES §2 (Exit/Block 판정, 목숨·Marked·배려 규칙, Exit 시 Marked 해제)
- 완료 조건: 테스트 통과, 기획 리뷰 통과 후 병합

### #3 [feat] LevelData · LevelLoader · `data` P0 · 상태: **`feat/level-data-validator` 진행 중**
- LEVEL_FORMAT v0.3. Frozen hits 생략 시 ArrowTypeConfig 기본값
- 완료 조건: LEVEL_DESIGN 1~10 JSON 이 모두 파싱됨 (2026-09-17 기획 측에서 확인 완료)

### #4 [feat] LevelValidator (에디터) · `editor` P0
- LEVEL_FORMAT 검증 규칙 1~6: 보드 범위·겹침, Long 직선/축, Locked↔Key 그룹, **탐욕 시뮬레이션**으로 해결 가능 판정, solution·minTaps 재계산해서 덮어쓰기
- 선택: free0 / depth / fill 도 meta 에 기록 (LEVEL_DESIGN §1 지표)
- 완료 조건 (테스트)
  - LEVEL_FORMAT 예시 레벨 통과, minTaps = 6
  - LEVEL_DESIGN 1~10 전부 통과, Record 결과가 문서의 solution·minTaps 와 동일 (2026-09-17 기획 측 확인 완료)
  - 순환 막힘 보드(LEVEL_FORMAT v0.1 예시) → 실패
  - Key 없는 Locked → 실패 / Frozen hits 1 이하 → 실패

### #5 [feat] GameSession — 탭 1회 처리 · `logic` P0
- 입력: 탭한 Arrow. 출력: 연출용 결과 이벤트 (Exit / Block / 얼음 깨짐 / 잠김 흔들림 / 클리어 / 실패)
- 규칙 (GAME_RULES §2·§3·§9)
  - Frozen: 남은 얼음이 있으면 경로와 무관하게 얼음만 깬다. 목숨 유지. 마지막 탭만 Fire 판정
  - Locked: 같은 keyGroup 의 Key 가 남아 있으면 흔들림만, 목숨 유지
  - Key Exit → 그룹의 Key 가 모두 나갔으면 해당 Locked 전부 해제
  - Block → LivesTracker.OnBlocked, Exit → 보드에서 제거 + LivesTracker.OnExit
  - 날아가는 중인 Arrow 는 재탭 불가, 칸은 탭 즉시 논리상 비움 (연속 탭)
  - 목숨 0 → 실패, 이어하기(AdsConfig.continueLives, 레벨당 maxContinues), 다시하기(처음 상태로 리셋)
- 완료 조건: 위 항목별 테스트 + LEVEL_DESIGN 레벨 하나를 solution 순서로 끝까지 진행하는 테스트

### #6 [feat] 에디터 도구 — 레벨 저장 메뉴 · 치트 · `editor` P1
- 저장 시 #4 검증 강제, 실패하면 저장 안 됨
- 치트: 레벨 점프, 즉시 클리어, 목숨 채우기 (에디터·개발 빌드 전용)

### #7 [chore] 레벨 1~10 JSON 반영 · `data` P1 · 선행: #4
- LEVEL_DESIGN.md §6 블록을 `Assets/_Project/Levels/level_001~010.json` 으로 저장, 검증기 통과

## M2 — 화면 · 흐름

### #8 [feat] BoardView · `view` P1
- 셀 생성, 화면 크기에 맞춘 자동 스케일(cellSize·cellGap), 등장 스태거(cellSpawnStagger)

### #9 [feat] ArrowView — 타입·상태 표시 · `view` P1 · ⚠ 아트 필요
- Basic / Long(2~3칸) / Frozen(얼음, 남은 횟수) / Locked(자물쇠, 그룹 색) / Key(그룹 색), Marked 빨간색
- 색·스프라이트는 인스펙터에서 교체 가능하게

### #10 [feat] 연출 · `view` P1
- Fire, Block 튕김, 얼음 깨짐, 자물쇠 흔들림, 클리어 대기 — 시간은 GameConfig §8 값
- 논리와 분리: #5 결과 이벤트만 받아서 재생

### #11 [feat] 입력 · `view` P1
- Input System 탭 → 월드 좌표 → 셀 → Arrow. 탭만 사용, 연속 탭 허용

### #12 [feat] HUD · `view` P1
- 상단: 레벨 번호, 설정 버튼. 목숨 하트 3개 (GAME_RULES §9). **코인·부스터 UI 없음**

### #13 [feat] 팝업 · `view` P1 · 문구는 UI_FLOW 확정 후
- 클리어 / 실패(광고 보고 이어하기 · 다시하기) / 설정

### #14 [feat] 씬 흐름 · 레벨 선택 · `logic` P1
- Boot(설정·저장 로드) → Main(시작·설정·레벨 선택) → Game → 클리어 → 다음 레벨 / Main

### #15 [feat] 저장 · `logic` P1
- 최고 도달 레벨, 설정(사운드·진동), 100단계 응모 코드 발급 여부. JSON, persistentDataPath

## M3 — 수익 · 출시

### #16 [feat] AdsManager 판단 로직 · `ads` P1
- 순수 C# + 테스트. adFreeLevels(1~5 광고 없음), 클리어 N레벨마다, 같은 레벨 실패 N회마다, 이어하기 횟수
- 게임플레이 코드는 클리어됨 / 실패됨 / 다시하기 눌림 이벤트만 보낸다

### #17 [feat] 광고 SDK 연동 · `ads` P2 · ⚠ **SDK 미정 (대표님·팀장 결정 필요)**

### #18 [feat] 100단계 응모 코드 · `release` P2 · ⚠ 참고 코드 위치 필요
- 기존 홈페이지 Firebase 응모 구조 재사용, 코드 생성 규칙은 Water Sort / Block Fill 과 동일
- 팀장: 기존 프로젝트의 해당 코드 경로를 클로드 코드에 전달

### #19 [feat] 애널리틱스 · `release` P2 · ⚠ 이벤트 목록 미정 (기획 #P4 선행)

### #20 [feat] 사운드 · 진동 · `view` P2 · 선행: UI_FLOW 사운드 리스트

### #21 [chore] Android 출시 빌드 · `release` P2
- IL2CPP / ARM64, 키스토어, 버전 규칙, AAB

## 기획 트랙 (클로드 데스크탑)
| # | 작업 | 상태 |
|---|---|---|
| P1 | LEVEL_DESIGN 11~20 (6×6, Frozen 16) | 대기 |
| P2 | UI_FLOW.md (씬·팝업 흐름, 버튼 문구, 튜토리얼, 사운드 리스트) | 대기 |
| P3 | STORE.md (스토어 문구, 스크린샷 구성, 개인정보처리방침) | 대기 |
| P4 | 애널리틱스 이벤트 목록 | 대기 |
| P5 | #2·#3 PR 코드 리뷰 | PR 올라오면 |

## 권장 진행 순서
#2·#3 병합 → **#4 → #5** → #7 → #8~#12 → #14·#15 → #13 → #16 → (SDK 결정 후) #17 → #18~#21

## 팀장 확인 필요 (문서 불일치)
- CLAUDE.md 에 `Undo 스택`(Gameplay 폴더 설명·역할 3번)과 `힌트 비용`(설정 값 예시)이 남아 있음 → GAME_RULES v0.5.1 에서 둘 다 v1 제외. 클로드 코드가 Undo 를 구현하지 않도록 CLAUDE.md 정리 필요 (CLAUDE.md 는 프로그래머 문서라 기획이 직접 고치지 않음)
- 현재 브랜치 이름에 이슈 번호가 없음 (`feat/gameplay-core`) → 이슈 등록 후부터 규칙 적용
