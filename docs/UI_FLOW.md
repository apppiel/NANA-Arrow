# NANA-Arrow — UI_FLOW.md (v0.2 — 2026-09-17 GAME_RULES v0.6.1 §0 HUD·§11 결정 반영, 프리팹 가이드, TutorialConfig, W-012)
> 작성: 기획자/PM. 기준: GAME_RULES v0.6.1 (§0 HUD·그래픽, §2 목숨·이어하기, §7 광고, §11 UI 결정), LEVEL_DESIGN v1.0
> v0.1 → v0.2 주요 변경
> - **게임 화면 HUD = 좌상단 원형 버튼 2개(뒤로가기·다시하기) + 상단 가운데 하트 3개.** 레벨 번호·설정 버튼 없음 → 게임 중 설정 팝업 삭제, 설정은 Main 에서만
> - §11 결정 5건 확정 반영 (⚠ 표시 제거): 진동 토글 추가, 다시하기는 실패로 안 셈, 중간 저장 없음, 한국어만(키 분리 유지), 재클리어도 광고 카운트
> - 튜토리얼 = 하단 말풍선 + 손가락, 레인 미리보기(길게 누르기) 안내 추가
> - §12 팀장용 단계별 프리팹 가이드, §7 TutorialConfig 필드 정의

## 1. 전체 흐름
```
[Boot] 로딩 ─▶ [Main] ─ 시작하기 ───────────▶ [Game Lv N]
                 │  └ 레벨 선택 ─ 레벨 탭 ─▶ [Game Lv K]
                 │  └ 설정 팝업 (사운드·진동)
                 │  └ 응모 코드 보기 (100 클리어 후)
                 └ 뒤로가기 ─▶ 종료 확인

[Game] 탭 플레이 (길게 누르면 레인 미리보기)
  ├ 모든 화살표 Exit ─(clearPopupDelay)─▶ 클리어 팝업
  │     ├ 다음 레벨 ─▶ (전면 광고 판정) ─▶ [Game Lv N+1]
  │     │               └ Lv100 클리어였다면 응모 코드 팝업 먼저
  │     └ 메인으로 ─▶ (전면 광고 판정) ─▶ [Main]
  ├ 하트 0 ─▶ 실패 팝업
  │     ├ 광고 보고 이어하기 ─▶ 보상형 광고 ─ 보상 ─▶ 하트 +1, 같은 보드에서 계속
  │     └ 다시하기 ─▶ (전면 광고 판정) ─▶ 같은 레벨 처음부터
  ├ HUD 다시하기 ─▶ 확인 없이 즉시 재시작 (실패로 안 셈, 광고 없음)
  └ HUD 뒤로가기 / Android 뒤로가기 ─▶ 메인으로 확인 팝업
```
- 광고 판정은 전부 `AdsManager`. UI 는 이벤트만 보내고 광고 종료 콜백 후 이동 (§8)

## 2. 공통 규칙
- 세로 고정, Safe Area 안에 HUD·버튼 배치. 배경 순백 `#FFFFFF`, 선·아이콘 짙은 남색 `#141A33` (GAME_RULES §0)
- 팝업이 열려 있는 동안 보드 입력 차단. 팝업은 한 번에 하나 (응모 코드 → 클리어처럼 연속이면 앞 팝업을 닫고 다음을 연다)
- 버튼: 주 버튼 1개(채운 색) + 보조 버튼(외곽선 또는 텍스트). 최소 터치 영역 약 48dp (1080 기준 약 130px)
- **문구는 키로 관리** (§9). v1 은 한국어만이지만 코드에 문구를 직접 쓰지 않는다 (영어 추가 대비)
- 팝업 열림·닫힘 시간, 로딩 최소 시간 등 UI 수치는 전부 인스펙터 값 (`UIConfig` ScriptableObject 제안)
- **Android 뒤로가기**
| 위치 | 동작 |
|---|---|
| 팝업 열림 | 팝업 닫기와 같음. 단 실패 팝업·응모 코드 팝업은 닫히지 않음(선택 강제) |
| Game (팝업 없음) | HUD 뒤로가기와 같음 → 메인으로 확인 팝업 |
| 레벨 선택 | Main 으로 |
| Main | 종료 확인 팝업 |

