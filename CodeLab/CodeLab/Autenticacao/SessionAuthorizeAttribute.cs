using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CodeLab.Autenticacao
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public string? RoleAnyOf { get; set; }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var http = context.HttpContext;
            var role = http.Session.GetString(SessionKeys.UserRole);
            var userId = http.Session.GetInt32(SessionKeys.UserId);

            if (userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (!string.IsNullOrWhiteSpace(RoleAnyOf))

            {
                var allowed = RoleAnyOf.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (!allowed.Contains(role))
                {

                    context.Result = new RedirectToActionResult("Login", "Auth", null);
                    return;
                }
                base.OnActionExecuted(context);
            }
        }

    }
}
