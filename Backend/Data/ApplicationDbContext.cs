using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<ProductMember> ProductMembers => Set<ProductMember>();
    public DbSet<InBodyMeasurement> InBodyMeasurements => Set<InBodyMeasurement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Base constraints for the programs table
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Id).HasColumnName("ProductId");
            entity.Property(product => product.Name).HasMaxLength(200).IsRequired();
            entity.Property(product => product.RegisteredAt).HasColumnType("date");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("Members");
            entity.HasKey(member => member.Id);
            entity.Property(member => member.Id).HasColumnName("MemberId").HasMaxLength(100).IsUnicode(false).ValueGeneratedNever();
            entity.HasIndex(member => member.UserToken).IsUnique();
            entity.Property(member => member.UserToken).HasMaxLength(15).IsUnicode(false).IsRequired();
            entity.Property(member => member.Name).HasMaxLength(100).IsRequired();
            entity.Property(member => member.Gender).HasMaxLength(1).IsUnicode(false).IsRequired();
        });

        modelBuilder.Entity<ProductMember>(entity =>
        {
            entity.ToTable("ProductMembers");
            entity.HasKey(link => link.Id);
            entity.Property(link => link.Id).HasColumnName("ProductMemberId");
            entity.HasIndex(link => new { link.ProductId, link.MemberId }).IsUnique();
            entity.Property(link => link.MemberId).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(link => link.AddedAt).HasColumnType("datetime2(0)");
            entity.HasOne<Product>().WithMany().HasForeignKey(link => link.ProductId);
            entity.HasOne<Member>().WithMany().HasForeignKey(link => link.MemberId);
        });

        modelBuilder.Entity<InBodyMeasurement>(entity =>
        {
            entity.ToTable("InBodyMeasurements");
            entity.HasKey(measurement => measurement.Id);
            entity.Property(measurement => measurement.Id).HasColumnName("MeasurementId");
            entity.Property(measurement => measurement.MemberId).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(measurement => measurement.MeasuredAt).HasColumnType("datetime2(0)");
            entity.Property(measurement => measurement.EquipSerial).HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(measurement => measurement.Smm).HasPrecision(6, 2);
            entity.Property(measurement => measurement.Bfm).HasPrecision(6, 2);
            entity.Property(measurement => measurement.Weight).HasPrecision(6, 2);
            entity.Property(measurement => measurement.Pbf).HasPrecision(6, 2);
            entity.HasIndex(measurement => new { measurement.MemberId, measurement.MeasuredAt, measurement.EquipSerial }).IsUnique();
            entity.HasOne<Member>().WithMany().HasForeignKey(measurement => measurement.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
