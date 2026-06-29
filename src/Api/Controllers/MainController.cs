using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Api.Controllers
{
    public class MainController : Controller
    {
        protected int UserId { get; private set; }
        protected string Nome { get; private set; } = string.Empty;
        protected string Email { get; private set; } = string.Empty;
        protected string Perfil { get; private set; } = string.Empty;

        protected readonly IConfiguration Configuration;

        public MainController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var identity = context.HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null && identity.IsAuthenticated)
            {
                if (identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                    UserId = int.Parse(identity.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                if (identity.HasClaim(c => c.Type == ClaimTypes.Name))
                    Nome = identity.FindFirst(ClaimTypes.Name)!.Value;

                if (identity.HasClaim(c => c.Type == ClaimTypes.Email))
                    Email = identity.FindFirst(ClaimTypes.Email)!.Value;

                if (identity.HasClaim(c => c.Type == ClaimTypes.Role))
                    Perfil = identity.FindFirst(ClaimTypes.Role)!.Value;
            }

            await base.OnActionExecutionAsync(context, next);
        }

        protected IActionResult CustomResponse(bool success, string title, string message)
        {
            if (success)
            {
                return Ok(new
                {
                    titulo = title,
                    mensagem = message
                });
            }

            return BadRequest(new
            {
                titulo = title,
                mensagem = message
            });
        }

        protected IActionResult CustomResponse<T>(bool success, string title, string message, T dados)
        {
            if (success)
            {
                return Ok(new
                {
                    titulo = title,
                    mensagem = message,
                    dados
                });
            }

            return BadRequest(new
            {
                titulo = title,
                mensagem = message
            });
        }
    }
}
