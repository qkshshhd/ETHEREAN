# 대검 기본 전투 구현 보고 — 2026-11-02

## 1. 목표와 완료 상태

사용자 승인 묶음: 공격 입력·지상/공중 단발 베기·공격 단계별 이동 제한·회복 후반 점프 캔슬·중복 타격 방지·테스트 대상과 전용 씬·검 궤적/명중 표현. 구현과 첫 실제54개 검사(9 Editor+45 Play)는 통과했다. 추가 회귀 검사를 포함한 최종 실행 결과는 아래 최종 검증 구역을 따른다. 사용자 조작감과 코드 리뷰는 대기다. 다음 체력·피격·적AI·저장 기능은 구현하지 않았다.

## 2. 파일별 변경 위치와 역할

### 수정 파일

| 전체 경로 | 수정 위치·역할 |
| --- | --- |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerMovement.cs | CalculateHorizontalSpeed: 선택적 speedMultiplier 인자, 공격 시 목표 속도와 실제 수평 속도 상한. 기본 배율1 동작 보존. Rigidbody2D 쓰기 소유 유지 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerLocomotion.cs | attack 참조·ResolveReferences·CalculateVelocity·TryStartJump·CancelPendingInput·ResetRuntimeState: 공격 단계 평가, 이동 배율 전달, 유효 점프 캔슬, 비활성화/입력 취소 정리 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab | Player 루트의 NewInputAttackReader·PlayerAttackHitbox·PlayerAttack·PlayerAttackVisual 추가, Locomotion.attack 연결. Temporary Blade/Silver Attack Trail/Hit Flash 자식 LineRenderer 연결. 기본 검 잠금 유지 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md | ‘대검 기본 전투 테스트’: 씬·입력·Inspector 데이터·수동 확인·임시값·제한 안내 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md | Current review unit: 이번 묶음 승인/완료·검증·한계 기록. 이전 완료 기록 보존 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Plans/ETHEREAN_Daily_Plan_2026-11-08.md | 11-02 재계획의 구현 기록: 공격 묶음 구현과 사용자 리뷰 대기 구분 |

### 추가 C# 파일

| 전체 경로 | 클래스·책임·연결 |
| --- | --- |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/NewInputAttackReader.cs | 기존 Player/Attack 버튼 액션을 독립 복제·소유, performed를 한 번 소비. 비활성화 알림을 PlayerAttack이 받음 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerAttackData.cs | PlayerAttackData ScriptableObject와 HitReaction: 단계 시간·배율·캔슬 지점·피해·반응·넉백·범위를 제공 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/AttackHitReceiver.cs | AttackHit 값과 추상 AttackHitReceiver.TryReceiveHit 계약. 표적이 실제 구현하는 피해 수신 경계이며 이동 처리 없음 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerAttack.cs | AttackPhase와 공격 시작·진행·취소. Tick은 Locomotion이 호출. 시작 데이터/방향 유지, 이동 배율과 CanJump 제공 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerAttackHitbox.cs | BeginAttack/Sample/EndAttack: Active의 OverlapBox, 동적 결과 목록, 수신자별 한 번 적중. HitLanded 이벤트로 표현 요청 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerAttackVisual.cs | LateUpdate·OnHit: 기하학 검/짧은 궤적/접점 섬광. 피해·물리 판정 결정에 관여하지 않음 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/CombatTestTarget.cs | 테스트 표적 HP·횟수·최근 전달 데이터·섬광. 실제 적 AI/넉백/경직이 아님 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/CombatTestPanel.cs | 테스트 전용 IMGUI 상태/표적 표시와 검 잠금·리셋 버튼. 게임 HUD가 아님 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerCombatTestSceneBuilder.cs | Create 메뉴: Editor API로 데이터·참조·재질·프리팹 연결·전용 씬을 생성. 기존 결과가 있으면 덮어쓰지 않음 |

위9개 스크립트의 동일 경로+.meta도 Unity가 생성했다. 기존 .meta/GUID는 유지했다.

### 추가 에셋·기록

| 전체 경로 | 내용과 연결 |
| --- | --- |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GroundAttack.asset | PlayerAttack.groundAttack 연결, 임시 지상 공격 데이터 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/AirAttack.asset | PlayerAttack.airAttack 연결, 별도 공중 공격 데이터 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/PlayerAttackAction.asset | 기존 Input Actions의 Player/Attack 참조. NewInputAttackReader에 연결 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Materials/CombatPreview.mat | URP Unlit 선 표현용 공유 재질. 색상은 Visual의 MaterialPropertyBlock으로 적용 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scenes/Tests/PlayerCombatTest.unity | Combat Camera, 바닥/양쪽 벽/발판, 검·벽 능력 해제 Player, 복합 판정 표적3개, 테스트 패널. 기존 씬에서 분리 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-02-combat-active.png | 실제640×360 카메라 렌더. 캐릭터 아트/최종 픽셀 크기 검증이 아님 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-02-greatsword-combat.md | 이9항목 보고·파일 지도·검증 기록 |

