using Domain.Interfaces;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory; // Нам понадобится IMemoryCache

namespace DataAccess
{
    // В реальном приложении этот класс будет использовать IMemoryCache, 
    // но для простоты мы начнем со статической коллекции, чтобы устранить ошибки.
    public class ItemsInMemoryRepository : IItemsRepository
    {
        // Используем статическое хранилище, имитирующее кэш (IMemoryCache)
        private static readonly List<IItemValidating> _items = new List<IItemValidating>();

        // Конструктор, в который обычно инжектируется IMemoryCache
        // public ItemsInMemoryRepository(IMemoryCache cache) { ... }

        // Метод GET (AA2.3.2)
        public Task<IEnumerable<IItemValidating>> GetAsync(bool onlyApproved = false)
        {
            IEnumerable<IItemValidating> result = _items;

            if (onlyApproved)
            {
                // Фильтрация по статусу "Approved"
                result = _items.Where(i => i.Status == "Approved");
            }

            return Task.FromResult(result);
        }

        // Метод SAVE (AA2.3.2)
        public Task SaveAsync(IEnumerable<IItemValidating> items)
        {
            // Здесь мы имитируем "запись в кэш" - просто добавляем или перезаписываем список
            _items.Clear();
            _items.AddRange(items);
            return Task.CompletedTask;
        }

        // Метод APPROVE (SE3.3.3)
        public Task ApproveAsync(IEnumerable<int> itemIds)
        {
            // Здесь должна быть логика поиска и обновления статуса
            // (Сложно реализовать, пока все ID не станут int/Guid)
            return Task.CompletedTask;
        }

        // Метод CLEAR (AA2.3.2)
        public void Clear()
        {
            _items.Clear();
        }
    }
}