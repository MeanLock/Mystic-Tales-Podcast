import React from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from 'react-redux'
import 'core-js'
import App from './App'
import { PersistGate } from 'redux-persist/integration/react'
import { persistor, store } from './redux/store'
import useAuthTabsSync from './hooks/useAuthTabsSync.hook'
import './index.css'
import { QueryClientProvider } from '@tanstack/react-query'
import queryClient from './lib/query'
const root = document.getElementById('root') as HTMLElement
if (!root) {
  throw new Error('Root element not found')
}

const AppWithAuthSync = () => {
  useAuthTabsSync(); // Sử dụng hook để đồng bộ hóa các tab
  return <App />;
};

// import reportWebVitals from './reportWebVitals'
createRoot(root).render(
  <Provider store={store}>
    <PersistGate loading={null} persistor={persistor}>
      <QueryClientProvider client={queryClient}>
        <AppWithAuthSync />
      </QueryClientProvider>
    </PersistGate>
  </Provider>,
);
