using Azure.Messaging;
using CitiesGame.Entities;
using CitiesGame.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitiesGame.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameDbContext _context;

        public GameController(GameDbContext context)
        {
            _context = context;
        }

        // 1. Create new Game
        // POST
        [HttpPost("start")]
        public async Task<IActionResult> StartGame(int userId)
        {
            // Проверка существов. игрока
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Игрок не найден");

            // Create session (ход того кто создал)
            var session = new GameSession
            {
                Player1Id = userId,
                Player2Id = null,
                CurrentTurnUserId = userId,
                IsActive = true
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Игра создана", SessionId = session.Id });
        }

        // Game List
        // GET
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveGames()
        {
            // поиск след. образом - где есть isActive = true, но нету Player2
            var games = await _context.Sessions.Where(s => s.IsActive && s.Player2Id == null).Select(s => new {s.Id, Player1Id =  s.Player1Id}).ToListAsync();
            return Ok(games);

        }

        // Connect to the game
        // POST
        [HttpPost("join")]
        public async Task<IActionResult> JoinGame(int sessionId, int userId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);

            // Проверки
            if (session == null) return NotFound("Игра не найдена");
            if (!session.IsActive) return BadRequest("Эта игра уже завершена");
            if (session.Player2Id != null) return BadRequest("В игре уже два игрока");
            if (session.Player1Id == userId) return BadRequest("Нельзя играть самому с собой");

            // second player
            session.Player2Id = userId;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Вы присоединились к игре!", SessionId = session.Id });
        }

        // Этот запрос с ошибкой!

        // GET Session state
        // Проверка хода раз в 2 секунды
        [HttpGet("{sessionId}/state")]
        public async Task<IActionResult> GetGameState([FromRoute] int sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Moves) // история ходов
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound("Игра не найдена");

            // Получаем последнее слово
            var lastMove = session.Moves.LastOrDefault();
            // Проверка слова на null
            string? lastWord = lastMove?.Word;

            // Проверка буквы, на которую нужно будет ответить
            char? nextLetter = null;
            if (lastWord != null)
            {
                nextLetter = GetLastValidLetter(lastWord);
            }

            return Ok(new
            {
                IsActive = session.IsActive,
                CurrentTurnUserId = session.CurrentTurnUserId,
                LastWord = lastWord,
                NextLetter = nextLetter, // Подсказка для игрока
                History = session.Moves.Select(m => new { m.UserId, m.Word }).ToList() // Вся история для чата
            });
        }

        // POST
        [HttpPost("word")]
        public async Task<IActionResult> MakeMove([FromBody] MakeMoveRequest request)
        {
            // Загрузка сессии с ходами
            var session = await _context.Sessions
                .Include(s => s.Moves)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId);

            // Проверки
            if (session == null) return NotFound("Игра не найдена");
            if (!session.IsActive) return BadRequest("Игра окончена");
            if (session.CurrentTurnUserId != request.UserId) return BadRequest("Сейчас не ваш ход!");

            string cleanWord = request.Word.Trim();

            // Проверка было ли слово в этой игре (без учета регистра)
            if (session.Moves.Any(m => m.Word.ToLower() == cleanWord.ToLower())) return BadRequest("Это слово уже было!");

            // Проверка буквы на которую нужно ответить
            var lastMove = session.Moves.LastOrDefault();
            if (lastMove != null)
            {
                char requiredChar = GetLastValidLetter(lastMove.Word); // Буква на которую следует отвечать
                char firstChar = char.ToLower(cleanWord[0]); // первая буква слова

                if (requiredChar != firstChar) return BadRequest($"Слово должно начинаться на букву '{requiredChar}'");
            }

            // Сохранение хода
            // фикс. ход
            var move = new GameMove
            {
                UserId = request.UserId,
                GameSessionId = request.SessionId,
                Word = cleanWord
            };
            _context.Moves.Add(move);

            // Передача хода другому игроку
            session.CurrentTurnUserId = (session.Player1Id == request.UserId)
                ? session.Player2Id!.Value
                : session.Player1Id;

            await _context.SaveChangesAsync();

            return Ok(new {Message = "Ход принят", Word = cleanWord });
        }

        // Проверка валид. буквы (фильтр по ы,ъ,ь)
        private char GetLastValidLetter(string word)
        {
            string badLetters = "ьыъ";
            word = word.ToLower();

            // Перебор букв с конца
            for (int i = word.Length - 1; i >= 0; i--)
            {
                char c = word[i];
                // Если буква не входит в список "плохих" букв то возвращаем её
                if (!badLetters.Contains(c))
                {
                    return c;
                }
            }

            return word[word.Length - 1];
        }

        // POST (сдаться)
        [HttpPost("surrender")]
        public async Task<IActionResult> Surrender(int sessionId, int userId)
        {
            // Поиск сессии
            var session = await _context.Sessions
                .Include(s => s.Moves) // подшрузка ходов для удаления
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound("Игра не найдена");

            // Проверка на участие игрока
            if (session.Player1Id != userId && session.Player2Id != userId)
            {
                return BadRequest("Вы не участник этой игры");
            }

            // Опред. победителя
            int winnerId = (session.Player1Id == userId)
            ? session.Player2Id ?? 0 // если сдался Игрок 1 то победил Игрок 2
            : session.Player1Id; // иначе Игрок 1


            // Чистка БД
            // Moves at first
            _context.Moves.RemoveRange(session.Moves);

            // and delete session
            _context.Sessions.Remove(session);

            // save changes
            await _context.SaveChangesAsync();

            return Ok(new { 
                Message = "Игра окончена, сессия удалена.",
                WinnerId = winnerId
            });
        }
    }
}
