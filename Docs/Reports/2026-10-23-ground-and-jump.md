# 바닥 감지·단발 점프·코요테 타임 — 2026-10-23

## 1. 목표와 완료 여부

바닥 감지, 수직 속도를 직접 제어하는 단발 점프, 코요테 타임을 완료했다. 기존 좌우 가속·감속을 유지했다. Unity 6000.4.7f1에서 Play Mode 검사 37개를 통과했고 사용자 검토를 기다린다.

## 2. 추가·수정 파일과 연결

프로젝트 루트: `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN`.

| 구분 | 경로 | 내용 |
| --- | --- | --- |
| 추가 | `Assets/_Project/Scripts/Runtime/Player/PlayerGroundSensor.cs` | CapsuleCollider2D 아래 방향 Cast, 레이어·트리거·법선 필터 |
| 수정 | `Assets/_Project/Scripts/Runtime/Player/PlayerMovementInput.cs` | 한 번 소비하는 ConsumeJumpPress 계약 |
| 수정 | `Assets/_Project/Scripts/Runtime/Player/NewInputMovementReader.cs` | 기존 Jump 액션 참조·복제·performed 입력 보관·정리 |
| 수정 | `Assets/_Project/Scripts/Runtime/Player/PlayerMovement.cs` | 접지 상태, 점프 권한, 코요테 타임, 직접 수직 속도 계산, 중력 소유권 |
| 추가 | `Assets/_Project/Data/Player/PlayerJumpAction.asset` | 기존 Player/Jump 액션 참조 |
| 수정 | `Assets/_Project/Prefabs/Player/Player.prefab` | 센서와 점프 참조, 조정 가능한 수치 연결 |
| 수정 | `Assets/_Project/Scenes/Tests/PlayerMovementTest.unity` | 높은 발판과 낮은 천장 추가 |
| 수정 | `Assets/_Project/Scripts/Editor/PlayerMovementTestSceneBuilder.cs` | 새 구성과 테스트 지형 생성 지원 |
| 수정 | `PLAYER_SETUP.md`, `AGENTS.md` | 사용법, 현재 검토 범위와 결과 |
| 추가 | 이 보고서 | 동작 원리·설계·검증·직접 확인 절차 |

새 에셋과 스크립트에 대응하는 .meta를 포함한다. 기존 프리팹·씬·입력 에셋의 GUID와 기존 Move/Jump 액션 ID는 보존했다. PrefabUtility, SerializedObject, EditorSceneManager를 사용해 에디터에서 연결·저장했다. 프로젝트 설정이나 패키지를 변경하거나 커밋·푸시하지 않았다.

Inspector 연결: Reader의 Jump Action Reference → PlayerJumpAction, Movement의 Ground Sensor → 같은 오브젝트의 PlayerGroundSensor. 기존 Movement Input Source와 Move Action Reference를 유지했다.

## 3. 실행 흐름과 상태

Space 입력 → 복제된 Jump 액션 performed 이벤트 → Reader가 요청 보관 → FixedUpdate가 ConsumeJumpPress로 한 번 소비 → 바닥/코요테/점프 권한 검사 → Y속도 설정 → 물리 충돌·착지 순서다.

| 상황 | 결과 |
| --- | --- |
| 하강 또는 정지 중 유효 바닥 감지 | IsGrounded=true, 마지막 지상 시각 갱신, 점프 권한 복구 |
| 지상에서 새 점프 입력 | Y=Jump Speed, 즉시 공중 상태로 전환 |
| 바닥 이탈 후 Coyote Time 이내 + 미사용 점프 권한 | 공중에서도 한 번 점프 |
| 코요테 타임 만료 또는 이미 점프 실행 | 새 공중 입력을 버림 |
| 점프 실행 | 점프 권한과 코요테 기록을 즉시 소비하여 중복 점프 차단 |
| 상승 중 | Rise Gravity × fixedDeltaTime만큼 Y 감소 |
| 정점 이후 공중 | Fall Gravity × fixedDeltaTime만큼 Y 감소, Max Fall Speed 제한 |
| 천장 충돌 | 물리 엔진이 상승을 막고 이후 낙하. 점프 권한을 복구하지 않음 |
| 착지 | 다음 새 누름에 대한 점프 권한 복구. 누르고 있던 키로 자동 점프하지 않음 |

Y속도는 AddForce로 누적하지 않고 jumpSpeed로 설정한다. 상승·낙하 가속도를 코드에서 적용하므로 Movement 활성 중 Rigidbody2D.gravityScale을 0으로 두어 기본 중력이 중복 적용되지 않게 한다. Movement 비활성화 시 원래 중력 배율을 복구한다.

GroundSensor는 접촉 여부를 검사할 뿐 속도를 변경하지 않는다. PlayerMovement가 수평·수직 속도를 계산해 Rigidbody2D.linearVelocity에 한 번 적용한다. 상승 중에는 발밑 센서가 가까운 바닥을 보고 있어도 지상으로 처리하지 않는다.

## 4. 설계 이유와 선택

바닥 감지를 별도 컴포넌트로 분리하여 지형 검사와 이동 실행의 책임을 나눴다. 기존 추상 입력 계약에 점프 요청 소비를 추가하고 실제 입력은 New Input System Reader가 제공한다. 새로운 범용 FSM이나 인터페이스 계층은 만들지 않았다.

Jump performed 이벤트는 다음 물리 업데이트까지 한 번 보관한다. 렌더 프레임 사이의 짧은 입력을 놓치지 않으면서 같은 요청을 여러 FixedUpdate에서 재사용하지 않는다. 이는 착지 전 입력을 일정 시간 보관하는 점프 버퍼와 다르며, 유효하지 않은 공중 입력은 그 물리 업데이트에서 버린다.

