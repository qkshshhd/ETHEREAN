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

## 대검 기본 전투 테스트 — 2026-11-02

씬: Assets/_Project/Scenes/Tests/PlayerCombatTest.unity. 저장된 전용 씬을 열어 Play한다. 기존 이동/벽 테스트 씬과 픽셀 비교 씬은 그대로 보존한다. 입력은 A/D 또는 방향키 이동, Space 점프, Enter/마우스 왼쪽/게임패드 서쪽 버튼 공격이다. 기존 Player/Attack Input Action을 복제하여 읽으며 inputactions 파일 자체는 바꾸지 않았다.

프리팹 PlayerAttack의 weaponUnlocked는 false이며 전용 씬만 true다. wallTraversalUnlocked도 전용 씬은 true, 프리팹은 기존 false다. 게임 내 검 획득·진행 저장 이벤트는 아직 연결하지 않았다. 테스트 패널의 Toggle test weapon lock은 검 잠금을 확인하고 Reset targets는 표적 체력과 횟수를 초기화한다. 실제 플레이어/적 체력·피격·AI·HUD 시스템은 다음 작업이다.

PlayerLocomotion.FixedUpdate가 입력·센서를 읽고 PlayerAttack.Tick을 호출한다. PlayerAttack은 None→Windup→Active→Recovery→None을 관리하고 시작 시 groundAttack/airAttack을 선택한다. 착지해도 진행 중 공격 종류는 유지한다. PlayerMovement.CalculateHorizontalSpeed에 공격 이동 배율을 전달하되 속도와 중력의 실제 쓰기는 기존 PlayerMovement 한 곳에서만 수행한다. 벽차기의 짧은 바깥 방향 제어 보호는 공격 속도 제한보다 우선한다.

준비·타격·회복 초반은 점프 시작을 막는다. 회복60% 이후 기존 점프/벽차기 조건에 맞는 점프가 실제 실행되면 공격을 취소한다. 기존0.1초 점프 버퍼는 유지되므로 허용 시점 직전 입력은 버퍼 안에서 실행될 수 있다. 공중에서 점프 조건이 없으면 공격을 취소하거나 추가 점프를 주지 않는다. 동시 공격·점프 입력은 공격 시작을 먼저 평가하며 준비 단계가 시작되면 점프는 버퍼 규칙을 따른다. 벽 미끄러짐 상태에서는 공격을 시작하지 못하고 상승/일반 공중에서는 가능하다. 공격 시작 후 방향은 고정되지만 이동 입력은 단계별 제한 안에서 바꿀 수 있다.

PlayerAttackHitbox.Sample은 Active에만 박스를 질의하고 AttackHitReceiver 단위로 중복을 제거한다. 하나의 표적에 콜라이더가2개여도 한 공격당 한 번만 맞는다. AttackHit에는 피해량·피격 유형·방향 반영 넉백·접점이 담긴다. CombatTestTarget은 테스트용 HP/적중 횟수/섬광만 처리하며 넉백 이동이나 실제 경직·사망 AI는 구현하지 않는다. CancelAttack은 진행 중 판정과 적중 기록을 종료하고 명중 콜백 중 취소도 남은 대상의 처리를 중단한다.

조절할 데이터: Assets/_Project/Data/Player/GroundAttack.asset과 AirAttack.asset. 임시 지상0.24/0.12/0.4초, 공중0.18/0.12/0.32초(준비/타격/회복), 이동 배율0.35/0.05, 회복 중0.05→1, 점프 캔슬 시작0.6, 피해10, Stagger, 넉백(3,2), 판정 중심오프셋(0.95,0.1), 크기(1.4,1.6)다. 기본 이동속도6 기준 준비 최대2.1u/s, 타격 최대0.3u/s다. 회복 이동은 기존 가속 제한도 받는다. 모두 테스트용이며 최종 스탯 공급 설계는 아니다.

PlayerAttackVisual은 기하학 검선·은회색 짧은 궤적·접점 섬광을 담당한다. 길이1.5u는32PPU 후보에서48px인 임시 선일 뿐 실제 캐릭터 아트가 아니다. 현재 청록 이동 사각형/충돌체1.8u는 그대로라서 임시40px 주인공에 대한 정확한1.2배 실루엣 검증은 아직 아니다. 히트스톱·화면 흔들림·효과음·먼지·원화·스프라이트 애니메이션은 이번에 추가하지 않았다.

직접 확인:
1. 바닥에서 Enter를 눌러 단계와 표적 HP를 확인한다. 버튼을 계속 눌러도 자동 연타하지 않는다.
2. 오른쪽 이동 중 공격하면 준비에서 느려지고 타격에서 크게 제한된 뒤 회복한다. 왼쪽도 확인한다.
3. 준비/타격/회복 초반 Space는 즉시 점프하지 않는다. 회복 후반에는 점프로 공격을 끝낼 수 있다.
4. 공중 공격은 낙하를 유지한다. 공격 중 착지해도 패널의 Air 선택이 유지된다.
5. 벽 미끄러짐 중 공격이 시작되지 않고, 벽차기 후 분리하면 공중 공격이 가능하다.
6. 검 잠금 토글 후 공격이 막히고, 다시 해제하고 새로 누르면 시작한다. 표적이0HP가 되면 Reset targets로 재검토한다.

전체 파일 지도·검사 결과·설계 한계는 Docs/Reports/2026-11-02-greatsword-combat.md에 기록한다.
## 최신:3타 대검 콤보와 폴더 분리 — 2026-11-02

기존 단발·선형 검 표시 안내보다 이 구역이 우선한다. 최신 씬은 Assets/_Project/Scenes/Tests/PlayerGreatswordComboTest.unity다. 처음 Enter/왼쪽 클릭으로1타, 타격/회복 중 새 입력으로2타, 다시 입력해3타를 예약한다. 준비 중 입력과 버튼 유지는 다음 타를 예약하지 않는다. 회복25%부터 예약 연결,65%부터 유효 점프로 취소한다. 지상/공중3타 데이터는 시작 시 고정하고 좌우 방향은 체인 동안 유지한다.

1타는 낮게 끄는 검을0.8초에 들어 올리고0.14초에 가속 내려친다.2타는 짧은 준비 후0.18초 올려 벤다.3타는 왼쪽 뒤에서 오른쪽으로 수평으로 벤다. 모션은 GreatswordMotions 폴더의 편집 가능한 AnimationClip이고 타격은 실제 BladeBase/BladeTip 사이의 지나간 경로에 연결된다. 무기 Rigidbody 중력을 추가한 것이 아니라 클립의 가속으로 무게를 표현한다.

독립 Greatsword.prefab은 현재1.8u 몸체 대비1.2배인2.16u 길이의 사각형이다. Facing/Motion/Blade에서 외형, BladeBase/BladeTip에서 판정 기준, EffectSocket에서 단계 효과 기준을 조절한다. 캐릭터의 최종 외형과40px 임시 기준은 그대로 별도 검토다.

코드로 그리던 PlayerAttackVisual·LineRenderer·CombatPreview 재질과 표적의 색상 섬광은 제거했다. 효과를 임의 생성하지 않는다. 각 GroundAttack/AirAttack 및2·3 데이터에서 Windup Effect Prefab/Swing Effect Prefab/Hit Effect Prefab을 연결하면 PlayerAttackEffects가 생성·정리한다. 기본값은 전부 None이므로 현재 궤적/명중 효과가 없는 것이 정상이다.

Runtime/Player/Movement에는 입력·이동·점프·센서·벽 능력10개, Runtime/Player/Combat에는 공격·데이터·무기·판정·효과·조정자·표적/패널10개를 둔다. 이름과 Etherean.Player 네임스페이스 및 .meta GUID를 보존했다. Movement는 공격을 직접 알지 않고 PlayerCombatCoordinator가 이벤트와 일반 이동 제약값으로 연결한다. 속도 쓰기는 PlayerMovement 한 곳이다.

파일별 전체 경로·상세 흐름·테스트와 설정은 Docs/Reports/2026-11-02-greatsword-combo.md를 따른다. 이 결과의 조작감/코드 리뷰 전에는 다음 기능으로 진행하지 않는다.
## 최신: 캐릭터 애니메이션과 시간 기반 판정 — 2026-11-03

