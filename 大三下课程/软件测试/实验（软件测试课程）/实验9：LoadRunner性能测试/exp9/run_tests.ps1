# Run both JMeter scenarios with server monitoring
$ErrorActionPreference = "Stop"
$BaseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $BaseDir

$candidates = @(
    $env:JMETER_HOME,
    "E:\apache-jmeter-5.6.3\apache-jmeter-5.6.3",
    "E:\apache-jmeter-5.6.3"
) | Where-Object { $_ }

$JMeterBin = $null
foreach ($jmHome in $candidates) {
    $candidate = Join-Path $jmHome "bin\jmeter.bat"
    if (Test-Path $candidate) {
        $JMeterBin = $candidate
        break
    }
}

if (-not $JMeterBin) {
    Write-Error "JMeter not found. Set JMETER_HOME to your apache-jmeter installation."
}

$env:JMETER_HOME = Split-Path (Split-Path $JMeterBin -Parent) -Parent
Write-Host "Using JMETER_HOME=$($env:JMETER_HOME)"

# Check if service is running
try {
    $resp = Invoke-WebRequest -Uri "http://127.0.0.1:8080/greeting?name=test" -UseBasicParsing -TimeoutSec 3
    Write-Host "Service OK: HTTP $($resp.StatusCode)"
} catch {
    Write-Host "WARNING: Spring Boot service not running on port 8080."
    Write-Host "Start it first: cd service; .\gradlew.bat bootRun"
    exit 1
}

$ResultsDir = Join-Path $BaseDir "jmeter\results"
New-Item -ItemType Directory -Force -Path $ResultsDir | Out-Null

function Clear-ReportDir {
    param([string]$ReportDir)
    if (Test-Path $ReportDir) {
        Remove-Item -Recurse -Force $ReportDir
    }
}

function Run-Scenario {
    param(
        [string]$Name,
        [string]$JmxFile,
        [string]$JtlFile,
        [string]$ReportDir,
        [string]$MonitorFile
    )

    Write-Host "`n========== Running $Name =========="
    Clear-ReportDir -ReportDir $ReportDir
    if (Test-Path $JtlFile) { Remove-Item -Force $JtlFile }
    if (Test-Path $MonitorFile) { Remove-Item -Force $MonitorFile }

    $monitorJob = Start-Job -ScriptBlock {
        param($Script, $Out)
        & $Script -OutputFile $Out -IntervalSec 2
    } -ArgumentList (Join-Path $BaseDir "monitor_server.ps1"), $MonitorFile

    Start-Sleep -Seconds 1

    Push-Location (Join-Path $BaseDir "jmeter")
    try {
        & $JMeterBin -n `
            -t $JmxFile `
            -l $JtlFile `
            -e -o $ReportDir `
            -j (Join-Path $ResultsDir "$Name.log")
        if ($LASTEXITCODE -ne 0) {
            throw "JMeter exited with code $LASTEXITCODE"
        }
    } finally {
        Pop-Location
    }

    Stop-Job $monitorJob -ErrorAction SilentlyContinue
    Remove-Job $monitorJob -Force -ErrorAction SilentlyContinue

    Write-Host "Results: $JtlFile"
    Write-Host "Report:  $ReportDir\index.html"
}

Run-Scenario `
    -Name "scenario1" `
    -JmxFile (Join-Path $BaseDir "jmeter\scenario1-performance.jmx") `
    -JtlFile (Join-Path $ResultsDir "scenario1-results.jtl") `
    -ReportDir (Join-Path $ResultsDir "scenario1-report") `
    -MonitorFile (Join-Path $ResultsDir "scenario1-server.csv")

Run-Scenario `
    -Name "scenario2" `
    -JmxFile (Join-Path $BaseDir "jmeter\scenario2-stress.jmx") `
    -JtlFile (Join-Path $ResultsDir "scenario2-results.jtl") `
    -ReportDir (Join-Path $ResultsDir "scenario2-report") `
    -MonitorFile (Join-Path $ResultsDir "scenario2-server.csv")

Write-Host "`n========== Analyzing results =========="
python (Join-Path $BaseDir "analyze_results.py")
python (Join-Path $BaseDir "generate_report.py")

Write-Host "`nDone! Open jmeter\results\scenario1-report\index.html and scenario2-report\index.html"
