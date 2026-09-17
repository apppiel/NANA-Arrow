# NANA-Arrow — GAME_RULES.md (v0.7.1 — §10 에 보드 확대·축소·이동 추가)
> 작성: 디렉터. **이 파일이 유일한 규칙 기준.** 이전 버전 문구와 충돌하면 이 파일이 우선.
> 레퍼런스: **Arrows – Puzzle Escape (Lessmore GmbH, com.ecffri.arrows)**. 스크린샷 docs/reference/.
> 모든 수치는 ScriptableObject(GameConfig / ArrowTypeConfig / AdsConfig / ArrowViewStyle / RewardConfig) 인스펙터 값. 여기 적힌 값은 기본값.

## 0. 한눈에
- 보드 위 화살표(Arrow)는 **그리드를 따라 꺾이는 선**. 탭하면 머리 방향으로 빠져나감. 앞이 다른 Arrow 에 막히면 하트 -1
- 하트 3개, 타이머·이동 제한 없음. 하트 0 → 광고 보고 +1 이어하기 / 다시하기
- 모든 Arrow 가 나가면 클리어. 100단계 클리어 시 응모 코드
- v1 에 없는 것: 힌트·Undo·Shuffle 등 부스터 전부, 코인, 별점, 인앱결제, Bomb

## 1. 용어
| 용어 | 정의 |
|---|---|
| Board | W×H 셀 그리드 (가로≠세로 허용). 원점 (0,0) 좌하단, x 오른쪽, y 위 |
| Arrow | **순서 있는 셀 경로** `cells[0]`(꼬리) → `cells[n-1]`(머리). 인접 셀은 상하좌우 연결, 직각 꺾임 자유, 자기 겹침 금지 |
| Head / Tail | 경로의 마지막 / 첫 셀 |
| Dir | 머리의 진행 방향 = 마지막 두 셀의 방향. 길이 1 이면 JSON `dir` |
| Lane | 머리 앞 칸부터 보드 가장자리까지의 직선 칸들 |
| Fire | Arrow 를 탭해 발사. 머리는 Lane 을 직진, 몸통은 자기 경로를 따라 뱀처럼 따라감 |
| Exit | 꼬리까지 보드 밖으로 나가 제거됨 |
| Block | Lane 에 다른 Arrow 의 셀이 있어 Fire 실패 |
| Marked | Block 당한 뒤 빨간색으로 표시된 상태 |

## 2. 코어 룰
1. 탭 → Lane 검사. 비어 있으면 Exit, 다른 Arrow 가 있으면 Block
2. **자기 몸통은 Lane 을 막지 않는다.** 단, 검증기가 "자기 Lane 위에 자기 몸통이 있는 레벨" 을 금지하므로 실제로는 발생하지 않음 (규칙 2-e)
3. Block → Lane 이 빨갛게 번쩍(`laneFlashDuration` 0.35) + 머리 살짝 튕김 + **하트 -1** + 그 Arrow 는 Marked
4. **Marked Arrow 를 다시 탭해 또 Block 되어도 하트는 깎이지 않는다** (배려 규칙). 어떤 Arrow 든 Exit 되면 Marked 전부 해제 (`markedResetOnExit` true). Marked Arrow 가 성공하면 그냥 Exit
5. 모든 Arrow Exit → 클리어
6. **하트 (`maxLives` 3)**: 레벨 시작 시 3 리셋, 레벨 간 공유 없음. 0 → 실패 팝업 (§7)
7. **Lane 미리보기**: Arrow 를 `longPressSeconds`(0.35) 이상 누르면 선 강조 + Lane 하이라이트. 하트 차감 없음, 놓으면 해제. 미리보기로 시작된 누름은 탭으로 세지 않음
8. 탭 히트 영역 = Arrow 의 모든 셀
9. **연속 탭 허용** (`allowInputDuringFire` true): 판정은 탭 즉시 논리 보드에서 확정, 연출은 뒤따라감. Fire 중인 Arrow 는 재탭 불가
10. Undo 없음, 시간·이동 제한 없음

