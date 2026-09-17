# NANA-Arrow — LEVEL_DESIGN.md (v0.3 — 2026-09-17 레벨 11~20 추가, W-008)
> 작성: 기획자/PM (클로드 데스크탑). 기준: GAME_RULES v0.5.2, LEVEL_FORMAT v0.4
> 모든 레벨은 **프로젝트의 `LevelValidator.Validate` 로 검증 통과** (오류 0). `solution`·`minTaps` 는 검증기 결과와 동일하게 적었으므로 `Record` 해도 바뀌지 않음. 파일 저장은 레벨 에디터 툴(프로그래머)에서 `Assets/_Project/Levels/`

## 1. 난이도 지표
레벨 난이도는 아래 4개 숫자로 관리한다. 검증기(LevelValidator)가 함께 계산해 주면 좋음 (이슈 #4 선택 항목)

| 지표 | 뜻 | 어려워지는 방향 |
|---|---|---|
| n | 화살표 수 | ↑ (탭 횟수·읽을 양 증가) |
| free0 | 시작 시점에 바로 쏠 수 있는 화살표 수 | **↓** (보드가 클수록 정답 후보 찾기 어려움) |
| depth | 의존 층 수. 쏠 수 있는 것을 한꺼번에 쏘는 걸 1층으로 셌을 때 몇 층에 끝나는지 | ↑ (앞을 내다봐야 하는 길이) |
| fill | 보드 점유율 (차지한 칸 / 전체 칸) | ↑ (빈칸이 적어 눈으로 경로 확인이 어려움) |

- 막힌 화살표 수 = n − free0. 이것이 곧 **목숨을 잃을 수 있는 함정 수**
- 기믹(Frozen·Locked)은 지표와 별개로 한 단계 난이도를 더한다고 본다

## 2. 난이도 곡선 원칙
1. **톱니형**: 5레벨 단위로 올라가고, 각 묶음의 마지막(5의 배수)이 그 구간의 고비. 바로 다음 레벨은 한 단계 쉬운 휴식 레벨
2. **새 요소는 하나씩**: 보드 크기 변경과 새 기믹 도입이 같은 레벨에 겹치지 않게 한다
3. **새 기믹 첫 레벨은 휴식 레벨**: 새 기믹만 이해하면 풀리는 쉬운 구조 (Lv6 Long 이 예시)
4. **1~5는 광고 없음 구간** (AdsConfig.adFreeLevels = 5) → 가장 친절하게. 6레벨부터 3레벨마다 전면 광고가 붙으므로 6~10에서 이탈하지 않도록 깊이를 천천히 올린다
5. 목숨 3개 기준으로, 초반은 함정(막힌 화살표)을 눈에 띄게 두고 후반은 함정이 정답처럼 보이게 한다

## 3. 구간 계획 (1~100)
| 구간 | 보드 | 새 요소 | n 목표 | depth 목표 | 비고 |
|---|---|---|---|---|---|
| 1~5 | 5×5 | Basic | 3→7 | 2→4 | 광고 없음, 튜토리얼 |
| 6~10 | 5×5 | **Long** (6) | 5→10 | 3→7 | 10 = 5×5 고비 |
| 11~15 | **6×6** | - | 8→12 | 4→6 | 11 은 휴식 (보드만 커짐) |
| 16~20 | 6×6 | **Frozen** (16) | 9→13 | 4→7 | 16 은 Frozen 1개만 |
| 21~25 | **7×7** | - | 10→15 | 5→7 | 21 은 휴식 |
| 26~30 | 7×7 | **Key + Locked** (26) | 11→16 | 5→8 | 26 은 Key 1 + Locked 1 만 |
| 31~39 | 7×7 | 기믹 조합 | 14→20 | 6→9 | 35 고비 |
| 40~100 | **8×8** | 조합 심화 | 18→28 | 7→12 | 40 은 휴식. 이후 5단위 톱니 반복, 100 = 응모 코드 레벨 |

- 보드 크기 전환 11 / 21 / 40 — **디렉터 승인, GAME_RULES v0.5.2 §4 반영**
- 11~100 은 역방향 생성기(클로드 코드) 결과에서 위 목표에 맞는 후보를 골라 채우고, 기믹 도입 레벨(16, 26)과 5의 배수 고비 레벨은 수동으로 설계한다

## 4. 레벨 1~10 요약
| Lv | 보드 | 요소 | n | free0 | depth | fill | minTaps | 설계 의도 |
|---|---|---|---|---|---|---|---|---|
| 1 | 5×5 | Basic | 3 | 2 | 2 | 3/25 | 3 | 튜토리얼: 탭하면 날아간다. a1 은 막혀 있어 Block·목숨 차감을 처음 경험 |
| 2 | 5×5 | Basic | 4 | 1 | 4 | 4/25 | 4 | 순서 개념: 한 줄 사슬, 처음 쏠 수 있는 건 1개 |
| 3 | 5×5 | Basic | 5 | 2 | 4 | 5/25 | 5 | 사슬 + 독립 화살표 1개 (선택지 2) |
| 4 | 5×5 | Basic | 6 | 3 | 4 | 6/25 | 6 | 선택지 3, 사슬 깊이 4 |
| 5 | 5×5 | Basic | 7 | 3 | 3 | 7/25 | 7 | Basic 구간 마무리: 두 갈래 사슬이 동시에 진행 |
| 6 | 5×5 | Basic+Long | 5 | 2 | 3 | 7/25 | 5 | Long 도입 (휴식 레벨): 세로·가로 Long 각 1개, 쉬운 구조 |
| 7 | 5×5 | Basic+Long | 6 | 2 | 4 | 9/25 | 6 | Long 3개, 긴 화살표가 길을 막는 구조 |
| 8 | 5×5 | Basic+Long | 8 | 3 | 5 | 11/25 | 8 | 8개, 3칸 Long 첫 등장 |
| 9 | 5×5 | Basic+Long | 10 | 3 | 6 | 14/25 | 10 | 10개, 선택지 3 / 깊이 6 |
| 10 | 5×5 | Basic+Long | 9 | 2 | 7 | 13/25 | 9 | 5×5 구간 보스: 9개, 선택지 2 / 깊이 7 |

- 레벨 9와 10은 생성 후보에서 골랐고, 층이 더 깊은 쪽(depth 7)을 10에 배치
- 모든 레벨의 minTaps = n (Frozen 없음)

## 5. 튜토리얼 연출 메모 (UI_FLOW 에서 상세화)
- **Lv1**: 손가락 표시가 a2 를 가리킴 → 탭 → 날아감. 다음 손가락은 a1 에 두지 **않는다**. 플레이어가 a1 을 먼저 눌러 Block·하트 감소·빨간색을 한 번 경험해도 목숨이 남도록 설계됨 (함정 1개)
- **Lv2**: 첫 화면에서 쏠 수 있는 게 a3 하나 → 문구 예) 앞이 비어 있는 화살표부터
- **Lv6**: Long 첫 등장 → 문구 예) 긴 화살표도 앞만 비어 있으면 통째로 날아가요

