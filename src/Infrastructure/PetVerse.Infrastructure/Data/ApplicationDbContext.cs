using Microsoft.EntityFrameworkCore;
using PetVerse.Core.Entities;
using PetVerse.Core.Interfaces;

namespace PetVerse.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserTag> UserTags { get; set; } = null!;
    public DbSet<UserTagRelation> UserTagRelations { get; set; } = null!;
    public DbSet<Pet> Pets { get; set; } = null!;
    public DbSet<PetVaccine> PetVaccines { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Like> Likes { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<Pettag> Pettags { get; set; } = null!;
    public DbSet<SystemConfig> SystemConfigs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);



        // 配置MySQL特定选项
        modelBuilder.HasCharSet("utf8mb4");

        // 配置User实体
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.Phone).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Status);
            
            entity.Property(e => e.Username).IsRequired();
            entity.Property(e => e.Status).HasDefaultValue(1);
        });

        // 配置UserTag实体
        modelBuilder.Entity<UserTag>(entity =>
        {
            entity.ToTable("user_tags");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.TagName).IsUnique();
            entity.Property(e => e.TagName).IsRequired();
        });

        // 配置UserTagRelation实体
        modelBuilder.Entity<UserTagRelation>(entity =>
        {
            entity.ToTable("user_tag_relations");
            entity.HasKey(e => new { e.UserId, e.TagId });
            
            entity.HasOne(utr => utr.User)
                .WithMany(u => u.UserTagRelations)
                .HasForeignKey(utr => utr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(utr => utr.UserTag)
                .WithMany(ut => ut.UserTagRelations)
                .HasForeignKey(utr => utr.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置Pet实体
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.ToTable("pets");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.UserId);
            
            entity.HasOne(p => p.User)
                .WithMany(u => u.Pets)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置PetVaccine实体
        modelBuilder.Entity<PetVaccine>(entity =>
        {
            entity.ToTable("pet_vaccines");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.PetId);
            
            entity.HasOne(pv => pv.Pet)
                .WithMany(p => p.PetVaccines)
                .HasForeignKey(pv => pv.PetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置Post实体
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("posts");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PetId);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.Property(e => e.Visibility).HasDefaultValue(1);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.LikesCount).HasDefaultValue(0);
            entity.Property(e => e.CommentsCount).HasDefaultValue(0);
            
            entity.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(p => p.Pet)
                .WithMany(p => p.Posts)
                .HasForeignKey(p => p.PetId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 配置Comment实体
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.ParentId);
            
            entity.Property(e => e.LikesCount).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue(1);
            
            entity.HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(c => c.Parent)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置Like实体
        modelBuilder.Entity<Like>(entity =>
        {
            entity.ToTable("likes");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => new { e.TargetType, e.TargetId, e.UserId }).IsUnique();
            entity.HasIndex(e => new { e.TargetType, e.TargetId });
            
            entity.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置Report实体
        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("reports");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => new { e.TargetType, e.TargetId });
            entity.HasIndex(e => e.Status);
            
            entity.Property(e => e.Status).HasDefaultValue(0);
            
            entity.HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(r => r.Handler)
                .WithMany()
                .HasForeignKey(r => r.HandledBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 配置Pettag实体
        modelBuilder.Entity<Pettag>(entity =>
        {
            entity.ToTable("pettags");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.SerialNumber).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.PetId });
            
            entity.Property(e => e.Status).HasDefaultValue(0);
            
            entity.HasOne(pt => pt.User)
                .WithMany()
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(pt => pt.Pet)
                .WithMany()
                .HasForeignKey(pt => pt.PetId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 配置SystemConfig实体
        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.ToTable("system_configs");
            entity.HasKey(e => e.Key);
        });
    }

    public IRepository<T> Repository<T>() where T : class
    {
        return new Repository<T>(this);
    }

    public async Task<int> SaveChangesAsync()
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        if (Database.CurrentTransaction == null)
        {
            await Database.BeginTransactionAsync();
        }
    }

    public async Task CommitTransactionAsync()
    {
        if (Database.CurrentTransaction != null)
        {
            await Database.CommitTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (Database.CurrentTransaction != null)
        {
            await Database.RollbackTransactionAsync();
        }
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}