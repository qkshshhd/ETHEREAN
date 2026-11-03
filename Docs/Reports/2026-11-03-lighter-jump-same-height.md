# 높이를 유지한 점프 무게감 완화 — 2026-11-03

## 1. 목표와 상태

사용자 요청: 무게감을 조금 줄이되 점프 높이는 바꾸지 않는다. 일반 최대 점프와 벽차기 최고점을 기준으로 중력과 수직 시작 속도를 함께 보정하고, 대표적인 짧은 탭 높이도 맞춘다. 좌우 즉시 제어 정책은 유지한다.

**구현·실제 Unity 저장·실측·임시 정리 완료, 사용자 조작감 리뷰 대기.** 최종 22개 검사(9 Editor + 13 실제 Play Mode)가 통과했다. 임시 검사기 제거 후 최종 28개 C# 소스도 별도 컴파일 오류 0을 확인했다.

## 2. 조정 값

| 필드 | 이전 실제 저장값 | 새 초기값 |
| --- | ---: | ---: |
| PlayerMovement.riseGravity | 40 | 32 |
| PlayerMovement.fallGravity | 65 | 52 |
| PlayerJump.jumpSpeed | 10 | 8.986667 |
| PlayerWallJump.wallJumpVerticalSpeed | 9 | 8.086154 |
| PlayerJump.jumpCutMultiplier | 0.5 | 0.51080966 |

상승/하강 중력을 각각 20% 낮춰 더 완만한 궤적으로 만든다. 중력만 줄이면 높아지므로 발사 속도도 줄여 최고점을 맞춘다. 최대 낙하 속도 16, 수평 속도 6, 벽차기 수평 4.5, 보호·코요테·버퍼 시간은 유지한다.

## 3. 수정한 파일과 위치

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerMovement.cs`: riseGravity/fallGravity 초기값. 물리 적용·즉시 수평 이동 로직은 유지.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerJump.cs`: jumpSpeed/jumpCutMultiplier 초기값. 단발·버퍼·컷 처리 로직은 유지.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerWallJump.cs`: wallJumpVerticalSpeed 초기값. 수평 발사·보호·벽 이력 처리 로직은 유지.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerMovementFeelSceneBuilder.cs`: ApplyTuning을 새 수치로 갱신하고 메뉴를 Apply Balanced Jump Timing으로 표시한다. 이전 메뉴를 사용해 옛 중력으로 되돌리지 않도록 한다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab`: 위 다섯 Inspector 항목을 실제 Editor API로 저장했다. 크기·원화·최고 속도·콜라이더·입력 참조는 유지.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md`, `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md`: 최신 튜닝·높이 조건·검증·리뷰 상태 기록.

## 4. 추가·제거 파일

