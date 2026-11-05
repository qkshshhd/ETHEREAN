# 숲 B 갈림길: 실제 맵에 타일맵 적용

36u 폭의 B 갈림길에서 본길 바닥과 뿌리 입구 천장을 0.5u 셀의 듀얼 그리드로 옮겼다. A와 이어지는 x14 및 BC로 나가는 x50의 바닥 높이는 y0으로 유지했다. 가운데 길은 y−0.5u로 낮아졌다가 원래 높이로 올라온다. 뿌리 입구는 이전보다 일찍 보이도록 천장이 x45에서 시작하며, 높은 어깨를 지나 x46.5부터 천장 밑면이 y2.5u가 된다.

| 파일 | 변경 위치와 동작 |
| --- | --- |
| `Assets/_Project/Scenes/ForestWhitebox.unity` | `B - Mountain Fork` 아래 `B - Editable Terrain Grid`를 추가했다. `Terrain Source - Paint Here`에 바닥과 천장 636칸을 배치했고, `Terrain Surface - Generated`를 오른쪽·위 0.25u씩 옮겨 기존 듀얼 그리드 도구에 연결했다. 기존 바닥 렌더 블록과 천장 박스를 교체했다. |
| 같은 씬 | `Terrain Source - Paint Here`의 `CompositeCollider2D`가 일반 타일 충돌과 `B - West Ramp Collision`, `B - East Ramp Collision`을 합친다. 완만한 경사 두 곳과 임시 경사 표시는 씬 지형으로 남겨 계단을 오르내릴 필요가 없게 했다. `Fork - Low root shelf`와 `Fork - Upper root path`의 단방향 발판은 유지했다. |
| `Assets/_Project/Scenes/Tests/DualGridTerrainTest.unity` | 지난 작업에서 위로 옮긴 발판의 기존 `CompositeCollider2D` 저장 경로가 낮은 위치에 남아 있어, 발판 세 칸의 충돌 경로도 새 위치와 맞췄다. 샘플 타일 배치는 이번에 다시 바꾸지 않았다. |
| `Docs/Reports/2026-11-05-forest-fork-tilemap.md` | 새 파일. 변경 내용, 편집 위치와 Play 검증 순서를 기록한다. |

숲 씬의 A, BC, C 및 이후 구간, 카메라와 진행 설정, 프리팹 오버라이드는 유지했다. 새 런타임 코드는 없다. 숲 타일 세트가 연결되기 전까지 기존 화이트박스 타일로 표시한다. 경사 두 곳의 실제 경계는 타일이 아니라 경사 콜라이더에 있으므로, 그 부근을 크게 바꿀 때는 경사 오브젝트도 함께 맞춰야 한다.

1. `ForestWhitebox`에서 `B - Mountain Fork > B - Editable Terrain Grid`를 선택한다. `Preview Dual Grid`로 바닥이나 천장에 마우스를 대면 마스크를 볼 수 있다. Tile Palette로 바꾸려면 `Terrain Source - Paint Here`를 선택한다.
2. 16:9 Game 뷰로 Play하고 오른쪽으로 진행한다. A에서 B로 들어와 낮은 본길을 걸어 내려갔다 다시 올라오고, 입구 천장 아래를 지나 BC로 이어지는지 본다.
3. B 가운데의 낮은 뿌리 턱과 위쪽 발판을 점프로 밟아 샛길을 타 본 뒤, 오른쪽에서 본길로 내려온다. 일반 바닥 한 칸을 칠하고 지운 뒤 Undo로 복구해 편집도 확인한다.
4. `DualGridTerrainTest`도 열어 기존에 낮았던 발판 아래를 오른쪽으로 지나가면서 점프한다. 화면에 보이는 위치와 실제 충돌이 맞는지 확인한다.

파일에서 636개 타일 좌표의 중복 여부, A·BC 연결 높이, 천장 여유, 콜라이더 참조, 씬 계층, 카메라·진행 및 프리팹 값 보존을 확인했다. Unity의 실제 임포트와 Play는 별도 검증이 필요하다.