## 6. 레벨 JSON (1~10)
> 각 블록을 그대로 `Assets/_Project/Levels/level_###.json` 으로 저장. 그림의 숫자는 화살표 id(a 생략), 화살표 기호는 방향. y는 위가 큼

### Level 1
```
y4  ·   ·   3↑  ·   ·  
y3  ·   ·   ·   ·   ·  
y2  ·   1→  ·   2→  ·  
y1  ·   ·   ·   ·   ·  
y0  ·   ·   ·   ·   ·  
    x0  x1  x2  x3  x4
```
```json
{
  "version": 1,
  "id": 1,
  "width": 5,
  "height": 5,
  "arrows": [
    { "id": "a1", "type": "Basic", "dir": "Right", "cells": [[1,2]] },
    { "id": "a2", "type": "Basic", "dir": "Right", "cells": [[3,2]] },
    { "id": "a3", "type": "Basic", "dir": "Up", "cells": [[2,4]] }
  ],
  "solution": ["a2", "a3", "a1"],
  "meta": { "author": "desktop", "difficulty": 1, "minTaps": 3, "note": "튜토리얼: 탭하면 날아간다. a1 은 막혀 있어 Block·목숨 차감을 처음 경험" }
}
```

### Level 2
```
y4  ·   ·   ·   ·   ·  
y3  3↓  ·   2←  ·   ·  
y2  ·   ·   1↑  ·   4← 
y1  ·   ·   ·   ·   ·  
y0  ·   ·   ·   ·   ·  
    x0  x1  x2  x3  x4
```
```json
{
  "version": 1,
  "id": 2,
  "width": 5,
  "height": 5,
  "arrows": [
    { "id": "a1", "type": "Basic", "dir": "Up", "cells": [[2,2]] },
    { "id": "a2", "type": "Basic", "dir": "Left", "cells": [[2,3]] },
    { "id": "a3", "type": "Basic", "dir": "Down", "cells": [[0,3]] },
    { "id": "a4", "type": "Basic", "dir": "Left", "cells": [[4,2]] }
  ],
  "solution": ["a3", "a2", "a1", "a4"],
  "meta": { "author": "desktop", "difficulty": 1, "minTaps": 4, "note": "순서 개념: 한 줄 사슬, 처음 쏠 수 있는 건 1개" }
}
```

### Level 3
```
y4  ·   ·   ·   ·   ·  
y3  ·   2→  ·   3↓  ·  
y2  ·   ·   ·   ·   ·  
y1  ·   1↑  ·   4→  ·  
y0  ·   ·   ·   ·   5↑ 
    x0  x1  x2  x3  x4
```
```json
{
  "version": 1,
  "id": 3,
  "width": 5,
  "height": 5,
  "arrows": [
    { "id": "a1", "type": "Basic", "dir": "Up", "cells": [[1,1]] },
    { "id": "a2", "type": "Basic", "dir": "Right", "cells": [[1,3]] },
    { "id": "a3", "type": "Basic", "dir": "Down", "cells": [[3,3]] },
    { "id": "a4", "type": "Basic", "dir": "Right", "cells": [[3,1]] },
    { "id": "a5", "type": "Basic", "dir": "Up", "cells": [[4,0]] }
  ],
  "solution": ["a4", "a5", "a3", "a2", "a1"],
  "meta": { "author": "desktop", "difficulty": 2, "minTaps": 5, "note": "사슬 + 독립 화살표 1개 (선택지 2)" }
}
```

