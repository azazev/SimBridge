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
$out = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
if (Test-Path -LiteralPath $out) { throw 'Output directory already exists; use a new -OutputDirectory.' }
$expected = [ordered]@{
    'AnoBossBridge.dll' = '9fc558a298736ba1817062fcf0ed7221a873ce155abaac7f7ceb06610783e48d'
    'AnoBossBridge.deps.json' = '279ce3716831af5981df8427361b30fdaa3f5c8e014e450d65b2c391893cb8a7'
    'AnoBossBridge.json' = '4f03ec37cb56756024a8b334e64079b8300e60ce834bea0e8619258a6c70265b'
}
foreach ($name in $expected.Keys) {
    $path = Join-Path $plugin $name
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing verified plugin file: $name" }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $expected[$name]) { throw "Unexpected SHA-256: $name" }
}
$manifest = Get-Content -LiteralPath (Join-Path $plugin 'AnoBossBridge.json') -Raw | ConvertFrom-Json
# Windows PowerShell 5.1 emits a JSON array as one pipeline object;
# newer PowerShell may enumerate it. Assign first, then normalize the variable.
$templateJson = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'repo.template.json') -Raw
if (-not $templateJson.TrimStart().StartsWith('[')) { throw 'Expected a JSON array in repository template' }
$decoded = ConvertFrom-Json -InputObject $templateJson
$entries = @($decoded)
if ($entries.Count -ne 1) { throw 'Expected one store entry' }
$entry = $entries[0]
if ($entry -isnot [System.Management.Automation.PSCustomObject]) { throw 'Expected a single JSON store object, not a nested array' }
foreach ($property in @('DownloadLinkInstall','DownloadLinkUpdate','IconUrl','LastUpdate','IsTestingExclusive','IsHide','Name','InternalName','AssemblyVersion','DalamudApiLevel','ApplicableVersion','Author')) {
    if ($null -eq $entry.PSObject.Properties[$property]) { throw "Missing store property: $property" }
}
foreach ($key in @('Name','InternalName','AssemblyVersion','DalamudApiLevel','ApplicableVersion','Author','Changelog')) {
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
$iconPath = Join-Path $root 'assets/icon.png'
$iconHash = '6ad4cf3e8554f2f7623316dc95f1387ffd7f2e4428002328c6ed762f477e7e65'
if ((Get-FileHash -LiteralPath $iconPath -Algorithm SHA256).Hash -ne $iconHash) { throw 'Unexpected icon hash' }
if ($entry.IconUrl -ne 'https://raw.githubusercontent.com/azazev/SimBridge/main/assets/icon.png') { throw 'Unexpected public icon URL' }
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
$parent = Split-Path -Parent $out
New-Item -ItemType Directory -Path $parent -Force | Out-Null
$temp = Join-Path $parent ('SimBridge-stage-' + [Guid]::NewGuid().ToString('N'))
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
    $sourceZip = Join-Path $result 'SimBridge-v0.9.0-source.zip'
    [IO.Compression.ZipFile]::CreateFromDirectory($binaryStage,$binaryZip,[IO.Compression.CompressionLevel]::Optimal,$false)
    [IO.Compression.ZipFile]::CreateFromDirectory($sourceStage,$sourceZip,[IO.Compression.CompressionLevel]::Optimal,$false)
    Verify-ZipEntries $binaryZip (@($expected.Keys) + $noticeFiles + @('SOURCE.md')) $true
    Verify-ZipEntries $sourceZip $sourceFiles $false
    $utf8 = [System.Text.UTF8Encoding]::new($false)
    # Planned feed is intentionally not active, and LastUpdate is stamped only at publication.
    # Preserve the reviewed public template exactly; serialize only the local variant.
    [IO.File]::WriteAllText((Join-Path $result 'repo.json'),$templateJson,$utf8)
    $localUrl = "http://127.0.0.1:$LocalTestPort/latest.zip"
    $entry.DownloadLinkInstall = $localUrl
    $entry.DownloadLinkUpdate = $localUrl
    $localIcon = "http://127.0.0.1:$LocalTestPort/icon.png"
    $entry.IconUrl = $localIcon
    Copy-Item -LiteralPath $iconPath -Destination (Join-Path $result 'icon.png')
    $entry.LastUpdate = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $localJson = ConvertTo-Json -InputObject (,$entry) -Depth 10
    $localDecoded = ConvertFrom-Json -InputObject $localJson
    $localEntries = @($localDecoded)
    if (-not $localJson.TrimStart().StartsWith('[') -or $localEntries.Count -ne 1 -or $localEntries[0] -isnot [System.Management.Automation.PSCustomObject]) { throw 'Local feed must be an array containing exactly one store object' }
    if ($localEntries[0].DownloadLinkInstall -ne $localUrl -or $localEntries[0].DownloadLinkUpdate -ne $localUrl -or $localEntries[0].InternalName -ne $manifest.InternalName -or $localEntries[0].IconUrl -ne $localIcon) { throw 'Local feed round-trip verification failed' }
    [IO.File]::WriteAllText((Join-Path $result 'repo.local.json'),$localJson,$utf8)
    $checksums = @(Get-ChildItem -LiteralPath $result -File -Filter *.zip | Sort-Object Name | ForEach-Object {
        "$((Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant())  $($_.Name)"
    })
    [IO.File]::WriteAllLines((Join-Path $result 'SHA256SUMS.txt'),[string[]]$checksums,$utf8)
    if (Test-Path -LiteralPath $out) { throw 'Output directory appeared during packaging; choose a new output directory.' }
    $stageRoot = [IO.Path]::GetFullPath($temp) + [IO.Path]::DirectorySeparatorChar
    if (![IO.Path]::GetFullPath($result).StartsWith($stageRoot,[StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetDirectoryName($out) -ne [IO.Path]::GetFullPath($parent)) { throw 'Unexpected staging path' }
    Move-Item -LiteralPath $result -Destination $out
    Write-Output 'PASS: all three original plugin payload hashes, manifest identity and ZIP entry lists verified.'
    Get-ChildItem -LiteralPath $out -File | Select-Object Name,Length
    Get-Content -LiteralPath (Join-Path $out 'SHA256SUMS.txt')
    Write-Output 'PRIVATE DRAFT ONLY. No installed plugins or remote repository changed.'
} finally {
    $tempParent = [IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($temp))
    if ($tempParent -ne [IO.Path]::GetFullPath($parent) -or [IO.Path]::GetFileName($temp) -notmatch '^SimBridge-stage-[a-fA-F0-9]{32}$') { throw 'Unexpected temporary path' }
    if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Recurse -Force }
}
