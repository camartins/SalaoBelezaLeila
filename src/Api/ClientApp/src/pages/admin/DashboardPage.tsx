import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  TextField,
  Typography,
} from '@mui/material'
import AttachMoneyIcon from '@mui/icons-material/AttachMoney'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import EventIcon from '@mui/icons-material/Event'
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import ScheduleIcon from '@mui/icons-material/Schedule'
import CancelIcon from '@mui/icons-material/Cancel'
import { useCallback, useEffect, useState } from 'react'
import Layout from '../../components/Layout'
import MetricCard from '../../components/MetricCard'
import type { DashboardSemanal } from '../../types'
import { dashboardApi } from '../../services/api'
import { formatCurrency, obterFimSemana, obterInicioSemana, toDateParam } from '../../utils/agendamento'

type Preset = 'semana' | 'mes' | '30dias' | 'custom'

function aplicarPreset(preset: Preset): { inicio: string; fim: string } {
  const hoje = new Date()
  if (preset === 'semana') {
    const ini = obterInicioSemana(hoje)
    return { inicio: toDateParam(ini), fim: toDateParam(obterFimSemana(ini)) }
  }
  if (preset === 'mes') {
    const ini = new Date(hoje.getFullYear(), hoje.getMonth(), 1)
    const fim = new Date(hoje.getFullYear(), hoje.getMonth() + 1, 0)
    return { inicio: toDateParam(ini), fim: toDateParam(fim) }
  }
  if (preset === '30dias') {
    const ini = new Date(hoje)
    ini.setDate(hoje.getDate() - 29)
    return { inicio: toDateParam(ini), fim: toDateParam(hoje) }
  }
  const ini = obterInicioSemana(hoje)
  return { inicio: toDateParam(ini), fim: toDateParam(obterFimSemana(ini)) }
}

