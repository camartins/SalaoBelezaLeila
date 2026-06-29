namespace Domain.Dto
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiracao { get; set; }
        public UsuarioDto Usuario { get; set; }
    }
}