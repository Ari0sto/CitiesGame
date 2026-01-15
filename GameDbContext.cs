using CitiesGame.Entities;
using Microsoft.EntityFrameworkCore;

namespace CitiesGame
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<GameSession> Sessions { get; set; }
        public DbSet<GameMove> Moves { get; set; }
    }
}