## 3. Arrow 종류
| 타입 | 설명 | 도입 레벨 |
|---|---|---|
| Basic | 경로 1~N칸 | 1 (직선 3개 튜토리얼) / **2 (첫 꺾임)** |
| Frozen | `hits`(기본 2) 회 탭. 마지막 탭 전 "얼음 깨기" 탭은 Lane 무관·하트 차감 없음. 마지막 탭만 Block 판정. Block 돼도 얼음은 다시 얼지 않음 | 16 |
| Key | Basic 과 같고 `keyGroup` 을 가짐. Exit 되면 같은 그룹 Locked 해제 | 26 |
| Locked | 같은 `keyGroup` 의 Key 가 **모두** Exit 되어야 해제. 잠긴 상태에서 탭 → **하트 차감 없음** + 자물쇠 흔들림(`lockShakeDuration` 0.2, `lockShakeDistance` 0.08셀) | 26 |
| ~~Long~~ / ~~Bomb~~ | 없음. enum 에도 넣지 않음 | - |

`ArrowType` enum: Basic, Frozen, Locked, Key. 타입 표시는 머리 위 아이콘.

## 4. 레벨 구조
- 보드: 가로·세로 독립. `minBoardSize` 3, **`maxBoardWidth` 10, `maxBoardHeight` 14**
- 구간 (LEVEL_DESIGN v1.0 승인): 1~3 소형(5×6·5×5·4×5) → 4~10 6×7~7×9 → 11~20 7×9→8×10 → 21~30 8×10~8×11 → 이후 기획자 제안. 기믹 도입 레벨(16 Frozen, 26 Key/Locked)과 보드 확대 레벨은 겹치지 않게
- **`maxArrowLength` 40** (레퍼런스 Lv4 에 34칸 Arrow)
- 점유율: 3레벨부터 90~100% 가 표준 (레퍼런스와 동일)
- 레벨 파일 `Assets/_Project/Levels/level_###.json`, 스키마 LEVEL_FORMAT v0.5. 작성·저장 담당 = 기획자
- 검증기 필수 통과 규칙: 0 스키마 / 1 범위·Arrow 간 겹침 없음 / 2 경로: (a) 인접 (b) 자기 겹침 없음 (c) dir = 마지막 세그먼트 (d) 길이 ≤ maxArrowLength **(e) 자기 Lane 위에 자기 몸통 없음** / 3 Locked↔Key / 4 해결 가능(탐욕 시뮬) / 5 solution·minTaps 기록
- 향후 v1.1: 비직사각 보드 `mask`

## 5. 별점 — v1 미포함
도입 시 기준은 "잃은 하트 수" (0=⭐⭐⭐, 1=⭐⭐, 2+=⭐). minTaps 기준 아님.

## 6. 힌트 / 부스터 / 코인 — v1 전체 제외
Hint·Undo·Shuffle 없음. 코인 없음 (쓸 곳이 없음). 보상형 광고는 §7 이어하기 한 곳에만.

## 7. 실패 / 광고 (확정, 전부 AdsConfig)
- 하트 0 → 실패 팝업: **광고 보고 이어하기**(보드 유지, 하트 +`continueLives` 1, 레벨당 `maxContinues` 1회) / **다시하기**(무료, 처음부터, 하트 3)

| 위치 | 종류 | 값 | 기본 |
|---|---|---|---|
| 하트 0 → 이어하기 | 보상형 | continueLives / maxContinues | 1 / 1 |
| 레벨 클리어 후 | 전면 | interstitialEveryNLevels / adFreeLevels | 3 / 5 (1~5레벨 없음) |
| 다시하기 반복 | 전면 | interstitialAfterFails | 2 (같은 레벨 2번 실패마다) |
| 광고 제거 IAP | - | v1 미포함 | - |

- 이미 깬 레벨 재클리어도 전면 카운트 포함. HUD 다시하기·설정 다시하기는 실패로 세지 않고 광고 없음
- 광고 판단은 `AdsManager` 한 곳. 게임플레이는 이벤트만 발행. SDK: **AdMob 단독 + Unity Ads 미디에이션** (기존 게임과 동일, ID 는 신규 발급)
- **100단계 클리어 → 응모 코드** (XXXX-XXXX, 0/O/1/I 제외 32자). 기존 RewardCode/RewardCodeService 이식, 홈페이지 `https://nanabox.co.kr/reward-claim.html` 은 `RewardConfig` SO

## 8. 연출 (GameConfig)
| 값 | 기본 |
|---|---|
| fireSpeedCellsPerSec | 24 (경로 길이 비례) |
| laneFlashDuration | 0.35s |
| blockBounceDuration / Distance | 0.15s / 0.2 cell |
| iceBreakDuration | 0.15s |
| lockShakeDuration / Distance | 0.2s / 0.08 cell |
| clearPopupDelay | 0.6s |
| cellSpawnStagger / Duration | 0.03s / 0.15s |
| longPressSeconds | 0.35s |

