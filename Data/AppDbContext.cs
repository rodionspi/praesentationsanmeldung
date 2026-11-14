using Microsoft.EntityFrameworkCore;

// Das brauche ich um C#-Code mit Datebank zu verbinden ohne dass
// man die manuelle SQL-Befehle schreiben muss.
// Allgemeimein Entity Framework Core (EF Core) ist ein objektrelationaler Mapper (ORM),
// der das Speichern und Lesen von Daten in einer Datenbank aus .NET-Anwendungen heraus vereinfacht.
using praesentationsanmeldung.Models;

namespace praesentationsanmeldung.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<G3Sus> G3Sus => Set<G3Sus>();
    public DbSet<Raum> Raeume => Set<Raum>();
    public DbSet<Praesentation> Praesentationen => Set<Praesentation>();
    public DbSet<Eintragung> Eintragungen => Set<Eintragung>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.Property(a => a.Benutzername).IsRequired().HasMaxLength(100);
            entity.Property(a => a.PasswortHash).IsRequired();
            entity.HasIndex(a => a.Benutzername).IsUnique();
        });

        modelBuilder.Entity<G3Sus>(entity =>
        {
            entity.Property(s => s.Vorname).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Nachname).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Klasse).IsRequired().HasMaxLength(20);
            entity.Property(s => s.Email).IsRequired().HasMaxLength(200);
            entity.Property(s => s.PasswortHash).IsRequired();
            entity.HasIndex(s => s.Email).IsUnique();
        });

        modelBuilder.Entity<Raum>(entity =>
        {
            entity.Property(r => r.Bezeichnung).IsRequired().HasMaxLength(50);
            entity.HasIndex(r => r.Bezeichnung).IsUnique();
            entity.ToTable(t => t.HasCheckConstraint("CK_Raeume_Kapazitaet", "[Kapazitaet] > 0"));
        });

        modelBuilder.Entity<Praesentation>(entity =>
        {
            entity.Property(p => p.Titel).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Beschreibung).HasMaxLength(2000);

            entity.HasOne(p => p.Raum)
                .WithMany(r => r.Praesentationen)
                .HasForeignKey(p => p.RaumId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Eintragung>(entity =>
        {
            entity.HasOne(e => e.G3Sus)
                .WithMany(s => s.Eintragungen)
                .HasForeignKey(e => e.G3SusId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Praesentation)
                .WithMany(p => p.Eintragungen)
                .HasForeignKey(e => e.PraesentationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.G3SusId, e.PraesentationId }).IsUnique();
        });
    }
}
