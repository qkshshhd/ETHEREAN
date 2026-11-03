# 오소리 구현 전 Gizmo 제거 및 커밋 준비 — 2026-11-03

## 1. 승인과 순서

사용자는 오소리 전투 사이클 구현을 승인했다. 오소리는 단순 정사각형 프리팹을 사용한다. 경고 VFX는 사용자가 제작하며, 코드로 임의의 디자인/이펙트/HUD를 만들지 않는다. Gizmo는 필요할 때 임시로만 허용하고 기능 완료 전 완전히 제거한다.

이번 선행 순서는 **기존 Gizmo 전부 제거 → 현재까지 작업 로컬 커밋 → 오소리 구현**이다. 커밋 전에는 새 전투 코드를 시작하지 않는다. push는 승인되지 않았다.

## 2. 수정 파일과 정확한 위치

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackHitbox.cs`: OnDrawGizmosSelected 전체와 전용 previewData 필드 제거. Sample/BeginAttack/EndAttack의 실제 판정·중복 방지·취소 처리 유지.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerCombatTestSceneBuilder.cs`: Create 내부의 previewData 연결 제거.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab`: Unity PrefabUtility 재저장으로 PlayerAttackHitbox.previewData 직렬화 항목 제거. 사용자 수치·다른 컴포넌트·참조·GUID 유지.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Plans/2026-11-03-health-reaction-badger-unit.md`: 승인 범위·정사각형 외형·사용자 경고 프리팹 슬롯·코드 디자인 금지·커밋 선행 조건 반영. CombatDebugHud 제안을 제외하고 Inspector/검증 로그로 상태 확인.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md`, `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md`: Badger cycle authorized after cleanup and commit 구역에 최신 결정과 작업 순서를 기록.

## 3. 추가·삭제 파일

영구 추가는 본 보고서 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-gizmo-cleanup-before-badger.md`뿐이다. 영구 파일 삭제나 이름 변경은 없다.

이번 프리팹 재저장에만 사용한 GizmoCleanupSave.cs와 meta 및 request는 실행 후 제거했다. 오소리 클래스/데이터/프리팹/씬/효과를 아직 추가하지 않았다.

## 4. 변경 전후와 원리

기존에는 플레이어 선택 시 에디터가 OnDrawGizmosSelected를 호출해 공격 데이터의 상자를 그렸다. 해당 호출 진입점과 그리기 코드·전용 데이터 참조를 모두 제거했으므로 더 이상 커스텀 상자를 그리지 않는다. 공격 입력 → PlayerAttack의 Active → PlayerAttackHitbox.Sample → Physics2D.OverlapBox → AttackHitReceiver 흐름은 그대로다.

에디터 전용 표시 기능만 제거하므로 공격 판정 책임을 다른 클래스에 옮기거나 새로운 인터페이스를 만들 필요가 없다. 사용하지 않는 preview 필드를 남기지 않았고 생성 도구도 없는 필드를 다시 연결하지 않게 정리했다.

## 5. 검증

- Assets 전체 C#에서 OnDrawGizmos/Gizmos/Handles/Debug.DrawLine/Debug.DrawRay/DrawGizmo 검색 결과 **0개**.
- 프로젝트 C#/프리팹에서 previewData 참조 **0개**.
- 실제 Unity Editor API가 프리팹을 재저장한 성공 로그 확인.
- 임시 도구 정리 후 최종 **28개 C#** 별도 컴파일 통과, 오류 0.
- 플레이 동작은 이번 표시 제거에서 새로 재검사하지 않았다. 과거 공격/이동 Play 검사 수를 이번 검사 수로 재사용하지 않는다.

이 검사는 프로젝트 커스텀 그리기 코드를 대상으로 한다. Unity 자체의 Collider 편집 도구나 에디터 내장 표시 기능을 삭제하는 변경은 아니다.

## 6. 직접 확인

Unity에서 플레이어를 선택해도 커스텀 공격 범위 상자가 나타나지 않아야 한다. PlayerAttackHitbox Inspector의 Preview Data 항목도 없어야 한다. Hitbox Offset/Size 등 실제 공격 데이터는 유지한다. 기능이 완성된 후 임시 그리기 코드가 다시 남지 않도록 이후 작업 종료 검사에도 검색을 포함한다.

## 7. 커밋 범위

현재 HEAD 관측값은 090f845다. 이전에 승인·구현한 3타 전투/시간 기반 판정, 이동·전투 폴더 분리, 캐릭터 이미지/애니메이션 연결, 이동 튜닝/테스트 맵, 관련 문서와 이번 Gizmo 제거를 한 체크포인트로 기록한다. 사용자가 staged해 둔 픽셀 씬 변경도 이 현재 작업 커밋에 포함하며 임의로 해제하지 않는다.

예정 메시지: `feat: integrate player animations and combat; remove debug gizmos`.

대상은 AGENTS.md, PLAYER_SETUP.md, Assets/_Project, Docs/Design, Docs/Plans, Docs/Reports의 현재 변경이다. Logs/Library/Temp나 임시 검사기는 포함하지 않는다. 실제 커밋 결과는 실행 후 최종 답변과 이 보고서에 구별해서 기록한다.

## 8. 제한

Git 메타데이터 쓰기가 차단되면 커밋 성공을 주장하거나 다른 경로로 우회하지 않는다. 사용자 로컬 커밋을 확인하기 전 오소리 구현을 시작하지 않는다. 기존 임시 아트 비교 자료나 과거 보고서 이미지는 Gizmo 코드가 아니므로 이번 정리에서 삭제하지 않는다.

## 9. 다음 승인 단위

커밋 선행 조건이 충족되면 이미 승인된 범위 안에서 오소리 전투를 구현한다. 정사각형 표시 외 임의 디자인을 만들지 않고, 경고 연출은 사용자 prefab 슬롯으로 연결한다. 새 디자인이나 완성 아트는 만들지 않는다.

Warning feedback follow-up (2026-11-03): until user warning VFX exists, log warning-state entry once with Debug.Log and the enemy context. Do not log every frame or draw replacement markers/effects. This is part of the already authorized badger cycle after the prerequisite cleanup commit.
## 실제 커밋 시도 결과

2026-11-03: 지정한 경로를 git add로 stage하려 했으나 `.git/index.lock: Permission denied`로 실패했다. 실행 환경에서 Git 메타데이터에 쓰지 못했고 커밋을 생성하지 않았다. 기존 HEAD 090f845와 사용자 staged 픽셀 씬은 유지한다. 자동 승인 검토 거부 메시지가 아니라 Git이 반환한 파일 쓰기 권한 오류다. 보호 경로를 우회하지 않는다.

사용자는 로컬 Fork에서 Changes의 현재 프로젝트 변경을 Stage All한 뒤 `feat: integrate player animations and combat; remove debug gizmos`로 Commit할 수 있다. Push는 필요하지 않다. 선행 커밋을 확인한 뒤 오소리 구현을 재개한다. 경고는 상태 진입 시 Debug.Log 1회로 확인하고, 경고 VFX는 사용자 프리팹 연결 슬롯만 사용한다. 아직 오소리 경고 코드/프리팹 자체를 구현한 것은 아니다.