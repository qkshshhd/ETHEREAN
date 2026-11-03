# 캐릭터 Idle / Walk / Run 연결 — 2026-11-03

## 1. 목표와 현재 완료 상태

사용자가 바탕화면에 제공한 기본 캐릭터 이미지와 Idle·Walk·Run 각 16프레임을 Unity에 저장하고 이동에 따라 표시한다. 이번 요청은 제공된 그림을 사용하는 승인이며 새로운 외형 제작이나 다른 기능 개발 승인이 아니다.

**연결·검증·임시 파일 정리 완료, 사용자 리뷰 대기.** 49 PNG, 루프 클립 3개, AnimatorController, Player 프리팹 참조를 실제 Unity Editor API로 저장했다. 새 검증은 **79개 통과: 59 Editor + 20 실제 Play Mode**. 임시 검사 파일을 제거한 최종 26개 C# 소스도 별도로 컴파일해 오류 0을 확인했다. 실제 캐릭터 렌더를 캡처해 시각적으로 확인했다. 실제 공격/점프 그림 제작은 이번 작업에 포함하지 않는다.

## 2. 수정·추가 파일과 위치

경로 기준: `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN`.

| 파일 | 변경 위치와 책임 |
| --- | --- |
| `Assets/_Project/Scripts/Runtime/Player/Combat/PlayerAttackAnimator.cs` | `IsDrivingAnimation` 조회 속성 추가. 실제 공격 애니메이션을 구동 중인지 표시 담당자가 확인할 수 있다. 공격 시간·판정은 변경하지 않는다. |
| `Assets/_Project/Scripts/Runtime/Player/PlayerCharacterAnimator.cs` (추가) | `LateUpdate`: 실제 수평 속도와 접지 여부를 읽어 Idle/Walk/Run 선택, SpriteRenderer.flipX 설정. `idleSpeedThreshold`, `runSpeedThreshold`, `playbackSpeed`를 Inspector에 노출. Rigidbody 속도를 쓰지 않는다. `OnDisable`: 재생 속도 복원. |
| `Assets/_Project/Scripts/Editor/PlayerCharacterAnimationBuilder.cs` (추가) | `Build`: 49 PNG의 TextureImporter 설정, 프레임 순서와 루프를 갖는 클립 및 컨트롤러 생성, 기존 Player 프리팹 연결. `Set`: SerializedObject를 통한 참조 연결. 기존 GUID가 있는 생성물은 재사용한다. |
| `Docs/Reports/2026-11-03-character-locomotion-animations.md` (추가) | 본 작업의 상태·파일 목록·설계·확인 절차 기록. |
| `AGENTS.md`, `PLAYER_SETUP.md` | 이번 승인 범위·완료 상태·검증·리뷰 대기 상태를 기록. |

기본 그림과 48 애니메이션 PNG의 전체 파일 목록은 아래 자동 생성 목록을 참조한다. 바탕화면 원본은 이동·수정하지 않았으며 모든 복사 파일은 원본과 바이트 단위로 동일하다. 새 Unity `.meta`는 Editor가 생성했다. 기존 파일 삭제·이름 변경은 없다. 이번에 만든 임시 검사 파일은 검증 후 제거했다.

