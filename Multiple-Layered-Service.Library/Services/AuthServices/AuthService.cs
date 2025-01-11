namespace Multiple_Layered_Service.Library.Services.AuthServices
{
    public class AuthService : IAuthService
    {
        readonly UserManager<User> _userManager;
        readonly SignInManager<User> _signInManager;
        readonly ILogger<AuthService> _logger;
        readonly ITokenService _tokenService;


        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AuthService> logger, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _tokenService = tokenService;
        }

        public async Task<(SignInResult SignInResult, Token? Token)> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user is null)
                {
                    _logger.LogWarning("Giriş başarısız: Kullanıcı bulunamadı - {Email}", loginDto.Email);
                    return (SignInResult.Failed, null);
                }

                // Kullanıcı kilitli mi kontrol et
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    _logger.LogWarning("Kullanıcı hesabı kilitli: {Email}, Kilit bitiş: {LockoutEnd}", loginDto.Email, lockoutEnd);
                    return (SignInResult.LockedOut, null);
                }

                var signInResult = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    loginDto.Password,
                    lockoutOnFailure: true
                );

                if (signInResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    await _userManager.ResetAccessFailedCountAsync(user);
                    _logger.LogInformation("Başarılı giriş: {Email}", loginDto.Email);

                    var token = await _tokenService.CreateToken(user);

                    user.RefreshToken = token.RefreshToken;
                    user.RefreshTokenEndDate = token.RefreshTokenExpiration;
                    await _userManager.UpdateAsync(user);

                    return (signInResult, token);
                }
                else
                {
                    var failCount = await _userManager.GetAccessFailedCountAsync(user);
                    _logger.LogWarning("Başarısız giriş denemesi - {Email}, Deneme sayısı: {FailCount}", loginDto.Email, failCount + 1);

                    // Başarısız giriş sayısını artır
                    await _userManager.AccessFailedAsync(user);
                    return (signInResult, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriş işlemi sırasında hata: {Email}", loginDto.Email);
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            var userId = _signInManager.Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is not null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenEndDate = DateTime.Now;
                    await _userManager.UpdateAsync(user);
                }
            }

            await _signInManager.SignOutAsync();
        }

        public async Task<Response> RegisterAsync(RegisterDto registerDto)
        {
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return new Response
                {
                    Succeeded = false,
                    Message = "Bu Email Zaten Kayıtlı"
                };

            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.UserName,
                Email = registerDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                return new Response
                {
                    Succeeded = false,
                    Message = result.Errors.First().Description
                };

            await _userManager.AddToRoleAsync(user, "User");

            var token = await _tokenService.CreateToken(user);

            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenEndDate = token.RefreshTokenExpiration;

            await _userManager.UpdateAsync(user);

            return new Response
            {
                Succeeded = true,
                Message = "Kayıt işlemi başarılı",
                Token = token
            };
        }
    }
}
