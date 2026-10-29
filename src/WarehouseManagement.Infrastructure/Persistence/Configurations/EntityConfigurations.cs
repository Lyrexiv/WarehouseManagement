using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.Id);
        b.HasIndex(u => u.Username).IsUnique();
        b.HasIndex(u => u.Email).IsUnique();
        b.Property(u => u.Username).HasMaxLength(50).IsRequired();
        b.Property(u => u.Email).HasMaxLength(200).IsRequired();
        b.Property(u => u.FullName).HasMaxLength(100).IsRequired();
        b.Property(u => u.PhoneNumber).HasMaxLength(20);
        b.HasQueryFilter(u => !u.IsDeleted);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.HasKey(r => r.Id);
        b.HasIndex(r => r.Name).IsUnique();
        b.Property(r => r.Name).HasMaxLength(50).IsRequired();
        b.HasQueryFilter(r => !r.IsDeleted);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.HasKey(ur => new { ur.UserId, ur.RoleId });
        b.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> b)
    {
        b.HasKey(w => w.Id);
        b.HasIndex(w => w.Code).IsUnique();
        b.Property(w => w.Code).HasMaxLength(20).IsRequired();
        b.Property(w => w.Name).HasMaxLength(200).IsRequired();
        b.Property(w => w.Address).HasMaxLength(500).IsRequired();
        b.Property(w => w.Capacity).HasPrecision(18, 2);
        b.HasQueryFilter(w => !w.IsDeleted);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.HasKey(c => c.Id);
        b.Property(c => c.Name).HasMaxLength(100).IsRequired();
        b.Property(c => c.Code).HasMaxLength(20);
        b.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> b)
    {
        b.HasKey(s => s.Id);
        b.HasIndex(s => s.Code).IsUnique();
        b.Property(s => s.Code).HasMaxLength(20).IsRequired();
        b.Property(s => s.Name).HasMaxLength(200).IsRequired();
        b.Property(s => s.Email).HasMaxLength(200);
        b.Property(s => s.PhoneNumber).HasMaxLength(20);
        b.HasQueryFilter(s => !s.IsDeleted);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.HasKey(p => p.Id);
        b.HasIndex(p => p.SKU).IsUnique();
        b.HasIndex(p => p.Barcode).IsUnique();
        b.Property(p => p.SKU).HasMaxLength(50).IsRequired();
        b.Property(p => p.Barcode).HasMaxLength(50).IsRequired();
        b.Property(p => p.Name).HasMaxLength(200).IsRequired();
        b.Property(p => p.Unit).HasMaxLength(20).IsRequired();
        b.Property(p => p.UnitPrice).HasPrecision(18, 2);
        b.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasQueryFilter(p => !p.IsDeleted);
    }
}

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> b)
    {
        b.HasKey(i => i.Id);
        b.HasIndex(i => new { i.WarehouseId, i.ProductId }).IsUnique();
        b.HasOne(i => i.Warehouse).WithMany(w => w.Inventories).HasForeignKey(i => i.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(i => i.Product).WithMany(p => p.Inventories).HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.Ignore(i => i.AvailableQuantity);
        b.Ignore(i => i.IsLowStock);
        b.HasQueryFilter(i => !i.IsDeleted);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> b)
    {
        b.HasKey(s => s.Id);
        b.HasIndex(s => new { s.WarehouseId, s.CreatedAt });
        b.HasIndex(s => s.ProductId);
        b.Property(s => s.ReferenceNumber).HasMaxLength(50);
    }
}

public class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> b)
    {
        b.HasKey(g => g.Id);
        b.HasIndex(g => g.ReceiptNumber).IsUnique();
        b.Property(g => g.ReceiptNumber).HasMaxLength(50).IsRequired();
        b.Property(g => g.TotalAmount).HasPrecision(18, 2);
        b.HasOne(g => g.Warehouse).WithMany(w => w.GoodsReceipts).HasForeignKey(g => g.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(g => g.Supplier).WithMany(s => s.GoodsReceipts).HasForeignKey(g => g.SupplierId).OnDelete(DeleteBehavior.Restrict);
        b.HasQueryFilter(g => !g.IsDeleted);
    }
}

