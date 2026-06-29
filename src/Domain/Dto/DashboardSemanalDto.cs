namespace Domain.Dto
{
    public class DashboardSemanalDto
    {
        public DateTime InicioSemana { get; set; }
        public DateTime FimSemana { get; set; }
        public int TotalAgendamentos { get; set; }
        public int Pendentes { get; set; }
        public int Confirmados { get; set; }
        public int Cancelados { get; set; }
        public int Concluidos { get; set; }
        public decimal FaturamentoEstimado { get; set; }
        public decimal FaturamentoRealizado { get; set; }
        public decimal TicketMedio { get; set; }
        public double TaxaCancelamento { get; set; }
        public double TaxaConclusao { get; set; }
        public int DuracaoMediaMinutos { get; set; }
        public string HorarioPico { get; set; } = string.Empty;
        public string DiaMaisMovimentado { get; set; } = string.Empty;
        public List<DesempenhoDiarioDto> AgendamentosPorDia { get; set; } = new();
        public List<ServicoRankingDto> TopServicos { get; set; } = new();
        public List<DashboardInsightDto> Insights { get; set; } = new();
    }
}
