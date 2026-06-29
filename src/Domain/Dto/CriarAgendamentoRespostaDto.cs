namespace Domain.Dto
{
    public class CriarAgendamentoRespostaDto
    {
        public AgendamentoDto Agendamento { get; set; } = null!;
        public SugestaoAgendamentoDto? Sugestao { get; set; }
    }
}
