using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MallMinder.Models;
using MallMinder.Models.ViewModels;


namespace MallMinder.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<Mall> Mall { get; set; } // Define DbSet for Mall entity
        public DbSet<MallManagers> MallManagers { get; set; } // Define DbSet for Mall entity
        public DbSet<Floor> Floor { get; set; }
        public DbSet<Room> Room { get; set; }
        public DbSet<RentType> RentType { get; set; }
        public DbSet<Rent> Rent { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
