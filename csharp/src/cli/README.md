# Hypar CLI
The Hypar command line interface enables users to create, execute, and publish functions to Hypar from the command line.

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
- Download for:
  - [Windows](https://s3-us-west-1.amazonaws.com/hypar-cli/hypar-win-x64.zip)
  - [Mac](https://s3-us-west-1.amazonaws.com/hypar-cli/hypar-osx.10.12-x64.zip)
  - [Linux](https://s3-us-west-1.amazonaws.com/hypar-cli/hypar-linux-x64.zip)
- Link
  - On Mac and Linux: `ln -s <path to hypar executable> /usr/bin/local/hypar`
  - On windows add `<path to hypar>` to your user `PATH`.

## Build
`dotnet build`

## Configure
Building the Hypar CLI requires an `appsettings.json` file.