public class GoodsReceiptDetailConfiguration : IEntityTypeConfiguration<GoodsReceiptDetail>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptDetail> b)
    {
        b.HasKey(d => d.Id);
        b.Property(d => d.UnitPrice).HasPrecision(18, 2);
        b.HasOne(d => d.GoodsReceipt).WithMany(g => g.Details).HasForeignKey(d => d.GoodsReceiptId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(d => d.Product).WithMany(p => p.GoodsReceiptDetails).HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.Ignore(d => d.TotalPrice);
        b.HasQueryFilter(d => !d.IsDeleted);
    }
}

public class GoodsIssueConfiguration : IEntityTypeConfiguration<GoodsIssue>
{
    public void Configure(EntityTypeBuilder<GoodsIssue> b)
    {
        b.HasKey(g => g.Id);
        b.HasIndex(g => g.IssueNumber).IsUnique();
        b.Property(g => g.IssueNumber).HasMaxLength(50).IsRequired();
        b.Property(g => g.ReceiverName).HasMaxLength(100).IsRequired();
        b.HasOne(g => g.Warehouse).WithMany(w => w.GoodsIssues).HasForeignKey(g => g.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasQueryFilter(g => !g.IsDeleted);
    }
}

public class GoodsIssueDetailConfiguration : IEntityTypeConfiguration<GoodsIssueDetail>
{
    public void Configure(EntityTypeBuilder<GoodsIssueDetail> b)
    {
        b.HasKey(d => d.Id);
        b.Property(d => d.UnitPrice).HasPrecision(18, 2);
        b.HasOne(d => d.GoodsIssue).WithMany(g => g.Details).HasForeignKey(d => d.GoodsIssueId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(d => d.Product).WithMany(p => p.GoodsIssueDetails).HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.Ignore(d => d.TotalPrice);
        b.HasQueryFilter(d => !d.IsDeleted);
    }
}

public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> b)
    {
        b.HasKey(s => s.Id);
        b.HasIndex(s => s.AdjustmentNumber).IsUnique();
        b.Property(s => s.AdjustmentNumber).HasMaxLength(50).IsRequired();
        b.Property(s => s.Reason).HasMaxLength(500).IsRequired();
        b.Ignore(s => s.AdjustmentQuantity);
        b.HasQueryFilter(s => !s.IsDeleted);
    }
}

public class InventoryAuditConfiguration : IEntityTypeConfiguration<InventoryAudit>
{
    public void Configure(EntityTypeBuilder<InventoryAudit> b)
    {
        b.HasKey(a => a.Id);
        b.HasIndex(a => a.AuditNumber).IsUnique();
        b.Property(a => a.AuditNumber).HasMaxLength(50).IsRequired();
        b.HasOne(a => a.Warehouse).WithMany(w => w.InventoryAudits).HasForeignKey(a => a.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasQueryFilter(a => !a.IsDeleted);
    }
}

public class InventoryAuditDetailConfiguration : IEntityTypeConfiguration<InventoryAuditDetail>
{
    public void Configure(EntityTypeBuilder<InventoryAuditDetail> b)
    {
        b.HasKey(d => d.Id);
        b.HasOne(d => d.InventoryAudit).WithMany(a => a.Details).HasForeignKey(d => d.InventoryAuditId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.Ignore(d => d.Discrepancy);
        b.HasQueryFilter(d => !d.IsDeleted);
    }
}
public class WarehouseAssignmentConfiguration : IEntityTypeConfiguration<WarehouseAssignment>
{
    public void Configure(EntityTypeBuilder<WarehouseAssignment> b)
    {
        b.HasKey(x => x.Id);

        // Mot user - mot kho: chi duoc phan cong 1 lan (unique)
        b.HasIndex(x => new { x.UserId, x.WarehouseId }).IsUnique();

        b.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.Note).HasMaxLength(500);
        b.HasQueryFilter(x => !x.IsDeleted && x.IsActive);
    }
}

public class StaffPermissionGrantConfiguration : IEntityTypeConfiguration<StaffPermissionGrant>
{
    public void Configure(EntityTypeBuilder<StaffPermissionGrant> b)
    {
        b.HasKey(x => x.Id);

        // Unique: user + permission + warehouse (null = global)
        b.HasIndex(x => new { x.UserId, x.Permission, x.WarehouseId }).IsUnique();

        b.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        b.Property(x => x.Permission).HasConversion<int>();
        b.HasQueryFilter(x => !x.IsDeleted && x.IsActive);
    }
}
