# Hypar CLI
The Hypar command line interface enables users to create, execute, and publish functions to Hypar from their *nix-compatible command line.

```
Hypar Command Line Usage:

hypar <command> <options>

Available Commands:
    execute
    executions
    functions
    help
    model
    results
    version

For additional command help:
    hypar <command> help
```

## Build
`dotnet build`

## Alias
It is recommended to create an alias for the build output to allow for `hypar <command>` during development. This can be done by calling the following in bash, or by adding this line to the `.bash_profile`:  
`alias hypar='dotnet <path to repo>/hypar/sdk/src/Hypar.CLI/bin/Debug/netcoreapp2.1/hypar.dll'`

## Configure
Building the Hypar CLI requires an `appsettings.json` file.
