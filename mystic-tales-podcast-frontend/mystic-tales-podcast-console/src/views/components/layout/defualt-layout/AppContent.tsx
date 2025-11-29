import React, { Suspense, useEffect, useState } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { CContainer, CSpinner } from '@coreui/react'
import { useDispatch, useSelector } from 'react-redux'
import routes from '../../../../routes'
import { RootState } from '../../../../redux/root-reducer'
import { JwtUtil } from '../../../../core/utils/jwt.util'
import { LocalStorageUtil } from '../../../../core/utils/storage.util'

const AppContent = () => {
  const authSlice = useSelector((state: RootState) => state.auth);
  const dispatch = useDispatch();
  const [ready, setReady] = useState(false);

  return (
<CContainer fluid className="app-content-container">
        <Suspense fallback={<CSpinner color="primary" />}>
        <Routes>
          {routes.map((route, idx) => {
            return (
              route.element && (LocalStorageUtil.getAuthUserFromPersistLocalStorage() && route.role_id?.some(item => item === Number(LocalStorageUtil.getAuthUserFromPersistLocalStorage().role_id))) && (
                <Route
                  key={idx}
                  path={route.path}
                  // exact={route.exact}
                  // name={route.name}
                  element={<route.element />}
                />
              )
            )
          })}
          <Route path="/" element={<Navigate to="dashboard" replace />} /> 
        </Routes>
        {/* <Routes>
          {routes.map((route, idx) => {
            return (
              route.element && (route.role_id?.some(item => item === 1)) && (
                <Route
                  key={idx}
                  path={route.path}
                  // exact={route.exact}
                  // name={route.name}
                  element={<route.element />}
                />
              )
            )
          })}
          <Route path="/" element={<Navigate to="dashboard" replace />} />
        </Routes> */}
      </Suspense>
    </CContainer>
  )
}

export default React.memo(AppContent)
