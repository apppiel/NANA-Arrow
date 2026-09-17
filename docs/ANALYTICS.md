# NANA-Arrow — ANALYTICS.md (v0.1 — 2026-09-17, W-014 / 이슈 #26)
> 작성: 기획자/PM. 전송: Firebase Analytics, 반드시 `SafeAnalytics.LogEvent` 래퍼 경유 (W-010 이식)
> 목적: **레벨 난이도 조정**(어디서 막히고 포기하나)과 **광고 수익 구조 확인**(이어하기·전면 빈도). 개인정보는 보내지 않는다

## 1. 규칙
- 이벤트·파라미터 이름: 영문 소문자 snake_case, 40자 이하. 문자열 값 100자 이하, 이벤트당 파라미터 25개 이하 (Firebase 제한)
- Firebase 예약 이름(`first_open`, `session_start`, `app_update`, `ad_impression` 등 자동 수집) 은 직접 보내지 않는다
- `level_start` 는 Firebase 권장 이벤트 이름과 같음 → 파라미터 `level_name` 을 함께 보내 권장 보고서에도 잡히게 한다
- 레벨 번호는 숫자 `level` + 문자열 `level_name`(`"level_012"`) 둘 다
- 이벤트 호출은 게임플레이 코드가 아니라 **이벤트를 받는 한 곳**(`AnalyticsReporter` 제안, Services)에서만. GameSession·AdsManager 는 기존 이벤트만 발행
- 에디터·개발 빌드는 전송하지 않음 (`sendInEditor` 인스펙터 값, 기본 false)
- 디바이스 ID·응모 코드 값은 보내지 않음

## 2. 공통 파라미터 (레벨 관련 이벤트 전부)
| 이름 | 타입 | 설명 |
|---|---|---|
| level | int | 레벨 번호 |
| level_name | string | `level_###` |
| board | string | `8x10` |
| arrows | int | 화살표 수 |
| is_replay | int | 이미 클리어한 레벨이면 1 |
| attempt | int | 이 레벨 누적 시도 횟수 (레벨 시작마다 +1, 로컬 저장) |

## 3. 이벤트 목록
### 3-1. 레벨 진행
| 이벤트 | 언제 | 추가 파라미터 |
|---|---|---|
| `level_start` | Game 씬에서 보드 등장 직후 (다시하기 포함) | `start_reason`: `first` / `next` / `select` / `retry_hud` / `retry_fail` |
| `level_clear` | 마지막 Exit (팝업 전) | `duration_sec`, `taps`, `blocks`, `lives_left`, `continues_used`, `lane_previews` |
| `level_fail` | 하트 0 (실패 팝업 표시 시) | `duration_sec`, `taps`, `blocks`, `arrows_left`, `continues_used` |
| `level_quit` | 메인으로 확인 `나가기` / 앱이 Game 중 종료(OnApplicationPause 에서 best effort) | `duration_sec`, `taps`, `arrows_left`, `lives_left`, `quit_reason`: `menu` / `app_pause` |
| `retry` | HUD 다시하기 또는 실패 팝업 다시하기 | `retry_source`: `hud` / `fail_popup`, `taps`, `arrows_left` |

- `taps` = 발사 시도 수(얼음 깨기 포함), `blocks` = Block 횟수(Marked 재탭 포함), `lane_previews` = 길게 누르기 횟수
- **난이도 판단 지표**: 레벨별 `level_fail ÷ level_start`, `level_quit` 비율, `attempt` 중앙값, `blocks` 평균

### 3-2. 광고
| 이벤트 | 언제 | 파라미터 |
|---|---|---|
| `continue_offer` | 실패 팝업에 이어하기 버튼이 보일 때 | 공통 + `ad_ready`: 0/1 |
| `continue_request` | 이어하기 버튼 탭 | 공통 + `continues_used` |
| `continue_granted` | 보상 지급 (OnUserEarnedReward) | 공통 + `lives_after` |
| `ad_interstitial` | 전면 광고 판정 결과 | `trigger`: `clear` / `fail_retry`, `result`: `shown` / `not_ready` / `skipped_free_level`, `level` |
| `ad_rewarded_result` | 보상형 광고 종료 | `result`: `rewarded` / `closed_early` / `failed_to_show`, `level` |

- 노출 수익 자체는 AdMob 연동 시 자동 `ad_impression` 으로 수집 → 직접 보내지 않음

### 3-3. 튜토리얼·기능 사용
| 이벤트 | 언제 | 파라미터 |
|---|---|---|
| `tutorial_step` | TutorialConfig 항목 표시 | `level`, `text_key`, `trigger` |
| `tutorial_done` | 항목의 hideOn 조건 충족 | `level`, `text_key`, `elapsed_sec` |
| `lane_preview_first` | 사용자 첫 길게 누르기 (1회만, 로컬 플래그) | `level` |

### 3-4. 메뉴·보상
| 이벤트 | 언제 | 파라미터 |
|---|---|---|
| `level_select_open` | 레벨 선택 패널 열기 | `highest_level` |
| `settings_change` | 설정 토글 변경 | `setting`: `sound` / `vibration`, `value`: 0/1 |
| `reward_code_issued` | 100 클리어로 코드 발급 | `synced`: 0/1 (Firestore 저장 성공 여부) |
| `reward_code_copy` | 코드 복사 | - |
| `reward_link_open` | 상품 받으러 가기 | - |

## 4. 사용자 속성 (setUserProperty)
| 이름 | 값 | 갱신 시점 |
|---|---|---|
| highest_level | 문자열 숫자 (`"12"`) | 클리어로 최고 레벨이 오를 때 |
| sound_on | `"1"`/`"0"` | 설정 변경, 앱 시작 |
| vibration_on | `"1"`/`"0"` | 설정 변경, 앱 시작 |
| reward_issued | `"1"`/`"0"` | 코드 발급 시 |

## 5. 보고서 (Firebase 콘솔에서 만들 것)
1. **레벨 퍼널**: level_start(level=1) → level_clear(1) → … → level_clear(20). 레벨별 이탈 위치
2. **레벨별 실패율 표**: level 차원으로 level_fail / level_start, 평균 blocks, 평균 attempt → LEVEL_DESIGN 조정 근거
3. **이어하기 전환율**: continue_offer → continue_request → continue_granted
4. **전면 광고 빈도**: 사용자당 ad_interstitial(result=shown) / 세션
5. **튜토리얼 완료율**: tutorial_step → tutorial_done (text_key 별)
6. 리텐션 D1/D7 (자동 수집)

## 6. 구현 메모 (프로그래머)
- `AnalyticsReporter` (Services, MonoBehaviour 또는 순수 C# + 초기화 훅): GameSession·GameController·AdsManager·UI 이벤트를 구독해서 위 표대로 변환
- 레벨 단위 카운터(taps, blocks, lane_previews, 시작 시각)는 GameSession 시작 시 초기화
- `attempt` 는 SaveData 에 레벨별로 넣지 말고 PlayerPrefs `attempt_###` (분석 전용, 재화 아님)
- 테스트: 이벤트 이름·파라미터 이름이 40자 이하, 파라미터 25개 이하인지 EditMode 테스트로 고정
