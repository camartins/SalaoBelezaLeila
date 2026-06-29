namespace Domain.Dto
{
    public class ServicoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int DuracaoMinutos { get; set; }
    }
}
