using Multiple_Layered_DataAccess.Library.Models;

namespace Multiple_Layered_Service.Library.Services.AuthServices
{
    public class AuthService : IAuthService
    {
        readonly UserManager<User> _userManager;
        readonly SignInManager<User> _signInManager;
        readonly ILogger<AuthService> _logger;


        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<SignInResult> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user is null)
                {
                    _logger.LogWarning("Giriş başarısız: Kullanıcı bulunamadı - {Email}", loginDto.Email);
                    return SignInResult.Failed;
                }

                // Kullanıcı kilitli mi kontrol et
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    _logger.LogWarning("Kullanıcı hesabı kilitli: {Email}, Kilit bitiş: {LockoutEnd}", loginDto.Email, lockoutEnd);
                    return SignInResult.LockedOut;
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
                }
                else
                {
                    var failCount = await _userManager.GetAccessFailedCountAsync(user);
                    _logger.LogWarning("Başarısız giriş denemesi - {Email}, Deneme sayısı: {FailCount}", loginDto.Email, failCount + 1);

                    // Başarısız giriş sayısını artır
                    await _userManager.AccessFailedAsync(user);
                }

                return signInResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriş işlemi sırasında hata: {Email}", loginDto.Email);
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDto registerDto)
        {
            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.UserName,
                Email = registerDto.Email
            };

            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return IdentityResult.Failed(new IdentityError { Description = "Bu Email Zaten Kayıtlı" });

            return await _userManager.CreateAsync(user, registerDto.Password);

        }
    }
}
