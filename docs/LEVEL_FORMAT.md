# NANA-Arrow — LEVEL_FORMAT.md (v0.5 — 2026-09-17 경로형 Arrow, GAME_RULES v0.6 §0 기준)
> 레벨 파일: `Assets/_Project/Levels/level_###.json` (3자리 0패딩, 1부터). **레벨 파일 저장은 기획자 담당**
> 로더: `NanaArrow.Data.LevelLoader`, 데이터 클래스: `LevelData`, `ArrowData`
> v0.4 → v0.5 변경: `Long` 삭제, **모든 타입의 cells 가 꼬리→머리 순서의 경로**, 검증 규칙 2 교체, meta 권장 키 추가, `mask` 예약

## 좌표계
- 원점 (0,0) = 좌하단, x → 오른쪽, y → 위. 정수만. 보드 **가로(width)와 세로(height)는 달라도 된다** (세로가 긴 보드 권장)
- `cells` 는 **순서 있는 경로**: `cells[0]` = 꼬리, `cells[n-1]` = 머리
  - 이웃한 두 칸은 상하좌우로 붙어 있어야 한다 (대각선 불가). 직각 꺾임은 몇 번이든 가능
  - 같은 칸을 두 번 지나지 않는다
  - 길이 1 ~ `GameConfig.maxArrowLength`
- `dir` = 머리의 진행 방향 (Up / Down / Left / Right)
  - 길이 2 이상: **마지막 두 칸(cells[n-2] → cells[n-1])의 방향과 같아야 한다.** 다르면 로더/검증기 오류
  - 길이 1: `dir` 그대로 사용
- **레인** = 머리 앞 칸부터 보드 가장자리까지의 직선. Fire 는 레인에 다른 Arrow 칸이 하나도 없을 때만 성공 (GAME_RULES §0). 자기 몸이 레인에 있어도 막힌 것으로 본다 → 그런 레벨은 풀 수 없으므로 만들지 않는다

## 스키마
```json
{
  "version": 1,
  "id": 1,
  "width": 5,
  "height": 5,
  "arrows": [
    { "id": "a1", "type": "Key",    "dir": "Up",   "cells": [[0,0],[0,1],[0,2],[0,3],[0,4]], "keyGroup": "red" },
    { "id": "a2", "type": "Locked", "dir": "Left", "cells": [[4,4],[3,4],[2,4],[1,4]], "keyGroup": "red" },
    { "id": "a3", "type": "Frozen", "dir": "Up",   "cells": [[2,1],[2,2],[1,2],[1,3]], "hits": 2 },
    { "id": "a4", "type": "Basic",  "dir": "Left", "cells": [[4,0],[4,1],[4,2],[4,3],[3,3],[2,3]] }
  ],
  "solution": ["a1", "a2", "a3", "a4"],
  "meta": { "author": "desktop", "difficulty": 1, "minTaps": 5, "note": "" }
}
```

예시 보드 (숫자 = id, `*` = Frozen, ▲▼◀▶ = 머리, ○ = 꼬리, 선 기호 = 몸통):
```
y4   1▲  2◀  2─  2─  2○
y3   1│ *3▲  4◀  4─  4┐
y2   1│ *3└ *3┐   ·  4│
y1   1│   · *3○   ·  4│
y0   1○   ·   ·   ·  4○
```
- 처음 쏠 수 있는 건 a1(Key) 뿐 → a1 Exit 로 a2(Locked) 해제, 레인 (0,4) 도 비어 a2 Exit → a3(Frozen) 레인 (1,4) 비어서 탭 2회로 Exit → a4 레인 (1,3) 비어서 Exit
- 정답 a1 → a2 → a3 → a4, 최소 탭 = 4 + Frozen 추가 1 = **5**
- 기획 측 경로형 시뮬레이터로 검증 (2026-09-17). W-015 검증기 머지 후 테스트 데이터로 사용 가능

