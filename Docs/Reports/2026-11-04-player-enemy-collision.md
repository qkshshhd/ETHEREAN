# 플레이어–오소리 몸체 통과와 전투 감지 유지

## 1. 요청과 구현

플레이어와 오소리는 몸체로 서로 막거나 밀지 않고, 양쪽 공격·주변 인식·지형 감지를 유지한다. 기존 Player8/Enemy9 번호를 그대로 사용하고 Physics2D의8↔9 충돌 관계만 끈다. 콜라이더 제거,Trigger 변환,물리 속도 코드 추가는 하지 않았다.

## 2. 수정한 파일과 위치

| 전체 경로 | 위치·역할 |
| --- | --- |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/ProjectSettings/Physics2DSettings.asset | m_LayerCollisionMatrix의8→9/9→8 비활성화. 다른 레이어 쌍 유지. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/ProjectSettings/TagManager.asset | layers[8]=Player,layers[9]=Enemy. 빈 이름만 채우고 번호 유지. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md | 최신 몸체 통과 구현·검증 기록 추가. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md | 설정 경로·직접 확인·검증 기록 추가. |

Unity가 물리 설정을 저장하면서 네이티브 형식을5→11로 재직렬화했다. m_VelocityThreshold는 같은 값1의m_BounceThreshold로 저장됐고,현재 엔진의 시뮬레이션·서브스텝·기즈모 표시 필드도 직렬화됐다. 패키지/Unity 버전을 올린 것이 아니다. 중력·재질·반복 계산·접촉 오프셋·시뮬레이션 모드·쿼리 옵션·AutoSyncTransforms는 이전 값과 비교해 같음을 확인했고 서브스테핑은 꺼져 있다. 파일 텍스트 차이가 충돌 비트 두 개뿐이라고 표현하지 않는다.

최종 저장 검사 중 PlayerBadgerCombatTest 씬의playerLayers override가0으로 다시 저장된 것을 발견했다. 공격 감지 유지 요청에 따라256으로 복구했으며,복구 후 씬 파일 SHA256은 이번 작업 시작 스냅샷과 완전히 일치했다. 다른 편집을 되돌린 것이 아니다. 열려 있는 원본 씬도 해당 필드만 맞추며 다른 dirty 편집을 함께 저장하지 않는다. 왜0으로 돌아갔는지는 추정하지 않는다.

## 3. 추가·삭제 파일

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-04-player-enemy-collision.md: 이 구현·근거·제한·직접 확인 기록.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-04-player-enemy-collision-validation.txt: 실제 Unity 검사와 씬 복원 로그.

영구 기능 C# 추가·수정 없음. 기존 프리팹·이동/전투 데이터·Input Actions·원화·GUID 유지. 임시 Editor/Runtime 검증 도구2개와 .meta,request/ready,복사 씬은 최종 재검사 후 제거했다. 기존 영구 파일 삭제·이름 변경 없음. 새 아트·VFX·Gizmo 없음.

## 4. 물리 충돌과 감지의 차이

물리 충돌 행렬은 두 몸체가 접촉해 막히거나 밀리는 반응을 결정한다. Player–Enemy만 해제하므로 서로 통과한다. Player–Default0와Enemy–Default0는 그대로 켜져 바닥·벽과 충돌한다. 원래 Collider는 enabled=true,isTrigger=false다.

프로젝트 설정에 저장하므로 다음 실행/빌드에도 적용된다. 매 프레임 IgnoreCollision을 호출할 컴포넌트를 추가하지 않는다. 검증용 Editor는 Physics2D.IgnoreLayerCollision(8,9,true)를 한 번 호출해 이미 열린 Editor 상태도 저장 설정에 맞췄다. 기능이 이 임시 도구에 의존하지 않는다.

## 5. 기존 클래스와 실행 순서

플레이어 공격: 입력 → PlayerAttack의Active → PlayerAttackHitbox.Sample → Physics2D.OverlapBox(targetLayers) → 적 수신기/체력/피격 반응.

오소리 공격: BadgerBrain의Active → BadgerAttack.Sample → OverlapBox(playerLayers) → 플레이어 수신기/체력/CombatReaction → 기존 이동 처리로 넉백 적용.

