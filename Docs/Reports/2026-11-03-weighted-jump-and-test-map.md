# 점프 무게감 조정과 테스트 맵 — 2026-11-03

## 1. 목표와 완료 상태

사용자 피드백: 점프가 너무 높고 가볍다. 현재 캐릭터 크기에 맞게 높이와 체공 시간을 줄이고 테스트 맵도 개선한다. 이번 단위는 점프 수치 튜닝과 이를 검토할 이동 테스트 씬이다.

**완료, 사용자 조작감 리뷰 대기.** 실제 Unity Editor API로 프리팹 저장 및 새 씬 생성을 마쳤다. 최종 **28개 검사(15 Editor + 13 실제 Play Mode)**가 통과했고, 640×360 실제 렌더를 확인했다. 임시 도구·meta·요청·복사 씬을 정리한 최종 28개 C# 소스도 별도 컴파일 오류 0을 확인했다.

## 2. 실제 시작 상태와 변경 이유

프리팹에 저장된 실제 값은 jumpSpeed=16, riseGravity=42, fallGravity=48, maxFallSpeed=20, wallJumpVerticalSpeed=16이었다. 이전에 제안한 8.333/21.875/25는 적용되지 않았다. 사용자가 충돌체를 size=(0.4948379,0.88451296), offset=(-0.003912464,-0.4577435)로 수정해 기존 크기 일괄 변환기가 안전 가드에서 중단된 로그를 확인했다. 그 충돌체와 CharacterVisual 위치는 보존한다.

| Inspector 값 | 실제 기존 | 새 초기값 |
| --- | ---: | ---: |
| PlayerJump.jumpSpeed | 16 | 10 |
| PlayerMovement.riseGravity | 42 | 40 |
| PlayerMovement.fallGravity | 48 | 65 |
| PlayerMovement.maxFallSpeed | 20 | 16 |
| PlayerWallJump.wallJumpVerticalSpeed | 16 | 10 |

상승 중력만 단독으로 낮추지 않는다. 발사 속도를 줄여 최고점을 낮추고, 하강 중력을 높여 떨어지는 구간을 짧게 한다. 낮춘 최대 낙하 속도는 긴 추락의 상한이며 이번 일반 점프의 짧은 낙하를 느리게 만드는 값은 아니다. 연속식의 새 예상 높이는 10²/(2×40)=1.25u, 최고점까지는 0.25s이다. 실제 FixedUpdate 결과와 구별한다.

기존 코요테/버퍼/가변 점프·벽차기 보호, 수평 이동, 콤보 후딜·판정·피해, PPU·그림·충돌체는 이번 튜닝으로 변경하지 않는다. 앞선 전체 크기 맞춤 작업은 완료된 것으로 바꾸어 기록하지 않는다.

## 3. 수정한 파일과 위치

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerMovement.cs`: riseGravity/fallGravity/maxFallSpeed 필드 초기값 40/65/16. 실제 수직 계산 로직은 유지.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerJump.cs`: jumpSpeed 초기값 10.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Runtime/Player/Movement/PlayerWallJump.cs`: wallJumpVerticalSpeed 초기값 10.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab`: 위 다섯 Inspector 값만 저장 변경. 사용자 충돌체·시각 위치·수평 및 나머지 수치 유지.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md`, `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md`: 최신 튜닝값·크기 변환 미완료 상태·새 맵 확인 절차 기록.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Design/2026-11-03-current-character-scale.md`, `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-current-character-scale.md`: 이전 값이 적용됐다는 오해를 막는 최신 상태 주석을 추가한다.

