# 플레이어 1타 몸 애니메이션 연결

`C:/Users/pjw/OneDrive/Desktop/ETHEREAN_SPRITE/공격자세/공격 자세_1.png`, `_2.png`, `_3.png`를 순서대로 하나의 **1타 몸 애니메이션**에 연결했다. 세 이미지를 서로 다른 콤보 타격으로 사용하지 않는다. 원본32×32 픽셀을 그대로 복사했고 추가 중간 그림이나 검/VFX를 그리지 않았다.

## 파일별 변경

다음 경로의 루트는 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN`이다. 새 Unity 자산 및 스크립트에는 각각 meta가 함께 생성됐다.

| 파일 | 구분·수정 위치 및 책임 |
|---|---|
| `Assets/_Project/Art/Sprites/Player/Attack1/image_1.png` ~ `image_3.png` | 추가. 사용자 원본3장을 바이트 그대로 복사한 1타 몸 자세 |
| 위 PNG의 `.meta`, `Attack1.meta` | 추가. Point 필터/32PPU/무압축/mipmap 없음/Single/FullRect. 기존 캐릭터와 같은 고정 pivot=(16,1)px |
| `Assets/_Project/Art/Animations/Player/Attack1.anim` + meta | 추가. Sprite만 교체하는 비반복 클립. 위치/회전/스케일/검/피해 이벤트 곡선 없음 |
| `Assets/_Project/Art/Animations/Player/PlayerCharacter.controller` | 수정. Base Layer.Attack1 상태를 추가하고 새 클립 연결. Idle 기본 상태와 기존 Idle/Walk/Run 연결 유지 |
| `Assets/_Project/Scripts/Editor/PlayerAttackAnimationBuilder.cs` + meta | 추가. Build 메뉴에서 세 이미지 import와 1타 클립/상태 연결. 기존 GroundAttack의 준비/판정/후딜 시간을 읽음 |
| `Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackAnimator.cs` | 수정. LateUpdate에서 공격 상태를 직접 샘플링할 때 자동 재생 속도0으로 두고, ReleaseAnimation에서 이전 속도 복원. 클립이 없거나 Animator가 유효하지 않으면 구동 소유권 해제 |
| `AGENTS.md`, `PLAYER_SETUP.md` | 최신 1타 몸 클립 연결·검증·미제공2/3타 복귀 정책 기록 |
| 이 보고서, `2026-11-04-player-attack1-animation-validation.txt`, `2026-11-04-player-attack1-pose1.png` ~ `pose3.png` | 추가. 파일별 변경·검증 로그·실제 입력 중 카메라 캡처 |

임시 `Scripts/Editor/Attack1BodyValidation.cs`, `Scripts/Runtime/Player/Attack1BodyValidationDriver.cs`와 각 meta, Logs의 request/ready 및 복사 씬은 검사 후 제거했다. 새 영구 Runtime 클래스는 추가하지 않았다.

## 재생 순서와 시간

| 시간 | 표시 이미지 | 의도 |
|---|---|---|
| 0 ~ 0.60초 | 1번 | 느린 공격 준비 구간 |
| 0.60 ~ 0.87초 | 2번 | 준비 후반부터 내려치기 전반 |
| 0.87 ~ 1.32초 | 3번 | 내려치기 후반과 후딜 유지 |

0.60은 Windup0.8초의75%, 0.87은 Windup0.8+Active0.14의 절반이다. 클립 끝1.32초까지3번 자세를 유지한다. 비반복 클립이며, 타격 판정은 그대로 Windup0.8/Active0.14/Recovery0.38초의 기존 PlayerAttack이 결정한다. 클립 frameRate=60은 시간 편집 기준이고 실제 그림은 제공된3장뿐이다.

지상/공중1타 데이터는 현재 같은 시간 비율과 `Base Layer.Attack1` 상태 이름을 사용하므로 같은 몸 클립을 공유한다. 앞으로 두 데이터의 단계별 시간 비율을 다르게 조정한다면 몸 클립의 자세 교체 시점도 다시 검토해야 한다.

## 동작 원리와 설계 이유

1. 기존 입력과 PlayerAttack이1타를 시작한다. 입력/콤보/피해/이동 제약은 그대로다.
2. PlayerAttackAnimator가 해당 데이터의 AnimationStateName을 읽어 Attack1을 선택하고, 기존 AttackProgress로 정규화 재생 위치를 직접 지정한다. Animator의 별도 자동 시계가 진행하지 않도록 speed=0으로 둔다.
3. PlayerCharacterAnimator는 공격 표시가 소유 중이면 Idle/Walk/Run으로 덮어쓰지 않고 기존 공격 방향에 맞춰 flipX를 적용한다.
4. PlayerSwordMotion은 기존 방식으로 검 모션을 따로 샘플링한다. 이번에는 검 크기/소켓/모션/공격 판정 범위를 바꾸지 않았다.
5. 공격 종료·비활성화·클립 부재 시 ReleaseAnimation이 몸 표시 소유권을 해제하고 이전 Animator 속도를 돌려준다. 기존 일반 이동 표시가 Idle/Walk/Run을 다시 선택한다.

기존 코드는 다음 콤보에 대응하는 몸 상태가 없어도 단순 return하여 `_driving=true`가 남을 수 있었다. 이제 1타만 몸 클립이 제공된 상태에서2/3타로 넘어가면 Idle 표시로 복귀한다. 존재하지 않는2/3타 몸 클립을 임의로 만들거나1타를 재사용하지 않았다. 검의 기존2/3타 움직임과 피해 처리는 유지된다.

Editor 도구는 자산 생성/연결, Runtime 어댑터는 전투 진행률에 따른 몸 표시만 담당한다. 새로운 전투 상태 머신이나 Rigidbody 쓰기/피해 이벤트는 도입하지 않았다.

## 위치와 픽셀 기준

세 이미지 모두32×32, 32PPU, 기존 Idle과 같은 고정 발 피벗(16,1)px, 단위 배율을 사용한다. 프레임마다 몸을 자동 확대/중앙 정렬하지 않았다. 원화2·3번의 불투명 발 끝은1번보다1px 높으며, 이 원래 자세 차이는 보존한다. root/CharacterVisual Transform은 애니메이션으로 이동하지 않는다. 검과 손의 최종 연출 적합성은 사용자가 캡처와 실제 플레이로 검토할 항목이다.

## 검증

실제 Unity6000.4.7f1에서 **76개(15 Editor + 61 Play Mode)** 통과했다.

- Editor: 세 이미지의 Point32/무압축/mipmap 없음/원본 크기/피벗, Sprite만 변경하는 클립, 0/.6/.87/1.32초 키와 이미지 순서, 비반복, Attack1 연결 및 Idle 기본 상태, 기존 어댑터 참조 확인.
- 실제 가상 키보드 입력: 지상 오른쪽·공중 왼쪽1타에서1/2/3 자세와 Attack1 상태, 직접 제어 속도0 및 몸 flipX를 각각 확인했다. 2·3타에서는 소유권이 해제되고 Idle로 복귀함을 확인했다. 콤보 종료 후 몸 Idle 복귀도 통과했다.
- 기존 전투 회귀: 3연타 완료, Active당1회 피해, 후딜 연결 간격 약.22/.52초, 고정 검 손잡이, 일시정지, 검의 피격/점프 취소 및 비활성화/재활성화/사망 시 정리, 무관한 Tween 보존 통과. 이 취소 항목은 기존 검/전투 검사이며 새로운 몸 피격 아트를 검사한 것은 아니다.
- 실제 지상 공격 중 세 자세의 카메라 캡처를 저장하고 열어 확인했다. 최종 손-검 접점의 아트 품질을 자동 판정한 검사는 아니다.
- 원래 열린/활성/dirty 씬 상태 비교 `RESTORED`. 임시 검사 소스/meta/요청/ready/복사 씬 제거 완료.
- 정리 후 영구 Runtime33개 / Editor11개 소스를 별도로 컴파일하여 각각 exit0. Inspector 직렬화 필드에 대한 CS0649 경고는 있으나 오류는 없다.
- 작업 직전 스냅샷 대비 기존 파일 변경은 PlayerCharacter.controller와 PlayerAttackAnimator.cs 두 개뿐이다. 새 자산/도구는 별도 추가다. 검/공격 데이터/플레이어 프리팹/오소리/기존 몸 이미지·클립/저장 씬/Packages/ProjectSettings 및 기존 GUID를 유지했다.

## 직접 확인과 편집

기존 PlayerBadgerCombatTest에서 무기 해금 상태로 공격을 한 번 누른다. 몸 자세1→2→3과 기존 검 모션을 함께 확인하고, 왼쪽/공중에서도 확인한다. 다음 타를 예약하면 아직 제공되지 않은2·3타 몸은 기본 자세로 복귀해야 한다.

클립은 Animation 창에서 Attack1.anim을 열어 Sprite 키 시점을 편집할 수 있다. `ETHEREAN > Player > Connect First Attack Animation`은 import와 기본 시간 배치를 다시 적용하므로 수동으로 키를 튜닝한 뒤 재실행하면 시간 배치가 덮어써진다. 이 메뉴는 원본 Desktop 폴더를 상시 감시하지 않는다. 사용자가 새 파일을 만들었다고 하면 AGENTS.md에 기록된 원본 폴더에서 다시 가져온다.

이번 요청의1타 몸 애니메이션 연결에서 멈추고 사용자 검토를 기다린다. 다음 기능/커밋/푸시는 진행하지 않았다.
