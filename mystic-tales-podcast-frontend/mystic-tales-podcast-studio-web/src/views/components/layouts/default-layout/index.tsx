import React, { useEffect, useState } from 'react'
import './styles.scss';
import { useDispatch, useSelector } from 'react-redux';
import { useLocation, useNavigate } from 'react-router-dom';
import { Box } from '@mui/material';
import type { RootState } from '../../../../redux/rootReducer';
import { clearAuthToken, setAuthToken } from '../../../../redux/auth/authSlice';
import { setUserContext } from '../../../../redux/navigation/navigationSlice';
import { JwtUtil } from '../../../../core/utils/jwt.util';
import { DefaultLayoutHeader } from './DefaultLayoutHeader';
import DefaultLayoutSideBar from './DefaultLayoutSideBar';
import DefaultLayoutContent from './DefaultLayoutContent';
import { set } from '@/redux/ui/uiSlice';
import { useNavigationContext } from '@/core/hooks/useNavigationContext';
import { _podcasterNav } from '@/router/_roleNav';
import { getPodcasterProfile } from '@/core/services/account/account.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { getPublicSource } from '@/core/services/file/file.service';
import { getChannelDetail } from '@/core/services/channel/channel.service';
import { getShowDetail } from '@/core/services/show/show.service';
import { initSessionSync, writeSession } from '@/core/auth/sessionSync';
import { toast } from 'react-toastify';

