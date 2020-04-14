## About timezonedata

This project is used to build the timezone database used by .NET WASM runtime to support timezones. The output of this project is checked in to this folder under the file named `dotnet.timezones.dat`. This output is currently manually regenerated.

### Building this project

Prereqs:
* *nix machine
* .NET Core SDK 3.1 or newer installed.

Run `run.sh`. This should update the `dotnet.timezones.dat` file. Commit this file to git.
