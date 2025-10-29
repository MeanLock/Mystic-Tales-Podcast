import React, { createContext, FC, useEffect, useState } from 'react';
import {
    Box,
    Typography,
    Tabs,
    Tab,
    IconButton,
} from '@mui/material';
import './styles.scss';
import { ArrowBack } from '@mui/icons-material';
import EpisodeInfo from './episode-info';
import EpisodeAudio from './epsiode-audio';

export const mockData = {
    Episode: {
        Id: "ep001",
        Name: "The Sound of Innovation",
        Description: "Exploring how AI transforms modern audio production.",
        ExplicitContent: false,
        ReleaseDate: "2025-10-22T06:45:10.073Z",
        EpisodeOrder: 1,
        IsReleased: true,
        MainImageFileKey: 'https://i.pinimg.com/736x/e8/c4/d3/e8c4d39d44c8945d62cd6f35e45959df.jpg',
        AudioFileKey: "/audio/ep001.mp3",
        AudioFileSize: 10485760, // 10 MB
        AudioLength: 1320, // 22 minutes
        PodcastEpisodeSubscriptionType: {
            Id: 2,
            Name: "subscriber-only",
        },
        PodcastShow: {
            Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            Name: "AI & Sound Design",
            MainImageFileKey: "/images/shows/show001.jpg",
        },
        Hashtags: [
            { Id: 1, Name: "#AI" },
            { Id: 2, Name: "#AudioTech" },
        ],
        SeasonNumber: 1,
        TotalSave: 254,
        ListenCount: 1820,
        IsAudioPublishable: true,
        TakenDownReason: null,
        CreatedAt: "2025-10-22T06:45:10.073Z",
        UpdatedAt: "2025-10-22T06:45:10.073Z",
        CurrentStatus: {
            Id: 1,
            Name: "Published",
        },
        Podcaster: {
            Id: 10,
            FullName: "Jane Doe",
            Email: "jane.doe@soundary.ai",
            MainImageFileKey: "/images/users/jane.jpg",
        },
    },
};
interface EpisodeDetailViewProps { }
interface EpisodeDetailViewContextProps {
    handleDataChange: () => void;
    episodeDetail: any;
}

export const EpisodeDetailViewContext = createContext<EpisodeDetailViewContextProps | null>(null);

const EpisodeDetail: FC<EpisodeDetailViewProps> = () => {
    const [activeTab, setActiveTab] = useState("episode-info");
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [episodeDetail, setEpisodeDetail] = useState<any>(null);
    const handleDataChange = async () => {
        setIsLoading(false);
        setEpisodeDetail(mockData.Episode);
    }

    useEffect(() => {
        handleDataChange()
    }, [])
    const handleTabChange = (tabKey: string | null) => {
        if (tabKey) {
            setActiveTab(tabKey)
        }
    }
    function TabPanel(props: { children?: React.ReactNode; value: string; index: string }) {
        const { children, value, index } = props;
        return (
            <div role="tabpanel" hidden={value !== index}>
                {value === index && <Box className="detail-tab-panel">{children}</Box>}
            </div>
        );
    }

    return (
        <EpisodeDetailViewContext.Provider value={{ handleDataChange: handleDataChange, episodeDetail: episodeDetail }}>
            <div className="episode-detail-page">
                <Box className="flex justify-start align-center" sx={{ padding: '10px 40px 0 48px' }}>
                    <IconButton className="episode-detail-page__back-button" onClick={() => window.history.back()}>
                        <ArrowBack sx={{ fontSize: '0.8rem', marginRight: '6px' }} /> Back
                    </IconButton>
                </Box>
                <Typography variant="h4" className="episode-detail-page__title">
                    Episode Detail
                </Typography>
                <Box className="detail-tabs">
                    <Tabs
                        value={activeTab}
                        onChange={(_, v) => handleTabChange(v as string)}
                        aria-label="Show detail tabs"
                        textColor="inherit"
                        indicatorColor="primary"
                    >
                        <Tab label="Informations" value="episode-info" />
                        <Tab label="Audio" value="episode-audio" />
                    </Tabs>

                    <TabPanel value={activeTab} index="episode-info">
                        <EpisodeInfo />
                    </TabPanel>

                    <TabPanel value={activeTab} index="episode-audio">
                        <EpisodeAudio />
                    </TabPanel>
                </Box>
            </div>
        </EpisodeDetailViewContext.Provider>

    );
};

export default EpisodeDetail;
