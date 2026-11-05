# 오소리 전투 사이클 — 2026-11-03

## 1. 범위와 상태

선행 커밋은 사용자가 만든 **141c843**으로 확인했다. 시작 시 작업 트리는 깨끗했다. 그 이후 승인된 체력·피격·사망과 오소리 전투만 구현한다. **구현·Unity 에셋 생성·실행 검사·임시 정리 완료, 사용자 리뷰 대기.** 최종 22개(6 Editor + 16 실제 Play Mode) 검사가 통과했다. 최종 36개 C# 소스를 Runtime 29개와 Editor 7개로 분리 컴파일해 모두 통과했다. 이 작업을 자동 커밋/push하지 않는다.

오소리 외형은 요청한 정사각형만 사용한다. 경고는 상태 진입 시 Debug.Log 한 번과 선택적인 사용자 prefab이다. 임의 VFX·색상 플래시·HUD·Gizmo는 만들지 않았다.

## 2. 수정 파일

루트: `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN`.

| 파일 | 위치·역할 |
| --- | --- |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/AttackHitReceiver.cs` | AttackHit에 명시적 damage/reaction/knockback/point 생성자 추가. 기존 대검 데이터 생성자는 유지. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerCombatCoordinator.cs` | OnEnable/OnDisable에서 체력 사망·피격 이벤트 연결, Prepare에서 사망/피격을 공격보다 먼저 처리. 즉시 공격·예약 입력 취소. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerLocomotion.cs` | SetExternalMotion과 CalculateVelocity의 일반 외부 이동 분기. 반응 중 점프/벽 이력 제거, 수평 입력 대신 제안받은 속도 사용. Combat 클래스 직접 참조 없음. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab` | 체력·반응·수신기 추가, 플레이어 레이어 8, 공격 targetLayers에 적 레이어 9 추가. 이동 수치·콜라이더·원화·기본 무기 잠금 유지. |
| `AGENTS.md`, `PLAYER_SETUP.md`, `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Plans/2026-11-03-health-reaction-badger-unit.md` | 승인/구현 상태, 연결·시험·제한 기록. |

## 3. 추가 파일과 책임

다음 경로는 모두 위 프로젝트 루트 하위다. 새 `.meta`는 Unity가 생성한다.

