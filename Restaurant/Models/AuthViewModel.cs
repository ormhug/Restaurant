namespace Restaurant.Models
{
    public class AuthViewModel
    {
        // Это свойство для формы входа
        public LoginViewModel Login { get; set; } = new LoginViewModel();

        // Это свойство для формы регистрации
        public RegisterViewModel Register { get; set; } = new RegisterViewModel();
    }
}