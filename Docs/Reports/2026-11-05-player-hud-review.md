# HUD 재검토 — 2026-11-05

사용자가 2D / Unity 6.4 프로젝트임을 재확인했다. ProjectVersion.txt 실제 버전은 6000.4.7f1이다.

## 확인한 문제

1. **UI 컴포넌트 선택을 충분히 검토하지 않았다.** PlayerHudView의 healthText/itemCountText/healingProgressText와 PlayerHudBuilder.Label은 UnityEngine.UI.Text를 사용하고 LegacyRuntime.ttf를 연결한다. 설치된 uGUI 패키지 소스에서 Text는 `UI (Canvas)/Legacy/Text`로 분류되고 TextMeshProUGUI도 제공된다. 새 HUD에 레거시 기본 글꼴을 선택한 이유나 픽셀 글꼴·한글 확장성을 충분히 검토하지 않았다. Unity6.4에서 Text가 작동하지 않는다는 뜻은 아니다. 이후 Canvas 텍스트를 선택할 때 TextMeshProUGUI와 적합한 폰트 자산을 우선 검토한다.
2. **픽셀 HUD의 실제 렌더링을 확인하지 않았다.** Canvas.pixelPerfect=true, 기준 640×360, ScaleWithScreenSize/Match=.5 설정만으로 정수 배율·픽셀 글꼴 선명도를 보장할 수 없다. 동적 기본 글꼴도 픽셀 전용 폰트가 아니다. 글꼴·아틀라스·필터·스케일·목표 해상도에서 화면을 확인해야 한다.
3. **Unity 검증 전에 수동 직렬화 프리팹을 플레이어에 연결했다.** 배치 실행이 실패했을 때 생성 도구 코드와 수동 YAML 파일을 작성했지만, 실제 Unity의 로드·화면·Play 동작 검증 없이 연결한 것은 임시 상태다. 앞선 보고서에 미검증이라고 표시했어도 실제 동작이 보장되는 결과물은 아니다. 컴파일·참조 검사와 엔진 검증을 계속 구분한다.

## 2D / 버전과 관련해 정확히 구분할 사항

- Screen Space Overlay Canvas와 RectTransform은 2D 게임 HUD에도 사용하는 구성이다. 이번 파일에 3D TextMeshPro 메시나 World Space Canvas를 사용한 사실은 없다.
- Canvas의 m_PlaneDistance나 Vector3 필드가 있다는 이유로 3D용 HUD라고 판단할 수 없다.
- Sprite의 32PPU와 CanvasScaler의 Reference Pixels Per Unit=100은 역할이 다르다. 숫자가 다르다는 이유만으로 오류라고 단정하거나 32로 바꾸지 않는다. 특히 기본 UI Text의 글꼴 픽셀 크기를 직접 정하는 값이 아니다.
- Legacy Text는 Unity6.4에서 이용 가능한 컴포넌트다. 이번 선택의 문제는 버전 호환 불가가 아니라 새 UI의 글꼴·커스텀 요구와 프로젝트 환경에 대한 검토 부족이다.

## 이번 검토의 변경 파일

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md`: 끝에 프로젝트 버전/2D UI 구분/픽셀 검증/임시 프리팹 취급 규칙 추가.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md`: 같은 재발 방지 규칙과 검증 한계 기록.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-05-player-hud-review.md`: 이 검토 보고서 추가.

이번 검토에서 HUD 코드·프리팹·게임플레이 설정은 변경하지 않았다. TextMeshPro 전환이나 새 폰트 적용은 수행하지 않았다. 기존 19항목은 정적 검사 기록이며 Play 검증 결과가 아니다. 현재 Editor.log에 PlayerHud.prefab import 기록이 확인되지만, 그것만으로 UI 외형과 체력/회복 연동 성공을 확인할 수는 없다. 실제 화면·Play 테스트는 여전히 미검증이다.