export default function DashboardPage() {
  const [dados, setDados] = useState<DashboardSemanal | null>(null)
  const [loading, setLoading] = useState(true)
  const [dataInicio, setDataInicio] = useState('')
  const [dataFim, setDataFim] = useState('')
  const [preset, setPreset] = useState<Preset>('semana')

  const carregar = useCallback((inicio: string, fim: string) => {
    setLoading(true)
    dashboardApi
      .desempenhoSemanal(inicio, fim)
      .then(({ data }) => setDados(data.dados || null))
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    const { inicio, fim } = aplicarPreset('semana')
    setDataInicio(inicio)
    setDataFim(fim)
    carregar(inicio, fim)
  }, [carregar])

  const aplicarFiltro = () => {
    if (!dataInicio || !dataFim) return
    setPreset('custom')
    carregar(dataInicio, dataFim)
  }

  const selecionarPreset = (p: Preset) => {
    const { inicio, fim } = aplicarPreset(p)
    setPreset(p)
    setDataInicio(inicio)
    setDataFim(fim)
    carregar(inicio, fim)
  }

  const formatPeriodo = () => {
    if (!dados) return ''
    const ini = new Date(dados.inicioSemana).toLocaleDateString('pt-BR')
    const fim = new Date(dados.fimSemana).toLocaleDateString('pt-BR')
    return `${ini} — ${fim}`
  }

  const maxQtd = dados
    ? Math.max(...dados.agendamentosPorDia.map((d) => d.quantidade), 1)
    : 1

  const maxFat = dados
    ? Math.max(...dados.agendamentosPorDia.map((d) => d.faturamento), 1)
    : 1

  const insightSeverity = (tipo: string): 'info' | 'warning' | 'success' | 'error' => {
    if (tipo === 'warning') return 'warning'
    if (tipo === 'success') return 'success'
    return 'info'
  }

  return (
    <Layout>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 2 }}>
        Análise de desempenho · {dados ? formatPeriodo() : '...'}
      </Typography>

      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
        {(['semana', 'mes', '30dias'] as Preset[]).map((p) => (
          <Chip
            key={p}
            label={p === 'semana' ? 'Esta semana' : p === 'mes' ? 'Este mês' : 'Últimos 30 dias'}
            clickable
            color={preset === p ? 'primary' : 'default'}
            onClick={() => selecionarPreset(p)}
            variant={preset === p ? 'filled' : 'outlined'}
          />
        ))}
      </Box>

      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, mb: 3, alignItems: 'flex-end' }}>
        <TextField
          type="date"
          label="Data início"
          size="small"
          value={dataInicio}
          onChange={(e) => setDataInicio(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          type="date"
          label="Data fim"
          size="small"
          value={dataFim}
          onChange={(e) => setDataFim(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <Button variant="contained" onClick={aplicarFiltro} disabled={!dataInicio || !dataFim}>
          Aplicar período
        </Button>
      </Box>

      {loading && <Typography color="text.secondary">Carregando...</Typography>}

      {!loading && dados && (
        <>
          <Grid container spacing={2} sx={{ mb: 3 }}>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
              <MetricCard title="Total" value={dados.totalAgendamentos} icon={<EventIcon />} color="#B8956C" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
              <MetricCard title="Pendentes" value={dados.pendentes} icon={<HourglassEmptyIcon />} color="#E8C4A0" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
              <MetricCard title="Confirmados" value={dados.confirmados} icon={<CheckCircleIcon />} color="#C99398" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
              <MetricCard title="Concluídos" value={dados.concluidos} icon={<CheckCircleIcon />} color="#A8C5A0" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
              <MetricCard
                title="Fatur. estimado"
                value={`R$ ${formatCurrency(dados.faturamentoEstimado)}`}
                icon={<AttachMoneyIcon />}
                color="#8B7355"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
              <MetricCard
                title="Fatur. realizado"
                value={`R$ ${formatCurrency(dados.faturamentoRealizado)}`}
                icon={<TrendingUpIcon />}
                color="#6B8E6B"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
              <MetricCard
                title="Ticket médio"
                value={`R$ ${formatCurrency(dados.ticketMedio)}`}
                icon={<AttachMoneyIcon />}
                color="#9A8570"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
              <MetricCard
                title="Taxa cancelamento"
                value={`${dados.taxaCancelamento}%`}
                icon={<CancelIcon />}
                color="#D4848A"
              />
            </Grid>
          </Grid>

          <Grid container spacing={2} sx={{ mb: 3 }}>
            <Grid size={{ xs: 12, md: 4 }}>
              <Card sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>Indicadores</Typography>
                  <Typography variant="body2" sx={{ mb: 1 }}>
                    <strong>Taxa de conclusão:</strong> {dados.taxaConclusao}%
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 1 }}>
                    <strong>Duração média:</strong> {dados.duracaoMediaMinutos} min
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 1 }}>
                    <strong>Horário de pico:</strong> {dados.horarioPico || '—'}
                  </Typography>
                  <Typography variant="body2">
                    <strong>Dia mais movimentado:</strong> {dados.diaMaisMovimentado || '—'}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid size={{ xs: 12, md: 8 }}>
              <Card sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>Top serviços</Typography>
                  {dados.topServicos.length === 0 && (
                    <Typography variant="body2" color="text.secondary">Sem dados no período</Typography>
                  )}
                  {dados.topServicos.map((s, i) => {
                    const max = dados.topServicos[0]?.quantidade || 1
                    return (
                      <Box key={s.nome} sx={{ mb: 1.5 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                          <Typography variant="body2">{i + 1}. {s.nome}</Typography>
                          <Typography variant="body2" color="text.secondary">
                            {s.quantidade}x · R$ {formatCurrency(s.faturamento)}
                          </Typography>
                        </Box>
                        <Box
                          sx={{
                            height: 8,
                            borderRadius: 1,
                            bgcolor: 'rgba(184,149,108,0.15)',
                            overflow: 'hidden',
                          }}
                        >
                          <Box
                            sx={{
                              width: `${(s.quantidade / max) * 100}%`,
                              height: '100%',
                              background: 'linear-gradient(90deg, #E8B4B8, #B8956C)',
                              borderRadius: 1,
                            }}
                          />
                        </Box>
                      </Box>
                    )
                  })}
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          {dados.insights.length > 0 && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mb: 3 }}>
              <Typography variant="h6">Insights para decisão</Typography>
              {dados.insights.map((insight, i) => (
                <Alert key={i} severity={insightSeverity(insight.tipo)} icon={<ScheduleIcon fontSize="inherit" />}>
                  {insight.mensagem}
                </Alert>
              ))}
            </Box>
          )}

          <Card sx={{ mb: 2 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Agendamentos por dia</Typography>
              <Box sx={{ display: 'flex', alignItems: 'flex-end', gap: 1, height: 220, pt: 2, overflowX: 'auto' }}>
                {dados.agendamentosPorDia.map((dia) => (
                  <Box key={dia.data} sx={{ minWidth: 48, flex: '1 1 0', textAlign: 'center' }}>
                    <Box sx={{ display: 'flex', gap: 0.5, justifyContent: 'center', alignItems: 'flex-end', height: 180 }}>
                      <Box
                        title={`${dia.quantidade} agendamentos`}
                        sx={{
                          width: 16,
                          height: `${(dia.quantidade / maxQtd) * 150}px`,
                          minHeight: dia.quantidade > 0 ? 6 : 2,
                          background: 'linear-gradient(180deg, #E8B4B8 0%, #B8956C 100%)',
                          borderRadius: '4px 4px 0 0',
                        }}
                      />
                      <Box
                        title={`R$ ${formatCurrency(dia.faturamento)}`}
                        sx={{
                          width: 16,
                          height: `${(dia.faturamento / maxFat) * 150}px`,
                          minHeight: dia.faturamento > 0 ? 6 : 2,
                          background: 'linear-gradient(180deg, #A8C5A0 0%, #6B8E6B 100%)',
                          borderRadius: '4px 4px 0 0',
                        }}
                      />
                    </Box>
                    <Typography variant="caption" sx={{ mt: 0.5, display: 'block', textTransform: 'capitalize' }}>
                      {new Date(dia.data).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {dia.quantidade}
                    </Typography>
                  </Box>
                ))}
              </Box>
              <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                  <Box sx={{ width: 12, height: 12, bgcolor: '#B8956C', borderRadius: 0.5 }} />
                  <Typography variant="caption">Qtd</Typography>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                  <Box sx={{ width: 12, height: 12, bgcolor: '#6B8E6B', borderRadius: 0.5 }} />
                  <Typography variant="caption">Faturamento</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>

          {dados.cancelados > 0 && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <CancelIcon sx={{ color: 'error.main', fontSize: 20 }} />
              <Typography variant="body2" color="text.secondary">
                {dados.cancelados} cancelamento(s) no período ({dados.taxaCancelamento}%)
              </Typography>
            </Box>
          )}
        </>
      )}
    </Layout>
  )
}
