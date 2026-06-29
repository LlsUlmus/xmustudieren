# 在项目根目录（作业20）下执行:  .\compile.ps1
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

if (-not (Test-Path out)) {
    New-Item -ItemType Directory -Path out | Out-Null
}

$sources = Get-ChildItem -Path "src" -Recurse -Filter "*.java" | ForEach-Object { $_.FullName }
if ($sources.Count -eq 0) {
    Write-Error "未找到 src 下的 .java 文件"
}

Write-Host "编译 $($sources.Count) 个源文件 -> out\"
javac -encoding UTF-8 -d out @sources
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "编译成功。运行 GUI: java -cp out gui.FileSystemGUI"
