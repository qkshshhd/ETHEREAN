# 캐릭터 애니메이션과 시간 기반 공격 판정 — 2026-11-03

## 1. 목표와 범위

최신 사용자 결정: 대검을 별도 프리팹으로 움직이지 않고 캐릭터 공격 애니메이션에 포함한다. 공격 판정은 검날 Transform이나 무기 모션이 아니라 입력 이후 설정된 준비·유효 판정·회복 시간으로 실행한다. 이 결정이 이전 대검 프리팹/클립/경로 판정 방식을 대체한다.

3타 순서와 입력 예약·이동 제한·후반 점프 캔슬·커스텀 VFX 프리팹 정책은 유지한다. 실제 캐릭터/대검 스프라이트와 공격 AnimatorController는 현재 프로젝트에 없으므로 임의 제작하지 않는다. 전투는 이 아트가 없어도 작동하며, 시각 애니메이션은 이후 사용자 리소스 연결이 필요하다. 완료 검증은 마지막 구역에 기록한다.

## 2. 수정·추가·삭제 파일

| 전체 경로 | 수정 위치·책임 |
| --- | --- |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttack.cs | Tick/AdvancePhase/StartStep/EnterPhase/CancelAttack: 검·모션 참조 제거, 시간으로만 단계 전환. 준비/Active의 이전 마감 시각을 이어 받아 누적 지연 방지. Active 밖의 마지막 경로 판정 제거. AttackProgress로 전체 타격 진행률 제공 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackData.cs | 무기 모션/검날 반경/샘플 밀도 제거. hitboxOffset/hitboxSize/hitboxAngle/animationStateName/TotalTime 추가. 기존 시간·피해·이동 배율·콤보·효과 슬롯 유지 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackHitbox.cs | Sample: 캐릭터 위치 기준 방향 반영 박스 질의. BeginAttack/EndAttack의 타격별 중복 방지와 취소 처리 유지. previewData 및 선택 Gizmo로 Inspector 범위 확인 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackEffects.cs | weapon 참조 제거, effectOrigin 사용. 준비/타격 효과는 캐릭터 기준점에, 명중 효과는 실제 접점에 생성. 로컬 위치/크기를 공격 방향에 맞춰 반전 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerCombatTestSceneBuilder.cs | Create/Configure/DeleteOwned: 기존6개 공격 데이터 GUID와 타이밍 보존, 무기 제거·새 기준점·선택적 Animator 연결·별도 테스트 씬 생성, 명시한 구 무기 에셋 정리 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab | 중첩 Greatsword 제거. AttackEffectOrigin과 PlayerAttackAnimator 추가. Hitbox.previewData/Effects.effectOrigin 연결. 검 잠금 false(weaponUnlocked=false), 이동 설정 및 기존 컴포넌트 GUID 유지 |

수정 데이터6개:
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GroundAttack.asset
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GroundAttack2.asset
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GroundAttack3.asset
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/AirAttack.asset
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/AirAttack2.asset
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/AirAttack3.asset

각 데이터는 해당 타격의 판정 위치·크기·각도와 캐릭터 Animator 상태 이름을 저장한다. 시간·피해·콤보·기존 커스텀 효과 설정을 보존한다.

추가:
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackAnimator.cs 및 .meta: 선택적 캐릭터 Animator 연결. AttackProgress로 지정 상태를 샘플링한다. 판정이나 이동을 실행하지 않는다.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scenes/Tests/PlayerTimedComboTest.unity 및 .meta: 저장된 이전 콤보 씬을 복사한 새 시간 기반 확인 씬. 실제 원화가 없으므로 사각형 몸체만 표시된다.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-timed-character-attacks.md: 이 보고와 검증 기록.

삭제:
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerWeaponMotion.cs 및 .meta
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Greatsword.prefab 및 .meta
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions의 Carry.anim, Hit1_Lift.anim, Hit1_Down.anim, Hit1_Recover.anim, Hit2_Brace.anim, Hit2_Up.anim, Hit2_Recover.anim, Hit3_DrawBack.anim, Hit3_Across.anim, Hit3_Recover.anim과 각각의 .meta. 알려진 이전 생성물만 개별 삭제한 후 폴더가 비었을 때 폴더·.meta 정리.

문서 수정: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md의 최신 전투 결정/현재 리뷰, C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md의 최신 시간 기반 안내, C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Design/2026-11-02-vfx-direction.md의 기준점 변경, C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Plans/ETHEREAN_Daily_Plan_2026-11-08.md의11-03 실행 기록. 과거 보고서와 원본 GDD/마스터 스냅샷은 이력으로 보존한다. 파일 이름 변경이나 전투/이동 폴더 이동은 이번에 하지 않는다.

