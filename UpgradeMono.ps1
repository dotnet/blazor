[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)] [String] $MonoRootDir
)

# Verify the new binaries can be found at the expected paths
$inputWasmDir = Join-Path -Path $MonoRootDir -ChildPath "builds\release"
$inputBclDir = Join-Path -Path $MonoRootDir -ChildPath "wasm-bcl\wasm"
$inputBclFacadesDir = Join-Path -Path $inputBclDir -ChildPath "Facades"
$inputFrameworkDir = Join-Path -Path $MonoRootDir -ChildPath "framework"
$inputLinkerDir = Join-Path -Path $MonoRootDir -ChildPath "wasm-bcl\wasm_tools"

foreach ($dirToCheck in ($MonoRootDir, $inputWasmDir, $inputBclDir, $inputBclFacadesDir, $inputFrameworkDir, $inputLinkerDir)) {
    if (-not (Test-Path -LiteralPath $dirToCheck)) {
        Write-Error -Message "Directory '$dirToCheck' not found." -ErrorAction Stop
    }
}

# Delete old binaries
$outputRoot = Join-Path -Path $PSScriptRoot -ChildPath "src\Microsoft.AspNetCore.Components.WebAssembly.Runtime\incoming"
if (-not (Test-Path -LiteralPath $outputRoot)) {
    Write-Error -Message "Directory '$outputRoot' not found." -ErrorAction Stop
}
Write-Host "Deleting existing Mono binaries from '$outputRoot'..."
Remove-Item -Recurse -Force $outputRoot

# Copy new binaries
$outputWasmDir = Join-Path -Path $outputRoot -ChildPath wasm
$outputBclDir = Join-Path -Path $outputRoot -ChildPath bcl
$outputBclFacadesDir = Join-Path -Path $outputBclDir -ChildPath Facades
$outputFrameworkDir = Join-Path -Path $outputRoot -ChildPath framework
$outputLinkerDir = Join-Path -Path $outputRoot -ChildPath "..\tools\binaries\monolinker"

Write-Host "Copying in new Mono binaries from '$MonoRootDir'..."
New-Item -Force -ItemType "directory" $outputWasmDir | Out-Null
Copy-Item "$inputWasmDir\dotnet.wasm" -Destination $outputWasmDir
Copy-Item "$inputWasmDir\dotnet.js" -Destination $outputWasmDir

New-Item -Force -ItemType "directory" $outputBclDir | Out-Null
Copy-Item "$inputBclDir\*.dll" -Destination $outputBclDir
Remove-Item "$outputBclDir\nunitlite.dll" # Not needed

New-Item -Force -ItemType "directory" $outputBclFacadesDir | Out-Null
Copy-Item "$inputBclFacadesDir\*.dll" -Destination $outputBclFacadesDir

New-Item -Force -ItemType "directory" $outputFrameworkDir | Out-Null
Copy-Item "$inputFrameworkDir\*.dll" -Destination $outputFrameworkDir

# We leave the existing linker dir in place, because we don't want to remove the .runtimeconfig.json file
Copy-Item "$inputLinkerDir\monolinker.exe" -Destination $outputLinkerDir
Copy-Item "$inputLinkerDir\Mono.Cecil.dll" -Destination $outputLinkerDir
