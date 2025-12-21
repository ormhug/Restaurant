using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces; // Добавляем ссылку на наш новый интерфейс

namespace Domain.Entities
{
    // Реализуем интерфейс IItemValidating
    public class Restaurant : IItemValidating
    {
        public int Id { get; set; }

        public string UniqueImportId { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        // 1. Поле Status (обязательно по критериям) [cite: 47]
        // Устанавливаем статус 'pending' по умолчанию (в Factory) [cite: 66]
        public string Status { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerEmailAddress { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        // Реализация GetValidators(): Ресторан одобряет Site Admin [cite: 53]
        public List<string> GetValidators()
        {
            // Email Site Admin должен быть загружен из настроек (appsettings.json)
            // Пока используем заглушку, которую позже заменим конфигурацией.
            return new List<string> { "admin@test.com" };
        }

        // Реализация GetCardPartial(): Возвращает имя Partial View [cite: 54]
        public string GetCardPartial()
        {
            // Имя представления для карточки Ресторана
            return "_RestaurantCardPartial";
        }

        //public List<string> Validate()
        //{
        //    throw new NotImplementedException();
        //}

        public List<string> Validate()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add("Restaurant Name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(OwnerEmailAddress))
            {
                errors.Add("Owner Email Address cannot be empty.");
            }

            return errors;
        }

        public string GetId()
        {
            return Id.ToString();
        }

    }
}