import React, { useEffect, useState, useRef, use, useContext, useMemo, useCallback } from 'react';
import {
    Box,
    Typography,
    TextField,
    Button,
    MenuItem,
    Chip,
    Card,
    CardMedia,
    CardContent,
    IconButton,
    InputAdornment,
} from '@mui/material';
import { Add, Explicit, PublishedWithChangesOutlined } from '@mui/icons-material';
import { useQuill } from 'react-quilljs';
 import 'quill/dist/quill.snow.css';
import Modal_Button from '@/views/components/common/modal/ModalButton';
import { useParams } from 'react-router-dom';
import { Episode } from '@/core/types/episode';
import { HashtagOption } from '@/core/types';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { getEpisodeDetail, updateEpisode } from '@/core/services/episode/episode.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import Loading from '@/views/components/common/loading';
import { isEqual } from 'lodash';
import { toast } from 'react-toastify';
import { fetchImage } from '@/core/utils/image.util';
import { createHashtag, getHashtags } from '@/core/services/misc/hashtag.service';
import { formatDate } from '@/core/utils/date.util';
import Image from '@/views/components/common/image';
import { EpisodeDetailViewContext } from '.';
import { getReviewSession } from '@/core/services/episode/review-session.service';
import * as signalR from "@microsoft/signalr";
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';
import LoadingProcessing from '@/views/components/common/loadingProcessing';

export const mockSubscriptionTypes = [
    { Id: 1, Name: "Free" },
    { Id: 2, Name: "Subscriber-Only" },
    { Id: 3, Name: "Bonus" },
    { Id: 4, Name: "Archive" },
];


interface EpisodeInfoProps {
    loading: boolean;
}

