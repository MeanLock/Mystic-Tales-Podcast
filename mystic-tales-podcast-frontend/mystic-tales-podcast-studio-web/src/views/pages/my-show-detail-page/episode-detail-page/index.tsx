import React, { createContext, FC, use, useEffect, useState } from 'react';
import {
    Box,
    Typography,
    Tabs,
    Tab,
    IconButton,
    Button,
    TextField,
} from '@mui/material';
import './styles.scss';
import { ArrowBack } from '@mui/icons-material';
import EpisodeInfo from './episode-info';
import EpisodeLicense from './episode-license';
import { X } from 'lucide-react';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { deleteEpisode, discardPublishEpisode, getEpisodeDetail, publishEpisode, requestPublish } from '@/core/services/episode/episode.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { useNavigate, useParams } from 'react-router-dom';
import { Episode } from '@/core/types/episode';
import { confirmAlert } from '@/core/utils/alert.util';
import { toast } from 'react-toastify';
import Modal_Button from '@/views/components/common/modal/ModalButton';
import Loading from '@/views/components/common/loading';
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';
import EpisodeAudio from './epsiode-audio';


interface EpisodeDetailViewProps { }
interface EpisodeDetailViewContextProps {
    episodeDetail: Episode | null;
    refreshEpisode: () => Promise<void>;
    authSlice?: RootState['auth'];
}

export const EpisodeDetailViewContext = createContext<EpisodeDetailViewContextProps | null>(null);

