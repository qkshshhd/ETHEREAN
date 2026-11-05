# 캐릭터 겹침 깜빡임 — 고정 Sorting Layer

## 1. 요청과 조사 결과

플레이어/오소리가 겹칠 때 앞뒤가 깜빡이는 현상을 수정한다. 저장된 두 활성 CharacterVisual SpriteRenderer가 모두Default/Order0이고 같은Z 평면을 사용했다.7개 클립의 바인딩은 m_Sprite만 있었고 정렬 값·활성화·색상·Transform을 애니메이션으로 변경하지 않았다. 두 캐릭터는 같은 재질을 쓰고 _ZWrite=0이었다. 따라서 스프라이트마다 정렬 키가 따로 바뀌는 구조를 찾은 것이 아니라, 명시적 캐릭터 앞뒤 우선순위가 없는 동순위 구성을 교정했다.

현재 오브젝트는Default(지형/배경) < Enemies < Player 순서로 고정했다. 수정 전 모든 장면의 원인을 하나로 증명했다고 주장하지 않으며, 수정 후 실제 게임 재생과 프레임별 렌더를 검증했다.

## 2. 수정한 파일·위치

| 전체 경로 | 위치와 역할 |
| --- | --- |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/ProjectSettings/TagManager.asset | m_SortingLayers에Enemies(ID1542101),Player(ID1542102)를Default 뒤에 순서대로 추가. 기존 물리Layer8/9와 다른 항목. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab | CharacterVisual.SpriteRenderer의sortingLayerID0→1542102,캐시sortingLayer0→2. Order0 유지. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Enemies/Badger.prefab | CharacterVisual.SpriteRenderer의sortingLayerID0→1542101,캐시sortingLayer0→1. Order0 유지. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerCharacterAnimationBuilder.cs | Build에서 옛 비활성Visual의 정렬 값을 복사하는 두 줄 제거,SpriteSortingSetup.ConfigurePlayer 호출. 연결 도구를 다시 실행해도Default로 돌아가지 않음. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/BadgerAnimationBuilder.cs | Build의 CharacterVisual 연결에서ConfigureEnemy 호출. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md | 최신 캐릭터 정렬 정책·방법·검증·제한 기록. |
| C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md | 같은 정책과Inspector/메뉴 확인 절차 기록. |

TagManager는 Unity 저장 과정에서 네이티브 직렬화2→3으로 갱신됐고 기본m_RenderingLayers와m_MigratedRenderPipelines 필드가 저장됐다. 엔진·패키지 업그레이드가 아니다. 기존 물리 레이어 이름/번호와 충돌 행렬은 그대로다.

## 3. 추가 파일·연결

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/SpriteSortingSetup.cs 및 .meta: Editor 전용 정렬 연결 도구. ConfigurePlayer/ConfigureEnemy가 공통 정책을 적용한다. EnsureLayer는 기존 이름/ID를 재사용하거나 빈ID를 찾아 새 레이어를 만들고, 생성 순서는 항상Enemies→Player로 맞춘다. 두 기존 애니메이션 빌더가 호출한다.
- 메뉴 ETHEREAN→Rendering→Apply Character Sorting: Apply가 두 기존 프리팹과 열린 씬의 관련 CharacterVisual을 갱신한다. 변경한 씬 인스턴스에는Undo/Prefab 변경 기록과dirty만 남기고 다른 편집을 함께 강제 저장하지 않는다. 개별 씬 인스턴스의 기존Order in Layer는 보존한다.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-04-sprite-sorting.md: 이 변경 보고서.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-04-sprite-sorting-validation.txt: 실제Editor/Play/픽셀 비교 로그.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-04-sprite-sorting-overlap.png: 실제두캐릭터 겹침 렌더.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-04-sprite-sorting-player-reference.png: 같은 위치의 플레이어 단독 렌더 기준.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-04-sprite-sorting-enemy-reference.png: 같은 위치의 적 단독 렌더 기준.

영구 Runtime 스크립트 추가 없음. 임시 검증Editor/Runtime 스크립트2개와 .meta,request/ready,복사 씬은 검사 후 제거했다. 기존 영구 파일 삭제·이름 변경 없음. 원화·클립·VFX를 새로 디자인하지 않았다.

## 4. 실행 원리

Unity SpriteRenderer는 먼저Sorting Layer의 순서를 비교하고 같은 레이어 안에서Order in Layer를 비교한다. 이제Enemy와Player가 다른 정렬 레이어이므로 두 Order가 모두0이어도Player가 항상 앞이다. 애니메이션이Sprite만 바꾸므로 이 렌더러 설정은 다음 프레임에도 유지된다.

