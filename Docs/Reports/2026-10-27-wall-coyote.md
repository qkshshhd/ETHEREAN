# 벽차기 코요테 타임 — 2026-10-27

## 1. 목표와 완료 여부

사용자가 보고한 벽 이탈 직후 점프 누락을 개선한다. 현재 접촉 또는 최근 유효 접촉을 사용해 벽차기를 허용하되, 한 번 사용한 기록은 소비해 재접촉 없는 연속 공중 점프를 막는다. 코드 구현·프리팹 새 값 저장·Unity 새 검사 109개(12 Editor + 97 실제 Play Mode)·임시 도구 정리·최종 11개 C# 소스의 별도 컴파일을 완료했다. 조작감 리뷰는 대기 중이다. 기존 책임 분리·공유 버퍼·점프 컷·잠금·재접촉 정책을 유지하고 이 개선 한 단위만 작업한다.

## 2. 수정·추가한 파일

| 구분 | 전체 경로 | 정확한 변경 위치와 역할 |
| --- | --- | --- |
| 수정 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Runtime\Player\PlayerWallJump.cs` | wallCoyoteTime 필드, 좌우 마지막 유효 접촉 시각, UpdateContacts/TryJump 확장, SelectCoyoteSide/SelectSide/ClearWallHistory/CancelPendingInput 추가. 벽 코요테 판정과 소비·취소를 기능 클래스에 캡슐화 |
| 수정 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Runtime\Player\PlayerLocomotion.cs` | FixedUpdate에서 UpdateContacts에 Time.fixedTime 전달. CancelPendingInput에서 새 WallJump.CancelPendingInput을 호출해 입력 비활성화 즉시 접촉 기억도 취소 |
| 수정 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Prefabs\Player\Player.prefab` | Player/PlayerWallJump의 새 wallCoyoteTime 값을 직렬화. 기존 속도·시간·참조·능력 잠금 보존 |
| 수정 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\AGENTS.md` | 최신 벽차기 정책과 Current review unit에 사용자 요청·독립 코요테 설정·소비/취소·검증 상태 기록 |
| 수정 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\PLAYER_SETUP.md` | Inspector 설정 표와 최신 벽 코요테 동작·확인 절차 |
| 수정 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Docs\Design\2026-10-26-wall-movement.md` | 최신 정보에 벽 이탈 후 유예 정책과 새로운 보고서 연결. 이전 설계 기록은 보존 |
| 수정 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Docs\Plans\ETHEREAN_Daily_Plan_2026-11-08.md` | DAY 02 사용자 피드백 작업과 변경 이력. 다른 일정·아트·리뷰 항목은 별도 유지 |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Docs\Reports\2026-10-27-wall-coyote.md` | 이 기능의 파일 위치·흐름·설계 이유·실제 새 검증·수동 확인 기록 |

새 영구 런타임 스크립트는 없다. 기존 파일 삭제·이름 변경과 패키지·Input Actions·씬 지형 변경은 없다. 임시 검증 도구와 복제 씬의 정리 결과는 5절에 기록한다. 기존 소스·프리팹·씬의 .meta와 GUID를 보존한다.

## 3. 변경 전후와 실제 흐름

이전 TryJump는 현재 WallContacts만 검사했으므로 벽 센서가 접촉을 잃은 다음 틱에는 점프를 눌러도 벽차기가 되지 않았다. 이제 접촉 중 기록한 시각을 짧게 유지하고 벽 이탈 후에도 유효한 요청을 처리한다.

