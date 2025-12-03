using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace DataAccess.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            // Конструктор необходим для конфигурации в Program.cs
        }

        // DbSets - это таблицы в вашей базе данных
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Здесь мы можем явно определить внешние ключи и другие связи.
            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Restaurant) // У MenuItem есть один Restaurant
                .WithMany(r => r.MenuItems) // У Restaurant много MenuItems
                .HasForeignKey(mi => mi.RestaurantId); // Внешний ключ
        }
    }
}
