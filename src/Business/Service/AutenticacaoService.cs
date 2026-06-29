using AutoMapper;
using Business.Configurations;
using Business.Helpers;
using Domain.Dto;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Business.Services
{
    public class AutenticacaoService : IAutenticacaoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ModalTitleConfig _modalConfig;
        private readonly JwtSettings _jwtSettings;

        public AutenticacaoService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ModalTitleConfig modalConfig,
            IOptions<JwtSettings> jwtSettings)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _modalConfig = modalConfig;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<(bool success, string title, string message, LoginResponseDto? data)> LoginAsync(LoginDto dto)
        {
            try
            {
                var erroValidacao = dto.ValidarCamposObrigatorios();

                if (!string.IsNullOrEmpty(erroValidacao))
                    return (false, _modalConfig.Error, erroValidacao, null);

                var usuario = (await _unitOfWork.UsuarioRepository
                    .CustomFind(x => x.Email == dto.Email))
                    .FirstOrDefault();

                if (usuario == null || !usuario.Ativo)
                    return (false, _modalConfig.Error, "E-mail ou senha inválidos.", null);

                if (!PasswordHelper.Verify(dto.Senha, usuario.Senha))
                    return (false, _modalConfig.Error, "E-mail ou senha inválidos.", null);

                var expiracao = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);
                var token = GerarToken(usuario, expiracao);

                return (true, _modalConfig.GeneralSuccess, "Login realizado com sucesso.", new LoginResponseDto
                {
                    Token = token,
                    Expiracao = expiracao,
                    Usuario = _mapper.Map<UsuarioDto>(usuario)
                });
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message, null);
            }
        }

        private string GerarToken(Usuario usuario, DateTime expiracao)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Nome),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role, ObterRole(usuario.PerfilId))
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiracao,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string ObterRole(int perfilId) => perfilId switch
        {
            (int)PerfilEnum.Usuario_admin => "Admin",
            (int)PerfilEnum.Cliente => "Cliente",
            _ => "Cliente"
        };
    }
}
