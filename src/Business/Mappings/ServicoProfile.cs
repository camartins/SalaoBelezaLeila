using AutoMapper;
using Domain.Dto;
using Domain.Entities;

namespace Business.Mappings
{
    public class ServicoProfile : Profile
    {
        public ServicoProfile()
        {
            CreateMap<Servico, ServicoDto>();
        }
    }
}
