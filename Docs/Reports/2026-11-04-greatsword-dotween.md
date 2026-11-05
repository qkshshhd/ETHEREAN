# 대검 프리팹과 DOTween 3연타 — 초기 작업 기록

> 상태 갱신: 초기 버전 Unity 검사 55개(16 Editor+39 Play)가 이후 통과했다. 이어 진행한 1타 개선도 64개 검사와 임시 파일 정리까지 완료했다. 아래 대기 상태는 당시 기록이며, 현재 구현/검증은 [1타 개선 보고서](2026-11-04-first-swing-smoothing.md)를 따른다.

사용자의 최신 요청에 따라 제공된 `ETHEREAN_SPRITE/그저 검.png`로 별도 검 프리팹을 만들었다. 과거의 ‘검은 몸 애니메이션에 포함’ 결정은 **검 표시 방식에 한해** 이번 요청으로 대체한다. 타격은 계속 기존 공격 입력과 Windup/Active/Recovery 시간에 따라 판정한다. 검의 Transform이나 Collider가 피해를 결정하지 않는다.

## 현재 확인 수준

- 제공 PNG와 프로젝트 복사본은 바이트가 같다. 픽셀을 다시 그리거나 리사이즈하지 않았다.
- 1차 실제 Unity 실행에서 Editor 16항목 통과. 잠금 시 숨김, 해금 시 뒤로 든 자세, 실제 지상 공격 시작, 1타 DOTween 길이 일치까지 Play 4항목 통과했다.
- 이어진 일시정지 검사에서 실패해 검사가 중단됐다. 검사 코루틴이 해당 프레임의 LateUpdate 이전 자세를 비교하던 문제를 확인하여, 일시정지 후 한 프레임을 정착시키고 비교하도록 **검사 코드를 수정**했다. 수정된 검사가 통과했다고 주장하지 않는다.
- 최신 검증 버전은 `greatsword-tween-3`이다. 현재 Unity가 불러온 버전은 `greatsword-tween-1`이어서 최종 3연타·좌우·공중·중단 검증은 대기 중이다. Unity에서 Play를 멈추고 포커스한 뒤 Ctrl+R을 누르면 준비된 검사가 복사 씬에서 실행된다.
- 1차 검사가 종료된 뒤 원래 열린/활성/dirty 씬 상태 비교는 `RESTORED`였다. 최종 버전에서도 다시 확인해야 한다.
- 최신 영구 Runtime 33개와 Editor 10개 소스를 분리 컴파일하여 각각 오류 0을 확인했다. Inspector 직렬화 필드에 대한 CS0649 경고는 존재한다. 이는 Unity 최신 소스 import/실행 완료를 뜻하지 않는다.
- 초기 Editor 실행은 기본 정지 이미지의 가시 높이 28px를 기준으로 크기를 설정했다. 이후 실제 Idle 첫 프레임의 30px를 기준으로 수정했다. 최신 prefab 크기/소켓 수정은 아직 Unity 재검증 전이다.

## 추가·수정 파일

아래 경로는 프로젝트 루트 기준이다. 새 Unity 자산 및 스크립트에는 각각 `.meta`가 함께 생성되어 있다.

| 파일 | 구분 및 변경 위치 | 책임 |
|---|---|---|
| `Assets/_Project/Art/Sprites/Weapons/Greatsword.png` | 추가, 제공 원본 그대로 복사 | 검 이미지. Point, 32 PPU, 무압축, mipmap 없음 |
| `Assets/_Project/Prefabs/Weapons/Greatsword.prefab` | 추가, Greatsword/BladeSprite | 검 표시 전용. Rigidbody/Collider/Trail/Particle 없음 |
| `Assets/_Project/Data/Player/PlayerSwordMotion.asset` | 추가, Carry와 Combo 3개 | 자세·회전·Ease·상대 표시 순서 및 복귀 설정 |
| `Assets/_Project/Scripts/Runtime/Player/Combat/PlayerSwordMotionData.cs` | 추가, SwordPose/SwordMotionStep/PlayerSwordMotionData | Inspector에서 편집 가능한 모션 데이터 |
| `Assets/_Project/Scripts/Runtime/Player/Combat/PlayerSwordMotion.cs` | 추가, OnPhaseChanged/LateUpdate/Append/ReturnToCarry/OnDisable | 기존 전투 상태를 읽고 DOTween으로 검 표시만 제어 |
| `Assets/_Project/Scripts/Editor/PlayerSwordMotionBuilder.cs` | 추가, Build/SaveSwordPrefab/VisibleHeight/AuthorMotion | 이미지 import, 검 prefab 생성, Player 연결을 반복 가능한 메뉴로 제공 |
| `Assets/_Project/Prefabs/Player/Player.prefab` | 수정, 루트 PlayerSwordMotion 및 CharacterVisual 하위 | WeaponFacing → WeaponSocket → nested Greatsword 참조 연결 |
| `ProjectSettings/ProjectSettings.asset` | DOTween Editor 초기화 과정에서 변경 확인 | 각 플랫폼의 scriptingDefineSymbols에 DOTWEEN 추가. 플러그인 소스/설정 자산은 수정하지 않음 |
| `AGENTS.md`, `PLAYER_SETUP.md` | 최신 결정 추가 | 별도 검 표시 허용, 현재 검증 상태와 후속 검토 기준 기록 |
| 이 보고서 및 `2026-11-04-greatsword-dotween.validation.txt` | 추가 | 파일별 책임·검증 근거·미확인 사항 |