## 3. 실제 동작 흐름

1. NewInputAttackReader가 새 공격 입력을 기억한다. Locomotion의 이동 프레임 이벤트를 받은 CombatCoordinator가 Attack.Tick을 호출한다.
2. 유효한 시작이면 지상/공중 배열과 방향을 선택한다. 입력 시각에 Windup을 시작하고 WindupTime 전에는 판정하지 않는다.
3. 준비 마감 시 Active가 되고 ActiveTime 동안에만 Hitbox.Sample을 실행한다. 범위는 캐릭터 위치+(방향×오프셋X,오프셋Y), 각도는 방향×설정각, 크기는 설정값이다. 무기/Animator 유무·애니메이션 이벤트와 무관하다.
4. 수신자 단위로 한 타격 한 번만 피해를 준다. 복합 콜라이더도 한 번이며 다음 타격에서는 다시 가능하다. Active 중 뒤늦게 범위로 들어온 대상도 맞을 수 있다. Active가 끝난 뒤 늦은 추가 판정은 없다.
5. 회복으로 넘어가면서 기존 예약 입력/연결25%와 점프 캔슬65% 규칙을 따른다. 예약된 다음 타격은 새 준비 시각으로 시작한다. 정상 타격의 단계 마감은 이전 시각+시간으로 이어서 물리 틱 오차가 단계마다 누적되지 않게 한다.
6. PhaseChanged/HitLanded는 커스텀 효과 프리팹을 생성·정리한다. 캐릭터 기준점이 무기 소켓을 대체한다. None 슬롯은 아무 효과도 만들지 않는다.
7. 선택적 PlayerAttackAnimator는 캐릭터 Animator가 연결되어 있고 해당 상태가 존재할 때만 전체 공격 진행률로 상태를 재생한다. 예: Base Layer.Attack1. 판정 시간을 애니메이션이 결정하지 않는다. 취소 시 지정한 Return State Name이 있으면 복귀하며 Animator가 없거나 꺼져도 전투는 계속 작동한다.
8. 입력/이동/공격/조정자 비활성화·검 잠금·유효한 후반 점프는 체인과 판정을 취소한다. 속도 쓰기는 기존 PlayerMovement 한 곳이며 전투/이동 폴더 분리는 유지한다.

## 4. 설계 이유와 트레이드오프

게임 판정을 전투 시간 데이터에 두고 외형과 분리했다. 캐릭터에 검을 그려 넣은 스프라이트 애니메이션을 교체해도 전투 타이밍이 바뀌지 않는다. 반대로 시각 리소스의 타격 장면을 데이터의 Active 구간에 맞추는 작업은 필요하다. Animation Event로 피해를 별도로 주면 중복 경로가 생기므로 사용하지 않는다.

판정은 현재 Active 박스다. 실제 검날을 추적하는 정밀 경로가 아니며 타격별 모양은 Inspector에서 조절한다. 빠르게 완성하고 사용자 애니메이션과 맞추기 쉬운 대신 정확한 칼날 윤곽이나 벽 차단을 제공하지 않는다. 기하학 디버그 Gizmo는 선택한 오브젝트의 범위 확인용이며 게임 효과를 그리는 코드가 아니다.

Animator 브리지는 선택한 공격 레이어의 시간을 전투에 맞춘다. 사용자 컨트롤러의 공격 상태를 직접 구동하므로 같은 레이어에 별도 공격 시간 제어를 중복하지 않는다. 캐릭터 Visual의 Sprite/자세만 애니메이션하고 Rigidbody2D 루트 위치를 공격 클립에서 움직이지 않는다. 전체 이동 Animator는 아직 별도 구현하지 않았다.

## 5. 검증 방법과 실제 결과

Unity6000.4.7f1·기존 패키지에서 컴파일/Editor 연결 검사 및 실제 Play Mode 입력·물리 검사를 실행한다. 준비 전 무피해, 설정 지연 후 피해, Active 종료 후 무피해, 범위 진입/이탈, 중복 방지,3타/캔슬/좌우/공중/벽 기능을 확인한다.

정확한 시간 경계는 별도로 통제 시각을 주입해 확인하며 실제 물리 검사의 개수와 구분한다. Animator 연결은 임시 컨트롤러와 보이지 않는 Transform 곡선으로 엔진 재생을 검증하고, 실제 캐릭터 공격 원화를 검증했다고 표현하지 않는다. 테스트 VFX도 렌더 없는 마커 프리팹이다. 공격 데이터는 Play 중 복제해 원본 슬롯을 보호한다.

