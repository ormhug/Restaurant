using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ImportMenuItemDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    // Нам не нужен RestaurantId здесь, так как иерархия обрабатывается Фабрикой
}