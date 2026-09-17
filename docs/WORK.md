# WORK.md — 작업 지시 (디렉터만 작성)
> 팀장은 각 AI에게 "docs/WORK.md 읽고 네 항목 처리해줘" 만 말한다.
> **이 파일은 디렉터만 수정한다.** 보고는 각자 파일에: 프로그래머 → docs/REPORT_PROGRAMMER.md, 기획자 → docs/REPORT_PLANNER.md
> 항목이 없으면 그 역할은 할 일 없음. W-### 은 GitHub 이슈 번호와 별개.

---

## 프로그래머

### W-005 머지 + 이슈 등록 + 보드 뷰 (Game 씬)
- `feat/level-data-validator` 를 PR 로 main 에 머지, origin 푸시
- docs/ISSUES.md 의 21+5건을 `gh issue create` 로 GitHub 에 등록 (라벨·마일스톤 포함). 이미 끝난 항목은 등록 후 바로 close
- **v1 에 없는 것 주의**: Undo·힌트·코인·별점 구현 금지 (CLAUDE.md 갱신됨)
- 프로그래머 보고의 가정 3건은 디렉터 승인: (1) Long 외 타입은 1칸 (2) 얼음 탭은 잠금 무관 (3) 파일 저장은 에디터 툴에서
- 새 브랜치 `feat/board-view`:
  - `Gameplay/View/BoardView` : LevelData → 셀·Arrow 스프라이트 생성, GameConfig.cellSize/cellGap 으로 화면 중앙 배치 (보드 크기 따라 자동 스케일)
  - `Gameplay/View/ArrowView` : 타입·방향별 표시, Marked(빨강)/Frozen(얼음 레이어)/Locked(자물쇠) 상태 표시, Fire·Bounce·IceBreak·Shake 연출 (시간은 GameConfig)
  - `Core/GameSession` : LevelLoader → TapHandler 연결, 클리어·실패 이벤트 발행. 광고 판단은 하지 않음
  - `Gameplay/Input/TapInput` : Input System 으로 탭 → 셀 좌표 → GameSession.Tap. `allowInputDuringFire` 준수 (Fire 중인 Arrow 만 잠금)
  - 스프라이트는 임시 (단색 사각형 + 삼각형 화살촉, 코드로 생성) — 아트는 나중에 교체
  - 팀장이 Game 씬에서 연결할 오브젝트·컴포넌트 목록을 보고에 명시

### W-009 이전 프로젝트 조사 (W-005 와 병행 가능, 조사만 — 코드 이식은 아직 금지)
- 참고 프로젝트 (읽기 전용): 
  - Block Fill Puzzle: `/Users/choiseungjin/letme/unity_/NANA_puzzle`
  - Water Sort Puzzle: `/Users/choiseungjin/letme/unity_/WaterSortPuzzle`
- 조사 항목 → docs/REPORT_PROGRAMMER.md 에 정리
  1. **광고**: AdMob + Unity Ads 를 어떻게 썼는지 (AdMob 단독 + Unity Ads 미디에이션인지, 둘 다 직접 호출인지), SDK 버전, 전면/보상형 호출 코드 위치, 앱 ID·유닛 ID 가 어디에 있는지
  2. **응모 코드**: 100단계 클리어 시 코드 생성·표시 스크립트 경로, 코드 생성 규칙(형식·시드), 홈페이지 URL, Firebase 관련 코드 유무
  3. 두 프로젝트 사이에 차이가 있으면 어느 쪽이 최신인지
  4. NANA-Arrow 에 이식할 때 어셈블리·네임스페이스 어떻게 나눌지 제안
- 이식 자체는 디렉터가 조사 결과 보고 W-010 으로 지시

### W-006 레벨 에디터 툴 (선행: W-005)
- `Editor/LevelValidatorWindow` : Levels/ 폴더 전체 검증, 결과 표시, `Record` 로 solution·minTaps 기록 저장
- `Editor/LevelCheat` : 메뉴에서 레벨 점프

---

## 기획자

### W-004 마무리
- 디렉터 답변: 보드 크기 11/21/40 **승인** → GAME_RULES v0.5.2 §4 반영됨. CLAUDE.md 의 Undo·힌트도 제거됨
- LEVEL_FORMAT v0.3 에 "Long 외 타입은 정확히 1칸" 명시 (v0.4)
- 레벨 10: 조건 완화해서 재생성 OK
- 지난 보고(채팅으로 준 것)를 docs/REPORT_PLANNER.md 맨 위에 옮겨 적기. 앞으로 보고는 그 파일에만

### W-007 UI 흐름 (W-004 마무리 다음, LEVEL_DESIGN 11~20 보다 먼저 — 프로그래머 W-005 가 곧 팝업을 필요로 함)
- docs/UI_FLOW.md : Boot→Main→Game 화면 구성, HUD(레벨·설정·하트3), 클리어 팝업, 실패 팝업(광고 이어하기/다시하기), 설정(사운드 토글) 버튼 문구

### W-008 LEVEL_DESIGN 11~20 (선행: W-007)
- 6×6, Frozen 도입(16). JSON 저장 + 풀이 검증

---

## 결정 사항 (참고)
- 광고 SDK: **AdMob + Unity Ads** (기존 두 게임과 동일) — 팀장 확정 09-17
- 응모 코드: 기존 프로젝트 코드 이식 (경로는 W-009)

## 완료
- W-001 초기화·asmdef·Gameplay 코어·테스트 49 — 프로그래머 (09-17)
- W-002 LevelLoader/Validator + W-003 규칙 v0.5.1(Key·Frozen·Locked·TapHandler·코인 제거), 테스트 110 — 프로그래머 (09-17)
- 규칙 v0.4 검토 7건 → v0.5.1 — 기획자/디렉터 (09-17)
