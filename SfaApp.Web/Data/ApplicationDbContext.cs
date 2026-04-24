using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.Identity;

namespace SfaApp.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Territory> Territories => Set<Territory>();
    public DbSet<Distributor> Distributors => Set<Distributor>();
    public DbSet<SalesRoute> SalesRoutes => Set<SalesRoute>();
    public DbSet<RouteAssignment> RouteAssignments => Set<RouteAssignment>();
    public DbSet<DaySession> DaySessions => Set<DaySession>();
    public DbSet<UploadJob> UploadJobs => Set<UploadJob>();
    public DbSet<SyncQueueItem> SyncQueueItems => Set<SyncQueueItem>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Customer>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OutletCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ContactPerson).HasMaxLength(120);
            entity.Property(x => x.Phone).HasMaxLength(20);
            entity.Property(x => x.City).HasMaxLength(80);
            entity.Property(x => x.Latitude).HasPrecision(10, 7);
            entity.Property(x => x.Longitude).HasPrecision(10, 7);
            entity.Property(x => x.CreatedAtUtc);
            entity.Property(x => x.UpdatedAtUtc);
            entity.HasIndex(x => x.OutletCode).IsUnique();
            entity.HasIndex(x => x.CustomerCode).IsUnique();

            entity.HasOne(x => x.Route)
                .WithMany(x => x.Customers)
                .HasForeignKey(x => x.RouteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Territory)
                .WithMany()
                .HasForeignKey(x => x.TerritoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Distributor)
                .WithMany()
                .HasForeignKey(x => x.DistributorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Product>(entity =>
        {
            entity.Property(x => x.Sku).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.Mrp).HasPrecision(18, 2);
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.HasIndex(x => x.ProductCode).IsUnique();
        });

        builder.Entity<Territory>(entity =>
        {
            entity.Property(x => x.TerritoryCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TerritoryName).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.TerritoryCode).IsUnique();
        });

        builder.Entity<Distributor>(entity =>
        {
            entity.Property(x => x.DistributorCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DistributorName).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.DistributorCode).IsUnique();
        });

        builder.Entity<SalesRoute>(entity =>
        {
            entity.Property(x => x.RouteCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RouteName).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.RouteCode).IsUnique();
            entity.HasOne(x => x.Territory)
                .WithMany(x => x.Routes)
                .HasForeignKey(x => x.TerritoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Distributor)
                .WithMany(x => x.Routes)
                .HasForeignKey(x => x.DistributorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RouteAssignment>(entity =>
        {
            entity.HasOne(x => x.Route)
                .WithMany(x => x.Assignments)
                .HasForeignKey(x => x.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.RouteId, x.IsActive });
            entity.HasIndex(x => new { x.RepUserId, x.IsActive });
        });

        builder.Entity<DaySession>(entity =>
        {
            entity.Property(x => x.StartDayLat).HasPrecision(10, 7);
            entity.Property(x => x.StartDayLong).HasPrecision(10, 7);
            entity.Property(x => x.EndDayLat).HasPrecision(10, 7);
            entity.Property(x => x.EndDayLong).HasPrecision(10, 7);
            entity.HasOne(x => x.SelectedRoute)
                .WithMany()
                .HasForeignKey(x => x.SelectedRouteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.RepUserId, x.BusinessDate });
            entity.HasIndex(x => x.ClientGeneratedUuid).IsUnique();
        });

        builder.Entity<UploadJob>(entity =>
        {
            entity.Property(x => x.UploadType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(200).IsRequired();
        });

        builder.Entity<SyncQueueItem>(entity =>
        {
            entity.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EntityClientUuid).HasMaxLength(60).IsRequired();
            entity.Property(x => x.LastErrorMessage).HasMaxLength(500);
            entity.HasIndex(x => x.EntityClientUuid).IsUnique();
            entity.HasIndex(x => x.SyncStatus);
        });

        builder.Entity<Visit>(entity =>
        {
            entity.Property(x => x.Outcome).HasMaxLength(200);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.CheckinLat).HasPrecision(10, 7);
            entity.Property(x => x.CheckinLong).HasPrecision(10, 7);
            entity.Property(x => x.CustomerRefLat).HasPrecision(10, 7);
            entity.Property(x => x.CustomerRefLong).HasPrecision(10, 7);
            entity.Property(x => x.CheckoutLat).HasPrecision(10, 7);
            entity.Property(x => x.CheckoutLong).HasPrecision(10, 7);
            entity.Property(x => x.GeoDistanceMeters).HasPrecision(10, 2);
            entity.HasIndex(x => x.ClientGeneratedUuid).IsUnique();
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Visits)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Route)
                .WithMany()
                .HasForeignKey(x => x.RouteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DaySession)
                .WithMany()
                .HasForeignKey(x => x.DaySessionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SalesOrder>(entity =>
        {
            entity.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.GrossAmount).HasPrecision(18, 2);
            entity.Property(x => x.NetAmount).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.HasIndex(x => x.OrderNumber).IsUnique();
            entity.HasIndex(x => x.ClientGeneratedUuid).IsUnique();
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.SalesOrders)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Route)
                .WithMany()
                .HasForeignKey(x => x.RouteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Distributor)
                .WithMany()
                .HasForeignKey(x => x.DistributorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DaySession)
                .WithMany()
                .HasForeignKey(x => x.DaySessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Visit)
                .WithMany()
                .HasForeignKey(x => x.VisitId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<SalesOrderLine>(entity =>
        {
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasOne(x => x.SalesOrder)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ApprovalRequest>(entity =>
        {
            entity.Property(x => x.DecisionRemarks).HasMaxLength(500);
            entity.HasOne(x => x.SalesOrder)
                .WithOne(x => x.ApprovalRequest)
                .HasForeignKey<ApprovalRequest>(x => x.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}