namespace Multiple_Layered.API.Endpoints.Role
{
    public class RoleEndpoint : IEndpointDefinition
    {
        readonly ILogger<RoleEndpoint> _logger;

        public RoleEndpoint(ILogger<RoleEndpoint> logger)
        {
            _logger = logger;
        }

        public void RegisterEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/role");

            group.MapPost("/assign", AssignRoleAsync)
                .WithName("AssignRole")
                .WithDescription("Kullanıcıya Admin Rolü Atar")
                .Produces(200)
                .Produces(400)
                .Produces(500)
                .RequireAuthorization();
        }


        private async Task<IResult> AssignRoleAsync(IRoleService roleService, [FromBody] AssignRoleDto assignRoleDto)
        {
            try
            {
                var result = await roleService.AssignRoleAsync(assignRoleDto);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

    }
}
