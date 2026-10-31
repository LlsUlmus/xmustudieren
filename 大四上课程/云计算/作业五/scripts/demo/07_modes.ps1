. (Join-Path $PSScriptRoot 'common.ps1')
Show "Smoke test: multi / basic / fast  x  3 / 4 voters (+1 listener), RTT = 10 ms, 6 closed-loop clients on all nodes"
Write-Host "Each run starts a fresh cluster, applies load for 3 s, then compares every decided slot across all learners."
Run "& `$PY experiments\smoke.py 10"
Done
