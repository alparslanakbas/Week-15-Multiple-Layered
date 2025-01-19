namespace Extensions.Endpoints;

public interface IEndpointDefinition
{
    void RegisterEndpoints(WebApplication app);
}