import React, { createContext, FC, useEffect, useState } from 'react';
import {
    Box,
    Typography,
    Tabs,
    Tab,
    IconButton,
} from '@mui/material';
import './styles.scss';
import ProfileInfo from './profile-info';
import BuddyAudio from './buddy-audio';
import { getAccountProfile, getPodcasterProfile } from '@/core/services/account/account.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';
import Loading from '@/views/components/common/loading';




interface ProfileViewProps { }
interface ProfileViewContextProps {
    profile: any;
    refreshProfile: () => Promise<void>;
}

export const ProfileViewContext = createContext<ProfileViewContextProps | null>(null);

const Profile: FC<ProfileViewProps> = () => {
    const [activeTab, setActiveTab] = useState("personal-info");
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [profile, setProfile] = useState<any>(null);
    const authSlice = useSelector((state: RootState) => state.auth);

    const fetchProfileDetail = async () => {
        setIsLoading(true);
        try {
            const res = await getPodcasterProfile(loginRequiredAxiosInstance, authSlice.user.Id);
            console.log("Fetched account detail:", res.data.PodcasterAccount);
            if (res.success && res.data) {
                setProfile(res.data.PodcasterAccount);
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch detail:', error);
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        fetchProfileDetail()
    }, []);
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
        <>
            {isLoading ? (
                <div className="flex justify-center items-center h-screen">
                    <Loading />
                </div>
            ) : (
                <ProfileViewContext.Provider value={{ refreshProfile: fetchProfileDetail, profile }}>
                    <div className="profile-page">
                        <Typography variant="h4" className="profile-page__title">
                            Profile
                        </Typography>
                        <Box className="profile-tabs">
                            <Tabs
                                value={activeTab}
                                onChange={(_, v) => handleTabChange(v as string)}
                                aria-label="Profile tabs"
                                textColor="inherit"
                                indicatorColor="primary"
                            >
                                <Tab label="Personal Info" value="personal-info" />
                                <Tab label="Buddy Audio" value="buddy-audio" />
                            </Tabs>

                            <TabPanel value={activeTab} index="personal-info">
                                <ProfileInfo loading={isLoading} />
                            </TabPanel>

                            <TabPanel value={activeTab} index="buddy-audio">
                                <BuddyAudio />
                            </TabPanel>
                        </Box>
                    </div>
                </ProfileViewContext.Provider>
            )}

        </>
    );
};

export default Profile;
