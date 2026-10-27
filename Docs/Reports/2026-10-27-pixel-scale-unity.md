# Unity 화면 규격 비교 샘플 — 2026-10-27

## 1. 목표·현재 상태

640×360/32 PPU/64×64 프레임 후보를 실제 Unity 카메라로 비교한다. 독립 테스트 씬과 생성/표시 도구, 네 테스트 스프라이트, RT, 실제 렌더 캡처를 완성했다. 새25개 검사(13 Editor+12 실제 Play Mode), 임시 도구 제거 후 최종13개 C# 별도 컴파일과 두 캡처의 시각 검토를 마쳤다. 정확한 주인공 외형·등신·60px 높이·팔레트와 기술 규격은 최종 승인되지 않았다. 기존 플레이어 동작과 프로젝트 설정을 보존한다.

## 2. 추가·수정 파일

| 구분 | 전체 경로 | 위치·책임·연결 |
| --- | --- | --- |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Runtime\Player\PixelScalePreview.cs` | OnEnable validates camera/target references and fixes 16:9; OnGUI displays the render at integer scale and switches camera Size 5.5/5.625 |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Runtime\Player\PixelScalePreview.cs.meta` | Unity-generated asset identity, referenced by the new sample |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Editor\PixelScaleTestSceneBuilder.cs` | Create and menu ETHEREAN/Tests/Create Pixel Scale Review Scene. Editor API creation with overwrite guards; SaveSprite/guide/calibration/world labels |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Editor\PixelScaleTestSceneBuilder.cs.meta` | Unity-generated asset identity, referenced by the new sample |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scenes\Tests\PlayerPixelScaleTest.unity` | Review Camera and PixelScalePreview, linked existing Player instance, ground/walls/platforms, three proportion guides and one-pixel checker |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scenes\Tests\PlayerPixelScaleTest.unity.meta` | Unity-generated asset identity, referenced by the new sample |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Data\Player\PixelScalePreview.renderTexture` | 640x360 camera render target, Point, one sample, no mipmaps/dynamic scale |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Data\Player\PixelScalePreview.renderTexture.meta` | Unity-generated asset identity, referenced by the new sample |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Art\Sprites\PixelScaleTests\Guide2.png` | Procedural monochrome proportion guide in a 64px frame (60px tall), or 32px alternating pixel checker. All use 32 PPU / Point / no compression |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Art\Sprites\PixelScaleTests\Guide2.png.meta` | Unity-generated asset identity, referenced by the new sample |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Art\Sprites\PixelScaleTests\Guide2_5.png` | Procedural monochrome proportion guide in a 64px frame (60px tall), or 32px alternating pixel checker. All use 32 PPU / Point / no compression |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Art\Sprites\PixelScaleTests\Guide2_5.png.meta` | Unity-generated asset identity, referenced by the new sample |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Art\Sprites\PixelScaleTests\Guide3.png` | Procedural monochrome proportion guide in a 64px frame (60px tall), or 32px alternating pixel checker. All use 32 PPU / Point / no compression |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Art\Sprites\PixelScaleTests\Guide3.png.meta` | Unity-generated asset identity, referenced by the new sample |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Art\Sprites\PixelScaleTests\PixelChecker.png` | Procedural monochrome proportion guide in a 64px frame (60px tall), or 32px alternating pixel checker. All use 32 PPU / Point / no compression |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Art\Sprites\PixelScaleTests\PixelChecker.png.meta` | Unity-generated asset identity, referenced by the new sample |
| 추가 | `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Art\Sprites\PixelScaleTests.meta` | New test-sprite folder identity |

- `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\AGENTS.md`: Current review unit: 승인 범위와 결과·미결정 구분
- `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\PLAYER_SETUP.md`: 640×360 실제 비교 샘플: 연결·조작·가독성 한계
- `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Docs\Design\2026-10-27-pixel-scale-review.md`: 실제 Unity 비교 자료 추가: 이전56px/새60px 가이드의 의미
- `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Docs\Plans\ETHEREAN_Daily_Plan_2026-11-08.md`: DAY02와 변경 이력: 샘플 완료, 규격·외형 선택 미완료
- `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Docs\Reports\2026-10-27-pixel-scale-unity.md`: 신규9항목 보고서와 전체 실제 로그
- `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Docs\Reports\2026-10-27-pixel-scale-candidate.png`: 신규 실제 Unity 카메라 Size5.625 원본640×360 캡처
- `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Docs\Reports\2026-10-27-pixel-scale-baseline.png`: 신규 같은 위치의 Size5.5 원본640×360 캡처

