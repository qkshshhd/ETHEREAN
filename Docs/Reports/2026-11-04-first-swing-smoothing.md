# 1타 연속 회전과 바닥 여유 조정

사용자의 검 크기 변경을 유지하면서 1타를 ‘아주 느리게 들어 올리기 → 중간 이후 빠른 내려치기’로 수정했다. 새 1타 구현과 Unity 검증은 완료했으며 조작감/연출 리뷰를 기다린다. 2·3타의 모션 조정이나 다음 기능을 시작하지 않았다.

## 왜 턱이 생겼는가

이전 1타는 Carry → Lift → Ready → Strike마다 별도 Tween을 만들었다. Lift와 Ready의 InOutSine이 각각 끝에서 멈추고 다시 출발해서 중간에 속도가 꺾였다. 각 자세의 손잡이 위치도 달라 회전 중 검 전체가 움직였다.

이번에는 1타에만 `Continuous Swing`을 켜고 Windup+Active 전체를 하나의 DOTween으로 보간한다. 기존 전투 시계를 계속 사용하되, 두 단계의 경계에서 회전 곡선을 다시 시작하지 않는다. 손잡이 위치는 Carry/Strike/Recovery 모두 로컬 `(0,0,0)`으로 맞췄다.

## 변경 파일과 위치

경로는 프로젝트 루트 기준이다. 새 영구 C# 파일은 없다. 기존 `.meta`와 GUID를 유지했다.

| 파일 | 수정 위치 | 기능/이유 |
|---|---|---|
| `Assets/_Project/Scripts/Runtime/Player/Combat/PlayerSwordMotionData.cs` | SwordMotionStep 필드·프로퍼티 | ContinuousSwing과 Inspector 편집용 SwingProgress(AnimationCurve) 추가. 기존 분할 모션은 기본값 false로 유지 |
| `Assets/_Project/Scripts/Runtime/Player/Combat/PlayerSwordMotion.cs` | OnPhaseChanged, 새 AppendContinuousSwing | 연속 모드에서 Windup+Active를 하나의 Tween으로 생성. Recovery와 기존 공격 판정 시계/취소/자체 Tween 정리 책임 유지 |
| `Assets/_Project/Data/Player/PlayerSwordMotion.asset` | Carry 및 Combo[0] | Carry -80°, 1타 Strike/Recovery -280°, 손잡이 위치 0, 연속 모드와 4키 곡선 설정 |
| `Assets/_Project/Scripts/Editor/PlayerSwordMotionBuilder.cs` | AuthorMotion의 신규 자산 기본 자세 | 새 모션 자산을 만드는 경우에도 1타 연속 모드와 같은 시작·끝자세 사용. 이번 작업에서는 Build 메뉴를 실행하지 않음 |
| `Assets/_Project/Prefabs/Player/Player.prefab` | 중첩 Greatsword의 회전 override | Carry -80°가 Editor 미리보기에도 반영되도록 quaternion 회전 override만 수정 |
| `AGENTS.md`, `PLAYER_SETUP.md` | 최신 작업 기록 추가 | 사용자 검 크기 유지, 1타 수정 이유/값/검증 완료 및 검토 대기 상태 저장 |
| 이 보고서와 validation.txt, 아래 5개 PNG | 추가 | 실제 검사 로그 및 실제 카메라의 동작 단계 캡처 |

이전 작업의 임시 `SwordTweenValidation.cs`, `SwordTweenValidationDriver.cs`와 각각의 meta, SwordTween/FirstSwing 요청·ready 파일을 제거했다. 복사 테스트 씬도 남기지 않았다.

## 적용한 값

| 항목 | 이전 | 현재 |
|---|---|---|
| 사용자 Greatsword/BladeSprite 스케일 | 사용자가 수정한 (1,1,1) | 그대로 유지 |
| Carry 회전 Z | -65° | -80°: 뒤쪽에 더 눕혀 바닥 여유 확보 |
| 1타 들어 올림/타격 방식 | 각 구간 Ease 재시작 | Windup+Active 단일 연속 곡선 |
| 1타 손잡이 위치 | 여러 로컬 위치 사이 이동 | (0,0,0) 고정 |
| 1타 끝 회전 Z | -295° | -280°: 전방 수평보다 10° 아래에서 마무리 |
| Windup / Active / Recovery | 0.8 / 0.14 / 0.38초 | 그대로 유지 |

정규화 시간 → 회전 진행률의 키는 `(0,0)`, `(0.5,0.1)`, `(0.8510638,0.65)`, `(1,1)`이다. 중간 키의 들어오는/나가는 접선은 각각 0.6과 3으로 맞췄고, 시작과 끝 접선은 0이다. 이로써 중간 연결에서 속도가 끊기지 않으며, 끝에서는 부드럽게 멈춘다.

실제 준비+타격 0.94초 중 처음 0.47초는 전체 회전의 10%만 움직인다. 준비 종료 0.8초에는 65%가 진행되고, 나머지 35%를 Active 0.14초 동안 빠르게 내려친다. 손잡이를 위아래로 움직여 끝을 피하는 방식 대신, 현재 크기를 유지한 채 전체 회전 범위와 끝각도를 조정했다.

`PlayerAttack`과 여섯 공격 데이터, 타격 박스, 이동/점프/중력/넉백/물리 설정, 검 PNG/import/meta/material/크기, 저장된 테스트 씬은 이 작업 직전 스냅샷과 바이트가 같다. 2·3타 모션 데이터도 바이트가 같다. Carry는 모든 공격 종료 후 돌아가는 공통 자세이므로 변경된 평상시 각도는 다른 타 종료 뒤에도 적용된다.

