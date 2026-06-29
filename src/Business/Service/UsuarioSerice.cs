using AutoMapper;
using Business.Configurations;
using Business.Helpers;
using Domain.Dto;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;

namespace Business.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ModalTitleConfig _modalConfig;

        public UsuarioService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ModalTitleConfig modalConfig)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _modalConfig = modalConfig;
        }

        public async Task<(bool success, string title, string message)> CadastrarAsync(CadastrarUsuarioDto dto)
        {
            try
            {
                var erroValidacao = dto.ValidarCamposObrigatorios();

                if (!string.IsNullOrEmpty(erroValidacao))
                    return (false, _modalConfig.Error, erroValidacao);

                var usuarioExistente = (await _unitOfWork.UsuarioRepository.CustomFind(x => x.Email == dto.Email)).FirstOrDefault();

                if (usuarioExistente != null)
                    return (false, _modalConfig.Error, "Já existe um usuário cadastrado com este e-mail.");

                var usuario = _mapper.Map<Usuario>(dto);
                usuario.Senha = PasswordHelper.Hash(dto.Senha);
                usuario.PerfilId = (int)PerfilEnum.Cliente;
                usuario.Ativo = true;

                _unitOfWork.UsuarioRepository.Save(usuario);

                if (!await _unitOfWork.CommitAsync())
                    return (false, _modalConfig.Error, "Erro ao cadastrar usuário.");

                return (true, _modalConfig.GeneralSuccess, "Usuário cadastrado com sucesso.");
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message);
            }
        }

        public async Task<(bool success, string title, string message)> EditarAsync(EditarUsuarioDto dto, int usuarioId)
        {
            try
            {
                var usuario = await _unitOfWork.UsuarioRepository.GetById(usuarioId);

                if (usuario == null)
                    return (false, _modalConfig.Error, "Usuário não encontrado.");

                var emailExistente = (await _unitOfWork.UsuarioRepository
                    .CustomFind(x => x.Email == dto.Email && x.Id != usuarioId))
                    .Any();

                if (emailExistente)
                    return (false, _modalConfig.Error, "E-mail já utilizado.");

                usuario.Nome = dto.Nome;
                usuario.Email = dto.Email;
                usuario.Telefone = dto.Telefone;
                usuario.PerfilId = dto.PerfilId;
                usuario.Ativo = dto.Ativo;

                _unitOfWork.UsuarioRepository.Update(usuario);

                if (!await _unitOfWork.CommitAsync())
                    return (false, _modalConfig.Error, "Erro ao editar usuário.");

                return (true, _modalConfig.GeneralSuccess, "Usuário atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message);
            }
        }

        public async Task<UsuarioDto> BuscarPorIdAsync(int usuarioId)
        {
            var usuario = await _unitOfWork.UsuarioRepository.GetById(usuarioId);

            if (usuario == null)
                return null;

            return _mapper.Map<UsuarioDto>(usuario);
        }

        public async Task<List<UsuarioDto>> ListarAsync()
        {
            var usuarios = await _unitOfWork.UsuarioRepository.GetAll();
            return _mapper.Map<List<UsuarioDto>>(usuarios);
        }

        public async Task<(bool success, string title, string message)> ExcluirAsync(int usuarioId)
        {
            try
            {
                var usuario = await _unitOfWork.UsuarioRepository.GetById(usuarioId);

                if (usuario == null)
                    return (false, _modalConfig.Error, "Usuário não encontrado.");

                usuario.Ativo = false;

                _unitOfWork.UsuarioRepository.Update(usuario);

                if (!await _unitOfWork.CommitAsync())
                    return (false, _modalConfig.Error, "Erro ao excluir usuário.");

                return (true, _modalConfig.GeneralSuccess, "Usuário excluído com sucesso.");
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message);
            }
        }
    }
}
