import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import type { Usuario } from '../types'

interface AuthContextType {
  usuario: Usuario | null
  isAdmin: boolean
  isAuthenticated: boolean
  login: (token: string, usuario: Usuario) => void
  logout: () => void
}

const AuthContext = createContext<AuthContextType | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(() => {
    const stored = localStorage.getItem('usuario')
    return stored ? JSON.parse(stored) : null
  })

  useEffect(() => {
    const token = localStorage.getItem('token')
    if (!token) setUsuario(null)
  }, [])

  const login = (token: string, user: Usuario) => {
    localStorage.setItem('token', token)
    localStorage.setItem('usuario', JSON.stringify(user))
    setUsuario(user)
  }

  const logout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('usuario')
    setUsuario(null)
  }

  return (
    <AuthContext.Provider
      value={{
        usuario,
        isAdmin: usuario?.perfil === 'Admin',
        isAuthenticated: !!usuario && !!localStorage.getItem('token'),
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth deve ser usado dentro de AuthProvider')
  return ctx
}
