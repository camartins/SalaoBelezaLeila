export function toDateParam(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

export function hojeParam(): string {
  return toDateParam(new Date())
}

export function obterInicioSemana(ref = new Date()): Date {
  const d = new Date(ref)
  const diff = (d.getDay() + 6) % 7
  d.setDate(d.getDate() - diff)
  d.setHours(0, 0, 0, 0)
  return d
}

export function obterFimSemana(inicio: Date): Date {
  const fim = new Date(inicio)
  fim.setDate(inicio.getDate() + 6)
  return fim
}

export function minDataAgendamento(): string {
  const d = new Date()
  d.setDate(d.getDate() + 2)
  return toDateParam(d)
}

export function podeAlterarAgendamento(dataHora: string): boolean {
  const data = new Date(dataHora)
  const hoje = new Date()
  hoje.setHours(0, 0, 0, 0)
  const diff = (data.setHours(0, 0, 0, 0) - hoje.getTime()) / (1000 * 60 * 60 * 24)
  return diff >= 2
}

export function agendamentoAtivo(status: string): boolean {
  return status !== 'Cancelado' && status !== 'Concluido'
}

export function formatCurrency(value: number | undefined | null): string {
  return Number(value ?? 0).toFixed(2)
}
