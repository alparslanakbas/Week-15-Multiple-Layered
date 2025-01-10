namespace Multiple_Layered_Service.Library.Services.UserServices
{
    public interface IUserService
    {
        Task<IEnumerable<ListAllUserDto>> GetAllAsync();
        Task<ListAllUserDto> GetByIdAsync(Guid id);
        Task<IdentityResult> UpdateAsync(UpdateUserDto updateUserDto);
        Task<IdentityResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        //Task DeleteAsync(Guid id);
    }
}
