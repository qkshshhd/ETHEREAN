# 대검 3타 콤보와 전투/이동 분리 — 2026-11-02

## 1. 목표와 승인 범위

기존 단발 확인용 구현을 실제3타 콤보로 교체한다. 사용자가 제시한 순서는 낮게 끄는 검 → 매우 느린 들어 올리기와 빠른 내려치기(1타) → 힘 있게 빠른 올려 베기(2타) → 왼쪽 뒤에서 오른쪽으로 가로 베기(3타)다. 기본 오른쪽 검, 반대 공격 방향은 좌우 반전한다. 최신 사용자 결정이 원본 기획의 기본 단발 사양보다 우선한다. 원본 기획서/마스터 프롬프트는 수정하지 않았다.

코드로 그리던 검선·궤적·명중 섬광·표적 색상 효과를 제거하고 사각형 검 프리팹과 커스텀 VFX 프리팹 슬롯으로 바꾼다. 전투와 이동 코드를 물리 폴더와 의존 관계 양쪽에서 분리한다. 실제 캐릭터 원화·적AI·피격 반응·저장·커밋/푸시는 이번 범위가 아니다. 완료 판정은 마지막 검증 구역을 따른다.

## 2. 수정·이동·추가·삭제 파일

아래 경로의 ROOT는 C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN이며 표를 저장할 때 전체 경로로 확장한다.

### 코드 변경

| 전체 경로 | 변경 위치와 역할 |
| --- | --- |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttack.cs | groundCombo/airCombo, Tick/StartStep/EnterPhase/AdvancePhase/CancelAttack:3타 선택·다음 입력 예약·단계 진행·취소. PhaseChanged 발행 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackData.cs | 콤보 연결 시점·검날 반경·경로 샘플 밀도·준비/타격/회복 AnimationClip·커스텀 효과 프리팹과 명중 효과 수명 추가. 고정 사각 판정 필드 제거 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackHitbox.cs | Sample: 검날 양 끝을 따라 캡슐 질의, 물리 틱 사이 모션도 나누어 검사. 타격마다 중복 수신 방지·콜백 취소 시 검사 중단 유지 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerWeaponMotion.cs | 추가. 프리팹의 Facing/Motion과 검날 기준점·끝점·EffectSocket, 모션 클립 샘플링·단계 간 블렌딩·끌기 자세 복귀. 모양/색/궤적을 생성하지 않음 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackEffects.cs | 추가. PhaseChanged/HitLanded 구독으로 사용자가 지정한 프리팹만 생성. 단계 효과 교체·취소 제거, 접점 효과 수명/비활성화 정리 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerCombatCoordinator.cs | 추가. 이동 프레임 알림을 받아 Attack.Tick 호출, 이동 배율/점프 허용 반환, 점프·제어 취소 시 전투 정리. 전투와 이동의 유일한 연결 담당 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerLocomotion.cs | MovementFrame/PreparingMovement/JumpStarted/ControlCancelled/SetActionConstraints 추가. PlayerAttack/NewInputAttackReader 직접 참조 제거. FixedUpdate와 센서·점프 우선순위 유지 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/CombatTestTarget.cs | 코드로 색을 바꾸던 피격 섬광·사망 색상 제거. HP/적중 횟수/마지막 전달 데이터만 처리 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/CombatTestPanel.cs | 콤보 번호·예약 여부 표시. 기존 검 잠금/표적 초기화 유지 |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerCombatTestSceneBuilder.cs | Create/FinishSourceSeparation/Motion/ConfigureAttack/MoveSources: Editor API로 모션·프리팹·데이터 이전·별도 콤보 씬·GUID 보존 폴더 이동. 기존 생성물 덮어쓰기 거부 |

### 원래 위치에서의 이동

