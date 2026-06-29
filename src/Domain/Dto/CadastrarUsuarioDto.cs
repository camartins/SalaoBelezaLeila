namespace Domain.Dto
{
    public class CadastrarUsuarioDto
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string Telefone { get; set; }

        public string ValidarCamposObrigatorios()
        {
            if (string.IsNullOrWhiteSpace(Nome))
                return "Informe o nome.";

            if (string.IsNullOrWhiteSpace(Email))
                return "Informe o e-mail.";

            if (string.IsNullOrWhiteSpace(Senha))
                return "Informe a senha.";

            if (string.IsNullOrWhiteSpace(Telefone))
                return "Informe o telefone.";

            return string.Empty;
        }
    }
}
