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
import ProfileInfo from './profile-info';
import BuddyAudio from './buddy-audio';


export const mockAccount = {
  Id: 1,
  Email: "alice.smith@example.com",
  Role: {
    Id: 2,
    Name: "Podcaster"
  },
  FullName: "Alice Smith",
  Dob: "1995-06-12",
  Gender: "Female",
  Address: "123 Main Street, Los Angeles, CA",
  Phone: "+1 202 555 0123",
  Balance: 245.75,
  MainImageFileKey: "user_images/alice_main.jpg",
  IsVerified: true,
  GoogleId: "google-acc-923842",
  PodcastListenSlot: 50,
  ViolationPoint: 2,
  ViolationLevel: 1,
  LastViolationPointChanged: "2025-09-12T09:18:46.276Z",
  LastViolationLevelChanged: "2025-09-12T09:18:46.276Z",
  LastPodcastListenSlotChanged: "2025-10-20T09:18:46.276Z",
  DeactivatedAt: "null",
  CreatedAt: "2024-04-21T14:03:11.102Z",
  UpdatedAt: "2025-10-25T09:18:46.276Z",
  PodcasterProfile: {
    AccountId: 1,
    Name: "Alice’s Sound Studio",
    Description: "Podcast channel sharing stories about music, emotions, and creativity.",
    AverageRating: 4.7,
    RatingCount: 315,
    TotalFollow: 8920,
    ListenCount: 123450,
    CommitmentDocumentFileKey: "docs/alice_commitment.pdf",
    BuddyAudioFileKey: "audios/alice_intro_sample.mp3",
    OwnedBookingStorageSize: 2048,
    UsedBookingStorageSize: 785,
    IsVerified: true,
    CreatedAt: "2024-04-21T14:03:11.102Z",
    UpdatedAt: "2025-10-25T09:18:46.276Z"
  }
};

interface ProfileViewProps { }
interface ProfileViewContextProps {
    handleDataChange: () => void;
    profile: any;
}

export const ProfileViewContext = createContext<ProfileViewContextProps | null>(null);

const Profile: FC<ProfileViewProps> = () => {
    const [activeTab, setActiveTab] = useState("personal-info");
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [profile, setProfile] = useState<any>(null);
    const handleDataChange = async () => {
        setIsLoading(false);
        setProfile(mockAccount);
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
                {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
            </div>
        );
    }

    return (
        <ProfileViewContext.Provider value={{ handleDataChange: handleDataChange, profile: profile }}>
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
                        <ProfileInfo />
                    </TabPanel>

                    <TabPanel value={activeTab} index="buddy-audio">
                        <BuddyAudio />
                    </TabPanel>
                </Box>
            </div>
        </ProfileViewContext.Provider>

    );
};

export default Profile;
