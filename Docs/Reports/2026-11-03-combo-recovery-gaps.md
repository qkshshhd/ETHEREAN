# 일반 공격 사이 재정비 시간 조정 — 2026-11-03

## 1. 목표와 완료 기준

각 타격 뒤 다음 타격의 준비를 시작하기 전에 회복 시간을 확보한다. 특히2타는 검을 뒤로 보낸 뒤 다시 잡는 시간이 필요하다는 사용자 피드백을 반영한다. 입력은 미리 예약하되 연결 시점 전에는 실행하지 않는다. 실제 애니메이션 리소스 제작은 이번 작업이 아니다. 검증 완료 상태는 마지막 구역을 따른다.

## 2. 수정·추가 파일과 위치

- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GroundAttack.asset — recoveryTime=0.38 유지, comboLinkStart=0.25→0.60. 최소 연결 대기0.095→0.228초.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/AirAttack.asset — 지상1타와 동일한 연결 시점 변경.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/GroundAttack2.asset — recoveryTime=0.34→0.65, comboLinkStart=0.25→0.80. 최소 연결 대기0.085→0.520초.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Data/Player/AirAttack2.asset — 지상2타와 동일한 재정비 시간 변경.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Assets/_Project/Scripts/Editor/PlayerCombatTestSceneBuilder.cs — ApplyComboRecoveryTiming 메뉴/메서드 추가. 기존4개 에셋을 SerializedObject로 조정하고 GUID를 유지해 저장한다. Create에서도 이 초기 튜닝을 적용한다. 이미 적용된 프로젝트에서 생성 메뉴를 다시 실행할 필요는 없다. 이 메뉴는 초기 튜닝값을 다시 적용하므로 이후 사용자 조정은 데이터 Inspector에서 직접 한다.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/AGENTS.md — 현재 리뷰 단위에 최신 타격별 간격과 리뷰 대기 기록.
- C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/PLAYER_SETUP.md — 최신 콤보 재정비 설정과 직접 확인 안내.
- 추가: C:/Users/pjw/OneDrive/Desktop/UnityGithub/ETHEREAN/Docs/Reports/2026-11-03-combo-recovery-gaps.md — 이 보고·시간 측정과 검증 기록. 영구 코드/에셋 추가·삭제·이름 변경은 없다.

이전 유닛의 미커밋 변경과 스테이징 상태는 보존한다. 이번 작업은 기존4개 데이터와 Editor 초기 튜닝 도구를 수정하며 새로운 전투 클래스나 상태를 만들지 않는다.

## 3. 기존 문제와 실제 동작

기존 PlayerAttack.Tick은 Recovery 진행률이 ComboLinkStart 이상이면 예약된 다음 타를 시작한다.2타는0.34초의25%=0.085초만 회복하고3타 준비로 넘어가 재정비가 거의 없었다.

동일한 구조에서 타격별 데이터를 조정했다. 입력→Windup→Active→Recovery를 거치고, 다음 입력이 예약돼 있어도1타는 최소0.228초,2타는 최소0.520초 회복 후 다음 Windup을 시작한다. 기다리는 동안은 Recovery이므로 피해 판정이 없다. 다음 타의 실제 피해는 해당 Windup도 지난 뒤 Active에서 발생한다.

예약이 있는 경우의 이론값:
- 1타 Active 종료→2타 준비 시작0.228초.2타 준비0.12초를 더한 뒤 피해 가능.
- 2타 Active 종료→3타 준비 시작0.520초.3타 준비0.32초를 더한 뒤 피해 가능.
- 3타는 다음 콤보 타격이 없으므로 전체 후딜0.5초를 유지하고 체인을 끝낸다.

## 4. 설계 이유와 트레이드오프

원인은 상태 부족이 아니라 기존 연결 설정이 너무 이른 것이므로 런타임 로직을 늘리지 않았다. 이미 있는 RecoveryTime과 ComboLinkStart를 사용해 중복 타이머나 추가 상태 없이 해결한다. 두 값은 기존 SerializeField라 Inspector에서 조정 가능하다.

2타 전체 회복이0.65초로 길어졌으므로 이동 배율 복구도 그 시간에 맞춰 천천히 진행된다. 점프 캔슬은 기존65%를 유지하므로2타 회복 시작 약0.4225초부터 가능하다. 예약한3타보다 먼저 유효 점프로 취소할 수 있고, 이는 기존 캔슬 정책을 보존한 것이다. 공격 Animator의 전체2타 길이도 바뀌므로 실제 애니메이션을 넣을 때 재정비 구간을 함께 맞춘다. 현재 실제 캐릭터 원화는 미연결이다.

## 5. 검증

새 Editor 직렬화 검사와 실제 Play Mode 가상 키보드/물리 검사를 준비했다. 실제 시간을 측정하여1→2/2→3이 최소 회복 시간을 건너뛰지 않는지, 기다리는 동안 피해가 없는지, 입력 예약·3타 후딜·단발 유지·취소·후반 점프 캔슬을 확인한다. 입력은 테스트용 복제 액션을 전용 가상 키보드에만 바인딩해 실제 사용자 입력 간섭을 차단한다. 기존55개 검사는 이전 실행 근거이며 새 검사 결과로 재사용하지 않는다.

최종 실행 결과·도구 정리·컴파일·보존 검사는 마지막 구역에 기록한다. 컴파일 통과를 실제 타이밍 검증으로 표현하지 않는다.

