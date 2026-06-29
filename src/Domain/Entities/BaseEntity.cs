using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        [Column("DATA_CADASTRO")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("DATA_ATUALIZACAO")]
        public DateTime? UpdatedAt { get; set; }
    }
}
