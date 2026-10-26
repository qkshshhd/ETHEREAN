# 능력 해금이 적용된 벽 미끄러짐

구현 및 Unity 검증: 2026-10-26. 정리 및 최종 보고 갱신: 2026-10-27.

## 1. 목표와 완료 여부

완료: 좌우 벽 감지, 기본 잠금인 벽 이동 능력, 해금 후 하강 속도를 제한하는 벽 미끄러짐, 프리팹 연결과 별도 테스트 씬을 구현했다. Unity 검사 66개가 통과했고 사용자가 동작을 긍정적으로 검토했다. 임시 검증 코드와 메타, 대기 요청 및 임시 복제 씬 정리를 확인했다. 최종 잔여 C# 코드의 별도 컴파일도 통과했다.

이 기능은 기초 이동 커밋 527bd83 이후 변경이다. 사용자가 2026-10-27에 현재 작업 커밋을 지시했다. 이 문서 작성만으로 실제 커밋 성공을 의미하지는 않는다. 벽 매달리기는 최신 사용자 결정으로 이번 범위에서 제외했고 벽차기, F 획득 이벤트와 영구 저장은 이후 별도 승인 단위다.

## 2. 추가 및 수정한 파일

프로젝트 루트: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN.

| 파일과 구분 | 수정 위치 및 역할 |
| --- | --- |
| Assets/_Project/Scripts/Runtime/Player/PlayerWallSensor.cs 추가 | WallContacts, CheckWalls와 CheckWall. 좌우 Cast 결과를 레이어·트리거·벽 각도로 필터링하고 0거리 중첩 법선을 별도 확인 |
| Assets/_Project/Scripts/Runtime/Player/PlayerTraversalAbilities.cs 추가 | 기본 잠금인 wallTraversalUnlocked, 읽기 전용 프로퍼티와 중복 호출에 안전한 UnlockWallTraversal |
| Assets/_Project/Scripts/Runtime/Player/PlayerMovement.cs 수정 | Reset·Awake·OnEnable의 참조 확인, CalculateVerticalSpeed의 벽 낙하 상한, OnDisable의 상태 정리와 IsWallSliding |
| Assets/_Project/Scripts/Editor/PlayerMovementTestSceneBuilder.cs 수정 | 기본 생성 시 센서·능력 연결, CreateWallSlideScene으로 별도 복제 씬과 높은 벽 확장부 생성 |
| Assets/_Project/Prefabs/Player/Player.prefab 수정 | 센서와 능력 컴포넌트 및 Movement 참조 연결. 프리팹 기본 능력은 잠금, wallSlideSpeed=3 |
| Assets/_Project/Scenes/Tests/PlayerWallSlideTest.unity 추가 | 기존 이동 씬을 복제한 별도 벽 이동 테스트 공간. 좌우 높은 벽 포함 |
| Assets/_Project/Scenes/Tests/PlayerMovementTest.unity 수정분 보존 | 저장된 프리팹 인스턴스의 wallLayers.m_Bits=51 및 wallTraversalUnlocked=1 오버라이드. 테스트 씬에서 즉시 벽 미끄러짐을 확인할 수 있는 설정 |
| PLAYER_SETUP.md와 AGENTS.md 수정 | 검증 결과, 현재 저장 설정, 사용법과 사용자 리뷰 및 제외 범위 기록 |
| Docs/Reports/2026-10-26-wall-slide.md 추가 | 파일별 책임, 실행 흐름, 검증 근거와 재현 절차의 최종 보고 |

새 런타임 스크립트와 별도 씬의 .meta도 함께 관리한다. 프리팹 GUID 3e46627fd46a4cac9d7422ce77818392는 유지했다. PlayerWallSensor GUID는 0517dee283b51db4cae4e65e813b89c5, PlayerTraversalAbilities GUID는 9eb0199cff056234fb313c6229b61a92다.

처음 도구는 기본 씬을 덮어쓰지 않고 별도 씬을 생성했다. 현재 기본 이동 씬에 저장된 두 오버라이드는 이후 저장된 테스트 설정으로 보존한다. 이동 속도, 점프값과 기존 바닥 마스크 및 Player 위치를 임의로 초기화하지 않았다.

