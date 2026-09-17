# WORK.md — 작업 보드
> 팀장은 각 AI에게 "docs/WORK.md 읽고 네 담당 섹션 처리해줘" 만 말한다.
> 규칙: "현재 작업" 은 디렉터만 수정. 프로그래머·기획자는 **자기 보고 섹션만** 쓴다. 끝난 항목은 디렉터가 "완료" 로 옮긴다.
> 작업 번호 W-### 은 GitHub 이슈 번호와 별개. 항목이 없으면 그 역할은 할 일 없음.

---

## 현재 작업 (디렉터 작성)

### W-003 규칙 v0.5.1 반영 — 프로그래머
- 선행: W-002(LevelLoader/Validator) 끝난 뒤
- GAME_RULES v0.5.1, LEVEL_FORMAT v0.3 다시 읽기
- ArrowType enum 에 Key 추가 (Basic + keyGroup)
- GameConfig 에서 coinPerClear 제거
- Frozen: hits 회 탭. 마지막 탭 전 "얼음 깨기" 탭은 경로 무관·목숨 차감 없음. 마지막 탭만 Block 판정
- Locked: 같은 keyGroup 의 Key 가 모두 Exit 되기 전 탭 → 목숨 차감 없음, 흔들림용 결과값만 반환
- 탭 처리 로직(TapHandler, 순수 C#) + 테스트
- 끝나면 아래 "프로그래머 보고" 에 PR 요약 + 팀장이 에디터에서 할 일

### W-004 GitHub 이슈 목록 + 레벨 설계 — 기획자
- docs/CLAUDE_DESKTOP.md 의 "v0.5 검토 잔여" 는 해결됨 확인
- 체크리스트 → GitHub 이슈 목록 (docs/ISSUES.md 로 저장, 제목·본문·라벨)
- docs/LEVEL_DESIGN.md 초안: 난이도 곡선 + 레벨 1~10 을 LEVEL_FORMAT v0.3 JSON 으로 Assets/_Project/Levels/ 에 저장
- 끝나면 아래 "기획자 보고" 에 요약

---

## 프로그래머 보고 (프로그래머만 작성)

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

---

## 기획자 보고 (기획자만 작성)


---

## 완료
- W-001 프로젝트 초기화·asmdef·Gameplay 코어·테스트 49개·ScriptableObject 3종 — 프로그래머 (2026-09-17)
- 규칙 v0.4 검토 요청 7건 → v0.5.1 반영 — 기획자/디렉터 (2026-09-17)
