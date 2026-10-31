# Shared helpers for the live demo steps (each step runs in a real console window and is screenshotted).
$ErrorActionPreference = 'Continue'
$env:PYTHONIOENCODING = 'utf-8'
if (-not $env:PAXOS_PYTHON) { $env:PAXOS_PYTHON = 'D:\Miniforge3\python.exe' }
$global:PY = $env:PAXOS_PYTHON
$global:ROOT = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $ROOT
$global:DEMO = Join-Path $ROOT 'run\demo'
New-Item -ItemType Directory -Force $DEMO | Out-Null

function Peers([int]$n) { (1..$n | ForEach-Object { "$_=127.0.0.1:$(7100 + $_)" }) -join ',' }

function Show([string]$text) { Write-Host ""; Write-Host "### $text" -ForegroundColor Cyan }

function Run([string]$cmd) {
    Write-Host "PS> $cmd" -ForegroundColor Yellow
    Invoke-Expression $cmd
}

function Start-Node([int]$id, [int]$n, [string]$mode, [int]$fsync = 1) {
    $a = @('-m', 'paxos.node', '--id', $id, '--peers', (Peers $n), '--listeners', '9=127.0.0.1:7109',
           '--mode', $mode, '--fsync', $fsync, '--data-dir', (Join-Path $DEMO 'data'))
    $cur = Join-Path $DEMO "node-$id.cur.log"      # keep the previous run's log, start a fresh one
    if (Test-Path $cur) { Get-Content $cur | Add-Content (Join-Path $DEMO "node-$id.old.log"); Remove-Item $cur -Force -ErrorAction SilentlyContinue }
    Write-Host ("PS> python -m paxos.node --id $id --peers $(Peers $n) --listeners 9=127.0.0.1:7109 --mode $mode --fsync $fsync") -ForegroundColor Yellow
    $p = Start-Process $PY -ArgumentList $a -PassThru -WindowStyle Hidden `
         -RedirectStandardError (Join-Path $DEMO "node-$id.cur.log")
    $p.Id | Out-File (Join-Path $DEMO "node-$id.pid") -Encoding ascii
}

function Node-Log([int]$id, [string]$filter = '.') {
    $f = Join-Path $DEMO "node-$id.cur.log"
    if (Test-Path $f) {
        Get-Content $f | Where-Object { $_ -match $filter } |
            ForEach-Object { "  [node $id log] " + $_.Substring([Math]::Min(15, $_.Length)) }
    }
}

function Kill-Node([int]$id) {
    $procId = [int](Get-Content (Join-Path $DEMO "node-$id.pid"))
    Write-Host "PS> Stop-Process -Id $procId -Force      # hard kill of node $id (TerminateProcess, like kill -9)" -ForegroundColor Yellow
    Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
}

function Stop-All {
    Get-ChildItem $DEMO -Filter 'node-*.pid' -ErrorAction SilentlyContinue | ForEach-Object {
        Stop-Process -Id ([int](Get-Content $_.FullName)) -Force -ErrorAction SilentlyContinue
        Remove-Item $_.FullName -Force
    }
}

function Leader([int]$n) {
    $out = & $PY -m paxos.client --peers (Peers $n) status
    foreach ($l in $out) { if ($l -match '^node (\d+):.*is_leader=True') { return [int]$Matches[1] } }
    return 0
}

function Bench([string]$peers, [int]$clients = 8, [int]$sec = 3) {
    Write-Host "PS> python -m paxos.client --peers $peers bench --clients $clients --duration $sec" -ForegroundColor Yellow
    $j = (& $PY -m paxos.client --peers $peers bench --clients $clients --duration $sec | Out-String) | ConvertFrom-Json
    "   ok={0} failed={1} throughput={2:N0} req/s  mean={3:N1}ms p50={4:N1}ms p95={5:N1}ms p99={6:N1}ms" -f `
        $j.ok, $j.failed, $j.throughput, $j.mean_ms, $j.p50_ms, $j.p95_ms, $j.p99_ms
}

function Done {
    Write-Host ""
    Write-Host ("[step finished {0} on {1}]" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $env:COMPUTERNAME) -ForegroundColor Green
    'done' | Out-File (Join-Path $DEMO 'step.done') -Encoding ascii
}
