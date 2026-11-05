# 듀얼 그리드: 직접 지정하는 경사 영역

타일의 외곽 그림을 고르는 기존 듀얼 그리드와 별도로, 실제 충돌의 계단을 경사로로 바꾸는 편집 도구를 추가했다. 적용 범위는 사용자가 Scene 뷰에서 드래그한 셀의 사각형이다. 기존에 칠한 원본 타일은 유지된다.

| 파일 | 위치와 책임 |
| --- | --- |
| `Assets/_Project/Scripts/Editor/DualGridTerrainInspectors.cs` | 기존 `DualGridTerrainInspector.OnInspectorGUI`에 `Draw Slope Region` 버튼을 추가했다. 선택된 지형의 Source Tilemap에 기존 Composite 충돌 구성이 있을 때 사용한다. |
| `Assets/_Project/Scripts/Editor/DualGridSlopeTool.cs` | 새 파일. `OnSceneGUI`에서 셀을 선택하고 `Create`가 선택 범위를 Source의 자식 오브젝트로 저장한다. `Bake`가 해당 범위를 실제 경사 충돌로 바꾼다. `DualGridSlopeRegionInspector`에서 범위 및 방향을 다시 수정할 수 있다. |
| `Assets/_Project/Scripts/Runtime/World/DualGridSlopeRegion.cs` | 새 파일. Inspector의 `Cells`와 `Rises Right`, 생성된 두 콜라이더 참조를 직렬화한다. 런타임에 경사를 계산하지 않는다. |
| `Docs/Reports/2026-11-05-dual-grid-slope-regions.md` | 새 파일. 사용법, 경사 적용 방식과 확인한 범위를 기록한다. |

새 스크립트의 `.meta`도 함께 추가한다. 기존 숲과 사용자가 편집한 샘플 씬 파일은 수정하지 않았다.

## 사용 방법

1. `Open Dual Grid Sample`로 열었던 씬에서 편집할 지형의 **루트**(`Dual Grid Sample` 등)를 선택한다. 자식인 `Terrain Source - Paint Here`를 고르면 버튼이 나오지 않는다.
2. Inspector의 **Draw Slope Region**을 누르고 Scene 뷰에서 높이가 달라지는 바닥 위를 드래그한다. 아래쪽 경계는 낮은 바닥의 윗면, 위쪽 경계는 높은 바닥의 윗면에 맞춘다. 높이 차가 한 칸이면, 폭은 3~4칸부터 시험해 볼 만하다. Esc로 드래그 모드를 취소할 수 있다.
3. 마우스를 놓으면 `Terrain Source - Paint Here`의 자식에 `Slope Region`이 생긴다. 선택하면 Scene에 적용 범위와 경사선이 나온다. Inspector의 **Cells**로 범위를, **Rises Right**로 방향을 바꿀 수 있다. 삭제하면 원래 타일 충돌로 복구된다. 경사끼리 모서리를 맞댈 수 있지만 영역을 겹칠 수는 없다.
4. Play로 점프 없이 양방향으로 걸어본다. 경사 양 끝에서 걸리지 않는지, 바깥쪽의 일반 단차가 그대로인지 본다. 영역을 삭제한 다음 기존 단차가 돌아오는지도 확인한다.

타일 충돌(순서 0)을 기준으로 영역의 `BoxCollider2D`가 해당 사각형을 차집합(10)으로 빼고, 삼각형 `PolygonCollider2D`가 실제 경사를 합집합(20)으로 더한다. 세 콜라이더는 Source의 같은 정적 Rigidbody2D에 속한다. 이 때문에 계단과 경사가 같은 자리에 겹치지 않으며, 경사 밖의 셀은 유지된다. 현재는 한 영역에서 직선 오르막·내리막을 만든다. 타일 외형은 기존 듀얼 그리드가 그대로 표시하므로 Game 뷰에서는 흰 계단 모양과 실제 경사가 다를 수 있다. 청록색 경사선은 Scene 뷰의 편집 안내다.

Unity 6.4.7f1 프로젝트 참조를 사용한 Runtime/Editor C# 컴파일을 통과했다. 컴파일된 코드에서 역방향 드래그의 선택 범위, 오르막·내리막 선택, 두 방향 삼각형의 끝점도 확인했다. 실제 Scene 드래그, CompositeCollider2D 결과, 플레이어 보행은 Unity에서 위 순서로 확인해야 한다.
