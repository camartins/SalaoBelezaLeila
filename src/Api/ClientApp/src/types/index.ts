export interface Usuario {
  id: number
  nome: string
  email: string
  telefone: string
  perfilId: number
  perfil: string
  ativo: boolean
}

export interface LoginResponse {
  token: string
  expiracao: string
  usuario: Usuario
}

export interface Servico {
  id: number
  nome: string
  valor: number
  duracaoMinutos: number
}

export interface AgendamentoServico {
  id: number
  servicoId: number
  nomeServico: string
  valor: number
  duracaoMinutos: number
  status: string
}

export interface Agendamento {
  id: number
  usuarioId: number
  nomeCliente: string
  dataHora: string
  dataHoraFim: string
  status: string
  valorTotal: number
  duracaoTotalMinutos: number
  servicos: AgendamentoServico[]
  createdAt: string
}

export interface AgendamentoResumo {
  id: number
  usuarioId: number
  nomeCliente: string
  dataHora: string
  dataHoraFim: string
  status: string
  quantidadeServicos: number
  duracaoTotalMinutos: number
  valorTotal: number
}

export interface SugestaoAgendamento {
  possuiAgendamentoNaSemana: boolean
  dataSugerida?: string
  mensagem?: string
}

export interface HorariosDisponiveis {
  data: string
  duracaoTotalMinutos: number
  horariosDisponiveis: string[]
}

export interface DesempenhoDiario {
  dia: string
  data: string
  quantidade: number
  faturamento: number
}

export interface ServicoRanking {
  nome: string
  quantidade: number
  faturamento: number
}

export interface DashboardInsight {
  tipo: 'info' | 'warning' | 'success'
  mensagem: string
}

export interface DashboardSemanal {
  inicioSemana: string
  fimSemana: string
  totalAgendamentos: number
  pendentes: number
  confirmados: number
  cancelados: number
  concluidos: number
  faturamentoEstimado: number
  faturamentoRealizado: number
  ticketMedio: number
  taxaCancelamento: number
  taxaConclusao: number
  duracaoMediaMinutos: number
  horarioPico: string
  diaMaisMovimentado: string
  agendamentosPorDia: DesempenhoDiario[]
  topServicos: ServicoRanking[]
  insights: DashboardInsight[]
}

export interface ApiResponse<T = unknown> {
  titulo?: string
  mensagem?: string
  dados?: T
}
