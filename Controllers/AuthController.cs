using CitiesGame.Entities;
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
        public async Task<IActionResult> Register(string username, string password)
        {
            // Проверка на наличие такого пользователя
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (existingUser != null)
            {
                return BadRequest("Такой пользователь уже существует");
            }

            // Создание пользователя
            var newUser = new User()
            {
                Username = username,
                Password = password
            };

            // Сохранение юзера
            _context.Users.Add(newUser);

            // сохр. в БД
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Регистрация успешна!", UserId = newUser.Id });
        }

        // POST (авторизация)
        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Поиск по имени и паролю
            var user = await _context.Users.FirstOrDefaultAsync( u => u.Username == username & u.Password == password);

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
