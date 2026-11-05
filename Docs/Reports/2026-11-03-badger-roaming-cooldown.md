# 오소리 방향 응답·홈 배회·공격 취소 쿨타임

현재 상태: 실제 Unity Editor84개 + Play Mode23개, 총107개 검사 통과. 임시 도구 정리와 최종 영구 코드39개 분리 컴파일 완료. 사용자 조작감 리뷰 대기.

## 1. 범위

사용자 요청에 따라 방향 응답·표시 반전 기준 문제, 홈 위치 기반 좌우 배회, 경직으로 공격 취소 시 다음 쿨타임까지 대기를 함께 구현한다. 현재 구현된 실제 적은 오소리이므로 오소리에 연결하며, 배회 컴포넌트는 향후 적에도 재사용할 수 있다. 다른 적/플레이어 공격 코드를 임의로 생성·수정하지 않는다.

## 2. 변경 파일·위치

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerBrain.cs: enum 끝에 Roam/Cooldown 추가(기존 저장 enum 숫자 유지). Awake가 최초 _home을 기억하고 EnemyRoaming.Initialize 호출. Idle/Roam에서 배회 의사결정, 지형 검사 후 Motor.Step. Change(Windup)에서 _nextAttackAt 소비. OnHit는 타격 취소와 Reacting 전환. 경직 후 남은 시간에 따라 Cooldown/Chase 선택. Recovery와 Cooldown에서 FaceTarget으로 방향 응답.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerAnimator.cs: Roam 실제 이동을 Walk로 표시. Animator.Play/Update 뒤 최종 flipX를 적용해 표시 값의 최종 적용 순서 고정. 물리 속도로 방향을 정하지 않음. 후속 사용자 지적에 따라 invertSpriteFacing=true Inspector 보정 추가: 게임 오른쪽은 flipX=true, 왼쪽은 false. 실제 좌우 렌더 확인.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerData.cs: Inspector attackCooldown=1.4초 추가. 준비 시작~다음 준비 사이 최소 간격이며 경직 취소로 리셋되지 않음.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/BadgerAnimationBuilder.cs: 기존 Badger.prefab에 EnemyRoaming을 연결.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Enemies/Badger.prefab: 루트 EnemyRoaming 컴포넌트와 설정값 연결 저장.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Enemies/BadgerData.asset: attackCooldown=1.4 직렬화.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md / C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md: 최신 구현/검증 대기 상태 기록.

초기 연결 후 실제 Unity Editor에서 BadgerAnimationBuilder를 실행해 프리팹·데이터를 재저장하고 참조를 검증했다. 프리팹의 BadgerAnimator.invertSpriteFacing=true와 초기 표시 flipX도 함께 저장했다.

## 3. 추가 파일

C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/EnemyRoaming.cs: Inspector의 속도·반경·홈 복귀 가중치·이동/휴식 시간·휴식 확률로 다음 방향(-1/0/+1)을 선택한다. 위치는 호출자가 전달하고 Rigidbody에 쓰지 않는다. Initialize가 홈X를 한 번 기억하며 Pause/TurnAtObstacle가 전투 전환/지형 반응을 담당한다. .meta는 기존 프로젝트 방식으로 유지한다. 새 아트/VFX/Gizmo 없음. 이 보고서와 다음 검증 증거도 추가했다.

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-badger-roaming-validation.txt: 최종107개 검사와 씬 복원 로그
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-badger-facing-left.png: 실제 Unity 카메라의 왼쪽 목표 방향 캡처
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-badger-facing-right.png: 실제 Unity 카메라의 오른쪽 목표 방향 캡처

검증용 Editor/Runtime 스크립트2개와 .meta, request/ready 마커, 복사 씬은 검사 성공 후 제거했다. 기존 영구 파일 삭제·이름 변경 없음.

## 4. 실제 코드 흐름

시작 시 Brain이 홈 위치를 기록한다. 비전투 Idle/Roam에서 EnemyRoaming.GetDirection이 일정 기간 같은 방향으로 걷거나 잠깐 멈추도록 선택한다. 홈 중앙에서는 좌우 동등, 멀수록 홈 쪽 선택 확률 증가, 반경 밖에서는 홈 쪽을 선택한다. Brain은 BadgerMotor.CanMove로 벽과 절벽을 확인하고 반대쪽이 안전하면 회전, 양쪽이 막혔으면 휴식한다. 최종 물리 이동은 BadgerMotor만 수행한다. AI 방향을 BadgerAnimator가 읽고 Walk/Idle과 flipX를 표시한다. 이번 화면 보정은 flipX=(FacingDirection<0) XOR invertSpriteFacing이며 현재 invertSpriteFacing=true다. 원본 그림이나 게임상의 이동·공격 방향을 뒤집지 않고 표시 대응만 바꿨다. 피격/사망 중 표시 우선순위는 유지한다.

플레이어가 영역에 오면 기존 Alert, 더 접근하면 Chase로 넘어가며 배회를 중단한다. 준비·타격 중에는 시작 방향을 유지한다. 회수/쿨타임에는 목표를 다시 바라본다. 사망·피격은 배회와 공격보다 우선한다.

