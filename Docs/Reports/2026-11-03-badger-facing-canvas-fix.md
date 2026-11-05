# 오소리 방향 흔들림·캔버스 크기 보정

## 1. 요청과 조사 결과

사용자: 방향이 계속 좌우로 바뀌며 애니메이션을 바꾸면 오브젝트 크기가 달라진다.

코드에는 표시 방향을 BadgerBrain.FacingDirection과 물리 속도 부호로 번갈아 결정하는 경로가 있었다. 제자리 Return도 홈 도착을 확인하기 전에 방향을 바꿨고, 목표와 X가 거의 같은 경우도 즉시 반전했다. 이 세 경로를 수정했다. 모든 실제 플레이 상황의 원인을 하나로 단정하지 않으며 아래의 회귀 조건을 실제 엔진에서 확인했다.

원본을 측정한 결과 Idle/Attack은 이미 모두96×96, Walk/Death가64×64였다. 32 PPU에서 FullRect 크기는 각각3×3u와2×2u이므로 클립 전환 때 렌더러 전체 사각 크기가 달랐다. 모든 클립을96×96과 같은 발 피벗으로 통일했다. 원화의 실제 자세·실루엣 크기 차이와 투명 캔버스 크기를 구분한다.

## 2. 수정 파일과 위치

| 전체 경로 | 변경 위치·역할 |
| --- | --- |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerAnimator.cs | LateUpdate의 flipX: 살아 있는 동안 Brain.FacingDirection만 사용. 물리 속도는 보행 클립 선택에만 사용한다. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerBrain.cs | facingDeadZone=0.08u Inspector 필드, FaceTarget 메서드 추가. Alert 진입/갱신과 Chase에서 공유. Return은 실제 홈 이동이 필요하고 이동 가능할 때만 방향을 변경. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/BadgerAnimationBuilder.cs | PadCanvas 추가, Build의 피벗 설정 변경. 원화 픽셀 그대로64→96 투명 패딩, 모든 클립 custom pivot=(48,21)px. 기존96프레임은 재작성하지 않음. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Enemies/Badger.prefab | BadgerBrain.facingDeadZone=0.08 직렬화. 기존 CharacterVisual 단위 배율, .7u 몸체, 물리/공격 수치는 유지. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/ | image_1.png~image_16.png: 좌우16px, 아래15px, 위17px 투명 패딩. 같은 경로 .meta 피벗을 공통 기준으로 변경. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/ | image_1.png~image_16.png: 좌우16px, 아래16px, 위16px 투명 패딩. 같은 경로 .meta 피벗 변경. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md | 최신 방향·캔버스 수정/검증 기록 추가. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md | 같은 최신 설정과 직접 확인 절차 추가. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Design/2026-11-03-badger-animation-structure.md | 상단 최신 연결 상태에 공통 캔버스와 단일 방향 기준 반영. |

Idle/Attack은 PNG 픽셀과 캔버스가 그대로다. Editor 생성 도구는 기존4클립/컨트롤러/프리팹을 다시 저장했으며 GUID를 보존했다. 플레이어·기존 씬·패키지·Input Actions·전투 타이밍·피해·콜라이더는 바꾸지 않았다. Desktop 원본도 변경하지 않았다.

## 3. 추가·삭제 파일

영구 C# 추가 없음. 이 보고서와 아래 증거 파일을 추가했다.

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-badger-facing-canvas-fix.md: 원인·실제 변경·직접 확인 기록.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-badger-facing-canvas-validation.txt: 최종78 Editor+16 Play 및 RESTORED 로그.

검증용 임시 Editor/Runtime C#2개와 .meta, request/ready 마커, 복사 씬은 검사 후 제거했다. 기존 프로젝트 파일 삭제·이름 변경 없음. PNG32개는 기존 .meta/GUID를 유지한 수정이다.

## 4. 변경 전후와 실행 흐름

기존: 실제 속도가 있으면 그 부호로 반전하고 정지/공격이면 AI 방향으로 되돌아갔다. 수정: AI가 방향을 정하고 표시 담당자는 그 한 값만 읽는다. 넉백이나 접촉 속도가 방향을 다시 결정하지 않는다.

FixedUpdate에서 Alert/Chase가 FaceTarget을 호출한다. 목표 X차가±0.08u 이하면 기존 방향을 유지하며, 그 밖에서는 목표 쪽으로 바꾼다. 공격 중에는 기존처럼 준비 당시 방향을 유지한다. 홈 Return은 도착 판정을 먼저 처리하므로 홈에서 Idle이 될 때 임의로 뒤집히지 않는다. LateUpdate의 BadgerAnimator는 최종 방향을 flipX에 반영한다. 오른쪽false, 왼쪽true다. BadgerMotor만 물리 속도를 쓴다.

Editor Build는64px Walk/Death 프레임의 픽셀을 투명96px 사각에 복사한다. 리사이즈·필터·보간·프레임별 실루엣 맞춤은 하지 않는다. 동일96캔버스를 다시 처리하면 PNG를 바꾸지 않는다. 이후32PPU/Point/무압축/Mipmap off를 설정하고 같은 발 피벗으로 임포트한 뒤 클립과 프리팹을 연결한다. 런타임 Transform 배율을 애니메이션마다 바꾸지 않는다.

