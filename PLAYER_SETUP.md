# 플레이어 좌우 이동 검토 단위

기초 이동과 점프 버퍼 구현을 완료했고, 2026-10-26 사용자가 커밋 후 다음 설계를 진행하도록 지시했다. 바닥 감지·단발 점프·코요테 타임을 유지한다. 기존 좌우 가속·감속을 유지하며 다음 기능은 사용자 검토 및 승인 후 진행한다.

## 실행

1. `Assets/_Project/Scenes/Tests/PlayerMovementTest.unity`를 연다.
2. Game 뷰를 16:9로 설정하고 Play를 누른 뒤 Game 뷰를 클릭한다.
3. A/D 또는 좌우 방향키로 이동하고 Space로 점프한다. W+D, S+D와 D 단독 입력의 수평 속도를 비교한다.
4. 키를 누르면 서서히 목표 속도에 도달하고, 키를 놓으면 감속하여 멈춘다. 반대 방향을 누르면 먼저 멈춘 뒤 새 방향으로 가속한다. 양쪽 벽은 통과할 수 없고 캐릭터는 회전하지 않는다.
5. 바닥의 눈금 간격은 1 Unity 유닛이다. 기본 이동 속도 6u/s는 테스트용 임시값이다.

Space를 일찍 놓으면 상승 속도를 한 번 줄여 낮게 뛰고, 충분히 오래 누르면 최대 높이까지 뛴다. 상승 중 다시 눌러도 줄어든 속도를 복원하거나 다시 줄이지 않는다. 누르고 있어도 착지 후 자동으로 반복하지 않는다. 애니메이션, 캐릭터 스프라이트 방향 반전, 전투, 카메라 추적은 아직 없다. 현재 카메라는 이동 검사용 고정 카메라다.

왼쪽의 초록 발판은 착지·걸어 나가기·코요테 타임 확인용이고, 오른쪽의 갈색 구조물은 천장 충돌 확인용이다. 2026-10-26 저장된 프리팹에서 발판 이탈 후 코요테 타임은 0.1초다. 점프 버퍼는 반대로 착지 전 입력을 0.1초 보관한다. 유효한 예약은 다음 바닥 판정에서 한 번 실행하고, 시간이 지나면 버린다. 버퍼가 남아 있어도 지상/코요테 조건을 만족하기 전에는 공중 점프를 실행하지 않는다.

## Inspector에서 조절할 값

Player 또는 Player 프리팹의 `Player Movement` 컴포넌트에서 조절한다.

| 표시 이름 | 직렬화 필드 | 현재 프리팹 임시값 | 의미 |
| --- | --- | --- | --- |
| Horizontal Move Speed | `horizontalMoveSpeed` | 6u/s | 입력에 따른 목표 속도의 상한 |
| Acceleration | `acceleration` | 40u/s² | 매초 증가할 수 있는 수평 속도 |
| Deceleration | `deceleration` | 60u/s² | 매초 줄일 수 있는 수평 속도 |
| Jump Speed | `jumpSpeed` | 16u/s | 점프 시작 시 설정하는 수직 속도 |
| Jump Cut Multiplier | `jumpCutMultiplier` | 0.5 | 키 해제 시 남은 상승 속도에 한 번 곱할 배율. 0은 즉시 상승 중단, 1은 높이 조절 해제 |
| Coyote Time | `coyoteTime` | 0.1s | 발판에서 벗어난 후 점프를 허용하는 시간 |
| Jump Buffer Time | `jumpBufferTime` | 0.1s | 착지 전 입력을 보관하는 시간. 0이면 예약 없이 현재 물리 틱에서만 허용 |
| Rise Gravity | `riseGravity` | 42u/s² | 상승 중 수직 속도 감소량 |
| Fall Gravity | `fallGravity` | 48u/s² | 낙하 중 수직 속도 감소량 |
| Max Fall Speed | `maxFallSpeed` | 20u/s | 최대 낙하 속도 |

가속도가 높으면 빠르게 출발하고 낮으면 천천히 속도가 붙는다. 감속도가 높으면 빨리 멈추고 낮으면 더 오래 미끄러진다. Inspector 최소값은 가속·감속 각각 0.01이다. 지상과 공중에 같은 값을 사용한다.