기존 파일 삭제/이름 변경은 없다. 임시 검증 파일과 씬은 5절에서 구분한다.

## 3. 실행 흐름과 전후

이전에는 수치·비율 도표만 있었다. 새 생성 메뉴는 기존 자산을 확인하고 네 테스트 PNG를 Texture2D/Editor importer로 만들며 RT와 새 씬을 생성·저장한 뒤 원래 활성 씬으로 돌아온다. 이미 대상이 있으면 덮어쓰지 않는다.

Play에서 Preview.OnEnable이 연결과 화면비를 확인한다. Review Camera는 640×360 RenderTexture에 실제 월드와 기존 Player를 렌더한다. OnGUI는 화면에 맞는 정수 배율을 계산해 중앙에 Point 확대 표시하고 두 버튼으로 같은 월드·카메라 위치에서 Size만 바꾼다. 이미지보다 작은 Game 뷰에서는 1배로 잘라 표시하며 안내한다.

A/D·Space는 기존 NewInputMovementReader→PlayerLocomotion→각 이동 기능→PlayerMovement를 사용한다. 프리팹은 변경하지 않으며 새 씬 인스턴스의 벽 이동 능력만 테스트용으로 켠다.

## 4. 설계 이유·제한

고정 RT는 기준 월드 해상도와 실제 창 크기를 분리한다. 정수 확대와 Point는 확대 단계의 흐림을 줄인다. Pixel Perfect Camera를 설치/구성한 것은 아니며 움직이는 물체의 좌표를 픽셀에 강제 고정하지 않는다. 서브픽셀 이동의 느낌은 직접 검토한다.

Size 5.625이면 세로 시야 11.25u이므로 360/11.25=32px/u다. Size 5.5이면 시야 11u, 약32.727px/u로 조금 확대된다. 두 비교 모두 동일한 640×360 타깃과 동일한 카메라 위치를 사용하며 기존 이동 씬 전체의 카메라 구도를 복제했다는 뜻은 아니다.

세 도형은 같은 60px 높이에서 머리 30/24/20px를 사용해 정확한 2/2.5/3 비율을 보여 준다. 60은 세 비율을 픽셀 정수로 맞추기 위한 비교 수치다. 기존 도표의 56px 역시 임시 가이드였으며 캐릭터 크기를 확정하지 않는다. 실제 Player 충돌체 1.8u는 후보 화면에서57.6px다. 회색 도형·테스트 배경은 주인공 외형/색상이나 최종 숲 아트 선택이 아니다.

## 5. 검증 상태

**실제 새25개 검사 전부 통과: 13 Editor+12 Play Mode, 실패0개.** 전체 실행 로그는 부록에 보존했다. 이전 이동/벽차기 검사를 이번 실행 결과로 재사용하지 않는다.

- Editor: 후보 카메라 Size, 고정 RT 연결/해상도/Point/MSAA, Preview 참조, 기존 프리팹 링크·충돌체·테스트 해금, 네 PNG의32 PPU/Point/압축·밉맵 없음, 세 가이드 존재, 덮어쓰기 방지를 검사했다.
- Play: RT 실제 생성, Preview 활성·참조, 고정16:9, 두 Size에서1.8u가57.6/58.909px로 투영됨, 두 실제 카메라 프레임 저장, 새 바닥 접지, 기존 New Input 이동·감속·점프·짧은 점프를 검사했다. 입력은 가상 Keyboard를 실제 Reader로 전달했고 임시 InputSettings/시간 값을 원복·장치 제거했다.
- 두 실제 Unity 카메라 PNG를 직접 열어 도형·지형·글자·프레임 구성을 확인했다. 캡처는640×360 RT의 실제 월드 렌더이며 정수 확대 후 Game 뷰 전체(UI 포함) 스크린샷과 구분한다.
- PNG 픽셀 검사: Guide2/2_5/3은 모두64×64이며 보이는 알파 높이는60px다. 후보 실제 캡처의 체크무늬32×32 영역에서 흰색/검정 각각512픽셀, 가로·세로 이웃이 모두 교차하는 것을 확인했다. 이는 정지 카메라/정적 가이드의1px 대응 검증이며 움직이는 캐릭터 전체의 픽셀 완벽성을 뜻하지 않는다.
- 작업 전 기록한 기존 Assets 소스·씬·프리팹·.meta 및 ProjectSettings.asset 해시가 그대로였다. 원래 활성 씬과 dirty 상태도 복구됐다. 기존 이동 설정/충돌체/프로젝트 해상도/패키지를 변경하지 않았다.
- 임시 PixelScaleValidationBootstrap.cs, PixelScaleValidationDriver.cs와 각각의 .meta, 요청/준비/중지 파일, 임시 PixelScaleValidationScene은 제거했다. 최종13개 C# 소스의 별도 새 컴파일은 exit0이었다. 기존 Reader2개와 새Preview Inspector참조2개의 CS0649 경고4개를 컴파일 오류와 구분한다.

