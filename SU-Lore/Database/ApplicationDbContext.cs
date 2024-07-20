﻿using Microsoft.EntityFrameworkCore;

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
        });
        
        modelBuilder.Entity<Models.File>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id);
        });
        
        modelBuilder.Entity<Models.Accounts.Account>().ToTable("Accounts");
        modelBuilder.Entity<Models.Pages.Page>().ToTable("Pages");
        modelBuilder.Entity<Models.File>().ToTable("Files");
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
}