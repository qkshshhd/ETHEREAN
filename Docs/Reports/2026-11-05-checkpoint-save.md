# 체크포인트·JSON 저장·사망 복귀·적 초기화

> 최신 변경: [의자 체크포인트](2026-11-05-checkpoint-chair.md). 아래의 접근 활성화 및 자동 저장 정책은 이전 구현 기록이다. 이전 버전은 Unity Editor 16개, Play 52개 검증을 통과했고 프리팹과 테스트 씬이 생성됐다. 의자 버전의 검증 상태는 새 보고서를 참고한다.

작성일: 2026-11-05. 현재 상태: 런타임/Editor 코드 작성 및 독립 컴파일 통과. **Unity가 새 검증 스크립트를 가져오지 않아 프리팹 생성과 실제 실행 검증 대기 중**이다. 아래 구현 설명을 실제 검증 완료로 읽지 않는다. 최종 검사 이후 상태를 갱신한다.

## 승인된 규칙과 이번 선택

- 로컬 JSON 파일로 진행, 회복 아이템 수량·총 소비량, 마지막 체크포인트를 보존한다. 현재 플레이어 좌표·속도·HP·적 AI/현재 위치는 저장하지 않는다.
- 새 접속과 사망 복귀 시 현재 진행/아이템은 유지하고 마지막 체크포인트의 SpawnPoint에서 체력 완전 회복 상태로 시작한다. 체크포인트 사용은 회복 아이템을 자동 보충하지 않는다.
- 사용자 최신 표현인 '밟으면'을 접촉 활성화로 구현했다. 이는 마스터 문서의 과거 상호작용 활성화 정책을 이번 단위에서 대체한다. 복합 콜라이더는 한 번의 진입으로 처리한다. 나갔다가 다시 들어오면 다시 쉴 수 있다.
- 체크포인트를 사용하거나 사망 복귀하면 해당 씬의 활성 EnemySpawnPoint가 관리하는 일반 적을 초기화하고 몬스터 드롭을 정리한다. 영구 진행과 고정 필드 아이템 획득 기록은 초기화하지 않는다.
- 사망 지연 1초, 부활/휴식 후 피해 보호 1초는 Inspector 임시값이다. 위치 자체가 안전해야 하며 보호 시간으로 잘못된 배치를 대신하지 않는다.
- 체크포인트 그림·효과·Gizmo를 생성하지 않는다. 프리팹에는 트리거와 SpawnPoint, 빈 Visual 자식만 둔다.

## 조사 근거와 적 배치 방식

공개된 Corgi Engine의 Respawn 문서는 체크포인트에 재생성 대상을 연결하고, 일반 적에 별도 AutoRespawn 컴포넌트를 두는 구조를 설명한다. Health API에는 StoreInitialPosition도 노출된다. Unity 2D Game Kit 공식 가이드 역시 체크포인트와 데이터 영속성을 별도 개념으로 다룬다.

- https://corgi-engine-docs.moremountains.com/respawn.html
- https://corgi-engine-docs.moremountains.com/API/class_more_mountains_1_1_corgi_engine_1_1_health.html
- https://learn.unity.com/tutorial/2d-game-kit-reference-guide-jp?language=ja
- https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/application/persistentdatapath

이는 공개 플랫폼/2D 게임 프레임워크의 근거이며, Hollow Knight나 Ori의 비공개 내부 구현을 확인했다는 뜻은 아니다. 아래 설계는 그 책임 분리 방식과 현재 프로젝트 구조를 바탕으로 선택했다. Corgi 패키지를 설치하거나 Unity 버전을 올리지 않는다.

처음 생성된 위치를 Awake에서 한 번 캡처하는 방식도 고정 배치 적에는 사용할 수 있다. 하지만 OnEnable마다 캡처하면 풀링/재활성화 후 이동한 위치가 초기 위치가 될 수 있다. ETHEREAN은 **씬에 남는 EnemySpawnPoint와 실제로 움직이는 적 인스턴스를 분리**한다.

