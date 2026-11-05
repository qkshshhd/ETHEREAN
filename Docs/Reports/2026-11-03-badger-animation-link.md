# 오소리 사용자 애니메이션 연결 — Unity 검증 완료, 사용자 리뷰 대기

## 1. 요청과 완료 범위

사용자가 제공한 Idle/Walk/Attack/Death 각 16프레임, 총 64 PNG를 복사했다. 원본을 수정하거나 새 그림·VFX·Gizmo를 만들지 않았다. Stagger는 제공되지 않았다. 오른쪽이 원본 기본 방향이며 왼쪽만 flipX 한다. 기존 일어서기 제외 결정을 유지한다.

상태 표시 코드, 4개 클립, 컨트롤러, 프리팹 연결과 텍스처 설정을 Unity Editor API로 재생성·저장하고 검증했다. **재시도에서 Editor 74개 + 실제 Play Mode 12개 = 86개 검사가 모두 통과했다.** 이전 전투 22개 검사와 구분한 이번 신규 실행 결과다. 임시 씬 종료 후 원래 씬 구성과 dirty 상태 비교가 RESTORED로 일치했다.

## 2. 수정 파일과 위치

| 전체 경로 | 수정 위치와 역할 |
| --- | --- |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerBrain.cs | FacingDirection, AttackProgress, _stateStarted, Change, FixedUpdate의 Alert/Return: 표시용 방향·전투 진행률 제공. Alert는 최소 경고 시간 후 engageRange 안에 들어와야 추적하며 감지 범위 밖에서는 복귀. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerData.cs | engageRange/EngageRange: 추가 접근 거리 Inspector 값 2.5u, AttackRange~DetectionRange로 읽기 시 제한. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Enemies/BadgerData.asset | engageRange=2.5 직렬화. 기존 준비 .35/타격 .12/회수 .65초 유지. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Enemies/Badger.prefab | Square.SpriteRenderer 비활성화, CharacterVisual의 SpriteRenderer·Animator 및 루트 BadgerAnimator 추가/참조 연결. CharacterVisual Y=-.35, Scale=1. 기존 Rigidbody/BoxCollider .7×.7u/컴포넌트 ID 유지. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Design/2026-11-03-badger-animation-structure.md | 상단 최신 연결 상태 추가. 기존 문서 본문은 당시 설계 기록으로 구분. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md | Latest badger sprite linkage 구역: 현재 결정·검증 제한 기록. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md | 같은 최신 구역과 직접 확인 절차 추가. |

플레이어 프리팹·이동·공격 데이터, 저장된 모든 씬, Input Actions, 패키지·프로젝트 설정은 이번 작업 시작 스냅샷과 동일하다. 이전 전투 구현의 미커밋 변경은 그대로 보존했다.

## 3. 추가 파일과 연결

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerAnimator.cs: LateUpdate에서 Brain/Motor/Health/Reaction을 읽고 표시만 담당. Badger.prefab 루트가 사용하며 Rigidbody에 쓰지 않는다.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/BadgerAnimationBuilder.cs: `ETHEREAN > Enemies > Connect Badger Animations` 메뉴. 사용자 PNG 임포트 설정, 클립, 상태, 프리팹 참조를 Editor API로 재생성/연결한다. 기존 에셋 GUID를 유지한다. 초기에는 기존 직렬화 형식으로 저장했으나 이번 재시도에서 이 도구를 실제 Unity 안에서 실행해 에셋·프리팹을 재생성하고 검증했다.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Animations/Badger/Badger.controller: Base Layer의 Idle/Walk/Attack/Death 네 상태. 자동 전환/파라미터 없이 BadgerAnimator가 상태를 선택한다.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Animations/Badger/Idle.anim: 8fps 반복, 2초.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Animations/Badger/Walk.anim: 12fps 반복, 약 1.333초.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Animations/Badger/Attack.anim: 1.12초 단발. 원화 확인 후 1~6번 준비, 7~8번 타격, 9~16번 회수에 배치. 60Hz 샘플 설정이며 프레임 키 간격은 전투 단계에 맞게 서로 다르다.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Animations/Badger/Death.anim: 12fps 단발, 약 1.333초, 마지막 프레임 유지.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/: Idle/Walk/Attack/Death 하위 폴더에 각 16 PNG. 모든 복사본은 원본과 바이트 일치한다. 파일별 목록은 아래 포함.
- 이 보고서: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-badger-animation-link.md.

