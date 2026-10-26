# 플레이어 가속·감속 검토 — 2026-10-22

## 목표와 상태

기존 좌우 이동에 약간의 관성을 추가하고, 가속도·감속도를 Inspector에서 설정하도록 완료했다. 프리팹 저장과 Unity Play Mode 검사 22개에 통과했다. 이 수정의 사용자 검토를 기다린다.

## 변경 파일

프로젝트 루트: `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN`.

| 경로 | 변경 |
| --- | --- |
| `Assets/_Project/Scripts/Runtime/Player/PlayerMovement.cs` | acceleration·deceleration 직렬화 필드, 점진적 속도 변경, 방향 전환 제동 |
| `Assets/_Project/Prefabs/Player/Player.prefab` | Editor API로 가속도 40, 감속도 60 저장. 기존 참조와 GUID 보존 |
| `PLAYER_SETUP.md` | 현재 이동 방식, Inspector 수치와 미래 스탯 공급 방향 설명 |
| `AGENTS.md` | 승인된 수정 범위와 사용자의 미래 스탯 설계 방향 기록 |
| 이 보고서 | 실행 흐름, 검증 근거, 한계와 리뷰 지점 기록 |

기존 입력 계약과 New Input System Reader를 재사용한다. 임시 검증 코드와 요청 파일은 검증 후 제거했다.

## 실행 흐름과 책임

NewInputMovementReader → HorizontalInput → PlayerMovement.FixedUpdate → 목표 속도 계산 → 가속·감속 선택 → Rigidbody2D X속도 적용 순서다.

목표 속도는 수평 입력에 최대 이동 속도를 곱한다. 현재 속도보다 목표 속도의 크기가 작으면 감속도를 사용한다. 입력이 현재 속도의 반대 방향이면 우선 0을 목표로 감속하고, 정지한 다음 새 방향으로 가속한다. 나머지 경우 가속도를 사용한다.

`Mathf.MoveTowards`가 한 번에 바꿀 수 있는 속도는 `changeRate × Time.fixedDeltaTime`이다. 가속도(u/s²)에 고정 시간 간격(s)을 곱하여 속도 변화량(u/s)을 얻는다. 계산한 속도를 Rigidbody2D에 적용할 때는 시간 간격을 다시 곱하지 않는다. Y속도는 보존한다.

입력을 해제하거나 Reader를 비활성화하면 감속하여 정지한다. PlayerMovement 자체를 비활성화하면 기존 OnDisable 정책대로 수평 속도가 즉시 0이 된다.

## Inspector와 임시값

| 항목 | 필드 | 초기값 | 조절 효과 |
| --- | --- | --- | --- |
| Horizontal Move Speed | horizontalMoveSpeed | 6u/s | 목표 속도 상한 |
| Acceleration | acceleration | 40u/s² | 높을수록 빠르게 출발 |
| Deceleration | deceleration | 60u/s² | 낮을수록 오래 미끄러짐 |

가속·감속의 Inspector 최소값은 0.01이다. 지상과 공중에 동일하게 적용한다. 이 값은 조작감 확인용 임시값이며 최종 밸런스 수치가 아니다.

## 설계 근거와 트레이드오프

속도 적용 주체를 PlayerMovement 한 곳에 유지하여 기존 구조를 보존했다. 입력 시스템에는 가속·감속 책임을 넣지 않았다. Rigidbody2D의 전체 Damping으로 관성을 만들면 수직 운동에도 영향을 줄 수 있으므로 X속도 변화량을 직접 조절한다.

이번 규모에는 별도 가속 인터페이스나 상태 머신이 필요하지 않다. 방향 전환은 감속 후 가속하는 정책을 택했으므로 즉시 반전보다 반응이 느리다. 사용자는 가속·감속 수치를 조절해 무게감을 검토한다.

사용자의 장기 방향은 최종 이동 속도를 캐릭터 스탯에서 공급하는 것이다. 스탯은 수치 계산을, PlayerMovement는 이동 실행을 담당하도록 연결한다. 현재 horizontalMoveSpeed는 임시 공급 값으로 유지한다. 구체적인 스탯 클래스, 보정 계산과 전달 방식은 해당 기능의 검토 단위에서 결정한다.

