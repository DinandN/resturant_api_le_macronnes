using LeMacronnesResturauntAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LeMacronnesResturauntAPI.Data
{
    public class RestaurantContext : DbContext
    {
        public RestaurantContext(DbContextOptions<RestaurantContext> options) : base(options) { }

        public DbSet<Tafel> Tafels { get; set; }
        public DbSet<Reservering> Reserveringen { get; set; }
        public DbSet<Rekening> Rekeningen { get; set; }
        public DbSet<Bestelling> Bestellingen { get; set; }
        public DbSet<BestelRegel> BestelRegels { get; set; }
        public DbSet<Gerecht> Gerechten { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1-op-1 relatie
            modelBuilder.Entity<Reservering>()
                .HasOne(r => r.Rekening)
                .WithOne(rek => rek.Reservering)
                .HasForeignKey<Rekening>(rek => rek.ReserveringID);

            // Seed Data 
            modelBuilder.Entity<Tafel>().HasData(
                new Tafel { TafelID = 1, Tafelnummer = 1, AantalPlaatsen = 4 },
                new Tafel { TafelID = 2, Tafelnummer = 2, AantalPlaatsen = 2 }
            );

            modelBuilder.Entity<Gerecht>().HasData(
                new Gerecht { GerechtID = 1, Naam = "Spaghetti Carbonara", Omschrijving = "Pasta met spek en ei", Prijs = 12.50m, Allergenen = "Gluten, Ei, Lactose" },
                new Gerecht { GerechtID = 2, Naam = "Biefstuk", Omschrijving = "Met pepersaus", Prijs = 24.00m, Allergenen = "Lactose" }
            );
        }
    }
}