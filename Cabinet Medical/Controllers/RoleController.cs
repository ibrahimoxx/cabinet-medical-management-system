using Microsoft.AspNetCore.Mvc;

namespace Cabinet_Medical.Controllers.Base
{
    public abstract class RoleController : Controller
    {
        private readonly string _requiredRole;

        protected RoleController(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public override void OnActionExecuting(
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectToActionResult(
                    "Login", "Account", null);
                return;
            }

            if (role != _requiredRole)
            {
                context.Result = new RedirectToActionResult(
                    "Index", "Dashboard", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
