import React, { useEffect } from 'react'
import './styles.scss';
import { useDispatch, useSelector } from 'react-redux';
import { useLocation, useNavigate } from 'react-router-dom';
import { Box } from '@mui/material';
import type { RootState } from '../../../../redux/rootReducer';
import { clearAuthToken } from '../../../../redux/auth/authSlice';
import { setUserContext } from '../../../../redux/navigation/navigationSlice';
import { JwtUtil } from '../../../../core/utils/jwt.util';
import { DefaultLayoutHeader } from './DefaultLayoutHeader';
import DefaultLayoutSideBar from './DefaultLayoutSideBar';
import DefaultLayoutContent from './DefaultLayoutContent';
import { set } from '@/redux/ui/uiSlice';
import { useNavigationContext } from '@/core/hooks/useNavigationContext';

const DefaultLayout = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
    const location = useLocation(); 
  const authSlice = useSelector((state: RootState) => state.auth);
  const navigation = useSelector((state: RootState) => state.navigation);
  const uiSlice = useSelector((state: RootState) => state.ui);
  const { switchContextByType } = useNavigationContext();

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

  // Don't render if not authenticated
  // if (!authSlice.token || !authSlice.user?.podcasterProfile) {
  //   return null;
  // }
  const detectContextFromPath = (pathname: string) => {
    // Channel context: /my-channel/:id/*
    const channelMatch = pathname.match(/^\/channel\/([^\/]+)/);
    if (channelMatch) {
      return {
        type: 'channel' as const,
        id: channelMatch[1],
        basePath: `/channel/${channelMatch[1]}`
      };
    }

    // Show/Episode context: /show/:id/*
    const showMatch = pathname.match(/^\/show\/([^\/]+)/);
    if (showMatch) {
      return {
        type: 'show' as const,
        id: showMatch[1],
        basePath: `/show/${showMatch[1]}`
      };
    }

    return {
      type: 'user' as const,
      id: 'user',
      basePath: '/'
    };
  };

  useEffect(() => {
    const pathname = location.pathname;
    const detectedContext = detectContextFromPath(pathname);
    if (
      navigation.contextType !== detectedContext.type ||
      navigation.currentContext?.id !== detectedContext.id
    ) {
      switchContextByType(detectedContext);
    }
  }, [location.pathname, navigation.contextType, navigation.currentContext?.id]);

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