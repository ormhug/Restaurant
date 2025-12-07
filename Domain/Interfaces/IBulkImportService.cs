//// IBulkImportService.cs

//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using BulkImport.Models; // Или где у вас находится модель BulkImportResult

//public interface IBulkImportService
//{
//    /// <summary>
//    /// Обрабатывает загруженный файл, парсит, валидирует и сохраняет данные в In-Memory кэш.
//    /// </summary>
//    Task<BulkImportResult> ImportBulkDataAsync(IFormFile file);

//    /// <summary>
//    /// Перемещает все валидированные данные из кэша в основной репозиторий (базу данных).
//    /// </summary>
//    Task CommitAsync();
//}