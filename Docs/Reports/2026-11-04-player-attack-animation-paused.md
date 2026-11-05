# 몸 공격 애니메이션 재생 보류

사용자가 몸 공격 애니메이션 개발을 나중으로 미루기로 했다. 기존 이미지3장·Attack1.anim·Controller의 Attack1 상태와 제작 도구를 보존하고, 게임에서 몸 공격 애니메이션을 실행하는 컴포넌트만 껐다.

## 변경 파일

- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Prefabs/Player/Player.prefab`: 루트 `PlayerAttackAnimator` 컴포넌트의 `m_Enabled`를1에서0으로 변경. 다른 프리팹 필드는 수정하지 않았다.
- `C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md`, `PLAYER_SETUP.md`: 몸 공격 애니메이션 보류 및 수동 재활성화 방법 기록.
- 이 보고서를 추가했다. 새 C# 코드·이미지·클립은 추가하거나 삭제하지 않았다.

## 동작

PlayerAttackAnimator가 비활성화되어 Attack1 상태를 재생하지 않는다. 기존 PlayerCharacterAnimator는 비활성 공격 어댑터에 표시 제어를 넘기지 않고, 공격 중에는 이전 방식대로 기본 Idle 자세를 유지한다. 이동할 때의 Idle/Walk/Run과 별도 PlayerSwordMotion, 입력·콤보·준비/판정/후딜·피해 처리는 기존 코드로 유지된다.

이후 작업을 재개할 때는 Player.prefab Inspector에서 **PlayerAttackAnimator 컴포넌트 이름 왼쪽의 체크박스**를 켜면 된다. CharacterVisual의 Animator나 PlayerAttack/PlayerSwordMotion을 끄는 것이 아니다. 제작 메뉴는 이 체크박스를 자동으로 켜지 않는다.

## 확인

파일 비교로 프리팹의 해당 enabled 필드 한 곳만 바뀐 것을 확인했다. 이미지3장 및 meta, Attack1.anim, PlayerCharacter.controller는 바이트가 같다. 저장 씬에 해당 컴포넌트의 활성 상태를 덮어쓰는 override가 없는 것도 확인했다. Runtime/Editor 코드는 수정하지 않았으며 새 Play 검사는 실행하지 않았다. 이전76개 검사는 애니메이션 활성 상태의 과거 검사로 구분한다.

Unity에서 Play 중이었다면 종료 후 다시 실행하여 공격을 입력한다. 몸은 Attack1의 세 자세로 바뀌지 않고 검의 기존3연타와 피해 판정은 동작해야 한다. 수동으로 별도 활성 override를 만든 새 인스턴스가 있다면 그 인스턴스에서도 PlayerAttackAnimator를 꺼야 한다.

이번 보류 설정에서 멈춘다. 다음 기능·커밋·푸시는 진행하지 않았다.
