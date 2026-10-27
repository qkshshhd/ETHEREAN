# 플레이어 이동 설정과 검토

2026-10-27 승인된 책임 분리를 완료했다. 현재 PlayerMovement는 수평 이동·중력·물리 적용, PlayerJump는 공통 점프 처리, PlayerWallJump와 PlayerWallSlide는 각 벽 동작, PlayerLocomotion은 입력·센서·실행 순서를 맡는다. 새 Unity 92개 검사와 구 구현 대조를 완료했으며 사용자 구조·조작감 리뷰를 기다린다. 책임 분리 기록은 Docs/Reports/2026-10-27-locomotion-refactor.md에 있다. 이후 벽 코요테 보정을 추가하고 새 Unity 109개 검사를 완료했으며 최신 동작은 아래 벽차기 코요테 구역과 Docs/Reports/2026-10-27-wall-coyote.md를 따른다. 아래 날짜별 검증 기록은 당시 구현의 역사적 근거다.

## 실행

1. `Assets/_Project/Scenes/Tests/PlayerMovementTest.unity`를 연다.
2. Game 뷰를 16:9로 설정하고 Play를 누른 뒤 Game 뷰를 클릭한다.
3. A/D 또는 좌우 방향키로 이동하고 Space로 점프한다. W+D, S+D와 D 단독 입력의 수평 속도를 비교한다.
4. 키를 누르면 서서히 목표 속도에 도달하고, 키를 놓으면 감속하여 멈춘다. 반대 방향을 누르면 먼저 멈춘 뒤 새 방향으로 가속한다. 양쪽 벽은 통과할 수 없고 캐릭터는 회전하지 않는다.
5. 바닥의 눈금 간격은 1 Unity 유닛이다. 기본 이동 속도 6u/s는 테스트용 임시값이다.

Space를 일찍 놓으면 상승 속도를 한 번 줄여 낮게 뛰고, 충분히 오래 누르면 최대 높이까지 뛴다. 상승 중 다시 눌러도 줄어든 속도를 복원하거나 다시 줄이지 않는다. 누르고 있어도 착지 후 자동으로 반복하지 않는다. 애니메이션, 캐릭터 스프라이트 방향 반전, 전투, 카메라 추적은 아직 없다. 현재 카메라는 이동 검사용 고정 카메라다.

왼쪽의 초록 발판은 착지·걸어 나가기·코요테 타임 확인용이고, 오른쪽의 갈색 구조물은 천장 충돌 확인용이다. 2026-10-26 저장된 프리팹에서 발판 이탈 후 코요테 타임은 0.1초다. 점프 버퍼는 반대로 착지 전 입력을 0.1초 보관한다. 유효한 예약은 다음 바닥 판정에서 한 번 실행하고, 시간이 지나면 버린다. 버퍼가 남아 있어도 지상/코요테 조건을 만족하기 전에는 공중 점프를 실행하지 않는다.

## Inspector에서 조절할 값

Player 또는 Player 프리팹의 기능별 컴포넌트에서 조절한다. 필드 이름과 저장 수치는 유지하고 소유 컴포넌트만 옮겼다.

