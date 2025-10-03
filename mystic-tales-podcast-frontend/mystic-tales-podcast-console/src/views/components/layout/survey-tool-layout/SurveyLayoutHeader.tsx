"use client"

import type { FC } from "react"
import type React from "react"
import { useCallback, useEffect, useRef, useState } from "react"
import { NavLink, useLocation, useNavigate } from "react-router-dom"
import { useSelector, useDispatch } from "react-redux"
import {
  CContainer,
  CDropdown,
  CDropdownMenu,
  CDropdownToggle,
  CHeader,
  CHeaderNav,
  CNavLink,
  CNavItem,
  useColorModes,
  CButton,
  CNav,
  CBadge,
  CDropdownItem,
  CAvatar,
} from "@coreui/react"
import { IconNfcOff } from "@tabler/icons-react"
import type { RootState } from "../../../../redux/root-reducer"
import { type AuthState, clearAuthToken } from "../../../../redux/auth/auth.slice"
import { JwtUtil } from "../../../../core/utils/jwt.util"
import { setUi } from "../../../../redux/ui/ui.slice"
import routes from "../../../../routes"
import { get_roleNav } from "@/_role-nav"
import logo from "../../../../assets/brand/logo.png"
import "./styles.scss"
import { avatar8 } from "@/assets/images"

// Define types for nav items
interface Badge {
  color: string
  text: string
}

interface NavItemType {
  component: React.ElementType
  name?: React.ReactNode
  badge?: Badge
  icon?: React.ReactNode
  to?: string
  href?: string
  items?: NavItemType[]
  [key: string]: any
}

type SurveyLayoutHeaderProps = {}

export const SurveyLayoutHeader: FC<SurveyLayoutHeaderProps> = ({ }) => {
  const authSlice = useSelector((state: RootState) => state.auth)
  const uiSlice = useSelector((state: RootState) => state.ui)
  const [navigation, setNavigation] = useState<NavItemType[]>([])
  const dispatch = useDispatch()
  const location = useLocation()
  const navigate = useNavigate()
  const [user, setUser] = useState<AuthState["user"]>(null)
  const { colorMode, setColorMode } = useColorModes("")

  useEffect(() => {
    if (authSlice && authSlice.token) {
      const user = JwtUtil.decodeToken(authSlice.token)
      setNavigation(get_roleNav(user.role_id, user.id))
    } else {
      navigate("/login")
    }
  }, [authSlice])



  const getScreenName = useCallback((pathname: string): string => {
    const route = routes.find((route) => {
      const pattern = route.path.replace(/:\w+/g, "[^/]+")
      const regex = new RegExp(`^${pattern}$`)
      return regex.test(pathname)
    })
    if (route?.parent) {
      const parentRoute = routes.find((r) => r.path === route.parent)
      return parentRoute?.name || route?.name || "Dashboard"
    }
    return route?.name || "Dashboard"
  }, [])

  useEffect(() => {
    if (authSlice && authSlice.token && JwtUtil.isTokenNotExpired(authSlice.token)) {
      setUser(authSlice.user)
    }
  }, [authSlice, location.pathname, getScreenName])

  useEffect(() => {
    if (uiSlice && uiSlice.theme) {
      setColorMode(uiSlice.theme)
    }
  }, [location.pathname, authSlice])

  const handleColorModeChange = (mode: string) => {
    setColorMode(mode)
    dispatch(setUi({ theme: mode }))
  }

  const handleLogout = () => {
    dispatch(clearAuthToken())
  }

  const renderNavItem = (item: NavItemType, index: number) => {
    const { name, badge, icon, to, href, items } = item
    return (
      <CNavItem key={index}>
        <CNavLink
          {...(to && { as: NavLink, to })}
          {...(href && { href, target: "_blank", rel: "noopener noreferrer" })}
          className="d-flex align-items-center nav-item-custom"
        >
          <span className="nav-text">{name}</span>
          {badge && (
            <CBadge color={badge.color} size="sm" className="nav-badge">
              {badge.text}
            </CBadge>
          )}
        </CNavLink>
      </CNavItem>
    )
  }

  return (
    <CHeader className="survey-tool-header w-full " >
      <CContainer className="!px-10 " fluid>
        <div className="survey-tool-header__logo">
          <img src={logo} alt="Survey Talk" style={{ width: "140px", height: "70px" }} />
        </div>

        <CHeaderNav className="survey-tool-header__nav d-flex align-items-center flex-grow-1 justify-content-center">
          <CNav className="d-flex align-items-center gap-5">
            {navigation && navigation.map((item, index) => renderNavItem(item, index))}
          </CNav>
        </CHeaderNav>

        <CHeaderNav className="d-flex align-items-center">
          <CDropdown variant="nav-item">
            <CDropdownToggle className="py-0 pe-00" caret={false}>
              <CAvatar src={avatar8} size="lg" />
            </CDropdownToggle>
            <CDropdownMenu>

              <CDropdownItem onClick={handleLogout} >Đăng xuất</CDropdownItem>
            </CDropdownMenu>
          </CDropdown>
        </CHeaderNav>
      </CContainer>
    </CHeader>
  )
}
