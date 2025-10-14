import { Suspense, type FC } from 'react';
import Box from '@mui/material/Box';
import { CircularProgress } from '@mui/material';
import { Outlet } from 'react-router-dom';

interface SurveyLayoutContentProps { }

export const SurveyLayoutContent: FC<SurveyLayoutContentProps> = ({ }) => {
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
