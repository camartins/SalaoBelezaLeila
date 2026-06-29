namespace Domain.Dto
{
    public class HorariosDisponiveisDto
    {
        public DateTime Data { get; set; }
        public int DuracaoTotalMinutos { get; set; }
        public List<string> HorariosDisponiveis { get; set; } = new();
    }
}
