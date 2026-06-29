using Domain.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IUsuarioService
    {
        Task<(bool success, string title, string message)> CadastrarAsync(CadastrarUsuarioDto dto);
        Task<(bool success, string title, string message)> EditarAsync(EditarUsuarioDto dto, int usuarioId);
        Task<UsuarioDto> BuscarPorIdAsync(int usuarioId);
        Task<List<UsuarioDto>> ListarAsync();
        Task<(bool success, string title, string message)> ExcluirAsync(int usuarioId);
    }
}