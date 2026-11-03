# 현재 캐릭터 크기에 맞춘 플레이 기준 — 2026-11-03

## 1. 목표와 상태

사용자 요청: 현재 작은 캐릭터에 맞춰 점프·중력과 다른 플레이 수치를 조정하고, 앞으로의 개발도 현재 캐릭터 크기를 기준으로 한다. 현재 대표 가시 높이는 30px/32 PPU=0.9375u이다. 크기 자체를 임의 확대하지 않는다.

현재는 **Unity 새 코드 import 대기**다. 아래 소스 기본값·Editor 적용 도구·상시 기준 문서를 작성했고 별도 C# 컴파일 오류 0을 확인했다. 프리팹 및 공격 데이터의 저장된 수치 적용, 새 테스트 씬 생성, 실제 Play 검사·측정·임시 파일 정리는 완료 로그를 확인한 뒤 기록한다. 소스 기본값 변경만으로 기존 Inspector 값이 바뀌었다고 판단하지 않는다.

## 2. 수정한 파일

모든 경로의 기준은 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN`이다. 아래는 변경 위치와 역할이다.

| 파일 | 수정 위치와 역할 |
| --- | --- |
| `Assets/_Project/Scripts/Runtime/Player/Movement/PlayerMovement.cs` | horizontalMoveSpeed/acceleration/deceleration/riseGravity/fallGravity/maxFallSpeed 초기값. 실제 계산 로직·유일한 Rigidbody 속도 쓰기 구조는 유지. |
| `Assets/_Project/Scripts/Runtime/Player/Movement/PlayerJump.cs` | jumpSpeed 초기값. 코요테·버퍼·가변 높이 알고리즘과 시간 유지. |
| `Assets/_Project/Scripts/Runtime/Player/Movement/PlayerWallJump.cs` | wallJumpHorizontalSpeed/wallJumpVerticalSpeed 초기값. 벽 코요테·입력 보호·재접촉 정책 유지. |
| `Assets/_Project/Scripts/Runtime/Player/Movement/PlayerWallSlide.cs` | wallSlideSpeed 초기값. |
| `Assets/_Project/Scripts/Runtime/Player/Movement/PlayerGroundSensor.cs` | groundCheckDistance 초기값. |
| `Assets/_Project/Scripts/Runtime/Player/Movement/PlayerWallSensor.cs` | wallCheckDistance 초기값. 필터와 접촉 판정 알고리즘 유지. |
| `Assets/_Project/Scripts/Runtime/Player/PlayerCharacterAnimator.cs` | idleSpeedThreshold/runSpeedThreshold 초기값. 새 이동 속도에서도 Idle/Walk/Run 전환이 같은 상대 속도에서 일어나도록 맞춤. |
| `Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackData.cs` | knockback/hitboxOffset/hitboxSize 초기값. 실제 공격 시간·피해·처리 로직 유지. |
| `Assets/_Project/Scripts/Editor/PlayerCombatTestSceneBuilder.cs` | Configure의 hitboxOffset/hitboxSize. 과거 생성 메뉴가 거대한 공격 판정으로 되돌리지 않도록 새 비율 적용. |
| `AGENTS.md`, `PLAYER_SETUP.md` | Current character size is the development baseline 구역. 과거 1.8u 보존 규칙에 대한 이번 명시적 변경 승인과 앞으로의 크기 기준 기록. |

Editor 실행 후 수정할 저장 에셋:

- `Assets/_Project/Prefabs/Player/Player.prefab`: CapsuleCollider2D.size/offset, 위 이동·점프·벽·센서·표시 임계값, CharacterVisual.localPosition과 AttackEffectOrigin.localPosition. CharacterVisual 스케일은 1 유지.
- `Assets/_Project/Data/Player/GroundAttack.asset`, `GroundAttack2.asset`, `GroundAttack3.asset`, `AirAttack.asset`, `AirAttack2.asset`, `AirAttack3.asset`: hitboxOffset/hitboxSize/knockback만 비례 조정. 기존 GUID·피해·준비·타격·후딜·재정비·VFX 슬롯은 유지.

## 3. 추가한 파일

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerScaleReviewBuilder.cs`: Apply는 현재 저장된 1.8u 프리팹의 공간 단위 수치를 0.5208333배 하고 저장한다. 이미 0.9375u면 다시 곱하지 않는다. 알 수 없는 높이는 임의 변환하지 않고 중단한다. CreateReviewScene은 별도 새 씬의 지형·벽·대상 배치를 같은 비율로 맞춘다. 런타임 자동 스케일링 시스템은 아니다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Design/2026-11-03-current-character-scale.md`: 현재 기준·앞으로의 개발 원칙·수치 표를 보존한다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-current-character-scale.md`: 본 변경 기록.
- 생성 예정: `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scenes/Tests/PlayerCurrentScaleTest.unity` 및 meta. 기존 PlayerTimedComboTest의 복사본을 현재 크기에 맞춘 리뷰 씬으로 만든다. 플레이어 Transform 스케일이나 카메라 orthographicSize를 줄이는 방식은 아니다.

