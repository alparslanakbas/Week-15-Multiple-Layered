using Multiple_Layered_DataAccess.Library.Models;

namespace Multiple_Layered_Service.Library.Services.AuthServices
{
    public class AuthService : IAuthService
    {
        readonly UserManager<User> _userManager;
        readonly SignInManager<User> _signInManager;
        

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<SignInResult> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if(user is null)
                return SignInResult.Failed;

            return await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);
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

        public async Task<IdentityResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            if (!changePasswordDto.PasswordsMatch)
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Şifreler Uyuşmuyor"
                });

            var user = await _userManager.FindByEmailAsync(changePasswordDto.Email);
            if (user is null)
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Kullanıcı Bulunamadı"
                });

            return await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
        }
    }
}
