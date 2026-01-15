using CitiesGame.Entities;
using CitiesGame.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitiesGame.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GameDbContext _context;

        // БД конструктор
        public AuthController(GameDbContext context)
        {
            _context = context;
        }

        // POST (регистрация)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Проверка на наличие такого пользователя
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                return BadRequest("Такой пользователь уже существует");
            }

            // Создание пользователя
            var newUser = new User()
            {
                Username = request.Username,
                Password = request.Password
            };

            // Сохранение юзера
            _context.Users.Add(newUser);

            // сохр. в БД
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Регистрация успешна!", UserId = newUser.Id });
        }

        // POST (авторизация)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Поиск по имени и паролю
            var user = await _context.Users.FirstOrDefaultAsync( u => u.Username == request.Username && u.Password == request.Password);

            // Если не нашли, то проброс 401
            if (user == null)
            {
                return Unauthorized("Неправильный логин или пароль");
            }

            // return ID user'a
            return Ok(new { UserId = user.Id, Username = user.Username });
        }
    }
}