이 구역이 이전 독립 대검/검날 경로 판정 안내보다 우선한다. 새 확인 씬은 Assets/_Project/Scenes/Tests/PlayerTimedComboTest.unity다. 검 프리팹·PlayerWeaponMotion·검 전용10개 클립은 제거했다. 무기를 따로 움직이지 않고 향후 캐릭터 스프라이트 애니메이션에 함께 포함한다. 실제 캐릭터 원화/컨트롤러는 아직 없으므로 현재 공격 그림이 없는 것은 미연결 상태이며 완료된 캐릭터 애니메이션이 아니다.

입력→Windup Time 대기→Active Time 동안 캐릭터 기준 Hitbox Offset/Size/Angle 검사→Recovery Time 순서다. 판정은 Animator/Animation Event/검 Transform에 의존하지 않는다. Active 동안만 수신자별 한 번 적중하고 취소하면 즉시 중단한다. 지상/공중3타·입력 예약·이동 제한·회복65% 이후 유효 점프 캔슬·벽 상태 제한은 유지한다.

조절할 위치: Assets/_Project/Data/Player의 GroundAttack/AirAttack 및2·3 데이터. 기존 시간/피해는 유지했다. 새 판정 범위는 임시값이므로 실제 그림을 준비하면 맞춘다. Scene에서 Player를 선택하고 Gizmos를 켜면 범위가 보인다. PlayerAttackHitbox.Preview Data로 대기 중 확인할 타격을 고를 수 있다.

애니메이션 연결: 캐릭터 Visual의 Animator를 PlayerAttackAnimator.Character Animator에 지정하고, 데이터 Animation State Name을 실제 상태 전체 이름에 맞춘다(초기값 Base Layer.Attack1/2/3). 필요하면 Return State Name을 지정한다. 브리지는 전체 전투 진행률로 지정 공격 상태를 샘플링하며 같은 레이어를 다른 공격 시간 코드로 중복 제어하지 않는다. 캐릭터 Sprite/자세만 애니메이션하고 Rigidbody2D 루트 위치는 클립으로 움직이지 않는다. 준비/타격 장면의 비율을 데이터에 맞춘다. 현재1타의 Active는 전체 길이 약60.6~71.2% 구간이다.

커스텀 효과는 기존 Windup/Swing/Hit Effect Prefab 슬롯을 사용한다. 준비·타격 효과의 기준은 검 소켓 대신 Player 아래 AttackEffectOrigin이며 로컬 위치/스케일이 공격 방향에 맞춰 반전된다. 명중 효과는 실제 접점에 생성한다. None이면 효과를 그리지 않는다.

전체 경로·설계/검증·미완료 사항: Docs/Reports/2026-11-03-timed-character-attacks.md. 새로운 사용자 리뷰 전에는 다음 기능으로 넘어가지 않는다.
## 최신 콤보 타격 후 재정비 시간 — 2026-11-03

지상/공중1타는 Recovery Time=0.38, Combo Link Start=0.60으로 다음2타 준비 전 최소0.228초를 기다린다.2타는 Recovery Time=0.65, Combo Link Start=0.80으로3타 준비 전 최소0.520초를 기다린다.3타의 전체 후딜0.5초는 그대로다. 입력은 기존처럼 타격/회복 중 예약 가능하지만 연결 시점 전에는 실행하지 않는다. 다음 타격의 Windup은 이 간격 뒤에 별도로 적용된다.

Inspector에서 GroundAttack/AirAttack 및2 데이터를 조절한다. Combo Link Start는 회복 진행률0~1이며, 최소 간격=Recovery Time×Combo Link Start다. 이미 저장된 튜닝을 적용했으므로 생성 메뉴를 재실행할 필요는 없다. ETHEREAN/Combat/Apply Combo Recovery Timing 메뉴는 이 초기값으로 되돌리는 명시적 튜닝 도구다.

2타 회복이 길어진 만큼 이동 복구도 길어지며, 기존65% 점프 캔슬은 약0.4225초부터 가능하다. 유효 점프로 예약된3타를 취소할 수 있다. 실제 캐릭터 애니메이션을 넣을 때 재정비 구간을 이 시간에 맞춘다. 수치는 확정값이 아닌 초기값이다.

새31개 검사(9 Editor+22 실제 Play)와 정리 후24개 소스 컴파일 통과. 타격별 측정·변경 파일·수동 확인은 Docs/Reports/2026-11-03-combo-recovery-gaps.md를 따른다.
## Character animation completed (2026-11-03)

User authorized importing Desktop/ETHEREAN_SPRITE base image and 16 frames each of Idle_Normal/Walk_Normal/Run_Normal. Completed via actual Unity Editor APIs: 49 byte-identical PNGs at 32 PPU, Point/uncompressed/no mipmaps, 3 looping sprite clips, PlayerCharacter.controller and Player.prefab connections. PlayerCharacterAnimator reads actual speed/grounding and only selects presentation; PlayerMovement remains the sole velocity writer. Old Visual renderer is disabled while its transform/scene overrides are preserved; new unit-scale CharacterVisual is anchored to unchanged collider bottom. PlayerAttackAnimator adds IsDrivingAnimation for playback priority. Grounded cutoff 0.1u/s and Run threshold 3u/s, rates 8/12/16fps are temporary Inspector/clip settings. No new sprint/walk input. Current native artwork has about 30px visible animated height; 40px remains provisional, not automatically rescaled. Missing air/attack artwork uses a frozen neutral pose; this is not final jump/attack animation. Fresh validation passed 79 checks (59 Editor + 20 actual Play Mode), including sustained analog Walk, acceleration/deceleration, frame progress, facing, jump/land, attack fallback/recovery and disable/reenable. Actual rendered capture inspected. Original open/active/dirty scene state restored. Temporary validators/meta/request/ready/copied scene were removed; final 26 C# sources separately compiled with zero errors. Existing scenes, movement, attack data, Input Actions, packages/settings and retained GUIDs preserved; staged pixel scene still has its original 45 additions. See Docs/Reports/2026-11-03-character-locomotion-animations.md. Await user visual/feel/code review; no next bundle, commit or push.

## Current character size is the development baseline (2026-11-03)

Latest user decision overrides historical preservation of the old 1.8u collider/tuning for this approved unit: fit jump, gravity and related gameplay to the currently supplied character, and use its current size for future development. Working reference is native 32 PPU, 30px typical visible height = 0.9375u (actual frames vary about 28-31px), not transparent 32px canvas, former 40px proposal or historical 60px guides. Do not enlarge artwork/change PPU to fit old physics. Scale player collider, movement speeds/accelerations/gravity, wall launch/slide, sensor distances, animation speed thresholds, attack offsets/sizes and knockback by 0.9375/1.8 initially; keep timing windows, combo recovery gaps, damage and input policies. Update prefab serialization and future component defaults. Create a current-scale review scene with matching geometry/targets while preserving historical saved scenes and user overrides. Future movement, combat, enemies, terrain, world VFX and items must be checked relative to current visible character height; do not auto-rescale runtime values if artwork later changes. This is a working gameplay baseline, not final art-size approval. See Docs/Design/2026-11-03-current-character-scale.md. Current implementation/Unity verification is in progress; do not claim completion before fresh tests. No unrelated feature, commit or push is authorized.
## Weighted jump review and movement test map (2026-11-03)

User reports overly high/floaty jumping and authorizes heavier tuning, then improvement of the test map in the same unit. Actual saved starting prefab still has jumpSpeed=16, riseGravity=42, fallGravity=48, maxFallSpeed=20 and wallJumpVerticalSpeed=16. The previous automatic size migration did NOT apply: it stopped at its safety guard because the user changed CapsuleCollider2D to size (0.4948379,0.88451296), offset (-0.003912464,-0.4577435). Preserve that custom collider, art positioning, saved horizontal settings and existing scenes. Current approved tuning preparation is jumpSpeed=10, riseGravity=40, fallGravity=65, maxFallSpeed=16 and wallJumpVerticalSpeed=10; old proportional proposal 8.333/21.875/25 is historical and not the applied baseline. Preserve coyote/buffer/cut, wall launch protection, combo regrip times and all other combat settings. PlayerMovementFeelSceneBuilder applies only these five saved values and builds PlayerMovementFeelTest: graded platforms, 1.25u/2u gaps, low ceiling, accessible wall-jump corridor and safety floor. Old pending CurrentScaleValidation helpers/meta/requests were removed to prevent obsolete checks. Completed: actual saved prefab has the five new values, PlayerMovementFeelTest is generated, final 28 checks passed (15 Editor + 13 actual Play Mode). Same-collider measured held height 3.2080u -> 1.3520u, observed apex 0.420s -> 0.280s, flight 0.780s -> 0.500s; new short height 0.4640u. Actual 640x360 map capture inspected. Temporary HeavyJumpValidation helpers/meta/request/ready/copied scene were removed; final 28 C# sources separately compiled with zero errors. Original open/active/dirty scene state restored. User feel review is pending. See Docs/Reports/2026-11-03-weighted-jump-and-test-map.md for completion status. Current character size remains the future development reference. No next feature, commit or push.
Observed during final comparison: PlayerCombatTest.unity was separately saved during the work and currently contains acceleration=1000/deceleration=1000 scene overrides. Preserve them. The new movement review scene uses prefab 40/60; do not claim those scene overrides were copied. Existing staged pixel scene remains 45 added lines.