기존 C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/ 바로 아래에 있던 다음 파일과 각각의 .meta를 Unity AssetDatabase.MoveAsset으로 이동했다. 클래스 이름과 Etherean.Player 네임스페이스는 유지했다. 따라서 경로 분리로 Inspector 스크립트 참조를 끊지 않는다. 기존 수정 여부는 위 표와 구분한다.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/NewInputMovementReader.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/NewInputMovementReader.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerMovementInput.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerMovementInput.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerMovement.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerMovement.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerJump.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerJump.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerWallJump.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerWallJump.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerWallSlide.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerWallSlide.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerGroundSensor.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerGroundSensor.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerWallSensor.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerWallSensor.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerTraversalAbilities.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerTraversalAbilities.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerLocomotion.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerLocomotion.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/NewInputAttackReader.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/NewInputAttackReader.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttack.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerAttack.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackData.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerAttackData.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackHitbox.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerAttackHitbox.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/AttackHitReceiver.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/AttackHitReceiver.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/CombatTestTarget.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/CombatTestTarget.cs
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/CombatTestPanel.cs ← C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/CombatTestPanel.cs

PlayerWeaponMotion/PlayerAttackEffects/PlayerCombatCoordinator는 이번에 새로 만든 뒤 Combat에 배치했다. PixelScalePreview는 화면 비교 도구이므로 Player 루트에 남긴다. Editor 도구는 기존 Editor 폴더에 남긴다.

### 에셋과 문서

- 수정: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab — 기존 PlayerAttack/Hitbox 유지, 이전 PlayerAttackVisual·세 LineRenderer 자식 제거, 중첩 Greatsword 프리팹·PlayerCombatCoordinator·PlayerAttackEffects와3타 데이터 배열 연결. 검/벽 능력 기본 잠금 유지.
- 수정: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GroundAttack.asset 및 C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/AirAttack.asset — 기존 GUID를 보존한1타 데이터. 새 모션 참조·시간·배율·커스텀 효과 슬롯.
- 추가: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GroundAttack2.asset, C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GroundAttack3.asset, C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/AirAttack2.asset, C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/AirAttack3.asset — 지상/공중 각각2·3타 데이터.
- 추가: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Greatsword.prefab — 흰색 사각 SpriteRenderer 검과 길이/모션 기준점. 실사용 무기 프리팹이며 구체 외형을 확정한 원화가 아님. 현재1.8u 몸체 대비 전체 길이2.16u=1.2배, 너비0.24u.
- 추가: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scenes/Tests/PlayerGreatswordComboTest.unity — 기존 저장된 전투 씬을 복사한 별도3타 확인 씬. 저장된 기존 씬은 덮어쓰지 않았다.
- 추가: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Carry.anim, Hit1_Lift.anim, Hit1_Down.anim, Hit1_Recover.anim, Hit2_Brace.anim, Hit2_Up.anim, Hit2_Recover.anim, Hit3_DrawBack.anim, Hit3_Across.anim, Hit3_Recover.anim — 끌기1개+3단계×3타의 수정 가능한 클립. 각 클립은 정규화1초이며 실제 재생 시간은 공격 데이터가 결정한다. 경로 바인딩은 Greatsword의 Facing/Motion 및 그 아래 Blade의 정렬 순서다.
- 삭제: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/PlayerAttackVisual.cs와 .meta — LineRenderer/색상/곡선/명중 모양 생성 제거.
- 삭제: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Materials/CombatPreview.mat와 .meta — 기존 선 표현 전용 재질 제거.
- 수정: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md 최신3타 안내, C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md 최신 전투 결정/리뷰 단위, C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Design/2026-11-02-vfx-direction.md 사용자 프리팹 정책, C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Plans/ETHEREAN_Daily_Plan_2026-11-08.md 최신 구현 기록.
- 추가: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-02-greatsword-combo.md — 이 보고. 같은 폴더의 2026-11-02-combo-carry.png, combo-hit1-lift.png, combo-hit1-down.png, combo-hit2-up.png, combo-hit3-across.png는 실제 카메라 캡처다.

