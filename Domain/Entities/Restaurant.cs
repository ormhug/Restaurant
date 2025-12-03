using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Restaurant
    {

        public string Id { get; set; } = string.Empty;

        // Основные свойства
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerEmailAddress { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Поле "type" можно использовать для внутреннего маппинга,
        // но, как правило, оно не нужно в таблице, если только не хранятся разные типы.
        // Оставим только бизнес-поля.

        // Навигационное свойство для связи "один ко многим"
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    }
}
