# 플레이어 HUD — 2026-11-05

> 최신 완료 기록: [TMP 전환 보고서](2026-11-05-player-hud-tmp.md). 아래 레거시 Text 구성 및 Unity 미검증 상태는 전환 전 기록이다. 현재는 TMP 전환과 Unity Editor8/Play13 검증을 완료했다.

상태: 코드·프리팹·플레이어 연결 파일 작성 완료. 독립 C# 컴파일과 직렬화 텍스트 검사 통과. **Unity에서의 프리팹 로드·Play Mode·화면 검증은 미완료**다. 배치 실행이 프로젝트 import 전에 WMI 초기화 assertion으로 종료했다. 실제 에디터에서 검증하기 전까지 동작 검증 완료로 간주하지 않는다.

## 이번 범위

현재/최대 체력, 현재/최대 회복 아이템 수량, 사용 중 회복 진행률을 표시한다. HUD를 독립 프리팹으로 만들고 Player 프리팹에 중첩 연결했다. 사용자는 프리팹에서 위치·폰트·색·표시 형식·사용자 이미지 등을 수정할 수 있다. 새로운 장식 그림·VFX·Gizmo는 만들지 않았다. 흰 기본 텍스트 세 줄은 기능 연결용이며 확정 HUD 디자인이 아니다. 기본 글꼴은 Unity 내장 글꼴로, 픽셀 전용 폰트나 한글 폰트가 아니다.

## 파일별 변경 위치와 책임

모든 경로의 기준 루트: `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/`.

| 구분 | 전체 경로 | 변경 위치 / 역할 |
|---|---|---|
| 추가 | `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/UI/PlayerHud.cs` | `OnEnable`: 부모 PlayerInventory를 통해 자신의 플레이어만 연결. `Bind`: 독립 배치 시 대상 교체. `LateUpdate/Refresh`: 체력·수량·회복 상태를 읽어 View에 전달. `OnDisable`: 표시 숨김. |
| 추가 | `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/UI/PlayerHudView.cs` | `Show`: Text 및 선택 Image.fillAmount 갱신, 회복 중에만 진행 상태 표시. `Hide`: Content 숨기고 표시 캐시 초기화. 직렬화 필드로 UI 참조·문자열 형식 노출. |
| 추가 | `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerHudBuilder.cs` | `Build`: HUD가 없을 때만 생성하고 Player에 없을 때만 중첩 연결. `CreatePrefab/Rect/Label`: Unity Editor API로 기본 텍스트 UI 생성. 기존 HUD 디자인은 재생성하지 않는다. |
| 추가 | `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/UI/PlayerHud.prefab` | Canvas, CanvasScaler, CanvasGroup, PlayerHud, PlayerHudView 및 Content의 HealthText/ItemCountText/HealingProgress/ProgressText. 이미지 슬롯은 None. |
| 수정 | `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab` | 루트 Transform의 자식 목록에 HUD 추가, 파일 끝에 중첩 PrefabInstance와 RectTransform 참조 추가. 기존 컴포넌트·수치·GUID·무기·애니메이션은 그대로. |
| 수정 | `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md` | 끝에 HUD 커스텀 정책과 검증 상태 기록. |
| 수정 | `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md` | 끝에 프리팹 위치와 연결·수정 방법, 미검증 상태 추가. |
| 추가 | `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-05-player-hud-validation.txt` | 정적 검사 근거와 Unity 실행 불가 구분. |
| 추가 | `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-05-player-hud.md` | 이 변경 보고서. |

새 스크립트 세 개와 HUD 프리팹의 `.meta`, 새 Runtime/UI 및 Prefabs/UI 폴더 `.meta`를 추가했다. 기존 meta 교체·파일 삭제·이름 변경은 없다. 저장된 씬·패키지·Input Actions·전투·회복 런타임 코드는 수정하지 않았다.

## 실행 흐름과 설계 이유

1. Player의 중첩 HUD가 활성화되면 부모의 PlayerInventory와 같은 오브젝트에 있는 CombatHealth/PlayerHealing을 연결한다. 전역 검색을 하지 않으므로 적의 체력을 가져오지 않는다.
2. PlayerHud가 LateUpdate에서 현재 체력·최대 체력·수량·용량·회복 여부·진행률을 읽는다. 실행 순서를 100으로 두어 기본 순서의 PlayerHealing.LateUpdate 완료 처리 이후 표시한다.
3. PlayerHudView가 숫자에 대응하는 Text를 갱신한다. 숫자가 같으면 문자열을 다시 만들지 않는다. 회복이 끝나거나 취소되면 진행 표시를 숨긴다. 사망 시 HUD는 체력 0을 표시한다.
4. HUD/플레이어를 비활성화하면 Content를 숨기고, 다시 활성화하면 현재 상태로 표시한다. 대상이 사라지면 Content를 숨긴다. 체력·아이템 소비·입력·속도에는 쓰지 않는다.

PlayerHud는 게임 데이터 연결, PlayerHudView는 화면 표시만 담당한다. 모델에 UI 참조나 이벤트를 추가하지 않았으며 회복량 적용과 아이템 소비의 기존 완료 순서에 개입하지 않는다. 작은 HUD이므로 매 프레임 몇 개의 값을 읽는 방식을 선택했다. 이벤트 등록·해제나 새 범용 인터페이스를 도입하지 않았고, UI 교체는 View의 직렬화 참조로 처리한다. Presenter는 현재 게임의 구체적인 모델 세 개에 의존하므로 모든 게임에서 재사용하는 범용 UI 계층은 아니다.

## Inspector에서 커스텀하기