## 필드
| 필드 | 타입 | 필수 | 설명 |
|---|---|---|---|
| version | int | O | 스키마 버전. **1 유지** (출시 전이라 v0.4 레벨 파일은 전부 새로 작성함 — 아래 버전 관리 참고) |
| id | int | O | 레벨 번호 (파일명과 일치) |
| width, height | int | O | 보드 크기. 각각 `minBoardSize`~`maxBoardSize` (3~10), 서로 달라도 됨 |
| lives | int | X | 생략 시 GameConfig.maxLives |
| arrows[].id | string | O | 레벨 내 고유. 머리 위치 기준 위→아래, 왼→오른 순으로 a1, a2 … (정답 순서가 드러나지 않게) |
| arrows[].type | enum | O | Basic / Frozen / Locked / Key (**Long 삭제**) |
| arrows[].dir | enum | O | 머리 진행 방향. 길이 2 이상이면 마지막 세그먼트와 일치 |
| arrows[].cells | int[][] | O | 꼬리→머리 경로. 다른 Arrow 와 겹치지 않음 |
| arrows[].hits | int | X | Frozen 전용. 총 탭 수(마지막 Fire 포함), 기본 2. 얼음 깨기 탭은 레인·잠금과 무관, 목숨 차감 없음 |
| arrows[].keyGroup | string | X | Locked/Key 전용 |
| mask | int[][] | X | **예약 (v1.1)**. 비직사각 보드에서 사용할 수 있는 칸 목록. v1 로더는 무시 |
| solution | string[] | X | 정답 Exit 순서 (검증기가 채움, 얼음 깨기 탭 제외) |
| meta | object | X | 자유. 검증기는 `minTaps` 만 기록 |
| meta.minTaps | int | X | Arrow 수 + Σ(Frozen.hits − 1) |
| meta.freeAtStart / depth / pathLength / bends | int | X | 기획 난이도 지표 (LEVEL_DESIGN §2). 게임 코드는 읽지 않음 |

## 검증 규칙 (LevelValidator, 저장 시 강제)
0. 스키마: 보드 크기 범위, id 고유, 셀 `[x,y]` 형식, Frozen hits ≥ 2
1. 모든 cells 가 보드 안에 있고 서로 겹치지 않는다 (다른 Arrow 끼리)
2. **경로**: (a) 이웃 셀이 상하좌우로 붙어 있음 (b) 자기 겹침 없음 (c) 길이 2 이상이면 dir = 마지막 세그먼트 방향 (d) 길이 1 ~ `maxArrowLength`
3. Locked 에 대응하는 keyGroup 의 Key 가 최소 1개 존재
4. **해결 가능**: 탐욕 시뮬레이션 — 레인이 비고 잠기지 않은 Arrow 를 반복해서 Exit, 보드가 비면 통과. 레인 판정은 v0.4 와 같음 (몸통은 자기 경로로만 움직이므로 레인만 보면 충분)
5. 통과 시 `solution` 과 `meta.minTaps` 기록 (다른 meta 키는 보존)
6. 이미 값이 있어도 저장 시 다시 계산해 덮어쓴다

## 기획 권장 (검증기 강제 아님)
- 길이 1 Arrow 는 튜토리얼 외에는 쓰지 않는다 (선이 아니라 점으로 보임). 기본 3칸 이상
- 점유율 90% 이상 (레퍼런스는 초반부터 빈칸이 거의 없음)
- 머리가 보드 가장자리를 향하고 레인이 1칸 이하인 화살표만 남는 보드는 피한다 (너무 쉬움)

## 버전 관리
- 스키마가 바뀌면 `version` 을 올리고 로더에 마이그레이션을 추가한다. 기존 레벨 파일은 직접 고치지 않는다.
- **예외 (2026-09-17)**: v0.6 코어 전환은 출시 전이므로 version 1 을 유지하고 레벨 1~20 파일을 경로형으로 다시 작성했다. 이 시점 이전의 직선형 레벨 파일은 더 이상 유효하지 않다
