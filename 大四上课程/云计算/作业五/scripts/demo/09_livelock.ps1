. (Join-Path $PSScriptRoot 'common.ps1')
Show "Basic Paxos livelock: proposers fight for the same instance (3 nodes, RTT = 10 ms, 6 s per run, request timeout 4 s)"
Write-Host "conflict_rate = rounds that failed because a higher ballot got in / all rounds;  diverged = slots on which learners disagree"
$code = @"
import asyncio, sys
sys.path.insert(0, 'experiments')
import run as R
from harness import new_loop
R.PORT_BASE[0] = 34000
async def main():
    out = R.Out('_demo_livelock')
    for name, k, bo in (('1 proposer, no backoff', 1, 0), ('3 proposers, NO backoff', 3, 0),
                        ('3 proposers, random backoff base 20 ms', 3, 20)):
        print('--', name, flush=True)
        await R.run_one(out, 'p%d-bo%d' % (k, bo), 'basic', 3, rtt=10, clients=k, targets=list(range(1, k + 1)),
                        dur=6, backoff_ms=bo, contend=1, timeout=4.0)
    out.close()
new_loop().run_until_complete(main())
"@
$f = Join-Path $DEMO 'livelock_demo.py'
[IO.File]::WriteAllText($f, $code)
Run "& `$PY run\demo\livelock_demo.py"
Remove-Item data\_demo_livelock.csv -Force -ErrorAction SilentlyContinue
Remove-Item data\raw\_demo_livelock -Recurse -Force -ErrorAction SilentlyContinue
Done
