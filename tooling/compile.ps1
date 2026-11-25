param(
    [Parameter(Mandatory=$true)][string]$DotnetRoot,
    [ValidateSet('BuildAndValidate','BuildOnly','ValidateOnly')]
    [string]$Mode = 'BuildAndValidate',
    [ValidateNotNullOrEmpty()]
    [string[]]$BossModVersions = @('7.5.8.9','7.5.8.10')
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$projectRoot = Join-Path $taskRoot 'AnoBossBridge'
if (-not (Test-Path -LiteralPath $projectRoot)) { $projectRoot = Join-Path $taskRoot 'source' }
$compiler = Join-Path $DotnetRoot 'sdk\10.0.100\Roslyn\bincore\csc.dll'
$runtime = Join-Path $DotnetRoot 'dotnet.exe'
$refs = Join-Path $DotnetRoot 'packs\Microsoft.NETCore.App.Ref\10.0.0\ref\net10.0'
$dalamud = Join-Path $env:APPDATA 'XIVLauncher\addon\Hooks\dev'
$release = Join-Path $projectRoot 'bin\Release'
foreach ($required in @($compiler, $runtime, $refs)) {
    if (-not (Test-Path -LiteralPath $required)) { throw "Missing build prerequisite: $required" }
}
if ($Mode -ne 'ValidateOnly') {
    foreach ($name in @('Dalamud.dll','Dalamud.Common.dll','Lumina.dll','Dalamud.Bindings.ImGui.dll')) {
        if (-not (Test-Path -LiteralPath (Join-Path $dalamud $name) -PathType Leaf)) { throw "Missing Dalamud development library: $name" }
    }
    New-Item -ItemType Directory -Path $release -Force | Out-Null
}
$globals = Join-Path $PSScriptRoot 'GlobalUsings.cs'
if (-not (Test-Path -LiteralPath $globals -PathType Leaf)) { throw 'Missing tracked GlobalUsings.cs' }
$attributes = Join-Path $PSScriptRoot 'AssemblyInfo.cs'
@'
using System.Reflection;
[assembly: AssemblyVersion("0.8.0.0")]
[assembly: AssemblyFileVersion("0.8.0.0")]
[assembly: AssemblyInformationalVersion("0.8.0-experimental")]
[assembly: System.Runtime.Versioning.TargetFramework(".NETCoreApp,Version=v10.0")]
[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
'@ | Set-Content -LiteralPath $attributes -Encoding utf8
$common = @('/nologo', '/nostdlib+', '/langversion:14.0', '/nullable:enable', '/optimize+', '/deterministic+', '/warnaserror+')
$common += Get-ChildItem -LiteralPath $refs -Filter '*.dll' | ForEach-Object { '/reference:"' + $_.FullName + '"' }
if ($Mode -ne 'ValidateOnly') {
    $pluginArgs = $common + @('/target:library', '/platform:x64', ('/out:"' + (Join-Path $release 'AnoBossBridge.dll') + '"'))
    $pluginArgs += @('Dalamud.dll','Dalamud.Common.dll','Lumina.dll','Dalamud.Bindings.ImGui.dll') | ForEach-Object { '/reference:"' + (Join-Path $dalamud $_) + '"' }
    $pluginArgs += @($globals, $attributes, (Join-Path $projectRoot 'Plugin.cs'), (Join-Path $projectRoot 'Probe.cs'), (Join-Path $projectRoot 'Activation.cs'), (Join-Path $projectRoot 'Compatibility.cs'), (Join-Path $projectRoot 'ContractShape.cs'), (Join-Path $projectRoot 'SimulationGate.cs'), (Join-Path $projectRoot 'RoleSync.cs'), (Join-Path $projectRoot 'DarklitSync.cs'), (Join-Path $projectRoot 'RelativitySync.cs'), (Join-Path $projectRoot 'CrystallizeSync.cs'), (Join-Path $projectRoot 'NativeDiamondSync.cs'), (Join-Path $projectRoot 'NativeLightRampantSync.cs')) | ForEach-Object { '"' + $_ + '"' }
    $response = Join-Path $PSScriptRoot 'plugin.rsp'
    $pluginArgs += '/resource:"' + (Join-Path $projectRoot 'Contracts.json') + '",SimBridge.Contracts.json'
    $pluginArgs | Set-Content -LiteralPath $response -Encoding utf8
    & $runtime $compiler "@$response"
    if ($LASTEXITCODE) { throw 'Plugin compilation failed' }
    Write-Output 'Plugin compiled with warnings treated as errors.'
}
if ($Mode -eq 'BuildOnly') { return }
if ($Mode -eq 'ValidateOnly' -and -not (Test-Path -LiteralPath (Join-Path $release 'AnoBossBridge.dll') -PathType Leaf)) {
    throw 'ValidateOnly requires an existing compiled DLL from this checkout. Run BuildOnly first.'
}
Write-Output "Offline validation targets: BossMod $($BossModVersions -join ', '); AnoMech 0.5.1.0"
$testArgs = $common + @('/target:exe', ('/out:"' + (Join-Path $PSScriptRoot 'ProbeTests.dll') + '"'))
$testArgs += @($globals, (Join-Path $projectRoot 'Probe.cs'), (Join-Path $projectRoot 'Activation.cs'), (Join-Path $projectRoot 'Compatibility.cs'), (Join-Path $projectRoot 'ContractShape.cs'), (Join-Path $projectRoot 'SimulationGate.cs'), (Join-Path $projectRoot 'RoleSync.cs'), (Join-Path $projectRoot 'DarklitSync.cs'), (Join-Path $projectRoot 'RelativitySync.cs'), (Join-Path $projectRoot 'CrystallizeSync.cs'), (Join-Path $projectRoot 'NativeDiamondSync.cs'), (Join-Path $projectRoot 'NativeLightRampantSync.cs'), (Join-Path $projectRoot 'tests\RoleTests.cs'), (Join-Path $projectRoot 'tests\NewMechanicTests.cs'), (Join-Path $projectRoot 'tests\NativeDiamondTests.cs'), (Join-Path $projectRoot 'tests\NativeTwinTests.cs'), (Join-Path $projectRoot 'tests\NativeLightRampantTests.cs'), (Join-Path $projectRoot 'tests\ContractTests.cs'), (Join-Path $projectRoot 'tests\SimulationGateTests.cs'), (Join-Path $projectRoot 'tests\ResilienceTests.cs'), (Join-Path $projectRoot 'tests\InstalledCompatibilityTests.cs')) | ForEach-Object { '"' + $_ + '"' }
$response = Join-Path $PSScriptRoot 'tests.rsp'
$testArgs += '/resource:"' + (Join-Path $projectRoot 'Contracts.json') + '",SimBridge.Contracts.json'
$testArgs | Set-Content -LiteralPath $response -Encoding utf8
& $runtime $compiler "@$response"
if ($LASTEXITCODE) { throw 'Tests compilation failed' }
'{"runtimeOptions":{"tfm":"net10.0","framework":{"name":"Microsoft.NETCore.App","version":"10.0.0"}}}' | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'ProbeTests.runtimeconfig.json') -Encoding utf8
& $runtime (Join-Path $PSScriptRoot 'ProbeTests.dll') @BossModVersions
if ($LASTEXITCODE) { throw 'Tests failed' }

$uiArgs = $common + @('/target:exe', ('/out:"' + (Join-Path $PSScriptRoot 'UiTests.dll') + '"'))
$uiArgs += @($globals, (Join-Path $projectRoot 'Plugin.cs'), (Join-Path $projectRoot 'tests\UiTests.cs')) | ForEach-Object { '"' + $_ + '"' }
$response = Join-Path $PSScriptRoot 'ui-tests.rsp'
$uiArgs | Set-Content -LiteralPath $response -Encoding utf8
& $runtime $compiler "@$response"
if ($LASTEXITCODE) { throw 'UI test compilation failed' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ProbeTests.runtimeconfig.json') -Destination (Join-Path $PSScriptRoot 'UiTests.runtimeconfig.json') -Force
& $runtime (Join-Path $PSScriptRoot 'UiTests.dll')
if ($LASTEXITCODE) { throw 'UI tests failed' }
Write-Output "10 focused offline test groups passed; installed compatibility coverage: BossMod $($BossModVersions -join ', '), AnoMech 0.5.1.0."


