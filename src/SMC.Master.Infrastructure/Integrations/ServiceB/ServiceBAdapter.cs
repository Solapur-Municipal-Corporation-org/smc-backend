namespace SMC.Master.Infrastructure.Integrations.ServiceB;

// Adapter translating Master Portal application requests into the
// third-party ServiceB API contract (endpoint, auth, and field mapping
// documented in /integrations/service-b/integration-spec.md).
public class ServiceBAdapter
{
    public Task<string> SubmitAsync(object payload)
    {
        throw new NotImplementedException("Call the ServiceB external API and map the response.");
    }

    public Task<string> GetStatusAsync(string referenceId)
    {
        throw new NotImplementedException();
    }
}
