using Domain.Dto;

namespace Domain.Interfaces.Services
{
    public interface IServicoService
    {
        Task<List<ServicoDto>> ListarAtivosAsync();
    }
}
