using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibraryBookLendingSystem_8._0_.Services.Interfaces;

namespace LibraryBookLendingSystem_8._0_.Controllers
{
    public static class ControllerResultExtensions
    {
        /// <summary>Turns a ServiceResult into 2xx with the value, or a ProblemDetails error.</summary>
        public static IActionResult ToActionResult<T>(this ControllerBase controller, ServiceResult<T> result)
        {
            if (result.Success)
            {
                return result.StatusCode == 204
                    ? controller.NoContent()
                    : controller.StatusCode(result.StatusCode, result.Value);
            }

            var title = result.StatusCode switch
            {
                400 => "Bad request",
                404 => "Not found",
                409 => "Conflict",
                _ => "Request failed"
            };
            return controller.Problem(detail: result.Error, statusCode: result.StatusCode, title: title);
        }
    }
}
