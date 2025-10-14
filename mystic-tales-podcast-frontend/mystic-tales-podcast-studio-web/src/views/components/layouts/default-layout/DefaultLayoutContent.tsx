import { Suspense, useContext, useEffect, useState } from 'react';
import Avatar from '@mui/material/Avatar';
import AvatarGroup from '@mui/material/AvatarGroup';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';
import FormControl from '@mui/material/FormControl';
import InputAdornment from '@mui/material/InputAdornment';
import OutlinedInput from '@mui/material/OutlinedInput';
import { styled } from '@mui/material/styles';
import SearchRoundedIcon from '@mui/icons-material/SearchRounded';
import RssFeedRoundedIcon from '@mui/icons-material/RssFeedRounded';
import { useDispatch, useSelector } from 'react-redux';
import { CircularProgress, Container } from '@mui/material';
import type { RootState } from '../../../../redux/rootReducer';
import { DefaultLayoutContext } from '.';
import { JwtUtil } from '../../../../core/utils/jwt.util';
import { Outlet } from 'react-router-dom';




export function DefaultLayoutContent() {


  // HOOKS
  const dispatch = useDispatch();

  // STATES
  const [ready, setReady] = useState(false);
  const [focusedCardIndex, setFocusedCardIndex] = useState<number | null>(
    null,
  );

  // REDUX
  const auth = useSelector((state: RootState) => state.auth);

  // CONTEXT
  const context: any = useContext(DefaultLayoutContext);

  useEffect(() => {
    if (auth.token && JwtUtil.isTokenValid(auth.token)) {
      setReady(true)
    } else {
      setReady(false)
    }
  }, [auth]);


  const handleFocus = (index: number) => {
    setFocusedCardIndex(index);
  };

  const handleBlur = () => {
    setFocusedCardIndex(null);
  };

  const handleClick = () => {
    console.info('You clicked the filter chip.');
  };


  return (
    <Box className={"default-layout-content"} sx={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
        <Suspense fallback={
          <Box sx={{ display: 'flex', height: '100vh', width: '100vh !important', position: "absolute", alignItems: 'center', justifyContent: 'end' }}>
            <CircularProgress color="primary" />
          </Box>
        }>
          <Outlet />
        </Suspense>
    </Box>
  );
}
