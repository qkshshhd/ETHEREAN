# 좌우 이동 검토 단위 결과 — 2026-10-22

## 목표 및 결과

좌우 이동 테스트 씬, 플레이어 프리팹, 키보드 수평 입력 보정을 완료했다. Unity 6000.4.7f1 에디터에서 에셋 생성과 Play Mode 검사 25개를 통과했다. 다음 기능은 구현하지 않았으며 사용자 검토를 기다린다.

## 추가·수정 파일

프로젝트 루트는 `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN`이다. 아래 경로는 이 루트를 기준으로 한다. 에셋의 대응 `.meta`를 포함하며 기존 GUID는 보존했다.

| 구분 | 경로 | 내용 |
| --- | --- | --- |
| 수정 | `Assets/InputSystem_Actions.inputactions` | 기존 Player/Move 키보드 Composite만 비정규화 Digital 모드로 변경 |
| 추가 | `Assets/_Project/Scenes/Tests/PlayerMovementTest.unity` | 플레이어 프리팹 인스턴스, 평지, 양쪽 벽, 1u 눈금, 고정 카메라 |
| 추가 | `Assets/_Project/Prefabs/Player/Player.prefab` | 입력·이동·물리·충돌체·표시 연결 |
| 추가 | `Assets/_Project/Data/Player/PlayerMoveAction.asset` | 기존 Move 액션의 영속 참조 |
| 추가 | `Assets/_Project/Data/Player/PlayerMovement.physicsMaterial2D` | 마찰 0, 반발력 0 |
| 추가 | `Assets/_Project/Art/Sprites/MovementSquare.png` | 직접 생성한 32×32 플레이스홀더 |
| 추가 | `Assets/_Project/Art/Materials/MovementPlaceholder.mat` | URP 2D Unlit 표시 |
| 추가 | `Assets/_Project/Scripts/Editor/PlayerMovementTestSceneBuilder.cs` | Unity Editor API 기반 에셋 구성 및 참조 검사 |
| Unity 자동 생성 | `ProjectSettings/SceneTemplateSettings.json` | 씬 생성 과정에서 에디터가 기록한 기본 씬 템플릿 설정 |
| 추가 | `AGENTS.md` | 이후 작업의 협업 규칙 및 현재 검토 지점 |
| 추가 | `Docs/References/ETHEREAN_Codex_Collaboration_Prompt.md` | 사용자 첨부 프롬프트 전문 |
| 추가 | `Docs/References/ETHEREAN_GDD.md` | 첨부 기획서 34쪽의 전체 추출 텍스트. 그림·레이아웃 제외 |
| 수정 | `PLAYER_SETUP.md` | 실행, 연결, 코드 동작, 검증, 임시값 안내 |
| 추가 | 이 보고서 | 검증 근거와 한계 기록 |

기존 `PlayerMovementInput`, `NewInputMovementReader`, `PlayerMovement` 런타임 코드는 수정하지 않았다. 이 세 파일은 작업 시작 전부터 Git 미추적 상태였으며 원본을 보호했다. 커밋·푸시·패키지 업그레이드는 하지 않았다.

## 실행 흐름과 설계

키 입력 → 기존 Move 액션의 복제본 → NewInputMovementReader.HorizontalInput → PlayerMovement.FixedUpdate → Rigidbody2D.linearVelocity.x 순서로 처리한다. Y는 그대로 두어 중력과 낙하를 유지한다.

입력 읽기와 실제 물리 이동을 분리하고, 이동은 작은 추상 입력 계약에 의존한다. 실제 속도 적용 주체는 PlayerMovement 하나다. 추가 런타임 클래스나 상태 머신은 만들지 않았다.

Move의 기존 Value/Vector2 형식과 액션 ID를 유지하고 키보드 Composite만 `2DVector(mode=1)`로 바꿨다. X가 대각선 정규화에 의해 약 0.707로 줄어들지 않게 한다. 게임패드 바인딩은 유지했지만 실제 기기로는 검증하지 않았다.

복제 액션은 원본 액션 맵의 Enable/Disable과 독립적이다. 이후 컷신·일시정지에서 Reader를 차단하거나 별도 입력 정책을 연결해야 한다. 이는 이번 범위의 구현 항목은 아니다.

## 실행한 검증

