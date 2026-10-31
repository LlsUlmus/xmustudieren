. (Join-Path $PSScriptRoot 'common.ps1')
$P = Peers 3
Show "1. Before the fault"
Run "& `$PY -m paxos.client --peers $P status"
$L = Leader 3
$other = (1..3 | Where-Object { $_ -ne $L })[0]
Show "2. Hard-kill the leader (node $L), then immediately write through node $other"
$t0 = Get-Date
Kill-Node $L
Run "& `$PY -m paxos.client --peers $P --node $other put after-kill ok"
Write-Host ("   wall time from the kill to the committed write, client process start-up included: {0:N0} ms" -f ((Get-Date) - $t0).TotalMilliseconds)
Show "3. A new leader with a higher ballot took over; the killed node is unreachable"
Run "& `$PY -m paxos.client --peers $P status"
1..3 | Where-Object { $_ -ne $L } | ForEach-Object { Node-Log $_ 'becomes leader|steps down' }
Show "4. Restart node $L : it replays its WAL and rejoins. Election-related log lines since the kill:"
Start-Node $L 3 multi 1
Start-Sleep -Seconds 3
1..3 | Where-Object { $_ -ne $L } | ForEach-Object { Node-Log $_ 'becomes leader|steps down' } | Select-Object -Last 3
Node-Log $L
Run "& `$PY -m paxos.client --peers $P status"
Run "& `$PY -m paxos.client --peers $P --node $L get after-kill"
Done