각 신규 Unity 파일/폴더의 .meta도 함께 저장했다. 기존 GUID는 바꾸지 않았다. 기존 프로젝트 파일 삭제·이름 변경 없음. 임시 검증 코드 2개와 .meta, request/ready 마커 및 복사 씬은 실제 검사 성공 후 제거했다. 프로젝트에 자동 Play 진입 도구를 남기지 않았다.

## 4. 실제 코드 실행 순서와 설계 이유

1. BadgerBrain.FixedUpdate가 감지/추적/준비/타격/회수/복귀를 결정한다. 실제 이동은 기존 BadgerMotor만 담당한다.
2. Change가 단계 시작 시각을 기억한다. AttackProgress는 준비·타격·회수의 누적 시간으로 0~1 진행률을 반환한다. 피해 판정은 여전히 BadgerAttack의 Active 시간에만 있다.
3. BadgerAnimator.LateUpdate는 사망 > 피격 > 공격 > 실제 보행 > 기본 대기 순서로 표시를 고른다. Idle와 Alert는 Idle을 공유한다. Chase/Return이어도 수평 속도가 거의 0이면 Idle이다.
4. Idle/Walk는 바뀔 때만 Play해 매 프레임 처음부터 반복하지 않는다. Attack은 Animator.speed=0에서 진행률로 직접 샘플링해 AI 시계와 맞춘다. Death는 한 번 재생하고 비반복 마지막 그림에 남는다.
5. 피격 중에는 Idle 첫 프레임에서 멈춘다. 실제 경직/넉백/Knockdown 동작은 기존 CombatReaction이 유지한다. 반응 아트가 생기면 이 표시 어댑터를 확장할 수 있다.
6. 오른쪽은 flipX=false, 왼쪽은 true. 공격 방향은 기존 Brain의 고정 방향을 사용한다. 사망 시 마지막 방향을 유지한다.

애니메이션은 게임 규칙을 결정하지 않는다. 표시와 AI/이동/타격 책임을 분리하여 Animation Event 누락으로 피해 판정이 남는 문제를 피한다. 대신 전투 시간을 변경하면 Editor 메뉴를 다시 실행해 클립 키 배치를 맞춰야 한다. 이것은 신규 아트가 필요할 때도 명시적으로 검토할 부분이다.

## 5. 픽셀·크기·피벗

모든 PNG는 Sprite Single, 32 PPU, Point(no filter), 무압축, Mipmap off, NPOT None, FullRect다. PNG 자체를 리사이즈하거나 잘라내지 않았다.

Idle/Attack은 96×96, Walk/Death는 64×64다. 첫 프레임의 투명 여백을 기준으로 고정 발 피벗을 적용했다: Idle/Attack (0.5,21/96), Walk (0.5,6/64), Death (0.5,5/64). 매 프레임 자동 재정렬하지 않아 원화의 의도된 움직임을 보존한다. 클립 간·프레임 간 크기/발 위치 변화는 실제 재생에서 검토해야 한다.

제공 그림은 32 PPU에서 기존 .7u 물리 몸체보다 크게 보인다. 현재 플레이어 원화보다도 크다. 원화 크기를 임의 축소하거나 몸체·타격 범위를 자동 확대하지 않았다. **시각적 크기와 판정 일치, 사망 자세의 바닥 위치는 사용자 확인이 필요하다.**

## 6. 직접 확인 순서와 예상 결과

1. Unity 6000.4.7f1에서 프로젝트를 열고 컴파일 완료를 기다린다. Console에 오류가 없어야 한다.
2. 필요하면 `ETHEREAN > Enemies > Connect Badger Animations`를 실행한다. 기존 BadgerData와 Badger.prefab이 필요하며 Play 중 실행하지 않는다.
3. PlayerBadgerCombatTest 씬을 열고 Play한다. 오소리 원화가 보이며 정사각형은 보이지 않아야 한다.
4. 감지 거리 4u 안, 추적 시작 2.5u 밖에 머문다. Console 경고는 Alert 진입 1회, 제자리 Idle을 반복해야 한다. 더 다가가면 실제 이동과 Walk를 표시한다.
5. 양쪽으로 이동해 원본 오른쪽/왼쪽 반전을 확인한다. 공격은 준비→타격→회수 그림을 재생하며 타격 외 시점에는 피해가 없어야 한다.
6. 공격으로 오소리를 맞힌다. Stagger가 없어 기본 자세로 잠시 정지하되 기존 경직·넉백은 적용되어야 한다. HP가 0이면 Death를 한 번 재생하고 마지막 프레임에 남아야 한다.
7. 멀어지면 Walk로 홈 복귀 후 Idle을 확인한다. 클립 전환 시 발이 튀는지, 원화와 몸체/공격 판정이 맞는지도 확인한다.

