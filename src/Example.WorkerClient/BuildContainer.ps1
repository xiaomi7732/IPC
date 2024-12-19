[CmdletBinding()]
param (
  [Parameter()]
  [string]
  $Name = "chattyipcclient",
  [Parameter(Mandatory)]
  [string]
  $Version
)

$tag = "${Name}:${Version}"

$RootFolder = Split-Path $PSScriptRoot -Parent
Push-Location $RootFolder

docker build -t "$tag" . -f dockerfile-worker-client

Pop-Location

