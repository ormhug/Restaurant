using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace DataAccess.Context
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
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

            base.OnModelCreating(modelBuilder);

            // Отношение "один ко многим"
            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Restaurant)
                .WithMany(r => r.MenuItems)
                .HasForeignKey(mi => mi.RestaurantId);

            
            modelBuilder.Entity<MenuItem>()
                .Property(mi => mi.Price)
                .HasPrecision(18, 2);

            //  нужно для Id: guid для MenuItem
             modelBuilder.Entity<MenuItem>()
                 .Property(mi => mi.Id)
                 .ValueGeneratedOnAdd(); 
        }
    }
}
