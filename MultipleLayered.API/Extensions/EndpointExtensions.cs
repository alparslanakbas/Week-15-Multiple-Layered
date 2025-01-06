public static class EndpointExtensions
{
    public static void RegisterEndpoints(this WebApplication app)
    {
        var endpointTypes = typeof(Program).Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpointDefinition))
            && !t.IsAbstract && !t.IsInterface);

        foreach (var type in endpointTypes)
        {
            var instance = ActivatorUtilities.CreateInstance(app.Services, type) as IEndpointDefinition;
            instance?.RegisterEndpoints(app);
        }
    }
}