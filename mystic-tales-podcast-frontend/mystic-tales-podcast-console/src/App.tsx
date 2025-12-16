import React, { Suspense, useEffect } from 'react'
import { createBrowserRouter, createHashRouter, HashRouter, Route, RouterProvider, Routes } from 'react-router-dom'
import { useSelector } from 'react-redux'

import { CSpinner, useColorModes } from '@coreui/react'
import './scss/style.scss'

// We use those styles to show code examples, you should remove them in your application.
import './scss/examples.scss'
import { RootState } from './redux/root-reducer'
import { ToastContainer } from 'react-toastify';


// Containers
const DefaultLayout = React.lazy(() => import('./views/components/layout/defualt-layout/index'))

// Pages
const Login = React.lazy(() => import('./views/pages/login/Login'))
const Register = React.lazy(() => import('./views/pages/register/Register'))
const Page404 = React.lazy(() => import('./views/pages/page404/Page404'))
const Page500 = React.lazy(() => import('./views/pages/page500/Page500'))



const router = createBrowserRouter([
  {
    path: '/login',
    element: <Login />,
  },
  {
    path: '/register',
    element: <Register />,
  },
  {
    path: '/404',
    element: <Page404 />,
  },
  {
    path: '/500',
    element: <Page500 />,
  },
  {
    path: '*',
    element: <DefaultLayout />,
  },
])
const App = () => {
  const { isColorModeSet, setColorMode } = useColorModes('')
  const uiSlice = useSelector((state: RootState) => state.ui)
  const storedTheme = useSelector((state: any) => state.theme)


  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.href.split('?')[1] || '')
    const themeMatch = urlParams.get('theme')?.match(/^[A-Za-z0-9\s]+/)
    const theme = themeMatch ? themeMatch[0] : undefined
    if (theme) {
      setColorMode(theme)
    }

    if (isColorModeSet()) {
      return
    }

    // setColorMode(storedTheme)
  }, [])


  useEffect(() => {
    if (uiSlice && uiSlice.theme) {

      setColorMode(uiSlice.theme)
    }
  }, [uiSlice])

  return (
    <Suspense
      fallback={
        <div className="pt-3 text-center">
          <CSpinner color="primary" variant="grow" />
        </div>
      }
    >
      <RouterProvider router={router} />
      <ToastContainer />
    </Suspense>
  )
}

export default App
