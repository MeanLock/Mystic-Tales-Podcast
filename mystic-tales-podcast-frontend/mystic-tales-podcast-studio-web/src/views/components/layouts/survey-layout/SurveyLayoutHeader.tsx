import { type FC } from 'react';
import Box from '@mui/material/Box';

interface SurveyLayoutHeaderProps { }

export const SurveyLayoutHeader: FC<SurveyLayoutHeaderProps> = ({ }) => {
  return (
    <Box className={"survey-layout-header"}>
      <h1>Survey layout header</h1>
    </Box>
  );
}