## 3. Boot
- 화면: 흰 배경 + 로고(NANABOX) + 로딩 표시
- 하는 일: 저장 로드 → 설정(사운드·진동) 적용 → 광고 SDK 초기화 시작(**완료를 기다리지 않음**, 실패해도 진행) → Main
- 최소 표시 시간 `bootMinDuration` (기본 1.0초)

## 4. Main
| 요소 | 내용 |
|---|---|
| 타이틀 | 게임 로고 |
| 주 버튼 | **시작하기** + 아래 작은 글씨 `레벨 N` (N = 아직 안 깬 가장 낮은 레벨. 전부 깼으면 마지막 레벨) |
| 보조 버튼 | **레벨 선택** |
| 우상단 | 설정(톱니) 아이콘 → 설정 팝업 (**설정은 여기서만**) |
| 조건부 | 100 클리어 후 **응모 코드 보기** 버튼 표시 |

### 레벨 선택 (Main 안의 패널)
- 5열 그리드, 세로 스크롤. 상단 좌측 뒤로(←), 가운데 `레벨 선택`
- 칸 상태 3가지: **클리어**(체크) / **다음 도전**(강조 테두리) / **잠김**(자물쇠, 탭하면 흔들림만)
- 열리는 조건: 클리어한 레벨 + 그 다음 1개. 클리어한 레벨은 다시 플레이 가능 (재클리어도 전면 광고 카운트에 포함)
- 별점 없음

## 5. Game
```
┌───────────────────────────────┐
│ (←)(↻)        ♥ ♥ ♥           │  ← HUD (상단, Safe Area). 레벨 번호·설정 없음
│                               │
│                               │
│          ┌───────┐            │
│          │ 보 드  │            │  ← 격자 없음, 선만. 화면 중앙
│          └───────┘            │     셀 크기는 레벨과 무관하게 일정 (LEVEL_DESIGN §1)
│                               │
│    ┌─────────────────────┐    │
│    │  이동하려면 탭하세요   │    │  ← 튜토리얼 말풍선 (평소 숨김)
│    └─────────────────────┘    │
└───────────────────────────────┘
```
- **HUD**: 좌상단 원형 버튼 2개 (연한 보라 `#E9E4FF` 배경 + 남색 아이콘) — 뒤로가기(←), 다시하기(↻). 상단 가운데 하트 3개(빨강)
- **다시하기(HUD)**: 확인 없이 즉시 레벨 처음부터. **실패 횟수에 포함하지 않음, 광고 없음** (GAME_RULES §11)
- **뒤로가기(HUD)**: 메인으로 확인 팝업(6-3)
- **하트 감소**: 해당 하트가 흔들리며 빈 하트 + 진동(설정 켜짐일 때, `vibrateOnLifeLost`). 이어하기로 회복하면 다시 채워지는 연출
- **Block**: 레인이 빨갛게 번쩍 + 머리 살짝 튕김. Marked 화살표는 선 전체 빨강
- **레인 미리보기**: 화살표를 `longPressSeconds`(0.35) 이상 누르면 레인 하이라이트, 손 떼면 사라짐. 목숨·Marked 영향 없음, 발사하지 않음
- **클리어 순간**: 마지막 Exit 후 입력 즉시 차단 → `clearPopupDelay` 뒤 클리어 팝업
- **앱 백그라운드 → 복귀**: 보드 그대로. **앱 종료 시 중간 상태 저장 없음** → 다음 실행 때 그 레벨 처음부터

## 6. 팝업
### 6-1. 클리어
| 요소 | 문구 키 → 문구 |
|---|---|
| 제목 | clear.title → `클리어!` |
| 부제 | clear.level → `레벨 {0}` |
| 주 버튼 | clear.next → `다음 레벨` |
| 보조 버튼 | clear.main → `메인으로` |
- 두 버튼 모두 먼저 `LevelCleared` 판정 → 전면 광고(해당 시) → 이동 (§8)
- 마지막 레벨(현재 탑재 수) 클리어 시 주 버튼은 `메인으로`, 부제에 clear.all_done

