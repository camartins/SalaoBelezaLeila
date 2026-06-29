using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("USUARIO")]
    public class Usuario : BaseEntity
    {
        [Column("NOME")]
        public string Nome { get; set; }

        [Column("EMAIL")]
        public string Email { get; set; }

        [Column("SENHA")]
        public string Senha { get; set; }

        [Column("TELEFONE")]
        public string Telefone { get; set; }

        [Column("ID_PERFIL")]
        public int PerfilId { get; set; }

        [ForeignKey(nameof(PerfilId))]
        public PerfilUsuario Perfil { get; set; }

        [Column("ATIVO")]
        public bool Ativo { get; set; }

        public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    }
}