최종 실행·정리·해시 비교는 아래 최종 검증 구역을 따른다. 기존66개 검사는 이전 검날 경로 방식의 역사 기록이며 이번 방식의 새 검증으로 재사용하지 않는다.

## 6. 직접 확인과 연결

- PlayerTimedComboTest.unity를 열어 Play. A/D 이동, Space 점프, Enter/마우스 왼쪽 공격. 이 씬은 검 해제 상태지만 기본 프리팹은 잠겨 있다.
- 처음 누른 후0.8초 준비 동안 표적 HP가 변하지 않고 Active에서10 감소하는지 본다. 이어서 타격/회복 중 새 입력으로2·3타를 연결한다. 길게 누른 자동 연타는 없다.
- 각 GroundAttack/AirAttack 및2·3 데이터에서 Windup Time(판정까지 대기), Active Time(유효 판정 길이), Recovery Time(후딜), Hitbox Offset/Size/Angle을 조절한다.
- Scene 뷰에서 Player를 선택하고 Gizmos를 켜 범위를 본다. PlayerAttackHitbox.Preview Data를 바꾸면 대기 중에도 선택한 데이터의 범위를 확인할 수 있다. 게임 화면의 이펙트가 아니다.
- 실제 캐릭터의 Visual에 Animator/컨트롤러/공격 스프라이트 클립을 준비한 뒤 PlayerAttackAnimator.Character Animator에 연결한다. 데이터의 Animation State Name을 실제 상태 전체 경로에 맞춘다. 필요하면 Return State Name도 설정한다.
- 공격 클립은 몸체와 검을 함께 포함한다. AttackProgress 기준 Active 비율 구간에 타격 모션을 배치한다. 1타 현재 설정에서는 약60.6~71.2% 구간이다(0.8/1.32~0.94/1.32). 연결하지 않으면 공격 판정은 작동하지만 공격 그림은 나타나지 않는다.
- VFX 프리팹은 기존 데이터 슬롯을 사용하며 캐릭터의 AttackEffectOrigin을 기준으로 배치한다. 실제 명중 효과는 접점에 생성된다.

## 7. 임시값·미완료·제한

기존 시간/피해는 유지한다:1타0.8/0.14/0.38초·피해10,2타0.12/0.18/0.34초·피해12,3타0.32/0.2/0.5초·피해16. 연결25%/점프 캔슬65%, 이동 배율도 그대로다.

초기 범위(오프셋/크기):1타(1,0.6)/(1.65,2.5),2타(0.95,0.8)/(1.8,2.4),3타(0,0.1)/(4.2,1.2), 각도0도. 실제 공격 그림이 준비되면 재조정할 임시값이다. 콜라이더/이동 수치·임시40px·2등신·대검1.2배 디자인 결정은 바꾸지 않았다.

미완료: 실제 캐릭터·대검 공격 스프라이트, 최종 AnimatorController/상태 연결, 실제 VFX, 체력/피격/AI/진행도. 애니메이션 리소스가 없어 사각형 몸체만 보이는 점을 숨기지 않는다. 벽 너머 타격 차단·공격 루트 모션·실제 빌드·게임패드/물리 장치 조작감·성능은 미검증이다.

## 8. 사용자 리뷰

이제 판정 시간·범위와 실제 캐릭터 애니메이션을 따로 조정할 수 있다. 실제 클립을 넣으면 Active 구간과 그림을 맞추는 검토가 필요하다. 현재 요청 밖의 외형을 임의 제작하지 않는다.

## 9. 다음 작업과 중단 지점

시간 기반 판정 전환과 기존 검 오브젝트 제거를 마무리해 사용자 리뷰를 받는다. 캐릭터 리소스를 제공하면 이 구조에 연결할 수 있으며, 다른 기능 묶음·커밋·푸시는 자동 진행하지 않는다.

## 최종 검증 기록

최종 새 실행55개 통과: Editor15개, 실제 입력/물리/엔진 Play37개, 통제 시각 경계3개다. 통제 시각3개를 실제 물리 검사로 표현하지 않았다. 전용 검/Animator가 없어도 시간 기반 판정이 동작하고 Active 전후의 무피해·복합 판정 중복 방지·3타·취소·좌우·공중·벽 기능·커스텀 프리팹·선택적 Animator 엔진 연결을 확인했다. 중간 실행의 실제 사용자 입력 간섭을 막기 위해 테스트 복제 액션을 전용 가상 키보드에만 바인딩하고 테스트 패널을 껐다. 실패한 중간 실행은 통과 수에 포함하지 않는다. 최종 로그는 아래와 같다. 임시 C# 검사2개와 .meta, ready/request, 마커 프리팹·테스트 AnimatorController(곡선 서브에셋 포함)·임시 씬은 제거했다. 정리 후 최종24개 C# 소스 별도 컴파일도 오류0으로 통과했다. 실제 캐릭터 원화·게임패드/물리 장치 조작감·배포 빌드·성능은 미검증이며 미연결 캐릭터 애니메이션을 완료로 취급하지 않는다. 원래 열린 PlayerCombatTest 씬의 clean 상태로 복귀했고 기존 사용자 스테이징 상태는 유지했다.