### Level 4
```
y4  4↓  ·   ·   ·   ·  
y3  ·   ·   ·   5→  ·  
y2  ·   ·   3←  ·   ·  
y1  ·   ·   ·   ·   6↑ 
y0  1→  ·   2↑  ·   ·  
    x0  x1  x2  x3  x4
```
```json
{
  "version": 1,
  "id": 4,
  "width": 5,
  "height": 5,
  "arrows": [
    { "id": "a1", "type": "Basic", "dir": "Right", "cells": [[0,0]] },
    { "id": "a2", "type": "Basic", "dir": "Up", "cells": [[2,0]] },
    { "id": "a3", "type": "Basic", "dir": "Left", "cells": [[2,2]] },
    { "id": "a4", "type": "Basic", "dir": "Down", "cells": [[0,4]] },
    { "id": "a5", "type": "Basic", "dir": "Right", "cells": [[3,3]] },
    { "id": "a6", "type": "Basic", "dir": "Up", "cells": [[4,1]] }
  ],
  "solution": ["a3", "a5", "a6", "a2", "a1", "a4"],
  "meta": { "author": "desktop", "difficulty": 2, "minTaps": 6, "note": "선택지 3, 사슬 깊이 4" }
}
```

### Level 5
```
y4  ·   7→  ·   3↓  ·  
y3  ·   ·   6←  ·   ·  
y2  2→  ·   ·   ·   5↓ 
y1  ·   ·   ·   4←  ·  
y0  1↑  ·   ·   ·   ·  
    x0  x1  x2  x3  x4
```
```json
{
  "version": 1,
  "id": 5,
  "width": 5,
  "height": 5,
  "arrows": [
    { "id": "a1", "type": "Basic", "dir": "Up", "cells": [[0,0]] },
    { "id": "a2", "type": "Basic", "dir": "Right", "cells": [[0,2]] },
    { "id": "a3", "type": "Basic", "dir": "Down", "cells": [[3,4]] },
    { "id": "a4", "type": "Basic", "dir": "Left", "cells": [[3,1]] },
    { "id": "a5", "type": "Basic", "dir": "Down", "cells": [[4,2]] },
    { "id": "a6", "type": "Basic", "dir": "Left", "cells": [[2,3]] },
    { "id": "a7", "type": "Basic", "dir": "Right", "cells": [[1,4]] }
  ],
  "solution": ["a4", "a5", "a6", "a2", "a3", "a7", "a1"],
  "meta": { "author": "desktop", "difficulty": 2, "minTaps": 7, "note": "Basic 구간 마무리: 두 갈래 사슬이 동시에 진행" }
}
```

### Level 6
```
y4  3→  ·   4↑  ·   ·  
y3  ·   ·   4↑  ·   ·  
y2  1→  1→  ·   2↓  ·  
y1  ·   ·   ·   ·   ·  
y0  ·   ·   ·   5←  ·  
    x0  x1  x2  x3  x4
```
```json
{
  "version": 1,
  "id": 6,
  "width": 5,
  "height": 5,
  "arrows": [
    { "id": "a1", "type": "Long", "dir": "Right", "cells": [[0,2],[1,2]] },
    { "id": "a2", "type": "Basic", "dir": "Down", "cells": [[3,2]] },
    { "id": "a3", "type": "Basic", "dir": "Right", "cells": [[0,4]] },
    { "id": "a4", "type": "Long", "dir": "Up", "cells": [[2,3],[2,4]] },
    { "id": "a5", "type": "Basic", "dir": "Left", "cells": [[3,0]] }
  ],
  "solution": ["a4", "a5", "a2", "a3", "a1"],
  "meta": { "author": "desktop", "difficulty": 3, "minTaps": 5, "note": "Long 도입 (휴식 레벨): 세로·가로 Long 각 1개, 쉬운 구조" }
}
```

### Level 7
```
y4  6↓  ·   5←  5←  ·  
y3  ·   2→  2→  ·   3↓ 
y2  ·   ·   ·   ·   ·  
y1  ·   1↑  ·   ·   4→ 
y0  ·   1↑  ·   ·   ·  
    x0  x1  x2  x3  x4
```
```json
{
  "version": 1,
  "id": 7,
  "width": 5,
  "height": 5,
  "arrows": [
    { "id": "a1", "type": "Long", "dir": "Up", "cells": [[1,0],[1,1]] },
    { "id": "a2", "type": "Long", "dir": "Right", "cells": [[1,3],[2,3]] },
    { "id": "a3", "type": "Basic", "dir": "Down", "cells": [[4,3]] },
    { "id": "a4", "type": "Basic", "dir": "Right", "cells": [[4,1]] },
    { "id": "a5", "type": "Long", "dir": "Left", "cells": [[2,4],[3,4]] },
    { "id": "a6", "type": "Basic", "dir": "Down", "cells": [[0,4]] }
  ],
  "solution": ["a4", "a6", "a3", "a5", "a2", "a1"],
  "meta": { "author": "desktop", "difficulty": 3, "minTaps": 6, "note": "Long 3개, 긴 화살표가 길을 막는 구조" }
}
```