2026-10-26 저장된 프리팹의 점프 속도 16, 상승 중력 42, 코요테 타임 0.1은 사용자가 Inspector에서 조정한 값이다. 2026-10-25 검증 당시 코요테 타임 0.2와 구분한다. 이번 버퍼 구현에서는 이 값을 변경하지 않았다. 새 컴포넌트의 코드 초기값(12, 24, 0.1)과 기존 프리팹 설정을 구분한다.

`Player Ground Sensor`의 Ground Layers, Ground Check Distance(0.05u), Max Ground Angle(50도)로 바닥 판정 대상을 설정한다. 현재 지형은 Default 레이어이며 센서가 자기 충돌체와 트리거를 제외한다. 경사 각도는 표면의 법선을 기준으로 제한한다. PlayerMovement.IsGrounded는 상승 여부까지 반영한 읽기 전용 코드 프로퍼티다.

## 에셋과 연결

아래 경로는 프로젝트 루트를 기준으로 한다.

| 파일 | 역할 |
| --- | --- |
| `Assets/_Project/Scenes/Tests/PlayerMovementTest.unity` | 플레이어 프리팹 인스턴스, 바닥, 양쪽 벽, 발판, 낮은 천장, 1u 눈금, 고정 카메라 |
| `Assets/_Project/Prefabs/Player/Player.prefab` | Rigidbody2D, CapsuleCollider2D, NewInputMovementReader, PlayerGroundSensor, PlayerMovement, Visual 자식 |
| `Assets/_Project/Data/Player/PlayerMoveAction.asset` | 기존 InputSystem_Actions의 Player/Move 참조 |
| `Assets/_Project/Data/Player/PlayerJumpAction.asset` | 기존 InputSystem_Actions의 Player/Jump 참조 |
| `Assets/_Project/Data/Player/PlayerMovement.physicsMaterial2D` | 마찰 0, 반발력 0의 테스트용 물리 재질 |
| `Assets/_Project/Art/Sprites/MovementSquare.png` | 직접 생성한 흰색 사각형 플레이스홀더 |
| `Assets/_Project/Art/Materials/MovementPlaceholder.mat` | URP 2D Unlit 표시 재질 |
| `Assets/_Project/Scripts/Editor/PlayerMovementTestSceneBuilder.cs` | Unity Editor API로 테스트 에셋을 구성하는 에디터 전용 도구 |

Player의 `NewInputMovementReader > Move Action Reference`는 PlayerMoveAction에, Jump Action Reference는 PlayerJumpAction에 연결한다. `PlayerMovement > Movement Input Source`는 같은 오브젝트의 NewInputMovementReader에, Ground Sensor는 PlayerGroundSensor에 연결한다.

Rigidbody2D는 Dynamic, Gravity Scale 1, Linear Damping 0, Interpolate, Continuous 충돌 감지, Freeze Rotation이다. 루트 Scale은 (1,1,1), CapsuleCollider2D의 Size는 (0.8,1.8), Visual의 Scale은 (0.8,1.8,1)이다.

Play 중 PlayerMovement가 활성화되면 Gravity Scale은 0이 된다. 상승·낙하 가속도를 코드에서 처리하므로 Unity 기본 중력을 중복 적용하지 않기 위해서다. PlayerMovement를 비활성화하면 원래 Gravity Scale을 복원한다. 점프 조절은 Rigidbody2D의 Gravity Scale 대신 위의 Rise/Fall Gravity를 사용한다.

플레이어 크기 0.8×1.8u, 이동 속도 6u/s, 카메라 Orthographic Size 5.5, 테스트 지형의 좌표는 모두 임시값이다. 기획서의 최종 확정 수치로 취급하지 않는다.

## 코드 동작

