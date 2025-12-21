using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Restaurant.Filters
{
    public class CheckValidatorAttribute : ActionFilterAttribute
    {
        // Список тех, кому разрешено утверждать (GetValidators)
        private readonly string[] _allowedValidators = new[] { "admin@test.com" };

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //получаем текущего пользователя
            var user = context.HttpContext.User;

            // проверка залогинен ли он И есть ли его email в списке разрешенных
            if (!user.Identity.IsAuthenticated ||
                !_allowedValidators.Contains(user.Identity.Name))
            {
                // если нет прав ошибка 403 Forbidden
                context.Result = new StatusCodeResult(403);
            }

            //метод контроллера выполнится дальше
            base.OnActionExecuting(context);
        }
    }
}