### 6-2. 실패 (하트 0)
| 요소 | 문구 |
|---|---|
| 제목 | fail.title → `하트를 모두 잃었어요` |
| 주 버튼 | fail.continue → `광고 보고 이어하기` + 아이콘(▶ + ♥1) |
| 보조 버튼 | fail.retry → `다시하기` |
| 광고 준비 안 됨 | 주 버튼 비활성 + fail.ad_unavailable |
- 레벨당 `maxContinues`(1)회를 다 쓰고 다시 하트 0 → 주 버튼 숨김, `다시하기` 만
- 광고를 끝까지 보지 않고 닫음 → 보상 없음, 팝업 그대로
- 광고 보상 → 팝업 닫고 하트 +`continueLives`, 보드·Marked 상태 그대로
- 닫기(X) 없음, 뒤로가기로 닫히지 않음
- 이 팝업의 `다시하기` 만 **실패 횟수에 포함** (`RetryPressed`)

### 6-3. 메인으로 확인 (Game)
- 본문 confirm_main.body `메인으로 갈까요?` / 작은 글씨 confirm_main.sub `진행 중인 레벨은 처음부터 다시 시작해요`
- 주 버튼 confirm_main.stay `계속하기`(닫기) / 보조 버튼 confirm_main.leave `나가기`(Main, 광고 없음)

### 6-4. 설정 (Main 전용)
| 요소 | 내용 |
|---|---|
| 제목 | settings.title `설정` |
| 토글 | settings.sound `사운드` (BGM·효과음 한 번에), 기본 켜짐 |
| 토글 | settings.vibration `진동` (하트 감소 시에만 진동), 기본 켜짐 |
| 닫기(X) | 우상단 |
- 두 값 모두 PlayerPrefs 저장 (재화 아님)

### 6-5. 응모 코드 (100 클리어)
- 100 클리어 직후 클리어 팝업보다 **먼저** 표시. Main 의 `응모 코드 보기` 로 다시 열 수 있음
- 제목 raffle.title `100단계 달성!` / 본문 raffle.body / 코드 `XXXX-XXXX` 크게 / raffle.copy `코드 복사` (누르면 raffle.copied 토스트) / raffle.go `상품 받으러 가기` → `https://nanabox.co.kr/reward-claim.html` (RewardConfig) / common.close
- 코드 규칙: XXXX-XXXX, 0/O/1/I 제외 32자 (W-009 조사, NO.3 이식)

### 6-6. 종료 확인 (Main)
- 본문 quit.body `게임을 종료할까요?` / 주 버튼 quit.cancel `취소` / 보조 버튼 quit.ok `종료`

## 7. 튜토리얼 — `TutorialConfig` ScriptableObject
레벨 JSON 이 아니라 SO 에 둔다. 인스펙터에서 전부 수정 가능. 튜토리얼 중에도 입력은 막지 않는다 (틀려도 목숨 규칙으로 흡수)

### 7-1. 필드 정의 (프로그래머 구현 기준)
```csharp
// Assets/_Project/Scripts/UI/Tutorial/TutorialConfig.cs (제안)
[CreateAssetMenu(menuName = "NanaArrow/Tutorial Config")]
public sealed class TutorialConfig : ScriptableObject
{
    [SerializeField] private TutorialStep[] steps;
    [SerializeField, Min(0f), Tooltip("말풍선 자동 숨김 (초). 0 이면 조건이 올 때까지 유지")]
    private float hideDelay = 3f;
    [SerializeField, Tooltip("같은 레벨을 다시 할 때도 표시")]
    private bool showOnReplay = false;
}

[Serializable]
public struct TutorialStep
{
    [Min(1)] public int level;                 // 레벨 번호
    public TutorialTrigger trigger;            // 언제 표시하나
    public string textKey;                     // §9 문구 키. 비우면 말풍선 숨김
    public string targetArrowId;               // 손가락이 가리킬 화살표 id. 비우면 손가락 없음
    public FingerAnchor fingerAnchor;          // 화살표 경로의 어느 칸을 가리키나
    public Vector2 fingerOffsetCells;          // 셀 단위 추가 오프셋
    public TutorialTrigger hideOn;             // 언제 숨기나 (None = hideDelay 만 사용)
}

public enum TutorialTrigger { None, LevelStart, FirstTap, FirstExit, FirstBlock, FirstIceBreak, FirstLongPress }
public enum FingerAnchor { Head, Middle, Tail }
```
- 저장: `tutorialSeen` 은 저장하지 않는다 (중간 저장 없음 원칙). 대신 `showOnReplay=false` 면 **그 레벨을 한 번이라도 클리어했으면** 표시 안 함 (최고 레벨 기준)
- 말풍선 위치·크기는 프리팹(§12-4)에서 조정