이 겹침 조회는 몸체 접촉 반응과 별도로 동작한다. 실제 검사에서도 충돌이 꺼진 양쪽 콜라이더를 찾고 피해를 전달했다. BadgerBrain의 거리·높이 기반 발견도 접촉 이벤트가 필요 없다. PlayerGroundSensor/WallSensor와BadgerMotor의 지형 마스크는 그대로 유지한다. 겹친 오소리가 플레이어의 바닥·벽차기 접촉으로 인식되지 않는 것도 확인했다.

## 6. 적용 범위와 트레이드오프

같은 Enemy9를 쓰는 다른 적도 Player8과 통과한다. 추가 몸체 콜라이더에도 해당 레이어가 필요하며 Default0에 둔 콜라이더는 이 규칙의 대상이 아니다.

Player–Enemy 사이 OnCollision/OnTrigger 접촉 이벤트를 요구하는 기능에는 영향이 있다. 현재 공격/주변 발견은 그런 이벤트에 의존하지 않아 유지된다. 이후 아이템 획득 등Trigger 기능의 레이어는 그 기능에 맞춰 결정해야 한다.

## 7. 직접 확인

1. Edit→Project Settings→Tags and Layers에서Player8/Enemy9 확인.
2. Physics2D→Layer Collision Matrix에서Player–Enemy 해제,양쪽Default와의 충돌 유지 확인.
3. PlayerBadgerCombatTest에서A/D로 오소리를 양쪽으로 통과한다. 몸체에 막히거나 적을 밀지 않아야 한다.
4. 가까이 가면 오소리가 경계/공격하며 맞으면HP/넉백이 적용돼야 한다.
5. 플레이어 공격으로 적에게 피해를 준다. 몸체가 겹쳐도 판정한다.
6. 바닥 착지·벽 차단 확인. 오소리 BadgerAttack의Player Layers는Player가 포함돼야 하며Nothing이면 안 된다.

## 8. 검증

Unity Editor7개+실제 Play18개=25개 검사 통과.

Editor: 레이어 이름,충돌 제외,양쪽 지형 충돌,씬 몸체 레이어,solid 콜라이더,양쪽 공격 마스크.

Play: 양방향Dynamic 플레이어 통과·접촉/적 밀림 없음·접촉 무피해,중첩 시 비접촉,OverlapBox의 양쪽 감지,겹친 적을 지형/벽차기 접촉으로 오인하지 않음,양쪽 벽 차단/적 벽 감지와 낙하·착지,오소리 실제 발견/피해/넉백,실제 플레이어 공격 입력의 적 피해1회/정상 종료,solid 상태/충돌 제외 유지.

해당 실행에서 피해8/10,플레이어 넉백X0.486u 기록. 성능 수치가 아니다. 검증 복사 씬 정리 후 씬 구성/dirty 상태 비교RESTORED. 실제로 수정한 원본 씬 필드가 있다면 그 변경은 별도로dirty가 될 수 있으며 강제 저장하지 않는다.

물리 행렬 전체를 해독한 비교에서변화는(8,9)/(9,8)뿐이었다. TagManager는8/9 이름만 달라졌다. 물리 설정의 엔진 재직렬화는2절에 구분했다. 씬 필드 복구 후 기존Assets/Packages는 스냅샷과 같음을 확인했다. 소스Gizmos/Handles/Debug.Draw 사용0건.

## 9. 검토와 남은 사항

현재 Player/Badger/테스트 지형·공격을 확인한 것이며 모든 향후 적/Trigger 기능을 검사한 것은 아니다. 사용자 플레이 리뷰를 기다린다. 다음 기능·커밋·푸시는 진행하지 않는다. 최종 재확인·임시 도구 정리 결과는 아래에 기록한다.

### 최종 재확인 완료

열려 있는 씬의 공격 Player Layers도 복구한 최신 도구로 다시7 Editor+18 Play 전부 통과했다. 최종 로그를 위 증거 파일에 저장했다. 임시 도구/메타/마커/복사 씬 정리 후 영구Runtime31/Editor8=39개 소스 분리 컴파일 양쪽exit0. 현재 저장된 씬을 포함한 기존Assets/Packages 전체는 최초 스냅샷과 같고 기존 프로젝트 변경은 두Settings 파일뿐이다. 원본 씬의 연관 필드 편집은dirty로 남을 수 있으며 다른 편집을 함께 저장하지 않았다.
