import AppBreadcrumb from './AppBreadcrumb'
import AppContent from './AppContent'
import AppFooter from './AppFooter'
import AppHeader from './AppHeader'
import AppSidebar from './AppSidebar'
import React, { useEffect, useState } from 'react'
// import { AppContent, AppSidebar, AppFooter, AppHeader } from '../components/layout/index'
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { RootState } from '../../../../redux/root-reducer'
import { clearAuthToken } from '../../../../redux/auth/auth.slice'
import { JwtUtil } from '../../../../core/utils/jwt.util'
import "./styles.scss"

const DefaultLayout = () => {
  const dispatch = useDispatch();
  const [toast, setToast] =useState(null);

  
  const navigate = useNavigate();
  const authSlice = useSelector((state : RootState) => state.auth);

  // useEffect(() => {
  //   if ( !authSlice || !authSlice.token || !JwtUtil.isTokenNotExpired(authSlice.token) ) {
  //     dispatch(clearAuthToken())
  //     navigate('/login');
  //   }
  // }, [authSlice, navigate]);
  return (
    <div>
      <AppSidebar />
      <div className="wrapper d-flex flex-column min-vh-100">
        <AppHeader />
        <div className="body flex-grow-1">
          <AppContent />
        </div>
        <AppFooter />
      </div>
    </div>
  )
}

export default DefaultLayout
