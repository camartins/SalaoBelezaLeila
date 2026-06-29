using Domain.Entities;

namespace Domain.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IBaseRepository<Usuario> UsuarioRepository { get; }
        IAgendamentoRepository AgendamentoRepository { get; }
        IBaseRepository<AgendamentoServico> AgendamentoServicoRepository { get; }
        IBaseRepository<Servico> ServicoRepository { get; }
        Task<bool> CommitAsync();
    }
}