1. NewInputMovementReader가 기존 Move/Jump 입력을 수집한다. PlayerLocomotion.FixedUpdate가 센서 결과·능력과 입력 활성 여부·현재 물리 시각을 WallJump.UpdateContacts에 전달한다.
2. 벽차기 재사용이 막히지 않은 유효한 왼쪽/오른쪽 접촉만 해당 마지막 관측 시각을 갱신한다. 능력·센서·입력 또는 WallJump가 비활성 상태이면 접촉 기록과 제어 제한을 취소한다.
3. PlayerJump는 기존 공유 버퍼를 갱신한다. PlayerLocomotion.TryStartJump는 지상 점프 → 유효한 벽차기 → 지상 코요테 점프 우선순위를 유지한다.
4. WallJump.TryJump는 실제 현재 접촉이 있으면 그 벽을 우선한다. 현재 벽이 없으면 SelectCoyoteSide가 현재 시각에서 마지막 접촉 시각을 뺀 값이 wallCoyoteTime 이하인지 검사한다. 0이면 이탈 후 유예를 사용하지 않는다.
5. 두 측면 이력이 모두 유효하면 더 최근에 관측한 벽을 선택한다. 같은 시각의 양쪽 접촉/이력은 기존처럼 방향 입력으로 딛을 벽을 정하고 무입력에는 임의 방향을 고르지 않는다.
6. 발동하면 벽 반대 방향 X속도·설정 Y속도를 반환하면서 양쪽 기억을 모두 비우고 딛은 측면을 재사용 제한한다. 남은 유예 시간으로 추가 도약하지 못하며 사용 직후 아직 닿아 있는 출발 벽도 기록을 재충전하지 못한다.
7. PlayerJump.StartJump가 공유 요청과 지상/코요테 권한을 한 번 소비한다. 조기 해제는 기존 컷을 적용하고 짧은 수평 제어 보호 뒤 기존 가속·감속으로 복귀한다. Rigidbody 적용은 계속 PlayerMovement 한 곳이다.
8. 새 유효 접촉을 거치면 벽차기와 유예 자격이 다시 생긴다. 착지·전체 이동 상태 초기화·WallJump 비활성화는 기억을 지우며 Reader.InputDisabled는 물리 틱 사이에도 즉시 지운다. 센서/능력 비활성화는 다음 물리 틱에서 확인한다.

## 4. 설계 이유와 트레이드오프

접촉 이력·만료·소비는 벽차기 규칙이므로 PlayerWallJump가 소유한다. PlayerLocomotion은 시각 전달과 취소 이벤트만 연결한다. PlayerMovement·PlayerJump·PlayerWallSlide·센서 코드를 변경하거나 다른 속도 적용 주체를 추가하지 않는다.

지상 코요테(coyoteTime)는 발판 이탈 후 일반 점프 자격이고, wallCoyoteTime은 벽 이탈 후 벽차기 자격이므로 별도 Inspector 값이다. 점프를 미리 누르는 jumpBufferTime과도 구분한다. 버퍼와 벽 코요테의 유효 조건이 같은 틱에 만족하면 기존 요청을 한 번 소비한다.

좌우 시각을 따로 저장하면 새로 닿은 벽을 우선할 수 있고 양쪽 접촉의 기존 입력 정책도 유지할 수 있다. 대신 단순 bool보다 작은 일시 상태가 늘어나므로 사용·착지·입력 차단·비활성화에서 정확히 취소한다. 개별 Collider ID가 아니라 기존 좌우 센서의 관측 이력을 사용한다.

## 5. 검증 근거

**새 실행 109개 전부 통과: 12 Editor + 97 실제 Play Mode, 실패 0개.** `Logs/WallCoyoteValidation.txt`의 SUCCESS와 RESTORED를 확인했고 전체 로그를 부록에 보존했다. 이전 92개나 65개 기록을 새 검사 수로 세지 않았다.

