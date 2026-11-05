# 멧돼지 Idle·Walk·Run·Death 연결

## 최신 요청과 적용 범위

사용자가 제공한 Idle/Walk/Run/Death 각16프레임을 상태에 맞춰 연결한다. 작업 도중 Death를 새로 만들었다는 지시에 따라 **`멧돼지_Death_new`를 최종 원본으로 사용**한다. 이전96×96 Death는 최종 적용 대상이 아니다.

원본 폴더는 `C:/Users/pjw/OneDrive/Desktop/ETHEREAN_SPRITE`이다. 최종4세트64장은 모두64×64이며, 원본 PNG를 그대로 복사했다. 이미지 리사이즈·패딩·재그림·프레임별 확대/축소를 하지 않는다. 캐릭터 Transform을 기존 사각형 크기에 억지로 맞추지도 않는다.

돌진 준비와 기절은 사용자가 이후 VFX로 표현하기로 했다. 이번에는 해당 VFX·전용 애니메이션·가짜 이펙트를 만들지 않고 기존 Idle 첫 자세를 유지한다. 기존 경고/준비/기절 진입 로그와 사용자 경고 프리팹 슬롯을 유지한다.

## 상태별 표시

| 게임 상태 | 연결한 클립 | 재생 방식 |
|---|---|---|
| Idle | Idle | 8fps 반복 |
| Roam/Return의 실제 이동 | Walk | 12fps 반복 |
| Alert/Recovery | Idle | 기존 Idle 재사용·반복 |
| Windup/Stun | Idle | 첫 자세 고정. 사용자 VFX 제작 대기 |
| Charge | Run | 16fps 반복. 돌진 최대 시간과 별개로 정상 속도 재생 |
| Stagger/KnockdownLaunch/KnockdownDown/GetUp | Idle | 미제공 피격 그림 대신 첫 자세 고정 |
| Dead | 새 Death | 12fps 단발, 마지막16번 프레임 유지 |

Controller의 논리 상태 이름은 기존 Charge/Windup/Stun 등을 유지한다. 따라서 Run은 Charge의 Motion이며 별도의 AI Run 상태를 추가하지 않는다. 미제공 상태마다 새로운 클립을 만들지 않고 동일 Idle 자산을 참조한다.

## 수정·추가 파일

