[CmdletBinding()]
param (
  [Parameter()]
  [string]
  $Name = "ipc-example-worker-server",
  [Parameter(Mandatory)]
  [string]
  $Version
)

$tag = "${Name}:${Version}"

$RootFolder = Split-Path $PSScriptRoot -Parent
Push-Location $RootFolder

docker build -t "$tag" . -f dockerfile-worker-server

Pop-Location

