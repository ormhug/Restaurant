using System.Linq;
using System.Threading.Tasks;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Controllers
{
    public class ItemsController : Controller
    {
        const string DbKey = "Database";

        // AA4.3.6: Метод Catalog
        [HttpGet]
        public async Task<IActionResult> Catalog(
            // Инъекция репозитория БД по ключу "Database"
            [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // 1. Получаем все элементы из БД
            var allItems = await dbRepo.GetAsync();

            // 2. Передаем коллекцию элементов в представление
            return View(allItems.ToList());
        }
    }
}
