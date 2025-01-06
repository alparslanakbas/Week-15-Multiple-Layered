namespace Multiple_Layered_Service.Library.Dtos.AuthDtos
{
   public record ChangePasswordDto
   (
    string Email,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
   )
    {
        public bool PasswordsMatch => NewPassword == ConfirmNewPassword;
    }
}
