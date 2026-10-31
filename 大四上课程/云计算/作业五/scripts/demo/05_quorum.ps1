. (Join-Path $PSScriptRoot 'common.ps1')
$P = Peers 3
$L = Leader 3
$f = @(1..3 | Where-Object { $_ -ne $L })
Show "1. Kill 2 of 3 nodes (both followers): the surviving leader (node $L) has no majority any more"
Kill-Node $f[0]; Kill-Node $f[1]
Show "2. A write cannot be chosen now: the client times out (liveness is lost, safety is kept)"
Run "& `$PY -m paxos.client --peers $P --node $L put no-quorum x --timeout 3"
Run "& `$PY -m paxos.client --peers $P status"
Show "3. Restart one node: 2 of 3 is a majority again, writes succeed"
Start-Node $f[0] 3 multi 1
Start-Sleep -Seconds 2
Run "& `$PY -m paxos.client --peers $P --node $L put quorum-back yes --timeout 5"
Show "4. Restart the last node; every node converges to the same prefix"
Start-Node $f[1] 3 multi 1
Start-Sleep -Seconds 3
Run "& `$PY -m paxos.client --peers $P,9=127.0.0.1:7109 status"
Run "& `$PY -m paxos.client --peers $P --node $($f[1]) get quorum-back"
Done
