using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Interfaces; // Добавляем ссылку на наш новый интерфейс

namespace Domain.Entities
{
    // Реализуем интерфейс IItemValidating
    public class MenuItem : IItemValidating
    {
        // Используем Guid, как указано в критериях [cite: 50]
        public Guid Id { get; set; }

        // 1. Поле Status (обязательно по критериям) [cite: 50]
        // Устанавливаем статус 'pending' по умолчанию (в Factory) [cite: 66]
        public string Status { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "EUR";

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        // Реализация GetValidators(): MenuItem одобряет Владелец Ресторана [cite: 54]
        public List<string> GetValidators()
        {
            // Возвращаем email владельца связанного ресторана
            if (Restaurant != null)
            {
                return new List<string> { Restaurant.OwnerEmailAddress };
            }
            return new List<string>();
        }

        // Реализация GetCardPartial(): Возвращает имя Partial View [cite: 54]
        public string GetCardPartial()
        {
            // Имя представления для элемента Меню
            return "_MenuItemRowPartial";
        }
    }
}