using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Filters;
using System.Linq;
using System.Threading.Tasks;


namespace Restaurant.Controllers
{
    public class ItemsController : Controller
    {
        const string DbKey = "Database";

        [HttpGet]
        public async Task<IActionResult> Catalog([FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // получаем все элементы
            var items = await dbRepo.GetAsync(onlyApproved: true);

            // это фильтр. Оставляет только рестораны
            var restaurantsOnly = items
                .OfType<Domain.Entities.Restaurant>()       
                .Cast<Domain.Interfaces.IItemValidating>()  
                .ToList();

            //передаем список
            return View(restaurantsOnly);
        }

        [HttpPost] // удаление должно быть post запросом (безопасность)
        public async Task<IActionResult> Delete(int id, [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // вызывает удаление
            await dbRepo.DeleteAsync(id);

            // перезагружает каталог
            return RedirectToAction("Catalog");
        }

        [HttpGet]
        [Authorize] // только для вошедших пользователей
        public async Task<IActionResult> Verification(
            [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            var userEmail = User.Identity?.Name;

            //  Хард код Админа ЭТО АДМИНКА
            const string AdminEmail = "admin@test.com";

            
            var allItems = await dbRepo.GetAsync(onlyApproved: false);

            if (userEmail == AdminEmail)
            {
                // админ видит все рестораны
                var pendingRestaurants = allItems
                    .OfType<Domain.Entities.Restaurant>()
                    .Where(r => r.Status == "Pending") 
                    .ToList();

                return View("VerificationAdmin", pendingRestaurants);
            }
            else
            {
                //для владельцев ресторанов
                var myRestaurants = allItems
                    .OfType<Domain.Entities.Restaurant>()
                    .Where(r => r.OwnerEmailAddress == userEmail)
                    .ToList();

                return View("VerificationOwner", myRestaurants);
            }
        }

        [HttpPost]
        [CheckValidator] //проверка прав
        public async Task<IActionResult> Approve(int id, [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            await dbRepo.ApproveAsync(id);

            return RedirectToAction("Verification");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> VerificationMenu(int id, [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            //Грузим всё
            var allItems = await dbRepo.GetAsync(onlyApproved: false);

            //Ищем нужный ресторан
            var restaurant = allItems.OfType<Domain.Entities.Restaurant>()
                                     .FirstOrDefault(r => r.Id == id);

            if (restaurant == null) return NotFound();

            // проверка реально ли это владелец или админ
            if (restaurant.OwnerEmailAddress != User.Identity.Name && User.Identity.Name != "admin@test.com")
            {
                return Forbid(); // 403 ошибка
            }

            // только рестораны с меню в статусе "Pending"
            var pendingMenu = restaurant.MenuItems
                .Where(m => m.Status == "Pending")
                .ToList();

            ViewBag.RestaurantName = restaurant.Name;

            return View("VerificationMenu", pendingMenu);
        }


        [HttpPost]
        [Authorize]

        public async Task<IActionResult> ApproveMenuItem(Guid id, [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // 1. Одобряем через репозиторий
            await dbRepo.ApproveMenuAsync(id);

          
            return RedirectToAction("Verification");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // Используем GetRestaurantByIdAsync, а не GetAsync
            var restaurant = await dbRepo.GetRestaurantByIdAsync(id);

            // Если ресторан не найден в БД — тогда 404
            if (restaurant == null)
            {
                return NotFound();
            }

            // Передаем название ресторана в ViewBag
            ViewBag.RestaurantName = restaurant.Name;

            // Фильтруем меню. показываем только одобренные.
            // Используем ? и ??, чтобы не упало, если меню пустое.
            var approvedMenu = restaurant.MenuItems?
                .Where(m => m.Status == "Approved")
                .ToList() ?? new List<Domain.Entities.MenuItem>();

            return View(approvedMenu);
        }
    }
}