- `PlayerMovementInput`은 HorizontalInput, IsJumpHeld, 한 번씩 소비하는 ConsumeJumpPress/ConsumeJumpRelease와 입력 비활성화 알림 InputDisabled를 제공하는 추상 MonoBehaviour다.
- `NewInputMovementReader.OnEnable`은 기존 Move·Jump 참조를 검사하고 복제하여 활성화한다. Jump performed/canceled 이벤트를 보관하고 다음 물리 업데이트가 ConsumeJumpPress/ConsumeJumpRelease로 한 번씩 소비한다. IsJumpHeld로 현재 유지 상태를 전달한다. 렌더 프레임 사이의 짧은 입력을 놓치지 않고, 여러 FixedUpdate에서 같은 입력으로 반복 점프하지 않는다. OnDisable은 복제한 액션과 미소비 입력을 정리하고 NotifyInputDisabled로 이동 컴포넌트의 예약도 취소한다.
- `PlayerGroundSensor.CheckGrounded`는 플레이어 CapsuleCollider2D를 아래로 짧게 Cast하고 레이어·트리거·표면 각도를 필터링한다. 이동은 하지 않는다.
- `PlayerMovement.Awake`는 Rigidbody2D, 입력 소스, 바닥 센서를 확보한다. 참조가 비어 있으면 같은 오브젝트에서 찾고, 그래도 없으면 오류를 출력하고 비활성화한다.
- `FixedUpdate`는 `HorizontalInput × horizontalMoveSpeed`를 목표 속도로 계산한다. 현재 속도보다 목표 속도의 절댓값이 작거나 입력이 반대 방향이면 감속도를, 그 외에는 가속도를 사용한다.
- 반대 방향 입력에서는 우선 목표를 0으로 둔다. 정지한 다음 물리 업데이트부터 새 방향으로 가속한다. 방향을 즉시 뒤집지 않는다.
- `Mathf.MoveTowards`의 최대 변화량은 `changeRate × Time.fixedDeltaTime`이다. 가속도의 단위가 u/s²이므로 이번에는 고정 시간 간격을 곱해 속도 변화량으로 바꾼다. 계산한 속도를 Rigidbody2D에 적용할 때 다시 시간 간격을 곱하지 않는다.
- FixedUpdate는 계산한 수평 속도와 수직 속도를 합쳐 Rigidbody2D에 한 번 적용한다. 다른 컴포넌트는 속도를 변경하지 않는다.
- CalculateVerticalSpeed는 하강/정지 중 바닥을 감지하면 착지로 처리하고 마지막 지상 시각과 점프 가능 여부를 갱신한다. 상승 중에는 발밑이 가까워도 지상으로 처리하지 않는다.
- UpdateJumpBuffer는 새 누름 시각과 대기 중 키 해제 여부를 보관한다. 새 누름은 기존 예약 한 건을 교체하고, 유효 시간이 지나면 삭제한다. InputDisabled 알림은 물리 틱을 기다리지 않고 예약을 지운다. Movement 활성화 시 알림을 구독하고 비활성화 시 해제한다.
- 지상 또는 유효한 코요테 타임 안에서 유효한 점프 예약이 있으면 Y속도를 Jump Speed로 설정하고 점프 권한과 코요테 기록을 즉시 소비한다. 착지 전에 이미 키를 놓은 예약은 시작 속도를 한 번 줄여 낮은 점프로 실행한다. AddForce는 사용하지 않는다.
- 상승 중 키 해제 이벤트 또는 유지 입력 해제를 감지하면 `ApplyJumpCut`이 남은 Y속도에 Jump Cut Multiplier를 한 번 곱한다. 다음 틱부터는 중력만 적용한다. 상승 종료·착지·Movement 비활성화 시 절단 권한을 정리한다. 해제 이벤트는 다음 물리 업데이트까지 보관하므로 그 사이 재누름에도 높이가 복원되지 않는다.
- 공중에서는 Y속도에서 Rise Gravity 또는 Fall Gravity × fixedDeltaTime을 빼고 최대 낙하 속도를 제한한다. 바닥에서는 수직 속도를 0으로 둔다. 천장 충돌은 물리 엔진이 상승을 막고 이후 낙하 계산이 이어진다.
- Reader를 끄면 수평 입력이 0이 되어 감속한다. Movement를 끄면 `OnDisable`에서 즉시 X 속도를 0으로 만들고 Y는 유지한다. 강제 비활성화와 일반 입력 해제를 구분한다.

