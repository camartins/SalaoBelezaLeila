using Domain.Dto;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]/[action]")]
    public class UsuarioController : MainController
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(
            IConfiguration configuration,
            IUsuarioService usuarioService) : base(configuration)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Cadastrar([FromBody] CadastrarUsuarioDto dto)
        {
            var resposta = await _usuarioService.CadastrarAsync(dto);

            return CustomResponse(
                resposta.success,
                resposta.title,
                resposta.message);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Editar([FromBody] EditarUsuarioDto dto)
        {
            var resposta = await _usuarioService.EditarAsync(dto, UserId);

            return CustomResponse(
                resposta.success,
                resposta.title,
                resposta.message);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> BuscarDados()
        {
            var usuario = await _usuarioService.BuscarPorIdAsync(UserId);

            return Ok(new
            {
                dados = usuario
            });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Listar()
        {
            var usuarios = await _usuarioService.ListarAsync();

            return Ok(new
            {
                dados = usuarios
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Excluir(int id)
        {
            var resposta = await _usuarioService.ExcluirAsync(id);

            return CustomResponse(
                resposta.success,
                resposta.title,
                resposta.message);
        }
    }
}