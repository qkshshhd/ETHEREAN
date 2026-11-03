# 좌우 가속·감속 제거 — 2026-11-03

## 1. 목표와 상태

최신 사용자 결정은 가속과 감속을 낮추는 것이 아니라 제거하는 것이다. 좌우 입력을 받으면 목표 속도로 바로 이동하고 놓으면 바로 멈추며 반대 입력이면 바로 방향을 바꾼다. 앞선 18/24 튜닝 제안보다 이번 결정이 우선한다.

**구현·Unity 저장·실행 검증·임시 정리 완료, 사용자 리뷰 대기.** 최종 29개 검사(13 Editor + 16 실제 Play Mode)가 통과했고, 임시 검사기를 제거한 최종 28개 C# 소스도 별도 컴파일 오류 0을 확인했다.

## 2. 수정한 파일과 위치

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerMovement.cs`: acceleration/deceleration 직렬화 필드 제거. CalculateHorizontalSpeed에서 currentSpeed/deltaTime 인자와 MoveTowards·반전 감속 단계를 제거하고 `Clamp(input) × horizontalMoveSpeed × speedMultiplier`를 반환한다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerLocomotion.cs`: CalculateVelocity에서 위 메서드의 새 인자를 사용한다. 벽차기 제어 보호 중에는 수평 재계산을 건너뛰는 기존 우선순위는 유지한다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerMovementFeelSceneBuilder.cs`: 이전 ApplySoftMovementTuning과 메뉴를 제거하고 SaveDirectMovementPrefab으로 대체한다. 없어진 필드를 다시 설정하지 않고 프리팹을 Editor API로 재저장해 오래된 직렬화 필드를 정리한다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerScaleReviewBuilder.cs`: Apply에서 제거된 acceleration/deceleration 프로퍼티를 찾던 코드를 제거한다. 사용자 충돌체 보호 가드는 유지한다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab`: PlayerMovement의 오래된 acceleration/deceleration 직렬화 항목을 실제 Editor 재저장으로 제거했다. 최고 속도·중력·충돌체·참조·GUID는 유지한다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md`, `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md`: 최신 이동 정책·동작 차이·검증·리뷰 상태를 저장한다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-soft-movement-and-wall-jump.md`: 앞선 가감속 완화 제안을 이번 결정이 대체함을 기록한다.

## 3. 추가·삭제 파일

영구 추가는 본 보고서 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-direct-horizontal-movement.md`뿐이다. 영구 스크립트·씬·프리팹을 새로 만들거나 기존 GUID를 바꾸지 않는다.

앞선 임시 SoftMovementValidation.cs/SoftMovementValidationDriver.cs와 meta/request/ready는 정리했다. 이번 임시 DirectMovementValidation.cs/DirectMovementValidationDriver.cs 및 meta/request/ready/복사 씬/meta도 최종 검사 후 제거했다. 이들은 게임의 영구 구조가 아니다.

## 4. 변경 전후와 실행 순서

기존: 입력 → 목표 속도 계산 → 가속/감속에 따라 속도 접근 → 반대 입력 시 0을 거쳐 반전.

변경: New Input System 입력 → PlayerLocomotion → PlayerMovement.CalculateHorizontalSpeed가 목표 속도를 즉시 반환 → PlayerMovement가 유일하게 Rigidbody 속도 적용. 키보드 D/A는 현재 프리팹 기준 +6/-6u/s, 무입력은 0이다. 방향을 바꿀 때 이전 속도를 거치지 않는다. 물리 업데이트는 기존 0.02초 간격이므로 입력과 화면이 임의 시점에 동기 처리된다는 의미는 아니다.

낮은 아날로그 입력은 입력 크기에 비례하는 낮은 속도를 즉시 사용한다. 지상과 공중의 일반 좌우 제어에 같은 규칙이 적용되며 공중에서 입력을 놓으면 X만 멈춘다. 점프·중력으로 진행 중인 Y 속도는 유지한다.

## 5. 유지한 정책과 설계 이유

벽차기 수평 4.5/수직 9, 0.1초 수평 보호·벽 코요테, 일반 점프 10, 중력 40/65, 낙하 상한 16은 유지한다. 벽차기 직후에는 강제 발사를 먼저 보존하고 보호가 끝나면 즉시 입력 방향 속도를 적용한다. 이 보호까지 제거하면 벽 방향 입력이 발사를 즉시 지워 기존 벽차기 정책을 깨뜨리므로 유지한다.

공격 중에는 기존 MoveMultiplier가 목표 속도를 제한한다. 공격 Recovery 후반에 허용 이동 비율이 증가하는 기존 설계는 일반 이동의 가감속과 구별하며 유지한다. 공격 판정·피해·재정비·입력 예약은 변경하지 않는다.

큰 가속값으로 흉내 내지 않고 기능과 Inspector 필드 자체를 제거해 현재 정책을 명확히 한다. 별도 이동 모드 인터페이스나 여러 속도 쓰기 컴포넌트는 추가하지 않는다.

## 6. 애니메이션·직접 확인

PlayerMovementFeelTest 또는 기존 전투 씬에서 D → A → 키 해제를 각각 눌러 즉시 출발·반전·정지하는지 확인한다. Space+D로 점프한 뒤 A·무입력을 적용해 공중 X 제어를 확인한다. 오른쪽 통로에서 벽차기 직후에는 밀려나는 방향이 잠깐 보존되고 이후 방향 입력이 바로 적용돼야 한다.

