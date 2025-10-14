import { type FC } from 'react';
import Box from '@mui/material/Box';

interface SurveyLayoutFooterProps { }

export const SurveyLayoutFooter: FC<SurveyLayoutFooterProps> = ({ }) => {
  return (
    <Box className={"survey-layout-footer"}>
      <h1>Survey layout footer</h1>
    </Box>
  );
}