- Editor 12개: 프리팹과 세 저장 씬의 기존 14개 수치·새 값 0.1 직렬화/상속(8개), 프리팹 기본 잠금·조정자 여덟 참조(2개), 벽 기능의 Rigidbody 비접근·입력 차단 알림 연결(2개)을 확인했다.
- Play 97개: 기존 65개 항목(벽차기 단계의 57개 + 분리 컴포넌트 수명주기 8개)을 실제 새 실행으로 다시 확인하고, 벽 코요테 전용 32개를 추가했다. 좌우 이탈 후 입력, 만료, 0 설정과 0.2초 증가, 한 번 소비, 사용 직후 출발 벽 기록 차단, 실제 재접촉 후 반복, 미접촉/미해금 상태, 센서·능력·입력·이동·조정자·Player·WallJump 토글, 착지, 마지막 벽 선택, 동시 모호성, 짧은 입력, 지상 코요테와 독립 설정을 검사했다.
- 가상 Keyboard 상태 이벤트 → 실제 Reader → 조정자 FixedUpdate → Physics2D 결과를 관찰했다. 포커스 정책은 검증용 InputSettings 복사본을 쓰고 시간은 일시적으로 0.02초 틱으로 고정한 후 원래 설정을 복원했다. 가상 입력 장치도 제거했다. 자동 입력 검사를 물리 키보드/게임패드 조작감 검증으로 표현하지 않는다.
- 좌우 이탈 시나리오는 실제 방향 이동으로 접촉을 잃은 뒤 누른 입력이다. **마지막 오른쪽 접촉 관측 후 0.040초에 누른 벽차기가 성공**했다. 접촉 순서/동시 모호성/착지 초기 조건 등은 일부 위치·속도·Inspector 값을 도구로 설정해 별도로 검사했다.
- 기존 벽차기 측정도 같은 실행에서 유지/짧은 높이 3.208/1.004u, 무방향 수평 이동 1.256u, 제어 복구 0.120s(설정 0.1, 틱 0.02)를 기록했다. 실제 B/I 지형 통과나 최적화 성능 측정과 구분한다.
- 모든 기존 테스트 씬과 복구 씬, 기존 .meta, PlayerMovement/PlayerJump/PlayerWallSlide/센서/Reader/능력 코드가 작업 전과 바이트 단위로 같았다. 프리팹 차이는 `wallCoyoteTime: 0.1` **한 줄 추가뿐**이다. 기본 이동 씬 수평 속도 4 등 기존 오버라이드를 유지했다.
- 원래 열린 PlayerWallJumpTest의 활성 상태와 dirty=False가 복구됐으며 임시 복제 씬은 제거됐다. 임시 도구 제거 후 최종 11개 C# 소스를 새로 별도 컴파일해 exit code 0을 확인했다. 기존 Reader Inspector 참조의 CS0649 경고 2개는 오류와 구분한다.

다음 임시 C# 파일과 각각의 `.meta`를 모두 제거했다. 영구 기능이 의존하지 않는다.

- `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Editor\WallCoyoteValidationBootstrap.cs`: 프리팹 새 설정 저장·Editor 확인·Play 시작/종료와 씬 복구
- `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Runtime\Player\WallCoyoteValidationDriver.cs`: 기존 회귀와 벽 코요테 실제 입력/물리 검사

요청·준비·중지 마커와 WallCoyoteVerify/WallCoyoteValidationScene 임시 에셋도 없다. 이전 수치 캡처와 실행 로그는 Logs에 증거로 남겼다. 이번 작업에서 커밋·푸시는 실행하지 않았다.

## 6. 직접 확인 방법

1. PlayerWallJumpTest를 단독으로 열고 Player의 PlayerWallJump → Wall Coyote Time을 확인한다. 초기 임시값은 0.1초다. 일반 점프의 Coyote Time은 PlayerJump에 따로 있다.
2. Play 후 해금된 상태에서 오른쪽 벽에 접촉하고 A로 벽에서 떨어진다. 접촉이 사라진 직후 Space를 누르면 오른쪽 벽을 딛듯 왼쪽 위로 도약해야 한다. 왼쪽 벽에서는 반대로 확인한다. 상승·낙하 중 모두 같은 규칙이다.
3. 벽을 떠난 후 충분히 기다려 Space를 누르면 이 벽차기는 실행되지 않아야 한다. 0으로 설정하면 접촉 중일 때만 되고, 0.2로 늘리면 늦게 누를 수 있는 시간이 늘어난다.
4. 벽 코요테 점프 직후 접촉 없이 Space를 놓았다가 다시 눌러도 두 번째 점프가 되지 않아야 한다. 같은 벽 또는 맞은편 벽에 실제 재접촉한 뒤에는 새 입력으로 다시 점프할 수 있다.
5. 버튼을 빨리 놓으면 짧게 점프하고, 누르고 있기만 해서 자동 반복하지 않아야 한다. Reader/Movement/Locomotion/Player 비활성화·재활성화 후에는 예전 벽 기억으로 도약하지 않아야 한다.
6. 능력 잠금 상태에는 적용되지 않는다. 기본 이동 씬의 수평 속도 4, 프리팹/벽차기 씬의 8 등 기존 수치는 그대로다. Play 중 Inspector 변경은 종료 시 복원되므로 최종 튜닝 저장은 사용자 검토 후 한다.

