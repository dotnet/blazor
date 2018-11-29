# Blazor

For information about using Blazor, see [blazor.net](http://blazor.net).

# Sources moved

Almost all the sources for Blazor and the Razor Components programming model have moved [here in the central ASP.NET Core repo](https://github.com/aspnet/AspNetCore/tree/master/src/Components). We are also in the process of migrating open issues from here to there.

This is in preparation for shipping Razor Components as a built-in feature of ASP.NET Core 3.0. *Note: client-side Blazor remains experimental while we continue to work on making the WebAssembly runtime complete.*

### Where should I post issues and PRs now?

New issues and PRs should be posted at the [central ASP.NET Core repo](https://github.com/aspnet/AspNetCore).

Please don't post new issues or PRs in this repo.

### What about the existing outstanding PRs in this repo?

Currently you don't need to do anything. Soon we will address each of the outstanding PRs on a case-by-case basis. In many cases we will ask the author to consider migrating their PR to the new repo and can provide guidance on that. In some cases we may think the PR is no longer applicable.

We apologize for the extra work this involves. This move had to happen at some point in order for the technology to ship.
