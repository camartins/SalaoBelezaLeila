import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  Grid,
  IconButton,
  Typography,
} from '@mui/material'
import CloseIcon from '@mui/icons-material/Close'
import EditIcon from '@mui/icons-material/Edit'
import CancelIcon from '@mui/icons-material/Cancel'
import VisibilityIcon from '@mui/icons-material/Visibility'
import { useCallback, useEffect, useState } from 'react'
import { toast } from 'react-toastify'
import Layout from '../components/Layout'
import type { Agendamento, AgendamentoResumo, Servico } from '../types'
import { agendamentoApi, servicoApi } from '../services/api'
import {
  agendamentoAtivo,
  formatCurrency,
  minDataAgendamento,
  podeAlterarAgendamento,
  toDateParam,
} from '../utils/agendamento'

const statusColor: Record<string, 'default' | 'warning' | 'success' | 'error' | 'info'> = {
  Pendente: 'warning',
  Confirmado: 'info',
  Cancelado: 'error',
  Concluido: 'success',
}

export default function HistoricoPage() {
  const [items, setItems] = useState<AgendamentoResumo[]>([])
  const [loading, setLoading] = useState(true)
  const [erro, setErro] = useState('')
  const [detalhe, setDetalhe] = useState<Agendamento | null>(null)
  const [editando, setEditando] = useState<Agendamento | null>(null)
  const [servicos, setServicos] = useState<Servico[]>([])
  const [selecionados, setSelecionados] = useState<number[]>([])
  const [data, setData] = useState('')
  const [horario, setHorario] = useState('')
  const [horarios, setHorarios] = useState<string[]>([])
  const [salvando, setSalvando] = useState(false)

  const carregar = useCallback(() => {
    setLoading(true)
    setErro('')
    const inicio = new Date()
    inicio.setMonth(inicio.getMonth() - 3)
    const fim = new Date()
    fim.setMonth(fim.getMonth() + 12)

    agendamentoApi
      .historico(toDateParam(inicio), toDateParam(fim))
      .then(({ data }) => setItems(data.dados || []))
      .catch((err: unknown) => {
        const msg = (err as { response?: { data?: { mensagem?: string } } })?.response?.data?.mensagem
        setErro(msg || 'Erro ao carregar agendamentos')
      })
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    carregar()
    servicoApi.listar().then(({ data }) => setServicos(data.dados || []))
  }, [carregar])

  useEffect(() => {
    if (!editando || !data || selecionados.length === 0) {
      setHorarios([])
      return
    }
    agendamentoApi
      .horariosDisponiveis(data, selecionados, editando.id)
      .then(({ data: res }) => setHorarios(res.dados?.horariosDisponiveis || []))
      .catch(() => setHorarios([]))
  }, [editando, data, selecionados])

  const verDetalhe = async (id: number) => {
    try {
      const { data } = await agendamentoApi.buscarPorId(id)
      setDetalhe(data.dados || null)
    } catch {
      toast.error('Erro ao carregar detalhes')
    }
  }

  const abrirEdicao = async (id: number) => {
    try {
      const { data } = await agendamentoApi.buscarPorId(id)
      const ag = data.dados
      if (!ag) return

      if (!podeAlterarAgendamento(ag.dataHora)) {
        toast.warning('Alterações só podem ser feitas até 2 dias antes do atendimento.')
        return
      }

      const dt = new Date(ag.dataHora)
      setEditando(ag)
      setSelecionados(ag.servicos.map((s) => s.servicoId))
      setData(toDateParam(dt))
      setHorario(dt.toTimeString().slice(0, 5))
    } catch {
      toast.error('Erro ao abrir edição')
    }
  }

  const fecharEdicao = () => {
    setEditando(null)
    setSelecionados([])
    setData('')
    setHorario('')
    setHorarios([])
  }

  const toggleServico = (id: number) => {
    setSelecionados((prev) =>
      prev.includes(id) ? prev.filter((s) => s !== id) : [...prev, id]
    )
    setHorario('')
  }

  const salvarEdicao = async () => {
    if (!editando || !data || !horario || selecionados.length === 0) {
      toast.warning('Selecione serviços, data e horário')
      return
    }
    setSalvando(true)
    try {
      const dataHora = `${data}T${horario}:00`
      const { data: res } = await agendamentoApi.editar(editando.id, dataHora, selecionados)
      toast.success(res.mensagem || 'Agendamento alterado!')
      fecharEdicao()
      carregar()
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensagem?: string } } })?.response?.data?.mensagem
      toast.error(msg || 'Erro ao alterar agendamento')
    } finally {
      setSalvando(false)
    }
  }

  const cancelarAgendamento = async (item: AgendamentoResumo) => {
    if (!podeAlterarAgendamento(item.dataHora)) {
      toast.warning('Cancelamentos só podem ser feitos até 2 dias antes do atendimento.')
      return
    }
    if (!window.confirm('Deseja realmente cancelar este agendamento?')) return

    try {
      const { data: res } = await agendamentoApi.cancelar(item.id)
      toast.success(res.mensagem || 'Agendamento cancelado')
      carregar()
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensagem?: string } } })?.response?.data?.mensagem
      toast.error(msg || 'Erro ao cancelar')
    }
  }

  const formatDate = (d: string) =>
    new Date(d).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

  return (
    <Layout>
      <Typography variant="h4" gutterBottom>
        Meus Agendamentos
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        Todos os seus agendamentos — passados, futuros e pendentes
      </Typography>

      {erro && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {erro}
        </Alert>
      )}

      {loading && (
        <Typography color="text.secondary" sx={{ mb: 2 }}>
          Carregando...
        </Typography>
      )}

      <Grid container spacing={2}>
        {!loading && items.length === 0 && (
          <Grid size={12}>
            <Typography color="text.secondary">Nenhum agendamento encontrado.</Typography>
          </Grid>
        )}
        {items.map((item) => {
          const editavel = agendamentoAtivo(item.status) && podeAlterarAgendamento(item.dataHora)
          return (
            <Grid key={item.id} size={{ xs: 12, sm: 6, md: 4 }}>
              <Card>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                    <Chip label={item.status} color={statusColor[item.status] || 'default'} size="small" />
                    <Box>
                      <IconButton size="small" onClick={() => verDetalhe(item.id)} title="Ver detalhes">
                        <VisibilityIcon fontSize="small" />
                      </IconButton>
                      {editavel && (
                        <>
                          <IconButton size="small" onClick={() => abrirEdicao(item.id)} title="Editar">
                            <EditIcon fontSize="small" />
                          </IconButton>
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => cancelarAgendamento(item)}
                            title="Cancelar"
                          >
                            <CancelIcon fontSize="small" />
                          </IconButton>
                        </>
                      )}
                    </Box>
                  </Box>
                  <Typography sx={{ fontWeight: 600 }}>{formatDate(item.dataHora)}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {item.quantidadeServicos} serviço(s) · {item.duracaoTotalMinutos} min
                  </Typography>
                  <Typography variant="body2" sx={{ mt: 1, fontWeight: 600 }}>
                    R$ {formatCurrency(item.valorTotal)}
                  </Typography>
                  {!editavel && agendamentoAtivo(item.status) && (
                    <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                      Edição/cancelamento indisponível (menos de 2 dias)
                    </Typography>
                  )}
                </CardContent>
              </Card>
            </Grid>
          )
        })}
      </Grid>

      <Dialog open={!!detalhe} onClose={() => setDetalhe(null)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          Detalhes do agendamento
          <IconButton onClick={() => setDetalhe(null)}><CloseIcon /></IconButton>
        </DialogTitle>
        <DialogContent>
          {detalhe && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
              <Typography><strong>Data:</strong> {formatDate(detalhe.dataHora)}</Typography>
              <Typography><strong>Término:</strong> {formatDate(detalhe.dataHoraFim)}</Typography>
              <Typography><strong>Status:</strong> {detalhe.status}</Typography>
              <Typography sx={{ fontWeight: 600, mt: 1 }}>Serviços:</Typography>
              {detalhe.servicos.map((s) => (
                <Box key={s.id} sx={{ pl: 1, borderLeft: '3px solid', borderColor: 'secondary.light' }}>
                  <Typography>{s.nomeServico}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {s.duracaoMinutos} min · R$ {formatCurrency(s.valor)} · {s.status}
                  </Typography>
                </Box>
              ))}
              <Typography sx={{ fontWeight: 700, mt: 1 }}>
                Total: R$ {formatCurrency(detalhe.valorTotal)}
              </Typography>
            </Box>
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={!!editando} onClose={fecharEdicao} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          Editar agendamento
          <IconButton onClick={fecharEdicao}><CloseIcon /></IconButton>
        </DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
          <Typography variant="subtitle2">Serviços</Typography>
          {servicos.map((s) => (
            <FormControlLabel
              key={s.id}
              control={
                <Checkbox
                  checked={selecionados.includes(s.id)}
                  onChange={() => toggleServico(s.id)}
                />
              }
              label={`${s.nome} (${s.duracaoMinutos} min · R$ ${formatCurrency(s.valor)})`}
            />
          ))}
          <Typography variant="subtitle2">Nova data</Typography>
          <input
            type="date"
            value={data}
            min={minDataAgendamento()}
            onChange={(e) => { setData(e.target.value); setHorario('') }}
            style={{ padding: '10px', borderRadius: 8, border: '1px solid #ccc' }}
          />
          <Typography variant="subtitle2">Horário</Typography>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
            {horarios.map((h) => (
              <Chip
                key={h}
                label={h}
                clickable
                color={horario === h ? 'primary' : 'default'}
                onClick={() => setHorario(h)}
                variant={horario === h ? 'filled' : 'outlined'}
              />
            ))}
            {horarios.length === 0 && data && selecionados.length > 0 && (
              <Typography variant="body2" color="text.secondary">
                Nenhum horário disponível nesta data
              </Typography>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={fecharEdicao}>Voltar</Button>
          <Button variant="contained" onClick={salvarEdicao} disabled={salvando || !horario}>
            {salvando ? 'Salvando...' : 'Salvar alterações'}
          </Button>
        </DialogActions>
      </Dialog>
    </Layout>
  )
}
