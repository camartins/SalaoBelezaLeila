namespace Business.Configurations
{
    public class SalaoHorarioConfig
    {
        public string HoraAbertura { get; set; } = "08:00";
        public string HoraFechamento { get; set; } = "18:00";
        public int IntervaloMinutos { get; set; } = 30;
    }
}
