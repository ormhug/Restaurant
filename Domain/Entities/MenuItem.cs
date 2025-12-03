using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class MenuItem
    {

        public string Id { get; set; } = string.Empty;

        // Поля, соответствующие вашему примеру
        public string Title { get; set; } = string.Empty; // Title вместо Name
        public decimal Price { get; set; }
        public string Currency { get; set; } = "EUR"; // Добавляем валюту

        // Внешний ключ (Foreign Key) - строковый
        public string RestaurantId { get; set; } = string.Empty;

        // Навигационное свойство
        public Restaurant Restaurant { get; set; }

    }
}