## 7. 임시값·제한

`wallCoyoteTime=0.1초`는 조작감 검토를 위한 임시 시작값이며 최종 밸런스가 아니다. 물리 틱 단위로 관측하므로 실제 입력 허용의 시간 분해능이 있다. 이 값이 길면 실제 벽에서 더 멀어진 위치에서도 차는 느낌이 생길 수 있다. 실제 물리 키보드·게임패드 조작감과 B/I 지형 통과 조건은 별도로 검토한다.

## 8. 사용자 리뷰 지점

반대편 벽으로 이동할 때 입력이 덜 누락되는지, 0.1초 유예가 너무 관대하거나 짧지 않은지 확인한다. 현재 접촉 우선·기억 한 번 소비·재접촉 후 반복과 기존 짧은 점프가 유지되는지도 확인한다.

## 9. 다음 작업 추천 — 실행하지 않음

이 수정의 실제 조작감과 수치 리뷰를 먼저 마친다. 이후 기존 아트·이동 성능·지형 작업은 별도 지시 후 진행한다. 자동 커밋·푸시는 하지 않는다.

## 부록: 실제 새 Unity 실행 로그

```text
Wall coyote validation 2026-10-27T23:40:17.3043572+09:00
PASS Editor: prefab: original 14 values preserved
PASS Editor: prefab: wallCoyoteTime serialized or inherited as temporary 0.1 seconds
PASS Editor: prefab remains ability locked
PASS Editor: eight coordinator references remain connected
PASS Editor: Assets/_Project/Scenes/Tests/PlayerMovementTest.unity: original 14 values preserved
PASS Editor: Assets/_Project/Scenes/Tests/PlayerMovementTest.unity: wallCoyoteTime serialized or inherited as temporary 0.1 seconds
PASS Editor: Assets/_Project/Scenes/Tests/PlayerWallSlideTest.unity: original 14 values preserved
PASS Editor: Assets/_Project/Scenes/Tests/PlayerWallSlideTest.unity: wallCoyoteTime serialized or inherited as temporary 0.1 seconds
PASS Editor: Assets/_Project/Scenes/Tests/PlayerWallJumpTest.unity: original 14 values preserved
PASS Editor: Assets/_Project/Scenes/Tests/PlayerWallJumpTest.unity: wallCoyoteTime serialized or inherited as temporary 0.1 seconds
PASS Editor: wall grace stays in wall-jump policy without Rigidbody access
PASS Editor: input-disabled notification clears wall history immediately
PASS Play: right wall detected in fixture
PASS Play: virtual keyboard input reaches real Reader regardless of Game View focus
PASS Play: locked ability rejects wall jump
PASS Play: right wall ascent launches left and up
PASS Play: wall jump is airborne and not sliding
PASS Play: wall jump consumes shared buffer once
PASS Play: wall jump consumes former ground eligibility
PASS Play: left wall ascent launches right without direction input
PASS Play: falling speed is replaced by consistent upward launch
PASS Play: input toward source wall cannot cancel initial outward travel
PASS Play: gravity continues during horizontal control lock
PASS Play: control restores through gradual braking
MEASURE: control restoration after 0.120s (setting 0.100s)
PASS Play: continued source-wall input eventually reverses direction
PASS Play: zero lock still applies launch velocity on launch tick
PASS Play: zero lock restores control on following tick
PASS Play: Inspector launch speed changes affect both axes
PASS Play: Inspector lock duration change extends protected travel
PASS Play: release cuts wall jump upward speed during lock
PASS Play: release does not cut horizontal launch
PASS Play: jump release cuts once rather than each tick
PASS Play: airborne repress cannot restore cut or create extra jump
PASS Play: fresh press cannot repeat on an unseparated source wall
PASS Play: blocked repeat request expires normally
PASS Play: real physics leaves and returns to the same wall
PASS Play: holding jump does not automatically jump again on recontact
PASS Play: new press after real same-wall recontact jumps again
PASS Play: real physics crosses to the opposite wall
PASS Play: fresh press at opposite wall launches back across
PASS Play: ground jump has priority beside wall
PASS Play: valid airborne wall jump takes priority over coyote jump
PASS Play: early wall press waits for contact
PASS Play: precontact jump buffer executes wall jump within window
PASS Play: buffered wall jump consumes request
PASS Play: release before contact produces a short buffered wall jump
PASS Play: expired wall input does not jump on later contact
PASS Play: reader toggle between ticks clears precontact buffer
PASS Play: reader disable immediately clears horizontal lock
PASS Play: horizontal movement resumes normal braking after reader toggle
PASS Play: movement disable stops X and restores native gravity
PASS Play: reenable owns gravity with no stale lock
PASS Play: player toggle clears launch state and preserves acquired ability
PASS Play: disabled wall sensor rejects wall jump
PASS Play: disabled ability component rejects wall jump
PASS Play: ability disable cancels protected horizontal control
PASS Play: narrow fixture detects both walls
PASS Play: ambiguous two-wall contact does not choose arbitrary side without input
PASS Play: direction input resolves two-wall contact toward selected source wall
PASS Play: baseline horizontal acceleration remains 40 units per second squared
PASS Play: baseline horizontal speed remains six
PASS Play: opposite input first brakes existing movement
PASS Play: release decelerates horizontal motion to rest
PASS Play: normal prelanding jump buffer still works
PASS Play: normal coyote jump still works away from wall
PASS Play: ordinary maximum fall speed remains twenty
PASS Play: unlocked wall sliding retains three-unit falling cap
PASS Play: locked wall contact does not cap falling speed
PASS Play: measured held wall jump is higher than tapped wall jump
MEASURE: held height=3.208u; tap height=1.004u; no-direction horizontal travel=1.256u
PASS Play: coordinator disable stops X and restores native gravity
PASS Play: coordinator reenable resets pending wall control and reacquires gravity
PASS Play: wall jump component can be disabled independently
PASS Play: ordinary jump remains available when wall jump is disabled
PASS Play: wall slide component independently disables falling cap
PASS Play: wall jump remains available when wall slide is disabled
PASS Play: disabled common jump processing discards requests while passive wall slide remains
PASS Play: jump component reenable does not replay held input
PASS Play: right wall contact recorded before actual outward movement
PASS Play: late press after leaving right wall launches left within grace
MEASURE: late wall jump after last right contact=0.040s; setting=0.100s
PASS Play: wall jump consumes both stored contact histories
PASS Play: fresh airborne repress cannot reuse consumed wall grace
PASS Play: late press after leaving left wall launches right regardless of directional input
PASS Play: expiry fixture stays airborne without new wall contact
PASS Play: press after wall grace expires does not create an airborne jump
PASS Play: zero wall grace disables post-detachment wall jump
PASS Play: zero wall grace retains direct contact wall jump
PASS Play: Inspector wall grace increase accepts later input independently of ground coyote
PASS Play: blocked source-wall contact after launch does not refresh grace
PASS Play: leaving just-used source wall cannot unlock a second jump through grace
PASS Play: coyote launch can physically return to its source wall
PASS Play: actual recontact rearms wall jump after a coyote launch
PASS Play: no wall history never grants wall grace
PASS Play: contacts while ability locked cannot be reused after unlocking in air
PASS Play: sensor disable clears stored wall grace
PASS Play: ability disable clears stored wall grace
PASS Play: reader toggle between physics ticks immediately clears wall grace
PASS Play: reader reenable away from wall cannot replay its former contact
PASS Play: movement toggle clears wall grace
PASS Play: coordinator toggle clears wall grace
PASS Play: player toggle clears wall grace
PASS Play: wall jump component toggle clears wall grace
PASS Play: landing clears wall history even while the configured grace would still be valid
PASS Play: sequential left and right contacts remember distinct observation times
PASS Play: most recently touched wall wins when both grace windows remain valid
PASS Play: present wall contact takes priority over earlier opposite-wall history
PASS Play: simultaneous ambiguous wall histories do not choose a side without input
PASS Play: direction input resolves simultaneous wall histories during grace
PASS Play: released short input inside wall grace keeps buffered jump cut behavior
PASS Play: wall grace remains available when ordinary ground coyote is zero
SUCCESS: 12 Editor + 97 Play Mode checks; failures=0
RESTORED: original active scene and open-scene dirty flags match; temporary scene removed
```