### 7-2. 레벨별 항목 (LEVEL_DESIGN v1.0 §6)
| level | trigger | textKey | targetArrowId | fingerAnchor | hideOn |
|---|---|---|---|---|---|
| 1 | LevelStart | tut.tap | a2 | Middle | FirstExit |
| 2 | LevelStart | tut.free_first | a3 | Head | FirstTap |
| 2 | FirstBlock | tut.block | (비움) | - | None (3초) |
| 4 | LevelStart | tut.long_press | (비움) | - | FirstLongPress |
| 16 | LevelStart | tut.frozen | a2 | Head | FirstIceBreak |
| 26 | LevelStart | tut.key | a12 | Head | FirstExit |

## 8. 광고 연결 지점 (GAME_RULES §7, §11)
| UI 동작 | UI 이벤트 | AdsManager 판단 | 끝난 뒤 |
|---|---|---|---|
| 클리어 팝업 버튼 | `LevelCleared(level, alreadyCleared)` | level > adFreeLevels 이고 클리어 N회마다 → 전면. **재클리어도 카운트** | 다음 레벨 / Main |
| 실패 팝업 `다시하기` | `RetryPressed(failCountThisLevel)` | 같은 레벨 실패 `interstitialAfterFails` 회마다 → 전면 | 재시작 |
| 실패 팝업 `광고 보고 이어하기` | `ContinueRequested(continuesUsed)` | maxContinues 남았는지 + 보상형 준비 | 보상 → 하트 회복 |
| HUD 다시하기, 메인으로 확인 `나가기` | 없음 | - | 바로 이동 |
- 전면 광고가 준비 안 됐으면 기다리지 않고 바로 이동. 광고 재생 중 BGM 일시정지

## 9. 문구 표 (키 → 한국어)
| 키 | 문구 |
|---|---|
| main.start | 시작하기 |
| main.level_sub | 레벨 {0} |
| main.level_select | 레벨 선택 |
| main.raffle | 응모 코드 보기 |
| select.title | 레벨 선택 |
| clear.title | 클리어! |
| clear.level | 레벨 {0} |
| clear.next | 다음 레벨 |
| clear.main | 메인으로 |
| clear.all_done | 준비된 레벨을 모두 깼어요! |
| fail.title | 하트를 모두 잃었어요 |
| fail.continue | 광고 보고 이어하기 |
| fail.retry | 다시하기 |
| fail.ad_unavailable | 지금은 광고를 불러올 수 없어요 |
| settings.title | 설정 |
| settings.sound | 사운드 |
| settings.vibration | 진동 |
| confirm_main.body | 메인으로 갈까요? |
| confirm_main.sub | 진행 중인 레벨은 처음부터 다시 시작해요 |
| confirm_main.stay | 계속하기 |
| confirm_main.leave | 나가기 |
| raffle.title | 100단계 달성! |
| raffle.body | 아래 코드로 경품 이벤트에 응모할 수 있어요 |
| raffle.copy | 코드 복사 |
| raffle.copied | 복사했어요 |
| raffle.go | 상품 받으러 가기 |
| common.close | 닫기 |
| quit.body | 게임을 종료할까요? |
| quit.cancel | 취소 |
| quit.ok | 종료 |
| tut.tap | 이동하려면 탭하세요 |
| tut.free_first | 앞이 비어 있는 화살표부터 날려 보세요 |
| tut.block | 앞이 막혀 있으면 하트가 줄어요 |
| tut.long_press | 화살표를 길게 누르면 가는 길이 보여요 |
| tut.frozen | 얼음 화살표는 한 번 탭해 얼음을 깨고, 다시 탭해 날려요 |
| tut.key | 열쇠 화살표를 모두 날리면 같은 색 자물쇠가 풀려요 |
- v0.1 의 hud.level, settings.retry, settings.main 은 삭제 (게임 화면에 레벨 번호·설정 없음)
- 저장 형식 제안: `Assets/_Project/Settings/Strings_ko.asset` (키·문구 리스트 SO). 코드는 키만 참조

