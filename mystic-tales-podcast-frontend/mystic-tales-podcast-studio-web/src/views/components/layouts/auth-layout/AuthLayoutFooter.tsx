import { type FC } from 'react';
import Box from '@mui/material/Box';

interface AuthLayoutFooterProps { }

export const AuthLayoutFooter: FC<AuthLayoutFooterProps> = ({ }) => {
  return (
    <Box className={"auth-layout-footer"}>
      <h1>Auth layout footer</h1>
    </Box>
  );
}
