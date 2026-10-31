. (Join-Path $PSScriptRoot 'common.ps1')
Stop-All; Remove-Item (Join-Path $DEMO '*') -Recurse -Force -ErrorAction SilentlyContinue
Show "1. Start 3 voting nodes + 1 listener (Multi-Paxos, fsync ON): one OS process per node, TCP 127.0.0.1:7101-7103 / 7109"
1..3 | ForEach-Object { Start-Node $_ 3 multi 1 }
Start-Node 9 3 multi 1
Start-Sleep -Seconds 3
Show "2. Node logs (stderr): startup, quorum sizes, leader elected by a range Prepare"
1, 2, 3, 9 | ForEach-Object { Node-Log $_ }
Show "3. The node processes and their listening TCP ports"
Run "Get-NetTCPConnection -State Listen -LocalPort 7101,7102,7103,7109 | Sort-Object LocalPort | Format-Table LocalAddress,LocalPort,OwningProcess -AutoSize | Out-String | ForEach-Object Trim"
Show "4. Cluster status through the client"
Run "& `$PY -m paxos.client --peers $(Peers 3) status"
Done
