using Domain.Enums;

namespace Domain.Dto
{
    public class AlterarStatusServicoDto
    {
        public int AgendamentoServicoId { get; set; }
        public StatusServico Status { get; set; }

        public string ValidarCamposObrigatorios()
        {
            if (AgendamentoServicoId <= 0)
                return "Informe o serviço do agendamento.";

            if (!Enum.IsDefined(typeof(StatusServico), Status))
                return "Status inválido.";

            return string.Empty;
        }
    }
}
