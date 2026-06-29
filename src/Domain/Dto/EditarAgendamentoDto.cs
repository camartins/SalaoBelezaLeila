namespace Domain.Dto
{
    public class EditarAgendamentoDto
    {
        public int AgendamentoId { get; set; }
        public DateTime DataHora { get; set; }
        public List<int> ServicoIds { get; set; } = new();

        public string ValidarCamposObrigatorios()
        {
            if (AgendamentoId <= 0)
                return "Informe o agendamento.";

            if (DataHora == default)
                return "Informe a data e hora do agendamento.";

            if (DataHora <= DateTime.Now)
                return "A data do agendamento deve ser futura.";

            if (ServicoIds == null || ServicoIds.Count == 0)
                return "Informe ao menos um serviço.";

            return string.Empty;
        }
    }
}
