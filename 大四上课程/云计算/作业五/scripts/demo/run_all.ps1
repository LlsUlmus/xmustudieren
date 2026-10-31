# Run every demo step in its own real console window and screenshot each one into report\screenshots\.
#   powershell -ExecutionPolicy Bypass -File scripts\demo\run_all.ps1
$cap = Join-Path $PSScriptRoot 'capture.ps1'
if (-not $env:PAXOS_PYTHON) { $env:PAXOS_PYTHON = 'D:\Miniforge3\python.exe' }
$steps = @(
    @('01_start', 150, 34, 60), @('02_client', 150, 40, 90), @('03_wal', 150, 44, 60),
    @('04_kill_leader', 150, 44, 90), @('05_quorum', 150, 40, 90), @('06_netfault', 150, 34, 120),
    @('07_modes', 150, 30, 240), @('08_fast4', 150, 52, 120), @('09_livelock', 150, 20, 240)
)
foreach ($s in $steps) { & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $cap -Step $s[0] -Cols $s[1] -Lines $s[2] -Timeout $s[3] 2>&1 | Select-Object -Last 1 }
& $env:PAXOS_PYTHON (Join-Path $PSScriptRoot 'crop.py')   # remove title bar, host name and local path from the screenshots