| 컴포넌트 | 표시 이름 | 직렬화 필드 | 현재 프리팹 임시값 | 의미 |
| --- | --- | --- | --- | --- |
| PlayerMovement | Horizontal Move Speed | `horizontalMoveSpeed` | 6u/s | 입력에 따른 목표 속도의 상한 |
| PlayerMovement | Acceleration | `acceleration` | 40u/s² | 매초 증가할 수 있는 수평 속도 |
| PlayerMovement | Deceleration | `deceleration` | 60u/s² | 매초 줄일 수 있는 수평 속도 |
| PlayerJump | Jump Speed | `jumpSpeed` | 16u/s | 점프 시작 시 설정하는 수직 속도 |
| PlayerJump | Jump Cut Multiplier | `jumpCutMultiplier` | 0.5 | 키 해제 시 남은 상승 속도에 한 번 곱할 배율. 0은 즉시 상승 중단, 1은 높이 조절 해제 |
| PlayerJump | Coyote Time | `coyoteTime` | 0.1s | 발판에서 벗어난 후 점프를 허용하는 시간 |
| PlayerJump | Jump Buffer Time | `jumpBufferTime` | 0.1s | 착지 전 입력을 보관하는 시간. 0이면 예약 없이 현재 물리 틱에서만 허용 |
| PlayerMovement | Rise Gravity | `riseGravity` | 42u/s² | 상승 중 수직 속도 감소량 |
| PlayerMovement | Fall Gravity | `fallGravity` | 48u/s² | 낙하 중 수직 속도 감소량 |
| PlayerMovement | Max Fall Speed | `maxFallSpeed` | 20u/s | 최대 낙하 속도 |
| PlayerWallJump | Wall Jump Horizontal Speed | `wallJumpHorizontalSpeed` | 8u/s | 벽 반대 방향 초기 수평 도약 |
| PlayerWallJump | Wall Jump Vertical Speed | `wallJumpVerticalSpeed` | 16u/s | 벽차기 초기 상승 속도 |
| PlayerWallJump | Wall Jump Control Lock Time | `wallJumpControlLockTime` | 0.1s | 수평 입력 제어 복구 전 보호 시간 |
| PlayerWallJump | Wall Coyote Time | `wallCoyoteTime` | 0.1s | 마지막 유효 벽 접촉 후 벽차기를 허용하는 시간. 0이면 접촉 중만 허용 |
| PlayerWallSlide | Wall Slide Speed | `wallSlideSpeed` | 3u/s | 벽 미끄러짐 낙하 상한 |

가속도가 높으면 빠르게 출발하고 낮으면 천천히 속도가 붙는다. 감속도가 높으면 빨리 멈추고 낮으면 더 오래 미끄러진다. Inspector 최소값은 가속·감속 각각 0.01이다. 지상과 공중에 같은 값을 사용한다.

2026-10-26 저장된 프리팹의 점프 속도 16, 상승 중력 42, 코요테 타임 0.1은 사용자가 Inspector에서 조정한 값이다. 2026-10-25 검증 당시 코요테 타임 0.2와 구분한다. 이번 버퍼 구현에서는 이 값을 변경하지 않았다. 새 컴포넌트의 코드 초기값(12, 24, 0.1)과 기존 프리팹 설정을 구분한다.

`Player Ground Sensor`의 Ground Layers, Ground Check Distance(0.05u), Max Ground Angle(50도)로 바닥 판정 대상을 설정한다. 현재 지형은 Default 레이어이며 센서가 자기 충돌체와 트리거를 제외한다. 경사 각도는 표면의 법선을 기준으로 제한한다. PlayerLocomotion.IsGrounded는 상승 여부까지 반영한 읽기 전용 코드 프로퍼티다.

## 에셋과 연결

아래 경로는 프로젝트 루트를 기준으로 한다.

| 파일 | 역할 |
| --- | --- |
| `Assets/_Project/Scenes/Tests/PlayerMovementTest.unity` | 플레이어 프리팹 인스턴스, 바닥, 양쪽 벽, 발판, 낮은 천장, 1u 눈금, 고정 카메라 |
| `Assets/_Project/Prefabs/Player/Player.prefab` | Rigidbody2D, CapsuleCollider2D, NewInputMovementReader, 두 센서, PlayerTraversalAbilities, Movement/Jump/WallJump/WallSlide/Locomotion, Visual 자식 |
| `Assets/_Project/Data/Player/PlayerMoveAction.asset` | 기존 InputSystem_Actions의 Player/Move 참조 |
| `Assets/_Project/Data/Player/PlayerJumpAction.asset` | 기존 InputSystem_Actions의 Player/Jump 참조 |
| `Assets/_Project/Data/Player/PlayerMovement.physicsMaterial2D` | 마찰 0, 반발력 0의 테스트용 물리 재질 |
| `Assets/_Project/Art/Sprites/MovementSquare.png` | 직접 생성한 흰색 사각형 플레이스홀더 |
| `Assets/_Project/Art/Materials/MovementPlaceholder.mat` | URP 2D Unlit 표시 재질 |
| `Assets/_Project/Scripts/Editor/PlayerMovementTestSceneBuilder.cs` | Unity Editor API로 테스트 에셋을 구성하는 에디터 전용 도구 |