제거한 임시 파일: Assets/_Project/Scripts/Editor/WallSlideValidation.cs 및 .meta, Temp/WallSlideValidation.request, Logs/WallSlideEditorReady.txt와 WallSlideEditorState.txt. 요청과 임시 복제 씬은 검사 종료 때 이미 정리됐으며 부재를 확인했다. Logs/WallSlideValidation.txt와 WallSlidePreview.png는 검증 근거로 남기고 로그의 핵심 내용은 이 보고서에 보존한다.

## 3. 실제 실행 흐름

1. PlayerMovement.FixedUpdate가 현재 X와 Y 속도를 읽는다.
2. 기존 가속·감속으로 X를 계산한다. Y는 바닥 상태와 기존 점프·코요테·버퍼·키 해제 감속을 먼저 처리한다.
3. 일반 공중 상태에서 상승 또는 하강 중력을 적용하고 maxFallSpeed로 제한한다.
4. 하강 또는 수직 정지 중이며 능력 컴포넌트가 활성화돼 있고 WallTraversalUnlocked가 true이면 PlayerWallSensor.CheckWalls를 호출한다.
5. 센서는 CapsuleCollider2D를 좌우로 Cast하고 유효한 벽 접촉 사실만 반환한다. 0거리 중첩은 Collider2D.Distance로 확인한다.
6. 어느 한쪽에 유효 벽이 있으면 IsWallSliding을 true로 하고 계산한 Y속도를 -wallSlideSpeed 이상으로 제한한다.
7. PlayerMovement가 계산한 X와 Y를 한 번에 Rigidbody2D.linearVelocity에 적용한다. 센서와 능력 컴포넌트는 물리 속도를 쓰지 않는다.

| 조건 | 결과 |
| --- | --- |
| 능력 미해금 | 기존 점프와 낙하 유지 |
| 지상 또는 상승 중 벽 접촉 | 벽 미끄러짐 아님 |
| 해금 후 하강 중 벽 접촉 | 방향 입력 없이 벽 미끄러짐 |
| 벽 이탈 또는 센서·능력 컴포넌트 비활성화 | 일반 낙하 복귀 |
| 착지 | 지상 이동과 일반 점프 권한 복구 |
| Reader 비활성화 | 입력과 버퍼는 정리하고 수동 입력 없는 벽 미끄러짐은 유지 |
| Movement 비활성화 | IsWallSliding 해제, 원래 gravityScale 복원 |

벽 접촉은 지상 시각이나 일반 점프 권한을 복구하지 않는다. 미끄러지는 동안 Space를 눌러도 아직 벽차기는 하지 않는다. 기존 유효 코요테 점프 및 착지 시 버퍼 점프는 유지한다.

## 4. 설계 이유와 트레이드오프

센서는 접촉 사실, 능력 컴포넌트는 보유 사실, Movement는 실제 이동 실행을 맡는다. 책임을 나누면서 기존 단일 물리 속도 적용 지점을 유지했다. 새 전역 싱글턴, 저장 시스템과 사용하지 않는 스탯 인터페이스를 미리 만들지 않았다.

하강 중 벽에 닿으면 방향 입력 없이 감속하고 상승 접촉은 유지하는 정책을 사용자가 검토했다. wallSlideSpeed 등 수치는 Inspector에서 조절하는 임시 튜닝 값이다. 속도 상한을 적용하므로 벽에 빠르게 떨어져 접촉한 순간 제한 속도로 바뀐다.

Collider2D.Cast는 시작 중첩에서 검사 역방향의 법선을 반환할 수 있어 0거리 중첩을 Collider2D.Distance로 추가 확인한다. 실제 바닥·천장·벽의 얕은 중첩을 Unity에서 검사했다. 참고: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Collider2D.Cast.html 및 https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ColliderDistance2D-normal.html.

능력 bool은 같은 인스턴스의 비활성화로 지우지 않는다. 새 인스턴스와 재실행의 영구 보존은 아직 없으므로 테스트 bool을 GameProgress와 저장 완료로 표현하지 않는다.

## 5. 검증 근거

