# 듀얼 그리드: 원본 셀 확인과 편집

기존 듀얼 그리드는 반 칸 이동한 표시 격자의 한 칸마다 원본 타일맵의 2×2를 읽는다. 원본 셀에 타일이 있으면 땅, 없으면 공기다. 이번 작업은 이 판정을 Scene 뷰에서 확인하고, 원본 네 칸을 직접 토글할 수 있게 한다. 별도의 땅 데이터를 만들지 않는다.

| 파일 | 변경 위치와 역할 |
| --- | --- |
| `Assets/_Project/Scripts/Editor/DualGridCellPreview.cs` | 새 파일. `Draw`는 마우스 아래 표시 칸을 노란색으로, 이를 결정하는 원본 네 칸을 청록색으로 표시한다. 기존 `GetMask`의 번호와 연결된 타일 이름을 표시한다. 표시 칸을 클릭해 고정한 후, `DrawCorner`의 Ground/Air 버튼으로 Source 셀의 흰 타일을 넣거나 지운다. Undo, 씬 저장 및 기존 타일 갱신 흐름을 사용한다. |
| `Assets/_Project/Scripts/Editor/DualGridTerrainInspectors.cs` | 지형 루트 Inspector에 `Preview Dual Grid` 버튼을 추가했다. |
| `Assets/_Project/Scripts/Editor/DualGridSlopeTool.cs` | 경사 편집과 듀얼 그리드 미리보기가 동시에 Scene 입력을 잡지 않게 전환 처리를 추가했다. 경사 동작은 변경하지 않았다. |
| `Docs/Reports/2026-11-05-dual-grid-cell-preview.md` | 새 작업 기록. 새 스크립트의 `.meta`도 추가됐다. 씬, 런타임, 아트 및 기존 마스크 계산은 변경하지 않았다. |

## Unity 확인

1. 샘플의 지형 루트를 선택하고 `Preview Dual Grid`를 누른다. Scene에서 바닥과 공기가 맞닿는 곳에 마우스를 대면 청록색 원본 4칸과 노란색 표시 1칸이 어긋나 보인다. 아트가 없어도 마스크 번호가 표시된다.
2. 위치를 클릭해 고정한 뒤, 패널의 2×2 `Ground`/`Air` 버튼 중 하나를 누른다. 해당 Source 타일이 바뀌고, 패널의 상태와 번호도 바뀐다. 네 칸이 모두 비면 `00`, 모두 땅이면 `15`다. 이 Source 셀을 공유하는 다른 표시 칸의 판정도 함께 바뀐다.
3. `Ctrl+Z`로 타일을 되돌린다. `Esc`로 미리보기를 닫고, 평소처럼 Tile Palette로 같은 Source 타일맵을 편집할 수 있다. 아트를 배정한 경우 기존 표시 Tilemap 갱신도 확인한다.

Unity 6.4 프로젝트의 참조로 Editor 스크립트를 컴파일했다. Scene 마우스 입력, 버튼, Undo의 실제 UI 동작은 에디터에서 확인해야 한다.
