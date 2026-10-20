# ETHEREAN 폴더 구조

직접 제작하는 게임 콘텐츠는 `Assets/_Project`에 저장합니다.
Unity 템플릿의 기존 `Assets/Scenes`, `Assets/Settings`와 입력 에셋은 현재 위치를 유지합니다.
외부 에셋은 제작자가 제공한 폴더를 유지하여 업데이트 시 프로젝트 콘텐츠와 섞이지 않게 합니다.

| 경로 (`Assets/_Project/` 기준) | 용도 |
| --- | --- |
| `Art/Sprites`, `Animations`, `Materials`, `Shaders` | 스프라이트, 애니메이션, 머티리얼, 셰이더 |
| `Audio/Music`, `SFX` | 배경 음악과 효과음 |
| `Prefabs/Player`, `Enemies`, `World`, `UI` | 역할별 프리팹 |
| `Scenes/Gameplay`, `Tests` | 실제 게임 씬과 수동 테스트 씬 |
| `Scripts/Runtime/Core` | 기능 간 공통 계약과 최소한의 공통 기반 |
| `Scripts/Runtime/Player` | 플레이어 입력과 이동 |
| `Scripts/Runtime/Combat` | 공격, 피해, 체력 등 전투 규칙 |
| `Scripts/Runtime/Enemies` | 적 행동과 AI |
| `Scripts/Runtime/World` | 지역 연결, 문, 체크포인트 등 월드 동작 |
| `Scripts/Runtime/Progression` | 능력 해금과 탐험 진행 상태 |
| `Scripts/Runtime/Save` | 진행 상태의 저장과 복원 |
| `Scripts/Runtime/UI`, `Audio` | 화면 표시와 오디오 재생 |
| `Scripts/Editor` | Unity Editor 전용 도구 |
| `Data/Player`, `Combat`, `Enemies`, `World`, `Progression` | 해당 기능의 ScriptableObject 설정 에셋 |

## 사용 원칙

- 스크립트는 폴더가 아닌 실제 책임을 기준으로 설계합니다. Core에 모든 기능을 모으지 않습니다.
- 데이터 에셋과 실행 로직을 분리하고, 저장할 진행 상태는 설정 에셋과 별도로 관리합니다.
- 내부 상태를 캡슐화하고, 교체할 동작에 필요한 인터페이스나 추상화를 도입합니다. 상속은 공통 계약과 동작이 실제로 필요할 때 사용합니다.
- 기본 네임스페이스는 `Etherean`으로 하고 기능에 따라 `Etherean.Player`처럼 구분합니다.
- Editor 전용 코드는 `Scripts/Editor`에 둡니다.
- 빈 폴더의 `.gitkeep`은 Git에 폴더를 보존하기 위한 파일입니다. 실제 파일이 생기면 제거해도 됩니다.
- `.meta`는 에셋과 함께 커밋합니다. 에셋 이동은 Unity Project 창에서 수행하여 참조를 유지합니다.

현재는 폴더 구조만 준비했습니다. 게임플레이 코드, 어셈블리 정의, 저장 방식은 첫 기능 구현 시 의존성을 확인하며 추가합니다.