새 Unity 에셋·코드·폴더에는 대응하는 .meta가 추가됐다. 이전 코드/에셋 .meta는 이동하거나 보존했고 명시한 폐기물만 삭제했다. 사용자 스테이징된 픽셀 씬은 보존했다.

## 3. 실제 실행 흐름과 콤보 규칙

1. NewInputAttackReader가 기존 New Input System Attack의 새 press를 한 번 소비한다. 버튼 유지 자동 연타는 없다.
2. PlayerLocomotion은 입력·센서를 읽고 MovementFrame을 발행한다. PlayerCombatCoordinator가 Attack.Tick을 호출하고 일반적인 이동 배율/점프 허용값만 돌려준다. 이동 코드에는 구체 전투 클래스 참조가 없다.
3. 처음 시작할 때 방향과 지상/공중3타 배열을 고른다. 방향은 체인 전체에서 고정하며 착지해도 공격 배열은 바꾸지 않는다. 벽 미끄러짐 중 시작 금지·중력 유지·검 잠금은 유지한다.
4. 1타는 오른쪽 바닥 가까이 둔 검을0.8초 동안 들어 올리고,0.14초의 가속하는 내려치기로 연결한다. 여기서 ‘중력 같은 가속’은 애니메이션 곡선으로 표현하며 실제 무기에 Rigidbody 중력을 추가한 것은 아니다. 손에 붙인 검의 동작과 타격 시간을 일치시키기 위한 선택이다.
5. 타격 또는 회복 중 새 press 하나를 예약한다. 준비 중 입력은 예약하지 않는다. 회복25% 이후 예약이 있으면 다음 타 준비로 연결한다. 연타해도 여러 타를 한 번에 예약하지 않으며3타 다음은 없다. 예약 없이 회복이 끝나면 끌기 자세로 복귀한다.
6. 2타는 짧은 힘주기 후0.18초에 빠르게 올려 벤다. 끝에서 검을 왼쪽 뒤로 둔다. 3타는 왼쪽에서 준비해 깊이 방향을 포함한 수평 회전으로 오른쪽까지 벤다. 반대 방향 공격은 Facing 축을 반전한다. 3타 중앙의 검이 카메라에 날을 세우는 순간에는 얇아 보이는 원근 표현이 있다.
7. PlayerWeaponMotion이 선택한 클립을 샘플링한다. 타격 중에는 검날 시작점과 끝점의 캡슐 범위를 모션 중간 시점까지 나눠 질의한다. 보이는 검과 판정이 같은 클립을 사용한다. 같은 타격의 복합 콜라이더는 수신자별 한 번만 적중하고 다음 타는 다시 적중할 수 있다.
8. 효과는 AttackData의 Windup/Swing/Hit Effect Prefab을 사용한다. 준비/타격 효과는 EffectSocket 아래 생성되어 검을 따라가고, 단계 종료/취소 시 제거된다. 명중 효과는 실제 접점에 생성되고 설정 수명 후 제거된다. None이면 아무 효과도 만들지 않는다. 효과 컴포넌트 비활성화 시 소유 인스턴스를 정리한다.
9. 준비/타격 중 점프 금지, 회복65% 이후 유효 점프 성공 시 체인/예약/판정을 취소한다. 예약이 다음 타로 이미 전환된 틱에는 새 준비 단계이므로 점프할 수 없다. 입력·이동·조정자·공격 비활성화, 검 재잠금도 체인을 취소한다. 속도 쓰기는 여전히 PlayerMovement 한 곳이다.

## 4. 설계 이유와 트레이드오프

