using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("AGENDAMENTO_SERVICO")]
    public class AgendamentoServico : BaseEntity
    {
        [Column("ID_AGENDAMENTO")]
        public int AgendamentoId { get; set; }

        [Column("ID_SERVICO")]
        public int ServicoId { get; set; }

        [Column("STATUS")]
        public StatusServico Status { get; set; }

        [ForeignKey(nameof(AgendamentoId))]
        public Agendamento Agendamento { get; set; }

        [ForeignKey(nameof(ServicoId))]
        public Servico Servico { get; set; }
    }
}