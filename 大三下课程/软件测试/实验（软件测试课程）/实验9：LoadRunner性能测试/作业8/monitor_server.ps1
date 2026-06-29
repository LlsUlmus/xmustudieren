# Monitor Spring Boot JVM process CPU and memory during JMeter test
param(
    [Parameter(Mandatory = $true)]
    [string]$OutputFile,
    [int]$IntervalSec = 2
)

$javaProcs = Get-Process -Name "java" -ErrorAction SilentlyContinue
if (-not $javaProcs) {
    "timestamp,cpu_percent,memory_mb" | Out-File -FilePath $OutputFile -Encoding utf8
    exit 0
}

"timestamp,cpu_percent,memory_mb" | Out-File -FilePath $OutputFile -Encoding utf8

$prevCpu = $null
$prevTime = $null
$cores = [Environment]::ProcessorCount

while ($true) {
    $procs = Get-Process -Name "java" -ErrorAction SilentlyContinue
    if (-not $procs) { break }

    $cpuSec = ($procs | Measure-Object -Property CPU -Sum).Sum
    $mem = ($procs | Measure-Object -Property WorkingSet64 -Sum).Sum / 1MB
    $now = Get-Date
    $cpuPct = 0.0

    if ($null -ne $prevCpu -and $null -ne $prevTime) {
        $elapsed = ($now - $prevTime).TotalSeconds
        if ($elapsed -gt 0) {
            $cpuPct = [math]::Min(100, [math]::Round((($cpuSec - $prevCpu) / $elapsed / $cores) * 100, 2))
        }
    }

    $prevCpu = $cpuSec
    $prevTime = $now
    $ts = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$ts,$cpuPct,$([math]::Round($mem, 2))" | Add-Content -Path $OutputFile -Encoding utf8
    Start-Sleep -Seconds $IntervalSec
}