## 10. 사운드 리스트
| 키 | 언제 | 비고 |
|---|---|---|
| bgm_main | Main·레벨 선택 | 루프 |
| bgm_game | Game | 루프. 1곡 공용 가능 |
| sfx_button | 모든 버튼 | 짧게 |
| sfx_fire | Fire 시작 | 연속 탭 대비 짧고 겹쳐도 괜찮게 |
| sfx_block | Block (레인 번쩍) | |
| sfx_life_lost | 하트 감소 | sfx_block 과 동시 |
| sfx_ice_break | Frozen 얼음 깨짐 | |
| sfx_lock_shake | 잠긴 Locked 탭 | |
| sfx_unlock | 그룹 Key 전부 Exit → Locked 해제 | |
| sfx_clear | 클리어 팝업 | |
| sfx_fail | 실패 팝업 | |
| sfx_heart_restore | 이어하기 보상 | |
| sfx_popup | 팝업 열림 | |
- 사운드 토글 하나로 BGM·효과음 동시 on/off. 볼륨은 인스펙터. 레인 미리보기는 소리 없음

## 11. 확정 사항 (v0.1 ⚠ 5건 → GAME_RULES §11)
1. 진동 토글 추가, 기본 켜짐, 하트 감소 시에만
2. 다시하기(HUD)는 실패로 안 셈, 광고 없음 — v0.1 의 설정 팝업 다시하기가 HUD 버튼으로 옮겨짐
3. 레벨 중간 저장 없음
4. v1 한국어만, 문구는 키로 분리
5. 재클리어도 전면 광고 카운트 포함

## 12. 팀장용 프리팹·씬 조립 가이드
> 전제: 프로그래머 W-010 의 `PopupBase`, `SafeAreaAdapter`, `SceneLoader`, `AudioManager`, `LevelCatalog` 가 머지된 뒤. 아래 **[스크립트]** 표시는 그 이름을 쓴다 (이름이 다르면 W-010 보고를 따른다). **[필요]** 표시는 아직 만들 사람이 정해지지 않은 스크립트 → 프로그래머 요청 목록(§12-8)
> 좌표는 Canvas 기준 해상도 1080×1920

### 12-1. 공통 준비 (한 번만)
1. **한글 폰트**: TextMeshPro 기본 폰트에는 한글이 없음
   - Window → TextMeshPro → Import TMP Essential Resources (처음 한 번)
   - 한글 폰트 파일(예: Pretendard 또는 Noto Sans KR, 상업 이용 가능한 것)을 `Assets/_Project/Fonts/` 에 넣기 → 기존 게임(NO.3) 에 쓰던 폰트가 있으면 그것을 그대로 사용
   - Window → TextMeshPro → Font Asset Creator → Source Font = 위 폰트, Atlas 4096×4096, Character Set = Custom Characters(§9 문구 + 숫자·영문) 또는 Dynamic → Generate → `Assets/_Project/Fonts/Main_SDF.asset` 으로 저장
   - Edit → Project Settings → TextMesh Pro → Settings → Default Font Asset 에 `Main_SDF` 지정
2. **색**: 남색 `#141A33`, 연보라 `#E9E4FF`, 하트 빨강 `#E8453C`, 흰색 `#FFFFFF`, 딤 `#000000` 알파 50%
3. 폴더: `Assets/_Project/Prefabs/UI/` 생성

### 12-2. 각 씬의 캔버스 (Boot / Main / Game 모두 같은 방법)
1. Hierarchy 우클릭 → UI → Canvas → 이름 **`UICanvas`**
   - Canvas: Render Mode = Screen Space - Overlay
   - Canvas Scaler: UI Scale Mode = **Scale With Screen Size**, Reference Resolution = **1080 × 1920**, Screen Match Mode = Match Width Or Height, **Match = 0** (폭 기준)
2. 함께 생긴 **`EventSystem`** 선택 → `Standalone Input Module` 이 있으면 인스펙터의 **Replace with InputSystemUIInputModule** 버튼 클릭
3. UICanvas 아래 빈 오브젝트 **`SafeArea`** → Rect Transform 앵커 프리셋에서 Alt+Shift 누르고 **stretch-stretch** (전체 채움) → **[스크립트] SafeAreaAdapter** 추가
4. SafeArea 아래 빈 오브젝트 **`Popups`** (stretch-stretch). 팝업은 모두 여기 아래에 둔다 (마지막 자식이 위에 그려짐)

