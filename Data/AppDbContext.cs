using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MallMinder.Models;
using MallMinder.Models.ViewModels;

namespace MallMinder.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<Mall> Malls { get; set; } // Define DbSet for Mall entity
        public DbSet<RoomOccupancies> RoomOccupancies { get; set; } // Define DbSet for Mall entity
        public DbSet<MallManagers> MallManagers { get; set; } // Define DbSet for Mall entity
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RentType> RentTypes { get; set; }
        public DbSet<Rent> Rents { get; set; }
        public DbSet<PricePerCare> PricePerCares { get; set; }
        public DbSet<RoomPrice> RoomPrices { get; set; }
        public DbSet<MaintenanceStatusType> MaintenanceStatusTypes { get; set; }
        public DbSet<MaintenanceStatus> MaintenanceStatuss { get; set; }
        public DbSet<MaintenanceType> MaintenanceTypes { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseType> ExpenseTypes { get; set; }
        public DbSet<TenantPayment> TenantPayments { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
