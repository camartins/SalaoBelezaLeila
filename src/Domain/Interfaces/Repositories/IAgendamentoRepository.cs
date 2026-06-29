using Domain.Entities;

namespace Domain.Interfaces.Repositories
{
    public interface IAgendamentoRepository : IBaseRepository<Agendamento>
    {
        Task<Agendamento?> GetCompletoByIdAsync(int id);
        Task<List<Agendamento>> ListarPorUsuarioNaSemanaAsync(int usuarioId, DateTime dataReferencia);
        Task<List<Agendamento>> ListarHistoricoAsync(DateTime inicio, DateTime fim, int? usuarioId);
        Task<List<Agendamento>> ListarAtivosNoDiaAsync(DateTime data, int? excluirAgendamentoId = null);
        Task<List<Agendamento>> ListarRecebidosAsync(
            DateTime? dataInicio, DateTime? dataFim, int? statusAgendamento, string? buscaCliente = null);
        Task<AgendamentoServico?> GetAgendamentoServicoCompletoAsync(int agendamentoServicoId);
    }
}