영구 추가는 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-lighter-jump-same-height.md`뿐이다. 새 런타임 프레임워크나 스탯 시스템은 만들지 않는다. 기존 영구 파일 삭제·이름 변경은 없다.

LighterJumpValidation.cs/LighterJumpValidationDriver.cs와 meta/request/ready/검사 복사 씬은 이번 단위 전용 임시 파일이며 최종 검증 후 모두 제거했다.

## 5. 실행 원리와 높이 보정

입력 → 기존 PlayerLocomotion → PlayerJump/PlayerWallJump가 보정된 시작 속도 생성 → PlayerMovement가 낮춘 중력을 적용하고 유일하게 Rigidbody 속도를 쓴다. 점프 시작·최고점·하강·착지의 기존 상태 흐름은 유지한다.

단순 연속식에서는 높이 `v²/(2g)`를 같게 하려면 v를 중력 비율의 제곱근만큼 줄이면 된다. 그러나 현재 구현은 발사 틱에 중력을 적용하지 않고 이후 고정 틱에서 속도를 줄인다. 단순 제곱근 비례만 쓰면 실제 최고점이 조금 달라진다.

현재 Fixed Timestep=0.02s에서 상승 구간의 변위 합 `dt × [n×v - g×dt×n×(n-1)/2]`를 기준으로 보정했다. 일반 최대 점프의 이전 실제 상승량 1.352u와 벽차기 상승량 1.104u를 목표로 한다. 짧은 탭은 한 틱 발사 후 해제하는 조건의 높이 0.464u에 맞춰 컷 비율을 소폭 보정했다. 실제 물리 검증과 비교해야 한다.

## 6. 트레이드오프와 범위

최고점은 유지하지만 올라가고 내려오는 시간이 조금 길어진다. 같은 수평 속도를 더 오래 유지할 수 있어 공중 수평 도달 거리는 늘 수 있다. 일반 최대·벽차기 최고점·대표적인 한 틱 탭 높이를 맞추는 조정이며, 모든 가능한 중간 입력 길이에서 기존 높이 곡선이 정확히 동일하다는 의미는 아니다.

높이 자동 잠금 시스템을 추가한 것은 아니다. Inspector에서 중력만 독립 변경하거나 Fixed Timestep을 바꾸면 최고점을 다시 측정·보정해야 한다. 현재 수치는 사용자 조작감 리뷰 전 초기값이다.

## 7. 연결과 직접 확인

PlayerMovementFeelTest에서 같은 발판을 대상으로 Space를 길게 눌러 최대 점프를 확인하고, 짧게 눌러 낮은 점프를 비교한다. 오른쪽 통로에서 벽차기 높이와 상승 감각을 확인한다. A/D 출발·반전·해제는 여전히 즉시 적용돼야 한다.

Inspector에서 PlayerJump의 Jump Speed/Cut Multiplier, PlayerMovement의 Rise/Fall Gravity, PlayerWallJump의 Vertical Speed를 함께 확인한다. 32 PPU·스케일·사용자 충돌체·테스트 맵 지형은 변경하지 않는다.

## 8. 검증

**22개 검사 통과: 9 Editor + 13 실제 Play Mode.**

Editor는 새 수치의 실제 저장, 수평 이동/낙하 상한/보호/코요테/버퍼/사용자 충돌체·0.02s 고정 간격 보존을 확인했다. 실제 Play에서는 같은 캐릭터와 충돌체에서 기존 값을 테스트 중 일시 적용해 측정한 후 새 값으로 측정했다. 가상 Input System 키보드·실제 Physics2D로 실행했으며 원본 프리팹을 이전 값으로 재저장하지 않았다.

| 실제 측정 | 이전 | 변경 후 |
| --- | ---: | ---: |
| 일반 최대 상승량 | 1.352000u | 1.352000u |
| 한 틱 짧은 탭 상승량 | 0.464000u | 0.464000u |
| 벽차기 발사 위치 대비 최고점 | 1.104000u | 1.104000u |
| 일반 최고점 관측 시간 | 0.280s | 0.320s |
| 일반 착지까지 | 0.500s | 0.560s |

일반 최대 높이 차이는 0.00000024u였다. 높이 검사는 0.001u 이내를 통과 기준으로 사용했으며 이는 32 PPU에서 0.032px이다. 반올림된 표가 모든 부동소수점 값을 비트 단위로 같다고 의미하지 않는다. 벽 최고점은 각 발사 지점으로부터의 상대 상승량이다.

가변 점프의 차이 유지, X 즉시 출발/정지, 공중 재입력 이중 점프 차단 및 정상 착지도 확인했다. 테스트 전 열린 씬 목록·활성 씬·dirty 상태 비교는 RESTORED로 통과했다. 검사 복사 씬/meta, 임시 검사 소스 2개/meta/request/ready를 모두 제거하고 최종 28개 소스를 별도로 컴파일해 오류 0을 확인했다. 이 별도 컴파일을 실제 Play 검사 수에 포함하지 않는다.

Assets/Packages/ProjectSettings 시작 해시와 비교해 바뀐 기존 파일은 Player.prefab과 위 네 C# 소스뿐이며 기존 파일 누락은 없다. 기존 씬·Input Actions·공격 데이터·원화/클립·사용자 충돌체·설정·패키지·기존 meta/GUID와 staged 픽셀 씬의 45줄 추가를 보존했다. 테스트 추가 씬 진입 중 AudioListener 중복 경고가 있었으며 다른 씬은 검사 중에만 비활성화하고 종료 후 복원했다. 물리 장치 조작감과 전체 중간 입력 길이·전체 전투/코요테 회귀를 모두 새로 검사한 것은 아니다.

## 9. 리뷰

높이가 같으면서 무게감이 적절히 줄었는지 사용자 검토가 필요하다. 입력 길이에 따른 중간 가변 점프 감각도 직접 확인한다. 사용자 검토 전 다음 기능·커밋·push는 진행하지 않는다.

## 최종 Unity 검사 로그

```text
Character animation validation 2026-11-03T19:48:40.8008545+09:00
EDITOR PASS Gravity reduced twenty percent
EDITOR PASS Discrete-height-corrected ground launch saved
EDITOR PASS Quick-tap cut correction saved
EDITOR PASS Height-corrected wall launch saved
EDITOR PASS Wall outward launch and forgiveness preserved
EDITOR PASS Direct horizontal control and fall cap preserved
EDITOR PASS Ground forgiveness preserved
EDITOR PASS Custom collider preserved
EDITOR PASS Calibrated fixed timestep unchanged
SUCCESS EDITOR 9
BEFORE full=1.352000 apex=0.280 flight=0.500
BEFORE short=0.464000
PLAY PASS Measurement begins at real wall contact
PLAY PASS Measurement begins with actual wall launch
BEFORE wall=1.104000
PLAY PASS Actual full jump height unchanged within 0.001u
PLAY PASS Same height with a gentler longer trajectory
AFTER full=1.352000 apex=0.320 flight=0.560 delta=0.00000024
PLAY PASS Actual quick-tap height unchanged within 0.001u
PLAY PASS Variable height remains distinct
AFTER short=0.464000
PLAY PASS Measurement begins at real wall contact
PLAY PASS Measurement begins with actual wall launch
PLAY PASS Actual wall-jump apex unchanged within 0.001u
AFTER wall=1.104000
PLAY PASS No horizontal acceleration reintroduced
PLAY PASS Release still stops immediately
PLAY PASS Air repress does not create another jump
PLAY PASS New trajectory lands normally
SUCCESS PLAY 13
RESTORED
```