기존 PlayerAttack/Hitbox/Locomotion/Movement 및 공격 데이터 6개를 변경하지 않았다. 이동 속도 3과 현재 점프/중력/벽차기 수치, 저장 씬, 물리 레이어/충돌 정책도 유지한다. 이번 작업 직전 스냅샷 대비 기존 파일 변경은 Player.prefab과 위 ProjectSettings뿐이었다. 새 파일은 이 비교와 별개다.

## 동작 원리와 설계 이유

1. 기존 `PlayerAttack`이 입력을 받고 공격 단계와 콤보 번호를 결정한다. 이 코드가 준비 시간, 실제 피해 시간, 후딜과 다음 타 연결 가능 시점을 계속 소유한다.
2. `PlayerSwordMotion.OnPhaseChanged`는 해당 타의 모션 데이터를 읽고, 현재 검 자세에서 Lift → Ready → Strike → Recovery로 가는 DOTween Sequence를 만든다.
3. Sequence는 Manual/Paused로 두고 `LateUpdate`에서 기존 공격 단계와 진행률을 `Goto`로 샘플링한다. 렌더 프레임 사이의 짧은 시간은 현재 단계 안에서만 보간한다. DOTween 콜백으로 피해를 발생시키지 않으므로 전투와 별개의 시계가 판정을 앞당기지 않는다.
4. 연속 공격은 직전 검 자세에서 시작한다. 원본 Euler 각도를 그대로 보간하여 아래로 베는 회전이 최단 경로 계산 때문에 거꾸로 뒤집히지 않게 한다.
5. `WeaponFacing`의 X 스케일로 좌우를 반전한다. 공격 중에는 기존 공격의 확정된 방향을 쓰고, 평상시에는 캐릭터의 방향을 따른다. 캐릭터 몸/물리 루트의 스케일을 변경하지 않는다.
6. 공격 종료 또는 취소 시 Carry로 복귀한다. 비활성화·사망·잠금 시 자체 Sequence/복귀 Tween만 종료하고 검을 숨긴다. `KillAll`이나 전역 ManualUpdate를 쓰지 않는다. 다른 DOTween 연출은 영향을 받지 않는다.

공격 판정, 이동, 검 표시, 표시 데이터, Editor 제작 도구를 나눠 책임을 분리했다. 검 프리팹은 Player의 중첩 자산으로 유지하여 검 이미지를 바꾸어도 전투 코드를 고치지 않아도 된다.

## 3연타 기본 설정

| 타격 | 움직임 | 기존 Windup / Active / Recovery | Strike Ease |
|---|---|---|---|
| 1타 | 뒤로 끌던 검을 천천히 들어 올리고 앞·아래로 내려침 | 0.8 / 0.14 / 0.38초 | InQuad: 내려칠 때 가속 |
| 2타 | 내려친 위치에서 빠르게 위로 올리고 뒤쪽으로 보냄 | 0.12 / 0.18 / 0.65초 | OutQuad: 초기에 힘 있게 상승 |
| 3타 | 뒤쪽에서 재정비 후 가로로 휘두름 | 0.32 / 0.2 / 0.5초 | InOutSine |

3타는 같은 수직 회전을 반복하지 않고 Y 회전과 약간의 기울기를 이용해 가로베기의 원근 단축을 표현한다. 스프라이트를 새로 그리거나 궤적을 생성하지 않는다. 기존 1타 후 최소 약 0.228초, 2타 후 최소 약 0.52초의 연결 대기 정책을 유지한다. 이는 데이터상 값이며 최신 실제 입력 검증 결과는 아직 없다. 공중도 해당 공중 공격 데이터의 시간을 읽고 같은 3개 자세 프로필을 사용한다.

