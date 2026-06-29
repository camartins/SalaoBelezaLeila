using Data.Context;
using Data.Repositories;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly SalaoCabeleleilaDbContext _context;

        private IBaseRepository<Usuario>? _usuarioRepository;
        private IAgendamentoRepository? _agendamentoRepository;
        private IBaseRepository<AgendamentoServico>? _agendamentoServicoRepository;
        private IBaseRepository<Servico>? _servicoRepository;

        public UnitOfWork(SalaoCabeleleilaDbContext context)
        {
            _context = context;
        }

        public IBaseRepository<Usuario> UsuarioRepository =>
            _usuarioRepository ??= new BaseRepository<Usuario>(_context);

        public IAgendamentoRepository AgendamentoRepository =>
            _agendamentoRepository ??= new AgendamentoRepository(_context);

        public IBaseRepository<AgendamentoServico> AgendamentoServicoRepository =>
            _agendamentoServicoRepository ??= new BaseRepository<AgendamentoServico>(_context);

        public IBaseRepository<Servico> ServicoRepository =>
            _servicoRepository ??= new BaseRepository<Servico>(_context);

        public async Task<bool> CommitAsync()
        {
            try
            {
                var entries = _context.ChangeTracker.Entries()
                    .Where(e => e.Entity is BaseEntity &&
                               (e.State == EntityState.Added ||
                                e.State == EntityState.Modified));

                foreach (var entityEntry in entries)
                {
                    if (entityEntry.State == EntityState.Added)
                        ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.Now;

                    if (entityEntry.State == EntityState.Modified)
                        ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.Now;
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
