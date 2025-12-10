import React, { useCallback, useEffect, useRef, useState } from 'react'
import { NavLink, useLocation } from 'react-router-dom'
import { useSelector, useDispatch } from 'react-redux'
import {
  CContainer,
  CDropdown,
  CDropdownItem,
  CDropdownMenu,
  CDropdownToggle,
  CHeader,
  CHeaderNav,
  CNavLink,
  CNavItem,
  useColorModes,
  CButton,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilBell,
  cilContrast,
  cilEnvelopeOpen,
  cilList,
  cilMoon,
  cilSun,
} from '@coreui/icons'

import { AppHeaderDropdown } from './header/index'
import AppBreadcrumb from './AppBreadcrumb'
import { IconNfcOff } from '@tabler/icons-react'
import { RootState } from '../../../../redux/root-reducer'
import { AuthState, clearAuthToken } from '../../../../redux/auth/auth.slice'
import { JwtUtil } from '../../../../core/utils/jwt.util'
import { setUi } from '../../../../redux/ui/ui.slice'
import routes from '../../../../routes'

const AppHeader = () => {
  const headerRef = useRef<HTMLDivElement>(null)
  const authSlice = useSelector((state: RootState) => state.auth);
  const uiSlice = useSelector((state: RootState) => state.ui);

  const dispatch = useDispatch()
  const location = useLocation()

  const [user, setUser] = useState<AuthState["user"]>(null);
  const { colorMode, setColorMode } = useColorModes("")
  const [screenName, setScreenName] = useState<string>('Dashboard')



  useEffect(() => {
    document.addEventListener('scroll', () => {
      headerRef.current &&
        headerRef.current.classList.toggle('shadow-sm', document.documentElement.scrollTop > 0)
    })
  }, [])

  const getScreenName = useCallback((pathname: string): string => {
    const route = routes.find(route => {
      const pattern = route.path.replace(/:\w+/g, '[^/]+');
      const regex = new RegExp(`^${pattern}$`);
      return regex.test(pathname);
    });
    if (route?.parent) {
      const parentRoute = routes.find(r => r.path === route.parent);
      return parentRoute?.name || route?.name || 'Dashboard';
    }
    return route?.name || 'Dashboard';
  }, []);

  useEffect(() => {
    if (authSlice && authSlice.token && JwtUtil.isTokenNotExpired(authSlice.token)) {
      setUser(authSlice.user);
      setScreenName(getScreenName(location.pathname))
    }
  }, [authSlice, location.pathname, getScreenName]);

  useEffect(() => {
    if (uiSlice && uiSlice.theme) {
      setColorMode(uiSlice.theme);
    }
  }, [location.pathname, authSlice]);


  const handleColorModeChange = (mode: string) => {
    setColorMode(mode);
    dispatch(setUi({ theme: mode }));
  }

  const handleLogout = () => {
    dispatch(clearAuthToken());
    window.location.href = '/login';
  }


  return (
    <CHeader position="sticky" className="app-header mb-4 p-0" ref={headerRef}>
      <CContainer className="border-bottom px-4" fluid>
        <CHeaderNav className="d-none d-md-flex">
          <CNavItem className="app-header__title ms-2">
            {screenName}
          </CNavItem>
        </CHeaderNav>
        {/* <CHeaderNav className="ms-auto">
          <CNavItem>
            <CNavLink href="#">
              <CIcon icon={cilBell} size="lg" />
            </CNavLink>
          </CNavItem>
          <CNavItem>
            <CNavLink href="#">
              <CIcon icon={cilList} size="lg" />
            </CNavLink>
          </CNavItem>
          <CNavItem>
            <CNavLink href="#">
              <CIcon icon={cilEnvelopeOpen} size="lg" />
            </CNavLink>
          </CNavItem>
        </CHeaderNav> */}
        {/* <CHeaderNav>
          <li className="nav-item py-1">
            <div className="vr h-100 mx-2 text-body text-opacity-75"></div>
          </li>
          <CDropdown variant="nav-item" placement="bottom-end">
            <CDropdownToggle caret={false}>
              {colorMode === 'dark' ? (
                <CIcon icon={cilMoon} size="lg" />
              ) : colorMode === 'auto' ? (
                <CIcon icon={cilContrast} size="lg" />
              ) : (
                <CIcon icon={cilSun} size="lg" />
              )}
            </CDropdownToggle>
            <CDropdownMenu>
              <CDropdownItem
                active={colorMode === 'light'}
                className="d-flex align-items-center"
                as="button"
                type="button"
                onClick={() => handleColorModeChange('light')}
              >
                <CIcon className="me-2" icon={cilSun} size="lg" /> Light
              </CDropdownItem>
              <CDropdownItem
                active={colorMode === 'dark'}
                className="d-flex align-items-center"
                as="button"
                type="button"
                onClick={() => handleColorModeChange('dark')}
              >
                <CIcon className="me-2" icon={cilMoon} size="lg" /> Dark
              </CDropdownItem>
              <CDropdownItem
                active={colorMode === 'auto'}
                className="d-flex align-items-center"
                as="button"
                type="button"
                onClick={() => handleColorModeChange('auto')}
              >
                <CIcon className="me-2" icon={cilContrast} size="lg" /> Auto
              </CDropdownItem>
            </CDropdownMenu>
          </CDropdown>
          <li className="nav-item py-1">
            <div className="vr h-100 mx-2 text-body text-opacity-75"></div>
          </li>
        </CHeaderNav> */}
        {user ? (
          <CHeaderNav >
            <AppHeaderDropdown user={user} />
            <CNavItem className='mx-3 d-flex align-items-center'>
              <CButton onClick={handleLogout} title='logout' className='bg-danger btn-sm fw-bold '>
                LOG OUT
                {/* <SignOut className='fw-bold' size={25} color="hotpink" weight="bold" /> */}
                {/* <IconNfcOff stroke={1} accentHeight={12} width={20} height={20} /> */}
              </CButton>
            </CNavItem>
          </CHeaderNav>
        ) : (
          <CHeaderNav >
            <CNavItem className='mx-1'>
          
            </CNavItem>
          </CHeaderNav>

        )}
      </CContainer>
      <CContainer className="px-4" fluid>
        <AppBreadcrumb />
      </CContainer>
    </CHeader>
  )
}

export default AppHeader
