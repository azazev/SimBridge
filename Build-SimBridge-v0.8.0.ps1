param([string]$DotnetRoot = "$env:ProgramFiles\dotnet")
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
& (Join-Path $root 'tooling\compile.ps1') -DotnetRoot $DotnetRoot | Tee-Object -FilePath (Join-Path $root 'VALIDATION.txt')
if ($LASTEXITCODE) { throw 'Build/validation failed; plugin files were not replaced.' }
$dll = Join-Path $root 'source\bin\Release\AnoBossBridge.dll'
if ([Reflection.AssemblyName]::GetAssemblyName($dll).Version.ToString() -ne '0.8.0.0') { throw 'Unexpected assembly version' }
$manifest = Get-Content -LiteralPath (Join-Path $root 'plugin\AnoBossBridge.json') -Raw | ConvertFrom-Json
if ($manifest.AssemblyVersion -ne '0.8.0.0') { throw 'Manifest version mismatch' }
Copy-Item -LiteralPath $dll -Destination (Join-Path $root 'plugin\AnoBossBridge.dll') -Force
Write-Output 'Build and ten offline groups passed. The plugin folder is ready for manual installation. Live tests remain required.'