아래 경로의 루트는 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN`이다. 새 Unity 자산/스크립트에는 meta가 함께 생성된다.

| 파일 | 구분·변경 위치와 책임 |
|---|---|
| `Assets/_Project/Art/Sprites/Boar/Idle`, `Walk`, `Run`, `Death`의 `image_1.png`~`image_16.png` 및 meta | 추가. 최종64장 제공 PNG와 동일한 픽셀/바이트. Death 폴더는 새 Death_new 내용으로 교체하되 작업 중 생성된 Sprite GUID를 유지 |
| `Assets/_Project/Art/Animations/Boar/Idle.anim`, `Walk.anim`, `Run.anim`, `Death.anim` 및 meta | 추가. Sprite만 바꾸는17키 클립. 이동·회전·스케일·공격 이벤트 곡선 없음 |
| `Assets/_Project/Art/Animations/Boar/Boar.controller` | 수정. 기존12개 논리 상태의 Motion을 제공된4클립으로 연결. 기본 Idle과 기존 상태 ID 유지 |
| `Assets/_Project/Prefabs/Enemies/Boar.prefab` | 수정. Square Renderer 끄기, CharacterVisual Renderer 켜기/Idle 첫 이미지, 오른쪽 원본 방향 보정false. 기존 물리/데이터/드롭 참조 유지 |
| `Assets/_Project/Scripts/Runtime/Enemies/BoarAnimator.cs` | 수정. Windup/Stun/피격은 중립 자세 고정, Charge는 일반 속도 Run 반복. 사망 우선과 방향 반전/미제공 상태의 Idle 대체 유지 |
| `Assets/_Project/Scripts/Editor/BoarAnimationBuilder.cs` 및 meta | 추가. import/Point32/고정 피벗/4클립 생성/Controller 매핑/기존 프리팹 표시 전환을 담당하는 메뉴 |
| `Docs/Design/2026-11-04-boar-animation-states.md` | 최신 결정 추가. 과거6종 우선 제작안보다 사용자4종+준비/기절 VFX 방침 우선 |
| `AGENTS.md`, `PLAYER_SETUP.md` | 최종 원본·매핑·원본 크기 보존·VFX 보류·검증/검토 상태 기록 |
| 이 보고서와 검증 로그/카메라 캡처 | 추가. 실행 근거와 직접 확인 절차 |

## 픽셀 크기와 기준점

- 원본64×64 / Point 필터 / 32PPU / 무압축 / mipmap 없음 / NPOT None / Single Sprite / FullRect.
- 각 클립의 모든 프레임에 동일한 custom pivot **(32,12)px**를 사용한다. 그림을 매 프레임 자동 중앙 정렬하지 않아 원화의 보행/피격 움직임을 보존한다.
- Idle/Walk/Run 발 높이는0~1px, 새 Death는0~2px의 원화 변화가 있다. 피벗은 발 아래 최소 여백을 기준으로 맞추고, 포즈 자체를 늘리거나 줄이지 않는다.
- CharacterVisual의 기존 발 기준 localY=-0.45와 scale1을 유지한다. 루트 BoxCollider2D의0.9×0.9 크기, 돌진 속도/범위 등 전투 수치를 변경하지 않는다.
- 원화의 실제 몸 폭은 기존 임시 콜라이더보다 넓다. 이번에는 원화를 사각형에 축소하지 않았으며, 아트와 타격/벽 충돌 범위의 최종 적합성은 사용자 플레이 리뷰에서 따로 판단한다.

## 동작과 설계 이유

BoarBrain/Motor의 실제 상태·속도, CombatHealth/CombatReaction을 기존 BoarAnimator가 읽는다. 사망은 항상 최우선이다. 일반 이동에서 Walk, 돌진에서 Run, 정지/경고/회복에서 Idle을 선택한다. 준비/기절/그림 없는 피격은 상태를 유지하되 Idle 첫 프레임만 표시한다.

기존 표시 코드는 Charge에서 전체 클립을 StateProgress에 맞춰 강제 샘플링했다. 이를 일반 속도 반복 재생으로 바꿔 돌진 시간을 늘려도 Run이 느리게 늘어나지 않게 했다. 공격 판정과 돌진 종료 시간은 여전히 BoarBrain/BoarAttack이 소유한다. 애니메이션에서 피해·속도·기절을 발생시키지 않는다.

프리팹의 SpriteRenderer.flipX는 원본이 오른쪽을 보는 기준으로 설정한다. 실제 좌/우 카메라 캡처로 방향을 검토한다. 이미지 자체는 반전해 저장하지 않는다.

## 제작 도구와 직접 확인

`ETHEREAN > Enemies > Connect Boar Animations`가 프로젝트에 복사된64장을 import하고 연결한다. 기존 클립/GUID와 상태 자리를 재사용하며, 재실행하면 기본 fps·키·Motion 매핑을 다시 적용한다. 이후 수동으로 키/fps/상태 Motion을 편집했다면 재실행 전에 변경 범위를 확인한다. 실제 원본 가져오기는 사용자 요청 시 ETHEREAN_SPRITE에서 수행하며 상시 폴더 감시는 아니다.

기존 PlayerBoarCombatTest 또는 PlayerHealingTest에서 다음을 확인한다.

1. 배회/복귀할 때 Walk, 정지할 때 Idle이 나오고 방향이 실제 이동과 맞는지 본다.
2. 접근해 돌진을 유도한다. 준비 중 Idle 첫 자세, 돌진 중 Run, 벽 기절 중 Idle 첫 자세가 나와야 한다. 준비/기절 VFX는 아직 없다.
3. 체력을0으로 만들면 새 Death가 한 번 재생되고 마지막 자세에 머물러야 한다.
4. 기존 무적이 아닌 슈퍼아머, 벽/절벽 중단, 피해/드롭·회복 아이템 연결이 유지되는지 직접 조작한다. 플레이어 몸 공격 애니메이션은 보류 상태를 유지한다.

## 검증 상태

최종 **Death_new 기준 새 Unity 검사202개(152 Editor + 50 Play Mode)**가 통과했다. 이전96px Death로 첫 검사를 마친 뒤 새64px 원본과 피벗으로 전체 검사를 다시 실행했다. 전체 로그는 `2026-11-05-boar-animation-validation.txt`다.

- Editor:64장 각각 Point32/무압축/mipmap 없음/원본64px/고정 피벗 확인,4클립의 Sprite 전용17키·반복 여부,12개 상태 Motion 매핑, 원본 표시/사각형 숨김, 기존 물리·발 기준 Transform, 오른쪽 원본 방향 설정과 VFX None을 확인했다.
- 실제 FSM/물리 회귀: 경고 거리·방향 확정·슈퍼아머 피해/저항·벽 Stun·기절 중 피해와 기간 불변·절벽 Recovery·돌진당1회 피해/복수 콜라이더·플레이어 실제 Knockdown/복귀·사망/공중 낙하·비활성화 정리 등 기존37개 조건을 통과했다. 제어된4u 위치 변화 쿼리 검사는 물리 성능 측정과 구분한다.
- 실제 홈 주변 배회에서 Walk 선택·여러 프레임 진행·물리 이동을 확인했다. 표시 검사용 상태 고정에서는 Idle 진행, 좌우 flip, 준비/기절 Idle1 고정, Recovery의 Idle 반복, Run이 돌진 진행률에 맞춰 늘어지지 않고 정상 속도로 반복되는 조건, 표시 Transform 유지가 통과했다.
- 실제 체력 피해로 새 Death 전환·프레임 진행·16번 마지막 프레임 유지를 확인했다. 물리 회귀와 별도로 표시 검사 일부에서는 AI/물리를 멈춰 재생 상태를 고정했으며 실제 전투 입력 전체를 반복한 것으로 표현하지 않는다.
- `2026-11-05-boar-ActualWalk.png`, `IdleRight.png`, `IdleLeft.png`, `Run.png`, `DeathStart.png`, `DeathFinal.png` 카메라 캡처를 보존했다. 좌우/돌진/최종 사망 모습을 열어 실제 방향과 원본 표시를 확인했다.
- 마지막 원본 PNG64장 모두 프로젝트 복사본과 바이트 동일, 이전 Death를 포함한 원본 폴더의 파일들이 수정되지 않았음을 해시로 확인했다. 이미지 리사이즈/패딩이나 강제 Transform 배율 조정은 없다.
- 원래 열린/활성/dirty 씬 상태 비교 `RESTORED`. 임시 BoarAnimationValidation 소스2개와 meta/요청/ready/복사 씬 제거 완료. 정리 후 영구 Runtime45개/Editor14개를 별도로 컴파일하여 각각 exit0. 직렬화 필드 경고는 있으나 오류는 없다.
- 작업 직전의 기존 Assets/Packages/ProjectSettings 중 변경은 Boar.controller, Boar.prefab, BoarAnimator.cs3개뿐이며 누락은 없다. 기존 플레이어·몸 공격 보류·회복/드롭·오소리·물리/전투 수치·저장 씬·설정과 GUID는 유지했다. 새64 PNG/meta·4클립/meta·Editor 도구는 별도 추가다.

이번 단위가 끝나면 사용자 리뷰를 기다린다. HUD/실제 VFX·다음 기능·커밋·푸시는 진행하지 않는다.