const EpisodeDetail: FC<EpisodeDetailViewProps> = () => {
    const { episodeId } = useParams<{ episodeId: string }>();
    const { id } = useParams<{ id: string }>();
    const authSlice = useSelector((state: RootState) => state.auth);
    const [activeTab, setActiveTab] = useState("episode-info");
    const [episodeDetail, setEpisodeDetail] = useState<Episode | null>(null);
    const [releaseDate, setReleaseDate] = useState<string>(() => {
        const today = new Date();
        return today.toISOString().split('T')[0];
    });
    const [isDeleting, setIsDeleting] = useState<boolean>(false);
    const [isRequestingPublish, setIsRequestingPublish] = useState<boolean>(false);
    const [isPublishing, setIsPublishing] = useState<boolean>(false);
    const [loading, setLoading] = useState<boolean>(false);
    const navigate = useNavigate();


    const { startPolling } = useSagaPolling({
        timeoutSeconds: 120,
        intervalSeconds: 1,
    })

    const handleTabChange = (tabKey: string | null) => {
        if (tabKey) {
            setActiveTab(tabKey)
        }
    }

    function TabPanel(props: { children?: React.ReactNode; value: string; index: string }) {
        const { children, value, index } = props;
        return (
            <div role="tabpanel" className='!p-0' hidden={value !== index}>
                {value === index && (
                    <div style={{ padding: 0, margin: 0 }}>
                        {children}
                    </div>
                )}
            </div>
        );
    }

    const fetchEpisodeDetail = async () => {
        setLoading(true);
        try {
            const res = await getEpisodeDetail(loginRequiredAxiosInstance, episodeId);
            console.log("Fetched episode detail:", res.data.Episode);
            if (res.success && res.data) {
                setEpisodeDetail(res.data.Episode);
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch show detail:', error);
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        fetchEpisodeDetail()
    }, []);

    const handleDelete = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        const alert = await confirmAlert("Are you sure to DELETE this Episode?");
        if (!alert.isConfirmed) return;
        try {
            setIsDeleting(true);
            const res = await deleteEpisode(loginRequiredAxiosInstance, episodeId);
            const sagaId = res?.data.SagaInstanceId
            if (!res.success && res.message.content) {
                toast.error(res.message.content)
                return
            }
            if (!sagaId) {
                toast.error(`Episode deletion failed, please try again.`)
                return
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: () => {
                    toast.success(`Episode deleted successfully.`)
                    navigate(`/show/${id}/episode`);
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error deleting episode");
        } finally {
            setIsDeleting(false);
        }
    };
    const handleRequestPublish = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        if (episodeDetail.AudioFileKey === null || episodeDetail.AudioFileKey === "") {
            toast.warning("Please upload audio file before requesting to publish.");
            return
        }
        if (episodeDetail.IsAudioPublishable === false) {
            toast.warning("Your episode is non-publishable due to audio content issues, please update a new audio file");
            return
        }
        const alert = await confirmAlert("Request to publish will take a few minutes to verify your audio, are you sure?");
        if (!alert.isConfirmed) return;
        try {
            setIsRequestingPublish(true);
            const res = await requestPublish(loginRequiredAxiosInstance, episodeId);
            const sagaId = res?.data.SagaInstanceId
            if (!res.success && res.message.content) {
                toast.error(res.message.content)
                return
            }
            if (!sagaId) {
                toast.error(`Publish request failed, please try again.`)
                return
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    toast.success(`Request to publish successfully, please wait for processing audio.`)
                    await fetchEpisodeDetail();
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error publishing episode");
        } finally {
            setIsRequestingPublish(false);
        }
    };

    const handlePublish = async (isPublish: boolean) => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        if (!isPublish) {
            const alert = await confirmAlert("Are you sure to unpublish this episode?");
            if (!alert.isConfirmed) return;
        }
        try {
            const payload = isPublish ? { EpisodePublishInfo: { ReleaseDate: releaseDate } } : { EpisodePublishInfo: { ReleaseDate: "" } };
            setIsPublishing(true);
            const res = await publishEpisode(loginRequiredAxiosInstance, episodeId, isPublish, payload);
            const sagaId = res?.data?.SagaInstanceId
            if (!res.success && res.message.content) {
                toast.error(res.message.content)
                return
            }
            if (!sagaId) {
                toast.error(`Episode ${isPublish ? 'published' : 'unpublished'} failed, please try again.`)
                return
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    toast.success(`Episode ${isPublish ? 'published' : 'unpublished'} successfully.`)
                    await new Promise((r) => setTimeout(r, 1000));
                    navigate(0);
                },
                onFailure: (err) => {
                    if (err.includes("has been removed")) return toast.error("Show has been removed. Cannot update episode.");
                    toast.error(err || "Saga failed!");
                },
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error publishing episode");
        } finally {
            setIsPublishing(false);
        }
    };

    const handleDiscardPublish = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        const alert = await confirmAlert(" Are you sure to discard publish request for this episode?");
        if (!alert.isConfirmed) return;

        try {
            setIsPublishing(true);
            const res = await discardPublishEpisode(loginRequiredAxiosInstance, episodeId);
            const sagaId = res?.data?.SagaInstanceId
            if (!res.success && res.message.content) {
                toast.error(res.message.content)
                return
            }
            if (!sagaId) {
                toast.error(`Discard publish request failed, please try again.`)
                return
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    toast.success(`Discard publish request successfully.`)
                    fetchEpisodeDetail();
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error publishing episode");
        } finally {
            setIsPublishing(false);
        }
    };



    return (
        <>
            {loading ? (
                <div className="flex justify-center items-center h-screen">
                    <Loading />
                </div>
            ) : (
                <EpisodeDetailViewContext.Provider
                    value={{
                        episodeDetail,
                        refreshEpisode: fetchEpisodeDetail,
                        authSlice,
                    }}
                >
                    <div className="episode-detail-page">
                        <Box className="flex justify-start align-center" sx={{ padding: '10px 40px 0 48px' }}>
                            <IconButton className="episode-detail-page__back-button" onClick={() => window.history.back()}>
                                <ArrowBack sx={{ fontSize: '0.8rem', marginRight: '6px' }} /> Back
                            </IconButton>
                        </Box>

                        <div className="flex justify-between items-center pt-[5px] px-[48px]" >
                            <Typography variant="h4" className="episode-detail-page__title">
                                Episode Detail
                            </Typography>
                            {episodeDetail && episodeDetail?.CurrentStatus?.Id !== 7 && (
                                <div className='flex gap-2 '>

                                    <Button
                                        className="episode-info-page__action-btn episode-info-page__action-btn--remove"
                                        variant="outlined"
                                        disabled={isDeleting || isRequestingPublish || isPublishing}
                                        startIcon={<X size={15} />}
                                        onClick={() => handleDelete()}
                                    >
                                        {isDeleting ? 'Deleting...' : 'Delete'}
                                    </Button>
                                    {episodeDetail?.CurrentStatus?.Id !== 8 &&
                                        (
                                            <>
                                                {episodeDetail?.CurrentStatus?.Id === 1 &&
                                                    (
                                                        <Button
                                                            className="episode-info-page__action-btn episode-info-page__action-btn--unpublish"
                                                            variant="outlined"
                                                            disabled={isDeleting || isRequestingPublish}
                                                            onClick={() => handleRequestPublish()}
                                                        >
                                                            Request Publish
                                                        </Button>
                                                    )}
                                                {episodeDetail?.CurrentStatus?.Id === 4 && episodeDetail?.IsAudioPublishable &&
                                                    (
                                                        <Modal_Button
                                                            className="episode-info-page__action-btn episode-info-page__action-btn--save"
                                                            content="Publish"
                                                            variant="outlined"
                                                            size='sm'
                                                        >
                                                            <div className="flex flex-col gap-4 p-8 ">
                                                                <TextField
                                                                    label="Release Date"
                                                                    value={releaseDate}
                                                                    variant="standard"
                                                                    type='date'
                                                                    required
                                                                    onChange={(e) => setReleaseDate(e.target.value)}
                                                                    className="episode-info-page__input episode-info-page__input--name"
                                                                    inputProps={{
                                                                        min: new Date().toISOString().split('T')[0]
                                                                    }}
                                                                    sx={{
                                                                        '& .MuiOutlinedInput-root': {
                                                                            '& fieldset': { borderColor: '#999999 !important' },
                                                                            '&:hover fieldset': { borderColor: '#999999 !important' },
                                                                            '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                                                        },
                                                                    }}
                                                                />
                                                                <Button
                                                                    className="episode-info-page__action-btn episode-info-page__action-btn--save "
                                                                    onClick={() => handlePublish(true)}
                                                                    disabled={isPublishing || !releaseDate}
                                                                >
                                                                    {isPublishing ? "Publishing..." : "Confirm"}
                                                                </Button>
                                                            </div>
                                                        </Modal_Button>
                                                    )}
                                                {episodeDetail?.CurrentStatus?.Id === 5 &&
                                                    (
                                                        <Button
                                                            className="episode-info-page__action-btn episode-info-page__action-btn--unpublish"
                                                            variant="outlined"
                                                            disabled={isPublishing}
                                                            onClick={() => handlePublish(false)}
                                                        >
                                                            {isPublishing ? "Unpublishing..." : "Unpublish"}
                                                        </Button>
                                                    )}
                                                {episodeDetail?.CurrentStatus?.Id === 2 &&
                                                    (
                                                        <Button
                                                            className="episode-info-page__action-btn episode-info-page__action-btn--remove--contained"
                                                            variant="contained"
                                                            disabled={isPublishing}
                                                            onClick={() => handleDiscardPublish()}
                                                        >
                                                            {isPublishing ? "Discarding..." : "Discard Publish Request"}
                                                        </Button>
                                                    )}
                                            </>
                                        )}

                                </div>
                            )}
                        </div>
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
                                <Tab label="License" value="episode-license" />
                            </Tabs>

                            <TabPanel value={activeTab} index="episode-info">
                                <EpisodeInfo loading={loading} />
                            </TabPanel>

                            <TabPanel value={activeTab} index="episode-audio">
                                <EpisodeAudio />
                            </TabPanel>

                            <TabPanel value={activeTab} index="episode-license">
                                <EpisodeLicense />
                            </TabPanel>
                        </Box>

                    </div>
                </EpisodeDetailViewContext.Provider>
            )}

        </>
    );
};

export default EpisodeDetail;
