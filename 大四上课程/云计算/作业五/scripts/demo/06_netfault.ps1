. (Join-Path $PSScriptRoot 'common.ps1')
$P = Peers 3
Show "1. Baseline on loopback (no injected delay), 8 closed-loop clients, fsync ON"
Bench $P 8 3
Show "2. Inject RTT = 10 ms on every node at runtime (control message), same load"
Run "& `$PY -m paxos.client --peers $P net --rtt-ms 10 --loss 0"
Bench $P 8 3
Show "3. RTT = 10 ms + 5% message loss"
Run "& `$PY -m paxos.client --peers $P net --rtt-ms 10 --loss 0.05 | Out-Null"
Bench $P 8 3
Show "4. RTT = 50 ms + 10% message loss"
Run "& `$PY -m paxos.client --peers $P net --rtt-ms 50 --loss 0.10 | Out-Null"
Bench $P 8 4
Show "5. Remove the faults; all learners agree, violations=0"
Run "& `$PY -m paxos.client --peers $P net --rtt-ms 0 --loss 0 | Out-Null"
Start-Sleep -Seconds 2
Run "& `$PY -m paxos.client --peers $P,9=127.0.0.1:7109 status"
Stop-All
Done
