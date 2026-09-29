namespace SMC.Master.Infrastructure.Integrations.ServiceA;

// Adapter translating Master Portal application requests into the
// third-party ServiceA API contract (endpoint, auth, and field mapping
// documented in /integrations/service-a/integration-spec.md).
public class ServiceAAdapter
{
    public Task<string> SubmitAsync(object payload)
    {
        throw new NotImplementedException("Call the ServiceA external API and map the response.");
    }

    public Task<string> GetStatusAsync(string referenceId)
    {
        throw new NotImplementedException();
    }
}
