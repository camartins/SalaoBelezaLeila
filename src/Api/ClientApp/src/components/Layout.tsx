import {
  AppBar,
  Box,
  Button,
  Container,
  IconButton,
  Toolbar,
  Typography,
} from '@mui/material'
import ContentCutIcon from '@mui/icons-material/ContentCut'
import LogoutIcon from '@mui/icons-material/Logout'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

const clienteLinks = [
  { to: '/agendar', label: 'Agendar' },
  { to: '/historico', label: 'Histórico' },
]

const adminLinks = [
  { to: '/admin/dashboard', label: 'Dashboard' },
  { to: '/admin/agendamentos', label: 'Agendamentos' },
]

export default function Layout({ children }: { children: React.ReactNode }) {
  const { usuario, isAdmin, logout } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const links = isAdmin ? adminLinks : clienteLinks

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar position="sticky" elevation={0}>
        <Container maxWidth="lg">
          <Toolbar disableGutters sx={{ gap: 2, py: 0.5 }}>
            <ContentCutIcon sx={{ color: 'secondary.main', fontSize: 28 }} />
            <Typography
              variant="h6"
              sx={{ flexGrow: 1, fontWeight: 700, color: 'text.primary' }}
            >
              Cabeleleila Leila
            </Typography>
            {links.map((link) => (
              <Button
                key={link.to}
                component={Link}
                to={link.to}
                sx={{
                  color: location.pathname === link.to ? 'primary.main' : 'text.secondary',
                  bgcolor: location.pathname === link.to ? 'rgba(184,149,108,0.12)' : 'transparent',
                }}
              >
                {link.label}
              </Button>
            ))}
            <Typography variant="body2" sx={{ color: 'text.secondary', display: { xs: 'none', sm: 'block' } }}>
              {usuario?.nome}
            </Typography>
            <IconButton onClick={handleLogout} sx={{ color: 'text.secondary' }}>
              <LogoutIcon />
            </IconButton>
          </Toolbar>
        </Container>
      </AppBar>
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {children}
      </Container>
    </Box>
  )
}
