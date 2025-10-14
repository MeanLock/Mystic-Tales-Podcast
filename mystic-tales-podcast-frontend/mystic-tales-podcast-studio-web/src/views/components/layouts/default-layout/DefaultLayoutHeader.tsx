import { useEffect, useState, type FC } from 'react';
import { alpha, styled } from '@mui/material/styles';
import Box from '@mui/material/Box';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Button from '@mui/material/Button';
import Container from '@mui/material/Container';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { clearAuthToken } from '../../../../redux/auth/authSlice';
import type { RootState } from '../../../../redux/rootReducer';
import { JwtUtil } from '../../../../core/utils/jwt.util';
import { FacebookIcon } from '../../common/mui-ui/MuiUiCustomIcons';

const StyledToolbar = styled(Toolbar)(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  flexShrink: 0,
  borderRadius: `calc(${theme.shape.borderRadius}px + 8px)`,
  backdropFilter: 'blur(24px)',
  border: '1px solid',
  borderColor: (theme.vars || theme).palette.divider,
  backgroundColor: theme.vars
    ? `rgba(${theme.vars.palette.background.defaultChannel} / 0.4)`
    : alpha(theme.palette.background.default, 0.4),
  boxShadow: (theme.vars || theme).shadows[1],
  padding: '8px 12px',
}));

function useQuery() {
  return new URLSearchParams(useLocation().search);
}

interface DefaultLayoutHeaderProps {
  navItems: {
    label: string,
    path: string
  }[]
}

export const DefaultLayoutHeader: FC<DefaultLayoutHeaderProps> = ({ navItems }) => {
  const [open, setOpen] = useState(false);

  const toggleDrawer = (newOpen: boolean) => () => {
    setOpen(newOpen);
  };

  // HOOKS
  const query = useQuery();
  const navigate = useNavigate();

  // STATES
  const [value, setValue] = useState('');
  const [isLogin, setIsLogin] = useState(false);


  // REDUX
  const dispatch = useDispatch();
  const auth = useSelector((state: RootState) => state.auth);

  useEffect(() => {
    setValue(window.location.pathname)
  }, [query]);



  useEffect(() => {
    if (auth.token && JwtUtil.isTokenValid(auth.token)) {
      setIsLogin(true)
    } else {
      setIsLogin(false)
    }
  }, [auth])

  const handleLogout = () => {
    dispatch(clearAuthToken())
    navigate('/login')
  }

  return (
    <AppBar
      position="fixed"
      enableColorOnDark
      sx={{
        boxShadow: 0,
        bgcolor: 'transparent',
        backgroundImage: 'none',
        mt: 'calc(var(--template-frame-height, 0px) + 28px)',
      }}
    >
      <Container maxWidth="xl">
        <StyledToolbar variant="dense" disableGutters>
          <Box sx={{ flexGrow: 1, display: 'flex', alignItems: 'center', px: 0 }}>
            <FacebookIcon />
            <Box sx={{ display: { xs: 'none', md: 'flex' }, paddingLeft: 3 }}>
              {navItems ? navItems.map((item, index) => {
                return (
                  <Button
                    variant={value === item.path ? 'contained' : "text"}
                    color={value === item.path ? 'primary' : "info"}
                    size="small"
                    key={index}
                    defaultValue={item.path}
                    component={Link}
                    to={item.path}
                  >
                    {item.label}
                  </Button>
                )
              }) : null}

            </Box>
          </Box>


          {isLogin ?
            <Box
              sx={{
                display: { xs: 'none', md: 'flex' },
                gap: 1,
                alignItems: 'center',
              }}
            >
              <Button
                color="error"
                variant="contained"
                size="small"
                onClick={handleLogout}>
                Log out
              </Button>
            </Box>
            :
            <Box
              sx={{
                display: { xs: 'none', md: 'flex' },
                gap: 1,
                alignItems: 'center',
              }}
            >
              <Button color="primary" variant="outlined" size="small" component={Link} to="/login">
                Log in
              </Button>
              <Button color="success" variant="contained" size="small" component={Link} to="/register">
                Register
              </Button>
            </Box>
          }
        </StyledToolbar>
      </Container>
    </AppBar>
  );
}
