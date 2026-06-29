import { Box, Button, Card, CardContent, Link as MuiLink, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { authApi } from '../services/api'

export default function CadastroPage() {
  const [form, setForm] = useState({ nome: '', email: '', senha: '', telefone: '' })
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    try {
      await authApi.cadastrar(form)
      toast.success('Cadastro realizado! Faça login.')
      navigate('/login')
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensagem?: string } } })?.response?.data?.mensagem
      toast.error(msg || 'Erro ao cadastrar')
    } finally {
      setLoading(false)
    }
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(160deg, #FAF7F5 0%, #F5E6E0 50%, #EDE0D4 100%)',
        p: 2,
      }}
    >
      <Card sx={{ maxWidth: 460, width: '100%' }}>
        <CardContent sx={{ p: 4 }}>
          <Typography variant="h5" gutterBottom sx={{ fontWeight: 700 }}>
            Criar conta
          </Typography>
          <Typography color="text.secondary" sx={{ mb: 3 }}>
            Cadastre-se para agendar online
          </Typography>
          <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField label="Nome" value={form.nome} onChange={(e) => setForm({ ...form, nome: e.target.value })} required fullWidth />
            <TextField label="E-mail" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required fullWidth />
            <TextField label="Telefone" value={form.telefone} onChange={(e) => setForm({ ...form, telefone: e.target.value })} required fullWidth />
            <TextField label="Senha" type="password" value={form.senha} onChange={(e) => setForm({ ...form, senha: e.target.value })} required fullWidth />
            <Button type="submit" variant="contained" size="large" disabled={loading}>
              {loading ? 'Cadastrando...' : 'Cadastrar'}
            </Button>
          </Box>
          <Typography variant="body2" sx={{ mt: 2, textAlign: 'center' }}>
            Já tem conta?{' '}
            <MuiLink component={Link} to="/login" underline="hover">
              Entrar
            </MuiLink>
          </Typography>
        </CardContent>
      </Card>
    </Box>
  )
}
