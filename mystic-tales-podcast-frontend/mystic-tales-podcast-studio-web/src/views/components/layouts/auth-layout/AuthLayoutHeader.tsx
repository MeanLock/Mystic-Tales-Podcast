import { type FC } from 'react';
import Box from '@mui/material/Box';

interface AuthLayoutHeaderProps { }

export const AuthLayoutHeader: FC<AuthLayoutHeaderProps> = ({ }) => {
  return (
    <Box className={"auth-layout-header"}>
      <h1>Auth layout header</h1>
    </Box>
  );
}
