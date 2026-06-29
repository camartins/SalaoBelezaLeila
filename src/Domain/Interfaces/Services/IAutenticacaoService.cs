using Domain.Dto;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IAutenticacaoService
    {
        Task<(bool success, string title, string message, LoginResponseDto? data)> LoginAsync(LoginDto dto);
    }
}
