namespace Domain.Dto
{
    public class CriarAgendamentoDto
    {
        public DateTime DataHora { get; set; }
        public List<int> ServicoIds { get; set; } = new();

        public string ValidarCamposObrigatorios()
        {
            if (DataHora == default)
                return "Informe a data e hora do agendamento.";

            if (DataHora <= DateTime.Now)
                return "A data do agendamento deve ser futura.";

            if ((DataHora.Date - DateTime.Today).TotalDays < 2)
                return "Agendamentos devem ser feitos com pelo menos 2 dias de antecedência.";

            if (ServicoIds == null || ServicoIds.Count == 0)
                return "Informe ao menos um serviço.";

            return string.Empty;
        }
    }
}
