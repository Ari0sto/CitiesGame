using System.ComponentModel.DataAnnotations;

namespace CitiesGame.Models
{
    public class MakeMoveRequest
    {
        [Required(ErrorMessage = "ID сессии обязателен")]
        public int SessionId { get; set; }

        [Required (ErrorMessage = "ID пользователя обязателен")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Слово не может быть пустым")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Слово должно быть от 2 до 50 букв")]

        // Допуск городов на кирилице
        [RegularExpression(@"^[а-яА-ЯёЁ\-]+$", ErrorMessage = "Используйте только кирилицу (и дефис для составных названий)")]
        public string Word { get; set; } = string.Empty;
    }
}