키보드는 가속 구간이 없어 곧바로 Run으로 전환한다. Walk는 낮은 아날로그 입력에서 사용할 수 있다. 새 걷기 전용 키를 추가하지 않는다. Animator 상태·기존 clip·PPU는 변경하지 않는다. 최고 이동 속도는 PlayerMovement의 Horizontal Move Speed에서 조정한다. Acceleration/Deceleration 항목은 사라진다.

## 7. 검사

**29개 검사 통과: 13 Editor + 16 실제 Play Mode.**

Editor는 Inspector에서 두 필드가 사라지고 프리팹 텍스트에서도 제거됐는지, 최고 속도/벽차기/점프/중력/보호 시간/사용자 충돌체가 유지되는지, 기존 전투 씬과 이동 리뷰 씬의 플레이어 연결을 확인했다.

Play 검사는 실제 Input System 가상 키보드/게임패드와 Physics2D를 사용했다. 첫 입력 물리 틱 +6, 반대 입력 다음 틱 -6, 해제 다음 틱 0을 확인했다. 입력 컴포넌트 disable/reenable, 아날로그 입력 비례 속도, 지상 점프, 공중 X 반전/정지와 Y 지속, 벽차기 발사/보호/보호 종료 후 즉시 제어 복원, 공격 중 속도 제한/정지/취소 후 복원, 새 맵 2u 틈 통과도 확인했다.

검사 전 열린 씬 목록·활성 씬·dirty 상태를 비교해 RESTORED를 확인했다. 임시 소스/meta/request/ready/복사 씬/meta를 제거한 최종 28개 소스도 별도 컴파일 통과, 오류 0. 이 별도 컴파일은 Play 검사 수에 포함하지 않는다. 추가 씬 진입 중 AudioListener 중복 경고가 있었으며 다른 씬은 검사 중에만 비활성화하고 종료 후 복원했다.

시작 Assets/Packages/ProjectSettings 파일 해시 비교에서 직접 변경된 기존 파일은 위 프리팹과 네 C# 소스뿐이다. 제거된 기존 파일은 이전 임시 SoftMovement 검사기/meta 네 개뿐이며 다른 기존 파일은 동일하다. 기존 씬·공격 데이터·Input Actions·설정·패키지·GUID와 사용자 staged 픽셀 씬의 45줄 추가 상태를 유지했다. 과거 가감속 검사의 시간 수치는 이번 정책의 결과가 아니다. 물리 장치 조작감과 전체 점프/벽 코요테/버퍼/전투 회귀를 새로 모두 실행한 것으로 표현하지 않는다.

## 8. 제한과 임시값

물리 장치의 조작감과 전체 레벨 난이도는 사용자 리뷰 대상이다. 새 벽차기 값 4.5/9는 앞선 요청으로 적용된 임시 튜닝이며 이번 제거 작업에서 재조정하지 않는다. 입력 비활성화도 일반 수평 목표가 0이 되므로 다음 물리 틱에 멈춘다. 유효한 입력을 유지한 벽차기 보호 중에는 강제 발사 정책이 우선한다. 입력/이동 비활성화에 의한 기존 제어 취소 정책은 유지한다.

## 9. 리뷰

즉시 정지·반전이 게임에 맞는지 직접 확인한다. 사용자 검토 전 다음 기능·커밋·push는 진행하지 않는다.

## 최종 Unity 검사 로그

```text
Character animation validation 2026-11-03T19:41:00.0627941+09:00
EDITOR PASS Acceleration fields removed from Inspector
EDITOR PASS Unused fields removed from saved prefab
EDITOR PASS Maximum speed remains 6
EDITOR PASS Saved softer wall launch
EDITOR PASS Wall lock and coyote times preserved
EDITOR PASS Weighted ground jump preserved
EDITOR PASS User collider preserved
EDITOR PASS Assets/_Project/Scenes/Tests/PlayerCombatTest.unity has direct movement fields
EDITOR PASS Assets/_Project/Scenes/Tests/PlayerCombatTest.unity retains maximum speed
EDITOR PASS Assets/_Project/Scenes/Tests/PlayerCombatTest.unity player reference exists
EDITOR PASS Assets/_Project/Scenes/Tests/PlayerMovementFeelTest.unity has direct movement fields
EDITOR PASS Assets/_Project/Scenes/Tests/PlayerMovementFeelTest.unity retains maximum speed
EDITOR PASS Assets/_Project/Scenes/Tests/PlayerMovementFeelTest.unity player reference exists
SUCCESS EDITOR 13
PLAY PASS Full speed on first right-input physics tick
PLAY PASS Opposite input reverses on next physics tick
PLAY PASS Release stops on next physics tick
PLAY PASS Disabled input stops horizontal motion immediately
PLAY PASS Reenabled input restores immediate movement
PLAY PASS Analog input directly scales speed without inertia
PLAY PASS Ground jump preserved
PLAY PASS Air direction changes immediately
PLAY PASS Air release stops only horizontal movement
PLAY PASS Softer wall-jump launch preserved
PLAY PASS Wall launch protection survives direct input movement
PLAY PASS Full directional control returns after wall protection
PLAY PASS Attack movement restriction still applies immediately
PLAY PASS Release stops even during attack
PLAY PASS Attack cancellation restores direct normal speed
PLAY PASS Review two-unit gap remains traversable
SUCCESS PLAY 16
RESTORED
```