1. 기존 Input Actions ID, 중복 GUID, 에셋 참조와 프리팹 설정을 정적으로 확인했다.
2. 로컬 Unity 참조 어셈블리와 Roslyn `csc.dll`을 사용해 C# 코드를 별도 컴파일했다. 컴파일 오류는 없었다. 에디터 외부 컴파일에서는 Inspector가 할당하는 private 필드에 대한 CS0649 경고가 발생했다.
3. 열린 Unity 에디터에서 임시 검증 코드가 `PlayerMovementTestSceneBuilder.RebuildForValidation()`을 호출했다. EditorSceneManager, AssetDatabase, PrefabUtility, SerializedObject를 사용해 씬과 프리팹을 저장했다.
4. 편집 상태에서 프리팹 인스턴스 연결을 검사한 뒤 Play Mode에 진입했다. New Input System의 임시 Keyboard에 상태 이벤트를 주입하고, 실제 FixedUpdate와 Physics2D의 결과를 검사했다.
5. 1280×720 카메라 렌더를 확인했다. 청록색 플레이어, 바닥, 벽, 눈금이 정상 표시됐다.
6. 검증 종료 후 Play Mode에서 나왔고 임시 Keyboard, 자동 검증 코드와 요청 파일을 제거했다. 검증 로그는 `Logs/PlayerMovementValidation.txt`, 내부 확인용 렌더는 `Logs/PlayerMovementPreview.png`에 남았다. Logs는 기존 Git 제외 대상이다.

## Play Mode 검사 결과

| 번호 | 검사 | 결과 |
| --- | --- | --- |
| 1 | Player 존재 | 통과 |
| 2 | 입력·이동·Rigidbody2D·CapsuleCollider2D 구성 | 통과 |
| 3 | Move 액션 참조 연결 | 통과 |
| 4 | Movement의 입력 소스 연결 | 통과 |
| 5 | 누락된 플레이어 스크립트 없음 | 통과 |
| 6 | 바닥 착지 | 통과 |
| 7 | D 수평 속도 +6u/s | 통과 |
| 8 | A 수평 속도 -6u/s | 통과 |
| 9 | 오른쪽 방향키 +6u/s | 통과 |
| 10 | 왼쪽 방향키 -6u/s | 통과 |
| 11 | W+D 수평 속도 +6u/s | 통과 |
| 12 | S+D 수평 속도 +6u/s | 통과 |
| 13 | 위+오른쪽 방향키 +6u/s | 통과 |
| 14 | 아래+왼쪽 방향키 -6u/s | 통과 |
| 15 | 좌우 동시 입력 중립 | 통과 |
| 16 | 키 해제 후 정지 | 통과 |
| 17 | 실제 거리/시간 측정 | 4.800u / 0.800s = 6.000u/s |
| 18 | Reader 비활성화 시 정지 | 통과 |
| 19 | Reader 재활성화 시 누르고 있던 입력 복구 | 통과 |
| 20 | Movement 비활성화 시 X만 초기화, Y 보존 | 통과 |
| 21 | Movement 비활성화 중 중력 유지 | 통과 |
| 22 | 공중 수평 이동과 낙하 병행 | 통과 |
| 23 | 오른쪽 벽 통과 방지 | 통과 |
| 24 | 왼쪽 벽 통과 방지 | 통과 |
| 25 | 회전 고정 | 통과 |

수치 6u/s는 이번 테스트의 설정값과 측정 결과이며 최종 밸런스 확정이나 성능 최적화 효과가 아니다.

## 검증 중 수정한 문제

초기 생성 도구에서 참조 에셋을 읽은 뒤 새 씬을 열어, 아직 연결되지 않은 입력 참조를 잃는 문제가 있었다. 씬을 먼저 열고 참조를 읽도록 순서를 바꿨다. 저장 전 영속 참조와 저장 후 프리팹 참조 검사도 추가했다. 수정 후 Move 참조와 입력 동작이 모두 통과했다.

프리팹 연결 여부는 에디터 상태에서 확인한다. Play Mode의 런타임 객체에 동일한 에디터 메타데이터 검사를 적용하지 않도록 검증 위치를 바로잡았다.

## 직접 확인

`Assets/_Project/Scenes/Tests/PlayerMovementTest.unity`를 열고 Game 뷰를 16:9로 설정한다. Play 후 Game 뷰를 클릭하고 A/D, 방향키, W+D, S+D를 비교한다. 양쪽 벽으로 이동하고 키를 놓아 정지 여부를 확인한다. Inspector에서 Reader를 비활성화·재활성화해 입력 복구를 확인한다.

## 임시값·한계·다음 검토

플레이어 0.8×1.8u, 속도 6u/s, 중력 배율 1, 카메라 크기 5.5, 테스트 지형 크기는 임시값이다. 실제 게임패드, 실제 키보드의 조작감, 입력 참조 누락에 대한 수동 재현은 별도 확인 항목이다. 자동 검증은 주입한 키보드 상태에 대한 결과다.

점프·가속/감속·방향 반전·애니메이션·전투·카메라 추적은 아직 없다. 현재의 즉시 이동·정지 감각과 임시 크기부터 사용자가 검토한다. 다음 추천 단위는 바닥 감지와 단발 점프이며, 사용자 지시 전에는 시작하지 않는다.