### Level 8
```
y4  ·   1→  2↑  ·   ·  
y3  ·   3↑  4←  4←  4← 
y2  ·   ·   5→  ·   ·  
y1  7↑  ·   ·   ·   6↑ 
y0  7↑  8↑  ·   ·   ·  
    x0  x1  x2  x3  x4
```
```json
{
  "version": 1,
  "id": 8,
  "width": 5,
  "height": 5,
  "arrows": [
    { "id": "a1", "type": "Basic", "dir": "Right", "cells": [[1,4]] },
    { "id": "a2", "type": "Basic", "dir": "Up", "cells": [[2,4]] },
    { "id": "a3", "type": "Basic", "dir": "Up", "cells": [[1,3]] },
    { "id": "a4", "type": "Long", "dir": "Left", "cells": [[2,3],[3,3],[4,3]] },
    { "id": "a5", "type": "Basic", "dir": "Right", "cells": [[2,2]] },
    { "id": "a6", "type": "Basic", "dir": "Up", "cells": [[4,1]] },
    { "id": "a7", "type": "Long", "dir": "Up", "cells": [[0,0],[0,1]] },
    { "id": "a8", "type": "Basic", "dir": "Up", "cells": [[1,0]] }
  ],
  "solution": ["a2", "a5", "a7", "a1", "a3", "a4", "a6", "a8"],
  "meta": { "author": "desktop", "difficulty": 3, "minTaps": 8, "note": "8개, 3칸 Long 첫 등장" }
}
```

### Level 9
```
y4  ·   1→  3↓  2↑  ·  
y3  ·   ·   3↓  ·   4↓ 
y2  ·   ·   ·  10↑  ·  
y1  8↑  5←  6← 10↑  7→ 
y0  8↑  9↑  ·  10↑  ·  
    x0  x1  x2  x3  x4
```
```json
{
  "version": 1,
  "id": 9,
  "width": 5,
  "height": 5,
  "arrows": [
    { "id": "a1", "type": "Basic", "dir": "Right", "cells": [[1,4]] },
    { "id": "a2", "type": "Basic", "dir": "Up", "cells": [[3,4]] },
    { "id": "a3", "type": "Long", "dir": "Down", "cells": [[2,3],[2,4]] },
    { "id": "a4", "type": "Basic", "dir": "Down", "cells": [[4,3]] },
    { "id": "a5", "type": "Basic", "dir": "Left", "cells": [[1,1]] },
    { "id": "a6", "type": "Basic", "dir": "Left", "cells": [[2,1]] },
    { "id": "a7", "type": "Basic", "dir": "Right", "cells": [[4,1]] },
    { "id": "a8", "type": "Long", "dir": "Up", "cells": [[0,0],[0,1]] },
    { "id": "a9", "type": "Basic", "dir": "Up", "cells": [[1,0]] },
    { "id": "a10", "type": "Long", "dir": "Up", "cells": [[3,0],[3,1],[3,2]] }
  ],
  "solution": ["a2", "a7", "a8", "a10", "a4", "a5", "a6", "a3", "a1", "a9"],
  "meta": { "author": "desktop", "difficulty": 4, "minTaps": 10, "note": "10개, 선택지 3 / 깊이 6" }
}
```

### Level 10
```
y4  1→  1→  1→  2→  4↓ 
y3  ·   3←  ·   ·   4↓ 
y2  ·   ·   5→  ·   6↓ 
y1  9↑  ·   ·   7←  8→ 
y0  9↑  ·   ·   ·   ·  
    x0  x1  x2  x3  x4
```
```json
{
  "version": 1,
  "id": 10,
  "width": 5,
  "height": 5,
  "arrows": [
    { "id": "a1", "type": "Long", "dir": "Right", "cells": [[0,4],[1,4],[2,4]] },
    { "id": "a2", "type": "Basic", "dir": "Right", "cells": [[3,4]] },
    { "id": "a3", "type": "Basic", "dir": "Left", "cells": [[1,3]] },
    { "id": "a4", "type": "Long", "dir": "Down", "cells": [[4,3],[4,4]] },
    { "id": "a5", "type": "Basic", "dir": "Right", "cells": [[2,2]] },
    { "id": "a6", "type": "Basic", "dir": "Down", "cells": [[4,2]] },
    { "id": "a7", "type": "Basic", "dir": "Left", "cells": [[3,1]] },
    { "id": "a8", "type": "Basic", "dir": "Right", "cells": [[4,1]] },
    { "id": "a9", "type": "Long", "dir": "Up", "cells": [[0,0],[0,1]] }
  ],
  "solution": ["a3", "a8", "a6", "a4", "a5", "a2", "a1", "a9", "a7"],
  "meta": { "author": "desktop", "difficulty": 4, "minTaps": 9, "note": "5×5 구간 보스: 9개, 선택지 2 / 깊이 7" }
}
```

## 7. 레벨 11~20 (6×6, Frozen 도입) — W-008
> 모두 `LevelValidator.Validate` 오류 0, `Record` 해도 solution·minTaps 불변 확인 (2026-09-17). 그림의 `*` = Frozen(hits 2)
> 생성 방식: 역순 배치(나중에 나갈 화살표부터 놓되, 새 화살표를 기존 화살표의 경로 위에 두어 의존 사슬을 만듦) → 구간 목표에 맞는 후보 선택 → Frozen 조건(16: 처음부터 쏠 수 있음 / 17~20: 막힌 Frozen 1개 이상, Frozen 이 다른 화살표를 막음) 필터

