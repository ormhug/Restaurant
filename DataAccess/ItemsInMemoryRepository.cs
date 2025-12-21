using Domain.Interfaces;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace DataAccess
{

    public class ItemsInMemoryRepository : IItemsRepository
    {
        // статическое хранилище, имитирующее кэш (IMemoryCache)
        private static readonly List<IItemValidating> _items = new List<IItemValidating>();



        // Метод get
        public Task<IEnumerable<IItemValidating>> GetAsync(bool onlyApproved = false)
        {
            IEnumerable<IItemValidating> result = _items;

            if (onlyApproved)
            {
                // Фильтрация по статусу approved
                result = _items.Where(i => i.Status == "Approved");
            }

            return Task.FromResult(result);
        }

        // Метод save
        public Task SaveAsync(IEnumerable<IItemValidating> items)
        {
            // просто добавляем или перезаписываем список
            _items.Clear();
            _items.AddRange(items);
            return Task.CompletedTask;
        }

        // Метод approve
        public Task ApproveAsync(IEnumerable<int> itemIds)
        {

            return Task.CompletedTask;
        }

        // Метод clear
        public void Clear()
        {
            _items.Clear();
        }

        public Task DeleteAsync(int id)
        {
            // поиск в памяти по id и удаление
            var itemToRemove = _items.FirstOrDefault(i =>
                i is Domain.Entities.Restaurant r && r.Id == id);

            if (itemToRemove != null)
            {
                _items.Remove(itemToRemove);
            }

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