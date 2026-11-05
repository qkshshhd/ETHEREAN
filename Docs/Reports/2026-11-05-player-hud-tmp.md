# 플레이어 HUD TMP 전환 — 2026-11-05

Unity 6.4(6000.4.7f1)에서 HUD의 레거시 Text 세 개를 **TextMeshProUGUI**로 전환했다. 런타임 참조 타입은 **TMP_Text**다. Unity Editor API로 기존 프리팹의 컴포넌트를 교체하고 참조를 다시 연결한 뒤 저장했다. 기존 HUD 프리팹 GUID, GameObject, RectTransform 배치, 표시 형식, Player의 중첩 HUD 연결을 유지했다. 이전 보고서의 레거시 Text 및 TMP 전환 대기 설명은 이 보고서로 대체한다.

## 수정한 파일과 위치

| 전체 경로 | 변경 위치 / 역할 |
|---|---|
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/UI/PlayerHudView.cs` | `healthText`, `itemCountText`, `healingProgressText`: Text → TMP_Text. `using TMPro` 추가. Show/Hide의 체력·수량·진행률 표시 흐름은 유지. Image 참조는 그대로 사용. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerHudBuilder.cs` | `DefaultFontPath` 추가. `Label`의 반환 타입은 TMP_Text, 생성 컴포넌트는 TextMeshProUGUI. TMP 폰트 연결, 크기 14, 좌상단 정렬, 줄바꿈/리치텍스트/레이캐스트 비활성화. 폰트가 없으면 명시적 오류로 중단하며 레거시 글꼴로 대체하지 않음. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/UI/PlayerHud.prefab` | Content/HealthText, Content/ItemCountText, Content/HealingProgress/ProgressText의 Text 컴포넌트를 TextMeshProUGUI로 교체. PlayerHudView의 세 필드 재연결. TMP 폰트와 공유 머티리얼 연결. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md` | 끝에 TMP 전환 완료와 실제 검증 결과 기록. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md` | 끝에 TMP 전환 완료 및 폰트 커스텀 방법 기록. |

기존 Player.prefab은 이번 전환에서 수정하지 않았다. HUD 프리팹의 기존 GUID를 그대로 사용하므로 중첩 연결을 통해 새 TMP 컴포넌트를 사용한다. 텍스트 컴포넌트의 로컬 ID는 교체 과정에서 새로 생겼고, View 참조도 그 ID로 갱신했다. 기존 이미지 슬롯·게임플레이·회복 처리·이동 수치는 변경하지 않았다.

## 추가한 리소스와 문서

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/TextMesh Pro/`: **이미 설치된 com.unity.ugui 패키지에 포함된 TMP Essential Resources** 추가. TMP Settings, LiberationSans SDF 폰트/아틀라스/머티리얼, 셰이더, 스타일·줄바꿈 데이터와 폰트 라이선스 등 기본 의존성이다. Examples & Extras는 프로젝트에 추가하지 않았다. 패키지 버전 변경도 없다.
- 전체 추가 경로 목록: `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-05-player-hud-tmp-resources.txt`. 각 경로에 대응하는 `.meta`도 포함한다. Unity 기본 리소스의 GUID를 유지했다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-05-player-hud-tmp-validation.txt`: 실제 Editor/Play 검사 기록.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-05-player-hud-tmp.md`: 이 보고서.

영구 런타임 클래스나 별도 게임플레이 기능은 추가하지 않았다. 전환·검증용 임시 Editor/Runtime 스크립트 두 개와 해당 meta, 요청/준비 마커, 복사 테스트 씬은 실행 후 제거했다.

## 동작과 설계

PlayerHud가 기존 모델의 체력·아이템·회복 상태를 읽고 PlayerHudView.Show를 호출한다. View의 TMP_Text.text에 문자열을 넣으면 TextMeshProUGUI가 Canvas에서 글자를 표시한다. TMP_Text로 참조하여 표시 코드는 TMP의 구체적인 생성 방식에 의존하지 않지만, 실제 2D HUD 컴포넌트는 Canvas용 TextMeshProUGUI를 사용한다. World Space TextMeshPro 메시 컴포넌트는 사용하지 않는다.

생성 도구는 앞으로도 TMP로만 새 텍스트를 만든다. 기존 프리팹이 있으면 외형을 다시 만들지 않는 정책을 유지한다. 전환용 일회성 레거시 처리 코드는 영구 생성 도구에 남기지 않았다.

## Inspector 커스텀

PlayerHud.prefab을 Prefab Mode로 연 뒤 HealthText, ItemCountText, ProgressText의 **TextMeshPro - Text (UI)** 컴포넌트에서 Font Asset, Font Size, Vertex Color, Alignment를 수정한다. RectTransform과 PlayerHudView의 표시 형식·이미지 슬롯은 기존과 같다. 변경을 저장하면 Player의 중첩 HUD에도 반영된다.

연결한 LiberationSans SDF는 기본 동작 확인용이다. 픽셀 전용/한글 폰트가 아니며 최종 아트 승인을 의미하지 않는다. 사용자 픽셀 폰트의 TMP Font Asset을 나중에 연결할 수 있다. SDF 설정이나 Canvas.pixelPerfect만으로 모든 해상도에서 픽셀 단위 선명도를 보장한다고 주장하지 않는다.

## 검증

- 실제 Unity Editor **8항목 통과**: 세 텍스트의 TMP 폰트/머티리얼, 레거시 Text 제거, TMP 세 개, 생성 메뉴 재실행 시 HUD 파일 보존, Player의 단일 중첩 HUD 및 TMP 로드.
- 실제 Play Mode **13항목 통과**: 실제 모델 초기 체력/수량, ApplyDamage 반영, TryAdd 반영, Heal/TryConsumeOne 반영, 통제된 Show 호출의 42% 진행 표시, 세 텍스트의 TMP 글리프 메시 생성, 비회복 진행 숨김, HUD 비활성화/재활성화, 사망 체력 0 표시.
- 진행률 검사는 View에 직접 값을 전달한 표시 검사다. 회복 입력 전체를 재검증한 테스트나 실제 사용자 픽셀 폰트의 화면 검사는 아니다. 스크린샷을 통한 최종 외형 확인도 하지 않았다.
- 테스트용 시작 씬 설정 복원 및 복사 씬 제거 확인: RESTORED.
- 임시 파일 정리 후 영구 Runtime 47개 / Editor 15개 각각 독립 컴파일 오류 0.
- 이전 Assets/Packages/Settings 전체 해시 비교: 기존 파일 중 변경은 HUD prefab, PlayerHudView.cs, PlayerHudBuilder.cs 세 개뿐이다. 누락 파일 없음. 기존 여섯 RectTransform의 위치·회전·스케일·앵커·피벗·크기 설정이 모두 같음을 별도 검사했다.

## 사용자 확인 절차

1. PlayerHud.prefab의 세 텍스트가 TMP인지, PlayerHudView의 세 참조와 Font Asset/Material이 연결됐는지 확인한다.
2. PlayerHealingTest에서 피격·아이템 획득·R 회복 시 체력/수량/진행 표시를 확인한다.
3. 폰트와 배치를 커스텀하고 프리팹을 저장한다. `ETHEREAN > UI > Create and Connect Player HUD`를 다시 실행해도 설정이 유지돼야 한다.
4. 사용하려는 해상도에서 글꼴 가독성과 배치를 검토한다.

현재 단위 완료 후 사용자 외형·코드 검토 대기. 다음 기능, 커밋, 푸시는 진행하지 않았다.
