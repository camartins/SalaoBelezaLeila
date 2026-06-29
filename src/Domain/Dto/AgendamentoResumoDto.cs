namespace Domain.Dto
{
    public class AgendamentoResumoDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }
        public DateTime DataHoraFim { get; set; }
        public string Status { get; set; } = string.Empty;
        public int QuantidadeServicos { get; set; }
        public int DuracaoTotalMinutos { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