콤보 진행, 모션, 타격, 효과, 이동 연결을 분리했다. 클립과 프리팹이 실제 교체 지점이며 런타임에서 검 모양·색·궤적·섬광을 그리지 않는다. 기본 모션을 최초 Editor API로 작성해 에셋에 저장했고 이후 Animation 창에서 수정할 수 있다. Animation 컴포넌트의 playAutomatically는 false이며 단계와 판정의 기준 시간은 전투 데이터가 소유한다. Animator 전환 시간과 별도 물리 시간이 어긋나는 문제를 피하기 위해 단계 진행률로 클립을 샘플링한다.

복귀·늦은 콤보 연결의 포즈 단절을 줄이기 위해 단계 초반 블렌딩과 끌기 자세 복귀를 제공한다. 코드는 정해진 각도를 매 프레임 만들지 않고 클립을 읽는다. 수평3타는2D 화면에서 칼의 길이가 원근에 따라 줄어드는 방식을 사용한다.

현재 효과 정책은 준비/타격 효과를 단계에 종속시켜 즉시 정리하는 것이다. 긴 잔광을 독립적으로 남기는 정책은 아직 추가하지 않았다. 검의 실제 지형 충돌·벽 너머 타격 차단·손 IK·몸체 공격 애니메이션은 이번에 추가하지 않았다. 실사용 전투 로직과 구체 아트 완성은 구분한다.

## 5. 검증 근거

Unity6000.4.7f1 및 기존 패키지를 사용했다. 새로운 테스트는 실제 Input System 입력, 자동 물리, 실제 AnimationClip 샘플링, 복합 콜라이더, 커스텀 프리팹 인스턴스를 사용한다. 효과 모양을 테스트용으로 그리지 않고 렌더 없는 마커 프리팹으로 연결/수명만 확인한다. 테스트용 공격 데이터는 Play 중 복제해 사용하므로 원본 슬롯에 테스트 프리팹을 남기지 않는다.

첫 이전에서 새 폴더를 AssetDatabase.StartAssetEditing 안에 만든 탓에 이동이 한 번 실패했다. 폴더를 먼저 등록하고 이전만 재개해 해결했다. 중간 테스트의 ‘유지 입력’이 실제로 release 뒤 새 press였던 부분과 프리팹 이름을 고정한 검색도 수정했다. 실패한 중간 실행은 최종 통과로 세지 않는다. 최종 검사 수·로그·GUID·기존 파일 보존·도구 정리는 아래에 기록한다.

## 6. 직접 확인과 커스텀 방법

1. Assets/_Project/Scenes/Tests/PlayerGreatswordComboTest.unity를 열어 Play한다. A/D 이동, Space 점프, Enter 또는 마우스 왼쪽 공격이다. 기존 이동/전투 씬도 그대로 존재하지만 최신 확인은 이 씬에서 한다.
2. 한 번만 눌러 매우 느린 들어 올리기와 빠른1타를 확인한다. 계속 누른다고2타가 나가지 않아야 한다.
3. 1타의 타격/회복 중 다시 눌러2타를 예약하고,2타의 타격/회복 중 다시 눌러3타를 예약한다. 패널의 Combo/Next queued를 확인한다. 표적은3타 적중 시50→40→28→12다. Reset targets로 초기화한다.
4. 좌우 반전·공중 시작 후 착지·후반 점프 캔슬·벽 미끄러짐 중 시작 금지·검 잠금을 확인한다.
5. Greatsword.prefab을 Prefab Mode로 연다. Facing/Motion/Blade의 SpriteRenderer를 자신의 검 외형으로 바꿀 수 있다. 검 크기를 변경하면 BladeBase/BladeTip도 실제 검날 끝과 맞춘다. 현재 검은 외형 미정의 사각형이다.
6. Animation 창에서 Greatsword의 Carry 및 Hit1/2/3 모션을 수정한다. 모션의 정규화 시간은0~1이며 실제 시간은 GroundAttack/AirAttack 및2·3 데이터의 Windup/Active/Recovery Time에서 조절한다.
7. 각 공격 데이터의 Windup Effect Prefab·Swing Effect Prefab·Hit Effect Prefab에 자신의 프리팹을 연결한다. 단계 효과는 EffectSocket 기준 로컬 배치, 명중은 월드 접점 생성이다. 기본 슬롯은 전부 None이므로 지금 궤적/명중 이펙트가 안 보이는 것이 의도다. Hit Effect Lifetime은 프리팹의 실제 재생 길이에 맞춘다.