Player의 `NewInputMovementReader > Move Action Reference`는 PlayerMoveAction에, Jump Action Reference는 PlayerJumpAction에 연결한다. `PlayerLocomotion`의 Movement Input Source/Ground Sensor/Wall Sensor/Traversal Abilities는 같은 오브젝트의 Reader/두 센서/능력 컴포넌트에 연결한다. Movement/Jump/Wall Jump/Wall Slide도 같은 오브젝트의 해당 기능 컴포넌트에 연결한다.

Rigidbody2D는 Dynamic, Gravity Scale 1, Linear Damping 0, Interpolate, Continuous 충돌 감지, Freeze Rotation이다. 루트 Scale은 (1,1,1), CapsuleCollider2D의 Size는 (0.8,1.8), Visual의 Scale은 (0.8,1.8,1)이다.

Play 중 PlayerLocomotion이 활성 Movement의 물리 제어를 시작하면 Gravity Scale은 0이 된다. 상승·낙하 가속도를 코드에서 처리하므로 Unity 기본 중력을 중복 적용하지 않기 위해서다. PlayerMovement를 비활성화하면 원래 Gravity Scale을 복원한다. 점프 조절은 Rigidbody2D의 Gravity Scale 대신 위의 Rise/Fall Gravity를 사용한다.

플레이어 크기 0.8×1.8u, 이동 속도 6u/s, 카메라 Orthographic Size 5.5, 테스트 지형의 좌표는 모두 임시값이다. 기획서의 최종 확정 수치로 취급하지 않는다.

## 코드 동작

- Reader는 기존 New Input System Move/Jump를 읽고 누름·해제를 보관한다. 입력 계약과 액션 에셋은 변경하지 않았다.
- PlayerLocomotion만 FixedUpdate를 가진다. 센서 결과·입력·능력을 읽고 기능 호출 순서를 조정한다. 지상 점프 → 유효 벽차기 → 코요테 점프 순서로 한 입력을 한 번 소비한다.
- PlayerMovement.CalculateHorizontalSpeed는 기존 가속·감속과 방향 전환을 계산한다. CalculateGravitySpeed는 상승/낙하 중력과 최대 낙하 속도를 계산한다. BeginMovement/EndMovement/ApplyVelocity만 Rigidbody2D 속도와 중력 소유권에 접근한다.
- PlayerJump.UpdateState는 접지 시각과 공유 버퍼를 관리한다. CanGroundJump는 지상/코요테 자격, StartJump는 버퍼·점프 권한 소비, ApplyJumpCut은 한 번의 상승 절단을 담당한다. 일반 점프와 벽차기가 같은 처리를 사용한다.
- PlayerWallJump.UpdateContacts/TryJump/IsControlLocked는 재접촉 자격·벽 선택·두 축 도약 속도·짧은 수평 제어 제한을 관리한다. 상승 중에도 사용하며, 같은 측면 접촉을 벗어나 재접촉한 후 새 점프 요청으로 반복한다.
- PlayerWallSlide.LimitFallSpeed는 해금된 공중 하강 벽 접촉에서만 Y속도 상한을 적용한다. Reader가 꺼져도 수동 미끄러짐은 유지한다.
- 조정자는 기능 계산 결과를 받아 Movement.ApplyVelocity를 한 번 호출한다. 다른 컴포넌트는 Rigidbody2D를 직접 읽거나 쓰지 않는다.
- Reader.InputDisabled는 Jump 버퍼와 WallJump 제어 제한을 즉시 취소한다. Movement/Locomotion을 끄면 X속도를 즉시 0으로 만들고 원래 gravityScale을 복구한다. 재활성화 시 오래된 입력·일시 상태를 정리한다.
- WallJump/WallSlide는 독립적으로 비활성화할 수 있다. Jump는 공통 점프 요청/높이 조절의 소유자이므로 끄면 벽차기를 포함한 점프 요청 처리를 중단한다. 지상 이동·중력·수동 미끄러짐은 유지한다.
- 상태 조회는 PlayerLocomotion.IsGrounded/IsWallSliding을 사용한다. 최종 속도의 스탯 공급, 공격·피격·전체 제어 상태 시스템은 별도 승인 단위다.