- 이번 단위의 기존 Unity 실행 로그는 Editor 8개와 실제 Play Mode 58개, 합계 66개 통과다. 원래 편집 씬 복귀와 임시 복제 씬 제거가 기록됐다. 이 결과를 2026-10-27에 새로 Play Mode 실행한 것으로 표시하지 않는다.
- 실제 벽 미끄러짐 이동은 0.200초 동안 0.600u였다. 설정 속도 3u/s와 일치했다.
- 같은 검사에서 유지 점프와 짧은 점프 높이는 각각 3.208u와 1.450u였다. 특정 배치에서의 회귀 측정이며 모든 환경에서 같은 높이를 보장하는 값은 아니다.
- 센서의 좌우·양쪽·거리·레이어·트리거·경사·자기 몸체 제외, 바닥과 천장 중첩 제외, 벽 밀착, 설정 변경, 컴포넌트 토글 및 기존 점프와 낙하를 실제 검사했다.
- Logs/WallSlidePreview.png의 1280×720 이미지를 열어 플레이어와 높은 양쪽 벽, 플랫폼의 표현을 확인했다. 사용자가 실제 벽 미끄러짐을 긍정적으로 검토했다.
- 임시 검증 코드 제거 후 2026-10-27 최종 7개 C# 소스를 Unity 참조 어셈블리와 Roslyn csc.dll로 별도 컴파일했다. 종료 코드 0. CS0649 두 건은 Inspector에서 연결하는 Move와 Jump 참조 필드의 별도 컴파일 경고다.
- 별도 컴파일 명령: dotnet C:/Program Files/dotnet/sdk/10.0.301/Roslyn/bincore/csc.dll -noconfig @C:/Users/pjw/AppData/Local/Temp/EthereanWallSlideFinal20261027/compile.rsp. 실제 실행에서는 공백이 있는 경로를 인자로 구분했다. 응답 파일에는 현재 7개 소스와 Unity 참조를 넣었다.
- 검사 후 저장된 기본 이동 씬의 해금 및 마스크 오버라이드 전체에 대해 새 Play Mode 검사를 반복한 것은 아니다. 두 필드의 diff를 확인하고 보존했다.

커밋 실행 시도 기록 (2026-10-27): 에이전트의 git add가 .git/index.lock: Permission denied로 거부돼 로컬 커밋과 원격 업로드는 완료하지 못했다. 실패 시점 HEAD는 527bd833e42adc9dc0e7ea6a5cfbf942acf56f44이며 스테이징은 비어 있었다. Docs/Tools/CommitWallSlide.ps1은 로컬 PowerShell에서 명시된 12개 구현 파일과 7개 기획·도구 파일만 두 커밋으로 만든다. -Push가 있으면 origin의 현재 브랜치로 일반 push한다. helper 자체의 구문과 파일 목록을 검사했으며 에이전트가 helper의 커밋을 실행한 것은 아니다. 실제 성공 여부는 실행 뒤 git log로 확인한다.

## 6. 직접 확인하는 방법

1. Assets/_Project/Scenes/Tests/PlayerWallSlideTest.unity를 열고 Play 후 Game 뷰를 클릭한다.
2. Player Traversal Abilities의 Wall Traversal Unlocked를 끈 상태에서 A/D와 Space로 좌우 벽에 접근한다. 기존 낙하가 유지돼야 한다.
3. 같은 값을 켜고 벽 옆에서 점프한다. 상승은 유지되고 정점 이후 접촉 중 천천히 내려와야 한다.
4. 방향 키를 놓아도 벽 가까이에 있으면 미끄러짐을 유지한다. 반대 방향으로 벽을 떠나면 일반 낙하로 복귀한다.
5. Player Movement의 Wall Slide Speed를 바꾸어 속도 상한 변화를 확인하고 착지 후 일반 이동·점프를 확인한다.
6. 기존 PlayerMovementTest.unity는 저장된 인스턴스 오버라이드로 해금 상태가 true이고 벽 마스크가 51이다. 기본 잠금 비교는 위에서 체크를 직접 바꾸거나 별도 벽 테스트 씬에서 한다. Play 중 Inspector 변경은 종료하면 저장 상태로 돌아간다.

임시 자동 검증기는 제거했으므로 Ctrl+R로 검사가 자동 재실행되는 기능은 남아 있지 않다.

## 7. 임시값과 남은 제한

| 항목 | 저장 값 |
| --- | --- |
| wallSlideSpeed | 3u/s |
| wallCheckDistance | 0.05u |
| maxWallAngle | 수직 벽 대비 15도 |
| 프리팹 능력 초기값 | false |
| 기본 이동 씬의 테스트 오버라이드 | wallTraversalUnlocked=true, wallLayers=51 |
| 기존 튜닝 | 이동 6, 가속 40, 감속 60, 점프 16, 상승 중력 42, 낙하 중력 48, 최대 낙하 20, 코요테와 버퍼 0.1 |