| Lv | 보드 | 요소 | n | free0 | depth | fill | minTaps | 설계 의도 |
|---|---|---|---|---|---|---|---|---|
| 11 | 6×6 | Long 2 | 8 | 3 | 4 | 11/36 | 8 | 휴식: 6×6 첫 레벨. 보드만 커지고 구조는 쉬움 |
| 12 | 6×6 | Long 2 | 9 | 2 | 5 | 11/36 | 9 | 깊이 5, 선택지 2 |
| 13 | 6×6 | Long 3 | 10 | 2 | 5 | 13/36 | 10 | Long 3개 |
| 14 | 6×6 | Long 3 | 11 | 3 | 6 | 17/36 | 11 | 11개, 깊이 6 |
| 15 | 6×6 | Long 4 | 12 | 2 | 7 | 16/36 | 12 | 6×6 고비: 12개, 선택지 2, 깊이 7 |
| 16 | 6×6 | Long 2 · Frozen 1 | 9 | 3 | 4 | 11/36 | 10 | Frozen 도입(휴식): Frozen 1개가 처음부터 쏠 수 있고 다른 화살표의 길을 막음 |
| 17 | 6×6 | Long 2 · Frozen 2 | 10 | 3 | 5 | 13/36 | 12 | Frozen 2개. 그중 막힌 Frozen 은 얼음은 깨져도 발사하면 Block — 순서 학습 |
| 18 | 6×6 | Long 3 · Frozen 2 | 11 | 3 | 6 | 14/36 | 13 | Frozen 2개, 깊이 6 |
| 19 | 6×6 | Long 3 · Frozen 3 | 12 | 2 | 6 | 16/36 | 15 | Frozen 3개, 선택지 2 |
| 20 | 6×6 | Long 4 · Frozen 3 | 13 | 2 | 6 | 18/36 | 16 | Frozen 구간 마무리: 13개, 선택지 2, 깊이 6, 점유율 최고 |

- minTaps = n + Frozen 수 (hits 2 → 얼음 깨기 1회씩 추가)
- **Lv20 은 depth 6** (구간 목표 7 미달). Frozen 조건과 free0 2~3 을 함께 만족하는 depth 7 후보가 드물었음. 대신 화살표 수·점유율이 구간 최고라 Lv19 보다 무거움. 플레이 테스트에서 약하면 교체
- **Lv16 튜토리얼 손가락**: `a9` (UI_FLOW §7 의 Frozen)
- 톱니: 10(깊이 7) → **11 휴식(4)** → 15 고비(7) → **16 휴식(4, Frozen 도입)** → 20 마무리(6)

### Level 11
```
y5   1↓   ·    ·    ·    2→   ·  
y4   ·    ·    3↑   ·    ·    ·  
y3   4↓   ·    5↑   ·    8↑   ·  
y2   6↓   ·    ·    ·    8↑   ·  
y1   7←   7←   ·    ·    8↑   ·  
y0   ·    ·    ·    ·    ·    ·  
    x0   x1   x2   x3   x4   x5
```
```json
{
  "version": 1,
  "id": 11,
  "width": 6,
  "height": 6,
  "arrows": [
    { "id": "a1", "type": "Basic", "dir": "Down", "cells": [[0,5]] },
    { "id": "a2", "type": "Basic", "dir": "Right", "cells": [[4,5]] },
    { "id": "a3", "type": "Basic", "dir": "Up", "cells": [[2,4]] },
    { "id": "a4", "type": "Basic", "dir": "Down", "cells": [[0,3]] },
    { "id": "a5", "type": "Basic", "dir": "Up", "cells": [[2,3]] },
    { "id": "a6", "type": "Basic", "dir": "Down", "cells": [[0,2]] },
    { "id": "a7", "type": "Long", "dir": "Left", "cells": [[0,1],[1,1]] },
    { "id": "a8", "type": "Long", "dir": "Up", "cells": [[4,1],[4,2],[4,3]] }
  ],
  "solution": ["a2", "a3", "a5", "a7", "a8", "a6", "a4", "a1"],
  "meta": { "author": "desktop", "difficulty": 3, "minTaps": 8, "note": "휴식: 6×6 첫 레벨. 보드만 커지고 구조는 쉬움" }
}
```

### Level 12
```
y5   ·    ·    ·    1→   1→   2↑ 
y4   ·    ·    ·    3↑   4←   ·  
y3   5→   5→   ·    6↓   7→   8↑ 
y2   ·    ·    ·    ·    9↑   ·  
y1   ·    ·    ·    ·    ·    ·  
y0   ·    ·    ·    ·    ·    ·  
    x0   x1   x2   x3   x4   x5
```
```json
{
  "version": 1,
  "id": 12,
  "width": 6,
  "height": 6,
  "arrows": [
    { "id": "a1", "type": "Long", "dir": "Right", "cells": [[3,5],[4,5]] },
    { "id": "a2", "type": "Basic", "dir": "Up", "cells": [[5,5]] },
    { "id": "a3", "type": "Basic", "dir": "Up", "cells": [[3,4]] },
    { "id": "a4", "type": "Basic", "dir": "Left", "cells": [[4,4]] },
    { "id": "a5", "type": "Long", "dir": "Right", "cells": [[0,3],[1,3]] },
    { "id": "a6", "type": "Basic", "dir": "Down", "cells": [[3,3]] },
    { "id": "a7", "type": "Basic", "dir": "Right", "cells": [[4,3]] },
    { "id": "a8", "type": "Basic", "dir": "Up", "cells": [[5,3]] },
    { "id": "a9", "type": "Basic", "dir": "Up", "cells": [[4,2]] }
  ],
  "solution": ["a2", "a6", "a8", "a1", "a3", "a4", "a7", "a9", "a5"],
  "meta": { "author": "desktop", "difficulty": 4, "minTaps": 9, "note": "깊이 5, 선택지 2" }
}
```

