# NANA-Arrow — LEVEL_FORMAT.md (v0.3 — 2026-09-17 GAME_RULES v0.5 기준)
> 레벨 파일: `Assets/_Project/Levels/level_###.json` (3자리 0패딩, 1부터)
> 로더: `NanaArrow.Data.LevelLoader`, 데이터 클래스: `LevelData`, `ArrowData`

## 좌표계
- 원점 (0,0) = 좌하단, x → 오른쪽, y → 위. 정수만.
- `cells`는 Arrow가 차지하는 칸 목록. Basic은 1개, Long은 2~3개 (반드시 직선·연속).
- `dir`은 Up / Down / Left / Right. Long의 dir은 cells가 늘어선 축과 같아야 한다.

## 스키마
```json
{
  "version": 1,
  "id": 1,
  "width": 5,
  "height": 5,
  "lives": 3,
  "arrows": [
    { "id": "a1", "type": "Basic",  "dir": "Left",  "cells": [[0,2]] },
    { "id": "a2", "type": "Long",   "dir": "Right", "cells": [[1,4],[2,4],[3,4]] },
    { "id": "a3", "type": "Frozen", "dir": "Up",    "cells": [[2,0]], "hits": 2 },
    { "id": "a4", "type": "Locked", "dir": "Down",  "cells": [[4,4]], "keyGroup": "red" },
    { "id": "a5", "type": "Key",    "dir": "Up",    "cells": [[0,0]], "keyGroup": "red" }
  ],
  "solution": ["a1", "a5", "a4", "a2", "a3"],
  "meta": { "author": "desktop", "difficulty": 1, "minTaps": 6, "note": "" }
}
```

예시 보드 (5×5, `.`=빈칸):
```
y4  .  a2 a2 a2 a4
y3  .  .  .  .  .
y2  a1 .  .  .  .
y1  .  .  .  .  .
y0  a5 .  a3 .  .
    x0 x1 x2 x3 x4
```
- 처음 쏠 수 있는 건 a1 뿐 → a5(Key)가 뚫림 → a5 Exit 로 a4(Locked) 해제 → a4 가 비켜야 a2 → a2 가 비켜야 a3(Frozen, 탭 2회)
- 정답은 한 가지 순서뿐이고 최소 탭 = 화살표 5 + Frozen 추가 1 = **6**
- (v0.1 예시는 a1←a2←a4←a3←a5←a2 로 전부 막혀 풀 수 없었음)

## 필드
| 필드 | 타입 | 필수 | 설명 |
|---|---|---|---|
| version | int | O | 스키마 버전. 현재 1 |
| id | int | O | 레벨 번호 (파일명과 일치) |
| width, height | int | O | 보드 크기. 3~10 |
| lives | int | X | 생략 시 GameConfig.maxLives |
| arrows[].id | string | O | 레벨 내 고유 |
| arrows[].type | enum | O | Basic / Long / Frozen / Locked / Key (정의는 GAME_RULES §3) |
| arrows[].dir | enum | O | Up / Down / Left / Right |
| arrows[].cells | int[][] | O | 차지하는 칸. 다른 Arrow와 겹치면 안 됨 |
| arrows[].hits | int | X | Frozen 전용. Exit까지 필요한 **총 탭 수(마지막 Fire 포함)**. 기본 2 = 얼음 깨기 1 + Fire 1. 얼음 깨기 탭(hits−1회)은 경로 무관·목숨 차감 없음, 마지막 탭만 Block 판정. 2 이상 |
| arrows[].keyGroup | string | X | Locked/Key 전용. 같은 그룹의 Key가 모두 Exit 되면 Locked 해제 |
| solution | string[] | X | 한 가지 정답 **Exit 순서** (검증기가 채움, Frozen의 얼음 깨기 탭은 포함 안 함). 디버그·향후 힌트용 |
| meta | object | X | 자유 |
| meta.minTaps | int | X | 검증기가 기록. 향후 별점 기준 |

## 검증 규칙 (LevelValidator, 저장 시 강제)
1. 모든 cells가 보드 안에 있고 서로 겹치지 않는다
2. Long의 cells는 같은 행 또는 열의 연속 칸이며 dir과 축이 일치
3. Locked에 대응하는 keyGroup의 Key가 최소 1개 존재
4. **해결 가능**: 탐욕 시뮬레이션으로 판정 — "지금 Fire 가능한 Arrow를 아무거나 Exit" 를 반복해 보드가 비면 통과, 더 쏠 게 없는데 남아 있으면 실패
   - Exit 는 칸을 비우고 Key Exit 는 잠금을 풀 뿐, 상황을 나쁘게 만드는 행동이 없으므로 탐욕으로 충분 (백트래킹 불필요)
   - 단순 "막힘 그래프 사이클 검사"는 Locked/Key 의존을 놓치므로 쓰지 않는다
5. 통과 시 `solution` 과 최소 탭 수 `meta.minTaps` 를 기록
   - minTaps = Arrow 수 + Σ(Frozen.hits − 1). 순서와 무관한 고정값이라 탐색이 필요 없다
6. 레벨 JSON 에 `solution`/`meta.minTaps` 가 이미 있어도 저장 시 검증기가 다시 계산해 덮어쓴다

## 버전 관리
- 스키마가 바뀌면 `version` 을 올리고 로더에 마이그레이션을 추가한다. 기존 레벨 파일은 직접 고치지 않는다.
