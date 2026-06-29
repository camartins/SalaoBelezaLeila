using Domain.Dto;

namespace Domain.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardSemanalDto> ObterDesempenhoAsync(DateTime? dataInicio, DateTime? dataFim);
    }
}