## 7. 임시값과 한계

| 타격 | 준비/타격/회복 | 준비 이동 배율 | 피해 |
| --- | --- | --- | --- |
| 1 | 0.80/0.14/0.38초 | 0.15 | 10 |
| 2 | 0.12/0.18/0.34초 | 0.20 | 12 |
| 3 | 0.32/0.20/0.50초 | 0.10 | 16 |

타격 이동 배율0.05, 회복 중0.05→1, 연결 시작25%, 점프 캔슬65%, 검날 반경0.12u, 자세 복귀0.22초, 단계 초반 블렌딩12%는 전부 Inspector 튜닝값이다. 실제 스탯 시스템이 아니다. 현재 몸체1.8u에 검 길이2.16u를 맞췄으며 최종 캐릭터40px를 확정한 것은 아니다. 실제 아트 크기가 바뀌면1.2배 관계와 기준점을 다시 조정한다. 프리팹/데이터 GUID를 보존했지만 배포 저장 스키마나 실제 진행 이벤트는 아직 없다.

## 8. 사용자 리뷰

핵심 검토는1타를 들어 올리는0.8초가 의도한 힘겨움인지,2타의 상승 속도와3타 수평 베기가 서로 구분되는지, 연결 입력 창이 자연스러운지다. 실제 VFX 프리팹 제작/연결은 사용자가 담당하며 그 결과를 완성했다고 주장하지 않는다.

## 9. 다음 작업과 중단 지점

이번3타와 코드 분리 결과를 사용자에게 보여주고 멈춘다. 현재 기능의 수정 요청을 우선하며 체력/피격/적AI 등 다른 묶음으로 자동 진행하지 않는다. 커밋/푸시는 수행하지 않는다.

## 최종 검증 기록

최종 새 실행은 Editor15개+실제 Play51개=66개 전부 통과했다. 각3타 모션·입력 예약·복합 콜라이더·타격별 피해·점프 취소·지상/공중 선택·바닥/벽 코요테·점프 버퍼·사용자 효과 프리팹 생성/교체/수명/취소·콜백 중 판정 중단을 검사했다. 실제5개 모션 캡처를 시각 확인했다. 임시 C# 검사2개와 .meta, ready/request, 렌더 없는 마커 프리팹과 .meta 및 임시 복사 씬은 제거했다. 정리 후 최종24개 C# 소스의 별도 컴파일도 오류 없이 통과했다. Inspector 연결 필드의 CS0649 경고는 실제 연결 검사와 구분한다. 물리 장치 조작감·최종 아트·배포 빌드·성능은 미검증이다. 마지막 실행은 시작 당시 열린 PlayerCombatTest 씬의 clean 상태로 복귀했다. 기존 스테이징된 픽셀 씬 변경은 그대로 유지했다. 아래는 최종 통과 실행의 전체 로그다.