## Softer acceleration and wall jump review (2026-11-03)

User authorizes reducing acceleration/deceleration and wall-jump strength. Current saved prefab before this unit is acceleration=40, deceleration=60, wall horizontal=8/vertical=10; PlayerCombatTest has 1000/1000 scene overrides. Prepare/apply acceleration=18, deceleration=24, wall horizontal=4.5/vertical=9. Remove only those two CombatTest overrides so the scene inherits the new tuning; preserve other scene changes/overrides, especially historical base movement wall horizontal=4. Keep maximum horizontal speed=6, ground jump=10, gravity=40/65, fall cap=16, wall lock/coyote=0.1 and all attack timings unchanged. Existing PlayerMovementFeelSceneBuilder owns the Editor application method; no new runtime framework. Fresh Unity checks and temporary SoftMovementValidation cleanup are pending; separate compilation passed. See Docs/Reports/2026-11-03-soft-movement-and-wall-jump.md. Preserve current-size development policy and custom collider. No next feature, commit or push.
## Direct horizontal movement decision (2026-11-03)

Latest user instruction removes acceleration/deceleration entirely, superseding all earlier inertia and 18/24 tuning rules. PlayerMovement.CalculateHorizontalSpeed directly returns clamped input times allowed speed; remove acceleration/deceleration Inspector fields and obsolete current-speed/delta-time arguments. PlayerLocomotion still skips horizontal recalculation during the existing 0.1s wall-jump launch protection. Preserve saved maximum speed=6, ground jump=10, gravity=40/65, fall cap=16 and current wall launch=4.5/9, coyote/buffer/cut and combat recovery constraints. Keyboard starts/stops/reverses on the next physics tick; analog speed remains proportional; airborne horizontal control follows the same direct policy. Keyboard now goes straight to Run; Walk remains useful with lower analog input. Remove obsolete soft-tuning menu and scaling-tool references to removed fields; reserialize the existing prefab through Editor API. Old SoftMovementValidation helpers/meta/request/ready removed. Completed via actual Unity API prefab reserialization: acceleration/deceleration fields are removed from Inspector and prefab. Fresh validation passed 29 checks (13 Editor + 16 actual Play Mode), including first-tick +6/-6/0, input lifecycle, analog speed, air control, wall launch protection/recovery, attack constraints/cancel and two-unit gap traversal. Original open/active/dirty scene state restored. Temporary DirectMovementValidation sources/meta/request/ready/copied scene removed; final 28 C# sources separately compiled with zero errors. Prior saved scenes, combat data, project settings, retained GUIDs and user staged pixel-scene work preserved. Await user code/feel review. See Docs/Reports/2026-11-03-direct-horizontal-movement.md. No next bundle, commit or push.
## Lighter jump without changing peak height (2026-11-03)

User requests less weight but unchanged jump height. Preserve direct horizontal control, current size/custom collider, max speed=6, fall cap=16, wall horizontal=4.5 and existing timing windows. Prepare coupled tuning: riseGravity 40->32, fallGravity 65->52, ground jumpSpeed 10->8.986667, wall vertical 9->8.086154. These launch values correct the discrete 0.02s physics-step apex rather than using only continuous v^2/(2g). Target full height 1.352u and wall relative apex 1.104u. Adjust jumpCutMultiplier 0.5->0.51080966 to also preserve representative one-tick tap height 0.464u. All intermediate input-duration heights are not promised mathematically identical; longer flight also increases potential horizontal reach. No automatic runtime height/stat system added. Updated ApplyTuning menu is Apply Balanced Jump Timing. Completed: actual Unity saved the coupled tuning; 22 checks passed (9 Editor + 13 actual Play Mode). Actual full height 1.352000u -> 1.352000u (delta 0.00000024u), one-tick tap 0.464000u -> 0.464000u and relative wall apex 1.104000u -> 1.104000u. Observed apex time 0.280s -> 0.320s; flight 0.500s -> 0.560s. Direct X control, air repress and landing checks passed. Original open/active/dirty scene state restored. Temporary LighterJumpValidation helpers/meta/request/ready/copied scene removed; final 28 C# sources separately compiled with zero errors. Existing scenes/assets/settings/GUIDs and user staged pixel-scene changes preserved. Await user feel review. See Docs/Reports/2026-11-03-lighter-jump-same-height.md. No next feature, commit or push.
## Movement review accepted; speed 3 (2026-11-03)

User set horizontalMoveSpeed=3 and accepted current player values as the working baseline, then requested today's development. Verified saved Player.prefab speed=3; align new-component default to 3. Preserve direct horizontal control, jumpSpeed=8.986667, jumpCutMultiplier=0.51080966, gravity=32/52, fall cap=16, wall launch=4.5/8.086154 and current collider/art scale. Do not resume unsolicited movement tuning. Earlier Play checks at speed 6 are historical, not fresh checks at speed 3; the 2u review gap must be reassessed when reviewing level geometry. Latest daily plan 11-03 task is health/reactions/death and badger combat; older DAY 09 forest schedule is superseded. Concrete proposed next review bundle, class responsibilities and tests are in Docs/Plans/2026-11-03-health-reaction-badger-unit.md. Scope confirmation is pending under the existing agree-bundle-before-implementation rule; no health/AI code implemented by that planning record. No commit or push.
## Badger cycle authorized after cleanup and commit (2026-11-03)

User authorizes the badger combat-cycle review unit, using only a simple square badger prefab. User will create warning VFX; expose assigned-prefab slots, leave empty slots silent, and do not invent procedural rendering, effects, markers or HUD/design in code. Gizmos are allowed only as temporary implementation aids and must be completely removed on feature completion. First remove ALL existing custom Gizmo code, then commit current work to local Git, then start the badger implementation. This explicitly authorizes the preparatory local commit, not push. Preserve accepted speed=3 and current player tuning. PlayerAttackHitbox.OnDrawGizmosSelected, previewData and its builder assignment are the existing custom drawing found; remove all three and reserialize prefab. Scope plan updated at Docs/Plans/2026-11-03-health-reaction-badger-unit.md; do not treat the former proposed CombatDebugHud as approved. If local Git metadata writes are unavailable, complete/review cleanup, report the actual block, and wait for the prerequisite commit rather than starting the enemy unit out of order.
Warning feedback follow-up (2026-11-03): until user warning VFX exists, log warning-state entry once with Debug.Log and the enemy context. Do not log every frame or draw replacement markers/effects. This is part of the already authorized badger cycle after the prerequisite cleanup commit.Commit prerequisite blocked: 2026-11-03 git add failed with .git/index.lock Permission denied. No new commit or push. Gizmo removal, prefab reserialization and final 28-source compilation completed; custom drawing and preview references are both zero. Badger implementation, including warning-entry Debug.Log once, remains authorized but waits for the user's required local commit. See Docs/Reports/2026-11-03-gizmo-cleanup-before-badger.md.
## Badger combat implementation in progress (2026-11-03)

Verified user prerequisite commit 141c843 and a clean starting worktree. User explicitly confirmed that commit. Implement only the approved combat cycle: common CombatHealth/CombatReaction/CombatDamageReceiver; generic external movement in PlayerLocomotion, priority/cancellation in PlayerCombatCoordinator; BadgerData/Motor/Attack/Brain; Editor-generated square prefab and separate combat scene. No invented VFX/HUD/marker/Gizmo. Alert entry logs Debug.Log once; optional user WarningEffectPrefab is destroyed on exit/disable/death and None creates nothing. Player keeps accepted speed=3, direct X movement, current jump/collider. Player root layer 8, badger 9, terrain 0; existing attack target mask adds bit 9. Runtime Inspector health/phase/state fields are for review. Separate compilation passes, but latest Unity import, prefab/scene creation, actual engine checks and temporary BadgerValidation cleanup are PENDING; do not claim completion. Helpers poll Logs/BadgerValidation.request once Editor imports them. See Docs/Reports/2026-11-03-badger-combat.md. No new commit/push or next unit until review.