1. 사용자는 EnemySpawnPoint.prefab을 씬에 놓고 Enemy Prefab에 오소리/멧돼지 프리팹을 연결한다.
2. 스폰 지점 Transform이 배치 기준이다. 실제 적은 그 아래 별도 자식으로 생성돼 혼자 이동한다.
3. 휴식/사망 시 기존 적을 먼저 비활성화해 공격·경고·이동을 중단하고 제거한다. 같은 프리팹을 스폰 지점에서 새로 생성한다.
4. 새 인스턴스의 Awake가 체력·홈 위치·배회·AI를 초기화한다. 쿨타임·슈퍼아머·경직·드롭 1회 처리 상태도 새 인스턴스의 기본값이 된다.

스폰 좌표는 씬에 이미 저장되므로 JSON에 적 좌표를 중복 저장하지 않는다. 씬에서 스폰 지점을 옮겨 저장하면 이후 게임 실행과 재생성에 반영된다. 기존 적 프리팹을 씬에 직접 놓은 오브젝트는 자동으로 스폰 지점 관리 대상이 되지 않는다. 향후 실제 숲 배치에는 스폰 지점 프리팹을 사용한다. 새 테스트 씬의 적 배치는 이 방식으로 구성하는 생성 도구를 작성했다.

새 인스턴스 생성 방식은 초기화 누락을 줄이고 기존 두 AI를 수정하지 않는 장점이 있다. 대신 체크포인트 사용 때 Instantiate/Destroy 비용과 객체 참조 교체가 있다. 현재 작은 씬 규모에서 선택한 방식이며, 큰 맵에서는 구역별 재생성/풀링을 검토할 수 있다. 성능 실측 수치는 없다. 다른 코드가 적 인스턴스를 영구 참조해야 한다면 Anchor.Instance를 다시 조회해야 한다.

## 저장 구조와 흐름

저장 위치는 `Application.persistentDataPath` 아래의 JSON 파일이다. Windows에서는 일반적으로 `%USERPROFILE%/AppData/LocalLow/<회사명>/<제품명>/`에 해당한다. 정확한 전체 경로는 실행 시 [Save] 로그와 CheckpointSession.SavePath로 확인한다. 플레이어 빌드는 `etherean-save.json`, 일반 에디터 사용은 `etherean-editor-save.json`으로 분리한다. 새 테스트 씬은 별도 `etherean-checkpoint-test.json`을 사용하며 검증 도구는 매번 고유한 테스트 슬롯을 만든다.

데이터 필드는 signature, version, checkpointId, scenePath, healingItems, totalConsumed, flags, pickups다. flags는 WeaponUnlocked, WallTraversalUnlocked, BoarIntroSeen, BoarEncounterCleared, DExitOpened, ShortcutCEOpened, ShortcutHBOpened, ForestExitReached를 지원한다. 문/컷신/숲 배치 자체를 이번에 구현한 것은 아니며, 이후 이벤트에서 `session.Progress.Set(ProgressFlag.이름)`으로 영구 사실을 기록한다. 무기/벽 이동의 기존 해금 메서드는 이벤트를 통해 자동 반영한다.

현재 위치 몇 개를 직렬화하는 것 자체가 큰 비용은 아니다. 반복적인 디스크 쓰기를 피하는 정책과 안전한 재접속 위치를 분리해서 설계했다. 실제 변화가 있을 때 Dirty 표시하고 LateUpdate에서 그 프레임의 최종 상태를 한 번 저장한다. 회복 완료의 체력/아이템 처리와 필드 획득의 아이템/잔량 처리가 끝난 뒤 스냅샷을 만든다. 체크포인트·사망 복귀·앱 일시정지·종료에서는 필요한 저장을 Flush한다. 변함없는 프레임마다 파일을 쓰지 않는다.

임시 파일을 기록하고 Flush한 뒤 File.Replace로 교체하며 이전 정상 파일은 .bak에 보관한다. 새 슬롯은 Move한다. 손상된 주 파일은 유효한 백업으로 복구한다. 둘 다 읽을 수 없거나 더 높은 버전의 주 파일이면 자동 덮어쓰기를 금지하고 경고한다. 저장 실패는 오류 로그에 남기고 5초 이후 재시도한다. 하드웨어 장애까지 저장 성공을 보장하거나 파일 내용을 암호화하지는 않는다.

