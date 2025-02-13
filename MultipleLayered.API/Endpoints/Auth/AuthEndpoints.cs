﻿namespace Multiple_Layered.API.Endpoints.Auth;

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

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization();
    }

    private async Task<IResult> LoginAsync(LoginDto loginDto, IAuthService authService)
    {
        try
        {
            var (signInResult, token) = await authService.LoginAsync(loginDto);
            _logger.LogInformation("Kullanıcı Giriş Denemesi: {Email}", loginDto.Email);

            if (signInResult.IsLockedOut)
                return Results.BadRequest("Çok fazla başarısız deneme yaptınız. Daha sonra tekrar deneyin!");

            if (!signInResult.Succeeded)
                return Results.BadRequest("Lütfen Bilgilerinizi Tekrar Kontrol Edin!");

            return Results.Ok(new
            {
                Succeeded = true,
                Message = "Giriş başarılı",
                Token = token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Giriş Sırasında Bir Hata Meydana Geldi: {Email}", loginDto.Email);
            throw;
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
            throw;
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
            throw;
        }
    }
}