## Inspector 조정

`PlayerSwordMotion.asset > Combo > Element 0`에서:

- `Continuous Swing`: 이번 1타의 단일 곡선 사용 여부.
- `Swing Progress`: 가로축은 Windup+Active 전체의 시간, 세로축은 Carry에서 Strike까지의 회전 진행률. 중간 키의 양쪽 접선을 맞추면 연결 속도의 턱을 막을 수 있다.
- `Strike / Recovery > Rotation Z`: 내려친 끝각도와 후딜 자세. 현재 -280°.
- `Strike / Recovery > Position`: 현재 0으로 유지하여 손잡이 이동을 제거했다.
- `Lift/Ready` 위치·회전, `Windup Ease`, `Strike Ease`는 연속 모드의 경로에 사용하지 않는다. 기존 모드 및 2·3타에서는 계속 사용한다. 앞뒤 정렬 Offset과 Lift Fraction에 따른 Windup 정렬 선택은 기존 구조를 유지한다.

검 크기는 `Greatsword.prefab > BladeSprite`에서 계속 직접 조절할 수 있다. 현재 Builder의 Connect 메뉴는 크기 비율과 소켓을 다시 설정하는 도구이므로, 사용자 크기를 유지하려고 이번에는 실행하지 않았다. Runtime이 검 크기를 다시 설정하지도 않는다.

## 검증 결과

실제 Unity 6000.4.7f1에서 **64개: Editor 23 + Play Mode 41** 통과. 로그는 `2026-11-04-first-swing-smoothing-validation.txt`에 저장했다.

- Editor에서 단일 곡선의 단조 증가, 역회전/초과 없음, 초반 느린 진행, 중간 키·기존 분할 지점·Windup/Active 경계의 속도 연속성, 고정 손잡이를 확인했다.
- 현재 BladeSprite scale=(1,1,1), 실제 Sprite pivot/PPU와 모든 불투명 픽셀의 모서리를 사용해 전체 회전 경로 4,001개 자세를 검사했다. CapsuleCollider2D의 바닥 높이 대비 최소 여유는 **0.04646569u**였다. 평지에 선 현재 크기 기준의 샘플 검사이며 경사지·앞쪽 단차를 감지하는 자동 보정 기능은 아니다.
- 최초 바닥 검사에서는 FullRect mesh의 투명 캔버스 모서리까지 검으로 간주해 실패했다. 검사를 불투명 픽셀 기준으로 바로잡아 다시 실행했다. 실패한 검사를 통과로 바꿔 기록한 것이 아니라 수정 후 새 로그 전체를 남겼다.
- 실제 가상 키보드 입력으로 지상 오른쪽 3연타와 공중 왼쪽 3연타를 실행했다. 양쪽 모두 1타 손잡이 이동 없음, 자세·표시 순서, 회전 종료, Carry 복귀와 일시정지가 통과했다.
- 1타 후 연계 대기 약 0.22초, 2타 후 약 0.52초를 유지했다. 실제 지상 공격은 각 Active에 한 번씩 총 3회 피해를 냈다.
- 실제 피격/후딜 점프 취소, 비활성화/재활성화, 사망 시 숨김·자체 Tween 해제, 무관한 Tween 보존도 통과했다.
- 실제 카메라로 들어 올림 3단계·내려침·끝자세를 캡처하고 열어 확인했다. 몸 공격 클립과 손 위치를 최종 동기화한 검사는 아니다.
- 검사 후 열린/활성/dirty 씬 상태 비교 `RESTORED`. 관련 임시 코드/meta/요청/ready/복사 씬 제거 완료.
- 정리 후 영구 **Runtime 33개, Editor 10개**를 별도로 다시 컴파일해 각각 exit 0. 직렬화 필드 CS0649 경고는 있으며 오류는 없다.
- 스냅샷 비교에서 위 5개 구현 파일 외의 기존 Assets/Packages/ProjectSettings 변경은 없었다. 삭제된 Assets 파일은 임시 검사 코드/meta 4개뿐이다.

## 실제 카메라 캡처

| 파일 | 시점 |
|---|---|
| `2026-11-04-first-swing-FirstSwingLift0.png` | 천천히 들어 올리는 초반 |
| `2026-11-04-first-swing-FirstSwingLift1.png` | 중간 들어 올림 |
| `2026-11-04-first-swing-FirstSwingLift2.png` | 머리 위를 통과하는 구간 |
| `2026-11-04-first-swing-FirstSwingCut.png` | 빠르게 내려치는 구간 |
| `2026-11-04-first-swing-SwordCombo1End.png` | 바닥 위에서 멈춘 1타 끝자세 |

## 직접 검토할 순서

1. 기존 PlayerBadgerCombatTest에서 무기 해금 상태로 오른쪽을 보고 공격을 한 번 누른다. 들어 올리는 도중 두 번 멈추거나 손잡이가 위아래로 이동하지 않는지 본다.
2. 왼쪽을 보고 반복해 반전된 검도 평지 바닥을 파지 않는지 본다.
3. 1타 Active 또는 Recovery에 공격을 예약한다. 1타 후 기존 대기 뒤 2타로 연결되는지 본다. 2·3타 자체의 자세는 이번 조정 범위 밖이다.
4. 필요하면 Element 0의 Swing Progress와 끝각도만 조금씩 조절하고 체감을 비교한다. 아직 없는 몸 공격 애니메이션과의 최종 손맞춤은 이후 해당 리소스가 준비되면 검토한다.

현재 단위에서 멈추고 사용자 리뷰를 기다린다. 커밋/푸시하지 않았다.
