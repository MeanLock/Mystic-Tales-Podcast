import React, { useEffect, useState } from 'react'
import { useSelector, useDispatch } from 'react-redux'

import {
  CCloseButton,
  CHeaderToggler,
  CSidebar,
  CSidebarBrand,
  CSidebarFooter,
  CSidebarHeader,
  CSidebarToggler,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { AppSidebarNav } from './AppSidebarNav'
import { useNavigate } from 'react-router-dom'
import { RootState } from '../../../../redux/root-reducer'
import { JwtUtil } from '../../../../core/utils/jwt.util'
import { get_roleNav } from '../../../../_role-nav'
import { setUi, UiState } from '../../../../redux/ui/ui.slice'
import { cilMenu } from '@coreui/icons'
import logo from '../../../../assets/brand/logo.png'
const AppSidebar = () => {

  // const unfoldable = useSelector((state : State) => state.sidebarUnfoldable)
  // const sidebarShow = useSelector((state: UiState) => state.sidebarShow)

  // STATES
  const [navigation, setNavigation] = useState<any[]>([]);

  // HOOKS
  const navigate = useNavigate();

  // REDUX
  const dispatch = useDispatch()
  const uiSlice = useSelector((state: RootState) => state.ui)
  const authSlice = useSelector((state: RootState) => state.auth);


  useEffect(() => {
    if (authSlice && authSlice.token) {
      const user = JwtUtil.decodeToken(authSlice.token);

      //*[NOTE] Sau có thay đổi role thì gọi lại hàm này
      setNavigation(get_roleNav(user.role_id, user.id));

    } else {
      navigate("/login")
    }
  }, [authSlice])
  return (
    <CSidebar
      className={`app-sidebar`}
      position="fixed"
      narrow={uiSlice.sidebarNarrow}
    >
      <CSidebarHeader className="app-sidebar__header">
        <div className="app-sidebar__header-content">
          <CSidebarBrand >
            {uiSlice.sidebarNarrow ? (
              <CHeaderToggler onClick={() => dispatch(setUi({ sidebarNarrow: false }))}>
                <img src={logo} alt="Logo" className="sidebar-brand-narrow" />
              </CHeaderToggler>
            ) : (
              <img src={logo} alt="Logo" className="sidebar-brand-full" />)}
          </CSidebarBrand>
          {!uiSlice.sidebarNarrow && (
            <CHeaderToggler onClick={() => dispatch(setUi({ sidebarNarrow: true }))}>
              <CIcon icon={cilMenu} size="xxl" className="header-toggler-icon" />
            </CHeaderToggler>
          )}
        </div>

        <CCloseButton
          className="d-lg-none"
          dark
          onClick={() => dispatch({ type: 'set', sidebarShow: false })}
        />
      </CSidebarHeader>
      <AppSidebarNav items={navigation} />

      {/* <CSidebarFooter className="border-top d-none d-lg-flex">
        <CSidebarToggler
          onClick={() => dispatch({ type: 'set', sidebarUnfoldable: !unfoldable })}
        />
      </CSidebarFooter> */}
    </CSidebar>
  )
}

export default React.memo(AppSidebar)
