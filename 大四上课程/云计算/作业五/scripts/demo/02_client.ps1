. (Join-Path $PSScriptRoot 'common.ps1')
$P = Peers 3
Show "1. Write through different nodes (a non-leader forwards the command to the leader)"
Run "& `$PY -m paxos.client --peers $P --node 1 put course cloud-computing"
Run "& `$PY -m paxos.client --peers $P --node 2 put hw5 paxos"
Run "& `$PY -m paxos.client --peers $P --node 3 put hello world"
Show "2. Read the replicated state machine on each voter and on the listener (id 9: learns only, never votes)"
Run "& `$PY -m paxos.client --peers $P --node 1 get hello"
Run "& `$PY -m paxos.client --peers $P --node 2 get hw5"
Run "& `$PY -m paxos.client --peers $P --node 3 get course"
Run "& `$PY -m paxos.client --peers 9=127.0.0.1:7109 get hello"
Show "3. Closed-loop benchmark: 8 clients spread over the 3 nodes, 3 seconds, fsync ON"
Bench $P 8 3
Show "4. Status after the load: same decided prefix on every node (listener included), violations=0"
Run "& `$PY -m paxos.client --peers $P,9=127.0.0.1:7109 status"
Done
