namespace SMC.Master.Infrastructure.Integrations.ServiceC;

// Adapter translating Master Portal application requests into the
// third-party ServiceC API contract (endpoint, auth, and field mapping
// documented in /integrations/service-c/integration-spec.md).
public class ServiceCAdapter
{
    public Task<string> SubmitAsync(object payload)
    {
        throw new NotImplementedException("Call the ServiceC external API and map the response.");
    }

    public Task<string> GetStatusAsync(string referenceId)
    {
        throw new NotImplementedException();
    }
}