## Badger combat completed; review pending (2026-11-03)

Prerequisite commit 141c843 verified. Unity generated PlayerBadgerCombatTest, Badger.prefab (plain 0.7u square), BadgerData.asset and player health/reaction/receiver connections. Player remains speed 3 with accepted tuning; player layer 8, enemy 9, terrain 0, player attack mask includes enemy bit. Warning logs once on Alert entry; optional user VFX slot defaults None. No invented VFX/HUD/Gizmo. Fresh validation passed 22 checks (6 Editor + 16 actual Play Mode): actual reciprocal damage/contact safety, compound collider hit, reservation and hit cancellation, Knockdown auto recovery, cliff/high-target behavior and death gating. Strong Knockdown was injected as a test hit; ordinary badger damage remains Stagger. Initial temporary Runtime->project Editor class reference caused Unity compile failure and was corrected. Final compilation independently passed Runtime 29 sources and Editor 7 sources referencing the runtime DLL. Temporary validators/meta/request/ready/copied scene removed; original open/active/dirty scene state restored. Gizmo/drawing-code matches zero. See Docs/Reports/2026-11-03-badger-combat.md for exact files, settings, tests and limitations. Custom user warning VFX, final art/reaction animation, save/respawn/drop/healing are not implemented or validated. Do not start next unit or commit/push before user review.

## Simplified badger animation design (2026-11-03)

Latest user update removes standing up to reduce production work. Supersede the prior stand-up/settle-down/upright-walk proposal: no posture-transition states, clips or waiting times. Keep territory entry -> stationary warning -> further approach triggers chase/attack -> return. Use one consistent baseline pose without prescribing new appearance. Recommended basic clips are Idle (also used for Alert), Walk (Chase/Return), Attack (windup/active/recovery), Stagger and Death: 5 total. Optional strong-hit visuals add KnockdownLaunch/KnockdownDown/GetUp: 8 total. Knockdown recovery GetUp is distinct from removed warning stand-up and existing reaction gameplay is preserved. Presentation-only BadgerAnimator remains a future proposal. Current AlertTime->Chase code still needs a separate engage-distance condition; no runtime code/scene/prefab/clip was changed by this documentation request. Preserve user warning VFX slot, once-per-entry Debug.Log, accepted player tuning and no completed Gizmo. See Docs/Design/2026-11-03-badger-animation-structure.md.

## Latest badger sprite linkage (2026-11-03; engine verification pending)

User provided 16 frames each of Idle/Walk/Attack/Death; no Stagger. Source sprites all face right. 64 byte-identical PNG copies, Point/32 PPU/uncompressed/no mipmaps; keep native sizes. BadgerAnimator reads Brain/Motor/Health/Reaction: Idle for Idle/Alert, Walk only for real Chase/Return motion, timed Attack, one-shot Death with last frame held, frozen neutral Idle for missing reaction art. Right is flipX=false; left=true. No stand-up states/art, procedural design/VFX or Gizmo. BadgerBrain exposes facing/attack progress and implements previously requested further-approach condition using Inspector engageRange=2.5u (temporary); alert minimum .6s, detection4u and attack.9u unchanged. Attack frame keys use 1-6 preparation, 7-8 active, 9-16 recovery within existing .35/.12/.65s. Badger prefab adds unit-scale CharacterVisual and presenter, hides Square; old .7u body and player/scene tuning preserved. Mixed 96/64 canvases use fixed per-clip foot pivots; visual body is larger than old collider and needs user review. Assets and connections saved using existing serialized formats because Unity startup failed before loading project (ERROR_SUCCESS == status, exit1). Editor API rebuilding menu is available but not engine-tested. Final separate compilation Runtime30/Editor8 passes; static PNG/import/GUID/local-reference/hash checks pass; no actual Play Mode or render check for this unit. Temporary validators/request markers removed. See Docs/Reports/2026-11-03-badger-animation-link.md. The older simplified-design "not implemented" statements are historical and superseded here; engine verification remains pending. No next feature, commit or push.

## Badger sprite linkage verified in Unity (2026-11-03)

Supersedes the previous pending verification note. After Editor refresh, BadgerAnimationBuilder actually ran in Unity and saved imports/clips/controller/prefab connections. Fresh validation passed 86 checks: 74 Editor + 12 actual Play Mode, covering alert hold, further-approach Walk, idle/walk frame progress, both facings, combat Attack selection, missing Stagger neutral hold, Death interruption/progress/final frame. Actual 256x256 death render inspected. Original scene configuration/dirty state comparison logged RESTORED. Temporary two validator scripts/meta, request/ready and copied scene removed; final Runtime30 + Editor8 sources separately compile with exit0. Existing scenes/player settings retained. This does not verify exact visual strike/damage synchronization, final art size/body match, all Return behavior, user VFX or physical-device feel. Missing Stagger/Knockdown art remains a neutral fallback. See Docs/Reports/2026-11-03-badger-animation-link.md and its validation.txt/capture.png evidence. User review pending; no next feature, commit or push.

## Badger facing and canvas fix verified (2026-11-03)

User reported direction oscillation and size changes. BadgerAnimator now uses Brain.FacingDirection exclusively (physics velocity only selects walking). Brain.FaceTarget uses serialized facingDeadZone=.08u; Return changes facing only when actual return motion is required. Four clips now share96x96 canvases, pivot48,21px,32PPU/Point. Editor builder transparently pads old64px Walk/Death, preserving all source pixels; Idle/Attack were already96px and unchanged. No runtime per-clip scaling, collider/attack/movement retuning, original Desktop edits or procedural art. Native silhouette differences55px Idle vs51px Walk remain authored; common canvas is not silhouette resizing. Fresh94 checks passed:78 Editor+16 actual Play, including opposite physics velocity, small target jitter, deliberate crossing and home arrival regressions. Test crossing initially required explicit Transform/physics synchronization; final run passed. Temporary tools/meta/markers/copied scene removed, original scene dirty-state comparison RESTORED; final Runtime30/Editor8 separately compile exit0. See Docs/Reports/2026-11-03-badger-facing-canvas-fix.md. Prior mixed-canvas descriptions are historical. Await user feel/art review; no next bundle/commit/push.

## Badger roam and attack cooldown implementation pending engine verification (2026-11-03)

User authorized further direction response fix, weighted home roaming and attack interruption cooldown. Added reusable EnemyRoaming component (speed.55, radius2.5, homeBias.45, move.7-1.6s, pause.4-.9s, pauseChance.25). BadgerBrain appends Roam/Cooldown states preserving enum IDs; captures original home, checks terrain, lets Recovery/Cooldown face target, consumes attackCooldown1.4s at Windup entry and retains deadline after hits. BadgerAnimator shows real Roam as Walk and applies flipX after Animator sampling. Prefab/data connections saved; Editor builder also ensures roaming attachment. Separate Runtime/Editor compilation passes. Unity has not imported latest files; real AI/physics/cooldown checks remain PENDING. Temporary validation bootstrap/driver and Logs/BadgerRoamingValidation.request are waiting for Editor refresh; they will build assets, run in a copied scene and restore original scene. Do not claim tests passed or remove pending distinction. See Docs/Reports/2026-11-03-badger-roaming-cooldown.md. No next feature/commit/push.

## Badger roaming/cooldown and visual facing verified (2026-11-03)

Supersedes pending implementation and previous flip mapping. User reports default rendered pose opposite to intended direction. Keep provided artwork untouched; calibrate BadgerAnimator.invertSpriteFacing=true, so logical right uses flipX=true and logical left=false. Apply after Animator sampling; actual camera images for left/right targets were both opened and visually confirmed. Previous numeric flip tests were insufficient to establish visual orientation. Runtime Inspector can change this calibration if artwork changes. Builder preserves calibration and sets matching prefab preview.

