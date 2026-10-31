# Windows 单机起本地集群。  .\scripts\start_local.ps1 -N 3 -Mode multi -Fsync 0
param([int]$N = 3, [string]$Mode = "multi", [int]$Fsync = 0, [string]$Python = "python")
Set-Location (Join-Path $PSScriptRoot "..")
New-Item -ItemType Directory -Force run\local | Out-Null
$peers = (1..$N | ForEach-Object { "$_=127.0.0.1:$(7000 + $_)" }) -join ","
foreach ($i in (1..$N) + 9) {
    $p = Start-Process $Python -PassThru -WindowStyle Hidden -RedirectStandardError "run\local\node-$i.log" `
        -ArgumentList "-m", "paxos.node", "--id", $i, "--peers", $peers, "--listeners", "9=127.0.0.1:7009", `
                      "--mode", $Mode, "--fsync", $Fsync, "--data-dir", "run\local\data"
    $p.Id | Out-File "run\local\node-$i.pid" -Encoding ascii
}
"PEERS=$peers"
"查看状态: $Python -m paxos.client --peers $peers status"
"停止:     Get-Content run\local\*.pid | ForEach-Object { Stop-Process -Id `$_ -Force }"