### Level 13
```
y5   ·    ·    ·    ·    ·    ·  
y4   1↑   2←   3↓   ·    ·    4← 
y3   7↑   ·    5→   5→   6→   ·  
y2   7↑   ·    8←   ·    ·    ·  
y1   9↑   ·    ·    ·    ·    ·  
y0   9↑   ·    ·   10←   ·    ·  
    x0   x1   x2   x3   x4   x5
```
```json
{
  "version": 1,
  "id": 13,
  "width": 6,
  "height": 6,
  "arrows": [
    { "id": "a1", "type": "Basic", "dir": "Up", "cells": [[0,4]] },
    { "id": "a2", "type": "Basic", "dir": "Left", "cells": [[1,4]] },
    { "id": "a3", "type": "Basic", "dir": "Down", "cells": [[2,4]] },
    { "id": "a4", "type": "Basic", "dir": "Left", "cells": [[5,4]] },
    { "id": "a5", "type": "Long", "dir": "Right", "cells": [[2,3],[3,3]] },
    { "id": "a6", "type": "Basic", "dir": "Right", "cells": [[4,3]] },
    { "id": "a7", "type": "Long", "dir": "Up", "cells": [[0,2],[0,3]] },
    { "id": "a8", "type": "Basic", "dir": "Left", "cells": [[2,2]] },
    { "id": "a9", "type": "Long", "dir": "Up", "cells": [[0,0],[0,1]] },
    { "id": "a10", "type": "Basic", "dir": "Left", "cells": [[3,0]] }
  ],
  "solution": ["a1", "a2", "a6", "a7", "a8", "a9", "a10", "a5", "a3", "a4"],
  "meta": { "author": "desktop", "difficulty": 4, "minTaps": 10, "note": "Long 3개" }
}
```

### Level 14
```
y5   1→   1→   1→   2↑   3→   5↓ 
y4   ·    ·    ·    ·    ·    5↓ 
y3   ·    ·    ·    4↑   ·    5↓ 
y2   ·    ·    ·    6→   6→   6→ 
y1   ·    7→   ·    8→   ·    9→ 
y0  10→   ·   11↑   ·    ·    ·  
    x0   x1   x2   x3   x4   x5
```
```json
{
  "version": 1,
  "id": 14,
  "width": 6,
  "height": 6,
  "arrows": [
    { "id": "a1", "type": "Long", "dir": "Right", "cells": [[0,5],[1,5],[2,5]] },
    { "id": "a2", "type": "Basic", "dir": "Up", "cells": [[3,5]] },
    { "id": "a3", "type": "Basic", "dir": "Right", "cells": [[4,5]] },
    { "id": "a4", "type": "Basic", "dir": "Up", "cells": [[3,3]] },
    { "id": "a5", "type": "Long", "dir": "Down", "cells": [[5,3],[5,4],[5,5]] },
    { "id": "a6", "type": "Long", "dir": "Right", "cells": [[3,2],[4,2],[5,2]] },
    { "id": "a7", "type": "Basic", "dir": "Right", "cells": [[1,1]] },
    { "id": "a8", "type": "Basic", "dir": "Right", "cells": [[3,1]] },
    { "id": "a9", "type": "Basic", "dir": "Right", "cells": [[5,1]] },
    { "id": "a10", "type": "Basic", "dir": "Right", "cells": [[0,0]] },
    { "id": "a11", "type": "Basic", "dir": "Up", "cells": [[2,0]] }
  ],
  "solution": ["a2", "a4", "a6", "a9", "a5", "a8", "a3", "a7", "a1", "a11", "a10"],
  "meta": { "author": "desktop", "difficulty": 4, "minTaps": 11, "note": "11개, 깊이 6" }
}
```

### Level 15
```
y5   ·    ·    ·    1→   1→   4↓ 
y4   ·    2↓   ·    ·    3←   4↓ 
y3   ·    ·    5←   6↓   8↑   ·  
y2   ·    7↓   ·    ·    8↑   ·  
y1   9→   9→  10↑  11→   ·   12→ 
y0   ·    ·    ·    ·    ·    ·  
    x0   x1   x2   x3   x4   x5
```
```json
{
  "version": 1,
  "id": 15,
  "width": 6,
  "height": 6,
  "arrows": [
    { "id": "a1", "type": "Long", "dir": "Right", "cells": [[3,5],[4,5]] },
    { "id": "a2", "type": "Basic", "dir": "Down", "cells": [[1,4]] },
    { "id": "a3", "type": "Basic", "dir": "Left", "cells": [[4,4]] },
    { "id": "a4", "type": "Long", "dir": "Down", "cells": [[5,4],[5,5]] },
    { "id": "a5", "type": "Basic", "dir": "Left", "cells": [[2,3]] },
    { "id": "a6", "type": "Basic", "dir": "Down", "cells": [[3,3]] },
    { "id": "a7", "type": "Basic", "dir": "Down", "cells": [[1,2]] },
    { "id": "a8", "type": "Long", "dir": "Up", "cells": [[4,2],[4,3]] },
    { "id": "a9", "type": "Long", "dir": "Right", "cells": [[0,1],[1,1]] },
    { "id": "a10", "type": "Basic", "dir": "Up", "cells": [[2,1]] },
    { "id": "a11", "type": "Basic", "dir": "Right", "cells": [[3,1]] },
    { "id": "a12", "type": "Basic", "dir": "Right", "cells": [[5,1]] }
  ],
  "solution": ["a5", "a10", "a12", "a4", "a11", "a1", "a6", "a9", "a7", "a2", "a3", "a8"],
  "meta": { "author": "desktop", "difficulty": 5, "minTaps": 12, "note": "6×6 고비: 12개, 선택지 2, 깊이 7" }
}
```

