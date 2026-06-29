using Domain.Dto;
using Domain.Enums;

namespace Domain.Interfaces.Services
{
    public interface IAgendamentoService
    {
        Task<(bool success, string title, string message, CriarAgendamentoRespostaDto? data)> CriarAsync(CriarAgendamentoDto dto, int usuarioId);
        Task<(bool success, string title, string message, CriarAgendamentoRespostaDto? data)> CriarPorAdminAsync(CriarAgendamentoAdminDto dto);
        Task<(bool success, string title, string message)> EditarAsync(EditarAgendamentoDto dto, int usuarioId, bool isAdmin);
        Task<(bool success, string title, string message, AgendamentoDto? data)> BuscarPorIdAsync(int agendamentoId, int usuarioId, bool isAdmin);
        Task<(bool success, string title, string message, List<AgendamentoResumoDto>? data)> ListarHistoricoAsync(DateTime dataInicio, DateTime dataFim, int usuarioId, bool isAdmin);
        Task<(bool success, string title, string message, SugestaoAgendamentoDto? data)> ObterSugestaoAsync(DateTime dataHora, int usuarioId);
        Task<(bool success, string title, string message, HorariosDisponiveisDto? data)> ListarHorariosDisponiveisAsync(DateTime data, List<int> servicoIds, int? excluirAgendamentoId);
        Task<(bool success, string title, string message, List<AgendamentoResumoDto>? data)> ListarRecebidosAsync(
            DateTime? dataInicio, DateTime? dataFim, StatusAgendamento? status, string? buscaCliente);
        Task<(bool success, string title, string message)> ConfirmarAsync(int agendamentoId);
        Task<(bool success, string title, string message)> CancelarAsync(int agendamentoId, int usuarioId, bool isAdmin);
        Task<(bool success, string title, string message)> AlterarStatusServicoAsync(AlterarStatusServicoDto dto);
    }
}
