// Domain/ImportItemFactory.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;

namespace Domain
{
    public static class ImportItemFactory
    {
        // Внутренний вспомогательный DTO для парсинга типа
        private class ItemBaseDto { public string? Type { get; set; } }

        public static async Task<IEnumerable<IItemValidating>> CreateAsync(Stream jsonStream)
        {
            if (jsonStream == null || jsonStream.Length == 0)
            {
                return new List<IItemValidating>();
            }

            // Читаем весь JSON как массив объектов-заглушек для определения типа
            var rawElements = await JsonSerializer.DeserializeAsync<List<JsonElement>>(
                jsonStream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (rawElements == null) return new List<IItemValidating>();

            var itemsToSave = new List<IItemValidating>();
             const string DefaultStatus = "Pending"; 

            // Словарь для временного хранения DTO-ресторанов по их ID (например, "R-1001")
            var restaurantDtos = new Dictionary<string, ImportRestaurantDto>();

            // 1. Десериализация
            foreach (var element in rawElements)
            {
                if (element.TryGetProperty("type", out var typeElement) && typeElement.GetString() != null)
                {
                    string itemType = typeElement.GetString()!.ToLowerInvariant();
                    string jsonString = element.GetRawText();

                    if (itemType == "restaurant")
                    {
                        var dto = JsonSerializer.Deserialize<ImportRestaurantDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (dto?.Id != null)
                        {
                            restaurantDtos.Add(dto.Id, dto);
                        }
                    }
                    else if (itemType == "menuitem")
                    {
                        var dto = JsonSerializer.Deserialize<ImportMenuItemDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (dto != null)
                        {
                            // Добавляем menuItem к списку меню ресторана (будет использоваться на следующем шаге)
                            if (dto.RestaurantId != null)
                            {
                                if (!restaurantDtos.ContainsKey(dto.RestaurantId))
                                {
                                    // Если ресторан еще не обработан, создаем заглушку, чтобы меню-элементы не потерялись
                                    restaurantDtos.Add(dto.RestaurantId, new ImportRestaurantDto { Id = dto.RestaurantId });
                                }
                                // Добавляем меню-элемент к родительскому DTO
                                restaurantDtos[dto.RestaurantId].MenuItems.Add(dto);
                            }
                        }
                    }
                }
            }

            // 2. Конвертация DTO в доменные сущности
            foreach (var dto in restaurantDtos.Values)
            {
                // Создаем сущность Restaurant
                var restaurant = new Restaurant
                {
                    // Предполагаем, что Name, OwnerEmailAddress и другие поля не null в JSON
                    Name = dto.Name!,
                    Description = dto.Description ?? string.Empty,
                    OwnerEmailAddress = dto.OwnerEmailAddress!,
                    Address = dto.Address ?? string.Empty,
                    Phone = dto.Phone ?? string.Empty,
                    Status = DefaultStatus
                };
                itemsToSave.Add(restaurant);

                // Создаем сущности MenuItem
                foreach (var menuItemDto in dto.MenuItems)
                {
                    var menuItem = new MenuItem
                    {
                        // В Приложении A используется "title"[cite: 133], а не "name"
                        Title = menuItemDto.Title!,
                        Price = menuItemDto.Price,
                        Currency = menuItemDto.Currency ?? "EUR",
                        Status = DefaultStatus,
                        Restaurant = restaurant
                    };
                    restaurant.MenuItems.Add(menuItem);
                    itemsToSave.Add(menuItem);
                }
            }

            return itemsToSave;
        }
    }

    // --- Вспомогательные классы DTO для парсинга ---

    public class ImportRestaurantDto
    {
        public string? Type { get; set; }
        public string Id { get; set; } = string.Empty; // "R-1001"
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string OwnerEmailAddress { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        // Добавляем список для сбора связанных меню-элементов
        public List<ImportMenuItemDto> MenuItems { get; set; } = new List<ImportMenuItemDto>();
    }

    public class ImportMenuItemDto
    {
        public string? Type { get; set; }
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty; // Используем Title, как в Приложении A [cite: 133]
        public decimal Price { get; set; }
        public string? Currency { get; set; }
        public string? RestaurantId { get; set; } // "R-1001"
    }
}