Fresh final107 checks passed:84 Editor+23 real Play Mode, including real AI turn responses, roam both ways with pauses/Walk near original home, weighted home policy, wall/ledge avoidance, commitment during Windup, responsive facing in Recovery/Cooldown, interrupted Windup/Active closed/no early damage or restart, repeated hit retains deadline, death. Policy1000 samples:central right498,homeward from right860/from left862 (Editor controlled policy, not physics/performance). Original scene dirty state RESTORED, temporary validators/meta/markers/copied scene removed, final Runtime31/Editor8 separately compile exit0. New reusable EnemyRoaming connected on Badger.prefab; cooldown1.4s is temporary minimum between Windup starts and survives interruption. Source PNGs, player and existing scenes/settings unchanged. See Docs/Reports/2026-11-03-badger-roaming-cooldown.md and validation/left/right evidence. Await user review; no next feature/commit/push.

## Player knockback repaired and verified (2026-11-04)

User reported absent player knockback. Actual saved PlayerBadgerCombatTest Badger instance overrode playerLayers.m_Bits=0 despite prefab256; actual runtime mask0/hits0/HP unchanged reproduced. Restore only that scene override to256 (player layer8). Also repair that exact field in an already-open original test scene without saving unrelated dirty edits. Original scene may remain dirty from the requested repair; test scene restoration comparison is after this repair. Do not claim all pre-repair dirty state unchanged.

Controlled hit through real dynamic player receiver at old(1.5,1) produced onlyX.128u/Y.0272u. Increase BadgerData knockback default and existing asset to(3.5,2.5)u/s. Preserve user's latest AttackRange1.5/AttackCooldown2s, player speed3/gravity/jump/reaction durations/drag. BadgerAttack.Sample now derives frontal reach from max(1,AttackRange), retaining former .1..1 geometry at legacy range and .65 height; current1.5 attack range no longer exceeds hardcoded1.0 hit reach. No additional Rigidbody writer or AddForce.

Final44 checks passed:7 Editor+37 actual Play, including actual AI hits from both sides, held movement input during stagger, control restore, wall blocking, compound player collider one damage, and actual hit at1.48u. Flat ground no-inputX.52u/Y.1232u, held-key casesX.486u/Y.1232u, wallX~.0515u. Controlled old-hit baseline is distinct from actual new AI attacks. Temp scripts/meta/request/ready/copied scene removed; final Runtime31/Editor8 separately compile exit0. See Docs/Reports/2026-11-04-player-knockback.md and baseline/validation logs. Full combat feel review remains pending; no next feature, commit or push.

## Player/Enemy body collision disabled; reciprocal detection verified (2026-11-04)

Existing layer8/9 named Player/Enemy (previously unnamed); Physics2D matrix disables only8↔9; all other collision pairs retained. Solid colliders enabled/non-trigger and both terrain0 collision pairs retained. No permanent functional C#/prefab/data changes. Existing OverlapBox reciprocal attacks, Brain distance/height detection, player/Badger terrain sensors retained. Applies to all bodies on Player8/Enemy9, not colliders left on Default. Ignored contact callbacks are intentionally absent; future Trigger mechanics need appropriate layers. No per-frame IgnoreCollision script. Editor validation synchronizes current physics state to persisted matrix.

Unity reserialized Physics2D native format5→11: VelocityThreshold renamed BounceThreshold(same1), newer default simulation/substep/gizmo fields serialized. Not a Unity/package upgrade. Core gravity/material/iterations/contact-offset/simulation/query/autosync values checked equal; substepping off. Do not claim only two text-bit changes in file. TagManager changes only two names.

During final audit, original saved test scene's attack mask had again becomeNothing0. Restored only toPlayer256 and synchronized same field in already-open original scene without saving unrelated dirty edits. Restored disk scene exactly matches initial snapshot; open scene may remain dirty from requested repair. Do not infer why0 reappeared.

Final repeated25 checks passed (7 Editor+18 actual Play): dynamic pass both ways/no contact/push, overlap queries find both actors, no enemy-as-ground/wall confusion, both terrain walls/landings, actual enemy detection/damage/knockback and actual player attack damage once. Test damage8/10,player knockbackX.486u. Scene state comparisonRESTORED after related-field repair. Temp scripts/meta/markers/copied scene removed; final Runtime31/Editor8 separately compile exit0. Snapshot all existingAssets/Packages unchanged,only twoSettings files changed. See Docs/Reports/2026-11-04-player-enemy-collision.md and validation.txt. Await user review; no next feature/commit/push.

## Stable actor sprite sorting verified (2026-11-04)

User reports flashing overlap. Both active actors were Default/Order0,clips only animate sprite and shared material has ZWrite0. Explicit sorting policy is Default(world)<Enemies<Player,Order0 in each layer. New Editor-only SpriteSortingSetup.ConfigurePlayer/Enemy ensures layers(always creates Enemies beforePlayer),applies prefab policy,and Apply menu updates prefab/open-scene visuals with Undo/dirty handling while preserving per-instance Order. Player/Badger animation builders invoke policy instead of copying old placeholder sorting. No per-frame runtime sorter,no Z/art/animation/physics changes. Physics GameObject Layer8/9 distinct from Sorting Layers; collision/damage settings untouched. Same-enemy per-instance ordering remains Inspector-controlled,not a unique spawned-enemy ordering system.

Fresh23 checks passed:12 Editor+11 actual Play. Three checks exercise real animation/AI;controlled Animator key sampling renders96 frame pairs across7clips and compares119764 opaque overlap pixels against player-only image,0 mismatches(tolerance1/255). Actual composite viewed. Do not describe controlled samples as physical attack-input tests. Temp helpers/meta/markers/copied scene removed;finalRuntime31/Editor9 separately compile exit0. Snapshot changed onlytwo prefabs,two existing editor builders,TagManager;newEditorhelper/meta+reports separately added. TagManager natively reserialized2→3 with defaultRenderingLayers metadata;no engine/package upgrade. Original dirty-state comparison is after requested sorting application;unsaved related instance changes may remain dirty. See Docs/Reports/2026-11-04-sprite-sorting.md and log/reference/composite images. Await user feel review;no next feature/commit/push.


## Separate greatsword DOTween presentation (2026-11-04; final engine verification pending)

Latest user request authorizes separate sword prefab from provided sword PNG and DOTween combo presentation, superseding earlier no-separate-sword visual policy only. Preserve input-relative timed damage, existing six attack assets, recovery gaps and accepted movement tuning. New PlayerSwordMotionData owns poses/eases; PlayerSwordMotion samples paused DOTween sequences with existing attack phase/progress, mirrors visual children and sorts relative to body. Only own tweens are killed on interruption/disable/death. No weapon collision/physics writer, invented art/trail/VFX/Gizmo. PlayerSwordMotionBuilder Connect Greatsword Tween menu preserves existing motion data, reapplies import/size/socket and connects nested prefab. Source PNG byte-identical, Point32/uncompressed/no mip, grip pivot(16.5,54). Latest saved size follows animated Idle30px: visible sword1.125u=1.2x, uniform scale.6206897; socket(.1,.44). This latest size correction still needs Unity import verification. Default Player prefab stays weapon locked.

First Unity run passed16 Editor+4 Play checks then failed pause fixture sampled before LateUpdate. Fixture corrected to settle one paused frame; rerun NOT passed yet. Latest source/request greatsword-tween-3 but last imported ready greatsword-tween-1. Stop Play, focus Unity and Ctrl+R needed. Permanent Runtime33/Editor10 separately compile exit0; full ground/right and air/left combos, pause/cancel/lifecycle, actual capture checks and temp helper cleanup remain PENDING. Keep SwordTweenValidation.cs/SwordTweenValidationDriver.cs/meta/request/ready until rerun, then remove and recompile. Initial scene-state RESTORED. Plugin Editor initialization added DOTWEEN defines to ProjectSettings; existing plugin files/settings byte-identical. Only old files changed from task snapshot: Player.prefab and ProjectSettings.asset. Three provided attack pose PNGs are not continuous body clips; no invented body animation, final wrist sync and hitbox/visual fit unverified. See Docs/Reports/2026-11-04-greatsword-dotween.md and preliminary validation.txt. Stop for review, no next feature/commit/push.


## First-hit smooth swing reviewed in Unity (2026-11-04)

