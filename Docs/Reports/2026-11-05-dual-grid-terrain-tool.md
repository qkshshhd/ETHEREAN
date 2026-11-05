# 숲 제작 도구: 듀얼 그리드 타일맵

기존 숲 씬의 지형을 교체하기 전에, 셀을 칠하고 지우는 것만으로 지형을 수정할 수 있는 Unity 편집 도구와 독립 샘플 생성 메뉴를 추가했다.

| 추가한 파일 | 위치와 책임 |
| --- | --- |
| `Assets/_Project/Scripts/Runtime/World/DualGridTerrain.cs` | `DualGridTerrain`은 편집용 Tilemap, 표시용 Tilemap, 타일 세트의 참조를 씬에 저장한다. 런타임 갱신은 없다. |
| `Assets/_Project/Scripts/Runtime/World/DualGridTileSet.cs` | `DualGridTileSet` 에셋의 `tilesByMask`에 코너별 타일을 저장한다. 0번은 공기, 1~15번은 사용자가 배정하는 타일이다. |
| `Assets/_Project/Scripts/Editor/DualGridTerrainBaker.cs` | `QueueChanges`/`Flush`는 칠하거나 지운 셀에 접한 표시 셀만 다시 계산한다. `Rebuild`는 에셋 변경, Undo/Redo, 수동 재생성에 사용하며 더 이상 필요 없는 표시 타일도 지운다. |
| `Assets/_Project/Scripts/Editor/DualGridTerrainSetup.cs` | `Create Dual Grid Terrain`은 현재 씬에 편집과 표시 레이어를 만든다. `Open Dual Grid Sample`은 최초 실행 시에만 독립 씬을 만들며, 이후에는 저장된 씬을 다시 연다. 두 메뉴 모두 기존 프로젝트의 흰 사각형을 가져와 편집용 브러시를 한 번만 만든다. |
| `Assets/_Project/Scripts/Editor/DualGridTerrainInspectors.cs` | `DualGridTerrainInspector`는 칠할 레이어 선택과 `Rebuild Surface`를 제공한다. `DualGridTileSetInspector`는 위/아래 각 두 코너를 표시해 타일을 지정할 자리를 알려준다. |
| `Docs/Reports/2026-11-05-dual-grid-terrain-tool.md` | 사용 순서, 타일 배치 규약과 검증 범위를 기록한다. |

스크립트에 대응하는 Unity `.meta` 파일도 추가됐다. 기존 씬, 캐릭터, 입력, 물리 설정 및 패키지는 수정하지 않았다.

## 동작 방식

1. Tile Palette로 `Terrain Source - Paint Here`에 `WhiteboxSolid`를 칠한다. 흰 타일이 있는 칸이 곧 바닥이다. 이 Tilemap의 `TilemapCollider2D`가 충돌을 만들고 `CompositeCollider2D`가 이웃 칸의 충돌을 합친다.
2. `Terrain Surface - Generated`는 오른쪽과 위로 각각 반 칸 이동해 있다. 표시 칸 하나는 아래왼쪽, 아래오른쪽, 위왼쪽, 위오른쪽 네 원본 칸을 읽어, 채워진 칸의 값 1·2·4·8을 더한 마스크를 사용한다. 예를 들어 위쪽 두 칸만 채워졌다면 12번 타일을 쓴다. 대각선만 채워진 6번과 9번도 각각 독립 타일이다.
3. 타일 세트에 15개를 모두 채우면 편집용 사각형의 표시만 숨긴다. 충돌은 그대로 유지된다. 편집 중 변경을 반영하고, 저장 시 예약된 갱신을 마저 처리한다. 런타임에 타일을 다시 계산하지 않는다.

초기 셀은 0.5×0.5 Unity 유닛이다. 현재 32 PPU 기준 표시용 타일은 16×16픽셀, 중앙 피벗, Point 필터를 기준으로 준비한다. 편집용 흰 사각형은 기존 32×32 파일을 복제해 64 PPU로 임포트한다. 듀얼 그리드는 가장자리의 **그림**을 연결한다. 부드러운 경사 **충돌**은 이 도구의 정사각형 셀 충돌과 별개라, 필요한 장소의 실제 경사는 이후 지정해서 만들 수 있다. 붙어 있는 방의 지형은 같은 Source Tilemap을 공유해야 방 경계에서도 자동으로 이웃을 읽는다.

## Unity에서 확인

1. `ETHEREAN > Level Design > Open Dual Grid Sample`을 실행한다. 최초 실행 시 `Assets/_Project/Scenes/Tests/DualGridTerrainTest.unity`와 `Assets/_Project/Data/World/DualGrid/WhiteboxSolid.asset` 및 흰 스프라이트 복제본이 생성된다.
2. `Window > 2D > Tile Palette`에서 사각형 팔레트를 만들고 `WhiteboxSolid.asset`을 끌어 넣는다. `Dual Grid Sample` Inspector의 `Select Paint Target`을 눌러 `Terrain Source - Paint Here`를 선택한다. 팔레트의 Active Tilemap도 같은 레이어인지 확인한다. 타일을 칠하고 지운 뒤 Undo/Redo를 해 본다.
3. 타일 세트 없이 Play하면 흰색 바닥으로 나온다. 캐릭터가 이어진 바닥 위에서 움직이고, 셀 두 칸을 비운 1유닛 틈과 단차에서 점프할 수 있는지 본다. 바닥을 지우고 다시 Play해 충돌도 지워지는지 확인한다.
4. 표시 타일을 준비했다면 `Create > ETHEREAN > Dual Grid Tile Set`으로 에셋을 만든다. Inspector의 코너 모양에 맞춰 기본 `Tile` 에셋 15개를 채우고 루트의 `Tile Set`에 지정한다. 한 칸을 칠하거나 지울 때 외곽이 갱신되고, 저장 후 다시 열어도 표시와 충돌이 일치하는지 확인한다.

Unity 6.4.7f1 프로젝트의 참조를 사용한 Runtime/Editor C# 컴파일과 코너 마스크 16개를 확인했다. 이 환경의 Unity 배치 프로세스는 프로젝트 초기화 단계에서 종료됐다. 실제 Tile Palette 편집과 샘플 생성, Play 검증은 Unity에서 위 순서로 확인해야 한다. 표시용 픽셀 아트는 제작하지 않았다. 검토 후 숲의 B 갈림길부터 적용할 수 있다.
