# Upgrading Mono and Mono Linker

## Obtaining a Mono build

1. Find the latest Mono WebAssembly builds at https://jenkins.mono-project.com/job/test-mono-mainline-wasm/

1. Pick the build you want from the *Build History* pane (e.g., most recent green build).

1. At the bottom of the build info page, navigate to the chosen configuration (currently there's only one, called *Default*).

1. Now on the sidebar, navigate to *Azure Artifacts*.

1. Download the .zip file. Note that the commit's SHA hash is in the filename - you'll need that later to track which Mono version we're using in Blazor. 

**Shortcut:** Browse directly to https://jenkins.mono-project.com/job/test-mono-mainline-wasm/255/label=ubuntu-1804-amd64/Azure/, replacing the number 255 with the desired build number.

## Update files in this repo using the new Mono build

From the root directory of this repo, run the script

    UpgradeMono.[cmd|ps1] <path_to_extracted_mono_build>

For example,

    UpgradeMono.cmd C:\Users\you\Downloads\mono-wasm-a93e4712ecd

All this script does is remove older binaries from the `incoming` and `tools` directories in this repo, and then copy in an equivalent structure from the directory you indicated.

Verify that there were no errors, and that resulting git diff looks reasonable. For normal upgrades, you should see:

 * `mono.js` and `mono.wasm` have been modified
 * Many of the .NET assemblies in `bcl` and `bcl\facades` have been modified. Occasionally some new ones will be added, or some may be removed, though this is quite unusual.
 * The three .NET assemblies in `framework` will usually have been modified. It's not expected for assemblies to be added or removed here, so check with Mono if that seems to have happened.
 * The linker binaries in `tools` will usually have been modified.

Getting this far is not a guarantee that the resulting applications will run correctly. For example, it's possible that the linker has new dependencies and cannot run as-is. See verification steps later.

**Commit**

At this stage, make a Git commit with a message similar to `Upgrade Mono binaries to <their-commit-sha>`. Their commit SHA can be found in the filename of the Mono drop you downloaded.

## Verifying

Push your change as a PR to the `blazor` repo. Once the CI system has built the resulting package, download it. It usually has a name like `Microsoft.AspNetCore.Blazor.Mono.3.2.0-ci.nupkg`.

Switch over to the `aspnetcore` repo and modify the `eng\Versions.props` file to use your new version (e.g., `3.2.0-ci`). To restore this correctly,

 * Make sure you don't already have a `-ci` version of that package in your NuGet package cache
 * Run a `dotnet restore` with `-s <directory_containing_the_download>`

Now try running the StandaloneApp and perf benchmarks to be sure the apps still work, and the size and perf numbers haven't regressed badly. Ideally, also run all the E2E tests.

If everything looks good, you can proceed with merging your PR to the `blazor` repo.
