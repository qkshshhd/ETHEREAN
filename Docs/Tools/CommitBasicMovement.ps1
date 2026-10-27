Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$expectedHead = '1196a6f6c8c944bc157da67465ea36b5fb0826f0'
$expectedBranch = 'main'
$head = (& git -C $repoRoot rev-parse HEAD)
if ($LASTEXITCODE -ne 0) { throw 'Cannot read the ETHEREAN repository.' }
$branch = (& git -C $repoRoot branch --show-current)
if ($LASTEXITCODE -ne 0) { throw 'Cannot read the current branch.' }
if ($head.Trim() -ne $expectedHead -or $branch.Trim() -ne $expectedBranch) {
    throw 'HEAD or branch changed. Review the file list before committing.'
}
$staged = @(& git -C $repoRoot diff --cached --name-only)
if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect the staging area.' }
if ($staged.Count -gt 0) { throw 'Existing staged changes found. Review them before running this helper.' }
$files = @(
    'Assets/InputSystem_Actions.inputactions',
    'AGENTS.md',
    'PLAYER_SETUP.md',
    'Assets/_Project/Art/Materials/MovementPlaceholder.mat',
    'Assets/_Project/Art/Materials/MovementPlaceholder.mat.meta',
    'Assets/_Project/Art/Sprites/MovementSquare.png',
    'Assets/_Project/Art/Sprites/MovementSquare.png.meta',
    'Assets/_Project/Data/Player/PlayerJumpAction.asset',
    'Assets/_Project/Data/Player/PlayerJumpAction.asset.meta',
    'Assets/_Project/Data/Player/PlayerMoveAction.asset',
    'Assets/_Project/Data/Player/PlayerMoveAction.asset.meta',
    'Assets/_Project/Data/Player/PlayerMovement.physicsMaterial2D',
    'Assets/_Project/Data/Player/PlayerMovement.physicsMaterial2D.meta',
    'Assets/_Project/Prefabs/Player/Player.prefab',
    'Assets/_Project/Prefabs/Player/Player.prefab.meta',
    'Assets/_Project/Scenes/Tests/PlayerMovementTest.unity',
    'Assets/_Project/Scenes/Tests/PlayerMovementTest.unity.meta',
    'Assets/_Project/Scripts/Editor/PlayerMovementTestSceneBuilder.cs',
    'Assets/_Project/Scripts/Editor/PlayerMovementTestSceneBuilder.cs.meta',
    'Assets/_Project/Scripts/Runtime/Player/NewInputMovementReader.cs',
    'Assets/_Project/Scripts/Runtime/Player/NewInputMovementReader.cs.meta',
    'Assets/_Project/Scripts/Runtime/Player/PlayerGroundSensor.cs',
    'Assets/_Project/Scripts/Runtime/Player/PlayerGroundSensor.cs.meta',
    'Assets/_Project/Scripts/Runtime/Player/PlayerMovement.cs',
    'Assets/_Project/Scripts/Runtime/Player/PlayerMovement.cs.meta',
    'Assets/_Project/Scripts/Runtime/Player/PlayerMovementInput.cs',
    'Assets/_Project/Scripts/Runtime/Player/PlayerMovementInput.cs.meta',
    'Docs/References/ETHEREAN_Codex_Collaboration_Prompt.md',
    'Docs/References/ETHEREAN_GDD.md',
    'Docs/Reports/2026-10-22-horizontal-movement-validation.md',
    'Docs/Reports/2026-10-22-player-acceleration.md',
    'Docs/Reports/2026-10-23-ground-and-jump.md',
    'Docs/Reports/2026-10-25-jump-buffer.md',
    'Docs/Reports/2026-10-25-variable-jump.md'
)
foreach ($file in $files) {
    if (!(Test-Path -LiteralPath (Join-Path $repoRoot $file) -PathType Leaf)) {
        throw ('Missing commit file: ' + $file)
    }
}
& git -C $repoRoot add -- $files
if ($LASTEXITCODE -ne 0) { throw 'Staging failed. No commit was attempted.' }
& git -C $repoRoot diff --cached --check
if ($LASTEXITCODE -ne 0) { throw 'Staged whitespace validation failed. No commit was attempted.' }
& git -C $repoRoot diff --cached --stat
if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect the staged changes.' }
& git -C $repoRoot commit -m 'feat: 플레이어 기초 이동 및 점프 구현' -m '좌우 가속·감속, 바닥 감지, 가변 점프, 코요테 타임과 점프 버퍼를 구현한다. 테스트 씬·프리팹·입력 보정과 협업 기준 및 검증 기록을 포함한다.'
if ($LASTEXITCODE -ne 0) { throw 'Commit failed. Staged files were left intact for review.' }
& git -C $repoRoot log -1 --oneline
& git -C $repoRoot status --short