# NANA-Arrow — LEVEL_FORMAT.md (v0.1)
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
    { "id": "a1", "type": "Basic",  "dir": "Up",    "cells": [[2,2]] },
    { "id": "a2", "type": "Long",   "dir": "Right", "cells": [[0,4],[1,4],[2,4]] },
    { "id": "a3", "type": "Frozen", "dir": "Left",  "cells": [[4,0]], "hits": 2 },
    { "id": "a4", "type": "Locked", "dir": "Down",  "cells": [[4,4]], "keyGroup": "red" },
    { "id": "a5", "type": "Key",    "dir": "Up",    "cells": [[0,0]], "keyGroup": "red" }
  ],
  "solution": ["a1", "a5", "a4", "a2", "a3"],
  "meta": { "author": "desktop", "difficulty": 1, "note": "" }
}
```

## 필드
| 필드 | 타입 | 필수 | 설명 |
|---|---|---|---|
| version | int | O | 스키마 버전. 현재 1 |
| id | int | O | 레벨 번호 (파일명과 일치) |
| width, height | int | O | 보드 크기. 3~10 |
| lives | int | X | 생략 시 GameConfig.maxLives |
| arrows[].id | string | O | 레벨 내 고유 |
| arrows[].type | enum | O | Basic / Long / Frozen / Locked / Key |
| arrows[].dir | enum | O | Up / Down / Left / Right |
| arrows[].cells | int[][] | O | 차지하는 칸. 다른 Arrow와 겹치면 안 됨 |
| arrows[].hits | int | X | Frozen 전용. 기본 2 |
| arrows[].keyGroup | string | X | Locked/Key 전용. 같은 그룹의 Key가 모두 Exit 되면 Locked 해제 |
| solution | string[] | X | 한 가지 정답 순서 (검증기가 채움). 디버그·향후 힌트용 |
| meta | object | X | 자유 |

## 검증 규칙 (LevelValidator, 저장 시 강제)
1. 모든 cells가 보드 안에 있고 서로 겹치지 않는다
2. Long의 cells는 같은 행 또는 열의 연속 칸이며 dir과 축이 일치
3. Locked에 대응하는 keyGroup의 Key가 최소 1개 존재
4. **해결 가능**: 시뮬레이션으로 모든 Arrow를 Exit 시킬 순서가 존재 (막힘 그래프에 사이클이 없어야 함)
5. 통과 시 `solution` 과 최소 탭 수 `meta.minTaps` 를 기록

## 버전 관리
- 스키마가 바뀌면 `version` 을 올리고 로더에 마이그레이션을 추가한다. 기존 레벨 파일은 직접 고치지 않는다.