위 Unity 에셋5개의 동일 경로+.meta도 추가됐다. 영구 파일 삭제나 이름 변경은 없다. 임시 검증 파일 정리는 최종 검증 구역을 따른다.

## 3. 실제 실행 흐름

1. NewInputAttackReader가 Player/Attack 복제 액션의 performed를 기억한다. 유지 자동 연타나 공격 입력 버퍼는 없으며 이미 공격 중인 추가 입력은 소비 후 버린다.
2. PlayerLocomotion.FixedUpdate에서 센서/입력을 읽고 바닥·벽 미끄러짐을 평가한 뒤 Attack.Tick을 호출한다. 검 잠금·입력 비활성·벽 미끄러짐이면 시작하지 않는다. 일반 공중과 지상은 가능하다.
3. None에서 groundAttack 또는 airAttack과 방향을 선택하고 Windup→Active→Recovery→None으로 진행한다. 착지나 반대 이동 입력이 공격 종류/방향을 바꾸지 않는다. 각 단계는 물리 틱에서 전환해 최소 한 틱 유지한다.
4. Attack은 이동 배율만 제공한다. Movement가 기존 가속/감속에 배율을 적용하고 공격 속도 상한을 제한한다. 벽차기 직후 바깥 방향 보호 중에는 기존 발사 속도를 먼저 유지한다. 공중 중력과 기존 점프 컷은 계속 계산한다.
5. 점프 요청은 기존0.1초 버퍼를 사용한다. Windup/Active/Recovery 전반에는 실행을 막고 Recovery60% 이후 유효한 지상/코요테/벽 점프가 성공하면 Attack.CancelAttack한다. 조건 없는 공중 점프는 추가하지 않는다. 같은 틱의 공격/점프는 공격 시작을 우선 평가한다.
6. Active에서만 Hitbox.Sample이 박스를 질의한다. Collider가 아니라 부모 AttackHitReceiver별로 중복을 제거해 한 수신자의 복수 판정에도 피해는 한 번이다. 수신이 성공해야 HitLanded를 발행한다. 재공격은 기록을 비우므로 다시 맞힐 수 있다.
7. TestTarget은 HP/횟수/최근 AttackHit를 갱신하고 잠깐 밝아진다. Visual은 공격 단계로 검/궤적을 표시하고 HitLanded 접점에 섬광을 낸다.
8. 공격·입력·이동·Locomotion 비활성화나 검 재잠금은 진행을 정리한다. 공격 취소가 명중 콜백 안에서 발생하면 남은 대상 검사도 멈춘다. 다음 체력/피격 시스템은 공개 CancelAttack을 활용할 수 있지만 이번에 연결하지 않았다.

## 4. 설계 근거와 트레이드오프

공격 데이터를 MonoBehaviour와 분리해 두 공격을 Inspector에서 독립 조정한다. 물리 틱 조정자는 기존 Locomotion을 재사용하고 거대한 상태 머신이나 전체 아키텍처 교체는 하지 않았다. 이동은 Movement, 공격 단계는 Attack, 적중은 Hitbox, 표시는 Visual에 두어 디버깅할 위치가 구분된다. 추상 수신 경계는 실제 TestTarget이 사용하며 향후 적마다 히트박스를 수정하지 않고 수신 동작을 제공할 수 있다.

OverlapBox 결과는 List로 받아 고정 배열 포화 누락을 피한다. 최적화 성능을 측정한 것은 아니다. 실제 공격용 물리 Collider를 켰다 끄는 구조가 아니라 Active에만 질의하므로 취소 후 남은 공격 Collider가 없다. 단순 사각 범위는 현재 검 포즈/궤적과 대략 대응하는 테스트용 판정이다. 정교한 검날 추적·지형 가림(벽 너머 타격 차단)은 미구현이다.

공격 시작 때 기존 수평 속도를 상한으로 제한하므로 빠르게 달리다 공격하면 의도적으로 즉시 느려진다. 회복은 배율 증가와 기존 가속을 함께 따른다. 정교한 모션 블렌딩은 추후 아트 작업이다.

## 5. 검증 근거