const DefaultLayout = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const location = useLocation();
  const authSlice = useSelector((state: RootState) => state.auth);
  const navigation = useSelector((state: RootState) => state.navigation);
  const uiSlice = useSelector((state: RootState) => state.ui);
  const { switchContextByType } = useNavigationContext();
  const [profile, setProfile] = useState<any>(null);
  const [profileLoaded, setProfileLoaded] = useState(false);
  const [mainImageUrl, setMainImageUrl] = useState<string>("");

  useEffect(() => {
    if (!authSlice || !authSlice.token || !JwtUtil.isTokenValid(authSlice.token) || !authSlice.user?.IsPodcaster) {
      dispatch(clearAuthToken());
      navigate('/login');
      return;
    }
    (async () => {
      try {
        const profileRes = await getPodcasterProfile(loginRequiredAxiosInstance, authSlice.user.Id);
        if (profileRes?.success) {
          const podcaster = profileRes.data?.PodcasterAccount;
          setProfile(podcaster);
         dispatch(setAuthToken({ ...authSlice, user: { ...authSlice.user, IsBuddy: podcaster.PodcasterProfile.IsBuddy, ViolationLevel: podcaster.ViolationLevel, PricePerBookingWord: podcaster.PodcasterProfile.PricePerBookingWord } }));
          
          const fileKey = podcaster?.MainImageFileKey;
          if (fileKey) {
            try {
              const avatarRes = await getPublicSource(loginRequiredAxiosInstance, fileKey);
              if (avatarRes?.success && avatarRes.data) {
                setMainImageUrl(avatarRes.data?.FileUrl || "");
              } else {
                setMainImageUrl("");
              }
            } catch (e) {
              setMainImageUrl("");
            }
          } else {
            setMainImageUrl("");
          }
        } else {
          dispatch(clearAuthToken());
          navigate('/login');
        }
      } catch (error) {
        console.error('Lỗi khi fetch podcaster profile:', error);
        dispatch(clearAuthToken());
        navigate('/login');
      } finally {
        setProfileLoaded(true);
      }
    })();
  }, [authSlice?.token, authSlice?.user?.Id, dispatch, navigate]);

  // Write session + listen for cross-tab user changes
 useEffect(() => {
  if (!(authSlice?.token && authSlice?.user?.Id)) return;

  // Đẩy session của tab hiện tại (tab vừa đăng nhập)
  writeSession({ userId: authSlice.user.Id, token: authSlice.token, ts: Date.now() });

  // Lắng nghe thay đổi từ tab khác, nếu khác userId -> logout tab này
  const cleanup = initSessionSync(authSlice.user.Id, (otherUserId) => {
    if (otherUserId !== authSlice.user.Id) {
      dispatch(clearAuthToken());
      navigate('/login');
      toast.info('Bạn đã bị đăng xuất vì tài khoản khác đăng nhập ở tab khác.');
    }
  });

  return cleanup;
}, [authSlice?.token, authSlice?.user?.Id, dispatch, navigate]);

  useEffect(() => {
    if (!profileLoaded || !profile) return;
    dispatch(setAuthToken({
      ...authSlice,
      user: {
        ...authSlice.user,
        mainImageUrl: mainImageUrl
      }
    }));
    if (!navigation.currentContext) {
      dispatch(setUserContext({
        user: {
          id: 'user',
          name: profile.PodcasterProfile?.Name,
          email: profile.Email,
          avatar: mainImageUrl
        }
        // navItems: _podcasterNav
      }));
    }
  }, [profileLoaded, profile, mainImageUrl, navigation.currentContext, dispatch]);

  if (!authSlice.token || !authSlice.user?.IsPodcaster) {
    return null;
  }
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

    if (detectedContext.type === 'user') return;

    const fetchAndSetContext = async () => {
      if (detectedContext.type === 'channel') {
        try {
          const res = await getChannelDetail(loginRequiredAxiosInstance, detectedContext.id);
          if (res?.success) {
            const channel = res.data?.Channel;
            switchContextByType({
              ...detectedContext,
              name: channel?.Name,
              avatar: channel?.MainImageFileKey
            });
          }else {
            navigate('/channel');
          }
        } catch (e) {
          console.error(e);
        }
      } else if (detectedContext.type === 'show') {
        try {
          const res = await getShowDetail(loginRequiredAxiosInstance, detectedContext.id);
          if (res?.success) {
            const show = res.data?.Show;
            switchContextByType({
              ...detectedContext,
              name: show?.Name,
              avatar: show?.MainImageFileKey
            });
          }else {
            navigate('/show');
          }
        } catch (e) {
          console.error(e);
        }
      }
    };

    fetchAndSetContext();

  }, [location.pathname]);
  
  useEffect(() => {
    if (!(authSlice?.token && authSlice?.user?.Id)) return;

    let stopped = false;
    let inFlight = false;

    const fetchSilently = async () => {
      if (stopped || inFlight || document.hidden) return;
      inFlight = true;
      try {
        const res = await getPodcasterProfile(loginRequiredAxiosInstance, authSlice.user.Id);
        if (res?.success) {
          const pod = res.data?.PodcasterAccount;
          if (pod.DeactivatedAt !== null){
            dispatch(clearAuthToken());
            navigate('/login');
            toast.info('Your account has been deactivated.');
            return;
          }
          const nextBalance =
            pod?.Balance ??
            authSlice.user.Balance;
          const nextViolation =
            pod?.ViolationLevel ??
            authSlice.user.ViolationLevel;


          // Chỉ dispatch khi có thay đổi để tránh re-render không cần thiết
          if (
            nextBalance !== authSlice.user.Balance ||
            nextViolation !== authSlice.user.ViolationLevel
          ) {
            dispatch(
              setAuthToken({
                ...authSlice,
                user: {
                  ...authSlice.user,
                  Balance: nextBalance,
                  ViolationLevel: nextViolation,
                },
              })
            );
          }
        }
      } catch (_) {
        // im lặng, không toast
      } finally {
        inFlight = false;
      }
    };

    // chạy ngay 1 lần khi tab đang visible
    if (!document.hidden) fetchSilently();

    const id = window.setInterval(fetchSilently, 2000 * 60*5);//30 000 thôi
    const onVis = () => {
      if (!document.hidden) fetchSilently();
    };
    document.addEventListener('visibilitychange', onVis);

    return () => {
      stopped = true;
      window.clearInterval(id);
       document.removeEventListener('visibilitychange', onVis);
    };
  }, [
    authSlice?.token,
    authSlice?.user?.Id,
    authSlice?.user?.Balance,
    authSlice?.user?.ViolationLevel,
    dispatch,
  ]);

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