using Kismet.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Kismet.DataAccess.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customer { get; set; } = null!;
    public DbSet<Order> Order { get; set; } = null!;
    public DbSet<FinancialTransaction> FinancialTransaction { get; set; } = null!;
    public DbSet<Treasury> Treasury { get; set; } = null!;
    public DbSet<Billing> Billing { get; set; } = null!;
    public DbSet<Payment> Payment { get; set; } = null!;
    public DbSet<Shipment> Shipment { get; set; } = null!;
    public DbSet<Fabric> Fabric { get; set; } = null!;
    public DbSet<Batch> Batch { get; set; } = null!;
}

