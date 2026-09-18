# ASSETS.md — 에셋 출처·라이선스 대장

> 작성: 프로그래머 (W-028, 디렉터가 `docs/` 예외 허용). 에셋을 추가·교체하면 **이 표를 같이 갱신**한다.
> 목적: **스토어 심사·저작권 분쟁 대비**. "어디서 가져왔고, 상업적으로 써도 되는가" 를 한 곳에서 답할 수 있게 한다.
> ⚠️ **출시 전에 "교체 필요 = 예" 인 항목이 하나도 남아 있으면 안 된다.**

## 1. 우리 아트

| 파일 / 폴더 | 출처 | 라이선스 | 상업 이용 | 교체 필요 | 비고 |
|---|---|---|---|---|---|
| `Art/Sprites/Heart.png` | **망고보드** | 망고보드 이용약관 (플랜별) | ⚠️ **확인 필요** | 아니오 | 팀장 "사용해도 괜찮음"(09-18). 아래 §4 참고 |
| `Art/Fonts/NanumGothic.otf` | 네이버 나눔글꼴 | **SIL Open Font License 1.1** | ✅ 예 | 아니오 | 임베딩·재배포 자유. 폰트 파일만 판매 금지 |
| `Art/Fonts/NanumGothic SDF.asset` | 위 폰트로 생성 | 원본 따름 (OFL) | ✅ 예 | 아니오 | TMP Font Asset. 한글 표시에 필수 |
| `Art/FreeButtonSet/` (텍스처 107개) | **확인 필요 — 팀장이 임포트** | ❓ **불명** | ❓ **불명** | ⚠️ **예 (임시)** | 아래 §2 |
| `Art/FreeButtonSet/Fonts/Poppins-*.ttf` | Poppins (Indian Type Foundry) | 통상 SIL OFL 1.1 | ✅ 예(추정) | 아니오 | **게임에서 안 씀** — 한글이 없어 `NanumGothic SDF` 만 사용 |

## 2. ⚠️ `FreeButtonSet` — 출시 전 처리 필요

- **폴더 안에 LICENSE·README 가 없어 출처를 확인할 수 없습니다.** 이름으로 보아 무료 UI 팩으로 보이지만, 무료라고 상업 이용·재배포가 항상 허용되는 것은 아닙니다
- 디렉터 결정 (W-028): **금색 `FreeButtonSet` 은 임시 에셋.** 목표 팔레트는 GAME_RULES §9 의 **남색 `#141A33` + 연보라 `#E9E4FF`** 이고, 정식 아트는 팀장이 교체합니다
- **따라서 정식 아트로 교체되면 이 폴더는 프로젝트에서 삭제하는 것이 가장 안전합니다.** 교체 전까지 개발·내부 테스트에는 문제 없습니다
- 계속 쓰기로 한다면 **팀장이 받은 곳과 라이선스를 이 표에 채워 주세요** (에셋스토어 링크 등)

현재 쓰는 곳: 버튼 배경(`button_200`, `button_round_400`), 패널(`universal_panel_20/40`), 아이콘(`arrow_left`, `repeat`, `settings`, `x`, `checkmark`), 토글(`toggle_bg_100`, `radio_100/200`)

## 3. 코드·SDK (프로젝트에 포함되어 배포됨)

| 폴더 | 출처 | 라이선스 | 상업 이용 |
|---|---|---|---|
| `Assets/GoogleMobileAds`, `Plugins/Android/googlemobileads-*` | Google Mobile Ads (AdMob) Unity Plugin | Apache 2.0 | ✅ 예 |
| `Assets/Firebase`, `Plugins/Android/FirebaseApp.androidlib`, `FirebaseCrashlytics.androidlib` | Firebase Unity SDK | Apache 2.0 | ✅ 예 |
| `Assets/ExternalDependencyManager` | Google External Dependency Manager | Apache 2.0 | ✅ 예 |
| `Assets/TextMesh Pro` | Unity TextMeshPro | Unity Companion License | ✅ 예 |
| Unity 엔진 / URP / Input System | Unity Technologies | Unity 이용약관 (에디터 라이선스 따름) | ✅ 예 |

> SDK 는 표준 배포물이라 별도 조치가 필요 없습니다. 다만 **개인정보처리방침에 AdMob·Firebase 사용을 반드시 명시**해야 합니다 (스토어 심사 항목, `docs/STORE.md` 예정).

## 4. 망고보드 — 출시 전 한 번만 확인

망고보드는 **요금제와 용도에 따라 이용 범위가 다릅니다.** 특히 갈리는 지점:

- 모바일 **앱 스토어 배포**가 허용 범위에 들어가는가
- 결과물을 **재판매·재배포**하는 것으로 보는가 (게임 내 아이콘은 보통 아님)

개발·테스트 단계에서는 문제가 없습니다. **출시 직전에 현재 플랜이 앱 배포를 포함하는지 한 번만 확인**하고, 이 표의 "상업 이용" 칸을 ✅ 로 바꿔 주세요.

## 5. 아직 없는 에셋 (임시 대체 중)

| 필요한 것 | 현재 | 담당 |
|---|---|---|
| 빈 하트 스프라이트 | 가득 하트를 알파 30% 로 (`LivesView.emptyAlpha`) | 팀장(아트) |
| 튜토리얼 손가락 | `FreeButtonSet/radio_200` 원으로 대체 | 팀장(아트) |
| 자물쇠 아이콘 (레벨 선택 잠김) | `FreeButtonSet/x` 로 대체 | 팀장(아트) |
| 앱 로고 | TMP 글자 "NANA Arrow" | 팀장(아트) |
| 사운드 13종 (UI_FLOW §10) | 없음 = 무음 (정상 동작) | 팀장 |
| 앱 아이콘 | Unity 기본 | 팀장 |

## 6. 색은 어디서 정하나 (W-028)

코드에 색을 박지 않습니다. 아트를 교체할 때 아래 두 곳만 보면 됩니다.

| 대상 | 어디서 |
|---|---|
| 보드 (선·화살촉·레인 가이드·빈 칸 점·얼음·자물쇠) | `Settings/ArrowViewStyle.asset` |
| 코드가 런타임에 칠하는 UI (`#` 토글 켜짐/꺼짐) | `Settings/UITheme.asset` |
| 버튼 배경·글자 등 고정 UI | 씬·프리팹의 Image / TMP 인스펙터 값 |

> **주의**: `FreeButtonSet` 처럼 **이미 색이 칠해진 스프라이트**에 Image 의 Color 를 곱하면 색이 섞여 탁해집니다. 그림이 있는 스프라이트는 Color 를 **흰색**으로 두세요 (`docs/HANDOFF_PROGRAMMER.md` §7).
> 현재 씬의 UI 는 `FreeButtonSet` 의 **금색**이라 GAME_RULES §9 의 남색+연보라와 다릅니다 — 정식 아트 교체 시 함께 맞추면 됩니다.

## 7. 절대 커밋하지 않는 것 (CLAUDE.md)

`google-services.json` / `GoogleService-Info.plist` / `google-services.xml` / 실제 AdMob 앱·광고 단위 ID / `*.keystore`, `*.jks`, `ProjectSettings/keystore.local.json`

현재 광고는 **Google 테스트 광고 단위 ID** 로만 동작합니다 (`Services/AdMobService`).
