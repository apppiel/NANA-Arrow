# NANA-Arrow — CLAUDE.md (클라이언트 프로그래머: 클로드 코드)
> 팀 구성은 docs/TEAM.md. 지시는 팀장(사용자)에게서 오고, 규칙은 디렉터가 확정한 docs/GAME_RULES.md 를 따른다.

## v1 에 없는 것 (구현 금지)
Undo, 힌트, Shuffle 등 부스터 전부 / 코인 / 별점 / 인앱결제 / Bomb. GAME_RULES 가 기준.

## 프로젝트
- 장르: Arrow Puzzle (그리드 위 화살표 탭 → 가리키는 방향으로 발사, 앞이 막히면 실패 피드백, 전부 제거하면 클리어)
- Unity 6000.3.20f1 / Universal 2D (URP 2D Renderer) / Android IL2CPP, 세로(Portrait) 고정
- 패키지: Input System 1.19, Test Framework 1.6, uGUI
- 형상관리: GitHub. 브랜치 `feat/<이슈번호>-<짧은설명>`, main 직접 커밋 금지

## 폴더 규칙 (Assets/_Project 아래만 사용)
- Scenes/ : Boot, Main, Game (빌드 순서 동일)
- Scripts/Core : 씬 전환, 저장, 서비스 로케이터
- Scripts/Gameplay : 그리드, 화살표, 경로 계산, 탭 처리, 목숨
- Scripts/UI : 메뉴, HUD, 팝업
- Scripts/Data : 레벨 데이터 클래스, 로더
- Scripts/Services : 외부 SDK 연동(AdMob·Firebase·애널리틱스·캡처 방지). Core 는 Services 를 참조하지 않는다 — 인터페이스는 Core, 구현은 Services
- Scripts/Editor : 레벨 검증기, 생성기, 치트 메뉴
- Levels/ : 레벨 JSON (docs/LEVEL_FORMAT.md 준수)
- Tests/EditMode : 로직 유닛 테스트

## 내 역할 (클라이언트 프로그래머)
리포지토리 안의 파일을 만들고 고치는 일 전부.
1. .gitignore(Unity용) / 어셈블리 정의(asmdef) / 초기 커밋
2. Core: 씬 전환, 저장 시스템(JSON, Application.persistentDataPath)
3. Gameplay: 그리드 좌표계, 화살표 경로 계산, 막힘 판정, 목숨·Marked — **MonoBehaviour 없는 순수 C# 클래스로 먼저**, 연출은 분리
4. Data: 레벨 JSON 로더, docs/LEVEL_FORMAT.md 기준
5. Editor: 레벨 검증기(해결 가능 여부, 최소 탭 수), 역방향 자동 생성기, 레벨 점프 치트
6. 광고/애널리틱스 SDK 연동 코드
7. 모든 로직에 EditMode 테스트 작성 후 `unity` MCP 또는 CLI로 실행

## 절대 복사 금지 (이전 프로젝트에서 이식할 때)
google-services.json, GoogleService-Info.plist, GoogleMobileAdsSettings.asset 의 앱 ID, FirebaseApp.androidlib/res/values/google-services.xml, 실제 광고 단위 ID. 전부 NANA-Arrow 용 신규 발급 (팀장). 이식 코드는 테스트 ID·빈 값으로 둔다.

## 하지 않는 일
- 씬(.unity)·프리팹(.prefab) 직접 수정 → 컴포넌트 연결은 사용자가 에디터에서 함. 필요하면 "어떤 오브젝트에 무엇을 붙여야 하는지" 텍스트로 안내
- 기획 문서·레벨 디자인·스토어 문구 → docs/ 는 읽기만, 작성은 기획자/PM(클로드 데스크탑) 담당
- 대표님 보고용 요약

## 코딩 규칙
- 네임스페이스 `NanaArrow.<폴더명>`, 파일당 클래스 하나
- 필드는 `[SerializeField] private`, public 필드 금지
- Update() 안에서 GetComponent/Find 금지, 문자열 비교 대신 enum
- **모든 수치·설정 값은 인스펙터에서 수정 가능해야 한다 (최우선 규칙)**
  - 게임 밸런스 값(그리드 크기, 발사 속도, 애니메이션 시간, 광고 주기 등)은 `Assets/_Project/Settings/` 의 ScriptableObject(`GameConfig`, `LevelConfig`, `AdsConfig` 등)에 `[SerializeField]` 로 두고 코드는 그걸 참조만 한다
  - 컴포넌트별 값(이동 시간, 색상, 오프셋 등)은 해당 MonoBehaviour의 `[SerializeField]` 필드 + `[Tooltip]` + 기본값
  - 코드 안 매직 넘버 금지. 임시로 넣더라도 반드시 `[SerializeField]` 로 빼고 PR에 표시
  - `[Range]`, `[Min]`, `[Header]` 로 인스펙터에서 안전하게 조정 가능하게
- 커밋 메시지: `feat:`, `fix:`, `test:`, `chore:` 접두어

## git 규칙 (docs/TEAM.md 커밋 담당)
- 시작: `git checkout main && git pull`. 끝: PR 머지 후 main 으로 복귀
- **docs/ 는 add 하지 않는다** (REPORT_PROGRAMMER.md, HANDOFF_PROGRAMMER.md 만 예외). 문서·레벨·에셋 커밋은 디렉터 담당. "chore: sync docs" 류 커밋 금지
- 스택 PR 금지. main 기준 브랜치 하나씩

## 작업 흐름
0. **항상 docs/WORK.md 를 먼저 읽고 "프로그래머" 항목만 수행한다. WORK.md 는 절대 수정하지 않는다. 결과 보고는 docs/REPORT_PROGRAMMER.md 맨 위에 추가한다 (이 파일만 쓴다)**
1. GitHub 이슈 확인 → 브랜치 생성
2. docs/GAME_RULES.md, docs/LEVEL_FORMAT.md 먼저 읽기
3. 구현 → 테스트 → 컴파일 에러 0 확인 → PR 작성(변경 요약 + 사용자가 에디터에서 해야 할 일 목록)
