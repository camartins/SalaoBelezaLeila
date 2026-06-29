import { createTheme } from '@mui/material/styles'

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#B8956C',
      light: '#D4B896',
      dark: '#8B6914',
      contrastText: '#FFFBF9',
    },
    secondary: {
      main: '#E8B4B8',
      light: '#F5D5D8',
      dark: '#C99398',
      contrastText: '#5C4033',
    },
    background: {
      default: '#FAF7F5',
      paper: '#FFFBF9',
    },
    text: {
      primary: '#5C4033',
      secondary: '#8B7355',
    },
    divider: '#EDE0D4',
    success: { main: '#A8C5A0' },
    warning: { main: '#E8C4A0' },
    error: { main: '#D4A5A5' },
    info: { main: '#C4B5A0' },
  },
  typography: {
    fontFamily: '"Outfit", "Segoe UI", Roboto, sans-serif',
    h4: { fontWeight: 600, letterSpacing: '-0.02em' },
    h5: { fontWeight: 600 },
    h6: { fontWeight: 600 },
    button: { textTransform: 'none', fontWeight: 600 },
  },
  shape: { borderRadius: 16 },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          padding: '10px 24px',
          boxShadow: 'none',
          '&:hover': { boxShadow: '0 4px 14px rgba(184, 149, 108, 0.25)' },
        },
        contained: {
          background: 'linear-gradient(135deg, #C4A484 0%, #B8956C 100%)',
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 20,
          border: '1px solid #EDE0D4',
          boxShadow: '0 4px 24px rgba(92, 64, 51, 0.06)',
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          background: 'linear-gradient(135deg, #FFFBF9 0%, #FAF0EB 100%)',
          color: '#5C4033',
          borderBottom: '1px solid #EDE0D4',
          boxShadow: '0 2px 12px rgba(92, 64, 51, 0.04)',
        },
      },
    },
  },
})