`Assets/_Project/Prefabs/UI/PlayerHud.prefab`을 Prefab Mode로 연다.

- **Content / RectTransform**: 좌상단 여백·전체 위치. 현재 (12,-12), 크기 (220,64)는 임시 배치다. 하위 텍스트의 RectTransform으로 간격을 바꾼다.
- **HealthText / ItemCountText / ProgressText**: Font, Font Size, Color, Alignment. 픽셀 폰트나 한글을 쓰려면 사용자가 준비한 폰트를 연결한다.
- **PlayerHudView**: Health Format은 `HP {0} / {1}`, Item Count Format은 `Heal {0} / {1}`, Healing Progress Format은 `Healing {0}%`. 중괄호와 숫자 자리표시자는 C# string.Format 규칙을 유지한다.
- **Health Fill / Healing Progress Fill**: 원하는 UI Image를 추가해 연결한다. Image의 Source Image에 사용자 스프라이트를 넣고 Type=Filled, Fill Method=Horizontal, Fill Origin=Left로 설정하면 비율에 따라 표시된다. 장식 이미지와 프레임은 별도 자유 구성한다. UI Image/텍스트의 Raycast Target은 끄는 것을 권장한다.
- **Healing Progress Root**: 회복 중에만 보일 하위 오브젝트. Content에는 HUD 루트가 아니라 표시용 자식을 연결해야 한다.
- **Canvas / CanvasScaler**: Overlay, 기준 640×360, Match=.5, Pixel Perfect. 실제 픽셀 글꼴 및 정수 배율 보장은 별개이며 다양한 해상도 화면 확인이 필요하다.
- **CanvasGroup**: 알파로 전체 표시 강도 조절. Interactable/Blocks Raycasts는 false이고 GraphicRaycaster/EventSystem은 필요하지 않다.

플레이어 자식인 현재 구성에서는 PlayerHud의 대상 필드가 None이어도 자기 부모를 자동 연결한다. HUD를 별도로 씬에 둘 경우 PlayerHud의 Health/Inventory/Healing 필드에 같은 플레이어의 컴포넌트를 연결하거나 생성 코드에서 `Bind(health, inventory, healing)`를 호출한다. 이때 플레이어 안의 중첩 HUD는 비활성화해 중복 표시를 피한다. 화면을 끌 때에는 PlayerHud 컴포넌트 또는 프리팹 오브젝트를 비활성화한다.

메뉴 `ETHEREAN > UI > Create and Connect Player HUD`는 Unity에서 누락된 프리팹/플레이어 연결을 만들기 위한 도구다. 이미 있는 HUD 외형과 연결은 덮어쓰지 않는다. 현재 프리팹은 Unity 실행 불가로 직렬화 파일을 직접 작성했으므로 **최초 에디터 로드 확인이 반드시 필요**하다.

## 검증 결과

- Runtime 47개 / Editor 15개 소스를 각각 독립 컴파일: 오류 0. Inspector 할당 필드의 기존 CS0649 경고는 남는다.
- 정적 텍스트·참조 검사 19항목 통과: HUD 23개 오브젝트의 고유 ID/내부 참조, Overlay/스케일/입력 비차단, 사용자 이미지 미지정, 부모 중첩 연결/GUID, 모델 쓰기 없음 등을 검사했다. Unity 역직렬화 검사나 런타임 테스트는 아니다.
- 이전 Assets/Packages/ProjectSettings 전체 해시와 비교: 기존 파일 변경은 Player.prefab 한 개뿐이고, HUD 추가 구간을 제외하면 이전 바이트와 동일하다. 누락 파일 없음.
- Unity 배치 실행 2회 시도. 프로젝트 import 전 `SUCCEEDED(hr)`, `wmiOpened`, `ERROR_SUCCESS == status` assertion으로 종료. 실제 프리팹 로드/Play Mode/해상도 렌더링/게임 입력 확인은 하지 못했다. 임시 테스트 스크립트나 Gizmo를 추가하지 않았다.

## Unity에서 확인할 절차 — 미실행

1. 컴파일을 기다리고 PlayerHud.prefab을 연다. Missing Script/폰트/참조 오류가 없는지 확인한다. Player.prefab에는 PlayerHud가 한 개 있어야 한다.
2. PlayerHealingTest 씬을 열고 Play한다. HP는 현재/최대 값, 회복 수량은 시작 0/3으로 보여야 한다. Progress는 숨김 상태여야 한다.
3. 필드 아이템을 먹으면 수량이 증가하고, 최대 수량에서는 추가 획득되지 않는지 확인한다. 적에게 맞으면 HP가 감소해야 한다.
4. 부상·지상·아이템 보유 상태에서 R로 회복한다. 진행률이 올라가고 완료 시 HP 증가와 수량 1 감소가 함께 표시되어야 한다. 회복 도중 유효 피격이면 진행 표시만 사라지고 수량은 유지되어야 한다.
5. 체력 0, HUD 비활성화/재활성화, 플레이어 비활성화/재활성화, 별도 HUD의 Bind 교체 시 오래된 숫자가 남지 않는지 확인한다.
6. 640×360, 1280×720, 1920×1080 및 다른 화면비에서 배치·글꼴 가독성을 확인한다. 사용자 Image를 Filled로 연결해 체력/회복 비율을 확인한다.
7. HUD 프리팹의 배치나 폰트를 바꾸고 메뉴를 다시 실행해 커스텀 내용이 유지되는지 확인한다.

현재 단위의 사용자 검토 및 실제 Unity 검증 대기. 저장·리스폰·새 HUD 디자인·다음 기능·커밋·푸시는 진행하지 않았다.
