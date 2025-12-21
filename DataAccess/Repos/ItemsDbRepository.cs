using Domain.Interfaces;
using Domain.Entities;
using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using System;
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

        // --- ИСПРАВЛЕННЫЙ GET (Только рестораны) ---
        public async Task<IEnumerable<IItemValidating>> GetAsync(bool onlyApproved = false)
        {
            // Начинаем запрос к ресторанам
            // Include(r => r.MenuItems) подгружает меню, чтобы оно было доступно внутри
            var query = _context.Restaurants
                                .Include(r => r.MenuItems)
                                .AsQueryable();

            // Если просят только одобренные (для Гостей) - фильтруем
            if (onlyApproved)
            {
                query = query.Where(r => r.Status == "Approved");
            }

            // Выполняем запрос
            var restaurants = await query.ToListAsync();

            // Возвращаем как список IItemValidating
            // (Пункты меню не добавляем отдельно, они лежат внутри ресторанов)
            return restaurants.Cast<IItemValidating>();
        }

        public async Task SaveAsync(IEnumerable<IItemValidating> items)
        {
            // Берем только Рестораны (родительские объекты).
            // EF Core сам сохранит вложенные MenuItems, так как они находятся внутри свойства Restaurant.MenuItems
            var restaurants = items.OfType<Restaurant>().ToList();

            await _context.Restaurants.AddRangeAsync(restaurants);
            await _context.SaveChangesAsync();
        }

        // --- МЕТОДЫ ОДОБРЕНИЯ ---

        public async Task ApproveAsync(int id)
        {
            var item = await _context.Restaurants.FindAsync(id);
            if (item != null)
            {
                item.Status = "Approved";
                await _context.SaveChangesAsync();
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

        // --- ПОЛУЧЕНИЕ ПО ID (Для страницы Details) ---
        public async Task<Restaurant> GetRestaurantByIdAsync(int id)
        {
            // Важно: Include гарантирует, что мы получим ресторан + его меню (даже если оно пустое)
            return await _context.Restaurants
                                 .Include(r => r.MenuItems)
                                 .FirstOrDefaultAsync(r => r.Id == id);
        }

        // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

        public async Task DeleteAsync(int id)
        {
            var item = await _context.Restaurants.FindAsync(id);
            if (item != null)
            {
                _context.Restaurants.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public void Clear()
        {
            // Для БД не требуется, метод нужен только для In-Memory репозитория
        }

        // Метод ApproveAsync для списка (если используется где-то еще)
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
    }
}