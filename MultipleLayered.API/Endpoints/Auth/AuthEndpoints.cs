namespace Multiple_Layered.API.Endpoints.Auth;

public class AuthEndpoints : IEndpointDefinition
{
    readonly ILogger<AuthEndpoints> _logger;

    public AuthEndpoints(ILogger<AuthEndpoints> logger)
    {
        _logger = logger;
    }

    public void RegisterEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous();

        group.MapPost("/register", RegisterAsync)
            .AllowAnonymous();

        group.MapPost("/change-password", ChangePasswordAsync)
            .RequireAuthorization();

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization();
    }

    private async Task<IResult> LoginAsync(LoginDto loginDto, IAuthService authService)
    {
        try
        {
            var result = await authService.LoginAsync(loginDto);
            _logger.LogInformation("Kullanıcı Giriş Denemesi: {Email}", loginDto.Email);
            return result.Succeeded ? Results.Ok() : Results.BadRequest("Lütfen Bilgilerinizi Tekrar Kontrol Edin.!!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Giriş Sırasında Bir Hata Meydana Geldi: {Email}", loginDto.Email);
            return Results.StatusCode(500);
        }
    }

    private async Task<IResult> RegisterAsync(RegisterDto registerDto, IAuthService authService)
    {
        try
        {
            var result = await authService.RegisterAsync(registerDto);
            _logger.LogInformation("Kayıt Denemesi: {Email}", registerDto.Email);
            return result.Succeeded ? Results.Ok() : Results.BadRequest("Kayıt İşlemi Başarısız Oldu.!!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt Sırasında Hata Meydana Geldi: {Email}", registerDto.Email);
            return Results.StatusCode(500);
        }
    }

    private async Task<IResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto, IAuthService authService)
    {
        try
        {
            var result = await authService.ChangePasswordAsync(changePasswordDto);
            _logger.LogInformation("Şifre Değiştirme Denemesi: {Email}", changePasswordDto.Email);
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şifre Değiştirme Sırasında Bir Hata Meydana Geldi: {Email}", changePasswordDto.Email);
            return Results.StatusCode(500);
        }
    }

    private async Task<IResult> LogoutAsync(IAuthService authService)
    {
        try
        {
            await authService.LogoutAsync();
            _logger.LogInformation("Kullanıcı Çıkış Yaptı");
            return Results.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çıkış Yapılırken Hata Meydana Geldi");
            return Results.StatusCode(500);
        }
    }
}