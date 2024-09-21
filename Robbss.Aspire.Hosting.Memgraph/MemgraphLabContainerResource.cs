using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

public class MemgraphLabContainerResource(string name) : ContainerResource(name)
{
    private EndpointReference? primaryEndpoint;

    /// <summary>
    /// Gets the primary endpoint for the Memgraph lab instance.
    /// </summary>
    public EndpointReference PrimaryEndpoint
    {
        get
        {
            return primaryEndpoint ??= new(this, "http");
        }
    }
}