# Contents
- bcl Directory with the class libraries. Right now it includes a minimal list of assemblies based on what has being tested. More will be available as we ramp up the WASM BCL effort.
- driver.c The glue code being used to embed mono. See compilation instructions down
- libmonosgen-2.0.a: Archive file with the runtime.
- mono.js / mono.wasm: Compiled runtime together with driver.c
- main.cs / test.js: D8-based app used to run the interpreter test suite

# Compiling mono

Right now mono is compiled using the following emcc incantantion:
```
emcc -g4 -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 --js-library library_mono.js driver.o libmonosgen_ar_expanded/*o -o mono.js
```

The libmonosgen_ar_expanded directory is simply the contents libmonosgen-2.0.a expanded. You can do it with `ar -x`. Newer versions of emcc might properly support archive files and this no longer being necessary.

# Running mono

The environment must provide a _request_gc_cycle function in the imports list that schedules a background task and calls _mono_gc_pump_callback from the runtime. See tsst.js for an example of how to do it.

# Calling Mono from JS

The example in test.js highlight calling a C# method from JS using mono's embeding API. The mono embeding API can be further used beyond this.

It might look strange that the embedding API is not directly used from JS and that's by design. The embedding API is quite big and a reasonable way to get most of it linked out 
is by forcing each one to be wrapped/expoerted by the embedder as needed.

# Calling JS from mono

There are multiple ways to do so. Right now there's no provision to go straight from C# to JS without writing some C glue in driver.c. That allows for high perf interop and we should later investigate a more straightforward facility that bridges through a single function and requires no C changes.

The example is the AddJS function and the 3 parts can be seen in main.cs, driver.c and test.js.

# Release notes

## v2

Proper exception propagation when using call_method from JS and InvokeJS from C#.

## v1

Added WebAssembly.Runtime::InvokeJS method that evals a string and returns its value.
Moved the required JS code by mono to an emscripten js-library to further simplify deployment.

## v0

Initial code drop.