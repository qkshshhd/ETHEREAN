# 플레이어 넉백 복구 — 실제 타격 연결과 수치

현재 상태: 사거리 보정까지 반영한 최종 Unity7 Editor+37 실제 Play Mode=44개 검사 통과. 임시 도구 정리와 최종 영구 코드39개 분리 컴파일 통과. 사용자 조작감 리뷰 대기.

## 1. 확인한 원인

저장된 PlayerBadgerCombatTest 씬의 Badger 인스턴스에 playerLayers.m_Bits=0 override가 있었다. 프리팹의256과 달리 씬에서는 Nothing으로 덮여, 애니메이션/Active는 실행돼도 플레이어를 전혀 조회하지 못했다. 실제 런타임 mask=0, Active 열린 상태에서 hits=0, 플레이어 HP50 유지로 재현했다. 왜 이 override가 생겼는지는 추정하지 않는다.

플레이어 수신기→CombatReaction→PlayerCombatCoordinator→PlayerLocomotion→PlayerMovement 경로 자체는 실제 동적 Rigidbody 피격으로 작동했다. 기존(1.5,1) 피격을 직접 넣은 통제 검사에서 이동은X0.128u/Y0.0272u로 작았다. 이는 실제 오소리 AI 공격 성공 검사와 구분한다.

별도로 사용자 조정 공격 거리1.5u에 비해 BadgerAttack 박스 끝은 고정1.0u였다. 준비 시작은 사거리 안이어도 타격이 닿지 않을 수 있는 불일치를 함께 수정했다.

## 2. 수정 파일과 정확한 위치

| 전체 경로 | 위치와 역할 |
| --- | --- |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scenes/Tests/PlayerBadgerCombatTest.unity | Badger 인스턴스, BadgerAttack.Player Layers의 m_Bits override0→256(플레이어 레이어8). 다른 씬 오브젝트·연결은 유지. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Enemies/BadgerData.asset | knockback (1.5,1)→(3.5,2.5)u/s. 사용자 공격 거리1.5/쿨타임2초 보존. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerData.cs | knockback 초기값을 같은(3.5,2.5)로 변경해 이후 생성 데이터도 옛 약한 값으로 돌아가지 않게 함. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Enemies/BadgerAttack.cs | Sample의 고정 박스 중심/폭을 AttackRange에 연결. reach=max(1,AttackRange), 앞쪽0.1~reach, 높이0.65 유지. 기존 사거리0.9 때 기존0.1~1.0 영역을 보존하며 현재1.5에서는0.1~1.5로 확장. |

열려 있던 원본 테스트 씬에도 같은 Player Layers 필드만 Editor SerializedObject로 복구한다. 이 경우 씬은 dirty가 되며 다른 미저장 변경을 함께 강제 저장하지 않는다. 저장된 씬 파일에서는 해당 override 값만 교정했다. 플레이어 프리팹/이동/점프/피격 처리 코드, EnemyRoaming/방향·쿨타임 코드, 원화·클립·VFX는 바꾸지 않았다.

## 3. 추가·삭제 파일

영구 기능 C# 추가 없음. 다음 보고·증거 파일을 추가했다.

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-04-player-knockback.md: 이 기능 변경 보고서.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-04-player-knockback-baseline.txt: mask0/타격 조회0 재현과 기존 통제 피격 측정 로그. 실패는 수정 전 원인 재현 결과다.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-04-player-knockback-validation.txt: 사거리 보정 포함 최종44개 검사 결과와 씬 복원 로그.

임시 Editor/Runtime 스크립트2개와 .meta, request/ready, 임시 복사 씬은 검사 후 제거했다. 기존 영구 파일 삭제·이름 변경 없음.

문서 수정: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md 및 C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md의 최신 넉백 복구 구역에 원인·설정·검증·수동 확인 사항을 기록했다.

## 4. 실행 흐름

BadgerBrain이 Active로 진입 → BadgerAttack이 플레이어 레이어만 OverlapBox 조회 → CombatDamageReceiver.TryReceiveHit가 HP 감소와 CombatReaction.Begin → PlayerCombatCoordinator가 PreparingMovement에서 피격 반응을 우선 선택 → PlayerLocomotion이 외부 속도를 일반 입력·점프보다 우선 처리 → PlayerMovement가 유일하게 실제 Rigidbody2D 속도를 쓴다.

수평 성분은 knockbackDrag10으로 줄고 Stagger0.2초 동안 입력을 제한한다. 세로 성분은 최초1회 속도로 주고 기존 중력으로 처리한다. AddForce나 별도 Rigidbody writer를 추가하지 않았다. 경직 종료 후 기존 직접 입력 속도3으로 돌아간다.