## 4. 추가 파일과 책임

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerMovementFeelSceneBuilder.cs`: ApplyTuning은 Editor API로 다섯 저장 값을 변경한다. Create는 별도 이동 리뷰 씬을 만들고 기존 열린 씬을 보존한다. Platform/Label은 테스트용 충돌 지형과 구역 설명을 만든다. 게임 아트나 VFX를 대체하는 기능이 아니다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scenes/Tests/PlayerMovementFeelTest.unity`: 새 리뷰 씬. 기존 Player 프리팹을 사용하며 벽 능력만 테스트용으로 열어 둔다. 기본 프리팹은 잠금 유지.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-weighted-jump-and-test-map.md`: 본 기록.

이전에 만든 임시 CurrentScaleValidation.cs와 CurrentScaleValidationDriver.cs, 각 meta 및 요청/ready는 제거했다. 사용자 충돌체를 전제로 하지 않는 오래된 검사/일괄 변환 요청이 다시 실행되지 않도록 정리한 것이다. 새 임시 HeavyJumpValidation.cs와 HeavyJumpValidationDriver.cs 및 각 meta, request/ready, 검사 복사 씬/meta도 최종 검증 후 제거했다. 영구 런타임 기능은 추가하지 않는다.

## 5. 실제 실행 흐름과 설계

New Input System → PlayerLocomotion의 접지·입력 우선순위 → PlayerJump가 10u/s 발사 → PlayerMovement가 상승 40/하강 65로 속도 계산 → PlayerMovement만 Rigidbody2D에 적용. 해제 시 기존 jumpCutMultiplier=0.5가 남은 상승 속도를 한 번 줄인다. 벽차기도 같은 수직 속도 10을 사용하지만 수평 발사·보호 시간·벽 코요테는 기존 값을 따른다.

무게감은 짧고 낮아진 궤적으로 조정한다. 입력 지연이나 추가 대기 시간을 넣지 않는다. Inspector 수치만 달라지므로 책임 분리와 입력 규칙은 유지한다. 실제 조작감은 사용자 리뷰가 필요하며 새 값은 최종 확정값이 아니다.

## 6. 새 테스트 맵과 직접 확인

씬: `Assets/_Project/Scenes/Tests/PlayerMovementFeelTest.unity`. A/D로 이동, Space를 짧게/길게 눌러 점프한다.

왼쪽: 0.35/0.65/0.95u 발판. 가운데: 1.25u 틈, 낮은 천장, 2u 틈. 오른쪽: 낮은 발판과 벽차기 통로. 왼쪽 통로 벽 하단은 걸어 들어갈 수 있게 비웠다. 아래 안전 바닥과 양옆 경계는 실수로 떨어져 플레이어를 잃지 않도록 한다. 떨어지면 Play를 재시작해 시작 위치로 돌아온다. 자동 리스폰 기능은 추가하지 않는다.

카메라는 5.625 orthographicSize를 유지하며 640×360 / 16:9에서 전체 구간을 확인한다. 현재 32 PPU 원본을 확대하지 않는다. 구역 글자는 개발용 TextMesh이며 최종 픽셀 UI가 아니다.

확인 순서: 평지 긴 점프와 짧은 점프 비교 → 높이가 다른 세 발판 → 달리며 두 틈 통과 → 천장 충돌 후 착지 → 오른쪽 통로 벽차기 → 안전 바닥 낙하 확인.

## 7. 검증

최종 28개 검사 통과: **15 Editor + 13 실제 Play Mode**.

- Editor: 저장된 중력/점프/벽차기 값, 사용자 충돌체/수평 속도/프리팹 잠금/기존 점프 시간 설정 보존, 발판·천장·벽·안전 바닥 구성과 씬의 벽 능력 override 확인.
- 실제 Play: 같은 콜라이더에서 이전 16/42/48/20을 테스트 중 일시 적용해 기준을 측정한 뒤 새 10/40/65/16을 적용해 비교했다. 원본 프리팹을 이전 값으로 되돌려 저장하지 않았다. 실제 Input System 가상 키보드·고정 물리·충돌로 실행했다.
- 새 최고점/체공 단축, 짧은 점프, 공중 재입력의 이중 점프 차단, 실제 천장 충돌 후 착지, 새 통로의 벽 감지·벽차기·수평 보호, 안전 바닥 착지를 확인했다.
- 최종 안내 글자를 크게 수정한 씬을 다시 검증하고 실제 640×360 렌더를 시각 확인했다. 물리 값은 첫 검증과 동일하다.
- 검증 전 열린 씬 목록·활성 씬·dirty 상태를 비교해 `RESTORED`를 확인했다. 생성/검증 도구는 기존 씬에 SaveScene을 호출하지 않았다. 최종 해시 대조에서는 작업 도중 PlayerCombatTest.unity가 별도로 저장된 변화가 관측됐다. 현재 플레이어 acceleration=1000/deceleration=1000 인스턴스 override가 있으며 되돌리거나 새 맵에 복사하지 않았다. 새 맵은 원래 Player 프리팹의 40/60을 사용하므로 두 씬의 수평 조작감은 다를 수 있다.
- 임시 소스/meta/request/ready와 복사 씬/meta를 정리한 최종 **28개 C#** 별도 컴파일 통과, 오류 0. 이 별도 컴파일은 실제 Play 검사 수에 포함하지 않는다.

| 실제 측정 | 기존 값으로 실행 | 새 값으로 실행 |
| --- | ---: | ---: |
| 길게 누른 점프 높이 | 3.2080u | 1.3520u |
| 최고점 관측 시간 | 0.420s | 0.280s |
| 착지까지 시간 | 0.780s | 0.500s |
| 짧게 누른 점프 높이 | 미측정 | 0.4640u |

측정 간격은 0.02s이고, 입력/접지 처리 시점이 포함되므로 연속식 예상 높이·시간과 다르다. 물리 기기 조작감과 레벨 전체 완주, 원래 지상/벽 코요테·점프 버퍼·전투 전체 회귀는 이번에 모두 새로 실행한 것이 아니다. 해당 시간과 코드가 보존됐다는 검사와 과거 실행 기록을 구별한다. 테스트용 추가 씬 진입 시 AudioListener 중복 경고가 있었으며 테스트 중 다른 씬을 비활성화하고 종료 시 복원했다.

추가 캡처: `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-movement-feel-map.png`. UI 글자는 개발 안내이며 픽셀 스프라이트/최종 UI가 아니다.

## 8. 제한과 튜닝

최종 적·실제 레벨·점프/벽/공격 전용 애니메이션은 추가하지 않는다. 새 맵은 이동감 비교용이다. 수평 이동 속도 6은 현재 저장값을 보존한다. 이전 문서의 새 기본값 3.125가 프리팹에 적용됐다고 해석하지 않는다. 플레이어 크기 기준 개발 원칙은 유지한다.

## 9. 리뷰

현재 캐릭터 기준으로 너무 낮거나 착지가 급하게 느껴지는지 검토한다. 새 씬 생성 메뉴는 이미 생성된 씬을 덮어쓰지 않는다. 튜닝 메뉴는 이번 초기값으로 복원하는 도구이므로 Inspector 수동 튜닝 중 반복 실행하지 않는다. 사용자 리뷰 전 다음 기능·커밋·push를 진행하지 않는다.

## 최종 Unity 검사 로그

```text
Character animation validation 2026-11-03T19:20:36.3979534+09:00
EDITOR PASS Saved weighted gravity values
EDITOR PASS Saved ground jump speed
EDITOR PASS Saved matching wall vertical speed
EDITOR PASS User custom collider preserved
EDITOR PASS Actual horizontal tuning preserved
EDITOR PASS Base traversal lock preserved
EDITOR PASS Jump cut and timing windows preserved
EDITOR PASS Map has Step 0.35
EDITOR PASS Map has Step 0.65
EDITOR PASS Map has Step 0.95
EDITOR PASS Map has Low Ceiling
EDITOR PASS Map has Wall Left
EDITOR PASS Map has Wall Right
EDITOR PASS Map has Safety Floor
EDITOR PASS Review traversal override connected
SUCCESS EDITOR 15
MEASURED baseline height=3.2080 apex=0.420 flight=0.780
PLAY PASS New full jump stays near 1.4 character heights
PLAY PASS New full jump is less than half baseline height
PLAY PASS Quick ascent despite lower jump
PLAY PASS Less time floating than actual baseline
MEASURED weighted height=1.3520 apex=0.280 flight=0.500
PLAY PASS Short press retains variable jump height
MEASURED short height=0.4640
PLAY PASS Custom capsule lands at floor
PLAY PASS Airborne repress does not double jump
PLAY PASS Low ceiling stops upward movement
PLAY PASS Ceiling jump lands without sticking
PLAY PASS New corridor wall detected
PLAY PASS Weighted wall jump launches during wall contact
PLAY PASS Wall launch protection retained
PLAY PASS Gap fall reaches safety floor without losing player
SUCCESS PLAY 13
RESTORED
```

## 보존 및 파일 검사

시작 파일 해시와 비교한 직접 변경 대상은 Player.prefab, PlayerMovement.cs, PlayerJump.cs, PlayerWallJump.cs이다. 별도 저장 변화가 관측된 PlayerCombatTest.unity는 보존했다. 제거한 기존 파일은 이전 작업의 임시 CurrentScaleValidation 소스 2개와 meta 2개뿐이다. 다른 기존 에셋·공격 데이터·Input Actions·프로젝트 설정·패키지·meta/GUID는 시작 상태와 동일하다. 사용자 staged PlayerPixelScaleTest의 45줄 추가 상태도 유지했다. 새 영구 파일에는 생성된 Unity meta가 포함된다.
