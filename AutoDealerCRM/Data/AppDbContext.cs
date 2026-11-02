using AutoDealerCRM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoDealerCRM.Data;

/// <summary>
/// EF Core DbContext. Наслідується від IdentityDbContext для підтримки
/// ASP.NET Core Identity (таблиці Users, Roles, Claims тощо). ЛР №4.
/// </summary>
public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CarLot>       CarLots       { get; set; }
    public DbSet<BuyerRequest> BuyerRequests { get; set; }
    public DbSet<AuditLogEntry> AuditLogs    { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Налаштування таблиці AuditLogs: Timestamp DESC індекс для швидкої фільтрації
        builder.Entity<AuditLogEntry>()
            .HasIndex(e => e.Timestamp);

        builder.Entity<AuditLogEntry>()
            .HasIndex(e => e.EventType);

        // CarLots: AES-зашифровані поля зберігаються як nvarchar(512)
        builder.Entity<CarLot>()
            .Property(c => c.VinCodeEncrypted)
            .HasColumnName("VinCode")
            .HasMaxLength(512);

        builder.Entity<CarLot>()
            .Property(c => c.AuctionCostEncrypted)
            .HasColumnName("AuctionCost")
            .HasMaxLength(512);

        // BuyerRequests: зашифрований телефон
        builder.Entity<BuyerRequest>()
            .Property(b => b.PhoneEncrypted)
            .HasColumnName("Phone")
            .HasMaxLength(512);
    }
}
