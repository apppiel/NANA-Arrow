# BUILD.md — 개발 빌드 → 폰 설치 (팀장용)

1. 폰에서 **설정 → 휴대전화 정보 → 빌드번호 7번 탭** → 개발자 옵션의 **USB 디버깅** 켜고 USB 로 연결 (파일 전송 모드)
2. Unity 메뉴 **NanaArrow → Build → 개발 빌드 APK** (단축키 `Cmd+Shift+B`) → 끝나면 `Builds/` 폴더가 열린다
3. APK 를 폰으로 옮겨 설치하거나, 터미널에서 `adb install -r Builds/<파일이름>.apk`
4. 첫 설치 때 "출처를 알 수 없는 앱" 허용을 물으면 허용
5. 확인할 항목은 `docs/QA_DEVICE.md`

> 릴리스 빌드(스토어 업로드용)는 서명 키가 필요하다. `ProjectSettings/keystore.local.json` 에
> `{"keystorePath": "...", "keystorePass": "...", "keyaliasName": "...", "keyaliasPass": "..."}`
> 를 만들면 **NanaArrow → Build → 릴리스 빌드 APK** 가 쓴다. 이 파일과 `.keystore` 는 git 에 올라가지 않는다 — 분실하면 스토어 업데이트가 불가능하니 따로 백업할 것.
