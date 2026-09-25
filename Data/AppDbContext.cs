using Microsoft.EntityFrameworkCore;
// Das brauche ich um C#-Code mit Datebank zu verbinden ohne dass
// man die manuelle SQL-Befehle schreiben muss.
using praesentationsanmeldung.Models;

namespace praesentationsanmeldung.Data;

public class AppDbContext : DbContext
{
    // wegen dem DbContext bekommt eine Klasse die Methoden dazu, wie SaveChanges(), Add() und Remove().
    public AppDbContext(DbContextOptions<AppDbContext> options) :base(options)
    {
        // in options wird übergeben welche Datenbank verwendet wird also SQL Server wird bei uns
        //benutzt und es wird auch dort übergibt wo die datenbank liegt

        // :base(options) schickt einfach options zum Konstruktor von DbContext weiter,
        // da DbContext diesen SQL-Server-Pfad unbedingt kennen müss
    }

    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<G3Sus> G3Sus => Set<G3Sus>();
    public DbSet<Raum> Raeume => Set<Raum>();
    public DbSet<Praesentation> Praesentationen => Set<Praesentation>();
    public DbSet<Eintragung> Eintragungen => Set<Eintragung>();
    // DBSet<Admin> entspricht einer "Tabelle" mit Objekten vom Typ Admin(ist alles in Admin.cs)
    // Admins ist die Name der Eigenschfat und standartmässig auch der Tabellenname in SQL
    // Das => Set<Admin>() ist eine Anweisung, damit DbContext eine Verbindung zur Tabelle von Admins herstellt

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // mit override sagen wir, dass die geerbte Methode überschrieben werden soll.
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
        });

        modelBuilder.Entity<Raum>(entity =>
        {
            entity.Property(r => r.Bezeichnung).IsRequired().HasMaxLength(50);
            entity.HasIndex(r => r.Bezeichnung).IsUnique();
        });

        modelBuilder.Entity<Praesentation>(entity =>
        {
            entity.Property(p => p.Titel).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Beschreibung).HasMaxLength(2000);

            // Ein Raum mit geplanten Präsentationen darf nicht gelöscht werden.
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
