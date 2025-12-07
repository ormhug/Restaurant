using System.Linq;
using System.Threading.Tasks;
using Domain; // Добавили using для ImportItemFactory
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Добавили using для IFormFile

// Ключи, которые мы определили в Program.cs:
// const string InMemoryKey = "InMemory"; 
// const string DbKey = "Database";      


public class BulkImportController : Controller
{
    const string InMemoryKey = "InMemory";
    const string DbKey = "Database";

    // AA2.3.4: Метод BulkImport использует только InMemory репозиторий
    [HttpPost]
    public async Task<IActionResult> BulkImport( // Сделали метод асинхронным
        IFormFile jsonFile,
        // Инъекция ItemsInMemoryRepository по ключу "InMemory"
        [FromKeyedServices(InMemoryKey)] IItemsRepository inMemoryRepo)
    {
        if (jsonFile == null || jsonFile.Length == 0)
        {
            return BadRequest("File not provided.");
        }

        using (var stream = jsonFile.OpenReadStream())
        {
            // 1. Вызов ImportItemFactory.CreateAsync (AA2.3.1)
            // parsedItems теперь является List<IItemValidating>
            var parsedItems = await ImportItemFactory.CreateAsync(stream);

            // --- НОВАЯ ЛОГИКА ВАЛИДАЦИИ (AA4.3.1) ---
            var validItems = new List<IItemValidating>();
            var errorsDictionary = new Dictionary<IItemValidating, List<string>>();

            foreach (var item in parsedItems)
            {
                var itemErrors = item.Validate();

                if (itemErrors.Any())
                {
                    // Если есть ошибки, добавляем элемент и ошибки в словарь ошибок
                    errorsDictionary.Add(item, itemErrors);
                }
                else
                {
                    // Если ошибок нет, добавляем элемент в список для сохранения
                    validItems.Add(item);
                }
            }
            // --- КОНЕЦ ЛОГИКИ ВАЛИДАЦИИ ---

            // 2. Вызов SaveAsync: теперь сохраняем ТОЛЬКО валидные элементы
            await inMemoryRepo.SaveAsync(validItems);

            // В реальном приложении: Сохраните errorsDictionary в TempData/ViewData
            // чтобы отобразить эти ошибки на странице "Preview".
            // Например: TempData["ImportErrors"] = errorsDictionary; 

        }

        // Перенаправляем на страницу предпросмотра
        return View("Preview");
    }

    // AA4.3: Метод Commit использует оба репозитория
    [HttpPost]
    public async Task<IActionResult> Commit( // Сделали метод асинхронным
        [FromKeyedServices(InMemoryKey)] IItemsRepository inMemoryRepo,
        [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
    {
        // 1. Чтение данных из кэша: var items = await inMemoryRepo.GetAsync();
        var itemsToCommit = await inMemoryRepo.GetAsync();

        if (!itemsToCommit.Any())
        {
            return RedirectToAction("Catalog", "Items");
        }

        // 2. Сохранение в БД: await dbRepo.SaveAsync(items);
        await dbRepo.SaveAsync(itemsToCommit);

        // 3. Очистка кэша: inMemoryRepo.Clear();
        // Clear обычно синхронен, но если он асинхронен, нужно добавить await.
        // Предполагаем, что Clear в ItemsInMemoryRepository синхронен.
        inMemoryRepo.Clear();

        return RedirectToAction("Catalog", "Items");
    }
}