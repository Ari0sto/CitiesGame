using System.Text.Json.Serialization;

namespace CitiesGame.Entities
{
    public class GameMove
    {
        public int Id { get; set; }

        // Кто сказал слово
        public int UserId { get; set; } 
        public string Word { get; set; } = string.Empty;

        // Для какой сесии (ID)
        public int GameSessionId { get; set; }

        // Мув для связи EF но без зацикливания Swagger
        // P.S то что я сказал в шутку в обсуждении loop detected
        [JsonIgnore]
        public GameSession? GameSession { get; set; }
    }
}