실제 저장한 Unity 에셋:

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab`: 기존 `Visual/SpriteRenderer.enabled=false`; 신규 `CharacterVisual`에 SpriteRenderer와 Animator, 루트에 PlayerCharacterAnimator; PlayerAttackAnimator.characterAnimator와 returnStateName 연결. 기존 Visual Transform 및 콜라이더/물리/이동 설정은 유지한다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Animations/Player/Idle.anim`, `Walk.anim`, `Run.anim`: SpriteRenderer.m_Sprite만 바꾸는 루프 클립.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Animations/Player/PlayerCharacter.controller`: Base Layer의 Idle/Walk/Run 상태, 기본 Idle.
- 새 PNG·클립·컨트롤러·스크립트 및 폴더의 Unity `.meta`.

## 3. 변경 전후와 실행 원리

기존에는 청록색 사각형만 표시했다. 새 표시 담당자는 기존 이동 계산이 끝난 뒤 LateUpdate에서 PlayerMovement.Velocity와 PlayerLocomotion.IsGrounded를 읽는다. 실제 이동 속도에 따라 애니메이션을 선택하므로 벽에 막혀 정지했을 때 달리기 모션을 계속하지 않는다.

| 조건 | 표시 |
| --- | --- |
| 접지, 절대 수평 속도 ≤ 0.1u/s | Idle |
| 접지, 0.1u/s < 절대 수평 속도 < 3u/s | Walk |
| 접지, 절대 수평 속도 ≥ 3u/s | Run |
| 공중·이동 중지·공격 중이며 대응 애니메이션 없음 | Idle 첫 프레임 정지 |
| 별도 공격 어댑터가 실제로 유효한 공격 상태를 재생 중 | 이동 표시 담당자가 Animator 재생을 양보 |

새 상태에 진입할 때만 Animator.Play를 호출하며 같은 상태를 매 프레임 처음부터 재생하지 않는다. 중립 프레임 유지 상태에서는 0번 프레임을 고정한다. 반전은 실제 이동 속도의 부호를 따르며, 공격 중에는 기존 공격 방향을 따른다. 정지 시 직전 방향을 유지한다.

걷기/달리기 입력을 새로 추가한 것이 아니다. 키보드는 기존 가속 중 잠깐 Walk를 거쳐 Run에 도달하고, 낮은 아날로그 입력은 Walk를 유지할 수 있다. 기존 이동 속도나 가속·감속은 그대로다.

## 4. 설계 이유와 트레이드오프

물리와 표현 책임을 나누어 PlayerMovement가 유일한 속도 적용 담당자로 남는다. Animator 상태 선택은 간단한 세 가지 표현이므로 복잡한 상태 머신 프레임워크나 사용하지 않는 인터페이스를 만들지 않았다. 클립은 SpriteRenderer만 바꾸므로 루트 이동·콜라이더에 애니메이션이 개입하지 않는다.

기존 Visual은 사각형용 비균일 스케일과 씬 인스턴스 override가 있다. 이를 초기화하는 대신 기존 렌더러만 끄고 루트 아래 단위 스케일 CharacterVisual을 추가한다. 기존 씬의 스케일 override를 보존하면서 그림이 찌그러지는 것을 막는다. 발 기준 위치는 CapsuleCollider2D 하단이며 콜라이더 자체는 줄이지 않는다. 따라서 새 그림보다 기존 충돌체가 크다는 제한이 있다.

공격·점프·벽 미끄러짐 전용 그림은 제공되지 않았다. 해당 애니메이션을 완성했다고 주장하지 않으며 중립 프레임으로 대체한다. 이후 공격 상태를 실제 컨트롤러에 추가하면 PlayerAttackAnimator의 재생이 우선한다.

## 5. 픽셀 설정 및 임시값

- 기존 비교 에셋의 32 PPU를 확인했다. 새 이미지도 32 PPU로 설정한다.
- Point, 압축 없음, Mip Map 끄기, NPOT 스케일링 없음, Clamp, 투명 알파, Full Rect. 원본 PNG를 리샘플링하지 않는다.
- 원본 캔버스는 32×32. 애니메이션에서 보이는 높이는 약 28~31px, 기본 이미지의 보이는 높이는 28px이다. 기존 **40px 잠정 목표를 확정하거나 바꾼 것이 아니다.** 40px에 맞추어 임의 확대하지 않는다.
- 공통 피벗 `(0.5, 1/32)`: 캔버스 하단의 투명 여백 1px을 기준으로 발 위치를 맞춘다. 일부 Run 프레임은 그보다 아래쪽 픽셀을 사용하므로 1px 정도의 자연스러운 차이가 남는다.
- 임시 재생률: Idle 8fps, Walk 12fps, Run 16fps. 루프 끝에는 첫 프레임 키를 추가해 마지막 프레임도 온전히 한 프레임 지속한다.
- 임시 전환값: Idle 0.1u/s, Run 3u/s, 전체 재생 배율 1. 이 값은 PlayerCharacterAnimator Inspector에서 조정한다. 개별 클립 frameRate는 애니메이션 에셋에서 조정 가능하다.
- 생성 메뉴를 다시 실행하면 초기 클립 속도와 프리팹 연결을 다시 적용한다. 일상적인 튜닝을 위해 반복 실행할 필요는 없다.
- Point 필터는 텍스처 보간을 제거한다. 현재 모든 씬을 정수 확대/Pixel Perfect 카메라로 바꾼 것은 아니므로 임의 Game 뷰 크기나 카메라 위치에서 픽셀 흔들림까지 해결했다고 주장하지 않는다.

## 6. 실행한 검사와 미검증

완료:

- 원본과 복사된 49 PNG의 바이트 일치 확인.
- 48 애니메이션 프레임이 RGBA 32×32인지 확인.
- 실제 픽셀 프레임을 확대해 방향·투명 여백·프레임 변화를 시각적으로 확인.
- Unity가 새 스크립트를 import하고 Editor/Play 검사기를 실제 실행했다.
- **59 Editor 검사:** 49 이미지 import 설정, 이미지 수, 각 3개 클립의 sprite-only 바인딩·숫자 순서·루프, 프리팹 참조·기본 잠금·충돌체·기존 사각형 비활성·단위 스케일.
- **20 실제 Play Mode 검사:** Idle/Walk/Run 프레임 진행, 가상 게임패드 저속 Walk 유지, 실제 키보드 가속과 감속, 좌우 반전, 점프/공중 프레임 유지/착지, 기존 공격 시작·복귀, 이동 disable/reenable, 런타임 충돌체 및 실제 렌더 캡처.
- 테스트용 복사 씬을 닫고 삭제했으며, 시작 당시 열린 씬 목록·활성 씬·dirty 상태 비교가 `RESTORED`로 통과했다.
- 임시 검사 소스/meta/요청/ready/복사 씬을 제거한 최종 **26개 C#** 별도 Roslyn 컴파일 통과, 오류 0. 이 별도 컴파일 자체를 Play 검증으로 세지 않는다.
- 기존 staged PlayerPixelScaleTest.unity의 45줄 추가 상태 유지 확인.

미검증: 사용자의 실제 물리 키보드/게임패드 조작감, 다양한 화면 배율에서의 픽셀 흔들림, 최종 캐릭터/충돌체 크기 조율, 실제 공격/점프/벽 미끄러짐 아트. 처음 실행 시 누락 컴포넌트를 `??`로 추가하던 부분이 Unity의 특수 null 객체를 처리하지 못해 실패했다. `Build`를 Unity의 `== null` 판정으로 수정한 후 새로 실행한 최종 79개 검사가 통과했다. 실패한 최초 실행은 통과 수에 넣지 않았다. 테스트용 추가 씬 진입 과정에서 AudioListener 중복 경고가 있었으며, 검증 중 다른 씬 루트를 비활성화하고 종료 후 원래 씬 상태를 복원했다. 기존 씬은 수정하지 않았다.

기존 Assets/Packages/ProjectSettings 파일의 시작 스냅샷과 비교해 변경된 기존 파일은 Player.prefab과 PlayerAttackAnimator.cs 두 개뿐이다. 기존 씬·이동 소스/설정·공격 데이터·Input Actions·패키지·ProjectSettings·기존 meta/GUID는 동일하고 누락 파일은 없다. 전체 git diff --check에서 기존 미커밋 Unity YAML의 빈 필드 후행 공백이 보고되었으나 이번 작업과 무관하므로 고치지 않았다.

## 7. 직접 확인 순서

연결은 이미 완료되어 생성 메뉴를 다시 실행할 필요가 없다. Play를 끈 상태에서 `ETHEREAN > Player > Connect Character Animations` 메뉴를 실행하면 초기 import/클립 설정과 프리팹 연결을 재적용하므로 사용자 튜닝 중에는 주의한다.

연결 완료 후 `Assets/_Project/Scenes/Tests/PlayerTimedComboTest.unity`를 열고 Play한다. 정지 Idle, D/A 가속 중 Walk→Run, 입력 해제 시 감속→Idle, 좌우 반전, Space 점프 중 중립 프레임 유지와 착지 후 재생 복원을 확인한다. 공격 입력 후 기존 준비/타격/후딜이 끝나면 Idle로 복귀해야 한다. 공격 그림은 아직 없다.

PNG Inspector의 Pixels Per Unit=32, Filter Mode=Point, Generate Mip Maps=false, Compression=None를 확인한다. Player 프리팹의 CharacterVisual은 단위 스케일이고 Animator에 PlayerCharacter 컨트롤러가 연결되어야 한다. 기존 이동 수치와 콜라이더는 그대로여야 한다.

## 8. 추가 기록과 정리

추가 파일 `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-character-locomotion-capture.png`는 Unity Camera.Render로 캡처한 256×256 실제 렌더다. 카메라 Size=2에서 32 PPU 스프라이트가 원본 픽셀당 2×2 화면 픽셀로 표시된다. 새 게임 카메라 설정으로 저장한 것이 아니다.

이번 검증에 사용한 `Assets/_Project/Scripts/Editor/CharacterAnimationValidation.cs`, `Assets/_Project/Scripts/Runtime/Player/CharacterAnimationValidationDriver.cs`, 두 meta, Logs의 request/ready 및 `CharacterAnimationTemporary.unity`/meta는 모두 제거했다. 프로젝트 내 상시 테스트 시스템을 추가하지 않았다. 전체 최종 로그는 아래와 같다.

## 9. 리뷰와 다음 단계

사용자가 실제로 움직여 보며 기본 크기, 재생 속도 및 Walk/Run 전환을 검토한다. 키보드에서는 Walk 구간이 짧고 별도 걷기 키는 없다. 이번 작업은 공급된 검 없는 그림만 사용했다. 다음 기능 구현·커밋·push는 하지 않았다.

## 복사한 이미지 전체 경로

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/CharacterBase.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_1.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_10.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_11.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_12.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_13.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_14.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_15.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_16.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_2.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_3.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_4.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_5.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_6.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_7.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_8.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Idle/image_9.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_1.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_10.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_11.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_12.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_13.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_14.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_15.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_16.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_2.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_3.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_4.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_5.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_6.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_7.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_8.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Run/image_9.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_1.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_10.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_11.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_12.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_13.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_14.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_15.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_16.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_2.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_3.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_4.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_5.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_6.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_7.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_8.png`
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Art/Sprites/Player/Walk/image_9.png`

## 최종 Unity 검사 로그

```text
Character animation validation 2026-11-03T18:21:58.4518203+09:00
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\CharacterBase.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_1.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_10.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_11.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_12.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_13.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_14.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_15.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_16.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_2.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_3.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_4.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_5.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_6.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_7.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_8.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Idle\image_9.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_1.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_10.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_11.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_12.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_13.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_14.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_15.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_16.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_2.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_3.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_4.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_5.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_6.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_7.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_8.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Run\image_9.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_1.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_10.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_11.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_12.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_13.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_14.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_15.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_16.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_2.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_3.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_4.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_5.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_6.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_7.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_8.png
EDITOR PASS Pixel import Assets/_Project/Art/Sprites/Player\Walk\image_9.png
EDITOR PASS 49 original PNGs imported
EDITOR PASS Idle sprite-only binding
EDITOR PASS Idle numeric frame order and seamless loop endpoint
EDITOR PASS Walk sprite-only binding
EDITOR PASS Walk numeric frame order and seamless loop endpoint
EDITOR PASS Run sprite-only binding
EDITOR PASS Run numeric frame order and seamless loop endpoint
EDITOR PASS Prefab presenter and Animator connected
EDITOR PASS Collider and weapon lock preserved
EDITOR PASS Rectangle hidden and native sprite scale
SUCCESS EDITOR 59
PLAY PASS Grounded idle selected
PLAY PASS Idle sprite frames advance in engine
PLAY PASS Sustained analog low speed selects Walk
PLAY PASS Walk sprite frames advance in engine
PLAY PASS Actual acceleration selects Walk then Run without changing speed
PLAY PASS Rightward movement faces original art right
PLAY PASS Run sprite frames advance
PLAY PASS Release decelerates back to Idle
PLAY PASS Leftward movement mirrors Run
PLAY PASS Real jump remains functional
PLAY PASS Air holds neutral frame instead of ground running
PLAY PASS Unsupported air animation stays frozen
PLAY PASS Landing restores locomotion playback
PLAY PASS Timed attack input still works
PLAY PASS Missing attack artwork uses neutral fallback without competing playback
PLAY PASS Recovery completion restores Idle
PLAY PASS Disabled movement freezes ground animation
PLAY PASS Movement reenable restores animation
PLAY PASS Runtime collision geometry unchanged
PLAY PASS Actual character render captured
SUCCESS PLAY 20
RESTORED
```
