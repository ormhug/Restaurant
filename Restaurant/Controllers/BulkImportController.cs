using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;

public class BulkImportController : Controller
{
    const string InMemoryKey = "InMemory";
    const string DbKey = "Database";
    private readonly IZipService _zipService; // Поле для инжектированного сервиса

    // Конструктор для Dependency Injection
    public BulkImportController(IZipService zipService)
    {
        _zipService = zipService;
    }

    // AA2.3.4, AA4.3.3: Метод BulkImport теперь возвращает ZIP-файл
    [HttpPost]
    public async Task<IActionResult> BulkImport(
        IFormFile jsonFile,
        [FromKeyedServices(InMemoryKey)] IItemsRepository inMemoryRepo)
    {
        if (jsonFile == null || jsonFile.Length == 0)
        {
            return BadRequest("File not provided.");
        }

        using (var stream = jsonFile.OpenReadStream())
        {
            var parsedItems = await ImportItemFactory.CreateAsync(stream);

            var validItems = new List<IItemValidating>();
            var errorsDictionary = new Dictionary<IItemValidating, List<string>>();

            foreach (var item in parsedItems)
            {
                var itemErrors = item.Validate();

                if (itemErrors.Any())
                {
                    // Добавление ошибок...
                    errorsDictionary.Add(item, itemErrors);
                }
                else
                {
                    validItems.Add(item);
                }
            }

            // 1. Сохранение ТОЛЬКО валидных элементов в кэш
            await inMemoryRepo.SaveAsync(validItems);

            // 2. Генерация ZIP-файла (AA4.3.3)
            var zipBytes = _zipService.GenerateZipForDownload(validItems);

            // 3. Возвращаем файл для скачивания
            return File(
                zipBytes,
                "application/zip",
                "Items_for_Image_Upload.zip");
        }

        // НЕДОСТИЖИМЫЙ КОД (БЫЛ УДАЛЕН): return View("Preview");
    }

    // НОВЫЙ GET-метод для отображения страницы предпросмотра после скачивания ZIP
    [HttpGet]
    public IActionResult Preview()
    {
        return View();
    }

    // AA4.3: Метод Commit (будет обновлен на следующем шаге для приема ZIP)
    [HttpPost]
    public async Task<IActionResult> Commit(
        IFormFile zipFile, // НОВЫЙ ПАРАМЕТР: Принимаем ZIP-файл с изображениями
        [FromKeyedServices(InMemoryKey)] IItemsRepository inMemoryRepo,
        [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
    {
        // 1. Чтение данных из кэша
        var itemsToCommit = (await inMemoryRepo.GetAsync()).ToList();

        if (!itemsToCommit.Any())
        {
            return RedirectToAction("Catalog", "Items");
        }

        // 2. [НОВЫЙ ШАГ] Обработка и сохранение изображений из ZIP (AA4.3.4)
        if (zipFile != null && zipFile.Length > 0)
        {
            // Вызываем сервис для извлечения, сохранения файлов и обновления itemsToCommit
            using (var stream = zipFile.OpenReadStream())
            {
                await _zipService.ProcessUploadedZipAsync(stream, itemsToCommit);
            }
        }

        // 3. Сохранение ОБНОВЛЕННЫХ данных в БД:
        await dbRepo.SaveAsync(itemsToCommit);

        // 4. Очистка кэша:
        inMemoryRepo.Clear();

        TempData["SuccessMessage"] = "Импорт данных и изображений завершен!";
        return RedirectToAction("Catalog", "Items");
    }
}