| 파일 | 생성 목적과 연결 |
| --- | --- |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Combat/CombatHealth.cs` | 최대/현재 체력·피해·사망 통지. 플레이어/오소리 공통. HP 변화는 Console 로그와 currentHealth Inspector로 확인. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Combat/CombatReaction.cs` | Stagger/Launch/Down/GetUp 단계와 일회성 발사·수평 넉백 감쇠 계산. Rigidbody를 쓰지 않음. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Combat/CombatDamageReceiver.cs` | 기존 AttackHitReceiver를 구현해 체력·피격으로 전달. 짧은 중복 피격 보호, HitReceived 이벤트. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerData.cs` | 탐지·추적·공격·시간·피해·넉백·사용자 경고 prefab 슬롯. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerMotor.cs` | 오소리 Rigidbody의 유일한 속도 쓰기 담당. 지면·벽·앞쪽 절벽과 중력 처리. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerAttack.cs` | Active에서만 플레이어를 Box 판정. 공격당 수신기 1회, 취소 시 닫음. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerBrain.cs` | Idle/Alert/Chase/Windup/Active/Recovery/Return/Reacting/Dead 선택. 경고 진입 Debug.Log, 사용자 prefab 수명 관리. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/BadgerCombatSceneBuilder.cs` | Create가 Editor API로 기존 플레이어 연결과 정사각형 적·평지 테스트 씬 생성. 기존 씬은 보존. |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Enemies/BadgerData.asset` | 오소리 임시 수치 에셋(실제 Editor 생성 완료). |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Enemies/Badger.prefab` | 0.7u 정사각형 표시/충돌체와 전투 구성(실제 Editor 생성 완료). |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scenes/Tests/PlayerBadgerCombatTest.unity` | 플레이어와 오소리 1마리의 평지 전투. 무기는 테스트 인스턴스만 해금(실제 Editor 생성 완료). |
| `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-badger-combat.md` | 본 변경 보고서. |

영구 삭제/이름 변경은 없다. BadgerValidation.cs/BadgerValidationDriver.cs 및 meta/요청/검사 복사 씬은 일시적인 엔진 검사 도구이며 최종 실행 후 제거했다.

## 4. 실제 흐름

접근 → Alert(Debug.Log 1회, 사용자 prefab이 있으면 생성) → 경고 시간 경과 → Chase → 근접 거리 Windup → Active(Box 판정) → Recovery → 다시 추적/공격. 경고/추적 중 대상이 너무 높거나 추적 한계를 벗어나면 Return. 공격 방향은 Windup 진입 전 고정하며 Active 중 몸 접촉이 아닌 공격 판정만 피해를 준다. 경고 prefab은 경고 종료/중단/사망/disable 때 제거된다. 빈 슬롯이면 아무것도 만들지 않는다.

대검 Active → 기존 PlayerAttackHitbox → 오소리 CombatDamageReceiver → CombatHealth 피해 → CombatReaction → AI 공격 중단/넉백 → 반응 종료 후 추적 재개. 선제공격하면 경고 대기 없이 피격 후 대응할 수 있다. 사망하면 Dead로 전환해 공격을 닫고 이동을 멈춘다.

오소리 Active → 플레이어 CombatDamageReceiver → 체력 감소·Stagger → PlayerCombatCoordinator가 현재 공격/콤보 예약 취소 → PlayerLocomotion이 입력 대신 반응 속도 선택 → PlayerMovement만 플레이어 Rigidbody에 적용. 강한 테스트 피해는 Launch → 착지 → Down → GetUp → 자동 정상 복귀를 따른다. 사망은 반응보다 우선하고 이동·점프·공격을 막는다.

## 5. 설계와 트레이드오프

공통 체력/반응은 AI를 모르고, 수신기는 기존 공격 API를 재사용한다. 플레이어 이동은 전투 타입을 직접 알지 않고 일반 외부 이동 요청만 받는다. 플레이어/오소리는 각각 하나의 Rigidbody 속도 쓰기 담당자를 유지한다. 인터페이스/스탯 프레임워크는 추가하지 않는다.

고정 레이어 8(플레이어), 9(오소리), 지형 0을 테스트 연결에 사용한다. 기존 공격 대상 mask 51을 제거하지 않고 9 비트를 더한다. TagManager나 전역 충돌 행렬은 변경하지 않는다. 현재 프로젝트에서 이 레이어를 다른 용도로 재사용할 때 연결을 함께 검토해야 한다.

적 지형 검사는 앞쪽 벽 Cast와 앞발 지면 Raycast를 사용하며 추적 범위·높이를 분리한다. 복잡한 경로 탐색은 없으므로 끊긴 지형을 돌아가는 AI가 아니다. 반응 넉백으로 강제로 지형 밖에 밀려나는 경우까지 자동 복귀/리스폰하지 않는다. 실제 적 아트·피격·사망 애니메이션은 없다.

## 6. Inspector와 직접 확인

씬 생성 후 PlayerBadgerCombatTest에서 A/D 이동, Space 점프, Enter/마우스 왼쪽으로 공격한다. Console에서 `[Badger] 경고 시작`과 HP 로그를 확인한다. 경고는 매 프레임 출력하지 않는다. 오소리의 State, CombatHealth의 Current Health, CombatReaction의 Phase를 Inspector에서 확인한다. 몸만 겹쳐서는 HP가 줄지 않고 Active에서만 줄어야 한다. 죽으면 Play를 다시 시작해 재검사한다.

BadgerData의 Warning Effect Prefab에 사용자가 만든 prefab을 연결한다. 슬롯 None은 의도적으로 표시 없음이다. 생성 메뉴는 이미 만든 씬을 덮어쓰지 않으며 프리팹 기본 무기 잠금은 유지한다.

## 7. 임시값

플레이어/오소리 HP 50, 피격 무적 0.15s, Stagger 0.2s, Down 0.45s, GetUp 0.25s, 넉백 수평 감쇠 10u/s². 오소리 탐지 4u/홈 기준 추적 6u/높이 차 1.25u/이동 1.4u/s/근접 0.9u, 경고 0.6s, 준비 0.35s, 타격 0.12s, 회복 0.65s, 피해 8, Stagger 넉백 (1.5,1). 공격 Box는 수평 0.55u 전방에 0.9×0.65u다. 현재 캐릭터 높이 약 0.9375u를 기준으로 잡은 조정값이며 최종 밸런스가 아니다.

## 8. 검사와 미검증

**최종 22개 검사 통과: 6 Editor + 16 실제 Play Mode.**

- Editor: 플레이어 수신기, 승인된 속도 3, 기본 프리팹 무기 잠금, 적 컴포넌트, 0.7u 정사각형, 경고 prefab None 확인.
- 실제 Play: 접근 후 경고, 경고 매 프레임 로그 없음, 실제 오소리 타격, 몸체 접촉 무피해, 실제 공격 입력으로 복합 콜라이더 적에게 한 타격, 후속타 예약, 강한 테스트 피격 수신, 공격/예약 즉시 취소, Launch, 입력보다 넉백 우선, 자동 기상, 절벽 낙하 방지, 높은 대상 추적 종료, 플레이어 사망 후 행동 차단/추가 피해 거절, 적 사망 AI 종료.
- 일반 공격은 실제 입력·물리로 검사했다. Knockdown은 오소리 일반 공격이 아닌 테스트 전용 강한 AttackHit를 수신기에 전달한 뒤 실제 물리·착지·다운·기상을 관측했다. 오소리는 설계대로 Stagger 공격만 한다.
- 시작 당시 열린 씬 목록·활성 씬·dirty 상태 비교는 RESTORED 통과. 임시 소스 2개와 meta/request/ready, 복사 씬/meta 제거 완료.
- 초기 임시 검사기가 런타임 영역에서 프로젝트 Editor 클래스를 참조해 Unity 컴파일이 막혔다. 그 참조를 제거한 후 새로 검사했다. 처음 별도 컴파일은 소스를 한꺼번에 합쳐 이 문제를 발견하지 못했으므로, 최종 검사는 **Runtime 29개 → 그 DLL을 참조하는 Editor 7개**로 분리해 각각 오류 0을 확인했다. 이를 Play 검사 수에 포함하지 않는다.
- 최종 Assets 코드 검색에서 커스텀 Gizmo/Handles/Debug.DrawLine/DrawRay 0개. 새로운 HUD·임의 효과 디자인 없음. 실제 사용자가 만든 WarningEffectPrefab의 생성/종료 동작은 코드에 연결했지만 그 아트가 아직 없어 실제 사용자 효과 검증은 미실행이다.
- 기존 이동/점프/벽 코요테·전투 전체 회귀를 전부 다시 검사한 것은 아니다. 사용자 물리 장치의 조작감, 완성 아트, 저장/리스폰과 실전 레벨은 미검증이다. 검사 중 추가 씬 AudioListener 중복 경고가 있었으며 원래 씬은 검사 중에만 비활성화하고 종료 후 복원했다.

기준 커밋 141c843 대비 기존 추적된 코드 수정은 AttackHitReceiver/PlayerCombatCoordinator/PlayerLocomotion과 Player.prefab 및 기록 문서다. 기존 씬·이동 수치 소스·공격 데이터·Input Actions·패키지·ProjectSettings는 변경하지 않았다. 모든 기존 GUID를 보존했다. 이번 작업의 새 커밋이나 push는 하지 않았다.

## 9. 제외·다음 단계

저장/체크포인트 부활·아이템/드롭·회복·멧돼지·실제 숲 레벨·최종 아트/VFX/HUD는 제외한다. 사용자 리뷰 전 다음 단위로 넘어가지 않는다.

## 최종 Unity 검사 로그

```text
Character animation validation 2026-11-03T21:44:34.3253526+09:00
EDITOR PASS Player receiver connected
EDITOR PASS Accepted speed three preserved
EDITOR PASS Base weapon lock preserved
EDITOR PASS Enemy components connected
EDITOR PASS Square placeholder only
EDITOR PASS No invented warning effect
SUCCESS EDITOR 6
PLAY PASS Proximity starts warning before attack
PLAY PASS Alert does not log every frame
PLAY PASS Actual badger Active deals player damage
PLAY PASS Body contact does not deal damage
PLAY PASS Player real input hits compound enemy once per swing
PLAY PASS Followup attack can be reserved
PLAY PASS Strong test hit accepted
PLAY PASS Hit immediately cancels attack and reservation
PLAY PASS Knockdown starts launch
PLAY PASS Player input does not overwrite reaction motion
PLAY PASS Knockdown lands and automatically gets up
PLAY PASS Enemy refuses to walk over a cliff
PLAY PASS Unreachable high target ends pursuit
PLAY PASS Death suppresses player movement jump and attack
PLAY PASS Dead player rejects extra hits
PLAY PASS Enemy death cancels AI
SUCCESS PLAY 16
RESTORED
```
