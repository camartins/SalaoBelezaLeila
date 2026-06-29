using AutoMapper;
using Domain.Dto;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;

namespace Business.Services
{
    public class ServicoService : IServicoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServicoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ServicoDto>> ListarAtivosAsync()
        {
            var servicos = (await _unitOfWork.ServicoRepository
                .CustomFind(s => s.Ativo))
                .OrderBy(s => s.Nome)
                .ToList();

            return _mapper.Map<List<ServicoDto>>(servicos);
        }
    }
}
