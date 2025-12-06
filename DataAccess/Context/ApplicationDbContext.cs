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
            // Отношение "один ко многим"
            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Restaurant)
                .WithMany(r => r.MenuItems)
                .HasForeignKey(mi => mi.RestaurantId);

            // ИСПРАВЛЕНИЕ ПРЕДУПРЕЖДЕНИЯ ДЛЯ DECIMAL
            modelBuilder.Entity<MenuItem>()
                .Property(mi => mi.Price)
                // Устанавливаем точность: 18 цифр всего, 2 после запятой (например, 1234567890123456.78)
                .HasPrecision(18, 2);

            // Добавьте это, если вы используете Id: guid для MenuItem, как мы обсуждали ранее
             modelBuilder.Entity<MenuItem>()
                 .Property(mi => mi.Id)
                 .ValueGeneratedOnAdd(); 
        }
    }
}
