# Builds DOS2-FirstPerson-Mod.zip for sharing (Play + docs + optional mod only)
$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$zipPath = Join-Path $root "DOS2-FirstPerson-Mod.zip"

if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

$staging = Join-Path $env:TEMP "DOS2-FirstPerson-Mod-staging"
if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging | Out-Null

$items = @(
    "README.txt",
    "CONFIG.txt",
    "Play",
    "Optional - Hide Body Mod"
)
foreach ($item in $items) {
    Copy-Item (Join-Path $root $item) (Join-Path $staging $item) -Recurse -Force
}

Compress-Archive -Path (Join-Path $staging "*") -DestinationPath $zipPath -Force
Remove-Item $staging -Recurse -Force

Write-Host "Created: $zipPath"
Write-Host "Send this zip file. Recipient unzips and reads README.txt."
