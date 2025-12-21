using Domain.Interfaces;
using Domain.Entities;
using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ItemsDbRepository : IItemsRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemsDbRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Метод GET (Оставляем без изменений, он работает корректно для чтения)
        public async Task<IEnumerable<IItemValidating>> GetAsync(bool onlyApproved = false)
        {
            // 1. Получаем рестораны
            var restaurants = await _context.Restaurants
                .Include(r => r.MenuItems)
                .Where(r => !onlyApproved || r.Status == "Approved")
                .ToListAsync();

            // 2. Получаем меню-элементы
            var menuItems = await _context.MenuItems
                .Include(mi => mi.Restaurant)
                .Where(mi => !onlyApproved || mi.Status == "Approved")
                .ToListAsync();

            // Объединяем их
            var allItems = new List<IItemValidating>();
            allItems.AddRange(restaurants);
            allItems.AddRange(menuItems);

            return allItems;
        }

        // --- ИСПРАВЛЕННЫЙ МЕТОД SAVE ---
        public async Task SaveAsync(IEnumerable<IItemValidating> items)
        {
            // ИСПРАВЛЕНИЕ:
            // Мы отбираем только родительские объекты (Рестораны).
            // Пункты меню (MenuItems) находятся внутри свойства restaurant.MenuItems.
            var parentsOnly = items.OfType<Restaurant>().ToList();

            // Мы добавляем в контекст только родителей.
            // EF Core "умный": он увидит вложенные MenuItems и автоматически добавит их в БД,
            // сгенерировав правильные ID и связи.
            await _context.Restaurants.AddRangeAsync(parentsOnly);

            // Сохраняем все одной транзакцией.
            await _context.SaveChangesAsync();
        }

        // Метод APPROVE (Оставляем без изменений)
        public async Task ApproveAsync(IEnumerable<int> itemIds)
        {
            var itemsToApprove = await _context.Restaurants
                .Where(r => itemIds.Contains(r.Id))
                .ToListAsync();

            foreach (var item in itemsToApprove)
            {
                item.Status = "Approved";
            }

            await _context.SaveChangesAsync();
        }

        public void Clear()
        {
            // Не используется для БД
        }

        public async Task DeleteAsync(int id)
        {
            // 1. Ищем ресторан по ID
            var item = await _context.Restaurants.FindAsync(id);

            // 2. Если нашли — удаляем
            if (item != null)
            {
                _context.Restaurants.Remove(item);

                // Сохраняем изменения. 
                // Примечание: Если в БД настроено каскадное удаление (Cascade Delete), 
                // то меню удалится автоматически.
                await _context.SaveChangesAsync();
            }
        }

        public async Task ApproveAsync(int id)
        {
            var item = await _context.Restaurants.FindAsync(id);
            if (item != null)
            {
                item.Status = "Approved"; // Меняем статус
                await _context.SaveChangesAsync(); // Сохраняем в БД
            }
        }

        public async Task ApproveMenuAsync(Guid id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item != null)
            {
                item.Status = "Approved";
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Restaurant> GetRestaurantByIdAsync(int id)
        {
            // Include делает "Left Join" - ресторан найдется, даже если MenuItems пустое
            return await _context.Restaurants
                                 .Include(r => r.MenuItems)
                                 .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}