## 5. 설계 이유와 트레이드오프

방향은 AI의 의도와 공격 판정 방향에 일치시킨다. 넉백 중에는 날아가는 방향이 아니라 기존 바라보던 방향을 유지한다. facingDeadZone은 임시 Inspector 튜닝값으로, 목표와 거의 겹칠 때 잦은 반전을 막는 대신 극소 이동만으로는 돌아보지 않는다.

캔버스는 전체96×96으로 같아졌고 원화 픽셀 배율은32PPU 그대로다. Idle 첫 프레임의 불투명 높이는55px, Walk는51px인 등 원화 자체의 자세·크기 차이는 남는다. 이를 프레임마다 강제로 확대해 같게 만들면 보행/공격 자세가 펌핑하거나 픽셀이 변형되므로 이번 수정은 그러한 아트 재작업을 하지 않는다. 목표가 몸체 실루엣 자체의 완전한 동일화라면 별도 아트 검토가 필요하다. 기존 .7u 콜라이더와 원화 크기 차이도 이번에 조정하지 않았다.

## 6. 직접 확인

1. Unity에서 PlayerBadgerCombatTest를 열고 Play한다.
2. 왼쪽에서 접근해 경계·추적·공격을 확인한다. 멈췄다 움직이거나 맞아 밀릴 때 속도 부호 때문에 방향이 바뀌지 않아야 한다.
3. 오소리와 거의 같은X에서 작게 움직이면 기존 방향을 유지하고, 반대편으로 확실히 이동하면 돌아봐야 한다. 준비/타격/회수 중에는 기존 방향 고정 정책을 유지한다.
4. 멀리 이탈해 홈 복귀 후 멈출 때 불필요한 마지막 반전이 없는지 본다.
5. CharacterVisual은 Scale1을 유지하며 Idle/Walk/Attack/Death의 sprite rect는96×96, pivot은(48,21),32PPU다. 클립 전환 시 캔버스/기준 발 위치가 바뀌지 않아야 한다. 원화의 자세 변화는 정상이다.
6. Inspector BadgerBrain의 Facing Dead Zone을 조절해 필요하면 겹침 허용 폭을 검토한다. 현재값0.08u는 최종 밸런스가 아니다.

## 7. 실제 검증

- Unity Editor78개 + 실제 Play Mode16개, 총94개 최종 통과. 이전의 기본 재생12개를 포함하며 새 방향 회귀4개를 추가했다.
- 새 회귀: AI 방향과 반대인 물리 속도에도 표시 방향 유지, 목표±0.02u 흔들림에도 방향 유지, 목표가1u 반대편으로 이동하면 실제 반전, 홈 도착 시 기존 방향 유지.
- Editor는64 importer와4클립 구조/반복,4클립 공통 rect/pivot, 프리팹 연결/물리 크기 유지 확인.
- 파일 검사:64프레임 모두96×96. Idle/Attack32개는 원본 RGBA 완전 동일. Walk/Death32개는 패딩 안64×64 영역 RGBA가 Desktop 원본과 완전 동일. 새 그림·리샘플링 없음.
- 초기 반대편 이동 검사는 임시 테스트가 Rigidbody 위치만 바꾼 뒤 Transform 기준 FaceTarget을 너무 일찍 검사하여 실패했다. 테스트가 명시적으로 Transform/물리 위치를 맞추도록 고친 뒤 최종16개 통과했다. 이 과정에서 추가 기능 코드 변경 없음.
- 원래 씬 구성/dirty 상태 비교 RESTORED. 임시 도구 정리 후 최종Runtime30/Editor8 소스를 별도 컴파일: 양쪽exit0.
- Gizmos/Handles/Debug.Draw 구현 없음. 성능 수치는 측정하지 않았다.

## 8. 남은 검토와 제한

실제 사용자 입력으로 모든 지형·타깃 이동 상황의 방향 안정성을 확인한 것은 아니다. 통과한 조건과 일반화 추정은 구분한다. 발 기준은 고정되지만 원화의 발 움직임·자세 변화는 유지된다. 피해 판정 타이밍, 콜라이더/원화 몸체 맞춤, Stagger 아트 추가는 이번 범위가 아니다. 사용자 리뷰 후 다음 작업을 결정하며 커밋·푸시는 하지 않았다.

## 9. 변경 PNG 전체 경로

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_1.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_2.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_3.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_4.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_5.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_6.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_7.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_8.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_9.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_10.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_11.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_12.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_13.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_14.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_15.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Walk/image_16.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_1.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_2.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_3.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_4.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_5.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_6.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_7.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_8.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_9.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_10.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_11.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_12.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_13.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_14.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_15.png 및 .meta: 투명 패딩/공통 피벗.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Badger/Death/image_16.png 및 .meta: 투명 패딩/공통 피벗.