벽 확장부는 x=-8/+8, y=4, 크기 0.5×5u다. 자연 지형 전체, 이동 플랫폼과 실제 게임패드 조작감은 검증 범위 밖이다. F 이벤트, 영구 진행과 저장은 아직 미구현이며 매달리기는 이번 범위에서 제외한다.

## 8. 사용자 검토 결과

2026-10-26 사용자가 벽 미끄러짐 구현을 긍정적으로 검토했다. 이후 매달리기는 필요하지 않다는 결정으로 제외했고 벽차기는 다음 별도 구현 단위로 남겼다. 현재 사용자 지시는 오늘의 작업 종료와 커밋이다.

## 9. 다음 작업 추천

사용자가 다음 개발을 지시하면 벽차기 한 단위를 검토한다. 커밋 정리를 이유로 벽차기, F 이벤트와 저장 기능을 추가하지 않는다.

## 부록 Unity 실행 로그

아래는 검증이 실제 실행됐을 때 생성된 로그다. 새 실행 결과가 아니다.

```text
RUNNING: wall slide Unity validation
PASS: Editor: sensor and ability references connected
PASS: Editor: prefab starts locked
PASS: Editor: user jump tuning preserved
PASS: Editor: coyote and buffer tuning preserved
PASS: Editor: ground layer mask preserved
PASS: Editor: no missing player scripts
PASS: Editor: prefab GUID preserved
PASS: Editor: separate wall test scene saved
PASS: Play: required components present
PASS: Play: tall wall extensions connected
PASS: Sensor: empty space and self collider excluded
PASS: Sensor: right wall detected independently
PASS: Sensor: both sides can report contact
PASS: Sensor: triggers excluded
PASS: Sensor: layer mask applied
PASS: Sensor: distance limit applied
PASS: Sensor: floor is not a wall
PASS: Sensor: shallow floor overlap is not a wall
PASS: Sensor: ceiling is not a wall
PASS: Sensor: shallow ceiling overlap is not a wall
PASS: Sensor: tilted wall 10 degrees filtered correctly
PASS: Sensor: tilted wall 30 degrees filtered correctly
PASS: Sensor: touching wall at zero distance remains detected
PASS: Sensor: shallow wall overlap retains only the real side
PASS: Sensor: disabled component returns no contact
PASS: Sensor: disabled collider returns no contact
PASS: Sensor: same Rigidbody sibling collider excluded
PASS: Play: ability initially locked
PASS: Unlock is idempotent
PASS: Ability ownership survives component toggle
PASS: Locked right wall keeps normal falling
PASS: Locked left wall keeps normal falling
PASS: Unlocked right contact slides without horizontal input
PASS: Right slide speed capped at 3
PASS: Actual wall slide travel matches configured speed
MEASURE: wall slide travel=0.600u in 0.200s
PASS: Holding toward wall keeps sliding without false grounding
PASS: Pushing into wall does not add vertical friction
PASS: Unlocked left contact uses same cap
PASS: Inspector wall slide speed change applied
PASS: Grounded beside wall is not wall sliding
PASS: Actual jump rises beside unlocked wall without clipping speed
PASS: Rising jump transitions to slide only after descending
PASS: Wall contact does not restore grounded time or jump permission
PASS: Pressing jump while sliding does not implement a wall jump
PASS: Unused wall jump request expires normally
PASS: Leaving wall restores normal gravity
PASS: Disabled sensor ends sliding
PASS: Sensor reenable resumes valid slide
PASS: Disabled ability component blocks use without losing ownership
PASS: Ability component reenable restores allowed slide
PASS: Passive wall slide continues with reader disabled
PASS: Movement disable clears slide and restores original gravity
PASS: Native gravity continues while movement is disabled
PASS: Movement reenable regains custom gravity and valid slide
PASS: Player toggle retains ability and resumes slide
PASS: Wall slide lands and returns to grounded movement
PASS: Horizontal acceleration preserved
PASS: Horizontal deceleration preserved
PASS: Held and tap variable jump heights remain distinct
MEASURE: held height=3.208u tap height=1.450u
PASS: Coyote jump remains available after ledge exit
PASS: Coyote jump still supports early release
PASS: Buffer press does not jump early in air
PASS: Prelanding buffer still jumps on valid ground
PASS: Normal fall speed remains capped at 20 away from walls
PASS: High fall still lands without tunnelling
PASS: Ceiling collision remains valid
SUCCESS: 66 checks passed
RESTORED: Edit Mode and original scene; temporary scene removed
```
