# 가속·감속과 벽차기 완화 — 2026-11-03

## 1. 요청과 상태

현재 게임에서 가속/감속과 벽차기의 힘이 강하다는 사용자 피드백에 따른 수치 조정이다. 기존 속도 계산·입력·벽차기 정책은 유지한다. 현재 구현/별도 컴파일 완료, Unity 최종 검사 대기다.

## 2. 실제 값과 변경 이유

| 항목 | 이전 프리팹 | 새 초기값 |
| --- | ---: | ---: |
| PlayerMovement.acceleration | 40 | 18 |
| PlayerMovement.deceleration | 60 | 24 |
| PlayerWallJump.wallJumpHorizontalSpeed | 8 | 4.5 |
| PlayerWallJump.wallJumpVerticalSpeed | 10 | 9 |

최고 수평 속도 6, 일반 점프 10, 상승/하강 중력 40/65, 낙하 상한 16은 유지한다. 기존 PlayerCombatTest에는 가속/감속 1000/1000 인스턴스 override가 있어 프리팹 변경을 가렸다. 이번 사용자 요청에 따라 두 override만 해제해 공통 튜닝값을 따르게 한다. 다른 기존 씬이나 override를 일괄 초기화하지 않는다. 특히 과거 PlayerMovementTest의 wallJumpHorizontalSpeed=4는 유지한다.

## 3. 수정 파일과 위치

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerMovement.cs`: acceleration/deceleration 초기값 18/24.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerWallJump.cs`: wallJumpHorizontalSpeed/wallJumpVerticalSpeed 초기값 4.5/9.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerMovementFeelSceneBuilder.cs`: ApplySoftMovementTuning 추가. 프리팹 네 수치를 저장하고 PlayerCombatTest의 두 가감속 override만 해제한다. 이미 다른 변경이 있는 dirty 씬은 함께 저장하지 않고 편집 상태를 유지한다. ApplyTuning의 벽차기 수직값도 9로 맞춰 이전 튜닝 메뉴가 새 값을 되돌리지 않게 한다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab`: PlayerMovement/PlayerWallJump의 위 네 Inspector 값.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scenes/Tests/PlayerCombatTest.unity`: 플레이어의 acceleration/deceleration 인스턴스 override만 해제 대상.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md`, `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md`: 이번 최신값·적용·검증·리뷰 상태 기록.

## 4. 추가·제거 파일

영구 추가 파일은 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-soft-movement-and-wall-jump.md`뿐이다. 영구 런타임 컴포넌트·씬·프리팹·에셋은 새로 추가하지 않는다.

검증용 `Assets/_Project/Scripts/Editor/SoftMovementValidation.cs`와 `Assets/_Project/Scripts/Runtime/Player/SoftMovementValidationDriver.cs`, meta·request/ready·복사 씬은 검사 후 제거할 임시 파일이다. 기존 영구 파일 삭제나 이름 변경은 없다.

## 5. 실행 원리와 설계

입력 → 기존 PlayerLocomotion → PlayerMovement.CalculateHorizontalSpeed가 새 가속/감속으로 목표 속도에 접근 → PlayerMovement만 Rigidbody2D에 속도를 적용한다. 반대 방향 입력은 먼저 감속해 0을 지난 후 새 방향으로 가속한다. 별도 입력 지연을 넣지 않는다.

벽차기는 PlayerWallJump가 수평 4.5·수직 9의 발사를 제안하고, 기존 PlayerMovement가 적용한다. 0.1초 수평 제어 보호가 남아 있어 벽 방향 입력으로 즉시 발사가 취소되지 않는다. 벽 코요테·상승 중 사용·분리 후 같은 벽 재사용 정책은 유지한다.

최고 속도가 같아도 도달하는 시간이 길어지고, 놓았을 때 조금 더 부드럽게 멈춘다. 지나친 미끄러짐이 생길 수 있다는 트레이드오프가 있어 감속은 가속보다 높게 유지했다. 모든 새 수치는 최종 확정 전 초기 튜닝값이다.

## 6. 연결과 직접 확인

새 맵 `Assets/_Project/Scenes/Tests/PlayerMovementFeelTest.unity` 또는 기존 PlayerCombatTest에서 확인한다. 새 맵 구조와 연결은 변경하지 않는다.

정지 상태에서 D를 눌러 서서히 출발 → 놓아서 감속 → 달리는 중 A를 눌러 방향 전환 → 오른쪽 통로에서 벽차기 → 벽에서 떨어진 직후 점프 → 분리/재접촉 후 같은 벽 반복 → 충분한 도움닫기로 2u 틈 통과 순서로 확인한다.

Inspector에서 PlayerMovement의 Acceleration=18, Deceleration=24, PlayerWallJump의 Horizontal=4.5, Vertical=9를 조정할 수 있다. 최고 속도와 일반 점프 수치는 별도 필드다. 생성/튜닝 메뉴는 초기값 재적용 도구이므로 사용자 수동 튜닝 중 반복 실행할 필요가 없다.

## 7. 검증

별도 C# 컴파일 오류 0. 실제 저장 값과 씬 상속, 입력 기반 가속/정지 시간, 방향 전환, 벽차기 발사/보호/상승 중 재접촉/코요테 및 새 맵 2u 틈 통과는 최종 Unity 로그를 확인해 기록한다. 이론상 0→6 가속 시간은 6/18=0.333s, 6→0 정지 시간은 6/24=0.25s이며 실제 실행 측정과 구별한다.

## 8. 남은 사항

최종 물리 키보드/게임패드의 조작감, 다양한 레벨의 난이도는 사용자 리뷰가 필요하다. 기존 공격 데이터·재정비·입력·캐릭터 크기·사용자 충돌체·점프 버퍼/컷 알고리즘은 변경하지 않는다. 변경 중 편집 중인 씬이 dirty라면 관련 override는 메모리에서 조정하되 다른 변경을 저장하지 않으며 최종 상태를 별도 보고한다.

## 9. 리뷰

출발이 둔해졌는지, 정지 시 미끄러짐이 과한지, 벽차기 횡방향 거리가 적절한지 사용자 검토 후 다음 단위를 결정한다. 커밋·push는 하지 않는다.


## Superseded by latest user decision
Acceleration/deceleration are now removed entirely. Keep the applied wall launch 4.5/9. See Docs/Reports/2026-11-03-direct-horizontal-movement.md for the current policy and validation status.
