using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Interfaces; // Добавляем ссылку на наш новый интерфейс

namespace Domain.Entities
{
    // Реализуем интерфейс IItemValidating
    public class MenuItem : IItemValidating
    {
        


        public string ImagePath { get; set; } = string.Empty;


        // Используем Guid, как указано в критериях [cite: 50]

        public Guid Id { get; set; }

        public string UniqueImportId { get; set; } = string.Empty;

        // 1. Поле Status (обязательно по критериям) [cite: 50]
        // Устанавливаем статус 'pending' по умолчанию (в Factory) [cite: 66]
        public string Status { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "EUR";

        public int RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; } 

        // Реализация GetValidators(): MenuItem одобряет Владелец Ресторана 
        public List<string> GetValidators()
        {
            // Возвращаем email владельца связанного ресторана
            if (Restaurant != null)
            {
                return new List<string> { Restaurant.OwnerEmailAddress };
            }
            return new List<string>();
        }

        // Реализация GetCardPartial(): Возвращает имя Partial View 
        public string GetCardPartial()
        {
            // Имя представления для элемента Меню
            return "_MenuItemCardPartial";
        }

        // Add the missing UniqueImportId property implementation to satisfy IItemValidating


        public List<string> Validate()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Title))
            {
                errors.Add("Menu Item Title cannot be empty.");
            }
            if (Price <= 0)
            {
                errors.Add("Price must be greater than zero.");
            }

            return errors;
        }
    }
}