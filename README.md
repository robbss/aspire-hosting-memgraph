# Aspire.Hosting.Memgraph

Provides extension methods and resource definitions for .NET Aspire AppHost to configure a Memgraph resource.

## Getting started

### Install the package

In your AppHost Project, install the .NET Aspire Memgraph Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Aspire.Hosting.Memgraph
```

## Usage example

Then, in the _Program.cs_ file of `AppHost`, add a Memgraph resource and consume the connection using the following methods:

```csharp
var memegraph = builder
    .AddMemgraph("example-name");

var myservice = builder
    .AddProject<Projects.MyService>()
    .WithReference(memegraph);
```

### Memgraph Lab

An instance of [Memgraph Lab](https://memgraph.com/docs/data-visualization), configured with quick connect can also be included with the following method:

```csharp
var memegraph = builder
    .AddMemgraph("example-name")
    .WithMemgraphLab();
```

## Additional resources

https://memgraph.com/docs/getting-started/install-memgraph/docker
