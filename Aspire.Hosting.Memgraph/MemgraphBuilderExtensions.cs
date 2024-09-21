using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.Memgraph;

/// <summary>
/// Provides extension methods for adding Memgraph resources to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class MemgraphBuilderExtensions
{
    /// <summary>
    /// Adds a Memgraph resource to the application model. A container is used for local development.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="port">The host port used when launching the container. If null a random port will be assigned.</param>
    /// <param name="logPort">The host port used for logs when launching the container. If null a random port will be assigned.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MemgraphContainerResource> AddMemgraph(this IDistributedApplicationBuilder builder, string name, int? port = default, int? logPort = default)
    {
        var memgraph = new MemgraphContainerResource(name);

        return builder.AddResource(memgraph)
            .WithEndpoint(port: port, targetPort: 7687, name: "tcp")
            .WithEndpoint(port: logPort, targetPort: 7444, name: "logs")
            .WithImage(MemgraphImageTags.Image, MemgraphImageTags.Tag)
            .WithImageRegistry(MemgraphImageTags.Registry);
    }

    /// <summary>
    /// Adds a Memgraph Lab instance to the application model.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="configureContainer">Callback to configure the Memgraph Lab container resource.</param>
    /// <param name="containerName">The name of the container.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MemgraphContainerResource> WithMemgraphLab(this IResourceBuilder<MemgraphContainerResource> builder, Action<IResourceBuilder<MemgraphLabContainerResource>>? configureContainer = default, string? containerName = default)
    {
        if (builder.ApplicationBuilder.Resources.OfType<MemgraphLabContainerResource>().SingleOrDefault() is { } existingResource)
        {
            configureContainer?.Invoke(builder.ApplicationBuilder.CreateResourceBuilder(existingResource));
            return builder;
        }

        var labContainer = new MemgraphLabContainerResource(containerName ?? $"{builder.Resource.Name}-lab");

        var labBuilder = builder.ApplicationBuilder.AddResource(labContainer)
            .WithImage(MemgraphImageTags.LabImage, MemgraphImageTags.LabTag)
            .WithImageRegistry(MemgraphImageTags.Registry)
            .WithHttpEndpoint(targetPort: 3000, name: "http")
            .ExcludeFromManifest();

#pragma warning disable ASPIREEVENTING001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        builder.ApplicationBuilder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>((@event, ct) =>
        {
            var instances = builder.ApplicationBuilder.Resources.OfType<MemgraphContainerResource>();
            var instance = instances.FirstOrDefault();

            if (instance is null || !instance.PrimaryEndpoint.IsAllocated)
            {
                return Task.CompletedTask;
            }

            var endpoint = instance.PrimaryEndpoint;

            labBuilder
                .WithEnvironment("QUICK_CONNECT_MG_HOST", endpoint.ContainerHost)
                .WithEnvironment("QUICK_CONNECT_MG_PORT", endpoint.Port.ToString());

            return Task.CompletedTask;
        });
#pragma warning restore ASPIREEVENTING001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        configureContainer?.Invoke(labBuilder);

        return builder;
    }

    /// <summary>
    /// Adds a named volume for the data folder to a Memgraph container resource.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the volume.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MemgraphContainerResource> WithDataVolume(this IResourceBuilder<MemgraphContainerResource> builder, string? name = default, bool isReadOnly = false)
    {
        return builder
            .WithVolume(name ?? $"volume-{builder.Resource.Name}-data", "/var/lib/memgraph", isReadOnly);
    }

    /// <summary>
    /// Adds a bind mount for the data folder to a Memgraph container resource.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MemgraphContainerResource> WithDataBindMount(this IResourceBuilder<MemgraphContainerResource> builder, string source, bool isReadOnly = false)
    {
        return builder
            .WithBindMount(source, "/var/lib/memgraph", isReadOnly);
    }

    public static IResourceBuilder<MemgraphLabContainerResource> WithHostPort(this IResourceBuilder<MemgraphLabContainerResource> builder, int? port)
    {
        return builder
            .WithEndpoint("http", endpoint =>
            {
                endpoint.Port = port;
            });
    }
}