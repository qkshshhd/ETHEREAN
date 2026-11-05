# 체크포인트 벤치 스프라이트 적용

사용자가 그린 `C:/Users/pjw/OneDrive/Desktop/ETHEREAN_SPRITE/bench.png`를 기존 체크포인트 프리팹에 연결했다.

| 파일 | 변경 |
| --- | --- |
| Assets/_Project/Art/Sprites/World/Bench.png | 원본 64×42 PNG를 바이트 그대로 복사. |
| Assets/_Project/Art/Sprites/World/Bench.png.meta | Single Sprite, Point, 32 PPU, 무압축, mipmap 없음. 맨 아래 1px 투명 여백을 피벗에 반영해 보이는 다리 하단이 기준점에 맞도록 했다. |
| Assets/_Project/Art/Sprites/World.meta | 월드 스프라이트 폴더 메타데이터. |
| Assets/_Project/Prefabs/World/Checkpoint.prefab | `Visual` 자식에 SpriteRenderer를 추가하고 Bench를 연결. Default / Order 0으로 기존 플레이어 정렬 레이어보다 뒤에 그린다. |

그림은 원본 비율과 픽셀 크기를 유지한다. `Visual`이 표시만 담당하므로 상호작용 범위, `SeatPoint`, `SpawnPoint`, 저장 로직은 기존 값을 유지한다. 별도 착석 모션은 연결하지 않았다.

원본과 복사본의 SHA-256 일치, 스프라이트 GUID와 렌더러 참조, 피벗과 임포트 설정을 확인했다. 이번 변경의 Unity 화면 및 Play Mode는 미검증이다. Unity에서 `Assets/_Project/Scenes/Tests/PlayerCheckpointTest.unity`를 열면 프리팹 인스턴스에 벤치가 반영된다. 벤치 근처에서 E를 눌러 기존 착석 위치와 그림의 겹침을 확인할 수 있다.
