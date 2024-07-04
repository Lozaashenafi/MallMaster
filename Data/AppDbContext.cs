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

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