복제 액션은 원본 액션 맵의 Enable/Disable과 독립적이다. 향후 컷신·일시정지 입력 정책은 별도 연결하며, 현재 로컬 멀티플레이용 기기 할당은 없다.

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

기초 이동의 검증 근거는 `Docs/Reports/2026-10-25-jump-buffer.md`와 앞선 보고서를 참고한다. 2026-10-26 커밋 전 별도 C# 컴파일은 통과했으며, 이전 Play Mode 검사를 새로 실행했다고 표현하지 않는다. 이후 승인된 벽 미끄러짐의 현재 상태는 아래 최신 기록과 해당 최종 보고서를 따른다.


## 벽 미끄러짐 완료와 현재 저장 설정

벽 미끄러짐 구현과 프리팹 연결, 별도 테스트 씬 및 Unity 검사 66개(8 Editor + 58 Play Mode)를 완료했다. 2026-10-26 사용자가 동작을 긍정적으로 검토했고, 2026-10-27 임시 검증 파일 정리 및 잔여 7개 C# 소스의 새 별도 컴파일을 완료했다. 같은 날짜에 Play Mode 검사를 새로 반복한 것은 아니다.

- PlayerWallSensor.CheckWalls는 좌우 접촉만 반환한다.
- PlayerTraversalAbilities는 기본 잠금과 UnlockWallTraversal을 제공한다.
- 벽 미끄러짐 당시 PlayerMovement.CalculateVerticalSpeed(현재 CalculateVelocity)는 해금 후 하강 중 벽 접촉에서 Y속도를 -wallSlideSpeed 이상으로 제한한다. 상승 중에는 제한하지 않는다.
- Rigidbody2D의 X와 Y 속도는 PlayerMovement 한 곳에서 적용한다.

프리팹은 Wall Traversal Unlocked=false, Wall Slide Speed=3이다. 별도 테스트 씬은 Assets/_Project/Scenes/Tests/PlayerWallSlideTest.unity다. Play 중 해금 체크를 끄고 켜며 A/D와 Space로 잠금·상승·미끄러짐·벽 이탈·착지를 확인한다.

기존 PlayerMovementTest.unity에는 저장된 테스트 오버라이드 wallTraversalUnlocked=true와 wallLayers=51이 있다. 이 씬의 설정은 보존했다. 영구 능력 획득과 저장을 구현한 뜻은 아니다.

실제 이동 기록은 0.200초에 0.600u였다. 자세한 파일별 변경 위치, 흐름과 실제 실행 로그는 Docs/Reports/2026-10-26-wall-slide.md에 보존했다. 임시 자동 검증기는 제거했으므로 Ctrl+R만으로 검사가 자동 실행되지 않는다.

벽 매달리기는 이번 범위에서 제외한다. 벽차기는 아래 2026-10-27 승인 단위에서 완료했다. 실제 F 이벤트, GameProgress 및 영구 저장은 다음 별도 승인 단위다. 최종 이동 속도를 스탯에서 공급하는 방향은 유지하되 이번 기능에서는 horizontalMoveSpeed를 임시 공급 값으로 사용한다.

## 벽차기 완료 기록 — 책임 분리 이전(2026-10-27)

이 구역의 PlayerMovement 내부 메서드는 분리 이전 기록이다. 현재 기능별 위치는 위 코드 동작과 아래 최신 책임 분리 기록을 따른다.

벽차기는 해금된 공중 벽 접촉에서 상승·정점·하강 중 모두 가능하다. 벽 반대 방향 X속도와 Y속도를 설정하고, 짧게 수평 제어를 보호한 뒤 기존 가속·감속으로 조작을 복구한다. 같은 측면은 센서 접촉을 벗어났다가 재접촉하고 새 점프 입력을 해야 재사용할 수 있다. 버튼 유지 자동 반복은 없다. 기존 점프 컷·버퍼를 공유하며 지상 점프 → 유효 벽차기 → 코요테 점프 순서로 한 번 소비한다.

