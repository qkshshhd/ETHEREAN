# 플레이어 대검: 공격 중에만 표시

플레이어는 평소 검을 넣고 다닌다는 새 연출 결정을 적용했다. 대검은 공격 준비부터 공격 후 회복 동작까지 표시되며, 다음 콤보 타격으로 바로 이어지면 계속 표시된다. 공격이 끝나거나 피격·점프 캔슬 등으로 중단되면 즉시 숨긴다. 입력이 무시되어 실제 공격이 시작되지 않으면 표시하지 않는다.

| 파일 | 변경 위치와 역할 |
| --- | --- |
| `Assets/_Project/Scripts/Runtime/Player/Combat/PlayerSwordMotion.cs` | `OnPhaseChanged`에서 공격이 없으면 `Hide`한다. `LateUpdate`는 유효한 공격 동작을 샘플링한 뒤 검을 표시한다. 공격 종료 후 보이는 복귀 트윈을 제거했다. 검이 숨은 상태에서 다음 공격의 시작 자세를 복원한다. 사망과 컴포넌트 비활성화에도 같은 숨김 처리를 적용한다. |
| `Assets/_Project/Prefabs/Weapons/Greatsword.prefab` | 검의 `SpriteRenderer`를 기본 비활성화해 씬 로드 시에도 검이 먼저 보이지 않게 했다. |
| `Assets/_Project/Scripts/Editor/PlayerSwordMotionBuilder.cs` | `SaveSwordPrefab`에서 같은 기본값을 지정해 에디터 메뉴로 다시 연결해도 유지한다. |
| `Docs/Reports/2026-11-06-sword-attack-only.md` | 새 작업 기록. |

공격 판정, 콤보 타이밍, 공격 중 검 궤적, 무기 해금 조건은 그대로다. 별도의 검집 아트는 추가하지 않았다.

Unity 6.4 프로젝트 참조로 런타임·편집기 C# 컴파일을 확인했다. Play에서는 무기를 해금한 플레이어로 대기·이동·일반 점프 중 검이 안 보이는지, 단타와 콤보 준비부터 검이 보이는지, 마지막 회복이 끝나거나 피격·점프 캔슬로 공격이 끊기면 숨는지 확인한다.
