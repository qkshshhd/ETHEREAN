# 11월 3일 다음 개발 단위: 체력·피격·사망과 오소리

## 출발점과 범위

사용자는 수평 속도를 3으로 바꾸고 현재 플레이어 수치를 승인했다. Player.prefab에 horizontalMoveSpeed=3이 저장된 것을 확인했으며 새 컴포넌트 초기값도 3으로 맞춘다. 좌우 가감속 없음, jumpSpeed=8.986667, jumpCutMultiplier=0.51080966, riseGravity=32, fallGravity=52, maxFallSpeed=16, 벽차기 수평=4.5/수직=8.086154, 현재 사용자 콜라이더·32 PPU를 기준으로 한다. 사용자가 다시 요청하기 전 튜닝을 바꾸지 않는다.

최신 일일 계획표(2026-11-02 재계획)의 11-03 행은 체력·피격·사망과 오소리 전투다. 문서 아래의 예전 DAY 09 숲 전반부 목표는 최신 일정이 아니다. 현재 체력은 CombatTestTarget의 기록용 구현뿐이며 플레이어 체력·실제 피격·적 AI는 없다. 공격 3타와 이동은 구현돼 있다.

**사용자 승인 단위(2026-11-03 후속 지시):** 플레이어와 오소리가 서로 공격하고, 피해·경직/넉다운·행동 중단·자동 기상·처치/플레이어 사망까지 확인하는 별도 전투 테스트 씬. 검사·연결·설명까지 포함한다. 후속 사용자 지시에 따라 기존 Gizmo 제거와 현재 작업 커밋을 먼저 완료한 뒤 구현한다. 아직 전투 코드나 오소리 씬/프리팹을 만든 것은 아니다. 이번 승인에는 커밋 선행 조건이 있으며 push는 포함하지 않는다.

## 포함할 동작

1. 플레이어/적의 최대 체력·현재 체력·피해·사망 이벤트. 체력 0에서 추가 피해/행동 차단.
2. 약한 피격 Stagger와 강한 피격 Knockdown(Launch → Down → GetUp → 자동 복귀). 피해 데이터가 반응 종류를 결정한다.
3. 피격 시 공격/예약 입력을 취소하고 반응을 우선한다. 사망은 모든 일반 행동보다 우선한다. PlayerMovement만 플레이어 Rigidbody 속도를 쓴다.
4. 오소리의 영역 경고 → 접근 → 준비/타격/회복 근접 공격 → 이탈 복귀. 몸체 접촉은 피해를 주지 않는다. 벽·절벽·높이 차이에서 무한 추적하지 않는다. 오소리는 대검에 경직될 수 있다.
5. 별도 테스트 씬에서 Inspector/검증 로그로 체력·반응·AI 상태를 확인한다. 오소리 외형은 사용자가 명시한 단순 정사각형 프리팹만 사용한다. 경고 VFX는 사용자가 만든 프리팹을 연결하는 빈 슬롯으로 두며 슬롯이 비어 있으면 표시하지 않는다. 코드로 임의의 효과·마커·색상 플래시·HUD·디자인을 생성하지 않는다. 임시 Gizmo를 쓴다면 기능 완료 전 코드와 전용 필드/연결까지 제거한다.

강한 피격은 별도 테스트 발동으로 확인할 수 있게 하고, 오소리의 일반 공격 자체는 계획대로 Stagger를 사용한다. 사망 후 저장/체크포인트 복귀는 다음 단위이므로 이번 테스트에서는 Play 재시작으로 재검사한다.

## 클래스 책임 제안

| 대상 | 책임 |
| --- | --- |
| CombatHealth | 체력·피해 적용·사망 통지. 이동/AI를 직접 실행하지 않음. |
| CombatReaction | Stagger/Knockdown 단계·기간·넉백 상태 계산. Rigidbody를 직접 쓰지 않음. |
| PlayerDamageReceiver | 기존 AttackHitReceiver에서 플레이어에게 들어오는 피해를 체력/반응에 전달. |
| PlayerCombatCoordinator | 공격·피격·사망 우선순위를 연결하고 이동 계층에 일반 제약/외부 속도를 전달. |
| PlayerLocomotion / PlayerMovement | 기존 물리 순서와 단일 속도 적용 유지. 전투 클래스에 직접 종속되지 않도록 일반적인 외부 이동 요청 연결만 추가. |
| BadgerMotor | 오소리의 물리 이동·중력·벽/절벽 검사. 오소리 Rigidbody의 단일 쓰기 담당. |
| BadgerBrain | 경고/추적/공격 선택/복귀 및 체력·반응에 따른 행동 중단. |
| BadgerAttack / BadgerData | 공격 시점별 1회 판정과 Inspector 조정 수치. 캐릭터 외형과 분리. |
| BadgerDamageReceiver | 대검 피해를 오소리 체력/피격에 전달. |
| Inspector/검증 로그 | 체력·반응·AI 상태 확인. 새 코드 기반 HUD 디자인은 추가하지 않음. |

필요한 파일 수는 구현 과정에서 책임에 맞춰 줄이거나 합칠 수 있다. 사용하지 않는 인터페이스·스탯 시스템·대규모 상태 머신 프레임워크는 만들지 않는다.

## 수정·추가 예상 파일

프로젝트 루트: `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN`.

