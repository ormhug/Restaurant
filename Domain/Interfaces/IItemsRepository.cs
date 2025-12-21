using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Entities;

using System.Collections.Generic;
using Domain.Interfaces; // Нужен для IItemValidating

namespace Domain.Interfaces
{
    public interface IItemsRepository
    {
        // GET: Получает все элементы, реализующие IItemValidating
        Task<IEnumerable<IItemValidating>> GetAsync(bool onlyApproved = false);

        // SAVE: Сохраняет список элементов
        Task SaveAsync(IEnumerable<IItemValidating> items);

        // APPROVE: Обновляет статус элементов
        Task ApproveAsync(IEnumerable<int> itemIds);

        // CLEAR: Очищает хранилище (используется для InMemoryCache)
        void Clear();

        Task DeleteAsync(int id);

        Task ApproveAsync(int id);
        Task ApproveMenuAsync(Guid id);
        Task<Restaurant> GetRestaurantByIdAsync(int id);
    }
}