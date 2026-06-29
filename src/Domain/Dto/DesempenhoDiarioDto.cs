namespace Domain.Dto
{
    public class DesempenhoDiarioDto
    {
        public string Dia { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public int Quantidade { get; set; }
        public decimal Faturamento { get; set; }
    }
}
