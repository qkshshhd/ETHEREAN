[CmdletBinding()]
param([switch]$Push)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [Console]::OutputEncoding
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$expectedHead = '527bd833e42adc9dc0e7ea6a5cfbf942acf56f44'
$gitExe = 'C:\Users\pjw\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\git\cmd\git.exe'
if (-not (Test-Path -LiteralPath $gitExe)) {
    $command = Get-Command git -CommandType Application -ErrorAction SilentlyContinue
    if ($null -eq $command) { throw 'Git executable not found.' }
    $gitExe = $command.Source
}

function Invoke-ProjectGit {
    param([string[]]$GitArguments)
    & $gitExe -C $projectRoot @GitArguments
    if ($LASTEXITCODE -ne 0) { throw ('Git failed: ' + ($GitArguments -join ' ')) }
}

$featureFiles = @(
    'Assets/_Project/Prefabs/Player/Player.prefab'
    'Assets/_Project/Scenes/Tests/PlayerMovementTest.unity'
    'Assets/_Project/Scenes/Tests/PlayerWallSlideTest.unity'
    'Assets/_Project/Scenes/Tests/PlayerWallSlideTest.unity.meta'
    'Assets/_Project/Scripts/Editor/PlayerMovementTestSceneBuilder.cs'
    'Assets/_Project/Scripts/Runtime/Player/PlayerMovement.cs'
    'Assets/_Project/Scripts/Runtime/Player/PlayerWallSensor.cs'
    'Assets/_Project/Scripts/Runtime/Player/PlayerWallSensor.cs.meta'
    'Assets/_Project/Scripts/Runtime/Player/PlayerTraversalAbilities.cs'
    'Assets/_Project/Scripts/Runtime/Player/PlayerTraversalAbilities.cs.meta'
    'Docs/Reports/2026-10-26-wall-slide.md'
    'PLAYER_SETUP.md'
)
$planningFiles = @(
    'AGENTS.md'
    'Docs/Design/2026-10-26-wall-movement.md'
    'Docs/Design/2026-10-26-art-and-prologue-proposal.md'
    'Docs/Design/2026-10-26-prologue-storyboard.md'
    'Docs/Plans/ETHEREAN_Daily_Plan_2026-11-08.md'
    'Docs/Plans/ETHEREAN_Asset_Production_List.md'
    'Docs/Tools/CommitWallSlide.ps1'
)

$head = (Invoke-ProjectGit -GitArguments @('rev-parse','HEAD') | Out-String).Trim()
if ($head -ne $expectedHead) {
    throw 'HEAD changed. Inspect git log before running this one-time commit helper. If the commits already succeeded and only push failed, use git push origin main.'
}
$staged = @(Invoke-ProjectGit -GitArguments @('diff','--cached','--name-only'))
if ($staged.Count -gt 0) { throw 'The index already contains staged work. Review it before using this helper; nothing was unstaged.' }
$branchName = (Invoke-ProjectGit -GitArguments @('branch','--show-current') | Out-String).Trim()
if ([string]::IsNullOrWhiteSpace($branchName)) { throw 'A branch must be checked out.' }
if ($Push) {
    $remotes = @(Invoke-ProjectGit -GitArguments @('remote'))
    if ($remotes -notcontains 'origin') { throw 'origin remote is not configured.' }
}
foreach ($relative in ($featureFiles + $planningFiles)) {
    if (-not (Test-Path -LiteralPath (Join-Path $projectRoot $relative) -PathType Leaf)) { throw ('Missing file: ' + $relative) }
}
foreach ($temporary in @('Assets/_Project/Scripts/Editor/WallSlideValidation.cs','Assets/_Project/Scripts/Editor/WallSlideValidation.cs.meta','Temp/WallSlideValidation.request','Assets/_Project/Scenes/Tests/WallSlideValidation_TEMP.unity')) {
    if (Test-Path -LiteralPath (Join-Path $projectRoot $temporary)) { throw ('Temporary validation file remains: ' + $temporary) }
}
Invoke-ProjectGit -GitArguments @('var','GIT_AUTHOR_IDENT') | Out-Null
Invoke-ProjectGit -GitArguments @('var','GIT_COMMITTER_IDENT') | Out-Null

function Commit-SelectedFiles {
    param([string[]]$Files,[string]$Title,[string]$Body)
    Invoke-ProjectGit -GitArguments (@('add','--') + $Files)
    $actualStaged = @(Invoke-ProjectGit -GitArguments @('diff','--cached','--name-only'))
    if ($actualStaged.Count -eq 0) { throw ('Nothing to commit: ' + $Title) }
    foreach ($path in $actualStaged) {
        if ($Files -notcontains $path) { throw ('Unexpected staged file; inspect before committing: ' + $path) }
    }
    Write-Host ('Committing ' + $actualStaged.Count + ' selected files.')
    Invoke-ProjectGit -GitArguments @('commit','-m',$Title,'-m',$Body)
    Invoke-ProjectGit -GitArguments @('log','-1','--oneline')
}

Commit-SelectedFiles -Files $featureFiles -Title 'feat: 능력 해금 기반 벽 미끄러짐 구현' -Body '벽 센서와 기본 잠금 능력을 분리하고 PlayerMovement에서 낙하 속도를 제한한다. 프리팹, 별도 테스트 씬과 기존 씬의 저장된 테스트 설정을 포함한다. Unity 66개 검사 기록과 최종 별도 컴파일 결과를 보존하고 임시 검증기를 제거한다.'
Commit-SelectedFiles -Files $planningFiles -Title 'docs: 개발 일정과 아트 및 프롤로그 기획 정리' -Body '일일 실행 계획과 리소스 제작 목록, 9장면 프롤로그 및 협업 보고 규칙을 기록한다. 숲 B 분위기와 소녀의 2~3등신만 확정하고 예시 외형은 제외한다. 균열은 작은 배경 요소로 두고 집에서 물건 부재 인지 후 재앙이 시작되는 정보 공개 원칙을 반영한다.'

if ($Push) {
    Invoke-ProjectGit -GitArguments @('push','--set-upstream','origin',$branchName)
    Write-Host 'Two commits created and pushed to origin.'
} else {
    Write-Host 'Two local commits created. To upload them, run git push origin main with the Git executable above.'
}
Invoke-ProjectGit -GitArguments @('status','--short')