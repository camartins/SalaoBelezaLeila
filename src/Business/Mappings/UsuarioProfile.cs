using AutoMapper;
using Domain.Dto;
using Domain.Entities;
using Domain.Enums;

namespace Business.Mappings
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            CreateMap<CadastrarUsuarioDto, Usuario>()
                .ForMember(dest => dest.Senha, opt => opt.Ignore());

            CreateMap<Usuario, UsuarioDto>()
                .ForMember(dest => dest.Perfil, opt => opt.MapFrom(src => ObterNomePerfil(src.PerfilId)));
        }

        private static string ObterNomePerfil(int perfilId) => perfilId switch
        {
            (int)PerfilEnum.Usuario_admin => "Admin",
            (int)PerfilEnum.Cliente => "Cliente",
            _ => "Desconhecido"
        };
    }
}
