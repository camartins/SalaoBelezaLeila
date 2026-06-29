namespace Domain.Dto
{
    public class AgendamentoServicoDto
    {
        public int Id { get; set; }
        public int ServicoId { get; set; }
        public string NomeServico { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int DuracaoMinutos { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
