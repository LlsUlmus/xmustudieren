. (Join-Path $PSScriptRoot 'common.ps1')
Stop-All; Remove-Item (Join-Path $DEMO '*') -Recurse -Force -ErrorAction SilentlyContinue
$P = Peers 4
Show "1. Start a 4-node Fast Paxos cluster (classic quorum qc=3, fast quorum qf=3) + listener"
1..4 | ForEach-Object { Start-Node $_ 4 fast 1 }
Start-Node 9 4 fast 1
Start-Sleep -Seconds 3
1..4 | ForEach-Object { Node-Log $_ }
Run "& `$PY -m paxos.client --peers $P status"
& $PY -m paxos.client --peers $P net --rtt-ms 10 --loss 0 | Out-Null
$L = Leader 4; $nl = (1..4 | Where-Object { $_ -ne $L })[0]
Show "2. RTT = 10 ms injected. One client on NON-coordinator node $nl : committed in about 1 RTT (fast path = 2 message delays)"
Run "& `$PY -m paxos.client --peers $P --node $nl put k1 v1"
Run "& `$PY -m paxos.client --peers $P --node $nl put k2 v2"
Run "& `$PY -m paxos.client --peers $P --node $nl put k3 v3"
Show "3. Kill the coordinator (node $L): the 3 remaining acceptors are still a fast quorum, so the fast path keeps working"
Kill-Node $L
Run "& `$PY -m paxos.client --peers $P --node $nl put k4 no-coordinator"
Run "& `$PY -m paxos.client --peers $P --node $nl put k5 no-coordinator"
Start-Sleep -Seconds 1
Run "& `$PY -m paxos.client --peers $P status"
Stop-All
Done