임시 파일 전체 경로:
- `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Editor\PixelScaleValidationBootstrap.cs` 및 `.meta`
- `C:\Users\pjw\OneDrive\Desktop\UnityGithub\ETHEREAN\Assets\_Project\Scripts\Runtime\Player\PixelScaleValidationDriver.cs` 및 `.meta`

실제 장치 조작감·에디터 Game 뷰의 출력/줌 배율별 시각 품질·이동 중 픽셀 흔들림은 사용자 직접 검토가 남았다. 커밋/푸시는 실행하지 않았다.

## 6. 직접 확인

1. 기존 씬의 미저장 변경을 보존한 상태에서 새 PlayerPixelScaleTest를 단독으로 열고 Play한다.
2. Game 뷰를 실제 1280×720 또는1920×1080으로 설정하고 에디터 Game 뷰 확대율도 확인한다. 640×360 타깃은 각각2배/3배가 된다.
3. 화면 왼쪽 위 View A/B 버튼으로5.5와5.625를 비교한다. 체크무늬는1픽셀 단위, 격자는1u 단위다.
4. A/D·Space로 이동·점프하며 캐릭터 크기·발판 끝·픽셀 흔들림을 확인한다. 세 비율 도형을 최종 주인공 디자인으로 채택하지 않는다.
5. 이미지가 잘리면 Game 뷰를 최소640×360 이상으로 키운다. 기존 생성 파일이 있으므로 생성 메뉴를 다시 누를 필요가 없다.

## 7. 임시값·미검증

640×360,32 PPU,64px 프레임,Size5.625와60px 비율 가이드 모두 검토 후보다. 실제 캐릭터 디자인·대검·애니메이션·VFX·전체 카메라와 프롤로그/숲 이벤트는 별도 승인 단위다. 글로벌 해상도·프로젝트 설정·패키지는 바꾸지 않는다.

## 8. 사용자 검토 지점

Game 뷰에서 캐릭터 점유율과 픽셀 크기가 원하는 느낌인지,5.5/5.625 중 시야가 적절한지 확인한다. 이후 실제 주인공 실루엣은 별도 결정한다.

## 9. 다음 작업 — 실행하지 않음

규격 리뷰와 필요한 샘플 수정 후 사용자가 원할 때 주인공 실루엣·대검 비율을 검토한다. 아트 규격이나 인물 외형을 임의 확정하거나 다음 기능/커밋/푸시로 넘어가지 않는다.

## 부록: 실제 Unity 실행 로그

```text
Pixel scale validation 2026-10-28T00:03:30.8048414+09:00
PASS Editor: candidate camera framing saved at 5.625
PASS Editor: camera connected to fixed 640x360 texture
PASS Editor: preview uses Point filtering and no MSAA
PASS Editor: preview Inspector references connected
PASS Editor: review player is linked to existing prefab
PASS Editor: review collider keeps existing size
PASS Editor: review instance unlocked for wall testing
PASS Editor: Guide2 uses 32 PPU, Point, no mipmaps/compression
PASS Editor: Guide2_5 uses 32 PPU, Point, no mipmaps/compression
PASS Editor: Guide3 uses 32 PPU, Point, no mipmaps/compression
PASS Editor: PixelChecker uses 32 PPU, Point, no mipmaps/compression
PASS Editor: three equal-height proportion samples exist
PASS Editor: generation refuses overwriting existing review assets
PASS Play: actual camera target is created at 640x360
PASS Play: integer scale preview component active with connected references
PASS Play: runtime camera aspect fixed to 16:9
PASS Play: candidate projects 1.8u collision height to 57.6 pixels
PASS Play: captured camera frame has rendered calibration pixels: 2026-10-27-pixel-scale-candidate.png
PASS Play: baseline framing projects 1.8u to 58.909 pixels
PASS Play: captured camera frame has rendered calibration pixels: 2026-10-27-pixel-scale-baseline.png
PASS Play: prefab player grounded on new review floor
PASS Play: existing New Input System and movement work in review scene
PASS Play: existing deceleration stops player in review scene
PASS Play: existing jump works with preview camera rendering to texture
PASS Play: existing short jump remains active
SUCCESS SUMMARY: 13 Editor + 12 Play Mode checks; failures=0
RESTORED: original scene and dirty state match; temporary scene removed
```