공격 준비 진입 순간 다음 공격 가능 시각을 현재+attackCooldown으로 기록한다. 피격 시 즉시 타격창을 닫고 Reacting. 경직이 끝나도 그 시각 전이면 제자리 Cooldown에서 기다린다. 경직이 쿨타임보다 길면 경직이 끝난 후 추적·공격 판단을 재개한다. 대기 중 추가 피격도 기존 쿨타임을 없애거나 새로 시작하지 않는다. 정상 공격도 기존 .35/.12/.65초 단계와 같은 최소 간격을 적용한다.

## 5. 설계와 임시값

EnemyRoaming은 비전투 방향/시간 선택만 담당하고 Brain은 우선순위, Motor는 물리 이동을 담당한다. 거대한 공통 적 AI 프레임워크나 추상 인터페이스를 추가하지 않는다.

임시 Inspector값: 배회 .55u/s, 반경2.5u, 홈 가중치 .45, 이동 .7~1.6초, 휴식 .4~.9초, 휴식 확률25%. 홈에서 x만큼 오른쪽이면 오른쪽 확률은 .5-.45×clamp(x/2.5,-1,1)이며 왼쪽은 나머지다. 반경 밖에서는 즉시 홈 방향을 선택한다. 지형이 없으면 안전을 위해 이동하지 않는다. 물리 스텝만큼 경계 밖으로 조금 넘을 수 있다. 공격 쿨타임1.4초는 신규 조작감 튜닝값이며 기존 공격 총1.12초보다 .28초 길다.

## 6. 직접 확인

PlayerBadgerCombatTest 씬에서 멀리 떨어져 관찰한다. 오소리는 좌우 Walk와 잠깐 Idle을 번갈아 사용하고 홈 주변을 돌아다녀야 한다. 플레이어가 영역에 들어오면 경계 후 접근 시 추적/공격한다. 회수와 쿨타임 중 반대편으로 이동하면 돌아봐야 한다. 준비/타격 중 방향 고정은 유지한다.

공격 준비 또는 타격 중 오소리를 때린다. 경직 후 남은 쿨타임 동안 새 공격이 나오면 안 된다. 늦게 때려 이미 쿨타임이 끝났다면 경직 종료 후 다시 공격할 수 있다. Inspector EnemyRoaming과 BadgerData의 Attack Cooldown으로 수치를 조절한다. 실제 지형에서 벽/절벽 반응도 확인한다.

## 7. 실제 검증

- **Editor84개 통과:**64 이미지 픽셀 설정,4클립 공통 캔버스/피벗·구조·반복,프리팹 표시/물리 보존,반전 보정 저장,배회 연결,쿨타임 데이터,고정 시드 정책 표본 검사.
- 고정 시드 정책 표본(물리 이동 검사가 아닌 Editor 정책 호출): 각1000회 중 중앙 오른쪽498회, 홈 오른쪽2u 지점에서 홈 방향860회, 홈 왼쪽2u 지점에서 홈 방향862회. 반경 밖에서 홈 방향 선택도 확인.
- **실제 Play Mode23개 통과:**경계 대기와 실제 AI의 좌우 방향, 좌우 물리 배회·휴식·Walk·홈 범위 유지, 벽·절벽 회피, 준비 중 방향 유지, 회수/쿨타임의 목표 방향 응답, 준비/타격 중 실제 피격 취소, 타격창 즉시 닫힘, 남은 쿨타임 유지·조기 재공격 금지·취소 후 피해 없음, 추가 피격에도 시각 유지, 쿨타임 후 공격 가능, 사망 중단.
- 실제 카메라 좌우 캡처를 각각 열어 **왼쪽 목표 시 그림이 왼쪽, 오른쪽 목표 시 오른쪽**을 보는 것을 확인했다. 과거 flipX 값 검사만으로 실제 원화 방향까지 맞는다고 판단한 것은 불충분했다. 이번에는 실제 렌더를 추가했다.
- 원래 씬 구성과 dirty 상태 비교 RESTORED. 원본 씬을 저장하지 않고 임시 복사 씬에서 검사했다.
- 임시 도구2개/.meta/request/ready/복사 씬 제거 후 최종 Runtime31/Editor8=39개 소스를 분리 컴파일: 양쪽exit0.
- 기존 플레이어 프리팹,이동/공격 데이터,저장 씬,Input Actions,패키지/설정은 이전 스냅샷과 같은지 확인했다. 오소리 원본 PNG·보정된 캔버스 파일은 이번 작업으로 변경하지 않았다.
- 스크립트의 Gizmos/Handles/Debug.Draw 사용0건. 성능 수치는 측정하지 않았다.

## 8. 한계

모든 적을 새로 구현하는 작업이 아니다. 기존 오소리 연결과 재사용 가능한 배회 정책에 한정한다. 시각 원화 방향 자체의 재작업, Stagger 아트, VFX, 플레이어 수치·저장/리스폰·커밋·푸시는 포함하지 않는다. 홈은 시작 시 위치이며 나중에 이동해도 자동 갱신하지 않는다. 비활성화 후에도 같은 인스턴스의 홈/쿨타임을 유지한다.

## 9. 검토

실제 검증·임시 도구 정리·보고 완료. 사용자 조작감과 Inspector 수치 리뷰를 기다린다. 다음 기능으로 자동 진행하지 않으며 커밋·푸시하지 않았다.
