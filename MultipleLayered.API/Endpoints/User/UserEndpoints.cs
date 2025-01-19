namespace Multiple_Layered.API.Endpoints.User
{
    public class UserEndpoints : IEndpointDefinition
    {

        readonly ILogger<UserEndpoints> _logger;

        public UserEndpoints(ILogger<UserEndpoints> logger)
        {
            _logger = logger;
        }

        public void RegisterEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/user");

            group.MapGet("/all", GetAllUsersAsync)
                .WithName("GetAllUsers")
                .WithDescription("Tüm Kullanıcıları Getirir")
                .Produces<IEnumerable<ListAllUserDto>>(StatusCodes.Status200OK)
                .Produces(500)
                .AddEndpointFilter<TimeRestrictFilter>()
                .AllowAnonymous();


            group.MapGet("/{id}", GetUserByIdAsync)
                .WithName("GetUserById")
                .WithDescription("Id'ye Göre Kullanıcı Getirir")
                .Produces<ListAllUserDto>(200)
                .Produces(404)
                .Produces(500)
                .AllowAnonymous();

            group.MapPut("/update", UpdateUserAsync)
                .WithName("UpdateUser")
                .WithDescription("Kullanıcı Bilgilerini Günceller")
                .Produces(200)
                .Produces<IEnumerable<IdentityError>>(400)
                .Produces(500)
                .RequireAuthorization();

            group.MapDelete("/{id}", DeleteUserAsync)
                .WithName("DeleteUser")
                .WithDescription("Kullanıcıyı Sil")
                .Produces(200)
                .Produces(500)
                .RequireAuthorization();

        }

        private async Task<IResult> GetAllUsersAsync(IUserService userService, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                var pagination = new Pagination(page, size);
                var result = await userService.GetAllAsync(pagination);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcılar Getirilirken Bir Hata Meydana Geldi");
                return Results.StatusCode(500);
            }
        }

        private async Task<IResult> GetUserByIdAsync(Guid id, IUserService userService)
        {
            try
            {
                var result = await userService.GetByIdAsync(id);
                if (result is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı Getirilirken Bir Hata Meydana Geldi");
                return Results.StatusCode(500);
            }
        }

        private async Task<IResult> UpdateUserAsync([FromBody] UpdateUserDto updateUserDto, IUserService userService)
        {
            try
            {
                var result = await userService.UpdateAsync(updateUserDto);

                if (result.Succeeded)
                    return Results.Ok();

                _logger.LogWarning("Kullanıcı güncellenirken validasyon hataları: {@Errors}", result.Errors);
                return Results.BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı güncellenirken bir hata meydana geldi");
                return Results.StatusCode(500);
            }
        }

        private async Task<IResult> DeleteUserAsync(Guid id, IUserService userService)
        {
            try
            {
                await userService.DeleteAsync(id);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı silinirken bir hata meydana geldi");
                return Results.StatusCode(500);
            }
        }
    }
}

