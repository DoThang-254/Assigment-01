using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DataAccess.Models;

public partial class FunewsManagementContext : DbContext
{
    public string CurrentUserId { get; set; } = "System";
    public FunewsManagementContext()
    {
    }

    public FunewsManagementContext(DbContextOptions<FunewsManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<NewsArticle> NewsArticles { get; set; }

    public virtual DbSet<SystemAccount> SystemAccounts { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=thang\\SQLEXPRESS13;uid=sa;password=123;database=FUNewsManagement;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryDescription).HasMaxLength(250);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.ParentCategoryId).HasColumnName("ParentCategoryID");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_Category_Category");
        });

        modelBuilder.Entity<NewsArticle>(entity =>
        {
            entity.ToTable("NewsArticle");

            entity.Property(e => e.NewsArticleId)
                .HasMaxLength(20)
                .HasColumnName("NewsArticleID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedById).HasColumnName("CreatedByID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Headline).HasMaxLength(150);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.NewsContent).HasMaxLength(4000);
            entity.Property(e => e.NewsSource).HasMaxLength(400);
            entity.Property(e => e.NewsTitle).HasMaxLength(400);
            entity.Property(e => e.UpdatedById).HasColumnName("UpdatedByID");
            entity.Property(e => e.NewsImage).HasMaxLength(400).IsRequired(false);
            entity.Property(e => e.Views).HasDefaultValue(0);

            entity.HasOne(d => d.Category).WithMany(p => p.NewsArticles)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_NewsArticle_Category");

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.NewsArticles)
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_NewsArticle_SystemAccount");

            entity.HasMany(d => d.Tags).WithMany(p => p.NewsArticles)
                .UsingEntity<Dictionary<string, object>>(
                    "NewsTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NewsTag_Tag"),
                    l => l.HasOne<NewsArticle>().WithMany()
                        .HasForeignKey("NewsArticleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NewsTag_NewsArticle"),
                    j =>
                    {
                        j.HasKey("NewsArticleId", "TagId");
                        j.ToTable("NewsTag");
                        j.IndexerProperty<string>("NewsArticleId")
                            .HasMaxLength(20)
                            .HasColumnName("NewsArticleID");
                        j.IndexerProperty<int>("TagId").HasColumnName("TagID");
                    });
        });

        modelBuilder.Entity<SystemAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId);

            entity.ToTable("SystemAccount");

            entity.Property(e => e.AccountId)
                .ValueGeneratedNever()
                .HasColumnName("AccountID");
            entity.Property(e => e.AccountEmail).HasMaxLength(70);
            entity.Property(e => e.AccountName).HasMaxLength(100);
            entity.Property(e => e.AccountPassword).HasMaxLength(70);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK_HashTag");

            entity.ToTable("Tag");

            entity.Property(e => e.TagId)
                .ValueGeneratedNever()
                .HasColumnName("TagID");
            entity.Property(e => e.Note).HasMaxLength(400);
            entity.Property(e => e.TagName).HasMaxLength(50);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id); // Hoặc TokenId tùy theo class Model của bạn
            entity.ToTable("RefreshToken"); // Đảm bảo tên này khớp với tên bạn muốn trong DB

            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.Property(e => e.JwtId).HasMaxLength(100);

            // Cấu hình mối quan hệ với SystemAccount
            entity.HasOne(d => d.SystemAccount)
                  .WithMany() // Nếu trong SystemAccount không có List<RefreshToken>
                  .HasForeignKey(d => d.AccountId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_RefreshToken_SystemAccount");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLog");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(100);
            entity.Property(e => e.Action).HasMaxLength(20);
            entity.Property(e => e.TableName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private void OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges(); // Đảm bảo bắt hết thay đổi

        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
            (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

        var auditEntries = new List<AuditLog>();

        foreach (var entry in entries)
        {
            var auditLog = new AuditLog
            {
                TableName = entry.Metadata.GetTableName() ?? entry.Metadata.Name,
                UserId = CurrentUserId,
                Timestamp = DateTime.UtcNow,
                Action = entry.State.ToString()
            };

            var oldValues = new Dictionary<string, object>();
            var newValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue;
                string propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            auditLog.OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues);
            auditLog.NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues);
            auditEntries.Add(auditLog);
        }

        // Thêm danh sách log vào Context (để chuẩn bị Save chung 1 transaction)
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
        }
    }

    // 2. Override SaveChangesAsync (Dành cho code mới dùng await)
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaveChanges(); // Gọi hàm logic ở trên
        return await base.SaveChangesAsync(cancellationToken);
    }

    // 3. Override SaveChanges (Dành cho code cũ dùng _context.SaveChanges())
    public override int SaveChanges()
    {
        OnBeforeSaveChanges(); // Gọi hàm logic ở trên
        return base.SaveChanges();
    }
}
