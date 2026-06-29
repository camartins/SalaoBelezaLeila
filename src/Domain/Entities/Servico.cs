using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("SERVICO")]
    public class Servico : BaseEntity
    {
        [Column("NOME")]
        public string Nome { get; set; }

        [Column("VALOR")]
        public decimal Valor { get; set; }

        [Column("DURACAO_MINUTOS")]
        public int DuracaoMinutos { get; set; }

        [Column("ATIVO")]
        public bool Ativo { get; set; }

        public ICollection<AgendamentoServico> AgendamentoServicos { get; set; }
    }
}