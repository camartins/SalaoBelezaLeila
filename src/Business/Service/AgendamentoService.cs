using Business.Configurations;
using Business.Helpers;
using Domain.Dto;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;

namespace Business.Services
{
    public class AgendamentoService : IAgendamentoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ModalTitleConfig _modalConfig;
        private readonly SalaoHorarioConfig _horarioConfig;

        public AgendamentoService(
            IUnitOfWork unitOfWork,
            ModalTitleConfig modalConfig,
            SalaoHorarioConfig horarioConfig)
        {
            _unitOfWork = unitOfWork;
            _modalConfig = modalConfig;
            _horarioConfig = horarioConfig;
        }

        public async Task<(bool success, string title, string message, CriarAgendamentoRespostaDto? data)> CriarAsync(
            CriarAgendamentoDto dto, int usuarioId)
        {
            try
            {
                var erro = dto.ValidarCamposObrigatorios();
                if (!string.IsNullOrEmpty(erro))
                    return (false, _modalConfig.Error, erro, null);

                var servicos = await ValidarServicosAsync(dto.ServicoIds);
                if (servicos == null)
                    return (false, _modalConfig.Error, "Um ou mais serviços informados são inválidos ou inativos.", null);

                var erroHorario = await ValidarConflitoHorarioAsync(dto.DataHora, servicos, null);
                if (erroHorario != null)
                    return (false, _modalConfig.Error, erroHorario, null);

                var erroExpediente = ValidarExpediente(dto.DataHora, servicos);
                if (erroExpediente != null)
                    return (false, _modalConfig.Error, erroExpediente, null);

                var agendamentosNaSemana = await _unitOfWork.AgendamentoRepository
                    .ListarPorUsuarioNaSemanaAsync(usuarioId, dto.DataHora);
                var sugestao = MontarSugestao(agendamentosNaSemana, dto.DataHora);

                var agendamento = new Agendamento
                {
                    UsuarioId = usuarioId,
                    DataHora = dto.DataHora,
                    Status = StatusAgendamento.Pendente,
                    Servicos = servicos.Select(s => new AgendamentoServico
                    {
                        ServicoId = s.Id,
                        Status = StatusServico.Pendente
                    }).ToList()
                };

                _unitOfWork.AgendamentoRepository.Save(agendamento);

                if (!await _unitOfWork.CommitAsync())
                    return (false, _modalConfig.Error, "Erro ao criar agendamento.", null);

                var agendamentoCriado = await _unitOfWork.AgendamentoRepository.GetCompletoByIdAsync(agendamento.Id);

                return (true, _modalConfig.GeneralSuccess, "Agendamento criado com sucesso.", new CriarAgendamentoRespostaDto
                {
                    Agendamento = MapearAgendamentoDto(agendamentoCriado!),
                    Sugestao = sugestao
                });
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message, null);
            }
        }

        public async Task<(bool success, string title, string message, CriarAgendamentoRespostaDto? data)> CriarPorAdminAsync(
            CriarAgendamentoAdminDto dto)
        {
            try
            {
                var erro = dto.ValidarCamposObrigatorios();
                if (!string.IsNullOrEmpty(erro))
                    return (false, _modalConfig.Error, erro, null);

                var cliente = await _unitOfWork.UsuarioRepository.GetById(dto.UsuarioId);
                if (cliente == null || !cliente.Ativo)
                    return (false, _modalConfig.Error, "Cliente não encontrado ou inativo.", null);

                var servicos = await ValidarServicosAsync(dto.ServicoIds);
                if (servicos == null)
                    return (false, _modalConfig.Error, "Um ou mais serviços informados são inválidos ou inativos.", null);

                var erroHorario = await ValidarConflitoHorarioAsync(dto.DataHora, servicos, null);
                if (erroHorario != null)
                    return (false, _modalConfig.Error, erroHorario, null);

                var erroExpediente = ValidarExpediente(dto.DataHora, servicos);
                if (erroExpediente != null)
                    return (false, _modalConfig.Error, erroExpediente, null);

                var agendamentosNaSemana = await _unitOfWork.AgendamentoRepository
                    .ListarPorUsuarioNaSemanaAsync(dto.UsuarioId, dto.DataHora);
                var sugestao = MontarSugestao(agendamentosNaSemana, dto.DataHora);

                var agendamento = new Agendamento
                {
                    UsuarioId = dto.UsuarioId,
                    DataHora = dto.DataHora,
                    Status = StatusAgendamento.Confirmado,
                    Servicos = servicos.Select(s => new AgendamentoServico
                    {
                        ServicoId = s.Id,
                        Status = StatusServico.Pendente
                    }).ToList()
                };

                _unitOfWork.AgendamentoRepository.Save(agendamento);

                if (!await _unitOfWork.CommitAsync())
                    return (false, _modalConfig.Error, "Erro ao criar agendamento.", null);

                var agendamentoCriado = await _unitOfWork.AgendamentoRepository.GetCompletoByIdAsync(agendamento.Id);

                return (true, _modalConfig.GeneralSuccess, "Agendamento criado com sucesso.", new CriarAgendamentoRespostaDto
                {
                    Agendamento = MapearAgendamentoDto(agendamentoCriado!),
                    Sugestao = sugestao
                });
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message, null);
            }
        }

        public async Task<(bool success, string title, string message)> EditarAsync(
            EditarAgendamentoDto dto, int usuarioId, bool isAdmin)
        {
            try
            {
                var erro = dto.ValidarCamposObrigatorios();
                if (!string.IsNullOrEmpty(erro))
                    return (false, _modalConfig.Error, erro);

                var agendamento = await _unitOfWork.AgendamentoRepository.GetCompletoByIdAsync(dto.AgendamentoId);
                if (agendamento == null)
                    return (false, _modalConfig.Error, "Agendamento não encontrado.");

                if (!isAdmin && agendamento.UsuarioId != usuarioId)
                    return (false, _modalConfig.Error, "Você não tem permissão para alterar este agendamento.");

                if (agendamento.Status is StatusAgendamento.Cancelado or StatusAgendamento.Concluido)
                    return (false, _modalConfig.Error, "Não é possível alterar um agendamento cancelado ou concluído.");

                if (!isAdmin && !PodeAlterarPeloSistema(agendamento.DataHora))
                    return (false, _modalConfig.Error,
                        "Alterações só podem ser feitas até 2 dias antes do atendimento. Entre em contato por telefone.");

                var servicos = await ValidarServicosAsync(dto.ServicoIds);
                if (servicos == null)
                    return (false, _modalConfig.Error, "Um ou mais serviços informados são inválidos ou inativos.");

                var erroHorario = await ValidarConflitoHorarioAsync(dto.DataHora, servicos, dto.AgendamentoId);
                if (erroHorario != null)
                    return (false, _modalConfig.Error, erroHorario);

                var erroExpediente = ValidarExpediente(dto.DataHora, servicos);
                if (erroExpediente != null)
                    return (false, _modalConfig.Error, erroExpediente);

                agendamento.DataHora = dto.DataHora;

                foreach (var item in agendamento.Servicos.ToList())
                    _unitOfWork.AgendamentoServicoRepository.Delete(item);

                foreach (var servico in servicos)
                {
                    _unitOfWork.AgendamentoServicoRepository.Save(new AgendamentoServico
                    {
                        AgendamentoId = agendamento.Id,
                        ServicoId = servico.Id,
                        Status = StatusServico.Pendente
                    });
                }

                _unitOfWork.AgendamentoRepository.Update(agendamento);

                if (!await _unitOfWork.CommitAsync())
                    return (false, _modalConfig.Error, "Erro ao alterar agendamento.");

                return (true, _modalConfig.GeneralSuccess, "Agendamento alterado com sucesso.");
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message);
            }
        }

        public async Task<(bool success, string title, string message, AgendamentoDto? data)> BuscarPorIdAsync(
            int agendamentoId, int usuarioId, bool isAdmin)
        {
            try
            {
                var agendamento = await _unitOfWork.AgendamentoRepository.GetCompletoByIdAsync(agendamentoId);

                if (agendamento == null)
                    return (false, _modalConfig.Error, "Agendamento não encontrado.", null);

                if (!isAdmin && agendamento.UsuarioId != usuarioId)
                    return (false, _modalConfig.Error, "Você não tem permissão para visualizar este agendamento.", null);

                return (true, _modalConfig.GeneralSuccess, string.Empty, MapearAgendamentoDto(agendamento));
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message, null);
            }
        }

        public async Task<(bool success, string title, string message, List<AgendamentoResumoDto>? data)> ListarHistoricoAsync(
            DateTime dataInicio, DateTime dataFim, int usuarioId, bool isAdmin)
        {
            try
            {
                if (dataInicio == default || dataFim == default)
                    return (false, _modalConfig.Error, "Informe o período de consulta.", null);

                if (dataFim < dataInicio)
                    return (false, _modalConfig.Error, "A data final deve ser maior ou igual à data inicial.", null);

                var inicio = dataInicio.Date;
                var fim = dataFim.Date.AddDays(1).AddTicks(-1);

                var agendamentos = await _unitOfWork.AgendamentoRepository.ListarHistoricoAsync(
                    inicio, fim, isAdmin ? null : usuarioId);

                return (true, _modalConfig.GeneralSuccess, string.Empty,
                    agendamentos.Select(MapearResumoDto).ToList());
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message, null);
            }
        }

        public async Task<(bool success, string title, string message, SugestaoAgendamentoDto? data)> ObterSugestaoAsync(
            DateTime dataHora, int usuarioId)
        {
            try
            {
                if (dataHora == default)
                    return (false, _modalConfig.Error, "Informe a data e hora.", null);

                var agendamentosNaSemana = await _unitOfWork.AgendamentoRepository
                    .ListarPorUsuarioNaSemanaAsync(usuarioId, dataHora);
                var sugestao = MontarSugestao(agendamentosNaSemana, dataHora);

                if (sugestao == null)
                {
                    return (true, _modalConfig.GeneralSuccess, "Nenhuma sugestão disponível.", new SugestaoAgendamentoDto
                    {
                        PossuiAgendamentoNaSemana = false
                    });
                }

                return (true, _modalConfig.GeneralSuccess, string.Empty, sugestao);
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message, null);
            }
        }

        public async Task<(bool success, string title, string message, HorariosDisponiveisDto? data)> ListarHorariosDisponiveisAsync(
            DateTime data, List<int> servicoIds, int? excluirAgendamentoId)
        {
            try
            {
                if (data == default)
                    return (false, _modalConfig.Error, "Informe a data.", null);

                if (servicoIds == null || servicoIds.Count == 0)
                    return (false, _modalConfig.Error, "Informe ao menos um serviço.", null);

                var servicos = await ValidarServicosAsync(servicoIds);
                if (servicos == null)
                    return (false, _modalConfig.Error, "Um ou mais serviços informados são inválidos ou inativos.", null);

                var duracao = AgendamentoHorarioHelper.CalcularDuracaoTotalMinutos(servicos);
                var abertura = ObterHoraAbertura(data);
                var fechamento = ObterHoraFechamento(data);
                var agendamentosDia = await _unitOfWork.AgendamentoRepository
                    .ListarAtivosNoDiaAsync(data.Date, excluirAgendamentoId);

                var horarios = new List<string>();
                var candidato = abertura;

                while (candidato.AddMinutes(duracao) <= fechamento)
                {
                    var fim = AgendamentoHorarioHelper.CalcularDataHoraFim(candidato, duracao);
                    var conflito = PossuiConflito(candidato, fim, agendamentosDia);

                    if (!conflito && candidato > DateTime.Now)
                        horarios.Add(candidato.ToString("HH:mm"));

                    candidato = candidato.AddMinutes(_horarioConfig.IntervaloMinutos);
                }

                return (true, _modalConfig.GeneralSuccess, string.Empty, new HorariosDisponiveisDto
                {
                    Data = data.Date,
                    DuracaoTotalMinutos = duracao,
                    HorariosDisponiveis = horarios
                });
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message, null);
            }
        }

        public async Task<(bool success, string title, string message, List<AgendamentoResumoDto>? data)> ListarRecebidosAsync(
            DateTime? dataInicio, DateTime? dataFim, StatusAgendamento? status, string? buscaCliente)
        {
            try
            {
                var agendamentos = await _unitOfWork.AgendamentoRepository.ListarRecebidosAsync(
                    dataInicio, dataFim, status.HasValue ? (int)status.Value : null, buscaCliente);

                return (true, _modalConfig.GeneralSuccess, string.Empty,
                    agendamentos.Select(MapearResumoDto).ToList());
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message, null);
            }
        }

        public async Task<(bool success, string title, string message)> ConfirmarAsync(int agendamentoId)
        {
            try
            {
                var agendamento = await _unitOfWork.AgendamentoRepository.GetCompletoByIdAsync(agendamentoId);
                if (agendamento == null)
                    return (false, _modalConfig.Error, "Agendamento não encontrado.");

                if (agendamento.Status != StatusAgendamento.Pendente)
                    return (false, _modalConfig.Error, "Somente agendamentos pendentes podem ser confirmados.");

                agendamento.Status = StatusAgendamento.Confirmado;
                _unitOfWork.AgendamentoRepository.Update(agendamento);

                if (!await _unitOfWork.CommitAsync())
                    return (false, _modalConfig.Error, "Erro ao confirmar agendamento.");

                return (true, _modalConfig.GeneralSuccess, "Agendamento confirmado com sucesso.");
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message);
            }
        }

        public async Task<(bool success, string title, string message)> CancelarAsync(
            int agendamentoId, int usuarioId, bool isAdmin)
        {
            try
            {
                var agendamento = await _unitOfWork.AgendamentoRepository.GetCompletoByIdAsync(agendamentoId);
                if (agendamento == null)
                    return (false, _modalConfig.Error, "Agendamento não encontrado.");

                if (!isAdmin && agendamento.UsuarioId != usuarioId)
                    return (false, _modalConfig.Error, "Você não tem permissão para cancelar este agendamento.");

                if (!isAdmin && !PodeAlterarPeloSistema(agendamento.DataHora))
                    return (false, _modalConfig.Error,
                        "Cancelamentos só podem ser feitos até 2 dias antes do atendimento. Entre em contato por telefone.");

                if (agendamento.Status == StatusAgendamento.Cancelado)
                    return (false, _modalConfig.Error, "Agendamento já está cancelado.");

                if (agendamento.Status == StatusAgendamento.Concluido)
                    return (false, _modalConfig.Error, "Não é possível cancelar um agendamento concluído.");

                agendamento.Status = StatusAgendamento.Cancelado;

                foreach (var item in agendamento.Servicos.Where(s => s.Status != StatusServico.Concluido))
                    item.Status = StatusServico.Cancelado;

                _unitOfWork.AgendamentoRepository.Update(agendamento);

                if (!await _unitOfWork.CommitAsync())
                    return (false, _modalConfig.Error, "Erro ao cancelar agendamento.");

                return (true, _modalConfig.GeneralSuccess, "Agendamento cancelado com sucesso.");
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message);
            }
        }

        public async Task<(bool success, string title, string message)> AlterarStatusServicoAsync(AlterarStatusServicoDto dto)
        {
            try
            {
                var erro = dto.ValidarCamposObrigatorios();
                if (!string.IsNullOrEmpty(erro))
                    return (false, _modalConfig.Error, erro);

                var item = await _unitOfWork.AgendamentoRepository.GetAgendamentoServicoCompletoAsync(dto.AgendamentoServicoId);
                if (item == null)
                    return (false, _modalConfig.Error, "Serviço do agendamento não encontrado.");

                if (item.Agendamento.Status == StatusAgendamento.Cancelado)
                    return (false, _modalConfig.Error, "Não é possível alterar serviço de agendamento cancelado.");

                item.Status = dto.Status;
                _unitOfWork.AgendamentoServicoRepository.Update(item);

                await AtualizarStatusAgendamentoPorServicosAsync(item.AgendamentoId);

                if (!await _unitOfWork.CommitAsync())
                    return (false, _modalConfig.Error, "Erro ao alterar status do serviço.");

                return (true, _modalConfig.GeneralSuccess, "Status do serviço alterado com sucesso.");
            }
            catch (Exception ex)
            {
                return (false, _modalConfig.Error, ex.Message);
            }
        }

        private async Task AtualizarStatusAgendamentoPorServicosAsync(int agendamentoId)
        {
            var agendamento = await _unitOfWork.AgendamentoRepository.GetCompletoByIdAsync(agendamentoId);
            if (agendamento == null) return;

            if (agendamento.Servicos.All(s => s.Status == StatusServico.Concluido))
                agendamento.Status = StatusAgendamento.Concluido;
            else if (agendamento.Servicos.All(s => s.Status == StatusServico.Cancelado))
                agendamento.Status = StatusAgendamento.Cancelado;
            else if (agendamento.Servicos.Any(s => s.Status == StatusServico.EmAndamento))
                agendamento.Status = StatusAgendamento.Confirmado;

            _unitOfWork.AgendamentoRepository.Update(agendamento);
        }

        private async Task<string?> ValidarConflitoHorarioAsync(
            DateTime dataHora, List<Servico> servicos, int? excluirAgendamentoId)
        {
            var duracao = AgendamentoHorarioHelper.CalcularDuracaoTotalMinutos(servicos);
            var fim = AgendamentoHorarioHelper.CalcularDataHoraFim(dataHora, duracao);

            var agendamentosDia = await _unitOfWork.AgendamentoRepository
                .ListarAtivosNoDiaAsync(dataHora.Date, excluirAgendamentoId);

            foreach (var existente in agendamentosDia)
            {
                var duracaoExistente = AgendamentoHorarioHelper.CalcularDuracaoTotalMinutos(existente.Servicos);
                var fimExistente = AgendamentoHorarioHelper.CalcularDataHoraFim(existente.DataHora, duracaoExistente);

                if (AgendamentoHorarioHelper.HorariosConflitam(dataHora, fim, existente.DataHora, fimExistente))
                {
                    return $"Horário indisponível. Conflito com agendamento das {existente.DataHora:HH:mm} às {fimExistente:HH:mm}. " +
                           $"Próximo horário livre após {fimExistente:HH:mm}.";
                }
            }

            return null;
        }

        private string? ValidarExpediente(DateTime dataHora, List<Servico> servicos)
        {
            var duracao = AgendamentoHorarioHelper.CalcularDuracaoTotalMinutos(servicos);
            var fim = AgendamentoHorarioHelper.CalcularDataHoraFim(dataHora, duracao);
            var abertura = ObterHoraAbertura(dataHora.Date);
            var fechamento = ObterHoraFechamento(dataHora.Date);

            if (dataHora < abertura || fim > fechamento)
                return $"Agendamento fora do horário de funcionamento ({abertura:HH:mm} às {fechamento:HH:mm}).";

            return null;
        }

        private DateTime ObterHoraAbertura(DateTime data)
        {
            var hora = TimeSpan.Parse(_horarioConfig.HoraAbertura);
            return data.Date.Add(hora);
        }

        private DateTime ObterHoraFechamento(DateTime data)
        {
            var hora = TimeSpan.Parse(_horarioConfig.HoraFechamento);
            return data.Date.Add(hora);
        }

        private static bool PossuiConflito(DateTime inicio, DateTime fim, List<Agendamento> agendamentos)
        {
            foreach (var existente in agendamentos)
            {
                var duracao = AgendamentoHorarioHelper.CalcularDuracaoTotalMinutos(existente.Servicos);
                var fimExistente = AgendamentoHorarioHelper.CalcularDataHoraFim(existente.DataHora, duracao);

                if (AgendamentoHorarioHelper.HorariosConflitam(inicio, fim, existente.DataHora, fimExistente))
                    return true;
            }

            return false;
        }

        private async Task<List<Servico>?> ValidarServicosAsync(List<int> servicoIds)
        {
            var ids = servicoIds.Distinct().ToList();
            var servicos = (await _unitOfWork.ServicoRepository
                .CustomFind(s => ids.Contains(s.Id) && s.Ativo))
                .ToList();

            return servicos.Count == ids.Count ? servicos : null;
        }

        private static SugestaoAgendamentoDto? MontarSugestao(List<Agendamento> agendamentosNaSemana, DateTime dataProposta)
        {
            if (agendamentosNaSemana.Count == 0)
                return null;

            var primeiro = agendamentosNaSemana.First();

            if (primeiro.DataHora.Date == dataProposta.Date)
                return null;

            return new SugestaoAgendamentoDto
            {
                PossuiAgendamentoNaSemana = true,
                DataSugerida = primeiro.DataHora,
                Mensagem = $"Você já possui um agendamento nesta semana. Sugerimos marcar os novos serviços para {primeiro.DataHora:dd/MM/yyyy HH:mm}, mesma data do seu primeiro agendamento."
            };
        }

        private static bool PodeAlterarPeloSistema(DateTime dataHoraAgendamento)
            => (dataHoraAgendamento.Date - DateTime.Today).TotalDays >= 2;

        private static AgendamentoResumoDto MapearResumoDto(Agendamento agendamento)
        {
            var duracao = AgendamentoHorarioHelper.CalcularDuracaoTotalMinutos(agendamento.Servicos);

            return new AgendamentoResumoDto
            {
                Id = agendamento.Id,
                UsuarioId = agendamento.UsuarioId,
                NomeCliente = agendamento.Usuario?.Nome ?? string.Empty,
                DataHora = agendamento.DataHora,
                DataHoraFim = AgendamentoHorarioHelper.CalcularDataHoraFim(agendamento.DataHora, duracao),
                Status = agendamento.Status.ToString(),
                QuantidadeServicos = agendamento.Servicos.Count,
                DuracaoTotalMinutos = duracao,
                ValorTotal = agendamento.Servicos.Sum(s => s.Servico?.Valor ?? 0)
            };
        }

        private static AgendamentoDto MapearAgendamentoDto(Agendamento agendamento)
        {
            var servicos = agendamento.Servicos.Select(s => new AgendamentoServicoDto
            {
                Id = s.Id,
                ServicoId = s.ServicoId,
                NomeServico = s.Servico?.Nome ?? string.Empty,
                Valor = s.Servico?.Valor ?? 0,
                DuracaoMinutos = s.Servico?.DuracaoMinutos ?? 0,
                Status = s.Status.ToString()
            }).ToList();

            var duracao = servicos.Sum(s => s.DuracaoMinutos);

            return new AgendamentoDto
            {
                Id = agendamento.Id,
                UsuarioId = agendamento.UsuarioId,
                NomeCliente = agendamento.Usuario?.Nome ?? string.Empty,
                DataHora = agendamento.DataHora,
                DataHoraFim = AgendamentoHorarioHelper.CalcularDataHoraFim(agendamento.DataHora, duracao),
                Status = agendamento.Status.ToString(),
                ValorTotal = servicos.Sum(s => s.Valor),
                DuracaoTotalMinutos = duracao,
                Servicos = servicos,
                CreatedAt = agendamento.CreatedAt
            };
        }
    }
}