- 수정: `Assets/_Project/Scripts/Runtime/Player/Combat/PlayerCombatCoordinator.cs`, `Assets/_Project/Scripts/Runtime/Player/Movement/PlayerLocomotion.cs`, `Assets/_Project/Prefabs/Player/Player.prefab`의 체력/피격 참조와 제약 연결.
- 추가 제안: `Assets/_Project/Scripts/Runtime/Combat/CombatHealth.cs`, `CombatReaction.cs`; 플레이어 Combat 폴더의 `PlayerDamageReceiver.cs`; `Runtime/Enemies/`의 오소리 AI/이동/공격/피격 클래스; `Scripts/Editor/`의 전투 씬 생성 도구.
- 추가 제안: `Assets/_Project/Data/Enemies/BadgerData.asset`, `Assets/_Project/Prefabs/Enemies/Badger.prefab`, `Assets/_Project/Scenes/Tests/PlayerBadgerCombatTest.unity`와 Unity가 생성하는 meta.
- 기록: 단위별 상세 보고서, PLAYER_SETUP.md, AGENTS.md와 일일 계획의 실제 진행 상태.

## 검증 계획

- 체력 감소/사망 1회, 죽은 대상 재타격 차단, 복합 콜라이더 중복 피해 방지.
- 실제 공격 입력·Active에서만 피해, 피격 시 진행 중 공격/다음 타 예약 취소.
- Stagger 종료 복귀, Knockdown 발사/착지/다운/자동 기상, 반응 중 이동·공격 차단과 사망 우선순위.
- 오소리의 실제 경고/추적/공격/복귀·공격 중단/사망, 접촉 피해 없음, 절벽·벽·높은 플레이어 대응.
- 최고 수평 속도 3, 가감속 없음, 현재 점프 최고점과 벽차기 보호를 회귀 확인한다. 과거 속도 6으로 통과한 검사를 현재 속도 3의 새 검증으로 쓰지 않는다.
- 기존 파일/씬/dirty 상태·GUID·사용자 staged 픽셀 씬 보호, 임시 검사 도구 정리 후 별도 컴파일.

공격력·최대 체력·반응 시간·무적 시간·탐지/추적/공격 거리·오소리 속도는 아직 최종값이 아니며 현재 캐릭터 높이 약 0.9375u를 기준으로 Inspector 임시값을 문서화한다. 새 속도 3에서 기존 맵 2u 틈은 과거 통과 결과를 보장하지 않으며 레벨 검토에서 재조정 대상이다. 이번 전투 씬은 전투를 확인할 평지 위주로 만든다.

## 제외할 작업

멧돼지·회복 아이템·체크포인트·세이브·실제 A~I 숲·프롤로그 연출·드롭 시스템·최종 원화/공격 모션/VFX 제작, 커밋·push는 이 단위에 포함하지 않는다. 해당 기능을 완료했다고 표시하지 않는다.

## 구현 선행 조건

기존 커스텀 Gizmo와 전용 previewData/Editor 연결 제거 → 컴파일·정리 확인 → 현재까지 작업을 로컬 Git 커밋 → 오소리 전투 구현 순서다. Git 쓰기가 실행 환경에서 막히면 커밋 성공을 주장하거나 순서를 건너뛰어 새 전투를 시작하지 않는다. 사용자 로컬 커밋을 확인한 뒤 같은 승인 범위로 재개한다.

Warning feedback follow-up (2026-11-03): until user warning VFX exists, log warning-state entry once with Debug.Log and the enemy context. Do not log every frame or draw replacement markers/effects. This is part of the already authorized badger cycle after the prerequisite cleanup commit.
## Badger combat implementation in progress (2026-11-03)

Verified user prerequisite commit 141c843 and a clean starting worktree. User explicitly confirmed that commit. Implement only the approved combat cycle: common CombatHealth/CombatReaction/CombatDamageReceiver; generic external movement in PlayerLocomotion, priority/cancellation in PlayerCombatCoordinator; BadgerData/Motor/Attack/Brain; Editor-generated square prefab and separate combat scene. No invented VFX/HUD/marker/Gizmo. Alert entry logs Debug.Log once; optional user WarningEffectPrefab is destroyed on exit/disable/death and None creates nothing. Player keeps accepted speed=3, direct X movement, current jump/collider. Player root layer 8, badger 9, terrain 0; existing attack target mask adds bit 9. Runtime Inspector health/phase/state fields are for review. Separate compilation passes, but latest Unity import, prefab/scene creation, actual engine checks and temporary BadgerValidation cleanup are PENDING; do not claim completion. Helpers poll Logs/BadgerValidation.request once Editor imports them. See Docs/Reports/2026-11-03-badger-combat.md. No new commit/push or next unit until review.

## Badger combat completed; review pending (2026-11-03)

Prerequisite commit 141c843 verified. Unity generated PlayerBadgerCombatTest, Badger.prefab (plain 0.7u square), BadgerData.asset and player health/reaction/receiver connections. Player remains speed 3 with accepted tuning; player layer 8, enemy 9, terrain 0, player attack mask includes enemy bit. Warning logs once on Alert entry; optional user VFX slot defaults None. No invented VFX/HUD/Gizmo. Fresh validation passed 22 checks (6 Editor + 16 actual Play Mode): actual reciprocal damage/contact safety, compound collider hit, reservation and hit cancellation, Knockdown auto recovery, cliff/high-target behavior and death gating. Strong Knockdown was injected as a test hit; ordinary badger damage remains Stagger. Initial temporary Runtime->project Editor class reference caused Unity compile failure and was corrected. Final compilation independently passed Runtime 29 sources and Editor 7 sources referencing the runtime DLL. Temporary validators/meta/request/ready/copied scene removed; original open/active/dirty scene state restored. Gizmo/drawing-code matches zero. See Docs/Reports/2026-11-03-badger-combat.md for exact files, settings, tests and limitations. Custom user warning VFX, final art/reaction animation, save/respawn/drop/healing are not implemented or validated. Do not start next unit or commit/push before user review.