User requests smoothing hit1, very slow initial lift then fast downswing, no mid-motion translation/hesitation, ground clearance with their resized sword. Preserve current Greatsword/BladeSprite scale=(1,1,1), sprite/import/material, existing combat/movement/timed damage and 2/3 pose data. Hit1 alone enables new continuousSwing + Inspector AnimationCurve across Windup+Active: keys(0,0),(.5,.1),(.8510638,.65),(1,1), paired tangents0/.6/3/0. PlayerSwordMotion.AppendContinuousSwing uses one DOTween and existing combat clock; no easing restart at Lift/Ready/Active. Carry/Strike/Recovery positions0; CarryZ=-80, hit1 end/recoveryZ=-280. Player prefab preview rotation updated only; Editor builder defaults match but Build menu NOT run because it reapplies sword sizing/socket. No automatic runtime size/reset/floor-clamp system or invented art/VFX/Gizmo.

Fresh Unity64 checks passed:23 Editor+41 actual Play. Native curve monotonic/speed continuity/grip checks; 4001 opaque-pixel-corner poses at current scale clear flat collider floor minimum.04646569u. Initial FullRect geometry test incorrectly included transparent canvas; corrected fixture and full rerun logged. Actual ground-right/air-left combos preserve fixed hit1 grip, pause, endpoints/sorting, one Active hit per step, recovery gaps~.22/.52, carry return, actual stagger/jump cancel, disable/reenable/death, unrelated Tween lifetime. Five camera captures opened. Original scene-state RESTORED. Previous initial sword unit had also completed16 Editor+39 Play after prior turn, superseding older pending notes. All SwordTween/FirstSwing temporary helpers/meta/markers/copied scene now removed. Final permanent Runtime33/Editor10 separately recompile exit0. Snapshot changes only motion data, Player prefab carry preview, two Runtime sword scripts, Editor sword builder; plugins/settings/scenes/movement/attack assets/user sword scale preserved. See Docs/Reports/2026-11-04-first-swing-smoothing.md and validation/captures. Body attack clip/wrist sync and 2/3 retuning not part of this unit. Await user feel review; no next feature/commit/push.


## Animation source folder preference (2026-11-04)

사용자가 애니메이션을 만들었다/개선했다고 하면 먼저 `C:/Users/pjw/OneDrive/Desktop/ETHEREAN_SPRITE`에서 해당 리소스를 찾아 Unity로 가져온다. 이후에도 이 폴더를 기본 원본 위치로 사용한다. 원본 폴더/픽셀은 수정하지 않고 프로젝트에 복사하며, 기존 경로의 이미지를 교체할 때는 meta/GUID를 유지한다. 현재 픽셀 기준은 Point 필터,32 PPU,무압축,mipmap 없음이다. 캔버스 내 위치가 다르면 클립별 고정 발/몸 기준 피벗으로 맞추고 프레임마다 자동 중앙 정렬하지 않는다. 실제 형태/프레임 수/방향을 먼저 확인하고 기존 사용자 설정과 보행의 의도된 흔들림을 보존한다. 자동 폴더 감시/상시 import 기능을 요청한 것으로 해석하지 않는다.


## Badger Walk/Death replacement completed (2026-11-04)

Imported16 native96x96 frames each from Desktop/ETHEREAN_SPRITE/오소리_Walk new and 오소리 Death, byte-identical source copies with all32 existing PNG meta GUIDs retained. Fixed per-clip pivot Walk=(46,4)px, Death=(46,21)px at32PPU; Idle/Attack=(48,21) unchanged. Walk source offset compensated +2px X / -17px image-Y relative to old walk; fixed pivot preserves authored0-1px foot variation, Death feet stay0px. No per-frame recenter/rescale/Transform animation, invented art/VFX/Gizmo or new runtime logic. BadgerAnimationBuilder.RefreshWalkAndDeath menu updates only two clips and preserves prefab/Idle/Attack/data, full Build uses same calibrated pivots. Existing invertSpriteFacing=true preserved. Animation source folder preference above remains the default for future user-made animation imports.

Actual Unity121 checks passed:107 Editor+14 Play. Real roaming Walk selection/progress/motion/facing, controlled4clip*16frame Transform sampling, real health death transition/progress/image16/final hold and anchor; controlled sampling is not physical attack input testing. Five camera captures inspected. Original scene-state RESTORED. Initial connection loss interrupted cleanup; after reconnect temp helper sources/meta/markers/copied scene removed and final permanent Runtime33/Editor10 separately compiled exit0. Snapshot changed exactly32 PNG+32 meta+one Editor builder=65 existing files; other Assets/scenes/Packages/Settings byte-identical. Clips/controller remain byte-identical because old sprite GUID paths reused. Detailed report: Docs/Reports/2026-11-04-badger-animation-replacement.md, validation.txt and5 captures. User review pending; no next feature/commit/push.


## Player first-hit body animation connected (2026-11-04)

User supplied Desktop/ETHEREAN_SPRITE/공격자세/공격 자세_1,2,3.png as three sequential frames of ONE first-hit body animation. Imported byte-identically to Player/Attack1/image_1..3.png, Point32/uncompressed/no mips/native32x32/fixed pivot(16,1), preserving authored1px foot variation. New Attack1.anim keys0/.6/.87/1.32s use poses1/2/3/3, nonlooping sprite-only; existing PlayerCharacter.controller adds Attack1 state, Idle default retained. New Editor PlayerAttackAnimationBuilder Connect First Attack Animation menu creates/reuses imports/clip/state using current GroundAttack timing; rerun reapplies key timing. Existing ground/air1 use same Attack1 state/timing ratios. No weapon/movement/hitbox/timing/prefab/2-3 body art changes.

PlayerAttackAnimator now pauses automatic Animator advancement while directly sampling AttackProgress, restores previous speed on release, and releases ownership on missing/invalid states so absent Attack2/3 cannot leave first-hit pose stuck. Existing movement presenter and sword remain separate. Actual Unity76 checks passed:15 Editor+61 actual Play, including three correct body poses and flips in ground-right/air-left, missing2/3 fallback, combo end Idle, existing combat/sword pause/hit/cancel/lifecycle regression. Three actual camera captures viewed; final wrist/blade art fit remains user review. Scene-state RESTORED, all temporary Attack1Body validators/meta/markers/copied scene removed, final permanent Runtime33/Editor11 separately compile exit0. Snapshot old-file changes only controller and PlayerAttackAnimator; new images/clip/editor tool/meta/report separately added. See Docs/Reports/2026-11-04-player-attack1-animation.md and validation/captures. Stop for review; no next feature/commit/push.


## Body attack animation deferred by user (2026-11-04)

Latest user instruction keeps the provided3 first-hit body sprites, Attack1 clip/controller state and Editor builder, but defers automatic body attack animation playback. Set only Player.prefab root PlayerAttackAnimator.enabled=false (m_Enabled0). No C# changes or asset deletion. Existing PlayerCharacterAnimator handles neutral Idle during attacks; Idle/Walk/Run, sword DOTween motion and timed combo damage remain controlled by their existing components. To resume, manually check PlayerAttackAnimator in Player prefab Inspector; do not disable CharacterVisual Animator, PlayerAttack or PlayerSwordMotion. Builders do not automatically reenable the adapter. Static comparison verified single-field prefab change and retained sprite/clip/controller bytes; no new Play test for this reversible setting. Previous76 checks describe the earlier enabled configuration. See Docs/Reports/2026-11-04-player-attack-animation-paused.md. Await user direction, no next feature/commit/push.


## Boar combat and empty animation slots completed (2026-11-04)

User authorizes boar implementation with plain square prefab and animation-ready structure, actual animation import later. New BoarData/Brain/Motor/Attack/Animator, Editor BoarCombatBuilder, Boar.prefab, BoarData.asset, Boar.controller and separate PlayerBoarCombatTest. Square=.9u, Enemy9; CharacterVisual empty/disabled at footY=-.45,scale1. Future art enables it and hides Square. Existing common health/receiver/roaming reused. Only old runtime file changed: CombatReaction gains default-false SuppressNewReactions gate in Begin and reset on disable; health/HitReceived remain active. No player/boar physical contact but overlap damage retained; existing physics/layers untouched.

Cycle Idle/Roam -> Alert -> committed Windup -> Charge -> Recovery; frontal terrain wall during Charge -> fixed Stun; ledge/time/home limit -> Recovery. Preparation/charge resist Stagger/Knockdown but take damage; Stun has armor off and its own reaction control, incoming hits do not extend deadline. Death takes priority and still falls under gravity. Swept-box previous/current charge path, once per accepted receiver per charge, player hit requests Knockdown. Windup consumes cooldown; ordinary stagger cancels into recovery without early restart. Only BoarMotor writes body velocity. Entry Debug.Log for Alert/Windup/Stun; optional user warning prefab only during Windup, defaultNone. No invented art/VFX/HUD/Gizmo.

