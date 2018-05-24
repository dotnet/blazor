# Upgrading Mono

## Obtaining a Mono build

1. Find the latest Mono WebAssembly builds at https://jenkins.mono-project.com/job/test-mono-mainline-wasm/

1. Pick the build you want from the *Build History* pane (e.g., most recent green build).

1. At the bottom of the build info page, navigate to the chosen configuration (currently there's only one, called *Default*).

1. Now on the sidebar, navigate to *Azure Artifacts*.

1. Download the .zip file. Note that the commit's SHA hash is in the filename - you'll need that later to track which Mono version we're using in Blazor. 

**Shortcut:** Browse directly to https://jenkins.mono-project.com/job/test-mono-mainline-wasm/255/label=ubuntu-1804-amd64/Azure/, replacing the number 255 with the desired build number.

## Updating Blazor's `src\mono\incoming` directory

1. Extract the contents of the Mono build .zip file to a temporary directory.

1. In a side-by-side window, look at the contents of `Blazor\src\mono\incoming`

1. Delete the following from `Blazor\src\mono\incoming`, and then copy in the equivalent files/dirs from the new Mono drop:

   * `bcl\`
   * `driver.c`
   * `libmonosgen-2.0.a`
   * `library_mono.js`
   * `README.md`

   The net effect is that you're replacing everything with the newer versions. But don't copy any of the other files from the new Mono drop, as in general we don't need the extra stuff.

1. Clean up the `bcl\` directory. The Mono drops include a lot of unwanted files there. Retain *only* the `.dll` files, and **delete everything else**, including from the `facades` subdirectory. We don't need the `.pdb` files or `.tmp` or `.stamp` or anything else. All we want is the `.dll` files.

1. Re-apply the Blazor-specific edits to `driver.c`. Check your current Git diff - you should see two blocks of Blazor-specific edits are currently missing:

    * One that uses `#include` to insert the contents of `driver_blazor.c`. This should be just above `mono_wasm_load_runtime`.
    * One that registers implementations for the `BlazorInvokeJS` and `BlazorInvokeJSArray` internal methods. This should be inside `mono_wasm_load_runtime()`, at the end of it.

   Put back those two Blazor-specific blocks.
   
   If there are any other changes in `driver.c`, you probably should retain them, but it's worth trying to understand what Mono is changing and whether it affects us.

1. Check whether the build flags need to be updated. In the `README.md`, the Mono team provides recommended `emcc` arguments. If the git diff shows they have changed since the last version, figure out whether it's applicable to update the `emcc` arguments we're specifying in our `mono.targets` file.

**Commit**

At this stage, make a Git commit with a message similar to `Upgrade Mono to <their-commit-sha> - have not yet rebuilt the binaries`. Their commit SHA can be found in the filename of the Mono drop you downloaded.

This commit is needed because you'll want to have a clean git state before the next step, so you can check the diff from the next step is what you expect.

## Building Mono binaries

We don't use the prebuilt Mono WebAssembly binaries (or `mono.js`) because:

 * We need a build that includes our custom interop code from `driver_blazor.c`
 * We also need asm.js builds, which Mono doesn't supply

We may also have reasons to use custom build flags, e.g., to control the optimization level or change the default heap size. But for now the two points above are reason enough.

To build the Mono WebAssembly binaries, you need a working Emscripten toolchain. Follow the instructions at https://kripken.github.io/emscripten-site/docs/getting_started/downloads.html. You need at least version 1.37.36, though in general it's likely to be preferable to use whatever is the latest version.

On Windows,

 * `cd your\emscripten\installation\dir`
 * `emsdk activate`
 * Verify that `emcc --version` returns 1.37.36 or later
 * In the same command prompt, `cd` to `Blazor\src\mono`
 * `dotnet msbuild mono.targets /t:BuildMonoEmcc`

This will build both the wasm and asm.js binaries.

On the first run for any given command prompt instance, it takes a long time (e.g., 5 minutes). Subsequent builds in the same command prompt instance will be more like 30 seconds.

If you're not on Windows, it should work the same (verified in WSL), though the output files may have different line endings so the `git diff` may be hard to make sense of.

**Expected result**

Currently the build reports these warnings:

 * `EXEC : warning : unresolved symbol: putchar` (during both wasm and asm.js builds)
 * `EXEC : warning : root:BINARYEN_ASYNC_COMPILATION disabled due to user options` (during the asm.js build only)
 * `EXEC : warning : unresolved symbol: mono_wasm_invoke_js_with_args` (during both wasm and asm.js builds). I expect this is a temporary issue that will go away when Mono finishes implementing whatever they are doing with their JS interop, which we aren't currently using anyway since we have our own interop in `driver_blazor.c`.

These are OK, but anything other warnings or errors may imply problems like:

 * Your Emscripten toolchain isn't set up correctly
 * You need a newer version of Emscripten, because Mono now depends on it
 * You need to change the `emcc` arguments in `mono.targets`, because something about the incoming binaries now requires it (see whether the Mono drop has changed the recommended args in `incoming\README.md` since the last version)
 * You need to change something about `driver_blazor.c` because the newer Mono binary requires something different
 * Mono has added some new required files that don't fit into the pattern you used when updating the `incoming` directory. Figure out what you need to change/add/remove, and update these instructions.

Review the resulting Git diff. Check that the `.wasm` and `.js` files inside `src\mono\unoptimized` aren't radically bigger than before.

Note that the contents of `src\mono\optimized` are produced during Blazor's own build and aren't tracked in source control, so don't expect to see any Git diff for that.

**Commit**

Make a Git commit with a message similar to `Built binaries for Mono version <their-commit-sha>`.

## Verifying

Rebuild Blazor completely from a clean state:

 * `cd` to the root of your Blazor repo
 * Verify `git status` shows your working copy has no uncommitted edits
 * `git clean -xdf`
 * `build.cmd` or `./build.sh`

Now run the E2E tests.

If anything seems broken, you might want to start investigations in the MonoSanity project, since this just uses Mono directly, without most of what Blazor builds on top of it.
