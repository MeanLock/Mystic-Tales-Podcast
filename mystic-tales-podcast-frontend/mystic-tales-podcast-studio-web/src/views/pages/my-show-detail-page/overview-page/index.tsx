import React, { useState } from 'react';
import {
    Box,
    Typography,
    Tabs,
    Tab,
} from '@mui/material';
import ShowInfo from './show-info';
import ShowTrailer from './show-trailer';
import Loading from '@/views/components/common/loading';

const ShowOverview = () => {
    const [activeTab, setActiveTab] = useState("show-info");
    const handleTabChange = (tabKey: string | null) => {
        if (tabKey) {
            setActiveTab(tabKey)
        }
    }
    function TabPanel(props: { children?: React.ReactNode; value: string; index: string }) {
        const { children, value, index } = props;
        return (
            <div role="tabpanel" hidden={value !== index}>
                {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
            </div>
        );
    }

    return (
        <div className="show-overview-page">
            <Typography variant="h4" className="show-overview-page__title">
                Overview
            </Typography>
            <Box className="detail-tabs">
                <Tabs
                    value={activeTab}
                    onChange={(_, v) => handleTabChange(v as string)}
                    aria-label="Show detail tabs"
                    textColor="inherit"
                    indicatorColor="primary"
                >
                    <Tab label="Show Detail" value="show-info" />
                    <Tab label="Trailer Audio" value="show-trailer" />
                </Tabs>

                <TabPanel value={activeTab} index="show-info">
                    <ShowInfo />
                </TabPanel>

                <TabPanel value={activeTab} index="show-trailer">
                    <ShowTrailer />
                </TabPanel>
            </Box>

        </div>
    );
};

export default ShowOverview;
