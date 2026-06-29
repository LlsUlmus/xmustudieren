$sub = New-Object System.Net.Sockets.TcpClient("127.0.0.1", 9999)
$subStream = $sub.GetStream()
$subReader = New-Object System.IO.StreamReader($subStream, [System.Text.Encoding]::UTF8)
$subWriter = New-Object System.IO.StreamWriter($subStream, [System.Text.Encoding]::UTF8)
$subWriter.AutoFlush = $true
$c1 = $subReader.ReadLine()
$subWriter.WriteLine("SUBSCRIBE news")
$c2 = $subReader.ReadLine()

$pub = New-Object System.Net.Sockets.TcpClient("127.0.0.1", 9999)
$pubStream = $pub.GetStream()
$pubReader = New-Object System.IO.StreamReader($pubStream, [System.Text.Encoding]::UTF8)
$pubWriter = New-Object System.IO.StreamWriter($pubStream, [System.Text.Encoding]::UTF8)
$pubWriter.AutoFlush = $true
$null = $pubReader.ReadLine()
$pubWriter.WriteLine("PUBLISH news hello_topic")
$c3 = $pubReader.ReadLine()
$c4 = $subReader.ReadLine()

Write-Output $c1
Write-Output $c2
Write-Output $c3
Write-Output $c4

$pub.Close()
$sub.Close()
