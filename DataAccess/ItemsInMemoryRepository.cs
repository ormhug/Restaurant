using Domain.Interfaces;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

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

        public Task DeleteAsync(int id)
        {
            // Ищем элемент в списке памяти
            // (Нам нужно проверить, является ли элемент рестораном и совпадает ли ID)
            var itemToRemove = _items.FirstOrDefault(i =>
                i is Domain.Entities.Restaurant r && r.Id == id);

            if (itemToRemove != null)
            {
                _items.Remove(itemToRemove);
            }

            // Возвращаем завершенную задачу (т.к. метод асинхронный, но работаем мы в памяти)
            return Task.CompletedTask;
        }

        public Task ApproveAsync(int id)
        {
            // Находим в памяти и меняем статус
            var item = _items.OfType<Domain.Entities.Restaurant>().FirstOrDefault(r => r.Id == id);
            if (item != null)
            {
                item.Status = "Approved";
            }
            return Task.CompletedTask;
        }

        public Task ApproveMenuAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Restaurant> GetRestaurantByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

     
    }
}