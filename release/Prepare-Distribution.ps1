param(
    [string]$PluginDirectory,
    [string]$OutputDirectory,
    [ValidateRange(1024,65535)][int]$LocalTestPort = 8765
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not $PluginDirectory) { $PluginDirectory = Join-Path $root 'plugin' }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $root 'release-output' }
$plugin = (Resolve-Path -LiteralPath $PluginDirectory).ProviderPath
$out = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $out) { throw 'Output directory already exists; use a new -OutputDirectory.' }
$expected = [ordered]@{
    'AnoBossBridge.dll' = '2ee6b505d985121a8c9f37c43eeec1b8ce32c36b909fa0a3710d8eff39aa5993'
    'AnoBossBridge.deps.json' = '2bd145ab4e7c34da4dc5b080cf8bb3dc26ff995772bbc6424d17e77e05199c81'
    'AnoBossBridge.json' = 'e307627c2253c59ea6989269d3e016156c86985a8d0bdb8e3460851c5676ec71'
}
foreach ($name in $expected.Keys) {
    $path = Join-Path $plugin $name
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing verified plugin file: $name" }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $expected[$name]) { throw "Unexpected SHA-256: $name" }
}
$manifest = Get-Content -LiteralPath (Join-Path $plugin 'AnoBossBridge.json') -Raw | ConvertFrom-Json
$entry = @(Get-Content -LiteralPath (Join-Path $PSScriptRoot 'repo.template.json') -Raw | ConvertFrom-Json)
if ($entry.Count -ne 1) { throw 'Expected one store entry' }
$entry = $entry[0]
foreach ($key in @('Name','InternalName','AssemblyVersion','DalamudApiLevel','ApplicableVersion','Author')) {
    if ($entry.$key -ne $manifest.$key) { throw "Manifest/feed mismatch: $key" }
}
if ([Reflection.AssemblyName]::GetAssemblyName((Join-Path $plugin 'AnoBossBridge.dll')).Version.ToString() -ne $manifest.AssemblyVersion) { throw 'Assembly version mismatch' }
if ($entry.IsTestingExclusive -or $entry.IsHide) { throw 'Unexpected first-beta visibility flags' }
$sourceFiles = @(Get-Content -LiteralPath (Join-Path $PSScriptRoot 'SOURCE_FILES.txt') | Where-Object { $_.Trim() })
if ($sourceFiles.Count -eq 0 -or @($sourceFiles | Sort-Object -Unique).Count -ne $sourceFiles.Count) { throw 'Missing or duplicated source allowlist' }
foreach ($path in $sourceFiles) {
    if ($path -notmatch '^[A-Za-z0-9._/-]+$' -or $path -match '(^|/)\.\.(/|$)|^/' -or $path -match '(?i)\.(dll|exe|pdb|zip|log|jsonl)$') { throw "Unsafe source entry: $path" }
    if (-not (Test-Path -LiteralPath (Join-Path $root $path) -PathType Leaf)) { throw "Missing source entry: $path" }
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression
function Copy-RelativeFile([string]$FromRoot, [string]$ToRoot, [string]$Relative) {
    $dest = Join-Path $ToRoot $Relative
    New-Item -ItemType Directory -Path (Split-Path -Parent $dest) -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $FromRoot $Relative) -Destination $dest
}
function Verify-ZipEntries([string]$ZipPath, [string[]]$ExpectedFiles, [bool]$VerifyPayload) {
    $zip = [IO.Compression.ZipFile]::OpenRead($ZipPath)
    try {
        $actual = @($zip.Entries | Where-Object { $_.Name } | ForEach-Object { $_.FullName.Replace('\','/') })
        if ($actual.Count -ne $ExpectedFiles.Count -or @(Compare-Object ($actual | Sort-Object) ($ExpectedFiles | Sort-Object)).Count) { throw 'Unexpected ZIP entry list' }
        if ($VerifyPayload) {
            foreach ($name in $expected.Keys) {
                $stream = $zip.GetEntry($name).Open()
                $sha = [Security.Cryptography.SHA256]::Create()
                try {
                    $hash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','')
                    if ($hash -ne $expected[$name]) { throw "ZIP payload hash mismatch: $name" }
                } finally { $sha.Dispose(); $stream.Dispose() }
            }
        }
    } finally { $zip.Dispose() }
}
$temp = Join-Path ([IO.Path]::GetTempPath()) ('SimBridge-stage-' + [Guid]::NewGuid().ToString('N'))
try {
    $binaryStage = Join-Path $temp 'binary'
    $sourceStage = Join-Path $temp 'source'
    $result = Join-Path $temp 'output'
    New-Item -ItemType Directory -Path $binaryStage,$sourceStage,$result -Force | Out-Null
    foreach ($name in $expected.Keys) { Copy-RelativeFile $plugin $binaryStage $name }
    $noticeFiles = @('LICENSE','THIRD_PARTY_NOTICES.md','LICENSES/BossModReborn-BSD-3-Clause.txt')
    foreach ($name in $noticeFiles) { Copy-RelativeFile $root $binaryStage $name }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'SOURCE.md') -Destination (Join-Path $binaryStage 'SOURCE.md')
    foreach ($path in $sourceFiles) { Copy-RelativeFile $root $sourceStage $path }
    $binaryZip = Join-Path $result 'latest.zip'
    $sourceZip = Join-Path $result 'SimBridge-v0.8.0-beta.1-source.zip'
    [IO.Compression.ZipFile]::CreateFromDirectory($binaryStage,$binaryZip,[IO.Compression.CompressionLevel]::Optimal,$false)
    [IO.Compression.ZipFile]::CreateFromDirectory($sourceStage,$sourceZip,[IO.Compression.CompressionLevel]::Optimal,$false)
    Verify-ZipEntries $binaryZip (@($expected.Keys) + $noticeFiles + @('SOURCE.md')) $true
    Verify-ZipEntries $sourceZip $sourceFiles $false
    $utf8 = New-Object System.Text.UTF8Encoding($false)
    # Planned feed is intentionally not active, and LastUpdate is stamped only at publication.
    [IO.File]::WriteAllText((Join-Path $result 'repo.json'),(ConvertTo-Json -InputObject @($entry) -Depth 10),$utf8)
    $entry.DownloadLinkInstall = "http://127.0.0.1:$LocalTestPort/latest.zip"
    $entry.DownloadLinkUpdate = $entry.DownloadLinkInstall
    $entry.LastUpdate = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    [IO.File]::WriteAllText((Join-Path $result 'repo.local.json'),(ConvertTo-Json -InputObject @($entry) -Depth 10),$utf8)
    $checksums = @(Get-ChildItem -LiteralPath $result -File | Sort-Object Name | ForEach-Object {
        "$((Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant())  $($_.Name)"
    })
    [IO.File]::WriteAllLines((Join-Path $result 'SHA256SUMS.txt'),[string[]]$checksums,$utf8)
    New-Item -ItemType Directory -Path (Split-Path -Parent $out) -Force | Out-Null
    Move-Item -LiteralPath $result -Destination $out
    Write-Output 'PASS: all three original plugin payload hashes, manifest identity and ZIP entry lists verified.'
    Get-ChildItem -LiteralPath $out -File | Select-Object Name,Length
    Get-Content -LiteralPath (Join-Path $out 'SHA256SUMS.txt')
    Write-Output 'PRIVATE DRAFT ONLY. No installed plugins or remote repository changed.'
} finally {
    if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Recurse -Force }
}
