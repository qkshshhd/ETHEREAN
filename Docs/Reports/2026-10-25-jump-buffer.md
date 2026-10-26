# 점프 버퍼 — 2026-10-25

## 1. 목표/완료 여부

착지 직전에 누른 점프를 짧게 보관하고, 착지 또는 유효한 코요테 조건에서 한 번 실행한다. 완료: 코드·프리팹 저장·별도 C# 컴파일을 완료했고 Unity 검사 44개(편집 모드 6개 + 실제 Play Mode 38개)를 통과했다. 사용자 검토를 기다린다. 앞서 제안한 다음 작업에 대한 사용자 진행 지시로 이 단위를 구현한다.

기준: 마스터 프롬프트 2.1의 점프 버퍼, GDD 5.6의 가변 점프·코요테 타임을 실제 이동 성능과 함께 검증한다는 원칙. 버퍼 0.1초는 최종 기획 확정값이 아닌 임시값이다.

## 2. 추가·수정 파일

프로젝트 루트: `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN`.

| 파일 | 변경 이유 |
| --- | --- |
| `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Runtime\Player\PlayerMovement.cs` | jumpBufferTime, 마지막 입력 시각과 해제 상태, 예약 갱신·만료·실행·취소 |
| `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Runtime\Player\PlayerMovementInput.cs` | InputDisabled 이벤트와 보호된 알림 메서드 |
| `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Runtime\Player\NewInputMovementReader.cs` | 비활성화 시 대기 입력을 정리한 뒤 InputDisabled 알림 |
| `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Prefabs\Player\Player.prefab` | Unity Editor API로 jumpBufferTime=0.1 저장 완료 |
| `PLAYER_SETUP.md`, `AGENTS.md` | 현재 검토 단위·사용자 조정값·동작 원리 |
| `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Docs\Reports\2026-10-25-jump-buffer.md` | 이 보고서 |

씬·Input Actions·기존 참조와 GUID를 보존한다. 기존 씬의 Player 프리팹 연결로 새 필드를 사용한다. 사용자가 조정한 jumpSpeed=16, riseGravity=42, coyoteTime=0.2와 센서 레이어 설정을 덮어쓰지 않는다. 새 컴포넌트의 코드 초기값 12/24/0.1과 현재 프리팹 수치를 구분한다.

## 3. 실제 구현 흐름

1. Reader가 New Input System의 Jump performed/canceled를 다음 물리 틱까지 보관한다.
2. PlayerMovement.CalculateVerticalSpeed가 바닥 상태를 갱신한 뒤 입력을 소비한다.
3. UpdateJumpBuffer는 새 누름의 Time.fixedTime을 기록한다. 새 누름이 오면 기존 예약 한 건을 교체하며 키 해제 여부도 새 요청 기준으로 갱신한다.
4. 유효 시간이 지난 예약은 ClearJumpBuffer로 삭제한다. 기본 0.1초이며 0 설정에서는 새 입력을 받은 현재 물리 틱에서만 허용한다.
5. 예약이 유효하고 점프 권한이 있으며 바닥 또는 유효 코요테 조건이면 기존 점프를 실행한다. 실행 즉시 예약과 코요테 권한을 소비한다.
6. 예약 중 미리 키를 놓았다면 그 사실을 유지하여 점프 시작 속도를 한 번 줄인다. 누르고 있다면 최대 높이 점프를 시작한다.
7. 이후 기존 상승/낙하 중력·최대 낙하 속도·수평 가속·감속을 적용한다. 속도 적용 주체는 PlayerMovement 한 곳이다.

| 상황 | 결과 |
| --- | --- |
| 지상 새 입력 | 즉시 점프하고 예약 소비 |
| 코요테 이내 새 입력 | 기존 코요테 점프 실행 |
| 착지 직전 공중 입력 | 예약 후 지상 조건을 만족하는 틱에서 실행 |
| 착지 전 키 해제 | 예약은 남으며 실행 시 낮은 점프 |
| 입력 후 유효 시간 만료 | 예약 취소. 계속 누르고 있어도 착지 시 자동 실행 없음 |
| 새 누름 | 가장 최근 요청 한 건으로 교체 |
| 점프 성공 후 계속 유지 | 착지마다 반복 점프하지 않음 |
| Reader 비활성화 | 이벤트로 예약 즉시 취소. 같은 물리 틱 사이에 다시 켜도 복구 없음 |
| Movement/Player 비활성화 | 예약·해제 기록·점프 권한 정리 |