첫 Unity 실행은 Editor9+실제 Play45=54개 통과, 원래 열린 픽셀 비교 씬/dirty=false 복귀를 기록했다. 추가 취소 가드와 코요테/버퍼 회귀 검사 후 최종 실행·정리 결과는 아래 기록을 따른다. 스크립트 재로드로 중단된 실행은 최종 통과로 세지 않는다.

별도 Roslyn 컴파일은 Unity6000.4.7f1의 참조/정의로 실행했다. Unity가 Inspector에서 채우는 필드의 CS0649 경고는 예상 항목이며 검사에서 실제 참조를 별도로 확인한다. 렌더 캡처를 직접 열어 검·궤적·명중 섬광과 표적 피격 표시를 확인했다. 실제 장치 조작감·게임패드·최종 아트·빌드 배포·성능은 미검증이다.

기존 Assets/Packages/ProjectSettings의 작업 전 SHA256 스냅샷과 비교했다. 허용 변경은 PlayerMovement.cs·PlayerLocomotion.cs·Player.prefab뿐이다. 이전 프리팹 이동/점프 설정14개는 문자열 값 대조로 보존 확인했다. 사용자 스테이징된 픽셀 씬을 포함한 기존 씬·기존 GUID·Input Actions·패키지·프로젝트 설정은 보존 대상으로 확인한다.

## 6. 직접 재현 순서

1. Assets/_Project/Scenes/Tests/PlayerCombatTest.unity를 열어 Play한다. 생성 메뉴를 다시 실행하지 않는다.
2. Enter 또는 마우스 왼쪽으로 공격한다. 가까운 표적은 HP50→40, Hits1이어야 한다. 길게 눌러도 연타하지 않는다. 표적 복합 콜라이더로 피해가 두 번 나지 않아야 한다.
3. A/D 이동 중 공격해 단계별 제한과 회복을 본다. 반대 방향을 새 공격 시작 전에 입력하면 공격 방향도 바뀐다.
4. 준비·타격·회복 초반 점프는 즉시 실행되지 않고 후반에는 유효 점프로 취소된다. 패널에서 단계/배율을 본다.
5. 공중 공격 후 착지해도 선택은 Air로 남고, 계속 낙하한다. 진행 중 자동으로 지상 공격으로 바뀌지 않는다.
6. 벽 미끄러짐 중 공격은 막히고 벽차기 후 분리하면 공중 공격이 된다. 벽차기 제어 보호 중 벽쪽 입력으로 공격해도 바깥 발사가 즉시 사라지지 않아야 한다.
7. 패널 Toggle test weapon lock으로 시작 불가와 취소를 확인한다. 표적이0HP면 Reset targets로 회복한다. 사망 AI는 없다.
8. Player.prefab의 Attack은 잠금false, Ground/Air 데이터와 Attack Action Reference·LineRenderer 참조가 연결돼 있는지 확인한다. 데이터는 Inspector에서 임시 조정 가능하다.

## 7. 임시값과 제한

| 항목 | 테스트값 |
| --- | --- |
| 지상 Windup/Active/Recovery | 0.24/0.12/0.4초 |
| 공중 Windup/Active/Recovery | 0.18/0.12/0.32초 |
| 준비/타격 이동 배율 | 0.35/0.05 |
| 회복 이동 배율 | 0.05→1, 기존 가속 적용 |
| 점프 캔슬 | 회복 진행60% 이후 |
| 피해/피격 유형/넉백 | 10/Stagger/(방향×3,2) |
| 판정 오프셋/크기 | (방향×0.95,0.1)/(1.4,1.6)u |
| 표적 HP/피격 섬광 | 50/0.08초 |
| 기하학 검선 길이 | 1.5u(32PPU 후보에서48px) |

표적은 넉백/경직 값을 기록만 하고 실제 이동은 하지 않는다. PlayerHealth·피격·사망·적AI·검 획득 이벤트·저장은 다음 묶음이다. 실제 캐릭터 원화나 픽셀 스프라이트를 제작하지 않았다. 청록 몸체와1.8u 충돌체는 그대로라 임시40px/2등신/검1.2배의 최종 비례를 실제 아트로 검증한 상태가 아니다. 효과는 기하학 선 기반 임시 표현이며 최종 픽셀 VFX가 아니다. 히트스톱·화면 흔들림·효과음·먼지는 이번 구현에서 제외했다. 실제 캐릭터 스탯 공급원도 만들지 않았다.

## 8. 사용자 리뷰 쟁점

달리다가 공격할 때 즉시 느려지는 정도, 준비0.24초와 회복0.4초 및 후반 점프 캔슬 시점이 묵직함과 답답함 사이에서 적절한지 확인한다. 공격의 실제 범위와 궤적은 원화가 붙은 뒤 다시 검토한다.

## 9. 다음 작업 추천 — 실행하지 않음