### 12-3. Game 씬 HUD
SafeArea 아래 빈 오브젝트 **`HUD`**: 앵커 **top-stretch**, Pivot (0.5, 1), Pos Y 0, Height 200
| 오브젝트 | 앵커 | 위치 / 크기 | 컴포넌트 | 비고 |
|---|---|---|---|---|
| `BackButton` | top-left | Pos (100, -100), 120×120 | Image(원형 스프라이트, `#E9E4FF`) + Button | 자식 `Icon`: Image 56×56, 남색, ← 아이콘 |
| `RetryButton` | top-left | Pos (244, -100), 120×120 | 위와 같음 | 자식 `Icon`: ↻ 아이콘 |
| `Hearts` | top-center | Pos (0, -100), 240×64 | Horizontal Layout Group (Spacing 16, Child Alignment Middle Center, Control Child Size 끔) + **[필요] LivesView** | 자식 `Heart_0`~`Heart_2`: Image 64×64 `#E8453C` (가득/빈 하트 스프라이트 2종) |
- 원형 스프라이트가 없으면: Image 의 Source Image 에 Unity 기본 `Knob` 사용 (임시)
- 버튼 연결 (스크립트 준비 후): BackButton.OnClick → `Popup_ConfirmMain` 의 **PopupBase.Open** / RetryButton.OnClick → **GameController 의 재시작 메서드** ([필요] 이름 확인)

### 12-4. Game 씬 튜토리얼
1. SafeArea 아래 **`TutorialBubble`**: 앵커 **bottom-center**, Pivot (0.5, 0), Pos Y 240, 크기 820×150
   - Image: 흰색 둥근 사각형(없으면 기본 `UISprite`, Image Type = Sliced), 그림자는 `Shadow` 컴포넌트 (Effect Distance 0, -6, 알파 25%)
   - 자식 `Text`: TextMeshPro - Text(UI), stretch-stretch, 여백 32, 크기 44, 남색, 가운데 정렬
   - 처음엔 비활성
2. UICanvas 바로 아래(SafeArea 밖) **`Finger`**: Image 160×160 (손가락 스프라이트), Raycast Target **끔**, 처음엔 비활성
3. 빈 오브젝트 **`Tutorial`** → **[필요] TutorialPresenter** 추가 → Config = TutorialConfig 에셋, Bubble = TutorialBubble, Bubble Text = Text, Finger = Finger, Board View = Board
4. `Assets/_Project/Settings` 우클릭 → Create → NanaArrow → Tutorial Config → §7-2 표대로 입력

### 12-5. 팝업 프리팹 (공통 틀 한 번 만들고 복제)
1. Popups 아래 빈 오브젝트 **`Popup_Clear`** (stretch-stretch) → **Canvas Group** + **[스크립트] PopupBase** 추가
2. 자식 **`Dim`**: Image 검정 알파 50%, stretch-stretch, Raycast Target **켬** (뒤 보드 탭 차단)
3. 자식 **`Panel`**: 앵커 middle-center, 폭 880, Image 흰색 둥근 사각형(Sliced)
   - Vertical Layout Group: Padding 64, Spacing 32, Child Alignment Upper Center, Control Child Size 폭만 켬, Child Force Expand 끔
   - Content Size Fitter: Vertical Fit = Preferred Size
4. Panel 자식 (위에서부터)
   - `Title`: TMP 64, Bold, 남색, 가운데
   - `Subtitle`: TMP 40, 남색 알파 70%
   - `PrimaryButton`: Button, 높이 140 (Layout Element Preferred Height 140), Image 남색, 자식 TMP 48 흰색
   - `SecondaryButton`: Button, 높이 120, Image 투명(알파 0), 자식 TMP 44 남색