Data tuning:HP80,damage12,knockback(5,4),body.9,detect5,engage4,home7,height1.25,return1.1,charge8,alert.6,windup.7,charge.9,recovery>=.8 plus remaining cooldown,stun1.6,cooldown2.6. All provisional. Existing player speed3/movement/sword/attack and deferred body attack animator preserved.

BoarAnimator exposes Inspector state paths and invertSpriteFacing. Controller has12 empty Motion slots:Idle/Walk/Alert/Windup/Charge/Recovery/Stun/Stagger/Death + optional KnockdownLaunch/KnockdownDown/GetUp. Recommend first6 distinct clips Idle,Walk,Windup,Charge,Stun,Death; Alert/Recovery can reuseIdle, Stagger and strong-hit visuals can follow. No fabricated animation clips. Read Docs/Design/2026-11-04-boar-animation-states.md before art import; default source Desktop/ETHEREAN_SPRITE,Point32,consistent foot anchor and real direction review.

Actual final Unity59 checks passed:22 Editor+37 Play after airborne-death fix. Real FSM/physics armor damage/direction/wall/ledge/deadlines/dedup+compound colliders/player Knockdown launch+recovery/stagger/death/disable/reenable; controlled4u sweep and high-target miss clearly distinguished. Default reaction regression passed. Scene-state RESTORED; temp validators/meta/markers/copied scene removed; final Runtime38/Editor12 separately compile exit0. Existing-file snapshot only CombatReaction changed; saved player/badger/scenes/Packages/Settings/GUIDs retained. See Docs/Reports/2026-11-04-boar-combat.md and validation/captures. Actual art/VFX/final balance/complex terrain/save/cutscene not completed. Await user review; no next feature/commit/push.


## Healing item unit completed (2026-11-05)

User authorizes healing items after boar review. New HealingItemData/PlayerInventory/NewInputHealingReader/PlayerHealing, generic HealingPickup/HealingDropData/EnemyHealingDrop, Editor HealingItemBuilder, item/action/drop profiles, plain existing-square pickup prefab and separate PlayerHealingTest. Initial policy from master2.3:ground-only slow35%, no attack/jump, max3,heal20,time1.2s; R/Gamepad dpad-up. Data values provisional, recorded in Docs/Assumptions.md. Heal and inventory responsibilities separated. CombatHealth.Heal clamps and cannot revive; Coordinator owns action priority/constraints; Locomotion.DiscardJumpRequests clears buffered and raw jump input; PlayerMovement remains sole velocity writer.

Only normal completion heals and consumes1. Completion deferred to LateUpdate after physics damage; valid HitReceived/death/control disable/airborne/full-health change cancels without spending; rejected invulnerable hit does not cancel. Valid hit cancels even when its reaction is suppressed. Ended event clears raw/buffered heal/attack/jump inputs, including inputs arriving after last FixedUpdate, so no queued restart. Optional user healing VFX prefab defaultsNone; no invented art/HUD/Gizmo. Inspect health/count/progress and Debug.Log.

Automatic trigger pickup, capacity leaves items in field, Changed event retries while truly touching, reentrancy/compound guards and live collider-distance check prevent duplicate or remote pickup. Item on Default0 trigger retains player contact despite8/9 ignore. Separate enemy drop profiles:Badger35%,Boar50%,offset(0,.1),despawn16u provisional. One death attempt;0/1 probability boundaries reliable. Only monster drops disappear when far AND outside main gameplay camera full bounds; field loot stays. Inventory count and TotalConsumed never reset on same-player death/disable, but new-player/scene/relaunch restore awaits next save/checkpoint unit; no permanent save implemented.

Final Unity59 checks passed:10 Editor+49 Play after end-frame-input and stale-contact corrections. Actual R/Gamepad/physics movement/pause/success/cancel/trigger/loot/despawn/enemy drops; controlled completion-boundary and wall policy cases distinguished. Scene-state RESTORED. Temporary HealingValidation sources/meta/markers/copied scene removed; final Runtime45/Editor13 independently compile exit0. Existing-file snapshot only7 changes:InputActions,Player/Badger/Boar prefabs,Health/Coordinator/Locomotion; no missing old files. Input JSON unchanged after filtering addedHeal and native top-levelversion1 metadata; previous IDs/maps/bindings/schemes retained. Existing movement/jump/sword/combat/animations/scenes/Packages/Settings/GUIDs and disabled body attack animator preserved. Detailed Docs/Reports/2026-11-05-healing-items.md + validation.txt. Review pending; no next feature/commit/push.


## Boar provided animation linkage and latest Death source (2026-11-05)

User provided boar Idle/Walk/Run/Death, then explicitly replaced Death with Desktop/ETHEREAN_SPRITE/멧돼지_Death_new and rejected forced size matching. Final source:멧돼지_Idle/Walk/Run + 멧돼지_Death_new,16frames each,all64x64. Byte-identical PNG copies under Art/Sprites/Boar; no resize/padding/repaint/per-frame recenter or forced Transform scaling. Point32/uncompressed/no mips/fixed pivot(32,12); retain authored0-1px foot motion and Death0-2px. CharacterVisual existing footY=-.45/scale1 and collider.9 remain; native art is wider than proxy collider and physical fit is user review, not silently retuned.

New Editor BoarAnimationBuilder connects4sprite-only clips:Idle8fps,Walk12fps,Run16fps loops;Death12fps single/final hold. Existing12logical controller states retained:movement Walk,Charge->Run,Dead->Death,Alert/Recovery->Idle loops. User now intends Windup/Stun VFX later, no VFX authored now; BoarAnimator holds Idle1 during Windup/Stun and missing reaction phases, no fabricated extra clips. Charge ordinary Run loop replaces old StateProgress-stretched playback; AI/damage clocks unchanged. Prefab hides Square/enables native art, calibrates invertSpriteFacing=false after actual right/left camera inspection. Existing warning prefab staysNone/logs remain. Prior six-new-clips/empty-Motion notes are historical; use updated design/report.

Final new-Death Unity202 checks passed:152 Editor+50 Play, full rerun after old96px Death replaced. Per64frame pixel/native size/pivot,4clip curves/loop,12alias mappings,art/physics/no-VFX checks. Real combat FSM/physics regression37 plus actual roaming and controlled display/Run loops/Idle hold/facing/death progress+final hold. Six captures saved; right/left/run/death viewed. Scene-state RESTORED. Temporary BoarAnimationValidation helpers/meta/markers/copied scene removed, final Runtime45/Editor14 separately compile exit0. Snapshot only3 oldAssets changed:Boar.controller,prefab,BoarAnimator; other gameplay/healing/player/badger/scenes/Settings/GUIDs preserved. All64 finalPNG source byte equality and source-folder hashes verified, oldDeath originals untouched. See Docs/Reports/2026-11-05-boar-animation.md + validation/captures and Docs/Design/2026-11-04-boar-animation-states.md. Await user art/feel review; no HUD/VFX/next feature/commit/push.

## Customizable player HUD prepared (2026-11-05)

User authorizes player HUD as an editable prefab. Added Runtime/UI/PlayerHud (read-only model binding and LateUpdate presentation), PlayerHudView (Text and optional Image fill references), Editor/PlayerHudBuilder (create/connect missing assets only), Prefabs/UI/PlayerHud.prefab nested under existing Player.prefab. Current health/max, healing item count/capacity, active healing progress. No invented HUD art/VFX/Gizmo: basic text is only functional default, user owns layout/fonts/images. Keep existing customized HUD on builder rerun. No health/inventory/healing/gameplay writes. Existing body attack animator remains disabled; movement speed3 and all combat tuning retained.

Standalone Runtime47/Editor15 compile exit0;19 static text/reference checks passed. Old Assets/Packages/Settings snapshot differs only by nested HUD addition to Player.prefab; original bytes otherwise retained. IMPORTANT: Unity launch failed before project import with WMI/environment assertions. HUD prefab was written as serialized YAML fallback, NOT verified by native Unity deserialization/Play/rendering. Do not claim engine tests passed. See Docs/Reports/2026-11-05-player-hud.md and validation.txt. Need actual editor import and HUD/health/healing/manual display review. No next unit, commit or push.