플레이어/적 체력·유효 피격·취소 우선순위와 오소리 전투를 묶어 실제 맞고 때리는 한 사이클을 만든다. 사용자가 현재 결과를 검토하기 전 자동 착수하지 않는다. 커밋/푸시는 하지 않았다.

## 최종 검증 기록

최종 새 Unity 실행은 Editor9+실제 Play54=63개 모두 통과했다. 171개 기존 Assets/Packages/ProjectSettings 파일을 SHA256 비교했으며 변경은 의도한 PlayerMovement.cs·PlayerLocomotion.cs·Player.prefab 세 파일뿐이었다. 기존 설정14개 대조도 통과했다. 마지막 실행 시작 당시 열린 PlayerCombatTest 씬의 clean 상태로 복귀했다. 이전 실행은 당시 열린 픽셀 비교 씬으로 복귀했다. 사용자 스테이징 변경은 유지했다. 두 임시 C# 검사 도구와 각각의 .meta, ready/request 마커 및 임시 복사 씬을 제거했고 최종22개 C# 소스의 별도 컴파일을 통과했다(CS0649 Inspector 참조 경고12개, 오류0). 명중 콜백 중 취소 가드를 보강한 뒤 최종 검사를 다시 실행했다. 재로드로 중단된 실행 및 Space를 계속 누른 테스트 입력 때문에 실패한 중간 실행은 최종 통과로 세지 않았으며, 테스트에서 release 후 새 press를 생성하도록 고쳤다. 최종 로그는 아래와 같다.

```text
Combat validation / 2026-11-02T22:33:14.8752708+09:00
EDITOR PASS Prefab attack exists
EDITOR PASS Prefab weapon locked
EDITOR PASS Prefab traversal locked
EDITOR PASS Collider tuning preserved
EDITOR PASS Locomotion attack reference connected
EDITOR PASS Attack action reference connected
EDITOR PASS Separate ground and air data
EDITOR PASS Data tuning serialized
EDITOR PASS Three preview renderers connected
PLAY PASS Starts grounded
PLAY PASS Test scene weapon unlocked
PLAY PASS Target has compound hurtboxes
PLAY PASS Locked weapon rejects press
PLAY PASS Locked press not retained
PLAY PASS Normal acceleration reaches 6
PLAY PASS Normal deceleration stops
PLAY PASS Attack starts windup from input
PLAY PASS Ground attack selected
PLAY PASS Windup movement cap
PLAY PASS Windup jump blocked
PLAY PASS Active movement cap
PLAY PASS Active applies one damage across compound colliders
PLAY PASS Damage reaction and knockback payload
PLAY PASS Active jump blocked
PLAY PASS No repeated damage during active
PLAY PASS Early recovery jump blocked
PLAY PASS Recovery finishes
PLAY PASS Holding attack does not repeat
PLAY PASS New press starts next attack
PLAY PASS New attack may hit same target
PLAY PASS Recovery movement gradually restores
PLAY PASS Late recovery permits jump cancel
PLAY PASS Valid late jump cancels recovery
PLAY PASS Air attack selected
PLAY PASS Air attack preserves gravity
PLAY PASS Landing preserves air attack selection
PLAY PASS Attack before disable
PLAY PASS Attack component disable cancels
PLAY PASS Enable does not resume attack
PLAY PASS Attack reader disable cancels immediately
PLAY PASS Movement input disable cancels immediately
PLAY PASS Movement component disable cancels
PLAY PASS Locomotion disable cancels
PLAY PASS Relock cancels attack
PLAY PASS Wall sliding detected
PLAY PASS Wall sliding rejects attack
PLAY PASS Wall jump regression
PLAY PASS Attack preserves outward wall control lock
PLAY PASS Separated wall permits left air attack
PLAY PASS Left facing mirrored hitbox
PLAY PASS Left knockback mirrored
PLAY PASS Actual render capture saved
PLAY PASS Cancel removes hit sampling
PLAY PASS Whiff does not damage distant target
PLAY PASS Jump cut regression
PLAY PASS Platform grounded before coyote test
PLAY PASS Walked off platform
PLAY PASS Ground coyote regression
PLAY PASS Wall contact before grace test
PLAY PASS Separated from wall before late press
PLAY PASS Wall coyote regression
PLAY PASS Buffered prelanding jump regression
PLAY PASS Cancel inside hit callback stops remaining targets
Temporary ground tuning: 0.24 / 0.12 / 0.4s; air 0.18 / 0.12 / 0.32s
SUCCESS PLAY 54
RESTORED Assets/_Project/Scenes/Tests/PlayerCombatTest.unity|dirty=False
```
