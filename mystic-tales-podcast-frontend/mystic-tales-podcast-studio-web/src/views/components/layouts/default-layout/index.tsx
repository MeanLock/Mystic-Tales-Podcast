import React, { useEffect } from 'react'
import './styles.scss';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { Box } from '@mui/material';
import type { RootState } from '../../../../redux/rootReducer';
import { clearAuthToken } from '../../../../redux/auth/authSlice';
import { setUserContext } from '../../../../redux/navigation/navigationSlice';
import { JwtUtil } from '../../../../core/utils/jwt.util';
import { DefaultLayoutHeader } from './DefaultLayoutHeader';
import DefaultLayoutSideBar from './DefaultLayoutSideBar';
import DefaultLayoutContent from './DefaultLayoutContent';
import { _podcasterNav } from '../../../../router/_roleNav';
import { set } from '@/redux/ui/uiSlice';

const DefaultLayout = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const authSlice = useSelector((state: RootState) => state.auth);
  const navigation = useSelector((state: RootState) => state.navigation);
  const uiSlice = useSelector((state: RootState) => state.ui);

  // useEffect(() => {
  //   if (!authSlice || !authSlice.token || !JwtUtil.isTokenValid(authSlice.token)) {
  //     dispatch(clearAuthToken());
  //     navigate('/login');
  //     return;
  //   }

  //   // Check if user has podcaster profile
  //   const user = authSlice.user;
  //   if (!user?.podcasterProfile) {
  //     navigate('/login');
  //     return;
  //   }

  //   // Initialize user context if not already set
  //   if (!navigation.currentContext) {
  //     dispatch(setUserContext({
  //       user: {
  //         id: user.id,
  //         name: user.podcasterProfile?.name || user.name,
  //         email: user.email,
  //         avatar: user.podcasterProfile?.avatar || user.avatar
  //       },
  //       navItems: _podcasterNav
  //     }));
  //   }
  // }, [authSlice, navigation.currentContext, dispatch, navigate]);

  // // Don't render if not authenticated
  // if (!authSlice.token || !authSlice.user?.podcasterProfile) {
  //   return null;
  // }

  const handleOverlayClick = () => {
    dispatch(set({ sidebarMobileOpen: false }))
  }

  // Determine if we're on mobile
  const isMobile = window.innerWidth <= 768

  // Build content className
  const contentClassName = [
    "default-layout__content",
    uiSlice.sidebarNarrow && !isMobile ? "default-layout__content--narrow" : "",
  ]
    .filter(Boolean)
    .join(" ")

  // Build overlay className
  const overlayClassName = [
    "default-layout__overlay",
    uiSlice.sidebarMobileOpen && isMobile ? "default-layout__overlay--visible" : "",
  ]
    .filter(Boolean)
    .join(" ")
  return (
    <Box className="default-layout">
      <DefaultLayoutHeader />
      <DefaultLayoutSideBar />
      <Box className={overlayClassName} onClick={handleOverlayClick} />
      <Box className={contentClassName}>
        <DefaultLayoutContent />
      </Box>
    </Box>
  )
}

export default DefaultLayout