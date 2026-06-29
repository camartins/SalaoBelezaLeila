namespace Domain.Dto
{
    public class EditarUsuarioDto
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public int PerfilId { get; set; }
        public bool Ativo { get; set; }

        public string ValidarCamposObrigatorios()
        {
            if (string.IsNullOrWhiteSpace(Nome))
                return "Informe o nome.";

            if (string.IsNullOrWhiteSpace(Email))
                return "Informe o e-mail.";

            if (string.IsNullOrWhiteSpace(Telefone))
                return "Informe o telefone.";

            if (PerfilId <= 0)
                return "Informe o perfil.";

            return string.Empty;
        }
    }
}