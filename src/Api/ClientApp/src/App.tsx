import { Navigate, Route, Routes } from 'react-router-dom'
import ProtectedRoute from './components/ProtectedRoute'
import { useAuth } from './contexts/AuthContext'
import AgendarPage from './pages/AgendarPage'
import CadastroPage from './pages/CadastroPage'
import HistoricoPage from './pages/HistoricoPage'
import LoginPage from './pages/LoginPage'
import AgendamentosAdminPage from './pages/admin/AgendamentosPage'
import DashboardPage from './pages/admin/DashboardPage'

function HomeRedirect() {
  const { isAuthenticated, isAdmin } = useAuth()
  if (!isAuthenticated) return <Navigate to="/login" replace />
  return <Navigate to={isAdmin ? '/admin/dashboard' : '/agendar'} replace />
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<HomeRedirect />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/cadastro" element={<CadastroPage />} />
      <Route
        path="/agendar"
        element={
          <ProtectedRoute>
            <AgendarPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/historico"
        element={
          <ProtectedRoute>
            <HistoricoPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/admin/dashboard"
        element={
          <ProtectedRoute adminOnly>
            <DashboardPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/admin/agendamentos"
        element={
          <ProtectedRoute adminOnly>
            <AgendamentosAdminPage />
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
