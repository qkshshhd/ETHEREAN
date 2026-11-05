# 회복 아이템 획득·사용·취소·드롭

## 구현 결과와 범위

회복 아이템의 접촉 획득, 소지 상한, 느린 지상 사용, 정상 완료 시 회복/소비, 유효 피격 시 취소와 아이템 보존, 적별 확률 드롭과 거리/화면 밖 정리를 구현했다. 기존 사각형을 임시 아이템으로 사용한다. 원화·회복 몸 애니메이션·HUD·코드 생성 VFX는 이번에 제작하지 않았다. 사용자 VFX 슬롯은 None으로 두었다.

기존 협업 마스터2.3의 초기 정책대로 지상에서만 회복한다. 수치와 가정은 `Docs/Assumptions.md`에 별도로 기록했다. 같은 플레이어의 사망/비활성화에서 수량·누적 소비량을 유지한다. **새 플레이어 생성·체크포인트 복귀·게임 재실행 후 저장 복원은 다음 세이브 단위의 작업이며 이번에 영구 저장을 구현한 것은 아니다.**

## 변경 파일

아래 경로의 루트는 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN`이다. 기존 GUID와 사용자 미커밋 작업을 유지했다.

| 파일 | 변경 위치·기능 |
|---|---|
| `Assets/_Project/Scripts/Runtime/Combat/CombatHealth.cs` | Heal(amount) 추가. 살아 있을 때만 최대치까지 회복하고 실제 회복량을 반환. 피해/사망 처리 유지 |
| `Assets/_Project/Scripts/Runtime/Player/Combat/PlayerCombatCoordinator.cs` | Prepare에서 피격/사망 우선 → 회복 → 공격 순서 조정. 회복 중 이동 배율과 공격/점프 제한. 종료 이벤트의 남은 입력 정리 |
| `Assets/_Project/Scripts/Runtime/Player/Movement/PlayerLocomotion.cs` | DiscardJumpRequests 추가. 점프 버퍼·벽차기 대기·입력 원본 요청을 함께 비움. 실제 속도 쓰기는 여전히 PlayerMovement만 담당 |
| `Assets/InputSystem_Actions.inputactions` | Player/Heal Button 및 R·Gamepad dpad/up 바인딩 추가. 기존 입력의 ID/맵/액션/바인딩/컨트롤 스킴 동일함을 JSON 비교로 확인. Native ToJson이 최상위 version:1 표시를 추가했으며 패키지 업그레이드는 아님 |
| `Assets/_Project/Prefabs/Player/Player.prefab` | PlayerInventory/PlayerHealing/NewInputHealingReader와 데이터/입력 참조 연결. 수량 시작값0. 기존 이동/점프/검·몸 공격 애니메이션 비활성 설정 유지 |
| `Assets/_Project/Prefabs/Enemies/Badger.prefab`, `Boar.prefab` | EnemyHealingDrop과 적별 드롭 데이터 참조만 추가. 기존 AI/체력/공격/표시 수치 유지 |
| `AGENTS.md`, `PLAYER_SETUP.md`, `Docs/Plans/ETHEREAN_Daily_Plan_2026-11-08.md` | 완료 상태·임시 수치·검증·저장 연계의 남은 범위 기록 |

## 추가 파일

새 Unity 스크립트/자산에는 meta가 함께 생성된다.

| 파일 | 책임과 연결 |
|---|---|
| `Assets/_Project/Scripts/Runtime/Player/Items/HealingItemData.cs` | 최대 소지량·회복량·사용 시간·이동 배율·사용자 VFX 슬롯 설정 |
| `Assets/_Project/Scripts/Runtime/Player/Items/PlayerInventory.cs` | 현재 수량·누적 소비량·상한·획득·소비. Changed 이벤트로 접촉 중인 아이템이 빈자리를 다시 확인 |
| `Assets/_Project/Scripts/Runtime/Player/Items/NewInputHealingReader.cs` | 전용 액션 복제/해제,1회 입력 전달. 입력 해제 시 대기 요청 취소 |
| `Assets/_Project/Scripts/Runtime/Player/Items/PlayerHealing.cs` | 회복의 시작·진행·취소·완료. 유효 HitReceived/사망/입력 비활성 구독. Rigidbody를 쓰지 않음 |
| `Assets/_Project/Scripts/Runtime/World/HealingPickup.cs` | 실제 접촉에서만 자동 획득. 가득 차면 유지, 빈자리 발생 시 재시도. 중복 콜라이더/이벤트 재진입 방지. 드롭만 거리+화면 조건으로 정리 |
| `Assets/_Project/Scripts/Runtime/Enemies/HealingDropData.cs` | 적별 드롭 프리팹·확률·생성 오프셋·소멸 거리 설정 |
| `Assets/_Project/Scripts/Runtime/Enemies/EnemyHealingDrop.cs` | 공통 CombatHealth.Died에서1회 확률 추첨/생성. AI와 분리 |
| `Assets/_Project/Scripts/Editor/HealingItemBuilder.cs` | 기존 Input Actions를 보존해 Heal 추가, 프리팹/데이터/입력/적 드롭/테스트 씬 연결 |
| `Assets/_Project/Data/Player/HealingItem.asset` | 현재 임시 회복 설정 |
| `Assets/_Project/Data/Player/PlayerHealingAction.asset` | Player/Heal InputActionReference |
| `Assets/_Project/Data/Enemies/BadgerHealingDrop.asset`, `BoarHealingDrop.asset` | 적별 독립 드롭 설정 |
| `Assets/_Project/Prefabs/Items/HealingPickup.prefab` | 기존 작은 사각형과 Trigger만 있는 임시 필드 아이템 |
| `Assets/_Project/Scenes/Tests/PlayerHealingTest.unity` | 플레이어, 고정 회복 아이템4개, 오소리와 멧돼지로 확인하는 별도 씬 |
| `Docs/Assumptions.md`, 이 보고서, `2026-11-05-healing-items-validation.txt` | 임시 정책·코드 설명·실제 검증 로그 |

임시 HealingValidation.cs/HealingValidationDriver.cs와 meta, request/ready, 복사 씬은 검사 후 제거했다. 영구 파일 삭제나 이름 변경은 없다.

## 입력에서 완료까지

1. R키 또는 게임패드 방향키 위를 누르면 NewInputHealingReader가 요청1회를 저장한다.
2. PlayerCombatCoordinator.Prepare가 피격/사망을 우선 처리한다. 정상 제어 가능 상태이면 PlayerHealing.Tick이 요청을 소비하고 지상/벽/공격/체력/수량을 확인한다.
3. 회복 시작 시 이동 배율0.35를 설정하고 공격·점프 요청을 버린다. PlayerMovement의 기존 최대속도3에 배율을 적용하므로 지상 이동은1.05u/s이다. 기존 Rigidbody 쓰기 소유권은 바뀌지 않는다.
4. 진행 중 유효 피격은 HitReceived를 통해 즉시 Cancel한다. 피격 반응이 억제돼 Started 이벤트가 발생하지 않는 경우에도 회복은 취소된다. 무적 시간으로 거부된 피격은 유효 피해가 아니므로 취소하지 않는다.
5. 사용 시간이 지나면 완료 대기만 설정한다. LateUpdate에서 해당 프레임의 물리 피해 처리가 끝난 뒤 유효 상태를 재확인하고 체력을 회복한 다음1개를 소비한다. Heal 메서드는 상태 변경 이벤트를 호출하지 않으므로 회복과 소비 사이에 별도 프레임을 두지 않는다.
6. 종료 시 Ended 이벤트로 버퍼와 원본 공격/점프 요청을 비우고 회복 입력도 정리한다. 마지막 FixedUpdate 이후 들어온 입력이 다음 프레임에 새 공격/점프/회복으로 실행되지 않게 한다.

회복에 별도 거대한 FSM을 만들지 않고 IsHealing/진행 시간/완료 대기 상태를 기존 조정자에 연결했다. 소지 수량, 체력 계산, 행동 제약과 물리 쓰기를 각 책임에 남겼다.

## 시작·취소 정책

| 조건 | 처리 |
|---|---|
| 체력 최대/아이템0/공중·벽/기존 공격 중 | 시작 거부, 입력 예약 없음 |
| 회복과 공격·점프를 동시에 새로 입력 | 회복을 우선하고 공격·점프 요청 폐기 |
| 회복 중 이동 | 느린 지상 이동 허용 |
| 회복 중 반복 입력/버튼 유지 | 추가 사용 예약 없음 |
| 유효 피격/사망/지상 이탈/입력·이동·회복 제어 비활성 | 회복 취소, 회복량0·소비0, 자동 재시작 없음 |
| 진행 중 외부 기능으로 체력 최대 | 취소하고 소비 없음 |
| 정상 완료 | 최대 체력을 넘기지 않게 회복, 아이템1개와 누적 소비1회 적용 |

전역 게임 시간 정지에서는 진행도도 멈춘다. 체크포인트·저장 UI 등 이후 제어 체계와의 연결은 해당 단위에서 검토한다.

## 획득과 드롭

필드 아이템은 Default 레이어0의 Trigger로 두었다. 플레이어8과 적9의 물리 충돌 무시 정책을 아이템에 재사용하면 Trigger가 막히므로 Enemy 레이어를 사용하지 않는다. 지형 센서는 기존처럼 Trigger를 제외한다.

복수 플레이어 콜라이더를 같은 PlayerInventory로 묶고, 이미 수거된 상태/재진입 guard로 중복 지급을 방지한다. 가득 차서 남은 아이템은 수량 Changed 때 실제 콜라이더 접촉을 다시 확인하고 재시도한다. 이전 Trigger 기록만 남았거나 멀어진 상태에서는 획득하지 않는다. 획득 이벤트에서 다시 Changed가 발생해도 재진입하지 않는다.

적 드롭은 별도 EnemyHealingDrop이 담당하며 AI를 바꾸지 않는다. 사망1회에 확률 추첨1회만 수행한다.0%는 항상 미생성,100%는 항상 생성한다. 생성 시 해당 인스턴스를 monsterDrop으로 표시한다. 고정 배치 아이템의 기본 표시는 false다.

드롭은 **거리16u 이상 AND 실제 게임 카메라의 프러스텀 밖**일 때만 제거한다. Renderer 전체 bounds로 판정해 일부가 화면에 걸친 경우 남긴다. Scene 뷰의 가시성에 영향을 받는 Renderer.isVisible 대신 게임 카메라를 사용한다. 플레이어나 유효 카메라를 찾지 못하면 보수적으로 남긴다.

## Inspector와 직접 확인

- `HealingItem.asset`: 최대3개, 회복량20,1.2초, 이동35%. 사용자 HealingEffectPrefab은 None이다.
- `BadgerHealingDrop.asset`/`BoarHealingDrop.asset`: 임시 확률35%/50%, 생성 오프셋(0,0.1), 소멸 거리16u. 검토 전 확정값이 아니다.
- 플레이어 Inspector의 PlayerInventory.Current Count/Total Consumed와 CombatHealth.Current Health, PlayerHealing.Is Healing을 확인한다. 획득·회복 시작/완료/취소는 Debug.Log로도 확인한다. 임의 HUD는 추가하지 않았다.
- `PlayerHealingTest.unity`에서 오른쪽으로 이동해4개 필드 아이템을 만난다. 최대3개를 채우면 마지막 아이템은 남는다. 적에게 피해를 받은 뒤 R로 회복하거나 피격으로 취소해 본다. 가득 찬 아이템과 접촉한 채 회복을 마치면 소비한 자리에 즉시 새 아이템을 주워 수량이 다시3개가 될 수 있으며, Total Consumed는1회 늘어난다.
- 제작 메뉴는 `ETHEREAN > Tests > Create Healing Item Test`. 데이터/씬을 새로 만드는 경우에만 기본값을 만들며 기존 수치·바인딩을 초기화하지 않는다. 기존 플레이어/적 프리팹에는 연결 컴포넌트 참조를 적용한다.

## 검증 결과

최종 실제 Unity6000.4.7f1 검사 **59개(10 Editor + 49 Play Mode)**가 통과했다. 초기 실행 후 종료 프레임 입력 및 오래된 접촉 기록 조건을 보완하고 전체를 새로 재실행한 결과다.

- 실제 별도 가상 키보드 R/게임패드 dpad-up으로 시작, 느린 속도1.05와 정상 속도3 복구, 일시정지, 회복 최대치 제한/정상1회 소비, 유지 입력 비반복을 확인했다.
- 회복 중 마지막 구간의 공격·점프 입력, 마지막 물리 틱 이후 들어온 세 입력을 정리하는 조건을 확인했다. 공격 중/공중 시작 거부, 착지 후 자동 사용 없음, 지상 이탈 취소도 통과했다. 벽 플래그 거부는 제어된 Tick 조건 검사다.
- 실제 DamageReceiver의 유효 피격/반응 억제 피격은 취소하고, 무적으로 거부된 피해는 취소하지 않는 조건, 입력/회복 컴포넌트 해제, 외부 최대 회복, 동일 시점의 피해와 완료 우선순위를 확인했다. 완료 경계는 실제 피해 수신 API와 제어된 완료 대기를 조합한 검사이며 모든 가능한 엔진 실행 순서를 열거한 것은 아니다.
- 같은 플레이어의 사망 및 비활성화/재활성화에서 수량과 누적 소비량 유지, 실제 Trigger 획득/복수 콜라이더/한도 잔류/접촉 중 재시도/멀어진 뒤 수거 방지/죽은 플레이어 미획득을 확인했다.
- 고정 원거리 아이템 잔류, 드롭의 멀지만 화면 안/가깝지만 화면 밖 잔류, 멀고 화면 밖일 때 제거, 양쪽 적에서 확률0/1 생성과 사망 중복 드롭 방지를 확인했다. 적 드롭 검사에서는 데이터 복제본의 확률을 변경했고 저장된 기본35%/50%는 유지했다.
- 복사 씬 종료 후 원래 열린/활성/dirty 씬 상태 비교 `RESTORED`. 임시 검사 코드/meta/요청/ready/복사 씬 모두 제거.
- 정리 후 영구 **Runtime45개 / Editor13개**를 각각 분리 컴파일하여 exit0 확인. 직렬화 필드 CS0649 경고는 있으나 오류는 없다.
- 스냅샷에서 기존 파일 변경은 InputActions, 플레이어/두 적 프리팹, CombatHealth/PlayerCombatCoordinator/PlayerLocomotion의7개뿐이다. 누락0. 기존 입력은 Heal과 최상위 버전 표시를 제외하면 JSON 의미가 동일하다. 기존 이동·점프·검·공격 데이터, 애니메이션, 저장 씬, Packages/ProjectSettings와 기존 GUID를 보존했다.

실제 아트/VFX/HUD, 최종 밸런스, 새 플레이어/재실행 저장 복원은 미구현이다. 회복 아이템 단위에서 멈추고 사용자 코드·플레이 리뷰를 기다린다. 다음 기능·커밋·푸시는 진행하지 않았다.
