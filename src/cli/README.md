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

## Install
- [Download](https://s3-us-west-1.amazonaws.com/hypar-cli/hypar.zip)
- Create an alias in your nix shell:  
`alias hypar='dotnet <path to repo>/hypar.dll'`

## Build
`dotnet build`

## Configure
Building the Hypar CLI requires an `appsettings.json` file.