편집 시 정렬 연결 도구가TagManager와프리팹 값을 저장한다. 런타임에는 원래 Animator/표시 어댑터가 스프라이트를 재생하고 Unity 렌더러가 저장된 우선순위를 사용한다. 프레임마다 정렬 값을 계산하거나Z/위치를 흔드는 코드를 추가하지 않았다. 기존 이동·피격·공격 판정과 렌더링 책임을 섞지 않는다.

## 5. 선택 이유·트레이드오프

현재 횡스크롤 게임에서는 겹칠 때도 플레이어를 식별할 수 있게 고정된 전후 관계를 선택했다. 점프 중Y나Sprite 중심 변화로 앞뒤가 바뀌는 방식을 쓰지 않는다. 이것은 공격 범위나물리 충돌을 바꾸는 설정이 아니다. Sorting Layer(그리기 순서)와GameObject Layer(물리/조회)는 이름이 비슷해도 별개다.

같은Enemies 레이어의 여러 적 사이 세부 순서는 개별Order in Layer로 조절한다. 이번 프리팹 기본값은0이며 모든 미래 적 복제에 자동으로 고유 순번을 할당하는 시스템은 추가하지 않았다. 현재 플레이어–오소리 우선순위는 분리돼 있다. 다중 부위 캐릭터/외부 무기/VFX 같은 추후 리소스의SortingGroup 설계는 별도 검토 대상이다. 기본Default의모든 배경·지형 내부 순서를 새로 설계한 것도 아니다.

## 6. 직접 확인

1. Tags and Layers의Sorting Layers 순서가Default→Enemies→Player인지 확인한다. 물리Layers8/9 항목과 구분한다.
2. Player.prefab/Badger.prefab의CharacterVisual.SpriteRenderer에서Player/Enemies와Order0을 확인한다.
3. PlayerBadgerCombatTest에서 몸체를 겹쳐좌우 이동·정지·공격한다. 플레이어가 적 앞에 유지돼야 한다.
4. 별도 씬 인스턴스에 옛 정렬override가 있으면ETHEREAN→Rendering→Apply Character Sorting을 실행한다. 관련 인스턴스가dirty가 되면 변경 내용을 확인한 후 저장한다.
5. 캐릭터 이미지 연결 도구를 다시 실행해도 정렬 정책을 적용한다. 단, 직접Sorting Layers 순서를 뒤집거나다른 재질Render Queue를 사용하면 정책이 달라질 수 있다.

## 7. 실제 검증

Editor12개: 프리팹Sorting Layer,순서값Default<Enemies<Player,Order0,옛 플레이스홀더 숨김,재질깊이기록없음,7개클립이Sprite만 애니메이션함을 확인.

Play11개: 실제 이동에 따라플레이어 프레임진행,이동 중정렬유지,실제오소리 공격단계정렬유지. 이어 실제Player Idle/Walk/Run과Badger Idle/Walk/Attack/Death의키프레임을Unity Animator에서 통제샘플링해6쌍×16프레임=96조합을 렌더했다. 이 통제 샘플링은 실제 공격 입력 검사로 표현하지 않는다.

매조합에서Player 단독/Enemy 단독/겹침을 각각 카메라로 렌더하고 양쪽 모두 불투명한 영역을 비교했다.96조합 모두 겹침 영역이 있었고 총119,764픽셀에서 오차0이었다(RGB허용차1/255). 즉 해당샘플에서앞에보이는픽셀이Player단독기준과 일치했다. 실제 겹침 캡처도 열어육안검토했다. 고정화면/현재재질/현재7클립에대한 증거이지 모든 가능한사용자상황에대한 보증은 아니다.

최종총23개(12 Editor+11 Play) 통과. 임시 도구 정리 후 영구Runtime31/Editor9=40개 소스를 분리 컴파일해양쪽exit0. 원래씬 구성/dirty상태 비교RESTORED는요청된정렬적용이후를기준으로했다. 열려 있던 씬에 관련필드를수정했다면dirty가남을수있고다른미저장편집을함께저장하지않았다.

## 8. 보존·한계

작업시작SHA256비교상기존Assets/Packages/ProjectSettings중변경은2프리팹,2기존Editor빌더,TagManager뿐이다. 프리팹텍스트차이는활성CharacterVisual의SortingLayerID/캐시값뿐이다. 그림/클립/재질/물리설정/저장씬/Input Actions/이동·전투데이터와기존GUID는그대로다. 새파일은3절에구분했다. Gizmos/Handles/Debug.Draw구현0건이며성능수치는측정하지않았다.

## 9. 리뷰

플레이어–오소리겹침의표시일관성을직접검토한다. 이결과로오소리전투전체를완료로단정하지않는다. 다음기능·커밋·푸시는진행하지않았다.