```text
Timed attack validation / 2026-11-03T00:27:45.8871758+09:00
EDITOR PASS Prefab remains weapon locked
EDITOR PASS Combat coordinator remains connected
EDITOR PASS Player collider unchanged
EDITOR PASS No procedural weapon renderer
EDITOR PASS Player has no independent sword child
EDITOR PASS Character effect origin connected
EDITOR PASS No missing player scripts
EDITOR PASS Independent sword prefab deleted
EDITOR PASS Obsolete sword clips removed
EDITOR PASS Weapon motion source removed
EDITOR PASS Optional character Animator bridge connected
EDITOR PASS No fabricated character animation assigned
EDITOR PASS Both three hit sequences preserved
EDITOR PASS Existing timing tuning preserved
EDITOR PASS Timed bounds and empty custom VFX slots serialized
PLAY PASS Starts grounded
PLAY PASS No weapon or character Animator needed for attacks
PLAY PASS Weapon lock still rejects input
PLAY PASS Input starts first preparation
PLAY PASS No damage before configured delay
PLAY PASS Active begins timed damage without weapon transform
PLAY PASS First hit delay matches data within physics tick tolerance
PLAY PASS Active duration follows data
PLAY PASS Compound target hit only once in active interval
PLAY PASS Holding attack does not auto combo
MEASURED first hit delay=0.820s, configured=0.800s
PLAY PASS Active press queues second hit
PLAY PASS Second press queues third hit
PLAY PASS Third hit cannot queue fourth
PLAY PASS Timed three hit damage 10 12 16 preserved
PLAY PASS Preparation jump remains blocked
PLAY PASS Late recovery valid jump cancels attack
PLAY PASS Air sequence selected
PLAY PASS Air gravity retained
PLAY PASS Landing does not switch active attack data
PLAY PASS Input disable clears timed attack and queue
PLAY PASS Movement disable cancels timed attack
PLAY PASS Outside configured range is not hit
PLAY PASS Entering configured range during active is hit
CONTROLLED PASS No hit immediately before windup deadline
CONTROLLED PASS Hit immediately after windup deadline
CONTROLLED PASS No late endpoint hit when active interval already ended
PLAY PASS Timed offset and knockback mirror left
PLAY PASS Wall slide retained
PLAY PASS Wall slide rejects attack
PLAY PASS Wall jump retained
PLAY PASS Timed attack preserves outward wall lock
PLAY PASS Custom preparation prefab uses character origin
PLAY PASS Custom active prefab replaces preparation effect
PLAY PASS Custom hit prefab created at valid timed contact
PLAY PASS Cancel clears attached character effect
PLAY PASS Effect disable cleans impacts
PLAY PASS Optional bridge selects character Attack1 state
PLAY PASS Character animation pose follows timed attack progress
PLAY PASS Cancel returns configured character state
PLAY PASS Disabling animation does not disable combat timing
SUCCESS PLAY 37 CONTROLLED 3
RESTORED Assets/_Project/Scenes/Tests/PlayerCombatTest.unity|dirty=False
```

### 삭제한 이전 무기 클립 전체 경로

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Carry.anim 및 .meta
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit1_Lift.anim 및 .meta
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit1_Down.anim 및 .meta
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit1_Recover.anim 및 .meta
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit2_Brace.anim 및 .meta
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit2_Up.anim 및 .meta
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit2_Recover.anim 및 .meta
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit3_DrawBack.anim 및 .meta
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit3_Across.anim 및 .meta
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit3_Recover.anim 및 .meta


최종 보존 확인: 기존 Assets/Packages/ProjectSettings 스냅샷 대비 의도한12개 코드/데이터/프리팹/도구 파일만 수정했고 구 무기 관련25개 파일(메타 포함)을 제거했다. 기존 씬·이동 코드·Input Actions·패키지·프로젝트 설정·남겨 둔 .meta의 변경은0개다. 프리팹 기존 이동 설정14개도 동일하다. 최종24개 소스 컴파일 경고18개는 모두 Inspector 필드의CS0649이며 오류는0개다. 첫 피해 실제 측정0.820초는 설정0.800초에 물리 틱0.020초 지연이 포함된 값이다. 데이터는 고정 물리 틱에서 처리되므로 하위 프레임 정확도를 주장하지 않는다.
