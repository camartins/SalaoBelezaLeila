using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("AGENDAMENTO")]
    public class Agendamento : BaseEntity
    {
        [Column("ID_USUARIO")]
        public int UsuarioId { get; set; }

        [Column("DATA_HORA")]
        public DateTime DataHora { get; set; }

        [Column("STATUS")]
        public StatusAgendamento Status { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; }

        public ICollection<AgendamentoServico> Servicos { get; set; } = new List<AgendamentoServico>();
    }
}