## 9. 그래픽 (ArrowViewStyle, 레퍼런스 스크린샷 기준)
- **격자·셀 배경 없음.** 순백 #FFFFFF 배경에 선만
- 선: #141A33, `lineWidthCellRatio` 0.13~0.15, 둥근 꺾임(round join)·둥근 꼬리(round cap). 머리 삼각 화살촉 `arrowHead*CellRatio` 0.45~0.55
- Marked = 선·머리 빨강 / Block = Lane 빨간 번쩍 / 미리보기 = 선 파랑 + Lane 하이라이트
- **셀 크기 규칙 (기획자 W-016 제안 승인)**: 보드 크기와 무관하게 셀 간격 일정. `cell = min(화면폭 × cellWidthFraction(0.052), 화면폭 × maxAreaFraction(0.9) ÷ 가로칸수)`. 세로 중앙. (기존 `areaWidthFraction 0.5` 방식 폐기)
- 길이 1 Arrow 는 머리 뒤 반 칸 짧은 선
- 색·비율은 전부 인스펙터. v1 은 단색 클래식만

## 10. 화면 / UI (UI_FLOW v0.2 기준)
- 세로 고정
- **게임 화면 HUD**: 좌상단 원형 버튼 2개(뒤로가기=메인으로 확인 팝업, 다시하기=즉시 재시작) + 상단 중앙 하트 3개. **레벨 번호·설정 버튼 없음**. 게임 중 사운드 끄기 없음 (레퍼런스와 동일, 승인)
- 설정은 Main 전용: 사운드·**진동**(기본 켜짐, 하트 감소 시만 `vibrateOnLifeLost`)
- 튜토리얼: 하단 말풍선 + 손가락. `TutorialConfig` SO. Lv1 탭 안내, Lv2 꺾임, **Lv4 길게 누르기 안내(승인)**, Lv16 Frozen, Lv26 Key
- 팝업: 클리어 / 실패 / 메인으로 확인 / 설정 / 응모 코드 / 종료 확인
- 언어: v1 한국어만. 문구는 키로 분리 (`Strings` SO)
- 레벨 중간 저장 없음. 레벨 로드는 `LevelCatalog` SO
- **보드 확대·축소·이동 (대표 요구사항, 필수)** — `BoardCameraController`, 값은 GameConfig
  - 두 손가락 핀치 = 줌 (`zoomMin` 1.0 = 기본 크기, `zoomMax` 3.0). 에디터·PC 는 마우스 휠
  - 한 손가락 드래그 = 이동. 단 **드래그 판정은 `dragThresholdCells`(0.3칸) 이상 움직였을 때만** — 그 전까지는 탭/길게 누르기 후보. 드래그로 확정되면 그 터치는 탭·미리보기로 세지 않음 (하트 차감 없음)
  - 이동 범위: 보드가 화면 밖으로 완전히 나가지 않게 클램프 (`panMarginCells` 1). 줌 1.0 에서는 이동 불가 (항상 중앙)
  - **더블 탭 빈 곳 = 줌 리셋** (`doubleTapSeconds` 0.3). Arrow 위 더블 탭은 탭 2회로 처리
  - 레벨 시작·클리어·실패 시 줌 리셋
  - 줌 상태에서 Fire 연출·Lane 번쩍은 그대로. HUD 는 줌 영향 없음 (Screen Space Overlay)
  - 카메라 orthographicSize 를 바꾸는 방식 (보드 스케일 아님). `BoardLayout` 의 셀 크기 계산은 줌 1.0 기준으로 고정
- 치트(에디터·개발 빌드): 레벨 점프, 즉시 클리어, 하트 채우기

## 11. 씬 흐름
Boot(설정·저장 로드) → Main(시작·레벨 선택·설정) → Game → 클리어 팝업 → 다음 레벨 / 실패 팝업 → 이어하기·다시하기 / 뒤로가기 → Main

## 변경 이력
- v0.7.1 (09-17) §10 보드 줌·팬 추가 (대표 요구사항)
- v0.7 (09-17) 전면 재작성. maxArrowLength 40, 보드 가로·세로 분리, 셀 크기 규칙, 규칙 2-e, Lv4 튜토리얼, 게임 중 설정 없음 확정
- v0.6.x 레퍼런스 확정, 경로형 전환 / v0.5.x Key·Frozen·Locked 세부, 코인 삭제, 보드 11/21/40 / v0.4 광고 확정 / v0.3 하트·Marked / v0.2 부스터 제외
