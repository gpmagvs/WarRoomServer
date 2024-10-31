using Microsoft.EntityFrameworkCore;
using WarRoomServer.Data.Entities;

namespace WarRoomServer.Data.Contexts
{
    public class WarRoomDbContext : DbContext
    {
        public DbSet<FieldInfo> Fields { get; set; }

        public WarRoomDbContext(DbContextOptions<WarRoomDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}
