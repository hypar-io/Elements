## How to run benchmarks

From this folder, run:

```
dotnet run -c release
```

or for more specific benchmarks:

```
dotnet run -c release --filter '<benchmark class name>'
```

Look for outputs in `BenchmarkDotNet.Artifacts`. `.speedscope.json` files can be visualized at https://speedscope.app.