기존 파일 삭제·이름 변경은 없다. 새 임시 검사기 `Assets/_Project/Scripts/Editor/CurrentScaleValidation.cs`, `Assets/_Project/Scripts/Runtime/Player/CurrentScaleValidationDriver.cs` 및 request/ready/복사 씬은 실제 검사 후 제거할 예정이다.

## 4. 변경 전후와 실제 동작

기존에는 그림 약 0.94u 위에 높이 1.8u 충돌체가 있고, 점프 속도 16과 상승 중력 42를 사용했다. 새 기준에서는 충돌체 높이 0.9375, 폭 0.416667, 점프 속도 8.333334, 상승 중력 21.875, 하강 중력 25를 초기값으로 사용한다.

New Input System 입력 → 기존 PlayerLocomotion이 접지·벽·버퍼·우선순위 계산 → 각 Jump/WallJump/WallSlide가 새 수치로 속도 결정 → PlayerMovement가 유일하게 Rigidbody 속도 적용 → 표시 담당자가 새 속도 임계값으로 애니메이션 선택. 공격 입력은 기존 Windup/Active/Recovery를 유지하며 Active 중에만 축소된 OverlapBox로 대상별 한 번 판정한다.

## 5. 설계 이유와 트레이드오프

길이·속도·가속도·중력을 모두 s=0.9375/1.8배 해 캐릭터 높이 대비 이동 거리와 시간 리듬을 보존하는 출발점으로 삼았다. 점프 최고점 v²/(2g)는 s배, 최고점까지 v/g는 동일하다. 중력을 단독으로 낮춰 점프가 더 높고 오래 떠 있는 결과를 피한다.

물리 콜라이더 폭은 기존 비율을 유지하므로 그림의 머리카락/망토 전체보다 좁다. 최종 충돌체 형태나 점프 감각은 사용자 리뷰 대상이다. 이는 거대한 기존 플레이어 기준을 제거하는 일괄 초기 조정이며 모든 값을 최종 확정하는 결정은 아니다.

기존 저장 씬과 사용자 override는 보존하고 새로운 크기용 씬을 추가한다. 기존 PlayerMovementTest의 wallJumpHorizontalSpeed=4 override는 과거 값으로 남는다. 신규 크기 테스트에는 PlayerCurrentScaleTest를 사용한다. 카메라·화면 UI·PPU·원본 PNG·시간 값은 일괄 축소하지 않는다.

## 6. 연결과 직접 확인

Unity가 코드 import를 마치면 준비된 검증 요청이 실행된다. 완료 후 `Assets/_Project/Scenes/Tests/PlayerCurrentScaleTest.unity`를 열어 A/D 이동, Space 짧게/길게 점프, 오른쪽 벽 접촉 후 벽차기, Enter/마우스 왼쪽 공격을 확인한다. 테스트 씬은 무기/벽 능력을 열지만 기본 프리팹은 잠겨 있어야 한다.

Player의 CapsuleCollider2D.height=0.9375, PlayerMovement의 이동 3.125·중력 21.875/25, PlayerJump.jumpSpeed=8.333334를 Inspector에서 확인한다. 캐릭터 스프라이트 스케일은 1이고 발 위치가 콜라이더 하단과 맞아야 한다.

## 7. 검증과 임시값

완료: Git 상태·기존 프리팹/씬/코드·Unity/패키지/InputSystem 구조를 확인했고 시작 파일 해시를 기록했다. 준비된 코드와 임시 검사기 별도 C# 컴파일 오류 0. 이는 Unity 실행 검증이 아니다.

대기: 실제 Editor 저장·재적용 멱등성·씬 연결·접지·가속/감속·점프 실측·가변 점프·벽차기/코요테·콤보 판정과 후딜 검사. 미실행 검사를 통과로 세지 않는다. 새 수치는 모두 조작감 리뷰 전 초기값이다.

## 8. 피드백과 제한

현재 캐릭터를 더 크게 만드는 작업은 포함하지 않았다. 점프/공격 전용 그림은 여전히 없으며 기존 중립 프레임 대체를 유지한다. 최종 적·레벨·VFX 제작은 별도 작업이며 이번에는 앞으로의 크기 원칙을 기록한 것이다.

## 9. 다음 단계

새 코드 import 후 실제 검증·정리·보고를 끝내고 사용자의 조작감 리뷰를 기다린다. 다음 기능, 커밋, push는 시작하지 않는다.

## 2026-11-03 최신 상태 보정

실제 일괄 크기 변환은 사용자 수정 콜라이더를 감지한 안전 가드에서 중단되었다. 위 비례 조정 표를 적용 완료 값으로 해석하지 않는다. 현재 충돌체는 size=(0.4948379,0.88451296), offset=(-0.003912464,-0.4577435)이며 보존한다. 후속 사용자 피드백에 따라 점프 10, 상승 중력 40, 하강 중력 65, 낙하 상한 16, 벽차기 수직 10으로 조정하는 별도 단위를 진행한다. 실제 적용/검증 상태는 Docs/Reports/2026-11-03-weighted-jump-and-test-map.md가 우선한다. 현재 캐릭터 크기를 기준으로 앞으로 개발한다는 원칙은 유지한다.