namespace Multiple_Layered_Service.Library.Services.AuthRepo
{
    public interface IAuthService
    {
        Task<SignInResult> LoginAsync(LoginDto loginDto);
        Task<IdentityResult> RegisterAsync(RegisterDto registerDto);
        Task LogoutAsync();
        Task<IdentityResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    }
}
