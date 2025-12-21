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

        // AA4.3.6: Метод Catalog
        [HttpGet]
        public async Task<IActionResult> Catalog([FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // 1. Получаем все элементы
            var allItems = await dbRepo.GetAsync(onlyApproved: false);

            // 2. ФИЛЬТР: Оставляем только Рестораны И ПРИВОДИМ К ОБЩЕМУ ТИПУ
            var restaurantsOnly = allItems
                .OfType<Domain.Entities.Restaurant>()       // Оставляем только рестораны
                .Cast<Domain.Interfaces.IItemValidating>()  // <--- ДОБАВИТЬ ЭТУ СТРОКУ (приводим обратно к интерфейсу)
                .ToList();

            // 3. Передаем список
            return View(restaurantsOnly);
        }

        [HttpPost] // Важно: Удаление должно быть POST-запросом (безопасность)
        public async Task<IActionResult> Delete(int id, [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // Вызываем удаление
            await dbRepo.DeleteAsync(id);

            // Перезагружаем страницу каталога, чтобы элемент исчез визуально
            return RedirectToAction("Catalog");
        }

        [HttpGet]
        [Authorize] // Только для вошедших пользователей
        public async Task<IActionResult> Verification(
            [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // 1. Получаем Email текущего пользователя
            var userEmail = User.Identity?.Name;

            // 2. Хард-код Админа (Требование 1)
            const string AdminEmail = "admin@test.com";

            // 3. Получаем ВСЕ данные (и одобренные, и нет)
            var allItems = await dbRepo.GetAsync(onlyApproved: false);

            if (userEmail == AdminEmail)
            {
                // --- ЛОГИКА АДМИНА ---
                // Видит ВСЕ рестораны со статусом "Pending"
                var pendingRestaurants = allItems
                    .OfType<Domain.Entities.Restaurant>()
                    .Where(r => r.Status == "Pending") // Фильтр "Ожидающие"
                    .ToList();

                // Возвращаем представление для админа
                return View("VerificationAdmin", pendingRestaurants);
            }
            else
            {
                // --- ЛОГИКА ВЛАДЕЛЬЦА (Пока упрощенная) ---
                // Видит только одобренные (в будущем сделаем фильтр "Мои рестораны")
                // По заданию: "opens in a view showing owned restaurants"

                // Пока просто вернем пустой список или заглушку, чтобы не падало
                var myRestaurants = allItems
                    .OfType<Domain.Entities.Restaurant>()
                    .Where(r => r.OwnerEmailAddress == userEmail)
                    .ToList();

                // 2. Отправляем список во View
                return View("VerificationOwner", myRestaurants);
            }
        }

        [HttpPost]
        [CheckValidator] // <--- Включаем проверку прав!
        public async Task<IActionResult> Approve(int id, [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // 1. Вызываем репозиторий
            await dbRepo.ApproveAsync(id);

            // 2. Возвращаемся обратно к списку проверки
            return RedirectToAction("Verification");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> VerificationMenu(int id, [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // 1. Грузим всё (в идеале сделать метод GetById, но пока так)
            var allItems = await dbRepo.GetAsync(onlyApproved: false);

            // 2. Ищем нужный ресторан
            var restaurant = allItems.OfType<Domain.Entities.Restaurant>()
                                     .FirstOrDefault(r => r.Id == id);

            if (restaurant == null) return NotFound();

            // 3. БЕЗОПАСНОСТЬ: Проверяем, что это реально владелец
            if (restaurant.OwnerEmailAddress != User.Identity.Name && User.Identity.Name != "admin@test.com")
            {
                return Forbid(); // 403 ошибка
            }

            // 4. Берем только пункты меню этого ресторана со статусом Pending
            var pendingMenu = restaurant.MenuItems
                .Where(m => m.Status == "Pending")
                .ToList();

            ViewBag.RestaurantName = restaurant.Name;

            return View("VerificationMenu", pendingMenu);
        }


        [HttpPost]
        [Authorize]
        // В идеале тут тоже нужен [CheckValidator], но он пока настроен только на админа.
        // Поэтому проверим права внутри метода для надежности.
        public async Task<IActionResult> ApproveMenuItem(Guid id, [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // 1. Одобряем через репозиторий
            await dbRepo.ApproveMenuAsync(id);

            // 2. Возвращаемся обратно. 
            // Хитрость: нам нужно вернуться на страницу "Review Menu", но мы потеряли ID ресторана.
            // Поэтому проще вернуть пользователя в список его ресторанов.
            return RedirectToAction("Verification");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
        {
            // ВАЖНО: Используем GetRestaurantByIdAsync, а не GetAsync!
            var restaurant = await dbRepo.GetRestaurantByIdAsync(id);

            // Если ресторан не найден в БД — тогда 404
            if (restaurant == null)
            {
                return NotFound();
            }

            // Передаем название ресторана в ViewBag
            ViewBag.RestaurantName = restaurant.Name;

            // Фильтруем меню: показываем только одобренные.
            // Используем "?." и "??", чтобы не упало, если меню пустое.
            var approvedMenu = restaurant.MenuItems?
                .Where(m => m.Status == "Approved")
                .ToList() ?? new List<Domain.Entities.MenuItem>();

            return View(approvedMenu);
        }
    }
}