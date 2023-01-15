﻿#Requires -Version 7.1.3 -RunAsAdministrator
#------------------------------------------------------------------------------
# FILE:         build.ps1
# CONTRIBUTOR:  Marcus Bowyer
# COPYRIGHT:    Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
#
# Builds the Neon [neon-blazor-proxy] image.
#
# USAGE: pwsh -f build.ps1 REGISTRY VERSION TAG

param 
(
	[parameter(Mandatory=$True,Position=1)][string] $registry,
	[parameter(Mandatory=$True,Position=2)][string] $tag
)

$appname      = "neon-blazor-proxy"
$organization = SdkRegistryOrg

# Build and publish the app to a local [bin] folder.

DeleteFolder bin

$result = mkdir bin
ThrowOnExitCode

dotnet publish "$nfServices\$appname\$appname.csproj" -c Release -o "$pwd\bin"
ThrowOnExitCode

# Split the build binaries into [__app] (application) and [__dep] dependency subfolders
# so we can tune the image layers.

core-layers $appname "$pwd\bin"
ThrowOnExitCode

# Build the image.

$baseImage = Get-DotnetBaseImage "$nfRoot\Services\global.json"

Invoke-CaptureStreams "docker build -t ${registry}:${tag} --build-arg `"APPNAME=$appname`" --build-arg `"ORGANIZATION=$organization`" --build-arg `"BASE_IMAGE=$baseImage`" ." -interleave | Out-Null

# Clean up

DeleteFolder bin
