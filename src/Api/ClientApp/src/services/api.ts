import axios from 'axios'
import type {
  Agendamento,
  AgendamentoResumo,
  ApiResponse,
  DashboardSemanal,
  HorariosDisponiveis,
  LoginResponse,
  Servico,
  SugestaoAgendamento,
  Usuario,
} from '../types'
import { normalizeKeys } from '../utils/normalize'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '',
  headers: { 'Content-Type': 'application/json' },
  paramsSerializer: (params) => {
    const search = new URLSearchParams()
    Object.entries(params).forEach(([key, value]) => {
      if (value === undefined || value === null) return
      if (Array.isArray(value)) {
        value.forEach((item) => search.append(key, String(item)))
      } else {
        search.append(key, String(value))
      }
    })
    return search.toString()
  },
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (response) => {
    if (response.data && typeof response.data === 'object') {
      response.data = normalizeKeys(response.data)
    }
    return response
  },
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('usuario')
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

export const authApi = {
  login: (email: string, senha: string) =>
    api.post<ApiResponse<LoginResponse>>('/Autenticacao/Login', { email, senha }),
  cadastrar: (data: { nome: string; email: string; senha: string; telefone: string }) =>
    api.post<ApiResponse>('/Usuario/Cadastrar', data),
}

export const usuarioApi = {
  buscarDados: () => api.get<ApiResponse<Usuario>>('/Usuario/BuscarDados'),
  listar: () => api.get<ApiResponse<Usuario[]>>('/Usuario/Listar'),
}

export const servicoApi = {
  listar: () => api.get<ApiResponse<Servico[]>>('/Servico/Listar'),
}

export const agendamentoApi = {
  criar: (dataHora: string, servicoIds: number[]) =>
    api.post<
      ApiResponse<{
        agendamento: Agendamento
        sugestao?: SugestaoAgendamento
      }>
    >('/Agendamento/Criar', { dataHora, servicoIds }),

  criarAdmin: (usuarioId: number, dataHora: string, servicoIds: number[]) =>
    api.post<
      ApiResponse<{
        agendamento: Agendamento
        sugestao?: SugestaoAgendamento
      }>
    >('/Agendamento/CriarAdmin', { usuarioId, dataHora, servicoIds }),

  editar: (agendamentoId: number, dataHora: string, servicoIds: number[]) =>
    api.put<ApiResponse>('/Agendamento/Editar', { agendamentoId, dataHora, servicoIds }),

  buscarPorId: (id: number) =>
    api.get<ApiResponse<Agendamento>>(`/Agendamento/BuscarPorId/${id}`),

  historico: (dataInicio: string, dataFim: string) =>
    api.get<ApiResponse<AgendamentoResumo[]>>('/Agendamento/Historico', {
      params: { dataInicio, dataFim },
    }),

  horariosDisponiveis: (data: string, servicoIds: number[], excluirAgendamentoId?: number) =>
    api.get<ApiResponse<HorariosDisponiveis>>('/Agendamento/HorariosDisponiveis', {
      params: { data, servicoIds, excluirAgendamentoId },
    }),

  sugestao: (dataHora: string) =>
    api.get<ApiResponse<SugestaoAgendamento>>('/Agendamento/Sugestao', { params: { dataHora } }),

  listarRecebidos: (params?: {
    dataInicio?: string
    dataFim?: string
    status?: number
    buscaCliente?: string
  }) =>
    api.get<ApiResponse<AgendamentoResumo[]>>('/Agendamento/ListarRecebidos', { params }),

  confirmar: (id: number) => api.put<ApiResponse>(`/Agendamento/Confirmar/${id}`),

  cancelar: (id: number) => api.put<ApiResponse>(`/Agendamento/Cancelar/${id}`),

  alterarStatusServico: (agendamentoServicoId: number, status: number) =>
    api.put<ApiResponse>('/Agendamento/AlterarStatusServico', { agendamentoServicoId, status }),
}

export const dashboardApi = {
  desempenhoSemanal: (dataInicio?: string, dataFim?: string) =>
    api.get<ApiResponse<DashboardSemanal>>('/Dashboard/DesempenhoSemanal', {
      params: { dataInicio, dataFim },
    }),
}

export default api
