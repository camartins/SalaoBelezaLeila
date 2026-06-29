using Domain.Dto;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]/[action]")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly IAutenticacaoService _autenticacaoService;

        public AutenticacaoController(IAutenticacaoService autenticacaoService)
        {
            _autenticacaoService = autenticacaoService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var resposta = await _autenticacaoService.LoginAsync(dto);

            if (!resposta.success)
            {
                return BadRequest(new
                {
                    titulo = resposta.title,
                    mensagem = resposta.message
                });
            }

            return Ok(new
            {
                titulo = resposta.title,
                mensagem = resposta.message,
                dados = resposta.data
            });
        }
    }
}