5. PopupBase 에 Title/버튼 참조 연결 (필드는 W-010 보고 기준), 비활성 상태로 둔 뒤 Project 창 `Prefabs/UI/` 로 드래그 → 프리팹 생성
6. 프리팹을 복제(Ctrl+D)해서 나머지 팝업 생성
| 프리팹 | 씬 | 차이점 |
|---|---|---|
| `Popup_Clear` | Game | 기본 틀 그대로 |
| `Popup_Fail` | Game | PrimaryButton 안에 ▶ 아이콘 + ♥1 표시 추가, Panel 아래 `AdUnavailableText`(TMP 32, 비활성). Subtitle 삭제. **Dim 탭·뒤로가기로 닫히지 않게** (PopupBase 옵션 [필요]) |
| `Popup_ConfirmMain` | Game | Title → 본문(TMP 48), Subtitle → 작은 글씨 |
| `Popup_Raffle` | Game·Main | Subtitle 아래 `CodeText`(TMP 80 Bold, 자간 넓게) + `CopyButton`(보조 스타일) 추가, 우상단 `CloseButton`. 닫기 불가 옵션 |
| `Popup_Settings` | Main | Title + `SoundToggle`, `VibrationToggle` (Toggle, 행 높이 120, 왼쪽 라벨 TMP 44 / 오른쪽 스위치) + 우상단 `CloseButton`(X 72×72). Primary/Secondary 삭제 |
| `Popup_Quit` | Main | ConfirmMain 과 같은 모양 |

### 12-6. Main 씬
SafeArea 아래
| 오브젝트 | 앵커 | 위치 / 크기 | 비고 |
|---|---|---|---|
| `Logo` | top-center | Pos (0, -420), 600×300 | Image |
| `StartButton` | middle-center | Pos (0, -120), 640×180 | 주 버튼 스타일. 자식 `Label`(TMP 60) + `LevelLabel`(TMP 36, 알파 70%, 아래쪽) |
| `LevelSelectButton` | middle-center | Pos (0, -340), 640×130 | 보조 버튼 스타일 |
| `SettingsButton` | top-right | Pos (-100, -100), 120×120 | 원형 버튼 + 톱니 아이콘 → Popup_Settings.Open |
| `RaffleButton` | bottom-center | Pos (0, 200), 640×120 | 처음엔 비활성 (100 클리어 후 [필요] MainMenu 가 켬) |
| `LevelSelectPanel` | stretch-stretch | - | 흰 배경 Image, 처음엔 비활성 |

LevelSelectPanel 내부
1. `Header` (top-stretch, 높이 200): `BackButton`(원형, 좌) + `Title`(TMP 56, `레벨 선택`)
2. `Scroll`: UI → Scroll View, 앵커 stretch (Top 200), Horizontal 끔, 스크롤바 숨김
3. Scroll/Viewport/Content: **Grid Layout Group** (Cell 170×170, Spacing 30×30, Constraint = Fixed Column Count **5**, Child Alignment Upper Center, Padding 40) + Content Size Fitter (Vertical = Preferred)
4. 칸 프리팹 **`LevelCell`** (`Prefabs/UI/`): Button + Image(둥근 사각형) / 자식 `Number`(TMP 56) / `Check`(체크 아이콘, 우하단 48×48) / `Lock`(자물쇠 아이콘, 가운데 64×64) / `Highlight`(테두리 Image, 남색) — **[필요] LevelSelectView** 가 카탈로그 개수만큼 생성

### 12-7. Boot 씬
- UICanvas/SafeArea 아래 `Logo`(middle-center 600×300) + `Loading`(TMP 36 또는 회전 아이콘, Logo 아래 Pos Y -260)
- 빈 오브젝트 `Boot` → **[스크립트] BootLoader** (W-010)

### 12-8. 프로그래머에게 필요한 스크립트 (디렉터 배정 요청)
| 스크립트 | 역할 |
|---|---|
| `LivesView` | 하트 3개 표시·감소/회복 연출·진동 호출 |
| `TutorialPresenter` | TutorialConfig 읽기, 트리거 감지(GameSession 이벤트), 말풍선·손가락 표시 |
| `MainMenu` | 시작 레벨 계산, 응모 코드 버튼 표시, 설정 팝업 열기 |
| `LevelSelectView` | 레벨 칸 생성·상태 표시 |
| `SettingsPopup` | 토글 ↔ PlayerPrefs(sound, vibration) |
| `GameController` 재시작·메인 이동 메서드 | HUD 버튼·팝업에서 호출 |
| `PopupBase` 옵션 | Dim 탭/뒤로가기로 닫기 허용 여부 (`closableByBack`) |
| `Strings` (문구 SO) | §9 키 → 문구 조회 |
