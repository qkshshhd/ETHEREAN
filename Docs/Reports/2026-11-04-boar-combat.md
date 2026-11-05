# 멧돼지 전투와 애니메이션 연결 구조

> 후속 상태(2026-11-05): 이 문서는 최초 사각형 구현 기록이다. 현재는 제공된 Idle/Walk/Run/새 Death를 연결했으며, 준비·기절은 이후 사용자 VFX를 기다리며 Idle 첫 자세를 유지한다. 최신 표시 방식과 원본 크기 보존/검증은 [멧돼지 애니메이션 보고서](2026-11-05-boar-animation.md)를 따른다.

## 목표와 범위

멧돼지를 현재 플레이어 크기에 맞춘 단순0.9u 사각형 프리팹으로 구현한다. 경고·돌진 준비·돌진·회복·벽 충돌 기절·절벽 중단·슈퍼아머·피격/사망과 향후 애니메이션 연결 자리를 포함한다. 실제 몸 애니메이션과 사용자 VFX는 제작하지 않는다. 회복 아이템·저장·숲 진행·프롤로그·멧돼지 컷신은 이번 범위가 아니다.

## 변경·추가 파일

아래 파일의 전체 경로 루트는 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN`이다. 새 Unity 스크립트/자산에는 meta가 함께 생성된다.

| 파일 | 구분·위치·역할 |
|---|---|
| `Assets/_Project/Scripts/Runtime/Combat/CombatReaction.cs` | 수정. SuppressNewReactions 속성과 Begin의 반응 억제 조건 추가. OnDisable에서 억제 해제. 체력 피해 경로는 변경하지 않음 |
| `Assets/_Project/Scripts/Runtime/Enemies/BoarData.cs` | 추가. 인지/접근/홈 범위·상태 시간·돌진 속도·공격 범위/피해/넉백·사용자 경고 VFX 슬롯 |
| `Assets/_Project/Scripts/Runtime/Enemies/BoarBrain.cs` | 추가. 상태 전환, 방향 확정, 슈퍼아머/기절 기간, 타깃/홈 복귀, 경고 로그와 선택적 사용자 VFX 수명 관리 |
| `Assets/_Project/Scripts/Runtime/Enemies/BoarMotor.cs` | 추가. Rigidbody2D의 단일 속도 쓰기, 중력·접지, 다음 물리 이동량을 포함한 벽/절벽 구분 |
| `Assets/_Project/Scripts/Runtime/Enemies/BoarAttack.cs` | 추가. Charge에서만 타격 판정, 이전 위치와 현재 위치 사이 swept box, 돌진별 수신자 중복 방지, Knockdown 요청 |
| `Assets/_Project/Scripts/Runtime/Enemies/BoarAnimator.cs` | 추가. 행동·피격·사망 상태를 표시 상태에 연결. Inspector에서 상태 경로/원본 방향 보정 변경 가능. 이동/피해를 발생시키지 않음 |
| `Assets/_Project/Scripts/Editor/BoarCombatBuilder.cs` | 추가. Boar 데이터/빈 상태 Controller/사각형 프리팹/별도 테스트 씬 생성. 기존 파일은 삭제하거나 덮어 만들지 않음 |
| `Assets/_Project/Data/Enemies/BoarData.asset` | 추가. 현재 크기에 맞춘 임시 전투 수치 |
| `Assets/_Project/Prefabs/Enemies/Boar.prefab` | 추가. 루트 물리·체력·피격·AI·공격·공통 배회 + 임시 Square + 발 기준 CharacterVisual |
| `Assets/_Project/Art/Animations/Boar/Boar.controller` | 추가. 기본9상태와 선택적 강피격3상태. Motion은 모두 비어 있음 |
| `Assets/_Project/Scenes/Tests/PlayerBoarCombatTest.unity` | 추가. 왼쪽 기절 유도 벽, 오른쪽 절벽, 무기를 해금한 테스트 플레이어와 멧돼지. 기존 오소리 씬은 그대로 |
| `Docs/Design/2026-11-04-boar-animation-states.md` | 추가. 사용자가 제작할 애니메이션·재사용 후보·후속 연결 절차 |
| `AGENTS.md`, `PLAYER_SETUP.md` | 현재 구현·검증·임시 수치·아트 대기 상태 기록 |
| 이 보고서와 검사 로그/캡처 | 추가. 실행 근거와 검토 절차 |

공통 CombatHealth/CombatDamageReceiver/EnemyRoaming과 기존 플레이어의 피격·Knockdown을 재사용한다. 오소리 소스를 멧돼지 조건으로 확장하거나 플레이어 물리 쓰기를 다른 컴포넌트에 추가하지 않는다. 현재 멧돼지의 다른 지형 검사 요구를 별도 Motor에 두어 기존 오소리 동작을 보존한다.

## 상태 전환

| 현재 상태 | 조건 | 다음 상태/동작 |
|---|---|---|
| Idle/Roam | 접근 가능한 플레이어가 활동 범위 내 인지 거리 진입 | Alert, 진입 로그 |
| Idle/Roam | 타깃이 없음 | 기존 EnemyRoaming으로 홈 주변 가중 배회. 벽/절벽에서 방향 전환 |
| Alert | 경고 최소 시간·쿨타임 경과, 더 가까운 거리, 접지 | Windup. 방향 확정, 슈퍼아머 시작 |
| Alert | 타깃이 높거나 멀거나 활동 범위 이탈 | Return |
| Windup | 준비 시간 종료 | Charge. 확정 방향 유지, 돌진 판정 개방 |
| Charge | 다음 이동 구간에 유효한 정면 벽 | Stun. 판정 즉시 닫기, 슈퍼아머 해제 |
| Charge | 앞 절벽·돌진 시간 종료·홈 활동 한계 | Recovery. 기절 없이 판정/슈퍼아머 해제 |
| Stun | 고정 기절 시간 종료 | Alert 또는 Return |
| Recovery | 최소 회복 시간과 남은 공격 쿨타임 종료 | Alert 또는 Return |
| 슈퍼아머 없는 상태 | 피격 반응 발생 | Reacting, 실제 공통 반응/넉백 실행 후 Recovery |
| Return | 안전한 바닥으로 홈 도달 | Idle |
| 모든 상태 | 체력0 | Dead. 판정/슈퍼아머 종료, 이후 중력 유지 |

준비에 들어간 뒤에는 플레이어가 반대쪽으로 넘어가도 돌진 방향을 바꾸지 않는다. 준비/돌진을 시작한 뒤 타깃이 멀어지거나 공중으로 올라가도 확정 동작을 유지하고, 벽/절벽/기간으로 종료한다. 경고 단계에서는 높은 타깃을 따라 돌진하지 않는다.

## 슈퍼아머와 피해 처리

CombatDamageReceiver가 체력 피해를 적용하고, 살아 있으면 CombatReaction.Begin을 호출하는 기존 순서는 그대로다. 새 SuppressNewReactions는 Begin만 막는다. 따라서 준비/돌진 중에도 체력 피해와 HitReceived 통지는 정상이며, 기존 사용자 명중 VFX 경로도 피해가 수락되면 유지된다. 현재 두 반응 Stagger/Knockdown에 모두 저항한다. 사망은 항상 우선한다.

Stun은 슈퍼아머가 아니다. BoarBrain.IsSuperArmored는 false이며, 이미 기절 상태가 제어권을 소유하므로 새 움찔/날림으로 기절 시간을 덮어쓰지 않는다. 피해는 들어가지만 Stun의 종료 시각을 다시 설정하지 않는다. 컴포넌트를 끄면 판정/억제/경고 오브젝트와 수평 이동을 정리한다.

BoarMotor만 멧돼지 Rigidbody2D 속도를 쓴다. 평상시 지형 접촉은 기절을 만들지 않으며, Charge에서 정면 법선의 벽을 검사했을 때만 Stun으로 전환한다. 검사 길이에 돌진 속도×fixedDeltaTime을 포함한다. 절벽 검사도 다음 이동량만큼 앞에서 수행한다. 이 검사 방식은 평지와 명확한 벽/절벽용이며 복잡한 경사·움직이는 발판용 경로 탐색은 아니다.

BoarAttack은 이전 물리 위치와 현재 위치의 이동 구간을 포함한 직사각형으로 조회한다. Player/Enemy 물리 충돌이 꺼져 있어도 OverlapBox는 별개로 타격을 감지한다. 성공한 CombatDamageReceiver를 HashSet에 보관해 복수 콜라이더나 장시간 겹침에서도 돌진당1회 피해만 적용한다. 공격하지 않을 때 접촉 피해는 없다.

## 프리팹과 Inspector

- 루트: Enemy 레이어9,0.9×0.9 BoxCollider2D, gravityScale0의 Rigidbody2D, 연속 충돌 검사. 지형 레이어0과 충돌하고 플레이어8과는 기존 정책대로 서로 밀지 않는다.
- Square: 기존 MovementSquare 이미지와 기존 재질만 사용한0.9u 임시 사각형. 상태별 색/섬광/궤적/마커/코드 생성 VFX/Gizmo 없음.
- CharacterVisual: 발 기준 localY=-0.45, scale1. SpriteRenderer는 비어 있고 비활성, Animator는 빈 상태 Controller 연결. 실제 아트가 준비되면 이 Renderer를 켜고 Square를 끈다.
- BoarBrain의 Target은 테스트 씬의 플레이어 CombatHealth를 참조한다. 일반 배치 시 이 참조가 필요하다.
- BoarData의 WarningEffectPrefab 기본 None. 지정된 사용자 프리팹만 Windup 동안 생성/제거한다. None에서는 새 표시를 만들지 않으며 `[Boar] 경고 시작`, `돌진 준비 — 방향 확정`, `벽 충돌 기절` 진입 로그로 확인한다.
- BoarAnimator의 상태 경로는 Inspector에서 편집 가능하다. 원본 그림의 방향이 정해지면 invertSpriteFacing을 실제 좌우 렌더로 검토한다. 사각형에는 방향 모양을 임의로 추가하지 않았다.

## 임시 수치

| 항목 | 현재 값 |
|---|---|
| 최대 체력 / 피해 | 80 / 12 |
| 인지 / 돌진 접근 / 홈 한계 | 5 / 4 / 7u |
| 허용 높이 차 | 1.25u |
| 배회 속도 / 복귀 속도 / 돌진 속도 | 공통 배회0.55 / 1.1 / 8u/s |
| 경고 / 준비 / 최대 돌진 | 0.6 / 0.7 / 0.9초 |
| 최소 회복 / 기절 / 준비 시작 간 최소 쿨타임 | 0.8 / 1.6 / 2.6초 |
| Knockdown 넉백 | (5,4) |
| 돌진 타격 박스 크기 / 로컬 오프셋 | (1.05,0.65) / (0.15,0) |

실제 회복 대기는 최소 회복 시간과 남은 쿨타임 중 긴 쪽이다. 사용자 리뷰 전의 임시 조정값이며 최종 난이도 확정은 아니다. 플레이어 수평 속도3, 현재 점프/중력/검 크기, 보류한 몸 공격 애니메이션을 유지한다.

## 애니메이션 제작 목록

우선 Idle·Walk·Windup·Charge·Stun·Death의6종을 권장한다. Alert/Recovery는 처음에 Idle을 재사용할 수 있고, Stagger는 전용 움찔이 준비되면 연결한다. 강한 피격의 KnockdownLaunch/KnockdownDown/GetUp은 선택적3상태다. 별도 그림을12종 모두 당장 만들 필요는 없다.

자세한 상태별 조건·반복/단발·후속 연결 기준은 [애니메이션 제작 기준](../Design/2026-11-04-boar-animation-states.md)에 정리했다. 애니메이션에서 공격 피해나 이동을 호출하지 않는다.

## 검증 상태

최종 실제 Unity 검사 **59개(22 Editor + 37 Play Mode)**가 통과했다. 첫 실행22+35 이후 공중 사망 중력 경로를 수정하고, 추가 사례를 포함한 전체 검사를 새로 실행한 결과다. 로그는 `2026-11-04-boar-combat-validation.txt`다.

- Editor: Enemy 레이어/0.9u 몸체/임시 Square/별도 발 기준 표시 루트/데이터·공통 배회·표시 연결,12개 빈 상태와 Idle 기본 상태, 사용자 VFX None·발명한 클립 없음, 플레이어와 비충돌/지형과 충돌, 몸 공격 애니메이션 보류 설정을 확인했다.
- 실제 FSM/물리: 바깥 경고 거리에서 대기, 접근 후 준비·돌진, 준비 중 방향 고정, 준비·돌진 중 피해 수락 및 경직 저항, 실제 고속 절벽 정지, 정면 벽 기절, 기절 중3회 피해/기간 불변, 일반 접촉 시 비기절, 정상 시간 종료와 회복을 확인했다.
- 실제 피해/물리: 무적 시간을0으로 둔 검사 대상과 복수 콜라이더에 돌진당1회 피해, Knockdown 요청과 실제 플레이어 발사·넉백·자동 복귀를 확인했다. 검사에서만 무적 시간을 바꾸었고 저장된 게임 데이터는 바꾸지 않았다.
- 제어된4u 위치 이동의 swept query에서도 대상을1회 감지했고, 타격 박스 위의 플레이어는 맞지 않았다. 이는 실제4u/프레임 성능 측정이나 입력을 통한 점프 회피 검사가 아니라 제어된 쿼리 조건 검사다.
- 일반 상태의 Stagger, 기절/슈퍼아머보다 사망 우선, 공중 사망 후 낙하, 비활성화 시 판정/억제/이동 정리와 재활성화 시 이전 돌진을 재개하지 않는 조건, 공통 반응 기본값의 이전 동작 유지도 통과했다.
- 실제 상태에 따른 빈 Charge/Stun/Death/Stagger Animator 상태 선택을 확인했다. 몸 애니메이션 그림/프레임 재생 품질을 확인한 것은 아니다.
- 복사 씬 검사 후 원래 열린/활성/dirty 씬 상태 비교 `RESTORED`. 임시 BoarValidation 소스2개/meta/요청/ready/복사 씬 제거 완료. 두 카메라 캡처를 보고서 폴더의 `2026-11-04-boar-ledge.png`, `2026-11-04-boar-stun.png`로 보존했다.
- 정리 후 영구 Runtime38개 / Editor12개를 별도 컴파일하여 각각 exit0. 직렬화 필드 CS0649 경고는 있지만 오류는 없다.
- 스냅샷 비교에서 기존 Assets/Packages/ProjectSettings 중 변경은 CombatReaction.cs 하나뿐이고 누락은 없다. 플레이어 프리팹·보류한 몸 공격·검/이동/공격 수치·오소리·기존 씬·물리 설정·GUID를 보존했다. 새 프리팹/데이터/Controller/씬/스크립트는 별도 추가다.

## 직접 확인

1. `Assets/_Project/Scenes/Tests/PlayerBoarCombatTest.unity`를 열고 Play한다. 플레이어 몸 공격 애니메이션은 꺼진 상태이며 검과 피해는 동작한다.
2. 경고 로그와 BoarBrain의 State를 보고, 가까이 가서 준비를 유도한 뒤 점프로 넘는다. 준비 중 방향이 고정되고 돌진이 반대로 따라오지 않아야 한다.
3. 왼쪽 벽으로 돌진을 유도한다. Stun 동안 반격하되 타격마다 기절 시간이 늘어나지 않는지 본다.
4. 오른쪽 바닥 끝으로 유도한다. 절벽에서는 떨어지거나 기절하지 않고 Recovery로 멈춰야 한다.
5. 돌진 준비/실행 중 타격해 체력은 줄지만 경직으로 끊기지 않는지 확인한다. 돌진에 맞으면 플레이어는 Knockdown 후 자동 복귀한다.

실제 스프라이트/경고 VFX, 최종 밸런스, 복잡한 지형, 저장/재생성, 강제 전투 컷신은 이번에 완료했다고 표시하지 않는다. 사용자 검토 전 다음 기능이나 커밋/푸시를 진행하지 않는다.