```text
Combo validation / 2026-11-02T23:18:30.7168753+09:00
EDITOR PASS Prefab weapon locked
EDITOR PASS Dedicated combat coordinator connected
EDITOR PASS Player collider unchanged
EDITOR PASS No LineRenderer in player or weapon
EDITOR PASS Player has no missing scripts
EDITOR PASS Independent weapon prefab
EDITOR PASS Weapon is single rectangle sprite
EDITOR PASS Carry plus nine editable motion clips
EDITOR PASS Old procedural visual source removed
EDITOR PASS Procedural preview material removed
EDITOR PASS Combat and movement folders separated
EDITOR PASS Movement has no concrete attack dependency
EDITOR PASS Ground and air have three steps
EDITOR PASS Slow first lift and fast downswing data
EDITOR PASS Custom effect slots start empty
PLAY PASS Grounded and weapon visible
PLAY PASS Weapon length 1.2 times current 1.8u body
PLAY PASS Initial blade dragged low on right
PLAY PASS Weapon lock hides and blocks attack
PLAY PASS Rejected press not retained
PLAY PASS First input starts combo step one
PLAY PASS Slow lift does not deal damage
PLAY PASS Lift rises with heavy movement restriction
PLAY PASS Windup jump blocked
PLAY PASS First lift is approximately 0.8 seconds
PLAY PASS First active drives blade down
PLAY PASS Holding input produces one hit, no automatic combo
PLAY PASS Target no longer generates flash effect
PLAY PASS Single attack returns to drag carry
PLAY PASS New active press queues second hit
PLAY PASS Step one compound hurtbox only hit once
PLAY PASS Second active lifts blade rapidly
PLAY PASS Second hit accepts third input
PLAY PASS Second hit leaves weapon behind on left
PLAY PASS Third hit sweeps left to right
PLAY PASS Third hit cannot queue a fourth
PLAY PASS Three steps deal 10 12 16 once each
PLAY PASS Combo queue clears at completion
PLAY PASS New completed-chain press restarts at one
PLAY PASS Late recovery permits jump cancel
PLAY PASS Jump cancels combo and moves player
PLAY PASS Jump cancellation clears queued combo
PLAY PASS Air combo selected at start
PLAY PASS Air combo retains gravity
PLAY PASS Landing preserves selected air sequence
PLAY PASS Attack input disable cancels entire combo
PLAY PASS Movement disable notifies combat coordinator
PLAY PASS Coordinator disable cancels attack
PLAY PASS Wall slide regression
PLAY PASS Wall sliding rejects combo
PLAY PASS Wall jump regression
PLAY PASS Attack preserves wall outward lock
PLAY PASS New left input mirrors whole chain
PLAY PASS Mirrored first slash points left
PLAY PASS Platform grounded for coyote regression
PLAY PASS Walked off platform for coyote regression
PLAY PASS Ground coyote retained after separation
PLAY PASS Separated wall for grace regression
PLAY PASS Wall coyote retained after separation
PLAY PASS Prelanding jump buffer retained after separation
PLAY PASS Custom windup prefab instantiated on socket
PLAY PASS Custom swing prefab replaces windup prefab
PLAY PASS Custom impact prefab spawned once for compound target
PLAY PASS Cancellation removes attached custom effect
PLAY PASS Effect component disable cleans owned impacts
PLAY PASS Hit callback cancellation stops remaining sweep targets
All motion captures are actual camera renders. No procedural trails or impact artwork.
SUCCESS PLAY 51
RESTORED Assets/_Project/Scenes/Tests/PlayerCombatTest.unity|dirty=False
```

### 모션 에셋 전체 경로

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Carry.anim
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit1_Down.anim
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit1_Lift.anim
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit1_Recover.anim
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit2_Brace.anim
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit2_Recover.anim
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit2_Up.anim
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit3_Across.anim
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit3_DrawBack.anim
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GreatswordMotions/Hit3_Recover.anim


최종 보존 확인: 이전 스냅샷 대비 기존 소스17개와 각 .meta의34개 파일을 경로 이전했다. 기존 메타 내용 변경은0개이며 저장된 기존 씬·Input Actions·Packages·ProjectSettings 변경도0개다. 코드/데이터/프리팹/도구의 의도한10개 기존 파일만 수정했고 명시한 폐기 코드·재질 및 메타4개만 제거했다. Player 프리팹의 기존 이동 설정14개도 동일했다. 최종 별도 컴파일의21개 경고는 모두 Inspector 연결 필드의CS0649이며 오류는0개다.
