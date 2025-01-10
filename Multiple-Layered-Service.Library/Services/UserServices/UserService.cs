namespace Multiple_Layered_Service.Library.Services.UserServices
{
    public class UserService : IUserService
    {
        readonly IUnitOfWork _unitOfWork;
        readonly ILogger<UserService> _logger;
        readonly UserManager<User> _userManager;
        readonly SignInManager<User> _signInManager;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IEnumerable<ListAllUserDto>> GetAllAsync()
        {
            try
            {
                var userDtoList = await _unitOfWork.GetFromCacheAsync<IEnumerable<ListAllUserDto>>("users_cache");
                if (userDtoList is not null)
                {
                    _logger.LogInformation("Kullanıcılar cache'den getirildi");
                    return userDtoList;
                }

                var users = await _unitOfWork.Users.GetAllAsync();
                userDtoList = users.Select(user => new ListAllUserDto(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.UserName!,
                    user.Email!,
                    user.PhoneNumber ?? string.Empty,
                    user.EmailConfirmed,
                    user.PhoneNumberConfirmed,
                    user.TwoFactorEnabled
                ));

                await _unitOfWork.SetToCacheAsync("users_cache", userDtoList);
                _logger.LogInformation("Tüm kullanıcılar başarıyla getirildi");
                return userDtoList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcıları getirirken hata oluştu");
                throw;
            }
        }
        public async Task<ListAllUserDto> GetByIdAsync(Guid id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user is null)
                {
                    _logger.LogWarning("Kullanıcı bulunamadı: {Id}", id);
                    return null;
                }

                var userDto = new ListAllUserDto(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.UserName!,
                    user.Email!,
                    user.PhoneNumber ?? string.Empty,
                    user.EmailConfirmed,
                    user.PhoneNumberConfirmed,
                    user.TwoFactorEnabled
                );

                _logger.LogInformation("Kullanıcı başarıyla getirildi: {Id}", id);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı getirirken hata oluştu: {Id}", id);
                throw;
            }
        }
        public async Task<IdentityResult> UpdateAsync(UpdateUserDto updateUserDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var user = await _userManager.FindByIdAsync(updateUserDto.Id.ToString());
                if (user is null)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "Kullanıcı bulunamadı"
                    });
                }

                // Temel bilgileri güncelle
                user.FirstName = updateUserDto.FirstName;
                user.LastName = updateUserDto.LastName;
                user.UserName = updateUserDto.UserName;
                user.Email = updateUserDto.Email;
                user.PhoneNumber = updateUserDto.PhoneNumber;
                user.EmailConfirmed = updateUserDto.EmailConfirmed;
                user.PhoneNumberConfirmed = updateUserDto.PhoneNumberConfirmed;
                user.TwoFactorEnabled = updateUserDto.TwoFactorEnabled;

                // Email değişmişse
                if (user.Email != updateUserDto.Email)
                {
                    user.EmailConfirmed = false;
                    await _userManager.UpdateSecurityStampAsync(user);
                }

                // Kullanıcı bilgilerini güncelle eğer hata çıkarsa işlemi geri al
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    await _unitOfWork.RollbackAsync();
                    return updateResult;
                }

                // Şifre değiştirme
                if (!string.IsNullOrEmpty(updateUserDto.CurrentPassword) &&
            !string.IsNullOrEmpty(updateUserDto.NewPassword))
                {
                    var passwordResult = await _userManager.ChangePasswordAsync(
                        user,
                        updateUserDto.CurrentPassword,
                        updateUserDto.NewPassword
                    );

                    if (!passwordResult.Succeeded)
                    {
                        await _unitOfWork.RollbackAsync();
                        return passwordResult;
                    }

                    // Güvenlik damgasını güncelle
                    await _userManager.UpdateSecurityStampAsync(user);
                }

                await _unitOfWork.ClearCacheAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Kullanıcı başarıyla güncellendi: {Id}", updateUserDto.Id);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Kullanıcı güncellenirken hata oluştu: {Id}", updateUserDto.Id);

                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Kullanıcı güncellenirken beklenmeyen bir hata oluştu"
                });
            }

        }
        public async Task<IdentityResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

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

                var result = await _userManager.ChangePasswordAsync(user,
                    changePasswordDto.CurrentPassword,
                    changePasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    // Güvenlik damgasını güncelle
                    await _userManager.UpdateSecurityStampAsync(user);

                    // Tüm oturumları sonlandır
                    await _signInManager.SignOutAsync();

                    await _unitOfWork.CommitAsync();
                    _logger.LogInformation("Kullanıcı şifresi değiştirildi ve oturumlar sonlandırıldı: {Email}", user.Email);
                }
                else
                {
                    await _unitOfWork.RollbackAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Şifre değiştirme işlemi sırasında hata: {Email}", changePasswordDto.Email);
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Şifre değiştirme işlemi sırasında beklenmeyen bir hata oluştu"
                });
            }
        }
    }
}