## 6. 직접 확인

1. Assets/_Project/Scenes/Tests/PlayerTimedComboTest.unity를 열고 Play한다.
2. Enter/왼쪽 클릭으로1타, 타격/회복 중 다시 눌러2타를 예약한다.1타 후 바로 다음 준비로 넘어가지 않아야 한다.
3. 2타에서도 미리3타를 예약한다. 패널의 Combo=2와 Next queued=true가 재정비 중 유지되고 약0.52초 뒤 Combo=3/Windup이 되어야 한다.
4. 이0.52초는3타의 준비0.32초와 별개다. 따라서2타 피해 구간이 끝나고3타 피해가 시작되기까지 약0.84초+물리 틱 오차가 있다.
5. 각 데이터의 Recovery Time과 Combo Link Start를 조절한다. Combo Link Start는0~1의 비율이며 최소 간격=두 값의 곱이다.2타는0.65×0.8이다. 길게 누르기만 해서는 자동 콤보가 이어지지 않는다.

## 7. 임시값과 제한

0.228/0.520초는 초기 튜닝값이며 사용자 조작감 검토 전 확정하지 않는다. 입력 예약/공격 방향/피해/준비·Active 시간·범위·검 잠금·전투/이동 분리는 바꾸지 않았다.3타의 full Recovery도0.5초 그대로다. 판정은 고정 물리 틱이므로 실제 시작은 한 틱 정도 늦을 수 있다. 체감 무게와 실제 애니메이션 품질은 사용자 직접 검토가 필요하다. 아트/VFX·실제 장치·배포 빌드·성능 검증은 이번 작업이 아니다.

## 8. 사용자 리뷰

2타 뒤 재정비가 느껴지는지,0.52초가 지나치게 길지는 않은지 확인한다. 필요하면 데이터만 다시 조정한다. 새 원화나 다른 기능을 만들지 않고 현재 콤보 조작감을 먼저 검토한다.

## 9. 다음 작업과 종료

이 조정 결과를 사용자에게 보여주고 멈춘다. 다음 기능·커밋·푸시는 자동으로 진행하지 않는다.

## 최종 검증 기록

최종 새 Unity 실행은 Editor9개+실제 Play22개=31개 전부 통과했다. 물리 실행에서1→2/2→3 사이 간격을 측정하고 최소 대기를 건너뛰지 않음을 확인했다.2타 설정 최소0.520초에 실제0.540초로, 물리 틱0.020초의 지연이 포함됐다. 이후 타격의 Windup은 별도로 유지한다. 임시 C# 검사2개와 각각의 .meta, ready/request 마커 및 임시 복사 씬을 제거했고 최종24개 C# 소스의 별도 컴파일을 통과했다. 원래 열린 PlayerCombatTest 씬의 clean 상태로 복귀했다. 사용자 물리 장치 조작감·실제 재정비 스프라이트 모션은 미검증이다. 아래는 최종 실행 로그다.

```text
Recovery gap validation / 2026-11-03T00:43:16.9253724+09:00
EDITOR PASS GroundAttack first gap serialized 0.228s
EDITOR PASS GroundAttack second gap serialized 0.520s
EDITOR PASS GroundAttack second attack preparation active damage preserved
EDITOR PASS GroundAttack third recovery unchanged
EDITOR PASS AirAttack first gap serialized 0.228s
EDITOR PASS AirAttack second gap serialized 0.520s
EDITOR PASS AirAttack second attack preparation active damage preserved
EDITOR PASS AirAttack third recovery unchanged
EDITOR PASS Prefab lock and coordinator preserved
PLAY PASS Fresh press starts first attack
PLAY PASS Second input accepted before first recovery finishes
PLAY PASS Queued step 2 eventually starts
PLAY PASS Step 1 cannot bypass configured recovery interval
PLAY PASS No damage during step 1 recovery gap
MEASURED gap after hit1=0.240s, configured minimum=0.228s
PLAY PASS Second preparation starts only after first gap
PLAY PASS Third input accepted while second attack active
PLAY PASS Queued step 3 eventually starts
PLAY PASS Step 2 cannot bypass configured recovery interval
PLAY PASS No damage during step 2 recovery gap
MEASURED gap after hit2=0.540s, configured minimum=0.520s
PLAY PASS Third preparation starts after second regrip
PLAY PASS All three hits still damage once
PLAY PASS Third full 0.5 second recovery retained
PLAY PASS Holding does not restart after final recovery
PLAY PASS New completed-chain press restarts at first hit
PLAY PASS Next hit queued before cancellation
PLAY PASS Cancellation clears pending combo
PLAY PASS Cancelled reservation cannot launch later
PLAY PASS Second recovery permits existing late jump before third link
PLAY PASS Late jump cancels reserved third hit
PLAY PASS Jump-cancelled third does not fire after regrip delay
PLAY PASS Held button remains single attack with new gaps
SUCCESS PLAY 22
RESTORED Assets/_Project/Scenes/Tests/PlayerCombatTest.unity|dirty=False
```

최종 보존 검사: 기존 Assets/Packages/ProjectSettings 파일은 삭제 없이 유지됐으며 변경은 의도한4개 공격 데이터와 Editor 튜닝 도구1개뿐이다. 런타임 코드·씬·프리팹·입력·패키지·설정·기존 .meta는 바뀌지 않았다. 기존 사용자 스테이징 상태도 유지했다.
