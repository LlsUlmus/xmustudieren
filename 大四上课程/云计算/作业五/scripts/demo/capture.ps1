# Run one demo step in a real console window and save a screenshot of that window only.
#   capture.ps1 -Step 01_start [-Cols 150] [-Lines 44] [-Timeout 180]
param([Parameter(Mandatory)][string]$Step, [int]$Cols = 150, [int]$Lines = 44, [int]$Timeout = 180)
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$demo = Join-Path $root 'run\demo'; New-Item -ItemType Directory -Force $demo | Out-Null
$shots = Join-Path $root 'report\screenshots'; New-Item -ItemType Directory -Force $shots | Out-Null
$done = Join-Path $demo 'step.done'; Remove-Item $done -Force -ErrorAction SilentlyContinue
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System; using System.Runtime.InteropServices;
public class Win {
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr hdc, uint flags);
  [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
  [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr FindWindow(string cls, string title);
}
'@
[Win]::SetProcessDPIAware() | Out-Null
$script = Join-Path $PSScriptRoot "$Step.ps1"
$font = Join-Path $PSScriptRoot 'font.ps1'
$inner = "`$host.UI.RawUI.WindowTitle='Paxos demo - $Step'; . '$font'; mode con cols=$Cols lines=$Lines; . '$script'"
$p = Start-Process conhost.exe -PassThru -ArgumentList @('powershell.exe', '-NoProfile', '-NoExit', '-ExecutionPolicy', 'Bypass', '-Command', "`"$inner`"")
$t0 = Get-Date
while (-not (Test-Path $done) -and ((Get-Date) - $t0).TotalSeconds -lt $Timeout) { Start-Sleep -Milliseconds 300 }
Start-Sleep -Milliseconds 800
$title = "Paxos demo - $Step"
$h = [Win]::FindWindow($null, $title)
if ($h -eq [IntPtr]::Zero) {
    $wp = Get-Process | Where-Object { $_.MainWindowTitle -like "*$title*" } | Select-Object -First 1
    if ($wp) { $h = $wp.MainWindowHandle }
}
$r = New-Object Win+RECT; [Win]::GetWindowRect($h, [ref]$r) | Out-Null
$w = $r.R - $r.L; $ht = $r.B - $r.T
$bmp = New-Object System.Drawing.Bitmap $w, $ht
$g = [System.Drawing.Graphics]::FromImage($bmp); $hdc = $g.GetHdc()
$ok = [Win]::PrintWindow($h, $hdc, 2)          # PW_RENDERFULLCONTENT: this window only, nothing else on screen
$g.ReleaseHdc($hdc); $g.Dispose()
$out = Join-Path $shots "$Step.png"; $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
Get-CimInstance Win32_Process -Filter "Name='powershell.exe'" | Where-Object { $_.CommandLine -like "*$title*" -and $_.ProcessId -ne $PID } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
"{0}: finished={1} printwindow={2} hwnd={3} {4}x{5} -> {6}" -f $Step, (Test-Path $done), $ok, $h, $w, $ht, $out