## 4. 설계 근거·트레이드오프

입력 장치는 누름·해제·유지 상태만 제공하고, 점프를 언제 허용할지는 이동 컴포넌트가 결정한다. 보관 시간과 접지 조건을 같은 물리 시계로 판단해 별도 코루틴이나 범용 타이머를 만들지 않았다. 예약은 가장 최근 한 건만 보관하며 여러 번 누른 입력을 큐로 쌓지 않는다.

Reader의 미소비 입력 정리만으로는 이미 Movement로 넘어온 예약을 취소할 수 없다. InputDisabled 알림을 작은 추상 입력 계약에 추가하여 Reader가 Movement 구체 타입을 참조하지 않고 취소를 전달한다. Movement는 자신의 활성 수명 동안 구독하고 비활성화 시 해제한다. 물리 틱 사이에 Reader를 껐다 켜는 경우도 처리하기 위한 실제 요구다.

코요테 타임은 바닥을 떠난 **뒤** 점프 가능 시간이며, 점프 버퍼는 입력한 **뒤** 점프 조건을 기다리는 시간이다. 예약은 공중 점프 권한을 새로 만들지 않는다. 코요테가 끝난 공중 입력도 착지가 버퍼 안에 일어나면 착지 점프로 실행될 수 있다.

## 5. 검증 근거

Unity 6000.4.7f1에서 총 44개를 통과했다. 편집 모드 6개와 실제 Play Mode 38개를 구분한다.

| 분류 | 실제 확인한 내용 |
| --- | --- |
| 편집 모드 6개 | 버퍼 직렬화, 사용자 jumpSpeed=16/riseGravity=42/coyoteTime=0.2 보존, 참조 연결, 프리팹 GUID 보존 |
| 기본 동작 | 지상 즉시 점프, 성공 시 예약 소비, 착지 전 입력의 보관·실행, 공중에서 조건 없는 재점프 없음 |
| 가변 점프 | 유지한 예약은 최대 속도, 착지 전에 놓은 예약은 낮은 점프, 절단 1회 제한, 물리 틱 사이 짧은 입력 보존 |
| 만료·변경 | 만료 후 착지 점프 없음, 0 설정의 예약 차단과 지상 입력 유지, Inspector 보관 시간 변경 반영, 새 입력으로 최근 예약 교체 |
| 반복 방지 | 성공 후 누른 채 착지해도 자동 반복 없음, 공중 재누름으로 즉시 재점프하거나 여러 번 속도 절단하지 않음 |
| 취소 수명 | Reader 비활성화 즉시 삭제, 같은 물리 틱 사이에 다시 켜도 복구 없음, Movement/Player 비활성화·재활성화 정리 |
| 기존 동작 | 코요테 점프·가변 높이 유지, 코요테 만료 시 즉시 공중 점프 없음, 발판·천장 충돌, 최대 낙하 속도 20과 고속 착지, 공중 좌우 가속·감속, 중력 소유권 복구 |