현재 플레이어 좌표를 저장하지 않으므로 재실행 때 checkpointId와 scenePath를 찾아 체크포인트 자식 SpawnPoint 위치로 이동한다. 체크포인트 좌표를 에디터에서 조정해도 같은 ID로 참조한다. 저장된 ID가 없어지면 현재 씬의 기본 체크포인트를 사용하고 진행/아이템은 유지한다. 다른 씬의 체크포인트가 저장돼 있으면 Build Scene에 포함된 그 씬을 로드한다. 씬이 빌드에 없으면 오류를 표시하고 저장을 중단해 기존 진행을 덮어쓰지 않는다.

필드 회복 아이템에는 PersistentPickup의 고유 ID를 사용한다. 부분 획득 후 남은 수량까지 기록해 재실행으로 수량을 복제하지 않게 한다. 몬스터 드롭은 영구 기록하지 않고 휴식/사망 때 정리한다. 고정 필드 회복 아이템은 실제 맵에서 PersistentPickup을 연결하고 전역적으로 고유한 ID를 배정해야 한다. 기본 드롭 프리팹에 공통 ID를 넣으면 안 된다.

## 추가한 클래스와 책임

모든 경로는 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/` 아래다.

| 경로 | 주요 위치와 책임 |
|---|---|
| Assets/_Project/Scripts/Runtime/Persistence/SaveData.cs | SaveData/PickupSave: 버전 있는 JSON DTO. ProgressFlag: 영구 진행 키. |
| Assets/_Project/Scripts/Runtime/Persistence/JsonSaveStore.cs | Load/TryRead/Save/Valid: 파일 검증, 복구, 안전 교체. 게임 오브젝트를 조작하지 않음. |
| Assets/_Project/Scripts/Runtime/Persistence/GameProgress.cs | Has/Set, RecordPickup, Restore/Capture: 영구 사실과 필드 잔량 관리. 파일 경로/적 AI를 알지 않음. |
| Assets/_Project/Scripts/Runtime/Persistence/PlayerSaveBinding.cs | Initialize: 수량/능력 복원. Subscribe/RecordAbilities/ApplyProgress: 모델과 진행 이벤트 연결. |
| Assets/_Project/Scripts/Runtime/Persistence/PlayerRespawn.cs | ReturnAfterDeath/RestoreAt/Rest: 사망 지연, 플레이어 일시 상태 정리, 체력/피해 보호 복구. |
| Assets/_Project/Scripts/Runtime/Persistence/CheckpointSession.cs | Awake/Start: 슬롯·진행 로드 및 씬/체크포인트 선택. RestAt/RespawnPlayer: 생명주기 조정. MarkDirty/Flush: 저장 시점 관리. |
| Assets/_Project/Scripts/Runtime/Persistence/CheckpointWorldReset.cs | Reset: 같은 씬의 일반 적 재생성 및 몬스터 드롭 정리. |
| Assets/_Project/Scripts/Runtime/Enemies/EnemySpawnPoint.cs | Respawn/Clear: 배치 기준 Transform과 런타임 적 분리. 기존 AI 클래스에 초기화 코드를 분산하지 않음. |
| Assets/_Project/Scripts/Runtime/World/Checkpoint.cs | OnTriggerEnter/Exit, Activate/ClearContacts: 접촉 중복 처리와 체크포인트 진입 요청. |
| Assets/_Project/Scripts/Runtime/World/PersistentPickup.cs | Remaining/Record: 필드 회복 아이템과 영구 잔량 기록 연결. |
| Assets/_Project/Scripts/Editor/CheckpointBuilder.cs | Build/CreateScene: 기존 커스텀 자산을 덮어쓰지 않고 필요한 프리팹·별도 테스트 씬 생성 및 Player 컴포넌트 연결. |

## 기존 코드 수정 위치

| 경로 | 변경 |
|---|---|
| Assets/_Project/Scripts/Runtime/Combat/CombatHealth.cs | RestoreFull 추가: 부활/휴식용 체력 복구. Heal의 사망자 회복 금지는 유지. |
| Assets/_Project/Scripts/Runtime/Combat/CombatDamageReceiver.cs | ResetForRespawn 추가: 부활 피해 보호의 만료 시간 설정. |
| Assets/_Project/Scripts/Runtime/Combat/CombatReaction.cs | ResetState 추가 및 OnDisable 공용화: 경직/넉백/슈퍼아머 억제 상태 초기화. |
| Assets/_Project/Scripts/Runtime/Player/Items/PlayerInventory.cs | RestoreState 추가: 데이터 상한에 맞춰 수량·소비량 복원, Changed 발행. |
| Assets/_Project/Scripts/Runtime/Player/Movement/PlayerMovement.cs | Teleport 추가: 위치와 속도 초기화도 기존 물리 쓰기 담당자가 처리. |
| Assets/_Project/Scripts/Runtime/Player/Movement/PlayerLocomotion.cs | ResetForRespawn 공개 진입점: 기존 내부 런타임 정리 재사용. |
| Assets/_Project/Scripts/Runtime/Player/Movement/PlayerTraversalAbilities.cs | Changed 이벤트, SetWallTraversalUnlocked 추가. 기존 Unlock 메서드는 위임. |
| Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttack.cs | WeaponUnlockChanged 이벤트. 해금 상태가 바뀔 때만 발행. |
| Assets/_Project/Scripts/Runtime/World/HealingPickup.cs | Start에서 필드 잔량 복원. 실제 획득 후 잔량 기록. 기존 접촉·상한·드롭 정리 유지. |

런타임 이동 속도/점프 수치, 공격 데이터, 적 AI 데이터, 애니메이션·HUD 외형·Input Actions는 변경하지 않는다. 실제 생성된 prefab/meta와 기존 파일 해시 결과는 검증 뒤 추가 기록한다.

## Unity에서 생성·배치하기

메뉴: `ETHEREAN > Tests > Create Checkpoint Save Test`.

생성 대상: Assets/_Project/Prefabs/World/Checkpoint.prefab, Assets/_Project/Prefabs/World/CheckpointSession.prefab, Assets/_Project/Prefabs/Enemies/EnemySpawnPoint.prefab, Assets/_Project/Scenes/Tests/PlayerCheckpointTest.unity. 기존 Player.prefab에는 PlayerSaveBinding과 PlayerRespawn만 추가한다. 이미 있는 자산/씬은 다시 만들지 않는다.

- 체크포인트 프리팹을 배치한 **씬 인스턴스**의 Checkpoint Id를 고유한 값으로 설정한다. 중복하거나 비워두면 세션이 오류를 표시한다. ID는 저장 후 임의 변경하지 않는다.
- SpawnPoint를 플레이어 루트가 서 있을 안전한 위치로 옮긴다. 현재 플레이어는 땅 위에서 루트 Y 약0.91이므로 기본값도 그에 맞췄다. 캐릭터/콜라이더를 바꾸면 다시 확인한다. 지형 속/절벽/적 공격 범위에 두지 않는다.
- Visual에 사용자 스프라이트/애니메이션/VFX를 붙인다. 코드가 외형을 그리지 않는다.
- EnemySpawnPoint의 Enemy Prefab에 기존 적 프리팹을 연결하고 스폰 지점을 원하는 위치에 놓는다. 움직이는 런타임 적 대신 배치용 부모를 편집한다.
- CheckpointSession 프리팹은 씬에 하나만 놓고 Player와 Default Checkpoint를 연결한다. 다른 씬으로 이어질 때 각 씬에 세션과 기본 체크포인트가 있어야 하고 같은 슬롯 이름을 사용한다. 씬들은 빌드 프로필에 포함해야 한다.
- 필드 회복 아이템에는 PersistentPickup 및 전역 고유 ID를 연결한다. 몬스터 드롭에는 붙이지 않는다.

## 준비한 검증

별도 테스트 슬롯과 복사 테스트 씬을 사용하는 임시 검증 도구를 작성했다. JSON 신규/교체/백업/손상/미지원 버전, 실제 트리거 획득/휴식, 부분 필드 획득, 적 사망 후 초기화, 보호 시간, 사망 복귀, 진행/아이템 보존, 변화 없는 프레임의 파일 쓰기 없음 등을 검사하도록 준비했다. Play를 종료했다가 다시 시작하는 두 번째 실행에서는 같은 파일을 새 서비스로 읽어 체크포인트·진행·아이템·필드 잔량을 확인한다.

**현재 실행 전**이므로 통과 개수를 기재하지 않는다. 새 검증 스크립트가 Unity에 import되면 검사를 실행하고, 이후 임시 스크립트/meta/마커/복사 씬/고유 테스트 슬롯을 정리한 뒤 영구 소스를 다시 컴파일해야 한다. 다음 숲 제작·커밋·푸시는 시작하지 않았다.
