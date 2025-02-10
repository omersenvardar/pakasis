using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBGoreWebApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DBGoreWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Veritabanı tablolarına controllerdan vs. erişim sağlamak için propertyler
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<AdminSettings> AdminSettings { get; set; } // admin sayfa ayarları için

        public DbSet<Arsa> Arsa { get; set; } // Arsa sınıfını ekledik
        public DbSet<ArsaResim>? ArsaResimleri { get; set; }
        public DbSet<Adres> Adresler { get; set; }
        public DbSet<Araba> Arabalar { get; set; }
        public DbSet<ArabaResim> ArabaResimleri { get; set; }
        public DbSet<Duyuru> Duyurular { get; set; }
        public DbSet<EmlakBahce> EmlakBahceler { get; set; }
        public DbSet<IlanArsa> IlanArsalar { get; set; }
        public DbSet<AracMarkalari> AracMarkalaris { get; set; }
        public DbSet<AracModelListesi> AracModelListesis { get; set; }
        public DbSet<AracModelYillari> AracModelYillaris { get; set; }
        public DbSet<AracYilVersiyon> AracYilVersiyons { get; set; }

        public DbSet<Kullanici> Kullanicilar { get; set; } // Kullanici tablosunu ekledik
        public DbSet<Kategori> Kategoriler { get; set; } // Kategori tablosunu ekledik

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            // E-posta alanına benzersiz (unique) index ekleme
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ArsaResim>()
                .HasOne(r => r.EmlakBahce)
                .WithMany(e => e.ArsaResimleri)
                .HasForeignKey(r => r.ArsaId);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ArabaResim>()
                .HasOne(r => r.Araba)
                .WithMany(a => a.ArabaResimleri)
                .HasForeignKey(r => r.ArabaId);

            modelBuilder.Entity<Araba>()
        .HasOne(a => a.AdresKonumuNavigation)
        .WithMany()
        .HasForeignKey(a => a.AdresKonumu);

            modelBuilder.Entity<EmlakBahce>(entity =>
    {
        entity.ToTable("emlak_bahce"); // Tablo adını burada belirtiyoruz
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
    });
            // User - Role ilişkisi (Çoka çok ilişki)
            modelBuilder.Entity<UserRole>()
                    .HasKey(ur => new
                    {
                        ur.UserId,
                        ur.RoleId
                    });
            modelBuilder.Entity<Kullanici>()
                        .HasIndex(u => u.Email)
                        .IsUnique(); // Benzersiz olmasını sağla


            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                        .HasOne(ur => ur.Role)
                        .WithMany(r => r.UserRoles)
                        .HasForeignKey(ur => ur.RoleId);

            // Kullanici enum değerlerini string olarak saklama
            modelBuilder.Entity<Kullanici>()
                        .Property(k => k.Rol)
                        .HasConversion<string>();

            modelBuilder.Entity<Kullanici>()
                .Property(k => k.AbonelikTipi)
                .HasConversion<string>();

            modelBuilder.Entity<Kullanici>()
                .Property(k => k.AbonelikStatusu)
                .HasConversion<string>();

            // Opsiyonel: Varsayılan veri ekleme (Seed Data)
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, Name = "Admin" },
                new Role { RoleId = 2, Name = "User" },
                new Role { RoleId = 3, Name = "Uye" },
                new Role { RoleId = 4, Name = "Bayi" }
            );
        }
    }
}