- 실제 FixedUpdate와 Physics2D 충돌을 사용했다. 임시 Keyboard 이벤트로 입력했고 다른 입력 장치의 활성 상태는 검사 후 복구했다. 런타임 Inspector 수치 변화는 테스트에서만 적용하고 저장하지 않았다.
- 특정 낙하 배치에서 보관된 입력을 소비한 물리 틱부터 점프 실행까지 0.060초로 측정되어 0.1초 안에 실행됐다. 모든 점프에 0.060초 지연을 추가한다는 뜻은 아니며 지상 입력은 즉시 실행한다. 최적화 성능 수치가 아닌 동작 검증 기록이다.
- 원본 로그 `Logs/JumpBufferValidation.txt`의 마지막 결과는 `SUCCESS: 44 checks passed`와 `RESTORED: Edit Mode and original scene; no scene saved; temporary scene removed`다. Logs는 기존 Git 제외 폴더이므로 핵심 결과를 이 보고서에도 보존한다.
- 첫 실행은 열린 씬의 수치와 저장된 프리팹의 수치 차이로 사전 검사에서 중단됐다. 열린 씬을 덮어쓰지 않고, AssetDatabase.CopyAsset으로 **저장된 테스트 씬의 임시 복제본**을 만든 뒤 Additive로 열어 최종 검증했다. 원래 씬의 오브젝트는 Play Mode에서만 비활성화해 물리 간섭을 막았다. 종료 후 원래 활성 씬을 복구하고 임시 씬을 삭제했다. 열린 씬의 미저장 조작감 설정은 이번 저장본 검증과 구분한다.
- 저장된 원본 씬의 SHA256이 작업 전 복사본과 일치한다. 프리팹 차이는 jumpBufferTime 한 줄이며 기존 메타·GUID·사용자 조정값은 보존했다.
- `dotnet C:/Program Files/dotnet/sdk/10.0.301/Roslyn/bincore/csc.dll -noconfig @Temp/JumpBufferCompile/compile.rsp` 형태로 로컬 Unity 참조 어셈블리를 사용한 별도 컴파일을 통과했다. CS0649 두 건은 Inspector 참조 필드의 별도 컴파일 경고이며 실제 참조는 연결돼 있다.
- 임시 자동 검증 코드·.meta·요청·복제 씬은 제거했다. 커밋·푸시·패키지 변경은 하지 않았다.
- 미검증: 물리 키보드·게임패드를 직접 사용한 조작감.

## 6. 직접 확인할 방법

1. `Assets/_Project/Scenes/Tests/PlayerMovementTest.unity`를 열고 Play 후 Game 뷰를 클릭한다.
2. Space로 점프하고 키를 놓는다. 내려오는 중 바닥에 닿기 직전 다시 Space를 누르면 착지 시 바로 한 번 더 뛰어야 한다.
3. 같은 상황에서 착지 전에 짧게 누르고 놓으면 낮은 점프로 실행돼야 한다.
4. 충분히 높은 곳에서 착지보다 일찍(0.1초보다 여유 있게 먼저) 누르고 계속 유지하면 착지 시 점프하지 않아야 한다.
5. Player Movement의 Jump Buffer Time을 0으로 바꾸면 착지 전 예약이 꺼지고, 바닥에서 새로 누르는 점프는 정상이어야 한다.
6. 예약 후 Reader 또는 Movement를 비활성화·재활성화하면 예약 점프가 남지 않아야 한다. 값은 Play 종료 시 되돌아가므로 유지할 값은 편집 모드 프리팹에서 조정한다.

## 7. 임시값·한계

- 신규 jumpBufferTime=0.1초. 시간은 Reader의 입력을 소비한 물리 틱부터 계산하므로 실제 물리 입력 시각과 최대 물리 틱 정도의 차이가 날 수 있다.
- 현재 사용자 프리팹: jumpSpeed=16, riseGravity=42, coyoteTime=0.2. fallGravity=48, maxFallSpeed=20, jumpCutMultiplier=0.5 유지.
- 점프 발동은 기존 0.05u 바닥 센서 판정 기준이다. 지면 근처의 센서 여유가 포함되므로 시각적 충돌 순간과 완전히 동일한 시점이라고 단정하지 않는다.
- 이번 단위에서는 신규 벽 이동·전투·상태 머신·스탯 시스템을 만들지 않는다. 향후 Pause/Cutscene 입력 차단 시 Reader 비활성화 등 명시적인 제어 연결이 필요하다.

## 8. 검토할 부분

0.1초 보관이 입력 누락을 줄이면서 의도치 않은 착지 점프를 과하게 만들지는 않는지 확인한다. 착지 전에 놓은 키를 낮은 점프로 실행하는 정책이 원하는 조작감인지 검토한다.

## 9. 다음 작업 추천(실행하지 않음)

벽 이동의 첫 작은 단위를 설계한다. 벽 감지와 미끄러짐을 시작하기 전에 F구역 능력 획득 전 잠금/획득 후 허용 조건을 정리한다. 사용자 지시 없이 구현하지 않는다.