# NANA-Arrow — GAME_RULES.md (v0.6.1 — 레퍼런스 스크린샷 반영: 격자 없음, 보드 작게, HUD 미니멀, 꺾임 Lv2 부터)
> **레퍼런스: Arrows – Puzzle Escape (Lessmore GmbH, com.ecffri.arrows)** — 5천만+ DL, 퍼즐 1위. 아래 §0 이 v0.5 까지의 "직선 막대" 모델을 대체한다. §0 과 충돌하는 이전 조항은 §0 이 우선.

## 0. 코어 전환 (v0.6) — 화살표는 그리드를 따라 꺾이는 선
- **Arrow = 순서 있는 셀 경로** `cells[0]`(꼬리) → `cells[n-1]`(머리). 인접 셀은 상하좌우로 연결, 직각 꺾임 자유, 자기 자신과 겹치지 않음. 길이 1~`maxArrowLength`(=12)
- **Dir = 머리의 진행 방향** = 마지막 두 셀의 방향 (길이 1이면 JSON 의 `dir` 사용, 길이 2 이상이면 `dir` 은 검증용으로 일치해야 함)
- **Fire**: 머리가 Dir 방향으로 직진하고 몸통은 자기 경로를 따라 뱀처럼 따라간다. 꼬리까지 보드 밖으로 나가면 Exit
- **Block 판정은 v0.5 와 동일**: 머리 앞 칸부터 보드 가장자리까지 직선 레인이 비어 있어야 한다 (다른 Arrow 의 어떤 셀도 없어야 함). 몸통은 자기 경로로만 이동하므로 레인만 검사하면 충분
- **레인 미리보기 (레퍼런스 기능)**: Arrow 를 길게 누르면 그 Arrow 의 레인(머리 앞 직선 + 자기 경로)이 하이라이트. 목숨 차감 없음, 손 떼면 사라짐. `longPressSeconds`(=0.35)
- 타입 정리: `Long` 삭제 (모든 Arrow 가 경로). 남는 타입 = Basic / Frozen / Locked / Key
- **그래픽 (레퍼런스 스크린샷 docs/reference/ 기준, 2026-09-17 팀장 제공)**
  - **격자 없음, 셀 배경 없음.** 순백(#FFFFFF) 배경에 선만 그린다
  - 선: 짙은 남색(#141A33 근처), 굵기 = 셀의 약 15% (`lineWidthCellRatio` 0.15), **꺾임은 둥근 모서리**(round join), 꼬리 끝도 둥글게(round cap)
  - 머리: 선 끝에 삼각 화살촉 (`arrowHeadLengthCellRatio` 0.45, `arrowHeadWidthCellRatio` 0.45)
  - 선 사이 간격 = 셀 1칸. 인접한 두 Arrow 가 나란히 붙어 있어도 구분되게 굵기 여유 둘 것
  - **보드는 작게, 여백 크게**: 보드 폭이 화면 폭의 약 40~50% (`areaWidthFraction` 0.5). 세로 중앙. 큰 보드(10칸)도 이 안에 축소
  - 색은 `ArrowViewStyle` 로 교체 가능 (v1 은 단색 클래식만)
- **HUD (레퍼런스 그대로)**: 좌상단 원형 버튼 2개 — 뒤로가기, 다시하기 (연한 보라 배경). 상단 중앙 하트 3개(빨강). **레벨 번호·설정 버튼은 게임 화면에 없음** (설정은 Main 에서). 튜토리얼 문구는 하단 말풍선 (문구: 이동하려면 탭하세요)
- 나중(v1.1): 하트·별 모양 등 비직사각 보드 마스크 — LEVEL_FORMAT 에 `mask` 예약
- 레퍼런스 확인된 룰 (그대로 채택): 하트 3개/레벨, 막히면 레인이 빨갛게 번쩍 + 하트 -1, 타이머·이동 제한 없음, 하트 0 → 광고 보고 +1 이어하기, 힌트 있음(우리는 v1 제외 유지)

> 작성: 클로드 데스크탑 / **v1 범위 확정** (2026-09-17). 변경 시 버전 올리고 CLAUDE.md 담당에게 알릴 것.
> 모든 수치는 `GameConfig` ScriptableObject에서 인스펙터로 조정한다. 여기 적힌 값은 초기 기본값.

## 1. 용어
| 용어 | 정의 |
|---|---|
| Board | N×M 셀 그리드. 원점 (0,0)은 좌하단, x는 오른쪽, y는 위 |
| Cell | 그리드 한 칸. 비어 있거나 Arrow 한 개의 일부를 담는다 |
| Arrow | 1칸 이상을 차지하는 블록. `Dir` 하나를 가진다 (Up/Down/Left/Right) |
| Head | Arrow에서 Dir 방향의 맨 앞 칸 |
| Fire | Arrow를 탭해 Dir 방향으로 발사하는 행위 |
| Exit | Arrow가 보드 밖으로 완전히 나가 제거되는 것 |
| Block | Head 앞 칸에 다른 Arrow가 있어 Fire가 실패하는 것 |

## 2. 코어 룰
1. 플레이어가 Arrow를 탭하면 Head 앞 칸부터 보드 가장자리까지 검사한다.
2. 경로가 모두 비어 있으면 Arrow는 보드 밖으로 날아가 Exit 된다.
3. 경로에 다른 Arrow가 있으면 Block: Arrow는 그 앞까지 밀렸다가 원위치로 튕긴다 (연출), 상태 변화 없음.
4. 보드의 모든 Arrow가 Exit 되면 레벨 클리어.
5. ~~Undo~~ — v1 미포함 (부스터 전체 제외). 목숨+Marked 규칙이 실수를 흡수하므로 불필요.
6. **실패 조건: 목숨 3개** (`maxLives` = 3). 시간 제한 없음, 이동 횟수 제한 없음.
   - Block 된 Arrow를 탭하면 목숨 -1 하고 그 Arrow는 **빨간색(Marked)** 상태가 된다
   - Marked 상태의 Arrow를 다시 탭해서 또 Block 되어도 **목숨은 깎이지 않는다** (배려 규칙)
   - Marked 는 보드가 바뀌면(어떤 Arrow든 Exit 되면) 전부 해제된다 — 막힘 상황이 달라졌으므로 다시 판단해야 함 (`markedResetOnExit` = true)
   - Marked Arrow가 Fire에 성공하면 그냥 Exit 된다
   - 목숨 0 → 레벨 실패 팝업 (아래 두 버튼)
     - **광고 보고 이어하기**: 보상형 광고 시청 → 현재 보드 유지, 목숨 +`continueLives`(=1). 레벨당 `maxContinues`(=1)회. 모든 값 AdsConfig 인스펙터
     - **다시하기**: 무료, 레벨 처음부터. 목숨 3 리셋
   - 목숨은 레벨 시작 시 항상 3으로 리셋 (레벨 간 공유 없음)

## 3. Arrow 종류 (도입 순서)
| 타입 | 설명 | 도입 레벨(초안) |
|---|---|---|
| Basic | 경로 1~N칸, 꺾임 자유 | 1 (직선 3개, 튜토리얼) / **2 (첫 꺾임)** |
| ~~Long~~ | v0.6 에서 삭제 — 모든 Arrow 가 경로 | - |
| Frozen | 탭 1회로 얼음 해제, 2회째 Fire | 16 |
| Locked | 같은 색 Key Arrow가 Exit 되어야 해제 | 26 |
| Bomb | Fire 시 인접 8칸 Arrow 강제 Exit | **보류** (v1 미포함) |

각 타입은 `ArrowType` enum + 타입별 설정은 `ArrowTypeConfig` ScriptableObject.

## 4. 레벨 구조
- 보드 크기: 5×5 (Lv1~10) → 6×6 → 7×7 → 8×8 (Lv40+). 최대 `maxBoardSize` = 10
- 셀 크기와 간격은 `cellSize`, `cellGap` 으로 화면에 맞춰 자동 스케일
- 레벨 데이터: `Assets/_Project/Levels/level_###.json` (스키마는 LEVEL_FORMAT.md)
- 정답 보장: 모든 레벨은 검증기가 "해결 가능" 판정을 통과해야 저장 가능

## 5. 별점 (재플레이)
- ⭐⭐⭐: 최소 탭 수 이하 / ⭐⭐: +`star2Tolerance`(=3) 이내 / ⭐: 클리어
- **별점 시스템은 v1 미포함** (보류). 위 기준은 도입 시 초안

## 6. 힌트 / 부스터
**v1 전체 제외.** (Hint, Undo, Shuffle 모두 미포함) 이후 리텐션 지표 보고 Hint 부터 검토.
- 보상형 광고는 "목숨 0 → 이어하기" 한 곳에만 사용

## 7. 진행 / 보상
- 레벨 클리어 → `coinPerClear` 코인, 별 3개 시 보너스
- **100단계 클리어 → 응모 코드 표시 (확정)**. 기존 홈페이지 Firebase 응모 구조 재사용, 코드 생성 규칙은 Water Sort/Block Fill 과 동일하게
- 광고 정책 (확정, 전부 `AdsConfig` ScriptableObject)

| 위치 | 종류 | 값 | 기본 |
|---|---|---|---|
| 목숨 0 → 이어하기 | 보상형 | `continueLives`, `maxContinues` | 1, 1 |
| 레벨 클리어 후 | 전면 | `interstitialEveryNLevels`, `adFreeLevels` | 3, 5 (1~5레벨 광고 없음) |
| 다시하기 반복 | 전면 | `interstitialAfterFails` | 2 (같은 레벨 2번 실패마다) |
| 광고 제거 IAP | - | **v1 미포함** (인앱 결제 없음) | - |

- 광고 로직은 `AdsManager` 한 곳에서만 판단. 게임플레이 코드는 "클리어됨/실패됨/다시하기 눌림" 이벤트만 보낸다

## 8. 연출 타이밍 (모두 GameConfig)
| 값 | 기본 |
|---|---|
| fireSpeedCellsPerSec | 14 (경로 길이에 비례, 고정 시간 아님) |
| blockBounceDuration | 0.15s |
| blockBounceDistance | 0.2 cell |
| clearPopupDelay | 0.6s |
| cellSpawnStagger | 0.03s |
| cellSpawnDuration | 0.15s |
| iceBreakDuration | 0.15s |
| lockShakeDuration | 0.2s |
| lockShakeDistance | 0.08 cell |

- Block 튕김: 막은 Arrow 직전(FreeCells)까지 + blockBounceDistance 전진 후 복귀, 편도 blockBounceDuration

## 9. 화면 / 입력
- 세로 고정, 보드는 화면 중앙, 상단 HUD(레벨·코인·설정), 하단 부스터 바
- 입력: Input System, 탭만 사용 (드래그 없음). **연속 탭 허용** (`allowInputDuringFire` = true)
  - 판정은 탭 즉시 논리 보드에서 확정하고, 연출은 뒤따라감 (논리와 연출 분리 필수)
  - 이미 Fire 중인 Arrow는 다시 탭 불가. 날아가는 중인 Arrow가 차지했던 칸은 논리상 이미 비어 있음

## 10. 씬 흐름
Boot(설정·저장 로드) → Main(시작·설정·레벨 선택) → Game(레벨 N) → 클리어 팝업 → 다음 레벨 / Main

## 11. UI / 진행 결정 (UI_FLOW v0.1 확인 요청에 대한 답, 2026-09-17)
- 진동 토글: **추가**. 기본 켜짐, 하트 감소 시에만 진동 (`vibrateOnLifeLost`, 설정 저장)
- 설정 팝업의 "다시하기": 실패 횟수에 **포함하지 않음**, 광고 없음
- 레벨 중간 저장: **없음**. 앱 종료 시 그 레벨 처음부터
- 언어: v1 **한국어만**. 단, 문구는 UI_FLOW 의 키 표대로 코드와 분리해 둘 것 (나중에 영어 추가 대비)
- 이미 깬 레벨 재클리어: 전면 광고 카운트에 **포함**
- 레벨 로드: `LevelCatalog` ScriptableObject (TextAsset 목록, 인스펙터에서 순서 편집) — Resources 폴더 사용 안 함
- 치트 (에디터/개발 빌드 전용): 레벨 점프 + 즉시 클리어 + 목숨 채우기
- Lv20 depth 6: 허용. 플레이 테스트 후 조정
