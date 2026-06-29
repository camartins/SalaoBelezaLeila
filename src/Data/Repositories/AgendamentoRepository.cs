using Data.Context;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Data.Repositories
{
    public class AgendamentoRepository : BaseRepository<Agendamento>, IAgendamentoRepository
    {
        public AgendamentoRepository(SalaoCabeleleilaDbContext context) : base(context)
        {
        }

        public async Task<Agendamento?> GetCompletoByIdAsync(int id)
        {
            return await _context.Set<Agendamento>()
                .Include(a => a.Usuario)
                .Include(a => a.Servicos)
                    .ThenInclude(s => s.Servico)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Agendamento>> ListarPorUsuarioNaSemanaAsync(int usuarioId, DateTime dataReferencia)
        {
            var agendamentos = await _context.Set<Agendamento>()
                .Where(a => a.UsuarioId == usuarioId && a.Status != StatusAgendamento.Cancelado)
                .ToListAsync();

            return agendamentos
                .Where(a => MesmaSemana(a.DataHora, dataReferencia))
                .OrderBy(a => a.DataHora)
                .ToList();
        }

        public async Task<List<Agendamento>> ListarHistoricoAsync(DateTime inicio, DateTime fim, int? usuarioId)
        {
            var query = _context.Set<Agendamento>()
                .Include(a => a.Usuario)
                .Include(a => a.Servicos)
                    .ThenInclude(s => s.Servico)
                .Where(a => a.DataHora >= inicio && a.DataHora <= fim);

            if (usuarioId.HasValue)
                query = query.Where(a => a.UsuarioId == usuarioId.Value);

            return await query
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();
        }

        public async Task<List<Agendamento>> ListarAtivosNoDiaAsync(DateTime data, int? excluirAgendamentoId = null)
        {
            var diaInicio = data.Date;
            var diaFim = diaInicio.AddDays(1);

            var query = _context.Set<Agendamento>()
                .Include(a => a.Servicos)
                    .ThenInclude(s => s.Servico)
                .Where(a => a.Status != StatusAgendamento.Cancelado
                         && a.DataHora >= diaInicio
                         && a.DataHora < diaFim);

            if (excluirAgendamentoId.HasValue)
                query = query.Where(a => a.Id != excluirAgendamentoId.Value);

            return await query
                .OrderBy(a => a.DataHora)
                .ToListAsync();
        }

        public async Task<List<Agendamento>> ListarRecebidosAsync(
            DateTime? dataInicio, DateTime? dataFim, int? statusAgendamento, string? buscaCliente = null)
        {
            var query = _context.Set<Agendamento>()
                .Include(a => a.Usuario)
                .Include(a => a.Servicos)
                    .ThenInclude(s => s.Servico)
                .AsQueryable();

            if (dataInicio.HasValue)
                query = query.Where(a => a.DataHora >= dataInicio.Value.Date);

            if (dataFim.HasValue)
            {
                var fim = dataFim.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.DataHora <= fim);
            }

            if (statusAgendamento.HasValue)
                query = query.Where(a => (int)a.Status == statusAgendamento.Value);

            if (!string.IsNullOrWhiteSpace(buscaCliente))
            {
                var termo = buscaCliente.Trim().ToLower();
                query = query.Where(a => a.Usuario != null && a.Usuario.Nome.ToLower().Contains(termo));
            }

            return await query
                .OrderBy(a => a.DataHora)
                .ToListAsync();
        }

        public async Task<AgendamentoServico?> GetAgendamentoServicoCompletoAsync(int agendamentoServicoId)
        {
            return await _context.Set<AgendamentoServico>()
                .Include(s => s.Servico)
                .Include(s => s.Agendamento)
                .FirstOrDefaultAsync(s => s.Id == agendamentoServicoId);
        }

        private static bool MesmaSemana(DateTime data1, DateTime data2)
        {
            var calendario = CultureInfo.InvariantCulture.Calendar;
            var semana1 = calendario.GetWeekOfYear(data1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var semana2 = calendario.GetWeekOfYear(data2, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return data1.Year == data2.Year && semana1 == semana2;
        }
    }
}
