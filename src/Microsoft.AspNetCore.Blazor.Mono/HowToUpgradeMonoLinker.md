# Upgrading Mono Linker

 * First upgrade to whatever version of Mono you want to use as per `HowToUpgradeMono.md`
 * In the Mono build you downloaded and extracted, find the `wasm-bcl\wasm_tools` dir. From this directory, copy the following to `(blazorroot)/src/Microsoft.AspNetCore.Blazor.Mono/tools/binaries/monolinker`:
   * `monolinker.exe`
   * `Mono.Cecil.dll`

That should be all you need. Note that the target dir also contains `monolinker.runtimeconfig.json`, which is necessary for `dotnet monolinker.exe` to work.

Now commit this with a message similar to `Upgrade Mono linker binaries to <their-commit-sha>`.