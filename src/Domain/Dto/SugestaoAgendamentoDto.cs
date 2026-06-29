namespace Domain.Dto
{
    public class SugestaoAgendamentoDto
    {
        public bool PossuiAgendamentoNaSemana { get; set; }
        public DateTime DataSugerida { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
}
