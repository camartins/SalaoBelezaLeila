using Domain.Dto;
using Domain.Enums;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]/[action]")]
    [Authorize]
    public class AgendamentoController : MainController
    {
        private readonly IAgendamentoService _agendamentoService;

        public AgendamentoController(
            IConfiguration configuration,
            IAgendamentoService agendamentoService) : base(configuration)
        {
            _agendamentoService = agendamentoService;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarAgendamentoDto dto)
        {
            var resposta = await _agendamentoService.CriarAsync(dto, UserId);

            if (!resposta.success)
            {
                return BadRequest(new { titulo = resposta.title, mensagem = resposta.message });
            }

            return Ok(new
            {
                titulo = resposta.title,
                mensagem = resposta.message,
                dados = resposta.data
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CriarAdmin([FromBody] CriarAgendamentoAdminDto dto)
        {
            var resposta = await _agendamentoService.CriarPorAdminAsync(dto);

            if (!resposta.success)
            {
                return BadRequest(new { titulo = resposta.title, mensagem = resposta.message });
            }

            return Ok(new
            {
                titulo = resposta.title,
                mensagem = resposta.message,
                dados = resposta.data
            });
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] EditarAgendamentoDto dto)
        {
            var isAdmin = Perfil == "Admin";
            var resposta = await _agendamentoService.EditarAsync(dto, UserId, isAdmin);
            return CustomResponse(resposta.success, resposta.title, resposta.message);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            var isAdmin = Perfil == "Admin";
            var resposta = await _agendamentoService.BuscarPorIdAsync(id, UserId, isAdmin);

            if (!resposta.success)
                return BadRequest(new { titulo = resposta.title, mensagem = resposta.message });

            return Ok(new { dados = resposta.data });
        }

        [HttpGet]
        public async Task<IActionResult> Historico([FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim)
        {
            var isAdmin = Perfil == "Admin";
            var resposta = await _agendamentoService.ListarHistoricoAsync(dataInicio, dataFim, UserId, isAdmin);

            if (!resposta.success)
                return BadRequest(new { titulo = resposta.title, mensagem = resposta.message });

            return Ok(new { dados = resposta.data });
        }

        [HttpGet]
        public async Task<IActionResult> Sugestao([FromQuery] DateTime dataHora)
        {
            var resposta = await _agendamentoService.ObterSugestaoAsync(dataHora, UserId);

            if (!resposta.success)
                return BadRequest(new { titulo = resposta.title, mensagem = resposta.message });

            return Ok(new { dados = resposta.data });
        }

        [HttpGet]
        public async Task<IActionResult> HorariosDisponiveis(
            [FromQuery] DateTime data,
            [FromQuery] List<int>? servicoIds,
            [FromQuery] string? servicoIdsCsv = null,
            [FromQuery] int? excluirAgendamentoId = null)
        {
            var ids = servicoIds ?? new List<int>();
            if (ids.Count == 0 && !string.IsNullOrWhiteSpace(servicoIdsCsv))
            {
                ids = servicoIdsCsv
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(int.Parse)
                    .ToList();
            }

            var resposta = await _agendamentoService.ListarHorariosDisponiveisAsync(data, ids, excluirAgendamentoId);

            if (!resposta.success)
                return BadRequest(new { titulo = resposta.title, mensagem = resposta.message });

            return Ok(new { dados = resposta.data });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListarRecebidos(
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            [FromQuery] StatusAgendamento? status,
            [FromQuery] string? buscaCliente = null)
        {
            var resposta = await _agendamentoService.ListarRecebidosAsync(dataInicio, dataFim, status, buscaCliente);

            if (!resposta.success)
                return BadRequest(new { titulo = resposta.title, mensagem = resposta.message });

            return Ok(new { dados = resposta.data });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Confirmar(int id)
        {
            var resposta = await _agendamentoService.ConfirmarAsync(id);
            return CustomResponse(resposta.success, resposta.title, resposta.message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var isAdmin = Perfil == "Admin";
            var resposta = await _agendamentoService.CancelarAsync(id, UserId, isAdmin);
            return CustomResponse(resposta.success, resposta.title, resposta.message);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AlterarStatusServico([FromBody] AlterarStatusServicoDto dto)
        {
            var resposta = await _agendamentoService.AlterarStatusServicoAsync(dto);
            return CustomResponse(resposta.success, resposta.title, resposta.message);
        }
    }
}