## 5. 설계와 임시값

주원인은 레이어 연결이므로 공통 이동/피격 구조를 새로 만들지 않는다. 오소리의 공격 데이터에만 충격 속도를 조정한다. 벡터는 힘(N)이 아니라 속도(u/s)로 해석하며 질량에 곱하거나 AddForce하지 않는다. 새로운(3.5,2.5)은 현재 플레이어 크기에서 보이는 넉백을 위한 임시 튜닝값이다. 다른 적은 각자 공격 데이터로 정한다.

사거리 연동은 타격 폭의 불일치를 고친다. 높이·피해·경직 시간·쿨타임은 유지한다. 현재 박스 판정은 지형으로 가려지는 것을 검사하지 않으며, 원화의 모든 프레임 외곽을 따르는 판정은 아니다.

## 6. 직접 확인

1. PlayerBadgerCombatTest 씬에서 Badger → BadgerAttack → Player Layers에 플레이어 레이어가 포함되는지 확인한다. Nothing이면 피해/넉백 모두 없다.
2. Play 후 오소리 가까이에서 공격을 맞는다. Console HP가8 감소하고 오소리 반대쪽으로 밀리면서 조금 떠야 한다.
3. 양쪽에서 맞아 방향을 확인한다. 맞는 동안 오소리 쪽 이동 키를 누르고 있어도 즉시 넉백을 덮어쓰지 않아야 한다. 경직 후 입력은 복구돼야 한다.
4. 벽을 등지고 맞으면 벽을 통과하지 않아야 한다. 현재 사거리1.5 안에서도 타격이 닿는지 확인한다.
5. BadgerData.asset의 Knockback X/Y로 감각을 조절한다. 수치가 최종 밸런스는 아니다. 열려 있던 씬에 수정 표시가 있다면 관련 레이어 수정을 확인하고 저장한다.

## 7. 검증과 증거

초기 실제 공격 검사는 mask0 때문에 HP/타격 미발생으로 실패했으며 원인 재현 증거다. 수신기에 직접 넣은 통제 피격에서X.128/Y.0272를 확인했다.

레이어 복구/충격 속도 조정 후 실제 오소리 공격 검사7 Editor+31 Play 통과: 좌우 피격, 맞는 중 방향키 입력보다 넉백 우선, 입력 복구, 단일 피해, 벽 충돌, 복합 콜라이더 중복 피해 방지. 무입력X.52/Y.1232, 방향키 입력 케이스X.486/Y.1232. 벽 케이스X약.0515로 제한됐다.

**사거리 연동까지 반영한 최종 결과는7 Editor+37 실제 Play Mode=44개 전부 통과**다. 위31개 검사를 재실행하고, 실제 AI가1.48u 거리에서 타격한 뒤 피해1회·방향·명확한 넉백·입력 우선순위·경직 종료의6개 검사를 추가했다. 수정 후 모든 측정 케이스에서 플레이어는 실제 Dynamic Rigidbody이며 플레이어 이동 컴포넌트를 비활성화하지 않았다.

최종 로그에는 무입력 좌우X0.52u/Y0.1232u, 방향키 입력 케이스X0.486u/Y0.1232u, 벽 케이스X약0.0515u가 기록됐다. 환경/입력/벽에 따라 이동 거리가 달라진다.

임시 파일 정리 후 영구 Runtime31/Editor8=39개 소스를 별도 컴파일해 양쪽exit0 확인했다. 원래 씬 구성은 테스트 복사 씬 제거 후 RESTORED로 일치했다. 단, 열려 있던 원본 씬의 레이어 수정은 요청된 변경이므로 dirty로 남을 수 있다. 상태 비교 기준은 해당 연결 복구 후이며 다른 미저장 편집을 함께 강제 저장하지 않았다. Gizmos/Handles/Debug.Draw 검색0건. 수치는 물리 이동 측정이며 성능 벤치마크가 아니다.

## 8. 보존과 한계

공격 거리1.5/쿨타임2는 사용자가 최근 조정한 값으로 유지한다. 이전 보고서의0.9/1.4를 덮어쓰지 않는다. 씬의0 레이어 override만 문제 연결로 복구했다. 미저장 원본 씬은 다른 변경을 강제로 저장하지 않는다. 플레이어 이동/점프/가속 없는 입력 정책은 유지한다. 실제 조작감·원화/판정 정합성에 대한 사용자 검토는 별도로 필요하다. 성능 수치는 측정하지 않았다.

## 9. 마무리

최종 검사·임시 파일 정리·보고 기록을 완료했다. 오소리 전투를 완료로 단정하거나 다음 기능으로 넘어가지 않는다. 커밋·푸시하지 않았다.