## 입력 보정과 설계 이유

`Assets/InputSystem_Actions.inputactions`의 Player/Move 키보드 Composite만 `2DVector(mode=1)`로 변경했다. Digital 모드에서는 대각선 벡터를 정규화하지 않으므로 D와 W+D의 X가 모두 1이다. 이동 코드에서 X만 사용하므로 위·아래 키를 함께 눌러도 수평 속도가 감소하지 않는다. 기존 액션 ID와 게임패드 스틱 바인딩은 유지한다.

입력 읽기와 물리 이동의 책임을 나누고, 이동은 구체적인 New Input System 클래스 대신 작은 추상 입력 계약에 의존한다. 이번 단계에는 새로운 런타임 클래스나 상태 머신이 필요하지 않다. 물리 속도 적용은 PlayerMovement 한 곳에서 담당한다.

복제 액션은 원본 액션 맵의 Enable/Disable과 독립적이다. 향후 컷신·일시정지 입력 제어에서는 Reader 차단 또는 명시적인 입력 정책을 연결해야 한다. 현재 로컬 멀티플레이용 기기 할당은 없다.

## 검증 상태

아래 25개 검사는 가속·감속 추가 전의 기반 검증 기록이다. 각 수정의 별도 검증 기록을 구분한다.

- 정적 검사: 기존 액션 ID 보존, 입력 Composite 보정, 중복 GUID 여부, 프리팹의 입력·속도·충돌체 연결을 확인했다.
- 별도 C# 컴파일: 로컬 Unity 참조 어셈블리를 사용한 컴파일에 통과했다. Unity 에디터의 컴파일과 Play Mode 검증은 별개다.
- Unity 에디터 검증: Unity 6000.4.7f1에서 Editor API로 씬과 프리팹을 재생성하고 Play Mode 검사 25개를 통과했다. D, W+D, S+D 입력의 수평 속도는 모두 6u/s였고 실제 이동은 0.800초에 4.800u였다.
- 프리팹 연결은 Play 진입 전 에디터 상태에서 확인했다. 입력 참조, 스크립트 누락, 착지, 키 해제, 양방향 벽 충돌, 비활성화·재활성화, 중력 보존을 검사했다.
- 1280×720 렌더를 직접 확인했다. 임시 자동 검증 코드는 제거했으므로 새로고침만으로 테스트가 자동 시작되지 않는다.
- 상세 근거는 `Docs/Reports/2026-10-22-horizontal-movement-validation.md`에 기록했다. 실제 게임패드와 물리 키보드의 조작감은 자동 입력 검증과 구분하여 직접 확인한다.

가속·감속 변경: 별도 C# 컴파일과 Unity Play Mode 검사 22개에 통과했다. Inspector 수치 저장·변경 반영, 가속·제동·방향 전환, 시간 간격 반영을 제어된 호출로 확인하고, 자동 FixedUpdate와 실제 물리로 이동·정지·Reader 비활성화·공중 가속·Movement 강제 정지·벽 충돌·회전 고정을 확인했다. 임시 검증 코드는 제거했다. 상세 결과는 `Docs/Reports/2026-10-22-player-acceleration.md`에 기록했다.

바닥 감지·점프 변경(2026-10-23 기반 기록): Unity Play Mode 검사 37개를 통과했다. 센서의 레이어·트리거·벽·천장·경사 필터, 단발 점프, 코요테 타임 안/밖과 0 설정, 공중 재점프 차단, 착지 후 새 입력, 짧은 입력 보존, 낙하 속도 상한과 고속 착지, 기존 공중 가속·감속, 중력 소유권 복원을 확인했다. 현재 설정에서 점프 높이는 약 3.120u다. 경사 검사는 센서 판정이며 경사 이동 튜닝을 의미하지 않는다. 상세 근거는 `Docs/Reports/2026-10-23-ground-and-jump.md`에 있다. 임시 자동 검증 코드는 제거했다.