낙하 가속도를 상승 가속도보다 크게 두어 올라갈 때와 떨어질 때의 감각을 구분한다. 현재는 고정 높이 단발 점프다. 누르는 시간에 따른 높이 조절은 별도 검토 단위다. 최대 이동 속도를 이후 캐릭터 스탯에서 공급한다는 기존 설계 방향도 유지한다.

## 5. 검증 근거

- 로컬 Unity 참조 어셈블리를 사용한 Roslyn C# 컴파일에 통과했다. Inspector 참조 필드에 대한 CS0649 경고와 컴파일 오류를 구분했다.
- 열린 ETHEREAN 에디터에서 프리팹과 씬을 갱신하고 Play Mode에 진입했다.
- 바닥 센서 검사는 실제 Collider2D와 Physics2D 쿼리를 사용했다. 30도 경사 허용·60도 경사 제외는 센서 판정 검증이며 경사면 주행 튜닝 검증은 아니다.
- 점프·코요테·중력·충돌 검사는 실제 FixedUpdate와 물리 시뮬레이션에서 수행했다. 임시 Keyboard 이벤트를 사용하고 물리 입력 장치가 검증에 섞이지 않게 한 뒤 원상복구했다.
- 1280×720 카메라 렌더에서 플레이어·발판·천장·바닥·벽을 확인했다.
- 임시 검증 코드와 요청 파일을 제거했고 Play Mode를 종료했다. 최종 로그는 `Logs/PlayerJumpValidation.txt`에 있다.

결과: 37개 모두 통과.

| 분류 | 확인 결과 |
| --- | --- |
| 연결 | Movement·Reader·Sensor 구성, Jump 참조, 누락 스크립트 없음 |
| 바닥 | 평지, 자기 충돌체 제외, 트리거 제외, 레이어 마스크 적용 |
| 표면 | 벽·천장 제외, 30도 경사 허용, 60도 경사 제외 |
| 기본 점프 | 상승 중 지상 해제, 착지 복구, 측정 높이 약 3.120u |
| 단발성 | 공중 재누름 차단, 코요테 중복 사용 차단, 누른 채 착지 시 자동 반복 없음 |
| 재사용 | 착지 후 새 누름으로 다시 점프 |
| 코요테 | 발판 이탈 직후 허용, 만료 후 차단, 0 설정 시 이탈 점프 차단 |
| 입력 예외 | 공중 시작 무료 점프 없음, 무효 입력의 착지 예약 없음, 짧은 누름 한 번 보존 |
| 천장 | 상승 차단 후 낙하, 접촉으로 점프 권한 복구 없음 |
| 낙하 | 20u/s 상한, 그 속도로 떨어져도 바닥을 통과하지 않고 착지 |
| 기존 이동 | 점프 중 수평 가속·감속 유지 |
| 비활성화 | Reader 비활성화 시 미소비 점프 제거, 누른 채 재활성화해도 자동 점프 없음 |
| 중력 소유권 | 활성 중 중복 중력 없음, 비활성화 시 원래 중력 복구, 재활성화 정상 |

테스트 지형 추가 시 BoxCollider2D가 빈 SpriteRenderer 크기를 기준으로 작게 생성되는 문제를 발견했다. 스프라이트를 먼저 설정하고 충돌체 크기를 명시하여 수정했으며 최종 검증에서 발판·천장·코요테 동작을 확인했다.

## 6. 직접 확인

1. `Assets/_Project/Scenes/Tests/PlayerMovementTest.unity`를 열고 Game 뷰를 16:9로 둔다.
2. Play 후 Game 뷰를 클릭한다. A/D 또는 방향키로 이동하고 Space로 점프한다.
3. Space를 누르고 있어도 한 번만 점프하는지, 공중에서 다시 눌러도 추가 점프하지 않는지 확인한다.
4. 왼쪽 초록 발판으로 올라가 가장자리에서 걸어 나온 직후 Space를 눌러 코요테 타임을 확인한다. 잠시 기다린 뒤 누르면 점프하지 않아야 한다.
5. 오른쪽 갈색 천장 아래에서 점프하면 상승이 막히고 내려와야 한다.
6. Player Movement의 Jump Speed, Coyote Time, Rise Gravity, Fall Gravity, Max Fall Speed를 변경해 비교한다. 유지할 수치는 편집 모드의 프리팹에서 설정한다.

## 7. 임시값과 한계

| 항목 | 임시값 |
| --- | --- |
| Jump Speed | 12u/s |
| Coyote Time | 0.1초 |
| Rise Gravity / Fall Gravity | 24 / 36u/s² |
| Max Fall Speed | 20u/s |
| Ground Check Distance | 0.05u |
| Max Ground Angle | 50도 |

현재 설정의 측정 높이 3.120u는 최종 확정 높이가 아니다. 고정 시간 간격과 충돌체·센서 여유에 따라 실측값이 달라질 수 있다. 지상·공중 수평 가속·감속은 기존 같은 값을 사용한다. 실제 게임패드 조작감은 별도 확인 항목이다.

## 8. 검토할 부분

상승 속도와 더 빠른 낙하가 원하는 무게감에 맞는지, 코요테 타임 0.1초가 충분히 관대한지 사용자 검토를 기다린다.

## 9. 다음 작업 추천

짧게 누르면 낮게 뛰고 길게 누르면 높게 뛰는 가변 점프를 다음 작은 작업으로 제안한다. 점프 버퍼와 함께 자동으로 구현하지 않으며 사용자 지시 후 진행한다.
