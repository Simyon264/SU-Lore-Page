using Microsoft.EntityFrameworkCore;
using SU_Lore.Database.Models;
using SU_Lore.Database.Models.Accounts;
using SU_Lore.Database.Models.Pages;

namespace SU_Lore.Database;

/// <summary>
/// The database context for the application. Pretty self-explanatory name.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.Accounts.Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
        });

        modelBuilder.Entity<Models.Pages.Page>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id);

            entity.HasMany(p => p.Flags)
                .WithOne(f => f.Page)
                .HasForeignKey(f => f.PageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Models.File>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id);
        });

        modelBuilder.Entity<PageStat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id);
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Hex).IsUnique();
        });

        modelBuilder.Entity<FileChunk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id);
            entity.HasIndex(e => e.FileId);
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.AccountId);
            entity.HasOne(e => e.Account)
                .WithMany(a => a.Profiles)
                .HasForeignKey(e => e.AccountId);
        });

        modelBuilder.Entity<PageComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id);
            entity.Property(e => e.Content)
                .IsRequired();

            entity.Property(e => e.PageId);

            entity.Property(e => e.AccountId)
                .IsRequired();
            entity.Property(e => e.ProfileName)
                .IsRequired();
        });

        modelBuilder.Entity<Models.Accounts.Account>().ToTable("Accounts");
        modelBuilder.Entity<Models.Pages.Page>().ToTable("Pages");
        modelBuilder.Entity<Models.File>().ToTable("Files");
        modelBuilder.Entity<PageStat>().ToTable("PageStats");
        modelBuilder.Entity<Color>().ToTable("Colors");
        modelBuilder.Entity<FileChunk>().ToTable("FileChunks");
    }

    /// <summary>
    /// Represents the accounts in the database.
    /// </summary>
    public DbSet<Models.Accounts.Account> Accounts { get; set; }

    /// <summary>
    /// The pages in the database.
    /// </summary>
    public DbSet<Models.Pages.Page> Pages { get; set; }

    /// <summary>
    /// The uploaded files in the database.
    /// </summary>
    public DbSet<Models.File> Files { get; set; }

    public DbSet<PageStat> PageStats { get; set; }

    public DbSet<Color> Colors { get; set; }

    public DbSet<FileChunk> FileChunks { get; set; }

    /// <summary>
    /// Profiles in the database.
    /// </summary>
    public DbSet<Profile> Profiles { get; set; }

    public DbSet<PageComment> Comments { get; set; }
}