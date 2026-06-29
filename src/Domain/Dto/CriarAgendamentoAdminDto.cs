namespace Domain.Dto
{
    public class CriarAgendamentoAdminDto
    {
        public int UsuarioId { get; set; }
        public DateTime DataHora { get; set; }
        public List<int> ServicoIds { get; set; } = new();

        public string ValidarCamposObrigatorios()
        {
            if (UsuarioId <= 0)
                return "Selecione o cliente.";

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
