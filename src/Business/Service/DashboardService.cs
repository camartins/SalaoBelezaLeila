using Domain.Dto;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using System.Globalization;

namespace Business.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardSemanalDto> ObterDesempenhoAsync(DateTime? dataInicio, DateTime? dataFim)
        {
            var inicio = dataInicio?.Date ?? ObterInicioSemana(DateTime.Today);
            var fim = dataFim?.Date ?? inicio.AddDays(6);

            if (fim < inicio)
                (inicio, fim) = (fim, inicio);

            if ((fim - inicio).TotalDays > 90)
                fim = inicio.AddDays(90);

            var fimInclusive = fim.Date.AddDays(1).AddTicks(-1);

            var agendamentos = await _unitOfWork.AgendamentoRepository
                .ListarHistoricoAsync(inicio, fimInclusive, null);

            var ativos = agendamentos.Where(a => a.Status != StatusAgendamento.Cancelado).ToList();
            var concluidos = agendamentos.Where(a => a.Status == StatusAgendamento.Concluido).ToList();

            var diasNoPeriodo = (int)(fim.Date - inicio.Date).TotalDays + 1;
            var cultura = new CultureInfo("pt-BR");

            var porDia = Enumerable.Range(0, diasNoPeriodo)
                .Select(i =>
                {
                    var data = inicio.AddDays(i);
                    var doDia = agendamentos.Where(a => a.DataHora.Date == data.Date).ToList();
                    var doDiaAtivos = doDia.Where(a => a.Status != StatusAgendamento.Cancelado).ToList();

                    return new DesempenhoDiarioDto
                    {
                        Dia = data.ToString("ddd", cultura),
                        Data = data,
                        Quantidade = doDiaAtivos.Count,
                        Faturamento = doDiaAtivos.Sum(a => a.Servicos.Sum(s => s.Servico?.Valor ?? 0))
                    };
                }).ToList();

            var faturamentoEstimado = ativos.Sum(a => a.Servicos.Sum(s => s.Servico?.Valor ?? 0));
            var faturamentoRealizado = concluidos.Sum(a => a.Servicos.Sum(s => s.Servico?.Valor ?? 0));
            var total = agendamentos.Count;
            var cancelados = agendamentos.Count(a => a.Status == StatusAgendamento.Cancelado);
            var qtdConcluidos = concluidos.Count;

            var duracaoMedia = ativos.Count > 0
                ? (int)ativos.Average(a => a.Servicos.Sum(s => s.Servico?.DuracaoMinutos ?? 0))
                : 0;

            var horarioPico = ativos
                .GroupBy(a => a.DataHora.ToString("HH:00"))
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? string.Empty;

            var diaMaisMovimentado = porDia
                .OrderByDescending(d => d.Quantidade)
                .Where(d => d.Quantidade > 0)
                .Select(d => $"{d.Dia} ({d.Data:dd/MM})")
                .FirstOrDefault() ?? string.Empty;

            var topServicos = ativos
                .SelectMany(a => a.Servicos)
                .GroupBy(s => s.Servico?.Nome ?? "Desconhecido")
                .Select(g => new ServicoRankingDto
                {
                    Nome = g.Key,
                    Quantidade = g.Count(),
                    Faturamento = g.Sum(s => s.Servico?.Valor ?? 0)
                })
                .OrderByDescending(s => s.Quantidade)
                .Take(5)
                .ToList();

            var taxaCancelamento = total > 0 ? Math.Round(cancelados * 100.0 / total, 1) : 0;
            var taxaConclusao = ativos.Count > 0 ? Math.Round(qtdConcluidos * 100.0 / ativos.Count, 1) : 0;
            var ticketMedio = ativos.Count > 0 ? Math.Round(faturamentoEstimado / ativos.Count, 2) : 0;

            var insights = GerarInsights(
                taxaCancelamento, taxaConclusao, total, cancelados,
                diaMaisMovimentado, horarioPico, topServicos, faturamentoEstimado, faturamentoRealizado);

            return new DashboardSemanalDto
            {
                InicioSemana = inicio,
                FimSemana = fimInclusive,
                TotalAgendamentos = total,
                Pendentes = agendamentos.Count(a => a.Status == StatusAgendamento.Pendente),
                Confirmados = agendamentos.Count(a => a.Status == StatusAgendamento.Confirmado),
                Cancelados = cancelados,
                Concluidos = qtdConcluidos,
                FaturamentoEstimado = faturamentoEstimado,
                FaturamentoRealizado = faturamentoRealizado,
                TicketMedio = ticketMedio,
                TaxaCancelamento = taxaCancelamento,
                TaxaConclusao = taxaConclusao,
                DuracaoMediaMinutos = duracaoMedia,
                HorarioPico = horarioPico,
                DiaMaisMovimentado = diaMaisMovimentado,
                AgendamentosPorDia = porDia,
                TopServicos = topServicos,
                Insights = insights
            };
        }

        private static List<DashboardInsightDto> GerarInsights(
            double taxaCancelamento,
            double taxaConclusao,
            int total,
            int cancelados,
            string diaMaisMovimentado,
            string horarioPico,
            List<ServicoRankingDto> topServicos,
            decimal faturamentoEstimado,
            decimal faturamentoRealizado)
        {
            var insights = new List<DashboardInsightDto>();

            if (total == 0)
            {
                insights.Add(new DashboardInsightDto
                {
                    Tipo = "info",
                    Mensagem = "Nenhum agendamento no período selecionado. Considere campanhas de divulgação ou promoções."
                });
                return insights;
            }

            if (taxaCancelamento >= 20)
            {
                insights.Add(new DashboardInsightDto
                {
                    Tipo = "warning",
                    Mensagem = $"Taxa de cancelamento elevada ({taxaCancelamento}%). Avalie confirmar agendamentos com antecedência ou enviar lembretes."
                });
            }
            else if (taxaCancelamento <= 5 && cancelados > 0)
            {
                insights.Add(new DashboardInsightDto
                {
                    Tipo = "success",
                    Mensagem = $"Baixa taxa de cancelamento ({taxaCancelamento}%). Clientes estão mantendo os compromissos."
                });
            }

            if (taxaConclusao >= 70)
            {
                insights.Add(new DashboardInsightDto
                {
                    Tipo = "success",
                    Mensagem = $"{taxaConclusao}% dos agendamentos ativos foram concluídos — boa taxa de conversão em atendimento."
                });
            }
            else if (taxaConclusao < 40 && total > 3)
            {
                insights.Add(new DashboardInsightDto
                {
                    Tipo = "warning",
                    Mensagem = $"Apenas {taxaConclusao}% dos agendamentos foram concluídos. Verifique pendências e confirmações."
                });
            }

            if (!string.IsNullOrEmpty(diaMaisMovimentado))
            {
                insights.Add(new DashboardInsightDto
                {
                    Tipo = "info",
                    Mensagem = $"Dia mais movimentado: {diaMaisMovimentado}. Reforce a equipe neste dia."
                });
            }

            if (!string.IsNullOrEmpty(horarioPico))
            {
                insights.Add(new DashboardInsightDto
                {
                    Tipo = "info",
                    Mensagem = $"Horário de pico: {horarioPico}. Considere bloquear este slot para clientes VIP ou combos."
                });
            }

            if (topServicos.Count > 0)
            {
                var top = topServicos[0];
                insights.Add(new DashboardInsightDto
                {
                    Tipo = "info",
                    Mensagem = $"Serviço mais procurado: {top.Nome} ({top.Quantidade}x · R$ {top.Faturamento:N2})."
                });
            }

            if (faturamentoRealizado > 0 && faturamentoEstimado > faturamentoRealizado)
            {
                var pendente = faturamentoEstimado - faturamentoRealizado;
                insights.Add(new DashboardInsightDto
                {
                    Tipo = "info",
                    Mensagem = $"R$ {pendente:N2} ainda em agendamentos confirmados/pendentes — potencial de faturamento no período."
                });
            }

            return insights;
        }

        private static DateTime ObterInicioSemana(DateTime data)
        {
            var diff = (7 + (data.DayOfWeek - DayOfWeek.Monday)) % 7;
            return data.AddDays(-diff).Date;
        }
    }
}
