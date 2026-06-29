using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("PERFIL_USUARIO")]
    public class PerfilUsuario : BaseEntity
    {
        [Column("DESCRICAO")]
        public string Descricao { get; set; }

        [Column("ATIVO")]
        public bool Ativo { get; set; }
    }
}
