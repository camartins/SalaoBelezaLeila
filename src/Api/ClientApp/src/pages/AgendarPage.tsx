import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Chip,
  FormControlLabel,
  Grid,
  TextField,
  Typography,
} from '@mui/material'
import EventAvailableIcon from '@mui/icons-material/EventAvailable'
import { useEffect, useState } from 'react'
import { toast } from 'react-toastify'
import Layout from '../components/Layout'
import type { Servico, SugestaoAgendamento } from '../types'
import { agendamentoApi, servicoApi } from '../services/api'
import { formatCurrency, minDataAgendamento } from '../utils/agendamento'

export default function AgendarPage() {
  const [servicos, setServicos] = useState<Servico[]>([])
  const [selecionados, setSelecionados] = useState<number[]>([])
  const [data, setData] = useState('')
  const [horario, setHorario] = useState('')
  const [horarios, setHorarios] = useState<string[]>([])
  const [sugestao, setSugestao] = useState<SugestaoAgendamento | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    servicoApi.listar().then(({ data }) => setServicos(data.dados || []))
  }, [])

  useEffect(() => {
    if (!data || selecionados.length === 0) {
      setHorarios([])
      return
    }
    agendamentoApi
      .horariosDisponiveis(data, selecionados)
      .then(({ data: res }) => setHorarios(res.dados?.horariosDisponiveis || []))
      .catch((err: unknown) => {
        setHorarios([])
        const msg = (err as { response?: { data?: { mensagem?: string } } })?.response?.data?.mensagem
        if (msg) toast.error(msg)
      })
  }, [data, selecionados])

  useEffect(() => {
    if (!data || !horario) {
      setSugestao(null)
      return
    }
    const dataHora = `${data}T${horario}:00`
    agendamentoApi
      .sugestao(dataHora)
      .then(({ data: res }) => {
        const sug = res.dados
        if (sug?.possuiAgendamentoNaSemana) {
          setSugestao(sug)
        } else {
          setSugestao(null)
        }
      })
      .catch(() => setSugestao(null))
  }, [data, horario])

  const toggleServico = (id: number) => {
    setSelecionados((prev) =>
      prev.includes(id) ? prev.filter((s) => s !== id) : [...prev, id]
    )
    setHorario('')
  }

  const duracaoTotal = servicos
    .filter((s) => selecionados.includes(s.id))
    .reduce((acc, s) => acc + s.duracaoMinutos, 0)

  const valorTotal = servicos
    .filter((s) => selecionados.includes(s.id))
    .reduce((acc, s) => acc + s.valor, 0)

  const handleAgendar = async () => {
    if (selecionados.length === 0) {
      toast.warning('Selecione ao menos um serviço')
      return
    }
    if (!data || !horario) {
      toast.warning('Selecione data e horário')
      return
    }
    if (data < minDataAgendamento()) {
      toast.warning('Agendamentos devem ser feitos com pelo menos 2 dias de antecedência')
      return
    }

    const dataHora = `${data}T${horario}:00`
    setLoading(true)
    try {
      const { data: res } = await agendamentoApi.criar(dataHora, selecionados)
      if (res.dados?.sugestao?.possuiAgendamentoNaSemana) {
        setSugestao(res.dados.sugestao)
        toast.info(res.dados.sugestao.mensagem)
      }
      toast.success(res.mensagem || 'Agendamento criado!')
      setSelecionados([])
      setData('')
      setHorario('')
      setSugestao(null)
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensagem?: string } } })?.response?.data?.mensagem
      toast.error(msg || 'Erro ao agendar')
    } finally {
      setLoading(false)
    }
  }

  const aplicarSugestao = () => {
    if (!sugestao?.dataSugerida) return
    const dt = new Date(sugestao.dataSugerida)
    const y = dt.getFullYear()
    const m = String(dt.getMonth() + 1).padStart(2, '0')
    const d = String(dt.getDate()).padStart(2, '0')
    setData(`${y}-${m}-${d}`)
    setHorario(dt.toTimeString().slice(0, 5))
    setSugestao(null)
    toast.success('Data sugerida aplicada!')
  }

  return (
    <Layout>
      <Typography variant="h4" gutterBottom>
        Agendar Serviços
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        Escolha um ou mais serviços e selecione um horário disponível (mínimo 2 dias de antecedência)
      </Typography>

      {sugestao?.possuiAgendamentoNaSemana && (
        <Alert
          severity="info"
          sx={{ mb: 3, bgcolor: 'rgba(232,180,184,0.25)' }}
          action={
            <Button color="inherit" size="small" onClick={aplicarSugestao}>
              Usar data sugerida
            </Button>
          }
        >
          {sugestao.mensagem}
        </Alert>
      )}

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Serviços disponíveis
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                {servicos.map((s) => (
                  <Box
                    key={s.id}
                    sx={{
                      p: 2,
                      borderRadius: 2,
                      border: '1px solid',
                      borderColor: selecionados.includes(s.id) ? 'primary.main' : 'divider',
                      bgcolor: selecionados.includes(s.id) ? 'rgba(184,149,108,0.08)' : 'transparent',
                    }}
                  >
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={selecionados.includes(s.id)}
                          onChange={() => toggleServico(s.id)}
                        />
                      }
                      label={
                        <Box>
                          <Typography sx={{ fontWeight: 600 }}>{s.nome}</Typography>
                          <Typography variant="body2" color="text.secondary">
                            {s.duracaoMinutos} min · R$ {formatCurrency(s.valor)}
                          </Typography>
                        </Box>
                      }
                    />
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 5 }}>
          <Card>
            <CardContent sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Typography variant="h6">Data e horário</Typography>
              <TextField
                type="date"
                label="Data"
                value={data}
                onChange={(e) => { setData(e.target.value); setHorario('') }}
                slotProps={{
                  inputLabel: { shrink: true },
                  htmlInput: { min: minDataAgendamento() },
                }}
                fullWidth
              />
              {selecionados.length > 0 && (
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                  <Chip label={`${selecionados.length} serviço(s)`} size="small" />
                  <Chip label={`${duracaoTotal} min`} size="small" />
                  <Chip label={`R$ ${formatCurrency(valorTotal)}`} size="small" color="primary" variant="outlined" />
                </Box>
              )}
              <Typography variant="body2" color="text.secondary">
                Horários disponíveis
              </Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {horarios.length === 0 && data && selecionados.length > 0 && (
                  <Typography variant="body2" color="text.secondary">
                    Nenhum horário disponível nesta data
                  </Typography>
                )}
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
              </Box>
              <Button
                variant="contained"
                size="large"
                startIcon={<EventAvailableIcon />}
                onClick={handleAgendar}
                disabled={loading || !horario || selecionados.length === 0}
                sx={{ mt: 1 }}
              >
                {loading ? 'Agendando...' : 'Confirmar agendamento'}
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Layout>
  )
}
