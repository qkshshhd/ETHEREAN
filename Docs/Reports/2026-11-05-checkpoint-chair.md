# 의자 체크포인트

2026-11-05 사용자 결정: 체크포인트는 의자다. 접근은 상호작용 가능 범위 확인에만 사용한다. 의자 가까이에서 상호작용을 새로 눌러 착석하면 체력이 회복되고 일반 적 및 몬스터 드롭이 초기화되며, 현재 진행과 아이템을 JSON에 기록한다. 키를 누른 채 다가가거나 기본 시작 위치에 나타나는 것만으로는 저장되지 않는다.

- 조작: 기존 `Player/Interact`의 키보드 E, 게임패드 상단 페이스 버튼. 의자에서는 새 누름을 사용하므로 길게 누를 필요가 없다. 앉은 상태에서 다시 누르면 일어난다.
- 착석 중 이동·점프·공격·회복 행동을 막는다. 피격·사망 시 착석을 해제한다. 복귀 시에는 마지막 저장 지점의 `SpawnPoint`에서 서 있는 상태로 시작한다.
- 사망 후 같은 실행 안에서는 진행·아이템이 유지된다. 게임을 종료하고 다시 시작하면 마지막 착석 저장 시점의 진행·아이템을 복원한다. 접근, 아이템 변경, 사망, 종료에 따른 자동 파일 쓰기는 제거했다. 저장 실패 시 이전 복귀 지점을 유지하고 콘솔에 오류를 남긴다.
- 의자 프리팹의 `Visual`에 아트를 넣고, `SeatPoint`에 앉을 때의 플레이어 루트 위치를 맞춘다. `SpawnPoint`는 일어서거나 복귀할 때의 위치다. 두 위치는 처음에는 같다. 의자 스프라이트와 앉기 모션은 아직 없으며, 현재는 위치 정렬과 조작 잠금으로 착석을 구현한다.

| 변경 파일 | 기능 |
| --- | --- |
| Assets/_Project/Scripts/Runtime/World/Checkpoint.cs | 자동 활성화 콜백 제거. 물리 콜라이더 겹침으로 범위 판정, 착석 기준점 제공. |
| Assets/_Project/Scripts/Runtime/Persistence/CheckpointSession.cs | 입력 한 번에 가장 가까운 의자 선택. 착석과 일어서기, 착석 순간에만 저장. |
| Assets/_Project/Scripts/Runtime/Persistence/PlayerSaveBinding.cs | 진행 복원과 능력 동기화 유지. 자동 파일 쓰기 요청 제거. |
| Assets/_Project/Scripts/Runtime/Player/Movement/PlayerLocomotion.cs | 착석 상태와 이동 잠금, 기존 전투·회복 입력 처리에 조작 비활성 상태 전달. |
| Assets/_Project/Scripts/Editor/CheckpointBuilder.cs | 생성 및 기존 프리팹 보강에 의자 기준점과 상호작용 액션 연결 추가. |
| Assets/_Project/Prefabs/World/Checkpoint.prefab | `SeatPoint` 자식 및 참조 추가. |
| Assets/_Project/Prefabs/World/CheckpointSession.prefab | 기존 상호작용 액션 참조 연결. |
| Assets/_Project/Data/Player/PlayerInteractAction.asset 및 .meta (추가) | 기존 입력 액션 에셋의 `Player/Interact`를 가리키는 재사용 참조. |

기존 `PlayerCheckpointTest` 씬은 프리팹 연결을 그대로 상속한다. 이번 의자 버전은 Unity Editor 17개와 두 차례 Play(41개, 20개), 총 78개 검증을 통과했다. Input System의 E 입력 이벤트를 사용해 범위 밖 입력 무시, 키를 누른 채 접근했을 때 미저장, 새 입력 시 착석과 저장, 이동·점프 잠금, 재입력으로 기립 및 미저장을 확인했다. 체력 회복, 적 초기화, 사망 시 아이템과 진행 유지, 재접속 시 저장된 진행·아이템·위치 복원도 확인했다. 게임패드 실기기 테스트는 수행하지 않았다.

[Unity 검증 로그](2026-11-05-checkpoint-chair-validation.txt)를 보관했다. 테스트 시작 씬 설정을 복구하고 복제 씬, 격리 저장 슬롯, 임시 검증 스크립트와 마커를 정리했다. 정리 후 Runtime/Editor 독립 컴파일 모두 통과했다.
