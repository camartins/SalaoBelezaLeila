import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Link as MuiLink,
  TextField,
  Typography,
} from '@mui/material'
import ContentCutIcon from '@mui/icons-material/ContentCut'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { useAuth } from '../contexts/AuthContext'
import { authApi } from '../services/api'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [loading, setLoading] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    try {
      const { data } = await authApi.login(email, senha)
      const payload = data.dados!
      login(payload.token, payload.usuario)
      toast.success('Bem-vinda de volta!')
      navigate(payload.usuario.perfil === 'Admin' ? '/admin/dashboard' : '/agendar')
    } catch {
      toast.error('E-mail ou senha inválidos')
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
      <Card sx={{ maxWidth: 420, width: '100%' }}>
        <CardContent sx={{ p: 4 }}>
          <Box sx={{ textAlign: 'center', mb: 3 }}>
            <ContentCutIcon sx={{ fontSize: 48, color: 'secondary.main', mb: 1 }} />
            <Typography variant="h5" gutterBottom>
              Cabeleleila Leila
            </Typography>
            <Typography color="text.secondary">
              Faça login para agendar seus serviços
            </Typography>
          </Box>
          <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField label="E-mail" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required fullWidth />
            <TextField label="Senha" type="password" value={senha} onChange={(e) => setSenha(e.target.value)} required fullWidth />
            <Button type="submit" variant="contained" size="large" disabled={loading}>
              {loading ? 'Entrando...' : 'Entrar'}
            </Button>
          </Box>
          <Typography variant="body2" sx={{ mt: 2, textAlign: 'center' }}>
            Não tem conta?{' '}
            <MuiLink component={Link} to="/cadastro" underline="hover">
              Cadastre-se
            </MuiLink>
          </Typography>
          <Alert severity="info" sx={{ mt: 2, bgcolor: 'rgba(232,180,184,0.2)' }}>
            Admin demo: admin@cabeleleila.com / Admin@123
          </Alert>
        </CardContent>
      </Card>
    </Box>
  )
}