## 검증 근거

로컬 Unity 참조 어셈블리를 사용한 별도 Roslyn C# 컴파일에 통과했다. Inspector가 할당하는 기존 Reader 필드의 CS0649 경고 외에 컴파일 오류는 없었다.

Unity에서는 임시 Editor 검증 코드로 PrefabUtility.LoadPrefabContents, SerializedObject, PrefabUtility.SaveAsPrefabAsset을 호출해 두 수치를 저장했다. 이후 테스트 씬에서 Play Mode 검증을 수행한다.

가속·제동의 정확한 변화량은 실제 PlayerMovement.FixedUpdate를 제어된 호출로 실행하여 검사한다. 이어 자동 FixedUpdate와 실제 물리 시뮬레이션으로 이동, 정지, Reader 비활성화, 공중 이동과 벽 충돌을 확인한다. 이 두 종류의 검사를 구분하며 전부를 실제 시간 측정이라고 표현하지 않는다.

검증 결과: 22개 모두 통과했다. 앞의 15개는 Inspector 직렬화와 제어된 FixedUpdate 호출 검사이며, 뒤의 7개는 자동 FixedUpdate와 실제 물리 검사다. 로그는 `Logs/PlayerAccelerationValidation.txt`에 있다.

| 분류 | 확인 결과 |
| --- | --- |
| 프리팹 설정 | 가속 40·감속 60 로드 |
| 가속 | 0.02초 간격 호출에서 0→0.8u/s, 최대 속도 6u/s에 초과 없이 도달 |
| 제동 | 6→4.8u/s, 입력 해제 후 반대 방향으로 넘어가지 않고 정지 |
| 방향 전환 | 기존 방향 제동 → 정확히 0 도달 → 새 방향 가속 |
| Inspector 변경 | 가속 20이면 첫 변화 0.4u/s, 감속 30이면 6→5.4u/s |
| 목표 속도 감소 | 6에서 목표 3으로 낮추면 감속도 사용 |
| 시간 간격 | 0.01초에서는 가속 변화량 0.4u/s. 검사 후 원래 0.02초 복원 |
| 기존 입력 보정 | W+D의 최대 수평 속도 6u/s 유지 |
| 수직 운동 | 가속과 방향 전환이 Y를 덮어쓰지 않음 |
| 실제 이동 | 자동 FixedUpdate로 가속하여 이동하고 입력 해제 후 정지 |
| 입력 비활성화 | Reader를 끄면 감속 후 정지 |
| 공중 | 낙하하며 수평 가속 가능 |
| 강제 비활성화 | PlayerMovement를 끄면 X 즉시 정지 |
| 충돌 | 벽 통과 방지와 회전 고정 유지 |

자동 입력 장치와 임시 실행 설정은 정리·복원했고 Play Mode를 종료했다. 실제 입력 장치의 조작감은 사용자 직접 확인 항목이다.

## 직접 검토할 방법

1. `Assets/_Project/Scenes/Tests/PlayerMovementTest.unity`를 연다.
2. Player의 Player Movement에서 Acceleration과 Deceleration을 확인한다.
3. Play 후 Game 뷰를 클릭하고 D를 누르다가 놓는다. 출발과 정지의 속도 변화가 보여야 한다.
4. D로 이동 중 A를 누르면 제동 후 왼쪽으로 가속해야 한다.
5. Play 중 Acceleration을 20으로 낮춰 출발을, Deceleration을 30으로 낮춰 정지를 비교한다. 플레이 중 변경은 종료 후 복원된다. 유지할 값은 편집 모드의 프리팹에서 설정한다.
6. Reader를 끄면 감속하고, PlayerMovement를 끄면 즉시 정지하는 차이를 확인한다.

## 한계와 다음 검토

게임패드의 실제 조작감은 별도 확인 항목이다. 공중 전용 가속·감속, 외부 넉백 우선순위, 캐릭터 스탯 시스템은 이번 단위에 포함하지 않는다.

가속 40·감속 60의 관성이 적절한지 사용자 검토를 기다린다. 이 수정 검토가 끝난 뒤 바닥 감지·단발 점프를 다음 작은 단위로 제안하며 자동으로 시작하지 않는다.
