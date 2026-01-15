namespace CitiesGame.Entities
{
    public class GameSession
    {
        public int Id { get; set; }

        // Player 1 host
        public int Player1Id { get; set; }

        // Player 2 (кто подкл)
        public int? Player2Id { get; set; }

        // ID игрока (тот кто ходит)
        public int CurrentTurnUserId { get; set; }

        public bool IsActive { get; set; }

        // Word's List
        public List<GameMove> Moves { get; set; } = new List<GameMove>();
    }
}