가변 점프·낙하 중력 강화(2026-10-25 현재): Unity 검증 총 43개(편집 모드 직렬화 2개 + 실제 Play Mode 41개)를 통과했다. 짧게/중간/충분히 유지한 점프 높이는 약 1.272/1.997/3.120u였고, 같은 최대 점프의 정점 이후 착지까지 낙하 시간은 중력 36에서 0.420초, 48에서 0.360초였다. 키 해제 후 재누름, 절단 1회 제한, 배율 0/1, 코요테·천장·최대 낙하 속도·공중 좌우 이동·비활성화 회귀를 확인했다. 프리팹에 새 값을 저장했고 씬의 저장되지 않은 변경은 보존했다. 임시 검증 코드는 제거했다. 자세한 내용은 `Docs/Reports/2026-10-25-variable-jump.md`를 참고한다.

점프 버퍼(2026-10-25 현재): 프리팹에 jumpBufferTime=0.1을 저장하고 Unity 검사 44개(편집 모드 6개 + 실제 Play Mode 38개)를 통과했다. 보관 시간 안/밖, 0 설정, 새 입력 교체, 착지 전 키 해제, 한 번 소비, 비활성화 취소와 기존 이동·가변 점프·코요테·충돌·낙하 속도 상한을 확인했다. 특정 낙하 배치에서 입력 보관 후 0.060초에 착지 점프가 실행됐다. 열린 씬의 미저장 설정을 보존하려고 저장된 테스트 씬의 임시 복제본에서 검사했고, 종료 후 원래 씬으로 복구했다. 임시 코드·요청·복제 씬은 제거했다. 자세한 근거는 `Docs/Reports/2026-10-25-jump-buffer.md`에 있다. 이전 43개 검증은 가변 점프 단계의 기록이며 이번 검증과 구분한다.

## 직접 확인할 추가 상황

- Play 중 Reader 체크박스를 끄고 다시 켜서 정지와 입력 복구를 확인한다.
- Movement 체크박스를 끄면 수평 속도가 0이 되는지 확인한다.
- Play 중 Player를 공중으로 옮겨서 낙하하며 좌우 이동할 수 있는지 확인한다.
- Inspector에서 Move Action Reference를 비운 뒤 Reader를 재활성화하면 오류를 출력하고 비활성화되어야 한다. 테스트 뒤 참조를 복구한다.
- 실제 게임패드의 스틱 기울기에 따른 속도 변화와 물리 키보드 조작감은 별도로 확인한다.

`ETHEREAN > Tests > Create Horizontal Movement Scene` 메뉴는 에셋이 없는 초기 상태용이다. 기존 파일이 있으면 덮어쓰지 않고 중단한다. 일상적인 테스트에서는 저장된 씬을 직접 연다.

## 기준 문서와 다음 검토

`AGENTS.md`와 `Docs/References/ETHEREAN_Codex_Collaboration_Prompt.md`에 협업 규칙을 보존했다. `Docs/References/ETHEREAN_GDD.md`는 첨부 PDF 34쪽의 전체 추출 텍스트이며 그림과 원본 레이아웃은 포함하지 않는다.

사용자의 설계 방향: 이후 최종 이동 속도는 캐릭터 스탯에서 공급하고 PlayerMovement는 입력에 따른 가속·감속과 실제 물리 이동을 담당한다. 현재 `horizontalMoveSpeed`는 프로토타입용 임시 공급 값으로 유지한다. 스탯의 기본값·보정·최종값 관리 방식과 연결은 해당 기능을 승인받은 뒤 설계하며, 이번 작업에서는 스탯 클래스나 미사용 인터페이스를 추가하지 않는다.

기초 이동의 검증 근거는 `Docs/Reports/2026-10-25-jump-buffer.md`와 앞선 보고서를 참고한다. 2026-10-26 커밋 전 별도 C# 컴파일은 통과했으며, 이전 Play Mode 검사를 새로 실행했다고 표현하지 않는다. 사용자가 승인한 다음 작업은 벽 감지와 벽 이동 능력 잠금·해금 조건 설계다. 설계 검토 후 다음 구현 단위를 진행한다.
