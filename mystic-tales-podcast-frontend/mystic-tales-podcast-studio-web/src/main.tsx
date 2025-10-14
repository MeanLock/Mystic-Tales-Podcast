import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.scss'
import App from './App.tsx'
import { Provider } from 'react-redux'
import { PersistGate } from 'redux-persist/integration/react'
import { BrowserRouter } from 'react-router-dom'
import { GoogleOAuthProvider } from '@react-oauth/google'
import { persistor, store } from './redux/store.ts'

createRoot(document.getElementById('root')!).render(
  <Provider store={store}>
    <PersistGate loading={null} persistor={persistor}>
        <GoogleOAuthProvider clientId={'284467404982-b4j9dvoani124mce15l0mmtfucil72g0.apps.googleusercontent.com'}>
          <App />
        </GoogleOAuthProvider>
    </PersistGate>
  </Provider>
)