### Level 16
```
y5   1→   ·    ·    ·    ·    ·  
y4   ·    ·    ·    ·    ·    ·  
y3   ·    ·    2→   ·    4↓   3↓ 
y2   ·    ·    ·    ·    4↓   5↓ 
y1   6↓   7←   ·    8→   8→ * 9→ 
y0   ·    ·    ·    ·    ·    ·  
    x0   x1   x2   x3   x4   x5
```
```json
{
  "version": 1,
  "id": 16,
  "width": 6,
  "height": 6,
  "arrows": [
    { "id": "a1", "type": "Basic", "dir": "Right", "cells": [[0,5]] },
    { "id": "a2", "type": "Basic", "dir": "Right", "cells": [[2,3]] },
    { "id": "a3", "type": "Basic", "dir": "Down", "cells": [[5,3]] },
    { "id": "a4", "type": "Long", "dir": "Down", "cells": [[4,2],[4,3]] },
    { "id": "a5", "type": "Basic", "dir": "Down", "cells": [[5,2]] },
    { "id": "a6", "type": "Basic", "dir": "Down", "cells": [[0,1]] },
    { "id": "a7", "type": "Basic", "dir": "Left", "cells": [[1,1]] },
    { "id": "a8", "type": "Long", "dir": "Right", "cells": [[3,1],[4,1]] },
    { "id": "a9", "type": "Frozen", "dir": "Right", "cells": [[5,1]], "hits": 2 }
  ],
  "solution": ["a1", "a6", "a7", "a9", "a5", "a8", "a3", "a4", "a2"],
  "meta": { "author": "desktop", "difficulty": 3, "minTaps": 10, "note": "Frozen 도입(휴식): Frozen 1개가 처음부터 쏠 수 있고 다른 화살표의 길을 막음" }
}
```

### Level 17
```
y5   ·    ·    ·    ·    ·    ·  
y4   ·    ·    ·    1↓   ·    ·  
y3   ·    6↓   2↓   3↓   ·    ·  
y2   ·    6↓   4↓   ·    ·    ·  
y1 * 5←   6↓   ·    7↓   8←   8← 
y0   9↑ *10←   ·    ·    ·    ·  
    x0   x1   x2   x3   x4   x5
```
```json
{
  "version": 1,
  "id": 17,
  "width": 6,
  "height": 6,
  "arrows": [
    { "id": "a1", "type": "Basic", "dir": "Down", "cells": [[3,4]] },
    { "id": "a2", "type": "Basic", "dir": "Down", "cells": [[2,3]] },
    { "id": "a3", "type": "Basic", "dir": "Down", "cells": [[3,3]] },
    { "id": "a4", "type": "Basic", "dir": "Down", "cells": [[2,2]] },
    { "id": "a5", "type": "Frozen", "dir": "Left", "cells": [[0,1]], "hits": 2 },
    { "id": "a6", "type": "Long", "dir": "Down", "cells": [[1,1],[1,2],[1,3]] },
    { "id": "a7", "type": "Basic", "dir": "Down", "cells": [[3,1]] },
    { "id": "a8", "type": "Long", "dir": "Left", "cells": [[4,1],[5,1]] },
    { "id": "a9", "type": "Basic", "dir": "Up", "cells": [[0,0]] },
    { "id": "a10", "type": "Frozen", "dir": "Left", "cells": [[1,0]], "hits": 2 }
  ],
  "solution": ["a4", "a5", "a7", "a9", "a10", "a2", "a3", "a6", "a8", "a1"],
  "meta": { "author": "desktop", "difficulty": 4, "minTaps": 12, "note": "Frozen 2개. 그중 막힌 Frozen 은 얼음은 깨져도 발사하면 Block — 순서 학습" }
}
```

### Level 18
```
y5   1→   1→   4↓   ·    ·    2↑ 
y4   ·    3↑   4↓   ·    5←   ·  
y3 * 6→   ·    ·    ·    8↓   7↑ 
y2   ·    ·    ·    ·    8↓   9← 
y1   ·    ·    ·    ·    ·    ·  
y0   ·    ·  *10→  11↑   ·    ·  
    x0   x1   x2   x3   x4   x5
```
```json
{
  "version": 1,
  "id": 18,
  "width": 6,
  "height": 6,
  "arrows": [
    { "id": "a1", "type": "Long", "dir": "Right", "cells": [[0,5],[1,5]] },
    { "id": "a2", "type": "Basic", "dir": "Up", "cells": [[5,5]] },
    { "id": "a3", "type": "Basic", "dir": "Up", "cells": [[1,4]] },
    { "id": "a4", "type": "Long", "dir": "Down", "cells": [[2,4],[2,5]] },
    { "id": "a5", "type": "Basic", "dir": "Left", "cells": [[4,4]] },
    { "id": "a6", "type": "Frozen", "dir": "Right", "cells": [[0,3]], "hits": 2 },
    { "id": "a7", "type": "Basic", "dir": "Up", "cells": [[5,3]] },
    { "id": "a8", "type": "Long", "dir": "Down", "cells": [[4,2],[4,3]] },
    { "id": "a9", "type": "Basic", "dir": "Left", "cells": [[5,2]] },
    { "id": "a10", "type": "Frozen", "dir": "Right", "cells": [[2,0]], "hits": 2 },
    { "id": "a11", "type": "Basic", "dir": "Up", "cells": [[3,0]] }
  ],
  "solution": ["a2", "a7", "a8", "a9", "a11", "a6", "a10", "a4", "a1", "a3", "a5"],
  "meta": { "author": "desktop", "difficulty": 4, "minTaps": 13, "note": "Frozen 2개, 깊이 6" }
}
```

