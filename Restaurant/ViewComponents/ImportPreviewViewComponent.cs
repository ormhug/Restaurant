using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

// Временно используем DataAccess для доступа к интерфейсу репозитория
// В реальном приложении репозитории DataAccess лучше инжектировать через
// абстракцию в Core/Domain

public class ImportPreviewViewComponent : ViewComponent
{
    // AA2.3.4: Используем ItemsInMemoryRepository, инжектированный по ключу "InMemory"
    private readonly IItemsRepository _inMemoryRepo;
    const string InMemoryKey = "InMemory";

    public ImportPreviewViewComponent([FromKeyedServices(InMemoryKey)] IItemsRepository inMemoryRepo)
    {
        _inMemoryRepo = inMemoryRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // 1. Получаем все элементы из кэша (вне зависимости от статуса, т.к. только валидные)
        var items = await _inMemoryRepo.GetAsync(onlyApproved: false);

        // 2. Возвращаем элементы в представление
        return View(items.ToList());
    }
}