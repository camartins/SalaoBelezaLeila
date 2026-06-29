import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import CheckIcon from '@mui/icons-material/Check'
import CancelIcon from '@mui/icons-material/Cancel'
import EditIcon from '@mui/icons-material/Edit'
import FilterListIcon from '@mui/icons-material/FilterList'
import { useCallback, useEffect, useState } from 'react'
import { toast } from 'react-toastify'
import Layout from '../../components/Layout'
import AgendamentoAdminDialog from '../../components/AgendamentoAdminDialog'
import type { AgendamentoResumo } from '../../types'
import { agendamentoApi } from '../../services/api'
import { formatCurrency, hojeParam, obterFimSemana, obterInicioSemana, toDateParam } from '../../utils/agendamento'

const statusOptions = [
  { value: '', label: 'Todos' },
  { value: 1, label: 'Pendente' },
  { value: 2, label: 'Confirmado' },
  { value: 3, label: 'Cancelado' },
  { value: 4, label: 'Concluído' },
]

const statusColor: Record<string, 'default' | 'warning' | 'success' | 'error' | 'info'> = {
  Pendente: 'warning',
  Confirmado: 'info',
  Cancelado: 'error',
  Concluido: 'success',
}

export default function AgendamentosAdminPage() {
  const [items, setItems] = useState<AgendamentoResumo[]>([])
  const [filtroStatus, setFiltroStatus] = useState<number | ''>('')
  const [dataInicio, setDataInicio] = useState('')
  const [dataFim, setDataFim] = useState('')
  const [buscaCliente, setBuscaCliente] = useState('')
  const [dialogAberto, setDialogAberto] = useState(false)
  const [modoDialog, setModoDialog] = useState<'criar' | 'editar'>('criar')
  const [editandoId, setEditandoId] = useState<number | undefined>()

  const carregar = useCallback(() => {
    const params: {
      dataInicio?: string
      dataFim?: string
      status?: number
      buscaCliente?: string
    } = {}

    if (dataInicio) params.dataInicio = dataInicio
    if (dataFim) params.dataFim = dataFim
    if (filtroStatus !== '') params.status = filtroStatus as number
    if (buscaCliente.trim()) params.buscaCliente = buscaCliente.trim()

    agendamentoApi.listarRecebidos(params).then(({ data }) => setItems(data.dados || []))
  }, [filtroStatus, dataInicio, dataFim, buscaCliente])

  useEffect(() => {
    const ini = obterInicioSemana()
    const fim = obterFimSemana(ini)
    setDataInicio(toDateParam(ini))
    setDataFim(toDateParam(fim))
  }, [])

  useEffect(() => {
    if (dataInicio && dataFim) carregar()
  }, [carregar, dataInicio, dataFim])

  const confirmar = async (id: number) => {
    try {
      await agendamentoApi.confirmar(id)
      toast.success('Agendamento confirmado!')
      carregar()
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensagem?: string } } })?.response?.data?.mensagem
      toast.error(msg || 'Erro ao confirmar')
    }
  }

  const cancelar = async (id: number) => {
    if (!window.confirm('Deseja cancelar este agendamento?')) return
    try {
      await agendamentoApi.cancelar(id)
      toast.success('Agendamento cancelado')
      carregar()
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensagem?: string } } })?.response?.data?.mensagem
      toast.error(msg || 'Erro ao cancelar')
    }
  }

  const abrirCriar = () => {
    setModoDialog('criar')
    setEditandoId(undefined)
    setDialogAberto(true)
  }

  const abrirEditar = (id: number) => {
    setModoDialog('editar')
    setEditandoId(id)
    setDialogAberto(true)
  }

  const limparFiltros = () => {
    setFiltroStatus('')
    setBuscaCliente('')
    const ini = obterInicioSemana()
    setDataInicio(toDateParam(ini))
    setDataFim(toDateParam(obterFimSemana(ini)))
  }

  const formatDate = (d: string) =>
    new Date(d).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

  const editavel = (status: string) => status !== 'Cancelado' && status !== 'Concluido'

  return (
    <Layout>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 3, flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h4" gutterBottom>
            Agendamentos
          </Typography>
          <Typography color="text.secondary">
            Gerencie, crie e edite agendamentos dos clientes
          </Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={abrirCriar}>
          Novo agendamento
        </Button>
      </Box>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
            <FilterListIcon fontSize="small" color="action" />
            <Typography variant="subtitle1">Filtros</Typography>
          </Box>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                type="date"
                label="Data início"
                size="small"
                fullWidth
                value={dataInicio}
                onChange={(e) => setDataInicio(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                type="date"
                label="Data fim"
                size="small"
                fullWidth
                value={dataFim}
                onChange={(e) => setDataFim(e.target.value)}
                slotProps={{ inputLabel: { shrink: true }, htmlInput: { min: dataInicio || hojeParam() } }}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <FormControl size="small" fullWidth>
                <InputLabel>Status</InputLabel>
                <Select
                  value={filtroStatus}
                  label="Status"
                  onChange={(e) => setFiltroStatus(e.target.value as number | '')}
                >
                  {statusOptions.map((o) => (
                    <MenuItem key={String(o.value)} value={o.value}>{o.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                label="Buscar cliente"
                size="small"
                fullWidth
                value={buscaCliente}
                onChange={(e) => setBuscaCliente(e.target.value)}
                placeholder="Nome do cliente"
              />
            </Grid>
            <Grid size={12}>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                <Button variant="contained" onClick={carregar}>Aplicar filtros</Button>
                <Button variant="outlined" onClick={limparFiltros}>Limpar</Button>
                <Chip label={`${items.length} resultado(s)`} size="small" sx={{ ml: 'auto' }} />
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Grid container spacing={2}>
        {items.length === 0 && (
          <Grid size={12}>
            <Typography color="text.secondary">Nenhum agendamento encontrado com os filtros aplicados.</Typography>
          </Grid>
        )}
        {items.map((item) => (
          <Grid key={item.id} size={{ xs: 12, md: 6 }}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography sx={{ fontWeight: 700 }}>{item.nomeCliente}</Typography>
                  <Chip label={item.status} size="small" color={statusColor[item.status] || 'default'} />
                </Box>
                <Typography variant="body2" color="text.secondary">
                  {formatDate(item.dataHora)} — {formatDate(item.dataHoraFim)}
                </Typography>
                <Typography variant="body2" sx={{ mt: 0.5 }}>
                  {item.quantidadeServicos} serviço(s) · {item.duracaoTotalMinutos} min · R$ {formatCurrency(item.valorTotal)}
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, mt: 2, flexWrap: 'wrap' }}>
                  {item.status === 'Pendente' && (
                    <>
                      <Button
                        size="small"
                        variant="contained"
                        startIcon={<CheckIcon />}
                        onClick={() => confirmar(item.id)}
                      >
                        Confirmar
                      </Button>
                      <Button
                        size="small"
                        variant="outlined"
                        color="error"
                        startIcon={<CancelIcon />}
                        onClick={() => cancelar(item.id)}
                      >
                        Cancelar
                      </Button>
                    </>
                  )}
                  {editavel(item.status) && (
                    <>
                      <Button
                        size="small"
                        variant="outlined"
                        startIcon={<EditIcon />}
                        onClick={() => abrirEditar(item.id)}
                      >
                        Editar
                      </Button>
                      {item.status !== 'Pendente' && (
                        <Button
                          size="small"
                          variant="outlined"
                          color="error"
                          startIcon={<CancelIcon />}
                          onClick={() => cancelar(item.id)}
                        >
                          Cancelar
                        </Button>
                      )}
                    </>
                  )}
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <AgendamentoAdminDialog
        open={dialogAberto}
        modo={modoDialog}
        agendamentoId={editandoId}
        onClose={() => setDialogAberto(false)}
        onSuccess={carregar}
      />
    </Layout>
  )
}
