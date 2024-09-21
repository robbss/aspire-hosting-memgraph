using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.Memgraph;

/// <summary>
/// A resource that represents a Memgraph container.
/// </summary>
public class MemgraphContainerResource : ContainerResource, IResourceWithConnectionString
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemgraphContainerResource"/> class.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    public MemgraphContainerResource(string name) : base(name)
    {
        PrimaryEndpoint = new(this, "tcp");
    }

    /// <summary>
    /// Gets the connection string expression for the Memgraph instance.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression
    {
        get
        {
            if (this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var connectionStringAnnotation))
            {
                return connectionStringAnnotation.Resource.ConnectionStringExpression;
            }

            return ConnectionString;
        }
    }

    /// <summary>
    /// Gets the primary endpoint for the Memgraph instance.
    /// </summary>
    public EndpointReference PrimaryEndpoint { get; }

    private ReferenceExpression ConnectionString
    {
        get
        {
            return ReferenceExpression.Create($"bolt://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}");
        }
    }

    /// <summary>
    /// Gets the connection string for the Memgraph instance.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A connection string for the Memgraph instance in for them "bolt://{host}:{port}".</returns>
    public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        if (this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var connectionStringAnnotation))
        {
            return connectionStringAnnotation.Resource.GetConnectionStringAsync(cancellationToken);
        }

        return ConnectionStringExpression.GetValueAsync(cancellationToken);
    }
}