## HUD implementation review after user correction (2026-11-05)

Project identity is mandatory: 2D pixel-art, Unity 6.4 (ProjectVersion.txt currently 6000.4.7f1), URP and New Input System. Recheck actual project files before selecting components; do not apply a generic UI template without evaluating the installed version and pixel-art requirements.

HUD audit: PlayerHudView and PlayerHudBuilder use legacy UnityEngine.UI.Text / LegacyRuntime.ttf even though the installed com.unity.ugui includes TextMeshProUGUI. This is a component-selection/typography suitability issue, not proof that Legacy Text cannot run in Unity6.4. For future Canvas text, evaluate TextMeshProUGUI (UI) and suitable user font assets; do not confuse it with the world-space TextMeshPro mesh component. Do not silently migrate customized assets or introduce fonts/art without checking dependencies and preserving references.

ScreenSpaceOverlay Canvas is valid for a 2D HUD. Canvas planeDistance and Vector3 transforms do not make this a 3D implementation. Sprite32PPU and CanvasScaler.referencePixelsPerUnit100 describe different mappings; do not change one to the other solely on numeric mismatch. PixelPerfect=true with ScaleWithScreenSize / Match0.5 does NOT guarantee integer scaling or crisp glyphs. Validate actual font atlas/filter/material, scale and Game-view render at target resolutions before claiming pixel consistency.

The previous manually serialized HUD prefab and Player nesting were not engine-tested. Treat this as provisional integration, not a verified completion. Standalone C# and text/reference checks do not prove native loading, input behavior, placement or rendering. New Editor.log evidence shows a PlayerHud prefab import, but this alone does not establish correct appearance or Play behavior. Prefer Unity Editor APIs for asset generation; if unavailable, clearly flag provisional files and do not certify their engine validity. See Docs/Reports/2026-11-05-player-hud-review.md.

## Mandatory TMP policy — user decision (2026-11-05)

User explicitly confirms legacy Text was the HUD issue and requires TMP for all text implementations going forward. Use TMPro.TMP_Text for serialized display references and TextMeshProUGUI for Canvas UI objects. Do not create UnityEngine.UI.Text or LegacyRuntime.ttf-based UI. For this 2D HUD, use the Canvas UI component, not the world-space TextMeshPro mesh component. This is a mandatory user requirement, superseding the earlier wording to merely evaluate or prioritize TMP. Current HUD still contains legacy Text; migration is pending and must not be described as completed. Preserve existing customization, GUIDs and references when migrating, and verify TMP font dependencies and actual Unity behavior.

## HUD TMP migration completed (2026-11-05)

Mandatory TMP policy applied. Runtime/UI/PlayerHudView uses TMP_Text for all3 display fields; Editor/PlayerHudBuilder creates TextMeshProUGUI only and connects TMP_FontAsset, no legacy font fallback. PlayerHud.prefab migrated through actual Unity Editor APIs:3 legacy components replaced, View refs/font/shared material connected. Original prefab/meta GUID and all6 RectTransform layouts preserved. Player.prefab unchanged; its existing nested HUD loads3 TMP labels. Existing HUD preserved on builder rerun.

Installed existing uGUI package's TMP Essential Resources (Assets/TextMesh Pro), no examples/package upgrade/new artwork. LiberationSans SDF is a functional default, not final pixel/Korean typography. Customize Font Asset/size/color/alignment on the3 TextMeshProUGUI components. Do not claim pixel-crisp rendering at every resolution without visual tests.

Native Unity Editor8 + Play13 =21 checks passed (initial/damage/heal/inventory API display, controlled42% view,3 glyph meshes, idle progress hide, disable/enable and death zero). Not a fresh physical healing-input regression or screenshot review. Previous HUD provisional/legacy status is superseded by this native TMP migration. Test start-scene settings restored and copy removed; temporary2 validators/meta/markers removed. Final permanent Runtime47/Editor15 compile exit0. Old Assets/Packages/Settings changed only HUD prefab, View and Builder; no missing files, no gameplay changes. See Docs/Reports/2026-11-05-player-hud-tmp.md, validation.txt and resources.txt for full paths/details. User review pending; no next feature/commit/push.

## Checkpoint/save unit authorized and in progress (2026-11-05)

User explicitly authorizes checkpoints + JSON local persistence + death/reentry return + normal enemy reset. Persist latest progression, healing inventory and consumption history independently of checkpoint visits. Store checkpoint ID and scene path, NOT transient player position/velocity/AI/HP. On reentry return only position to checkpoint, preserve progress/items. New user contact/stepping wording selects trigger-entry checkpoint activation instead of older interact-only policy. No checkpoint/respawn art, code design, VFX or Gizmos. Empty Visual slot only.

Implementation in progress: 10 permanent Runtime files under Persistence plus World/Checkpoint, World/PersistentPickup and Enemies/EnemySpawnPoint; exact list in Docs/Reports/2026-11-05-checkpoint-save.md. Editor CheckpointBuilder creates missing prefabs/test scene, adds PlayerSaveBinding/PlayerRespawn to Player; does not overwrite existing customization. EnemySpawnPoint is a scene-authored stationary anchor and spawns fresh prefab instances on rest/death (health/AI/cooldown/home/drop reset), rather than capturing moved runtime positions. PersistentPickup saves partial field quantity to prevent reentry duplication. JsonSaveStore temp/backup/version/corrupt protection; events coalesced end-frame, pause/quit flush; Editor and build slots separate.

IMPORTANT CURRENT STATUS: Runtime/Editor standalone compile passed, but Unity has NOT imported the new validators; no CheckpointValidation.ready/result exists yet. Prefabs and new scene are NOT generated yet because builder must run in Unity. Current task is incomplete, not awaiting implementation approval. User must open/focus Unity with Play stopped and Ctrl+R so queued authorized validation can execute. Logs/CheckpointValidation.request is present. Temporary Scripts/Editor/CheckpointValidation.cs and Runtime/Persistence/CheckpointValidationDriver.cs are pending native tests, not permanent architecture. Test helper builds assets, tests file cases and two fresh Play sessions with a unique isolated slot. After success remove helper sources/meta/request/ready/copied CheckpointTemporary scene and isolated test saves, restore Editor play-start setting, recompile permanent Runtime57/Editor16 and update report. Do not erase user save slots. Do not start forest work or commit/push. Backup/snapshot/compiler logs: C:/Users/pjw/AppData/Local/Temp/EthereanCheckpoint20261105. Latest PlayerRespawn.OnDisable coroutine cleanup was added after last compile and needs final compile/native tests.

## Chair checkpoint policy (2026-11-05)

The latest user decision supersedes trigger-entry activation and automatic disk writes: checkpoint is a chair. A fresh Player/Interact press within reach seats and saves; the next press stands. E / gamepad north, no hold required for chairs. SeatPoint and SpawnPoint are separate, Visual is for user art; do not fabricate art or animation. Death retains current session progress and items; reopening restores last seated JSON snapshot. See Docs/Reports/2026-11-05-checkpoint-chair.md. Previous checkpoint version did pass Unity Editor16 / Play52; the chair rerun is queued with marker chair-2 and uses an isolated slot. Remove temporary validation scripts/assets/markers once verified.

Chair checkpoint completed: native Unity Editor17 + Play41 + Play20 = 78 checks passed, including real Input System E events and fresh Play save restoration. Test scene setting restored; temporary scripts/meta, markers, scene and isolated save removed. Final Runtime/Editor standalone compile exit 0. See Docs/Reports/2026-11-05-checkpoint-chair.md and validation log.

## First forest whitebox (2026-11-05)

User approved a large forest whitebox using current player scale and jump. Added ForestWhitebox.unity (141.55u x 41u camera bounds, 75 terrain blocks, A-I continuous loop), ForestWhiteboxCamera and ForestWhiteboxRoute. Prefab player/bench/enemy references reused, scene-specific save slots. Whitebox progression: D defeat then E interact, E opens C-E, F unlocks wall jump/slide, H returns to B, I records exit. Current fixed-step jump measured in Unity at 1.352u, configured gaps 1.0-1.25u and G/I wall spacing 1.45u. Native Editor11 + Play15 passed; full route/feel and sequential shaft traversal not yet playtested. Temporary native test scripts/markers/copy removed; final Runtime compile passed. Report and coordinate map: Docs/Reports/2026-11-05-forest-whitebox.md. Await user review before next feature.
