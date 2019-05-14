# Upgrading Mono Linker

 * Download the latest build from CI on dnceng
   * Go to build pipeline on dnceng internal
   * Read the build logs for the latest good build to find the URL of the last-pushed package, e.g., https://dotnetfeed.blob.core.windows.net/dotnet-core/flatcontainer/illink.tasks/0.1.6-prerelease.19263.1/illink.tasks.0.1.6-prerelease.19263.1.nupkg and download it
 * Unzip the nupkg
 * Open its `tools\netcoreapp2.0` directory
 * Copy the following to `(blazorroot)/mono/tools/binaries/illink`:
    * `illink.dll`
    * `Mono.Cecil.dll`
    * `Mono.Cecil.Mdb.dll`
    * `Mono.Cecil.Pdb.dll`

Presumably you should also copy any other new dependencies it has, though it's not necessary to copy `NuGet.*.dll` or `Newtonsoft.Json.dll` or the `runtimes` subdirectory (since we execute it as a framework-dependent app).

Note that `(blazorroot)/mono/tools/binaries/illink` also contains `illink.runtimeconfig.json`, which is necessary for `dotnet illink.dll` to work.
