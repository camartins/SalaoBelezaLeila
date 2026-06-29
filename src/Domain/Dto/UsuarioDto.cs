namespace Domain.Dto
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public int PerfilId { get; set; }
        public string Perfil { get; set; }
        public bool Ativo { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
