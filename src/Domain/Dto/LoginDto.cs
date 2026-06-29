namespace Domain.Dto
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Senha { get; set; }

        public string ValidarCamposObrigatorios()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return "Informe o e-mail.";

            if (string.IsNullOrWhiteSpace(Senha))
                return "Informe a senha.";

            return string.Empty;
        }
    }
}