이 항목들은 사용자 직접 확인 절차다. 아래의 12개 Play Mode 검사 범위는 자동 확인했으며, 별도의 물리 키보드 조작감·경계 왕복·복귀 보행 전체 흐름과 아트/판정 일치는 사용자 검토가 남아 있다.

## 7. 실행한 검증과 제한

- 최종 C# 38개를 Runtime 30개 / Editor 8개로 나누어 별도 컴파일: 양쪽 exit 0. Runtime을 먼저 DLL로 만든 뒤 Editor가 참조하도록 검사했다. Inspector 할당 필드에 대한 기존 CS0649 경고는 있다.
- 정적 검사 통과: 64 PNG 원본 바이트 일치, 64 importer 설정, 클립 키/루프 정책, 에셋 GUID의 중복·대상 존재, prefab/controller 로컬 ID 참조.
- 작업 시작 SHA256 비교: 기존 Assets/Packages/ProjectSettings 중 변경은 BadgerBrain.cs, BadgerData.cs, BadgerData.asset, Badger.prefab 네 파일뿐. 삭제 없음. 새 에셋은 별도 추가.
- 스크립트의 Gizmos/Handles/Debug.Draw 검색 결과 0건.
- 초기 별도 Unity 시작 시도는 `ERROR_SUCCESS == status`, exit 1로 실패했다. 이후 사용자가 열어 둔 Editor에서 새로고침 후 임시 검증 도구가 실행됐다. 아래 결과가 최신 증거다.
- **Editor 74개 통과:** 64프레임 임포트 설정, 4클립의 sprite-only/17키 구조, 4클립 반복 정책, 표시 어댑터 연결/정사각형 숨김, 기존 .7u 몸체와 원화 unit scale 유지.
- **실제 Play Mode 12개 통과:** 바깥 영역에서 경고 시간 후에도 Alert 유지, Alert의 Idle 표시, 왼쪽 반전, Idle 프레임 진행, 추가 접근 후 Chase+Walk, Walk 프레임 진행, 실제 전투 단계의 Attack 재생, Stagger 미제공 시 정지 Idle, 오른쪽 원본 방향, 사망 우선 전환, Death 프레임 진행, Death 마지막 프레임 유지.
- 이 테스트의 Attack 항목은 실제 공격 상태에서 클립을 선택함을 확인했다. 각 타격 프레임과 피해 발생 시각의 정밀 동시 측정이나 아트의 의도 적합성을 확인한 것은 아니다.
- 실제 256×256 카메라 캡처를 열어 픽셀 원화와 사망 자세가 표시됨을 확인했다. 발 기준의 클립 간 흔들림, 최종 아트 크기/판정 일치 검토를 대신하지 않는다.
- 검증 후 임시 도구/.meta/request/ready/복사 씬 정리. 최종 영구 Runtime30/Editor8 소스를 다시 분리 컴파일해 양쪽 exit0 확인.
- Git diff whitespace 검사에는 이전 작업의 Player.prefab 빈 m_Name 후행 공백 3건이 남아 있다. 이번 작업은 그 파일을 변경하지 않았다.

## 8. 임시값·성능·남은 작업

추적 시작 2.5u, 표시 속도 기준 .01u/s, Idle8/Walk12/Death12fps, 공격 프레임 단계 배치는 검토 가능한 임시값이다. 기존 이동/중력/물리 몸체/공격 시간은 유지했다. 공격 원화 타격 자세와 실제 판정의 완전한 일치 및 보행 미끄러짐은 미검증이다. 성능 수치는 측정하지 않았다. 새로운 상태 프레임워크나 가상 인터페이스는 추가하지 않았다.

Stagger 및 선택적 Knockdown 아트는 미제공이며 이 작업으로 완성되지 않는다. 기본 에디터 재생 검증은 완료했으며 사용자 조작감·아트/판정 검토가 필요하다. 다음 기능·커밋·푸시는 진행하지 않았다.


### 이번 재시도의 파일 변경

기능 C# 수정·추가 없음. BadgerAnimationBuilder가 기존 Badger.prefab, BadgerData.asset, 4개 클립/컨트롤러 및 64개 PNG importer .meta를 실제 Editor API로 재저장했다. 유지된 에셋 GUID와 기존 씬·플레이어 설정은 보존했다. 검증 종료 후 임시 스크립트 2개/.meta와 마커는 삭제했다.

