using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ImportRestaurantDto
{
    // Используем 'Name' вместо 'Title' для соответствия доменной модели
    public string Name { get; set; }
    public string OwnerEmailAddress { get; set; }
    // Иерархия: список позиций меню внутри ресторана
    public List<ImportMenuItemDto> MenuItems { get; set; } = new List<ImportMenuItemDto>();

    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
}
