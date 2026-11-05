# 오소리 Walk·Death 교체

## 요청과 적용

사용자가 개선한 `C:/Users/pjw/OneDrive/Desktop/ETHEREAN_SPRITE/오소리_Walk new`와 `오소리 Death`의 각 16프레임을 가져왔다. 원본은 모두 96×96 PNG이며, 리사이즈/재그림/패딩 없이 바이트 그대로 기존 Unity 경로에 덮어썼다. PNG의 기존 meta와 GUID는 보존해 Walk.anim/Death.anim의 Sprite 참조를 유지했다.

새 Walk는 기존 보행 이미지보다 몸이 왼쪽 약2px, 아래쪽 약17px에 있다. 프레임마다 모양의 중심을 재계산하면 발 위치와 몸의 흔들림까지 지워지므로, 클립 전체에 한 개의 고정 피벗을 사용했다. 현재 보행 첫 프레임의 기존 몸 위치를 기준으로 Walk=(46,4)px, 기존 Idle의 중립 몸 중심과 바닥을 기준으로 Death=(46,21)px를 적용했다. 기존 Idle/Attack은 (48,21)px를 유지한다.

## 파일별 변경 위치와 역할

전체 경로의 프로젝트 루트는 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN`이다.

| 파일 | 변경 위치 및 기능 |
|---|---|
| `Assets/_Project/Art/Sprites/Badger/Walk/image_1.png` ~ `image_16.png` | 새 Walk 원본 16장으로 교체. 원본 픽셀 그대로 |
| 같은 Walk PNG의 `.meta` 16개 | Custom Pivot=(46/96,4/96). Point32/무압축/mipmap 없음 유지. GUID 유지 |
| `Assets/_Project/Art/Sprites/Badger/Death/image_1.png` ~ `image_16.png` | 새 Death 원본 16장으로 교체. 원본 픽셀 그대로 |
| 같은 Death PNG의 `.meta` 16개 | Custom Pivot=(46/96,21/96). Point32/무압축/mipmap 없음 유지. GUID 유지 |
| `Assets/_Project/Scripts/Editor/BadgerAnimationBuilder.cs` | Build를 클립 목록/프리팹 연결 여부를 받는 내부 함수로 분리. RefreshWalkAndDeath 메뉴 추가. Import 시 클립별 고정 pivot 적용. 읽기만 하는 BadgerData를 불필요하게 저장하던 코드 제거 |
| `AGENTS.md`, `PLAYER_SETUP.md` | 기본 애니메이션 원본 폴더·픽셀 설정·원본/GUID 보존·고정 피벗 원칙 및 이번 결과 기록 |
| 이 보고서 및 validation/capture 파일 | 변경 이유와 실제 검증 근거를 기록하기 위한 문서/이미지 |

새 영구 Runtime/Editor 스크립트나 새로운 애니메이션 GUID는 추가하지 않았다. 기존 BadgerAnimator/Brain/Motor, 방향 보정 invertSpriteFacing=true, 몸체/전투/이동 수치는 바꾸지 않는다. Walk.anim/Death.anim은 기존 자산과 참조를 재사용하며 필요 시 기존 경로에 갱신한다. 전체 Connect 메뉴 대신 `ETHEREAN > Enemies > Update Badger Walk And Death`로 두 클립만 갱신해 Idle/Attack·프리팹 설정을 건드리지 않는다.

## 동작 원리

기존 BadgerBrain과 Motor의 실제 상태·속도를 BadgerAnimator가 읽어 Walk를 선택한다. 죽으면 CombatHealth.IsDead에 따라 Death를 선택한다. 바뀐 Sprite들이 기존 클립의 같은 GUID 경로에 있으므로 새로운 상태나 전환 조건이 필요 없다.

Walk는 12fps 반복 재생, Death는 12fps 단발 재생 후 마지막 프레임 유지 정책이다. 이번 수정은 표시 자산과 Editor import 책임에 한정한다. root/CharacterVisual의 Transform 애니메이션이나 런타임 위치 보정 스크립트를 추가하지 않는다.

## 위치 맞춤 기준

- Walk의 기존 첫 프레임 가시 영역 X 중심은47px, 기존 피벗48px이므로 상대 중심은-1px이다. 새 첫 프레임 가시 영역 중심45px에서 피벗46px을 빼면 동일한-1px이다.
- 새 Walk의 가시 영역 바닥은 원본 아래에서4~5px이다. 고정 Y피벗4px을 적용하면 바닥은 기준점 위0~1px에서 움직인다. 프레임별1px 보행 변화는 유지한다.
- 기존 Idle 첫 프레임 가시 중심50.5px-피벗48px=2.5px. 새 Death 첫 프레임은48.5px-피벗46px=2.5px로 중립 몸 중심을 맞춘다.
- Death의 가시 바닥은 모든 프레임에서 아래21px이므로 Y피벗21px으로 고정한다. 쓰러지는 과정의 의도된 실루엣/중심 변화까지 없애는 보정은 하지 않는다.

이는 현재 제공된 그림을 기존 그림에 정렬한 기준이다. 다른 자세의 실루엣 전체가 픽셀 단위로 같다는 뜻은 아니다. 이후 새 애니메이션도 먼저 위치/크기/프레임을 확인한 뒤 해당 클립의 고정 기준점을 정한다.

## 확인 상태

- 32개 PNG 모두 원본과 프로젝트 복사본이 바이트 동일함을 확인했다.
- Walk의 바닥 변동0~1px, Death0px를 PNG alpha 경계로 확인했다.
- 새 Editor/임시 검증 Runtime을 별도 Roslyn 컴파일하여 오류0을 확인했다.
- 실제 Unity 6000.4.7f1에서 **121개 검사(107 Editor + 14 실제 Play Mode)**가 통과했다. 전체 로그는 `2026-11-04-badger-animation-replacement-validation.txt`에 저장했다.
- Editor는 전체64프레임의 픽셀 설정, 교체32프레임의 고정 피벗/96×96, 네 클립의 sprite 전용 바인딩과 반복 정책, 기존 물리 크기/표시 연결/방향 보정 유지를 확인했다.
- 복사 씬의 실제 배회에서 새 Walk 선택·프레임 진행·물리 이동·방향 일치를 확인했다. Idle/Walk/Attack/Death 각16프레임의 통제된 Animator 샘플에서는 root와 CharacterVisual의 위치/배율이 바뀌지 않았다. 이 샘플은 실제 공격 입력 검사와 구분한다.
- 실제 CombatHealth 피해로 사망시켜 새 Death 전환·진행·image_16 도달·최종 프레임 유지·Transform 기준 유지를 확인했다.
- 실제 배회 LiveWalk와 통제된 IdleStart/WalkStart/DeathStart, 실제 사망 DeathFinal의 카메라 캡처5장을 저장하고 열어 확인했다. 파일은 같은 보고서 폴더의 `2026-11-04-badger-replacement-*.png`다. 형태가 다른 포즈의 실루엣까지 같게 만드는 검사는 아니다.
- 검사 종료 후 원래 열린/활성/dirty 씬 상태 비교가 `RESTORED`였다. 종료 후 연결이 끊겼지만 재연결하여 남은 정리를 완료했다.
- 임시 `BadgerReplacementValidation.cs`, `BadgerReplacementValidationDriver.cs`와 각각의 meta, request/ready, 복사 씬은 모두 제거했다. 정리 후 영구 **Runtime33개 / Editor10개**를 별도 컴파일하여 각각 exit0을 확인했다. 직렬화 필드의 CS0649 경고는 있으나 오류는 없다.
- 작업 직전 스냅샷 비교에서 기존 파일 변경은 교체32 PNG + 해당32 meta + BadgerAnimationBuilder.cs의 총65개뿐이었다. Idle/Attack, 네 클립/Controller, Badger prefab/data, 플레이어/검, 저장 씬, Packages/ProjectSettings 및 기존 GUID는 보존됐다. 재생은 기존 clip 참조 경로를 통해 새 PNG를 표시하므로 clip 파일 자체를 바꿀 필요가 없었다.
- VFX·Gizmo·코드 생성 아트는 추가하지 않았다. 커밋/푸시나 다음 기능 착수도 하지 않았다.

## 직접 확인

기존 PlayerBadgerCombatTest에서 오소리의 배회/추적을 관찰해 새 Walk가 재생되는지 확인한다. Idle→Walk→Idle에서 발 높이나 몸 기준이 갑자기 아래로 떨어지지 않아야 한다. 오소리를 처치하면 새 Death가 한 번 재생되고 마지막 프레임에서 머문다. 스프라이트 방향과 기존 공격/피격 동작은 함께 살펴본다.

## 앞으로의 원본 위치

사용자가 애니메이션을 만들었다고 하면 `C:/Users/pjw/OneDrive/Desktop/ETHEREAN_SPRITE`에서 해당 폴더를 먼저 찾아 Unity로 가져온다. 이 원칙을 AGENTS.md와 PLAYER_SETUP.md에 저장했다. 별도의 상시 폴더 감시 기능을 구현한 것은 아니다.
