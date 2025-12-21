using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;



public class ImportPreviewViewComponent : ViewComponent
{
    // Используем ItemsInMemoryRepository, инжектированный по ключу "InMemory"
    private readonly IItemsRepository _inMemoryRepo;
    const string InMemoryKey = "InMemory";

    public ImportPreviewViewComponent([FromKeyedServices(InMemoryKey)] IItemsRepository inMemoryRepo)
    {
        _inMemoryRepo = inMemoryRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // все элементы, включая неподтвержденные
        var items = await _inMemoryRepo.GetAsync(onlyApproved: false);

        return View(items.ToList());
    }
}