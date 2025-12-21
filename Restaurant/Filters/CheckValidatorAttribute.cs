using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Restaurant.Filters
{
    public class CheckValidatorAttribute : ActionFilterAttribute
    {
        // Список тех, кому разрешено утверждать (по заданию - GetValidators)
        private readonly string[] _allowedValidators = new[] { "admin@test.com" };

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Получаем текущего пользователя
            var user = context.HttpContext.User;

            // 2. Проверяем: залогинен ли он И есть ли его email в списке разрешенных
            if (!user.Identity.IsAuthenticated ||
                !_allowedValidators.Contains(user.Identity.Name))
            {
                // Если нет прав — возвращаем ошибку 403 Forbidden
                context.Result = new StatusCodeResult(403);
            }

            // Если всё ок — метод контроллера выполнится дальше
            base.OnActionExecuting(context);
        }
    }
}