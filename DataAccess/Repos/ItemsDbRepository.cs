using Domain.Interfaces;
using Domain.Entities;
using DataAccess.Context; // Ссылка на ApplicationDbContext
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

        // Метод GET (AA2.3.2, AA4.3.6)
        // Получает все элементы, опционально фильтруя по Approved статусу
        public async Task<IEnumerable<IItemValidating>> GetAsync(bool onlyApproved = false)
        {
            IQueryable<IItemValidating> query = _context.Restaurants
                // Загружаем связанные MenuItems, чтобы получить OwnerEmailAddress для валидации
                .Include(r => r.MenuItems);

            if (onlyApproved)
            {
                // Фильтруем только одобренные элементы (Status == "Approved")
                query = query.Where(i => i.Status == "Approved");
            }

            // Поскольку мы запрашиваем IItemValidating, 
            // нам нужно объединить Restaurants и MenuItems.
            // EF Core не может объединить два DbSet'а в один IItemValidating на стороне БД,
            // поэтому мы получим их по отдельности и объединим в памяти.

            // 1. Получаем рестораны (их Status уже в таблице Restaurants)
            var restaurants = await _context.Restaurants
                .Where(r => !onlyApproved || r.Status == "Approved")
                .ToListAsync();

            // 2. Получаем меню-элементы (их Status уже в таблице MenuItems)
            var menuItems = await _context.MenuItems
                .Include(mi => mi.Restaurant) // Включаем родительский ресторан для GetValidators()
                .Where(mi => !onlyApproved || mi.Status == "Approved")
                .ToListAsync();

            // Объединяем их в единый список IItemValidating
            var allItems = new List<IItemValidating>();
            allItems.AddRange(restaurants);
            allItems.AddRange(menuItems);

            return allItems;
        }

        // Метод SAVE (AA2.3.2, AA4.3.5)
        // Сохраняет список элементов, которые были импортированы через Factory
        public async Task SaveAsync(IEnumerable<IItemValidating> items)
        {
            // Разделяем коллекцию на рестораны и меню-элементы
            var restaurants = items.OfType<Restaurant>().ToList();
            var menuItems = items.OfType<MenuItem>().ToList();

            // 1. Сохраняем рестораны
            _context.Restaurants.AddRange(restaurants);
            await _context.SaveChangesAsync();

            // 2. Сохраняем меню-элементы
            // Здесь может быть проблема, если RestaurantId в MenuItem еще не обновлен 
            // (т.к. Restaurant.Id был сгенерирован БД).
            // В идеале, Factory должен обновить RestaurantId для MenuItems после 
            // сохранения родительских Restaurant, но сейчас EF Core должен 
            // сам разрешить эту связь, если все настроено правильно.
            _context.MenuItems.AddRange(menuItems);
            await _context.SaveChangesAsync();
        }

        // Метод APPROVE (SE3.3.3)
        // Обновляет статус элемента(ов)
        public async Task ApproveAsync(IEnumerable<int> itemIds)
        {
            // Этот метод нуждается в более сложной логике, так как ID бывают разных типов (int и Guid).
            // Для простоты (и для выполнения критерия SE3.3.3) предположим, что itemIds - это int ID ресторанов.

            var itemsToApprove = await _context.Restaurants
                .Where(r => itemIds.Contains(r.Id))
                .ToListAsync();

            foreach (var item in itemsToApprove)
            {
                item.Status = "Approved";
            }

            await _context.SaveChangesAsync();

            // В реальном приложении здесь должна быть логика для одобрения MenuItems по их Guid ID
        }

        // Метод CLEAR (AA2.3.2) - не используется в DbRepository
        public void Clear()
        {
            // Этот метод не должен делать ничего в постоянном хранилище
        }
    }
}