- PlayerMovement: `CalculateVelocity`, `SelectWallJumpSide`, `StartJump`, `CalculateHorizontalSpeed`, `CancelPendingInput`, `ResetWallJumpContacts`가 동작을 분담한다. Rigidbody2D 속도 적용은 FixedUpdate 한 곳을 유지한다.
- Inspector 임시값: Wall Jump Horizontal Speed=8, Wall Jump Vertical Speed=16, Wall Jump Control Lock Time=0.1. 기존 일반 이동·점프 수치는 유지한다.
- 전용 씬: `Assets/_Project/Scenes/Tests/PlayerWallJumpTest.unity`. 해당 인스턴스의 Wall Traversal Unlocked만 true이며 프리팹은 false다. 단독으로 열어 A/D와 Space로 중앙 두 벽 사이의 상승·왕복 및 외곽 단일 벽 재접촉을 확인한다. 상승 중 Space를 놓았다가 새로 누르면 하강을 기다리지 않고 벽차기해야 한다.
- 생성 도구: `ETHEREAN/Tests/Create Wall Jump Scene`은 파일이 없을 때만 새 씬을 만들며 기존 씬을 덮어쓰지 않는다. 새 씬을 저장한 뒤 이전 활성 씬으로 돌아간다. 기존 파일이 있으면 Project 창에서 직접 연다.
- Unity 검증: 65개(8 Editor + 57 실제 Play Mode) 모두 통과. 최종 7개 C# 소스의 별도 컴파일도 성공했다. 임시 검증 스크립트 2개와 .meta, 요청·중지·준비 파일은 제거했다. Ctrl+R로 자동 검사가 반복 실행되지 않는다.
- 측정: 해당 검사에서 제어 복구 0.120초(설정 0.1초, 물리 틱 0.02초), 벽차기 유지/1틱 후 해제 높이 3.208/1.004u, 무방향 입력 수평 이동 1.256u였다. 물리 키보드 조작감과 게임패드는 사용자 직접 검토 대상이다.
- 기존 기본 이동 씬의 테스트 해금·레이어·위치는 유지했다. 작업 중 저장된 Visual의 Y 스케일=1.8 오버라이드도 되돌리지 않았다. 별도 생성·검증 도구는 기본 씬을 저장하지 않는다.

파일별 수정 위치, 실행 순서, 검증 조건과 전체 로그는 `Docs/Reports/2026-10-27-wall-jump.md`를 참고한다. 벽 매달리기·F 이벤트·영구 저장·애니메이션·VFX와 실제 B/I 게이트는 이번 기능의 완료 범위가 아니다. 사용자 리뷰 후 다음 작업을 결정한다.

## 최신 책임 분리 결과 — 2026-10-27

PlayerMovement는 286줄에서 80줄로 나눴다. 새 PlayerJump 99줄, PlayerWallJump 70줄, PlayerWallSlide 28줄, PlayerLocomotion 153줄이며 줄 수에는 빈 줄을 포함한다. 기능 수식과 타이밍은 유지하고 변경 이유가 다른 정책·상태의 소유를 분리한 것이며 성능 향상이나 총 코드량 감소를 의미하지 않는다.

새 Unity 검사 92개(20 Editor + 72 실제 Play Mode)를 모두 통과했다. 같은 입력을 준 구 구현과 374개 물리 샘플을 비교했으며 속도·접지/미끄러짐 상태가 일치했고 위치 차이는 최대 0.000030u였다. 임시 구 구현·검증/이전 도구 4개와 .meta, 요청·준비 파일 및 임시 씬은 제거했다. 최종 11개 C# 소스의 별도 컴파일도 통과했다.

기본 이동 씬의 PlayerWallJump.wallJumpHorizontalSpeed=4 오버라이드는 유지했다. 프리팹과 벽차기 씬은 8이다. 프리팹 기본 능력 잠금, 씬별 해금·레이어·위치·Visual 크기와 기존 GUID를 보존했다. 벽 미끄러짐/벽차기 씬의 저장 내용은 동일하며, 기본 이동 씬은 해당 오버라이드 대상 컴포넌트만 바뀌었다.

PlayerWallJumpTest에서 위의 기능별 Inspector와 A/D·Space 조작을 직접 검토한다. 구조·조작감 리뷰가 끝나기 전에는 다음 기능으로 진행하지 않는다. 상세 파일 지도와 이전/검증 로그는 Docs/Reports/2026-10-27-locomotion-refactor.md에 있다.

