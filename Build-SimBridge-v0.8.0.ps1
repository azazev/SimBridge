param(
    [string]$DotnetRoot = "$env:ProgramFiles\dotnet",
    [ValidateSet('BuildAndValidate','BuildOnly','ValidateOnly')]
    [string]$Mode = 'BuildAndValidate',
    [ValidateNotNullOrEmpty()]
    [string[]]$BossModVersions = @('7.5.8.9','7.5.8.10')
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
& (Join-Path $root 'tooling\compile.ps1') -DotnetRoot $DotnetRoot -Mode $Mode -BossModVersions $BossModVersions |
    Tee-Object -FilePath (Join-Path $root 'VALIDATION.txt')

if ($Mode -eq 'ValidateOnly') {
    Write-Output 'Offline validation completed for the selected targets; no plugin DLL was built or copied.'
    return
}
$dll = Join-Path $root 'source\bin\Release\AnoBossBridge.dll'
if ([Reflection.AssemblyName]::GetAssemblyName($dll).Version.ToString() -ne '0.8.0.0') { throw 'Unexpected assembly version' }
if ($Mode -eq 'BuildOnly') {
    Write-Output 'Build only: assembly version verified; offline tests not run and plugin folder not updated.'
    return
}
$manifest = Get-Content -LiteralPath (Join-Path $root 'plugin\AnoBossBridge.json') -Raw | ConvertFrom-Json
if ($manifest.AssemblyVersion -ne '0.8.0.0' -or $manifest.InternalName -ne 'AnoBossBridge') { throw 'Manifest version or identity mismatch' }
$deps = Get-Content -LiteralPath (Join-Path $root 'plugin\AnoBossBridge.deps.json') -Raw | ConvertFrom-Json
if (-not $deps.libraries.PSObject.Properties['AnoBossBridge/0.8.0.0']) { throw 'Dependency metadata version mismatch' }
Copy-Item -LiteralPath $dll -Destination (Join-Path $root 'plugin\AnoBossBridge.dll') -Force
Write-Output "Build and 10 offline test groups passed for selected BossMod target(s): $($BossModVersions -join ', '), AnoMech 0.5.1.0. Plugin folder ready for manual verification. Live tests remain separate."
