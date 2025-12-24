import { useEffect, useState, type FC } from 'react';
import { Box, Paper, TableBody, Table as muiTable, TableContainer, TableRow, TableCell, TextField, IconButton, Card, CardMedia, CardContent, AvatarGroup, Avatar, CircularProgress, Grid } from '@mui/material';
import './styles.scss'


interface SurveyPageProps {}

const SurveyPage : FC<SurveyPageProps>=() => {



    return (
        <Box className="PerfumesPage" sx={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
            <h1>Survey page</h1>

        </Box>
    );
}


export default SurveyPage;