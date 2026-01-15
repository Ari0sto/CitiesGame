using System.ComponentModel.DataAnnotations;

namespace CitiesGame.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Имя должно быть от 3 до 20 символов")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(5, ErrorMessage = "Пароль должен быть не менее 5 символов")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}