## Inspector에서 조절하는 곳

- `PlayerSwordMotion.asset`: Carry, Return Duration/Ease, Combo 각 원소의 Lift/Ready/Strike/Recovery Position·Rotation·Order Offset, Lift Fraction, Windup/Strike/Recovery Ease.
- Position은 소켓 기준 로컬 좌표다. Rotation은 누적 회전 경로를 의도한 Euler 값이다. Order Offset -1은 몸 뒤, +1은 몸 앞이다. Windup은 Lift Fraction 전후에 Lift/Ready, Active는 Strike, Recovery는 Recovery의 표시 순서를 사용한다.
- `Player.prefab/CharacterVisual/WeaponFacing/WeaponSocket`: 손의 기준 위치. 현재 `(0.1, 0.44, 0)`이며 이후 몸 공격 클립의 실제 손 위치에 맞춰 조정해야 한다.
- `Greatsword.prefab/BladeSprite`: 제공 스프라이트의 균일 표시 크기. 현재 0.6206897배. 이미지 전체 64px가 아니라 투명 여백을 뺀 58px를 기준으로, 실제 Idle 높이 30/32=0.9375u의 1.2배인 1.125u를 목표로 한다. 이 비율은 임시 조정값이다.
- 검의 pivot은 손잡이 `(16.5, 54)`px. 텍스처의 중심이 아닌 손잡이 주변에서 회전한다.
- `Length Ratio` 변경은 런타임 자동 크기 변경이 아니다. `ETHEREAN > Player > Connect Greatsword Tween`을 실행해야 적용된다. 이 메뉴는 기존 모션 데이터의 자세 편집은 보존하지만, 검 import/균일 크기/소켓 기준 위치와 prefab 참조는 다시 설정한다. 손 위치를 직접 튜닝한 뒤에는 재실행 전에 주의한다.

## 최종 검증 계획과 임시 파일

아직 남은 실제 Unity 검사는 지상 오른쪽 3연타, 공중 왼쪽 3연타, 각 끝자세/표시 순서, 실제 Active 피해 1회씩, 후딜 연결 간격, 일시정지, 피격/점프 취소, 비활성화/재활성화, 사망, 무관한 Tween 보존이다. 임시 자동 검사는 물리 장치를 대신해 별도 가상 키보드를 사용하며 복사 씬에서만 수행한다. 하드웨어 조작감이나 최종 아트 적합성 검사는 아니다.

대기 중인 임시 파일은 `Assets/_Project/Scripts/Editor/SwordTweenValidation.cs`, `Assets/_Project/Scripts/Runtime/Player/SwordTweenValidationDriver.cs`와 각 meta, `Logs/SwordTweenValidation.request` 및 ready이다. **최종 검사가 끝나면 제거하고 영구 Runtime/Editor 소스를 다시 분리 컴파일해야 한다.** 현재 검증 도구가 남아 있는 상태를 최종 정리 완료로 간주하지 않는다. 복사 씬 `SwordTweenTemporary.unity`는 검사 중 생성되고 원래 상태 복원 시 삭제된다.

## 검토 시 한계

- 제공된 공격 자세 PNG 3개는 연속 몸 공격 애니메이션 클립이 아니다. 이번에는 몸 공격 아트를 임의로 만들거나 그 이미지들을 가짜 연속 동작으로 연결하지 않았다. 기존 선택적 PlayerAttackAnimator 구조를 유지하며, 실제 몸 클립이 준비되면 손과 검의 시간별 위치를 맞춰야 한다.
- 현재 몸이 중립 자세로 보이는 상태에서 검만 휘두르는 것은 최종 몸 애니메이션 연결 완료를 의미하지 않는다.
- 기존 타격 박스 크기는 그대로다. 현재 검 표시 길이와 실제 타격 영역이 시각적으로 정확히 일치하는지는 미확인이다. 별도 검토 없이 피해 범위를 바꾸지 않았다.
- 임의 VFX/잔상/명중 표시/코드 생성 아트/Gizmo를 추가하지 않았다. 기존 사용자 VFX prefab 슬롯은 그대로다.
- 최종 Unity 검증, 임시 파일 정리와 사용자 체감 검토가 남았다. 다음 기능으로 넘어가거나 커밋/푸시하지 않는다.