const EpisodeInfo: React.FC<EpisodeInfoProps> = ({ loading }) => {
    const ctx = useContext(EpisodeDetailViewContext);
    const episode = ctx?.episodeDetail;
    const refreshEpisode = ctx?.refreshEpisode;
    const authSlice = ctx?.authSlice;
    const { episodeId } = useParams<{ episodeId: string }>();
    const [episodeDetail, setEpisodeDetail] = useState<Episode | null>(null);
    const [originalEpisode, setOriginalEpisode] = useState<Episode | null>(null);
    const [selectedHashtags, setSelectedHashtags] = useState<HashtagOption[]>([]);
    const [hashtagInput, setHashtagInput] = useState<string>('');
    const [suggestions, setSuggestions] = useState<HashtagOption[]>([]);
    const [suggestLoading, setSuggestLoading] = useState<boolean>(false);
    const [openSuggest, setOpenSuggest] = useState<boolean>(false);
    const [previewImage, setPreviewImage] = useState<string>('');
    const [uploadImage, setUploadImage] = useState<string | null>(null);
    const [mainImageFile, setMainImageFile] = useState<File | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);
    // const [loading, setLoading] = useState<boolean>(false);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    
    const [reviewSession, setReviewSession] = useState<any>(null);
    const [reviewSessionLoading, setReviewSessionLoading] = useState<boolean>(false);

    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })

    const [formData, setFormData] = useState({
        Name: '',
        Description: '',
        ExplicitContent: false,
        PodcastEpisodeSubscriptionTypeId: 1,
        SeasonNumber: 1,
        EpisodeOrder: 1,
        HashtagIds: []
    });

    const { quill, quillRef } = useQuill({
        theme: 'snow',
        modules: {
            toolbar: [
                ['bold', 'italic', 'underline'],
                [{ 'align': '' }, { 'align': 'center' }, { 'align': 'right' }, { 'align': 'justify' }],
                [{ list: 'ordered' }, { list: 'bullet' }],
                ['link'],
                ['clean'],
            ],
        },
        placeholder: 'Add description...'
    });

    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const authSlice2 = useSelector((state: RootState) => state.auth);

    const token = authSlice2.token || "";
    const REST_API_BASE_URL = import.meta.env.VITE_BACKEND_URL;

    useEffect(() => {
        // Build connection
        console.log("Setting up SignalR connection...", token);
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${REST_API_BASE_URL}/api/podcast-service/hubs/podcast-content-notification`, {
                accessTokenFactory: () => {
                    return token;
                }
            })
            .withAutomaticReconnect()
            .build();

        connectionRef.current = connection;

        // Register events
        connection.on("PodcastEpisodeAudioProcessingCompletedNotification", async (data) => {
            console.log("Audio processing :", data);

            if (!data.IsSuccess) {
                console.error("Audio processing failed:", data.ErrorMessage);
                return;
            }

            //alert(`Audio processing completed for Podcast ID: ${data}`);
            await refreshEpisode?.();
        });

        // Start connection
        connection.start()
            .then(() => console.log("SignalR connected"))
            .catch(err => console.error("SignalR connection error:", err));

        // Cleanup
        return () => {
            connection.stop();
        };
    }, []);

    const fetchReviewSession = async () => {
        setReviewSessionLoading(true);
        try {
            const res = await getReviewSession(loginRequiredAxiosInstance, episodeId);
            console.log("Fetched review session:", res.data.ReviewSession);
            if (res.success && res.data) {
                setReviewSession(res.data.ReviewSession);

            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch show detail:', error);
        } finally {
            setReviewSessionLoading(false);
        }
    }

    useEffect(() => {
        if (!episode) return;
        setEpisodeDetail(episode);
        setOriginalEpisode(prev => prev ?? episode);
        setSelectedHashtags(episode.Hashtags.map((h: any) => ({ id: h.Id, name: h.Name })));
        setFormData(f => ({
            ...f,
            Name: episode.Name,
            Description: episode.Description || '',
            ExplicitContent: episode.ExplicitContent,
            PodcastEpisodeSubscriptionTypeId: episode.PodcastEpisodeSubscriptionType?.Id || 1,
            SeasonNumber: episode.SeasonNumber || 1,
            EpisodeOrder: episode.EpisodeOrder || 1,
            HashtagIds: episode.Hashtags?.map((h: any) => h.Id) || []
        }));
        if (episode.CurrentStatus.Id === 3) {
            fetchReviewSession();
        }
    }, [episode]);

    useEffect(() => {
        if (!quill) return;
        const serverHtml = episodeDetail?.Description ?? '';
        const currentHtml = quill.root.innerHTML;
        if (serverHtml !== currentHtml) {
            (quill.clipboard as any).dangerouslyPasteHTML(serverHtml);
        }
    }, [quill, episodeDetail?.Description]);

    useEffect(() => {
        if (!quill) return;
        const onTextChange = (_delta: any, _oldDelta: any, source: 'user' | 'api') => {
            if (source !== 'user') return;
            const htmlContent = quill.root.innerHTML;
            setEpisodeDetail(prev => {
                if (!prev || prev.Description === htmlContent) return prev;
                return { ...prev, Description: htmlContent };
            });
        };
        quill.on('text-change', onTextChange);
        return () => {
            quill.off?.('text-change', onTextChange);
        };
    }, [quill]);


    const nameChanged = useMemo(() => {
        if (!originalEpisode || !episodeDetail) return false;
        return originalEpisode.Name !== episodeDetail.Name;
    }, [originalEpisode, episodeDetail?.Name]);

    const orderChanged = useMemo(() => {
        if (!originalEpisode || !episodeDetail) return false;
        return originalEpisode.EpisodeOrder !== episodeDetail.EpisodeOrder;
    }, [originalEpisode, episodeDetail?.EpisodeOrder]);

    const seasonChanged = useMemo(() => {
        if (!originalEpisode || !episodeDetail) return false;
        return originalEpisode.SeasonNumber !== episodeDetail.SeasonNumber;
    }, [originalEpisode, episodeDetail?.SeasonNumber]);

    const explicitChanged = useMemo(() => {
        if (!originalEpisode || !episodeDetail) return false;
        return originalEpisode.ExplicitContent !== episodeDetail.ExplicitContent;
    }, [originalEpisode, episodeDetail?.ExplicitContent]);

    const subscriptionTypeChanged = useMemo(() => {
        if (!originalEpisode || !episodeDetail) return false;
        return originalEpisode.PodcastEpisodeSubscriptionType?.Id !== episodeDetail.PodcastEpisodeSubscriptionType?.Id;
    }, [originalEpisode, episodeDetail?.PodcastEpisodeSubscriptionType?.Id]);

    const descriptionChanged = useMemo(() => {
        if (!originalEpisode || !episodeDetail) return false;
        const norm = (s: string) => (s || '').trim();
        return norm(originalEpisode.Description || '') !== norm(episodeDetail.Description || '');
    }, [originalEpisode, episodeDetail?.Description]);

    const hashtagsChanged = useMemo(() => {
        if (!originalEpisode) return false;
        const originalIds = (originalEpisode.Hashtags || []).map(h => h.Id).sort();
        const currentIds = selectedHashtags.map(h => h.id).sort();
        return !isEqual(originalIds, currentIds);
    }, [originalEpisode, selectedHashtags]);

    const imageChanged = useMemo(() => !!mainImageFile, [mainImageFile]);

    const isDirty = useMemo(
        () => nameChanged || descriptionChanged || orderChanged || hashtagsChanged || imageChanged || seasonChanged || explicitChanged || subscriptionTypeChanged,
        [nameChanged, descriptionChanged, orderChanged, hashtagsChanged, imageChanged, seasonChanged, explicitChanged, subscriptionTypeChanged]
    );
    const handleSave = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        if (!episodeDetail || !isDirty) return;
        try {
            setIsSaving(true);
            const payload = {
                EpisodeUpdateInfo: {
                    Name: episodeDetail.Name,
                    Description: episodeDetail.Description || formData.Description || '',
                    ExplicitContent: episodeDetail.ExplicitContent,
                    PodcastEpisodeSubscriptionTypeId: episodeDetail.PodcastEpisodeSubscriptionType?.Id || 1,
                    SeasonNumber: episodeDetail.SeasonNumber || 1,
                    EpisodeOrder: episodeDetail.EpisodeOrder || 1,
                    HashtagIds: selectedHashtags.map(h => h.id),
                },
                MainImageFile: mainImageFile || undefined,
            };
            const res = await updateEpisode(loginRequiredAxiosInstance, String(episodeDetail.Id), payload);
            const sagaId = res?.data.SagaInstanceId
            if (!sagaId) {
                toast.error("Episode update failed, please try again.")
                return
            }
            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    toast.success('Episode updated successfully');
                    setOriginalEpisode(prev => ({
                        ...(prev || episodeDetail),
                        Name: episodeDetail.Name,
                        Description: episodeDetail.Description,
                        ExplicitContent: episodeDetail.ExplicitContent,
                        SeasonNumber: episodeDetail.SeasonNumber,
                        PodcastEpisodeSubscriptionType: { ...episodeDetail.PodcastEpisodeSubscriptionType, Id: formData.PodcastEpisodeSubscriptionTypeId },
                        EpisodeOrder: episodeDetail.EpisodeOrder,
                        Hashtags: selectedHashtags.map(h => ({ Id: h.id, Name: h.name }))
                    }));
                    setMainImageFile(null);
                    await refreshEpisode?.();

                },
                onFailure: (err) => {
                    if (err.includes("has been removed")) return toast.error("Show has been removed. Cannot update episode.");
                    toast.error(err || "Saga failed!");
                },
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {

            toast.error("Error updating episode");
        } finally {
            setIsSaving(false);
        }
    };

    const handleImageUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (file) {
             const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/svg+xml'];
                        if (!allowedTypes.includes(file.type)) {
                            toast.error('Invalid file type. Allowed: JPG, JPEG, PNG, GIF, WEBP, SVG');
                            return;
                        }
            
                        const maxSize = 3 * 1024 * 1024;
                        if (file.size > maxSize) {
                            toast.error('Image file size must be less than 3MB');
                            return;
                        }
            setMainImageFile(file);
        }
    };
    useEffect(() => {
        if (!mainImageFile) {
            setUploadImage(null)
            return
        }
        const url = URL.createObjectURL(mainImageFile)
        setUploadImage(url)
        return () => {
            URL.revokeObjectURL(url)
        }
    }, [mainImageFile])

    useEffect(() => {
        let alive = true;
        const load = async () => {
            if (uploadImage) {
                setPreviewImage(uploadImage);
                return;
            }
            const key = episodeDetail?.MainImageFileKey;
            if (key) {
                const url = await fetchImage(key);
                if (alive) setPreviewImage(url || '');
            } else {
                setPreviewImage('');
            }
        };
        load();
        return () => { alive = false; };
    }, [uploadImage, episodeDetail?.MainImageFileKey]);

    const handleAddHashtag = async (hashtagInput: string) => {
        try {
            const res = await createHashtag(loginRequiredAxiosInstance, { HashtagName: hashtagInput });
            if (res?.success) {
                const newHashtag: HashtagOption = {
                    id: res.data.NewHashtag.Id,
                    name: res.data.NewHashtag.Name
                };
                const exists = selectedHashtags.some(tag => tag.id === newHashtag.id || tag.name === newHashtag.name);
                if (!exists) {
                    setSelectedHashtags(prev => [...prev, newHashtag]);
                    setHashtagInput('');
                    setSuggestions([]);
                    setOpenSuggest(false);
                }
            }
        } catch (err) {
            toast.error("Error adding hashtag");
        }
    };
    const fetchHashtag = useCallback(async (keyword: string) => {
        if (!keyword.trim()) {
            setSuggestions([]);
            return;
        }
        try {
            setSuggestLoading(true);
            const res = await getHashtags(loginRequiredAxiosInstance, keyword.trim());
            if (res?.success) {
                const list = (res.data.HashtagList || []).map((h: any) => ({ id: h.Id, name: h.Name })) as HashtagOption[];
                setSuggestions(list);
            } else {
                setSuggestions([]);
            }
        } catch (err) {
            console.error('fetchHashtag error', err);
            setSuggestions([]);
        } finally {
            setSuggestLoading(false);
        }
    }, []);

    useEffect(() => {
        const t = setTimeout(() => {
            if (hashtagInput.trim().length > 0) {
                setOpenSuggest(true);
                fetchHashtag(hashtagInput);
            } else {
                setOpenSuggest(false);
                setSuggestions([]);
            }
        }, 300);
        return () => clearTimeout(t);
    }, [hashtagInput, fetchHashtag]);

    const handleHashtagKeyPress = (event: React.KeyboardEvent) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            handleAddHashtag(hashtagInput);
        }
    };

    const handleSelectSuggestion = (option: HashtagOption) => {
        const exists = selectedHashtags.some(tag => tag.id === option.id);
        if (!exists) {
            setSelectedHashtags(prev => [...prev, option]);
        }
        setHashtagInput('');
        setSuggestions([]);
        setOpenSuggest(false);
    };

    const handleRemoveHashtag = (tagToRemove: HashtagOption) => {
        setSelectedHashtags(prev => prev.filter(tag => tag.id !== tagToRemove.id));
    };

    if (loading || !episodeDetail || reviewSessionLoading) {
        return (
            <div className="flex justify-center items-center h-100">
                <Loading />
            </div>
        );
    }

    return (
        <div className="episode-info-page ">
            {episodeDetail.CurrentStatus?.Id === 7 && (
                <div
                    className="flex items-center  px-3 py-2 mt-6 mb-10"
                    style={{ width: "fit-content" }}
                >

                </div>
            )}
            {episodeDetail.TakenDownReason && (
                <div className="flex items-center gap-2 bg-red-100 border border-red-400  rounded px-3 py-2 mb-3 " style={{ width: "fit-content" }}>
                    <svg className="w-5 h-5 text-red-500 shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                    </svg>
                    <span className="text-xs text-red-700 font-medium">
                        <strong>Taken Down Reason:</strong> {episodeDetail.TakenDownReason}
                    </span>
                </div>
            )}



            {episodeDetail.CurrentStatus?.Id === 2 && (
                <div
                    className="flex items-center gap-2 bg-[#ffa72626] border border-[#ffa726] rounded-xs px-3 py-2 mt-6 mb-10"
                    style={{ width: "fit-content" }}
                >
                    <svg className="w-5 h-5 text-[#ffa726] shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                    </svg>
                    <span className="text-sm text-[#ffa726] font-medium">
                        <strong>Your episode audio is being verified, please wait for verification</strong>
                    </span>

                </div>
            )}
            {episodeDetail && episodeDetail.CurrentStatus?.Id === 3 && (
                <div className="flex justify-start flex-col gap-2 bg-red-100 border border-red-400  rounded px-3 py-2 mb-3 " style={{ width: "fit-content" }}>
                    <div className="flex items-center  gap-2">
                        <svg className="w-5 h-5 text-red-500 shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                        </svg>
                        <span className="text-sm text-red-700 font-medium">
                            <strong>Your episode is being required to edit</strong>
                        </span>
                    </div>
                    {reviewSession && (
                        <div className="flex flex-col justify-start items-start gap-2">
                            <p className="text-sm text-red-700 font-medium" style={{ fontFamily: 'inter' }}>Note : {reviewSession?.Note}</p>
                            <p className="text-sm text-red-700 font-medium" style={{ fontFamily: 'inter' }}>Deadline : {formatDate(reviewSession?.Deadline)}</p>
                            <p className="text-xs text-red-700 font-medium italic" style={{ fontFamily: 'inter' }}>Please upload new audio before deadline to avoid rejection</p>

                        </div>
                    )}
                </div>
            )}

            {episodeDetail.CurrentStatus?.Id !== 6 && episodeDetail.CurrentStatus?.Id !== 7 && episodeDetail.CurrentStatus?.Id !== 8 && (
                <div className="episode-info-page__actions mt-4">
                    {/* {(episodeDetail.CurrentStatus?.Id === 1 || episodeDetail.CurrentStatus?.Id === 2) && (
                        <Modal_Button
                            className="episode-info-page__action-btn episode-info-page__action-btn--unpublish"
                            content="Publish"
                            variant="outlined"
                            size='sm'
                        >
                            <div className="booking-detail__cancel-modal">
                                <label className="booking-detail__label">
                                    Release Date <span className="text-red-500">*</span>
                                </label>
                                <input
                                    type="date"
                                    className="booking-detail__input"
                                    placeholder="Enter release date"
                                    value={releaseDate}
                                    onChange={(e) => setReleaseDate(e.target.value)}
                                    onKeyPress={(e) => {
                                        if (e.key === 'Enter') {
                                            handlePublish(true)
                                        }
                                    }}
                                />
                                <button
                                    type="button"
                                    className="booking-detail__btn booking-detail__btn--resolve "
                                    onClick={() => handlePublish(true)}
                                    disabled={isPublishing}
                                >
                                    {isPublishing ? "Publishing..." : "Confirm"}
                                </button>
                            </div>
                        </Modal_Button>
                    )}

                    {episodeDetail.CurrentStatus?.Id === 3 && (
                        <Button
                            variant="outlined"
                            className="episode-info-page__action-btn episode-info-page__action-btn--unpublish"
                            onClick={() => handlePublish(false)}
                            disabled={isPublishing}
                        >
                            {isPublishing ? 'Unpublishing...' : 'Unpublish'}
                        </Button>
                    )}
                    {episodeDetail.PodcastChannel ? (
                        <Modal_Button
                            className="episode-info-page__action-btn episode-info-page__action-btn--unpublish"
                            content="Update Channel"
                            variant="outlined"
                            size='sm'
                        >
                            <AssignChannelModal onclose={() => { }} />
                        </Modal_Button>
                    ) : (
                        <Modal_Button
                            className="episode-info-page__action-btn episode-info-page__action-btn--unpublish"
                            content="Assign to Channel"
                            variant="outlined"
                            size='sm'
                        >
                            <AssignChannelModal onclose={() => { }} />
                        </Modal_Button>
                    )} */}

                    <Button
                        variant="outlined"
                        className="episode-info-page__action-btn episode-info-page__license-btn"
                        onClick={handleSave}
                        disabled={!isDirty || isSaving}

                    >
                        {isSaving ? 'Saving...' : 'Save'}
                    </Button>
                </div>
            )}

            {episodeDetail.CurrentStatus?.Id === 8 ? (
                <div className="flex justify-center items-center h-100">
              
                    <LoadingProcessing  />
                </div>
            ) : (
                <div className="episode-info-page__content">
                    <div className="episode-info-page__form">
                        <div className="episode-info-page__row">
                            <TextField
                                label="Name"
                                value={episodeDetail.Name}
                                variant="standard"
                                onChange={(e) => setEpisodeDetail({ ...episodeDetail, Name: e.target.value })}
                                className="episode-info-page__input episode-info-page__input--name"
                                sx={{
                                    '& .MuiOutlinedInput-root': {
                                        '& fieldset': { borderColor: '#999999 !important' },
                                        '&:hover fieldset': { borderColor: '#999999 !important' },
                                        '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                    },
                                }}
                            />

                            <TextField
                                id="filled-read-only-input"
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Status"
                                value={episodeDetail.CurrentStatus?.Name ?? ''}
                                className="episode-info-page__input episode-info-page__input--status"

                            />
                        </div>
                        <div className="episode-info-page__row">
                            <TextField
                                select
                                label="Subscription Type"
                                variant="standard"
                                value={episodeDetail.PodcastEpisodeSubscriptionType.Id ?? 1}
                                onChange={(e) => setEpisodeDetail({ ...episodeDetail, PodcastEpisodeSubscriptionType: { ...episodeDetail.PodcastEpisodeSubscriptionType, Id: e.target.value as unknown as number } })}
                                className="episode-info-page__select"
                            >
                                {mockSubscriptionTypes.map((type) => (
                                    <MenuItem
                                        key={type.Id}
                                        value={type.Id}
                                        sx={{
                                            '& .MuiPaper-root': { backgroundColor: '#77898e9d' },

                                        }}
                                    >
                                        {type.Name}
                                    </MenuItem>
                                ))}
                            </TextField>
                            <TextField
                                select
                                label="Explicit Content"
                                variant="standard"
                                value={episodeDetail.ExplicitContent ?? ''}
                                onChange={(e) => setEpisodeDetail({ ...episodeDetail, ExplicitContent: e.target.value === 'true' })}
                                className="episode-info-page__select"
                            >
                                <MenuItem value="true">True</MenuItem>
                                <MenuItem value="false">False</MenuItem>
                            </TextField>

                            <TextField
                                label="Season"
                                value={episodeDetail.SeasonNumber}
                                type="number"
                                variant="standard"
                                inputProps={{ min: 1 }}
                                onChange={(e) => {
                                    let val = e.target.value;
                                    if (val === '' || Number(val) < 1) {
                                        setEpisodeDetail({ ...episodeDetail, SeasonNumber: 1 });
                                    } else {
                                        setEpisodeDetail({ ...episodeDetail, SeasonNumber: Number(val) });
                                    }
                                }}
                                className="episode-info-page__input episode-info-page__input--number"
                                sx={{
                                    '& .MuiOutlinedInput-root': {
                                        '& fieldset': { borderColor: '#999999 !important' },
                                        '&:hover fieldset': { borderColor: '#999999 !important' },
                                        '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                    },
                                }}
                            />
                            <TextField
                                label="Episode Order"
                                value={episodeDetail.EpisodeOrder}
                                type="number"
                                variant="standard"
                                inputProps={{ min: 1 }}
                                onChange={(e) => {
                                    let val = e.target.value;
                                    if (val === '' || Number(val) < 1) {
                                        setEpisodeDetail({ ...episodeDetail, EpisodeOrder: 1 });
                                    } else {
                                        setEpisodeDetail({ ...episodeDetail, EpisodeOrder: Number(val) });
                                    }
                                }}
                                className="episode-info-page__input episode-info-page__input--number"
                                sx={{
                                    '& .MuiOutlinedInput-root': {
                                        '& fieldset': { borderColor: '#999999 !important' },
                                        '&:hover fieldset': { borderColor: '#999999 !important' },
                                        '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                    },
                                }}
                            />

                        </div>

                        <div className="episode-info-page__row">
                            <TextField
                                id="filled-read-only-input"
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Show"
                                value={episodeDetail.PodcastShow?.Name ?? ''}
                                className="episode-info-page__input episode-info-page__input--show"

                            />
                            <TextField
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Is Released"
                                value={episodeDetail.IsReleased ? 'Yes' : 'No'}
                                className="episode-info-page__input-small"

                            />
                        </div>

                        {/* Dates and Numbers Row */}
                        <div className="episode-info-page__row">
                            {episodeDetail.ReleaseDate != null && (
                                <TextField
                                    variant="filled"
                                    slotProps={{
                                        input: {
                                            readOnly: true,
                                        },
                                    }}
                                    label="Release Date"
                                    value={formatDate(episodeDetail.ReleaseDate)}
                                    className="episode-info-page__input-small"

                                />
                            )}
                            <TextField
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Created At"
                                value={formatDate(episodeDetail.CreatedAt)}
                                className="episode-info-page__input-small"

                            />
                            <TextField
                                id="filled-helperText"
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Updated At"
                                value={formatDate(episodeDetail.UpdatedAt)}
                                className="episode-info-page__input-small"

                            />
                            <TextField
                                id="filled-helperText"
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Total Save"
                                value={episodeDetail.TotalSave ?? 0}
                                className="episode-info-page__input-small"

                            />
                            <TextField
                                id="filled-helperText"
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Listen Count"
                                value={episodeDetail.ListenCount ?? 0}
                                className="episode-info-page__input-small"

                            />
                        </div>


                        <div className="episode-info-page__hashtags">
                            <div className="episode-info-page__hashtag-input ">
                                <TextField
                                    label="Add hashtag"
                                    value={hashtagInput}
                                    onChange={(e) => setHashtagInput(e.target.value)}
                                    onKeyPress={handleHashtagKeyPress}
                                    size="small"
                                    className="episode-info-page__hashtag-field"
                                    onFocus={() => {
                                        if (hashtagInput.trim()) setOpenSuggest(true);
                                    }}
                                    InputProps={{
                                        endAdornment: (
                                            <InputAdornment position="end">
                                                <IconButton
                                                    onClick={() => handleAddHashtag(hashtagInput)}
                                                    disabled={!hashtagInput.trim() || selectedHashtags.some(tag => tag.name === hashtagInput.trim())}
                                                    size="small"
                                                    sx={{ color: 'var(--primary-green)' }}
                                                >
                                                    <Add />
                                                </IconButton>
                                            </InputAdornment>
                                        ),
                                    }}
                                    sx={{
                                        '& .MuiOutlinedInput-root': {
                                            backgroundColor: '#2a2a2a',
                                            color: 'white',
                                            '& fieldset': { borderColor: '#444 !important' },
                                            '&:hover fieldset': { borderColor: '#666 !important' },
                                            '&.Mui-focused fieldset': { borderColor: 'var(--primary-green) !important' }
                                        },
                                        '& .MuiInputLabel-root': { color: '#888' },
                                        '& .MuiInputLabel-root.Mui-focused': { color: 'var(--primary-green)' }
                                    }}
                                />
                                {openSuggest && (suggestions.length > 0 || suggestLoading) && (
                                    <Box
                                        className="episode-info-page__hashtag-suggest"
                                        sx={{
                                            mt: 0.5,
                                            maxHeight: 200,
                                            overflowY: 'auto',
                                            border: '1px solid #444',
                                            borderRadius: 1,
                                            background: '#1f1f1f',
                                            boxShadow: '0 4px 12px rgba(0,0,0,0.5)'
                                        }}
                                    >
                                        {suggestLoading && (
                                            <Box sx={{ p: 1.5, color: '#aaa', fontSize: 13 }}>Searching…</Box>
                                        )}
                                        {!suggestLoading && suggestions.map((opt) => (
                                            <MenuItem
                                                key={opt.id}
                                                onClick={() => handleSelectSuggestion(opt)}
                                                sx={{ fontSize: 14 }}
                                            >
                                                #{opt.name}
                                            </MenuItem>
                                        ))}
                                    </Box>
                                )}
                            </div>
                            <div className="episode-info-page__hashtag-chips">
                                {selectedHashtags.map((tag, index) => (
                                    <Chip
                                        key={index}
                                        label={tag.name}
                                        onDelete={() => handleRemoveHashtag(tag)}
                                        size="small"
                                        sx={{
                                            backgroundColor: 'var(--primary-green)',
                                            color: 'black',
                                            margin: '2px',
                                            '& .MuiChip-deleteIcon': {
                                                color: 'black',
                                                '&:hover': { color: '#444' }
                                            },
                                            padding: '6px 4px',
                                            boxShadow: '2px 6px 6px rgba(0, 0, 0, 0.7)'
                                        }}
                                    />
                                ))}
                            </div>
                        </div>

                        <div className="episode-info-page__description">
                            <Typography variant="body2" className="episode-info-page__description-label">
                                Description
                            </Typography>
                            <div className="episode-info-page__description-editor">
                                <div ref={quillRef} />
                            </div>

                        </div>
                    </div>

                    <div className="episode-info-page__preview">
                        <div className="episode-info-page__main-image-container">
                            {uploadImage ? (
                                <img
                                    src={uploadImage}
                                    alt="Preview"
                                    className="episode-info-page__main-image-file"
                                />
                            ) : (
                                <Image
                                    mainImageFileKey={episodeDetail.MainImageFileKey}
                                    alt={episodeDetail.Name}
                                    className="episode-info-page__main-image-file"
                                />
                            )}

                            <Button
                                className="episode-info-page__change-artwork-btn"
                                onClick={() => fileInputRef.current?.click()}
                            >
                                Change Artwork
                            </Button>
                        </div>

                        <input
                            type="file"
                            ref={fileInputRef}
                            onChange={handleImageUpload}
                            accept=".jpg,.jpeg,.png,.gif,.webp,.svg"
                            style={{ display: 'none' }}
                        />

                        <Typography variant="h6" className="episode-info-page__preview-title">
                            Preview
                        </Typography>
                        <Card className="episode-info-page__preview-card">
                            <div className="episode-info-page__preview-image-container">
                                <CardMedia
                                    component="img"
                                    image={previewImage}
                                    alt={episodeDetail.Name}
                                    className="episode-info-page__preview-bg-image"
                                />
                                <div className="episode-info-page__preview-overlay">
                                    <div className="episode-info-page__preview-content">
                                        <img
                                            src={previewImage}
                                            className="episode-info-page__preview-avatar"
                                        />
                                        <div className="episode-info-page__preview-info">
                                            <Typography variant="h6" className="episode-info-page__preview-name">
                                                {episodeDetail.Name}
                                            </Typography>
                                            <Typography variant="body2" className="episode-info-page__preview-subtitle"

                                            >
                                            </Typography>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </Card>
                    </div>
                </div>
            )
            }
        </div>
    );
};

export default EpisodeInfo;
