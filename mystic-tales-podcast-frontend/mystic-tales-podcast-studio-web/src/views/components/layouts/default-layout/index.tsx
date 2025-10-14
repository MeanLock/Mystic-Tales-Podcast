import React, { createContext, useEffect, useState } from 'react'
import './styles.scss';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import type { RootState } from '../../../../redux/rootReducer';
import { clearAuthToken } from '../../../../redux/auth/authSlice';
import { JwtUtil } from '../../../../core/utils/jwt.util';
import { DefaultLayoutHeader } from './DefaultLayoutHeader';
import { DefaultLayoutContent } from './DefaultLayoutContent';
import { DefaultLayoutFooter } from './DefaultLayoutFooter';
import { _nonLoginNav, getRoleNavItems } from '../../../../router/_roleNav';
import { Box, Container } from '@mui/material';


interface DefaultLayoutContextProps {
  isAdmin: boolean;
  isLogin: boolean;
}

export const DefaultLayoutContext = createContext<DefaultLayoutContextProps>({
  isAdmin: false,
  isLogin: false
});

const DefaultLayout = () => {

  // HOOKS
  const dispatch = useDispatch();
  const navigate = useNavigate();

  // STATES
  const [member, setMember] = useState<any>(null);
  const [navItems, setNavItems] = useState<{
    label: string,
    path: string
  }[]>([]);

  // REDUX
  const auth = useSelector((state: RootState) => state.auth);



  useEffect(() => {
    if (!auth.token) {
      setNavItems(_nonLoginNav);

      dispatch(clearAuthToken())
      setMember(null);


    } else if (auth.token && JwtUtil.isTokenValid(auth.token)) {
      const member = auth.user;
      setMember(member);
      const navItems = getRoleNavItems(auth.user.isAdmin == true ? 1 : 0);
      setNavItems(navItems);
    } else {
      dispatch(clearAuthToken())
      setMember(null);
    }
  }, [auth, navigate]);



  return (
    <DefaultLayoutContext.Provider value={{
      isAdmin: member && member.isAdmin,
      isLogin: member ? true : false
    }}>
      <Box className={"default-layout"} display={'flex'} flexDirection='column' width={'100%'} height={'100vh'}>
        <DefaultLayoutHeader navItems={navItems} />
        <div className="wrapper d-flex flex-column min-vh-100">
          <div className="body flex-grow-1">
            <Container maxWidth="xl" className="default-layout-container" sx={{ height: '100vh', width: '100%' }}>
              <DefaultLayoutContent />
            </Container>
          </div>
          <DefaultLayoutFooter />
        </div>
      </Box>
    </DefaultLayoutContext.Provider>

  )
}

export default DefaultLayout
