namespace Restaurant.Models
{
    public class AuthViewModel
    {
        //свойство для формы входа
        public LoginViewModel Login { get; set; } = new LoginViewModel();

        //свойство для формы регистрации
        public RegisterViewModel Register { get; set; } = new RegisterViewModel();
    }
}