## 벽차기 코요테 타임 — 2026-10-27 사용자 피드백

벽에서 떨어진 직후 입력이 무효가 되는 상황을 개선하기 위해 PlayerWallJump에 별도 wallCoyoteTime=0.1초를 추가했다. 현재 벽 접촉을 우선하고, 벽이 없으면 최근 유효 접촉의 시각·측면으로 도약한다. 두 이력이 유효하면 더 최근 벽을 선택하고, 시각이 같으면 방향 입력으로 선택한다. PlayerJump의 지상 coyoteTime이나 jumpBufferTime과 다른 설정이다.

벽차기 발동은 양쪽 기록을 소비하므로 유예 시간 안에 재접촉 없이 추가 점프할 수 없다. 사용 직후 아직 닿아 있는 출발 벽도 기록을 갱신하지 않으며 실제 이탈·재접촉 후 다시 사용할 수 있다. 착지와 이동 상태 초기화에서 지우고, Reader.InputDisabled는 물리 틱 사이에도 즉시 지운다. 센서·능력 비활성화는 다음 물리 틱에서 기록을 취소한다.

새 Unity 검사 109개(12 Editor + 97 실제 Play Mode)가 모두 통과했다. 실제 벽 이탈 후 마지막 관측에서 0.040초 뒤 누른 점프가 발동했고, 만료/0설정/소비/재접촉/비활성화 정리와 기존 이동·점프도 확인했다. 임시 도구와 .meta·마커·복제 씬을 제거하고 최종 11개 C# 소스를 별도 컴파일했다. 직접 확인은 PlayerWallJumpTest에서 벽 접촉 후 반대 방향으로 이동하고 곧바로 Space를 눌러 벽차기를 연결한다. 0이면 이전 접촉 필수 동작, 0.2이면 더 긴 유예로 비교할 수 있다. 수평 속도·중력·기존 점프 값은 바꾸지 않는다. 새 검증과 파일별 결과는 Docs/Reports/2026-10-27-wall-coyote.md를 따른다.

## 640×360 / 32 PPU 실제 비교 샘플

별도 `Assets/_Project/Scenes/Tests/PlayerPixelScaleTest.unity`를 단독으로 열고 Play한다. Review Camera의 Target Texture는 `Assets/_Project/Data/Player/PixelScalePreview.renderTexture`이며 같은 오브젝트의 PixelScalePreview에 카메라와 타깃이 연결돼 있다. 타깃은 640×360·Point·MSAA 1이며 실제 Game 화면 크기와 분리한다.

Game 뷰를 1280×720 또는 1920×1080 고정 해상도로 설정하고 에디터 확대/축소 배율도 확인한다. OnGUI는 타깃을 2배/3배 정수 확대한다. 작은 Game 뷰에서는 1배로 잘라 보이고 안내한다. View A 버튼은 Size 5.5, View B는 5.625다. 기존 Player 입력 A/D·Space로 이동·점프한다.

회색 도형은 64×64 프레임 안의 같은 60px 높이에서 2·2.5·3등신만 비교한다. 주인공 외형이나 정확한 등신/크기를 선택하지 않는다. 실제 Player의 청록 사각형과 충돌체는 기존 크기를 사용한다. B 후보에서 1.8u는57.6px, A는약58.91px다. 1px 체크무늬와1u 격자로 정적 픽셀 배율을 확인한다. 이동 좌표를 픽셀에 스냅하거나 Pixel Perfect Camera를 추가한 것은 아니므로 움직임의 가독성/흔들림을 직접 검토한다.

새 Unity25개 검사(13 Editor+12 Play)와 최종13개 C# 소스의 별도 컴파일을 통과했다. 실제 카메라 캡처는 Docs/Reports/2026-10-27-pixel-scale-candidate.png와 baseline.png이며 전체 경로·로그·한계는 같은 폴더의2026-10-27-pixel-scale-unity.md에 있다. 기존 씬·프리팹·이동 코드는 변경하지 않았다. 새 생성 메뉴는 ETHEREAN/Tests/Create Pixel Scale Review Scene이며 기존 파일을 덮어쓰지 않는다.
