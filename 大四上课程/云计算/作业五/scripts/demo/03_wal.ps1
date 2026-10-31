. (Join-Path $PSScriptRoot 'common.ps1')
Show "1. Acceptor write-ahead logs on disk (one per voter; the listener never votes, so its file stays empty)"
Run "Get-ChildItem run\demo\data | Format-Table Name,Length,LastWriteTime -AutoSize | Out-String | ForEach-Object Trim"
Show "2. Decode node 1's WAL: the persisted promised_id / accepted_id / accepted_value"
Run "& `$PY scripts\wal_dump.py run\demo\data\acceptor-1.wal 12"
Show "3. Raw bytes of the first records: type(1) slot(8) ballot(8) len(4) value crc32(4), big-endian"
Run "& `$PY scripts\wal_dump.py --hex run\demo\data\acceptor-1.wal 144"
Done