문서 수정: 이 보고서의 1·3·6·7·8절, AGENTS.md와 PLAYER_SETUP.md의 최신 검증 완료 구역, Docs/Design/2026-11-03-badger-animation-structure.md의 최신 연결 상태.

추가 검증 증거:

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-badger-animation-validation.txt: 실제 74 Editor + 12 Play 검사와 RESTORED 로그.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-badger-animation-capture.png: 실제 카메라의 사망 자세 캡처. 원화 제작 파일이 아니다.

## 9. 신규 스프라이트 파일 목록

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_1.png (및 같은 경로의 .meta): Idle 1번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_2.png (및 같은 경로의 .meta): Idle 2번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_3.png (및 같은 경로의 .meta): Idle 3번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_4.png (및 같은 경로의 .meta): Idle 4번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_5.png (및 같은 경로의 .meta): Idle 5번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_6.png (및 같은 경로의 .meta): Idle 6번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_7.png (및 같은 경로의 .meta): Idle 7번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_8.png (및 같은 경로의 .meta): Idle 8번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_9.png (및 같은 경로의 .meta): Idle 9번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_10.png (및 같은 경로의 .meta): Idle 10번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_11.png (및 같은 경로의 .meta): Idle 11번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_12.png (및 같은 경로의 .meta): Idle 12번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_13.png (및 같은 경로의 .meta): Idle 13번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_14.png (및 같은 경로의 .meta): Idle 14번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_15.png (및 같은 경로의 .meta): Idle 15번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Idle/image_16.png (및 같은 경로의 .meta): Idle 16번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_1.png (및 같은 경로의 .meta): Walk 1번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_2.png (및 같은 경로의 .meta): Walk 2번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_3.png (및 같은 경로의 .meta): Walk 3번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_4.png (및 같은 경로의 .meta): Walk 4번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_5.png (및 같은 경로의 .meta): Walk 5번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_6.png (및 같은 경로의 .meta): Walk 6번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_7.png (및 같은 경로의 .meta): Walk 7번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_8.png (및 같은 경로의 .meta): Walk 8번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_9.png (및 같은 경로의 .meta): Walk 9번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_10.png (및 같은 경로의 .meta): Walk 10번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_11.png (및 같은 경로의 .meta): Walk 11번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_12.png (및 같은 경로의 .meta): Walk 12번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_13.png (및 같은 경로의 .meta): Walk 13번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_14.png (및 같은 경로의 .meta): Walk 14번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_15.png (및 같은 경로의 .meta): Walk 15번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_16.png (및 같은 경로의 .meta): Walk 16번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_1.png (및 같은 경로의 .meta): Attack 1번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_2.png (및 같은 경로의 .meta): Attack 2번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_3.png (및 같은 경로의 .meta): Attack 3번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_4.png (및 같은 경로의 .meta): Attack 4번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_5.png (및 같은 경로의 .meta): Attack 5번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_6.png (및 같은 경로의 .meta): Attack 6번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_7.png (및 같은 경로의 .meta): Attack 7번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_8.png (및 같은 경로의 .meta): Attack 8번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_9.png (및 같은 경로의 .meta): Attack 9번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_10.png (및 같은 경로의 .meta): Attack 10번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_11.png (및 같은 경로의 .meta): Attack 11번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_12.png (및 같은 경로의 .meta): Attack 12번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_13.png (및 같은 경로의 .meta): Attack 13번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_14.png (및 같은 경로의 .meta): Attack 14번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_15.png (및 같은 경로의 .meta): Attack 15번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Attack/image_16.png (및 같은 경로의 .meta): Attack 16번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_1.png (및 같은 경로의 .meta): Death 1번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_2.png (및 같은 경로의 .meta): Death 2번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_3.png (및 같은 경로의 .meta): Death 3번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_4.png (및 같은 경로의 .meta): Death 4번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_5.png (및 같은 경로의 .meta): Death 5번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_6.png (및 같은 경로의 .meta): Death 6번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_7.png (및 같은 경로의 .meta): Death 7번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_8.png (및 같은 경로의 .meta): Death 8번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_9.png (및 같은 경로의 .meta): Death 9번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_10.png (및 같은 경로의 .meta): Death 10번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_11.png (및 같은 경로의 .meta): Death 11번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_12.png (및 같은 경로의 .meta): Death 12번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_13.png (및 같은 경로의 .meta): Death 13번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_14.png (및 같은 경로의 .meta): Death 14번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_15.png (및 같은 경로의 .meta): Death 15번 프레임.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_16.png (및 같은 경로의 .meta): Death 16번 프레임.
