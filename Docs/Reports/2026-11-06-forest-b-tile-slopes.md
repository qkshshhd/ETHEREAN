# 숲 B 갈림길: 경사도 타일로 편집

이전 B 변환은 평지와 천장만 타일맵이었고, 경사에 별도의 표시 오브젝트와 콜라이더가 남았다. 이를 제거하고, 경사의 표시와 충돌을 같은 Source Tilemap 타일에 넣었다. 계단 없이 연결되는 높이는 0.5u이며, 한 경사는 폭 2u(0.5u 셀 네 칸)로 잡았다. 양 끝의 평지 높이와 샛길 위치는 유지했다.

| 파일 | 변경 위치와 역할 |
| --- | --- |
| `Assets/_Project/Scenes/ForestWhitebox.unity` | `B - Mountain Fork > B - Editable Terrain Grid > Terrain Source - Paint Here`에 하강 경사 x20~22, 상승 경사 x37~39를 타일로 배치했다. `B - West Ramp Collision`, `B - East Ramp Collision`과 경사 표시용 Rim 오브젝트 3개를 제거했다. Source의 `TilemapCollider2D`만 `CompositeCollider2D`에 지형 충돌을 공급한다. |
| `Assets/_Project/Data/World/DualGrid/WhiteboxSlopeDown01.asset` ~ `WhiteboxSlopeDown04.asset` | 새 흰색 하강 타일 4개. `01`은 왼쪽의 높은 땅, `04`는 오른쪽의 낮은 땅에 닿는다. |
| `Assets/_Project/Data/World/DualGrid/WhiteboxSlopeUp01.asset` ~ `WhiteboxSlopeUp04.asset` | 새 흰색 상승 타일 4개. `01`은 왼쪽의 낮은 땅, `04`는 오른쪽의 높은 땅에 닿는다. |
| 같은 폴더의 `WhiteboxSlopeDown01.png` ~ `WhiteboxSlopeUp04.png` 및 `.meta` | 8개의 흰색 블록 형상 스프라이트와 임포트 데이터. 32×32, 64 PPU, Point 필터다. 임포트 데이터에 타일 모서리에 정확히 닿는 물리 외곽선을 넣었다. 각 타일 에셋은 `Collider Type: Sprite`로 이 외곽선을 사용한다. 최종 숲 아트가 아니다. |
| `Docs/Reports/2026-11-06-forest-b-tile-slopes.md` | 새 작업 기록. |

타일을 지우면 해당 칸의 그림과 충돌이 함께 빠진다. 네 조각을 순서대로 옮겨 심으면 다른 곳에 같은 경사를 만들 수 있다. 기존 듀얼 그리드의 땅/공기 마스크 계산은 그대로다. 흰색으로 검토 중인 B는 Source 그림을 그대로 표시한다. 최종 숲 타일 세트를 연결할 때는 경사 표시용 아트가 별도로 필요하다. 기존 15개 땅/공기 마스크는 경사 타일 내부의 높이를 표현하지 않는다.

1. `ForestWhitebox`에서 B에 들어가 경사 두 곳을 점프 없이 양방향으로 왕복한다. 평지와 경사가 맞닿는 곳과 경사 타일 사이에서 걸리거나 튀지 않는지 본다.
2. Play를 종료하고 `Terrain Source - Paint Here`에서 경사 한 칸을 지운다. 그림과 경사 충돌이 함께 바뀌는지 확인하고 Undo로 되돌린다.
3. Tile Palette에서 `WhiteboxSlopeUp01`~`04`를 왼쪽부터 칠하고 양 끝을 0.5u 차이로 맞춰 같은 경사를 만들어 본다. 하강도 `Down01`~`04`를 왼쪽부터 사용한다.

저장된 셀과 타일 에셋 참조, 네 조각이 만나는 좌표, 통합 충돌 경로와 별도 오브젝트 제거를 확인했다. 새 런타임 코드는 없다. Unity 임포트와 실제 Play는 별도 검증이 필요하다.
