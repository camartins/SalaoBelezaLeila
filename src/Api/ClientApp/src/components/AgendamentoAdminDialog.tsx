import {
  Alert,
  Box,
  Button,
  Checkbox,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { toast } from 'react-toastify'
import type { Agendamento, Servico, Usuario } from '../types'
import { agendamentoApi, servicoApi, usuarioApi } from '../services/api'
import { formatCurrency, hojeParam, toDateParam } from '../utils/agendamento'

type Modo = 'criar' | 'editar'

interface Props {
  open: boolean
  modo: Modo
  agendamentoId?: number
  onClose: () => void
  onSuccess: () => void
}

export default function AgendamentoAdminDialog({
  open,
  modo,
  agendamentoId,
  onClose,
  onSuccess,
}: Props) {
  const [clientes, setClientes] = useState<Usuario[]>([])
  const [servicos, setServicos] = useState<Servico[]>([])
  const [clienteId, setClienteId] = useState<number | ''>('')
  const [selecionados, setSelecionados] = useState<number[]>([])
  const [data, setData] = useState('')
  const [horario, setHorario] = useState('')
  const [horarios, setHorarios] = useState<string[]>([])
  const [salvando, setSalvando] = useState(false)
  const [carregando, setCarregando] = useState(false)

  useEffect(() => {
    if (!open) return
    servicoApi.listar().then(({ data: res }) => setServicos(res.dados || []))
    usuarioApi.listar().then(({ data: res }) => {
      const lista = (res.dados || []).filter((u) => u.ativo && u.perfil !== 'Admin')
      setClientes(lista)
    })
  }, [open])

  useEffect(() => {
    if (!open || modo !== 'editar' || !agendamentoId) return
    setCarregando(true)
    agendamentoApi
      .buscarPorId(agendamentoId)
      .then(({ data: res }) => {
        const ag = res.dados as Agendamento | undefined
        if (!ag) return
        setClienteId(ag.usuarioId)
        setSelecionados(ag.servicos.map((s) => s.servicoId))
        const dt = new Date(ag.dataHora)
        setData(toDateParam(dt))
        setHorario(dt.toTimeString().slice(0, 5))
      })
      .catch(() => toast.error('Erro ao carregar agendamento'))
      .finally(() => setCarregando(false))
  }, [open, modo, agendamentoId])

  useEffect(() => {
    if (!open || modo === 'criar') {
      if (open && modo === 'criar') {
        setClienteId('')
        setSelecionados([])
        setData('')
        setHorario('')
      }
    }
  }, [open, modo])

  useEffect(() => {
    if (!data || selecionados.length === 0) {
      setHorarios([])
      return
    }
    agendamentoApi
      .horariosDisponiveis(data, selecionados, modo === 'editar' ? agendamentoId : undefined)
      .then(({ data: res }) => setHorarios(res.dados?.horariosDisponiveis || []))
      .catch(() => setHorarios([]))
  }, [data, selecionados, modo, agendamentoId])

  const toggleServico = (id: number) => {
    setSelecionados((prev) =>
      prev.includes(id) ? prev.filter((s) => s !== id) : [...prev, id]
    )
    setHorario('')
  }

  const handleSalvar = async () => {
    if (modo === 'criar' && !clienteId) {
      toast.warning('Selecione o cliente')
      return
    }
    if (selecionados.length === 0 || !data || !horario) {
      toast.warning('Preencha serviços, data e horário')
      return
    }

    const dataHora = `${data}T${horario}:00`
    setSalvando(true)
    try {
      if (modo === 'criar') {
        const { data: res } = await agendamentoApi.criarAdmin(clienteId as number, dataHora, selecionados)
        if (res.dados?.sugestao?.possuiAgendamentoNaSemana) {
          toast.info(res.dados.sugestao.mensagem)
        }
        toast.success(res.mensagem || 'Agendamento criado!')
      } else if (agendamentoId) {
        const { data: res } = await agendamentoApi.editar(agendamentoId, dataHora, selecionados)
        toast.success(res.mensagem || 'Agendamento alterado!')
      }
      onSuccess()
      onClose()
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensagem?: string } } })?.response?.data?.mensagem
      toast.error(msg || 'Erro ao salvar agendamento')
    } finally {
      setSalvando(false)
    }
  }

  const duracaoTotal = servicos
    .filter((s) => selecionados.includes(s.id))
    .reduce((acc, s) => acc + s.duracaoMinutos, 0)

  const valorTotal = servicos
    .filter((s) => selecionados.includes(s.id))
    .reduce((acc, s) => acc + s.valor, 0)

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {modo === 'criar' ? 'Novo agendamento' : 'Editar agendamento'}
      </DialogTitle>
      <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
        {carregando && <Typography color="text.secondary">Carregando...</Typography>}

        {modo === 'criar' && (
          <FormControl fullWidth>
            <InputLabel>Cliente</InputLabel>
            <Select
              value={clienteId}
              label="Cliente"
              onChange={(e) => setClienteId(e.target.value as number)}
            >
              {clientes.map((c) => (
                <MenuItem key={c.id} value={c.id}>
                  {c.nome} — {c.email}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        )}

        <Typography variant="subtitle2">Serviços</Typography>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
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
        </Box>

        {selecionados.length > 0 && (
          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            <Chip label={`${duracaoTotal} min`} size="small" />
            <Chip label={`R$ ${formatCurrency(valorTotal)}`} size="small" color="primary" variant="outlined" />
          </Box>
        )}

        <TextField
          type="date"
          label="Data"
          value={data}
          onChange={(e) => { setData(e.target.value); setHorario('') }}
          slotProps={{
            inputLabel: { shrink: true },
            htmlInput: { min: hojeParam() },
          }}
          fullWidth
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

        {modo === 'criar' && (
          <Alert severity="info" sx={{ mt: 1 }}>
            Agendamentos criados pela administração já entram como confirmados.
          </Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancelar</Button>
        <Button variant="contained" onClick={handleSalvar} disabled={salvando || !horario}>
          {salvando ? 'Salvando...' : 'Salvar'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
