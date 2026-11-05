# 듀얼 그리드 테스트 씬: 오른쪽 통과와 점프 판정

`DualGridTerrainTest`의 오른쪽 진행로 위 발판 밑면은 y=1u, 본길 바닥은 y=0.5u였다. 남은 높이 0.5u는 플레이어 캡슐 높이 0.8845u보다 작아 발판 아래를 걸을 수 없었다. 이동 애니메이터는 실제 수평 속도로 Idle/Walk/Run을 고르므로, 장애물에 막혀 속도가 떨어지면 오른쪽 키를 누르고 있어도 Idle이 될 수 있다. 이 씬에는 별도 애니메이션 설정이나 좌우 키 오버라이드가 없었다.

| 파일 | 수정 위치와 동작 |
| --- | --- |
| `Assets/_Project/Scenes/Tests/DualGridTerrainTest.unity` | `Dual Grid Sample/Terrain Source - Paint Here`의 낮은 공중 발판 타일 3개(x=-4~-2)를 셀 y=2에서 y=6으로 옮겼다. 발판 아래 여유는 0.5u에서 2.5u가 됐다. 다른 타일, 지형 루트, 플레이어 및 카메라 값은 유지했다. |
| `Assets/_Project/Scripts/Editor/DualGridTerrainSetup.cs` | `OpenSample`의 새 씬 생성에도 발판 높이를 반영했다. 이미 저장된 씬을 여는 동작은 그대로다. |
| `Assets/_Project/Scripts/Runtime/Player/Movement/PlayerGroundSensor.cs` | `CheckGrounded(velocity)`가 바닥 레이어·각도에 맞는 실제 접촉을 먼저 검사한다. 표면에서 떨어지는 중이 아니면 접지로 보고, 이 조건을 만족하는 접촉이 없을 때는 기존의 하향 캐스트를 사용한다. |
| `Assets/_Project/Scripts/Runtime/Player/Movement/PlayerLocomotion.cs` | `CalculateVelocity`에서 속도를 센서로 넘긴다. 충돌이나 경사 때문에 수직 속도가 잠깐 양수가 되어도 실제 지면 접촉을 검사하고, 의도적으로 점프해 표면에서 멀어질 때는 접지로 되돌리지 않는다. |
| `Docs/Reports/2026-11-05-dual-grid-movement-fix.md` | 새 기록. 재현 조건, 변경 이유 및 확인 절차를 남긴다. |

애니메이션 클립, 방향 전환, 입력 바인딩, 이동 속도, 점프 높이와 코요테·버퍼 시간은 변경하지 않았다. 기존 0.5u 단차와 1u 틈은 점프를 확인할 수 있도록 남겼다. 바닥 판정 변경은 공용 플레이어에 적용되므로 일반 평지, 단차, 경사에서 확인한다.

1. `DualGridTerrainTest`를 Play하고 오른쪽으로 이동한다. 첫 단차는 Space로 넘고, 위쪽 발판 아래에서 좌우로 왕복하며 이동 애니메이션을 확인한다.
2. 같은 발판 아래에서 멈춘 상태와 D를 누른 상태로 각각 Space를 누른다. 천장에 부딪히지 않고 시작하는지 확인한다.
3. 발판 아래 평지와 타일 경계에서 연속으로 착지 후 점프한다. 공중에서 별도 점프가 새로 나가지 않는지, 공중 이동 후 착지하면 이동 애니메이션이 다시 재생되는지 본다.

씬의 타일 좌표와 발판 여유 높이는 파일에서 확인했다. Unity 6.4.7f1 프로젝트 참조로 Runtime/Editor C# 컴파일을 검증했다. Unity 배치 실행은 프로젝트 초기화에서 종료돼 실제 Play 재현과 수정 결과는 위 순서로 확인해야 한다.
