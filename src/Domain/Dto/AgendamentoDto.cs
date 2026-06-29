namespace Domain.Dto
{
    public class AgendamentoDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }
        public DateTime DataHoraFim { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public int DuracaoTotalMinutos { get; set; }
        public List<AgendamentoServicoDto> Servicos { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