### Level 19
```
y5   1→   1→   1→   ·    2→   4↑ 
y4   ·    3↑   ·    ·    ·    4↑ 
y3   ·    5↑   ·  * 6←   ·    ·  
y2   ·    ·    ·    ·    ·    ·  
y1   7→   7→   ·    8↓ * 9→  10↓ 
y0   ·    ·    ·   11→   ·  *12→ 
    x0   x1   x2   x3   x4   x5
```
```json
{
  "version": 1,
  "id": 19,
  "width": 6,
  "height": 6,
  "arrows": [
    { "id": "a1", "type": "Long", "dir": "Right", "cells": [[0,5],[1,5],[2,5]] },
    { "id": "a2", "type": "Basic", "dir": "Right", "cells": [[4,5]] },
    { "id": "a3", "type": "Basic", "dir": "Up", "cells": [[1,4]] },
    { "id": "a4", "type": "Long", "dir": "Up", "cells": [[5,4],[5,5]] },
    { "id": "a5", "type": "Basic", "dir": "Up", "cells": [[1,3]] },
    { "id": "a6", "type": "Frozen", "dir": "Left", "cells": [[3,3]], "hits": 2 },
    { "id": "a7", "type": "Long", "dir": "Right", "cells": [[0,1],[1,1]] },
    { "id": "a8", "type": "Basic", "dir": "Down", "cells": [[3,1]] },
    { "id": "a9", "type": "Frozen", "dir": "Right", "cells": [[4,1]], "hits": 2 },
    { "id": "a10", "type": "Basic", "dir": "Down", "cells": [[5,1]] },
    { "id": "a11", "type": "Basic", "dir": "Right", "cells": [[3,0]] },
    { "id": "a12", "type": "Frozen", "dir": "Right", "cells": [[5,0]], "hits": 2 }
  ],
  "solution": ["a4", "a12", "a2", "a10", "a11", "a1", "a3", "a5", "a6", "a8", "a9", "a7"],
  "meta": { "author": "desktop", "difficulty": 5, "minTaps": 15, "note": "Frozen 3개, 선택지 2" }
}
```

### Level 20
```
y5   ·    ·    ·    1→   1→   1→ 
y4   ·    2→   2→   ·    3→ * 4↑ 
y3   ·    ·    ·    6↑ * 5↑   ·  
y2   ·    ·    ·    6↑   9↑   ·  
y1   ·    ·    7→ * 8↑   9↑  10↑ 
y0  11↑  12↑   ·   13←   ·    ·  
    x0   x1   x2   x3   x4   x5
```
```json
{
  "version": 1,
  "id": 20,
  "width": 6,
  "height": 6,
  "arrows": [
    { "id": "a1", "type": "Long", "dir": "Right", "cells": [[3,5],[4,5],[5,5]] },
    { "id": "a2", "type": "Long", "dir": "Right", "cells": [[1,4],[2,4]] },
    { "id": "a3", "type": "Basic", "dir": "Right", "cells": [[4,4]] },
    { "id": "a4", "type": "Frozen", "dir": "Up", "cells": [[5,4]], "hits": 2 },
    { "id": "a5", "type": "Frozen", "dir": "Up", "cells": [[4,3]], "hits": 2 },
    { "id": "a6", "type": "Long", "dir": "Up", "cells": [[3,2],[3,3]] },
    { "id": "a7", "type": "Basic", "dir": "Right", "cells": [[2,1]] },
    { "id": "a8", "type": "Frozen", "dir": "Up", "cells": [[3,1]], "hits": 2 },
    { "id": "a9", "type": "Long", "dir": "Up", "cells": [[4,1],[4,2]] },
    { "id": "a10", "type": "Basic", "dir": "Up", "cells": [[5,1]] },
    { "id": "a11", "type": "Basic", "dir": "Up", "cells": [[0,0]] },
    { "id": "a12", "type": "Basic", "dir": "Up", "cells": [[1,0]] },
    { "id": "a13", "type": "Basic", "dir": "Left", "cells": [[3,0]] }
  ],
  "solution": ["a1", "a4", "a6", "a8", "a10", "a11", "a3", "a5", "a9", "a2", "a7", "a12", "a13"],
  "meta": { "author": "desktop", "difficulty": 5, "minTaps": 16, "note": "Frozen 구간 마무리: 13개, 선택지 2, 깊이 6, 점유율 최고" }
}
```

## 8. 다음 작업
- [x] 1~10 검증기 재검증 (2026-09-17, 오류 0)
- [ ] Levels/ 저장 (프로그래머 레벨 에디터 툴, 이슈 #7)
- [x] 레벨 11~20 (6×6, Frozen 16 도입) — W-008 (§7)
- [ ] 레벨 21~30 (7×7, Key/Locked 26 도입)
