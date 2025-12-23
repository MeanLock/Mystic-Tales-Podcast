import React, { useEffect, useState, useRef, useCallback, useMemo, createContext } from 'react';
import {
    Box,
    Typography,
    TextField,
    Button,
    Select,
    MenuItem,
    FormControl,
    InputLabel,
    Chip,
    Card,
    CardMedia,
    CardContent,
    IconButton,
    InputAdornment,
    Modal
} from '@mui/material';
import { Add } from '@mui/icons-material';
import { useQuill } from 'react-quilljs';
import 'quill/dist/quill.snow.css';
import './styles.scss';
import { get, isEqual } from 'lodash';
import { getChannelDetail, publishChannel, updateChannel } from '@/core/services/channel/channel.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { useNavigate, useParams } from 'react-router-dom';
import Loading from '@/views/components/common/loading';
import { getCategories } from '@/core/services/misc/category.service';
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';
import { PodcastCategory, PodcastSubCategory } from '@/core/types/category';
import { ChannelDetail } from '@/core/types/channel';
import { createHashtag, getHashtags } from '@/core/services/misc/hashtag.service';
import { toast } from 'react-toastify';
import { fetchImage } from '@/core/utils/image.util';
import { formatDate } from '@/core/utils/date.util';
import Image from '@/views/components/common/image';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { confirmAlert } from '@/core/utils/alert.util';
import Modal_Button from '@/views/components/common/modal/ModalButton';
import { X } from 'phosphor-react';
import DeletionOptionModal from './DeletionOptionModal';


interface ChannelUpdateInfo {
    Name: string;
    Description: string;
    PodcasterId: number;
    PodcastCategoryId: number;
    PodcastSubCategoryId: number;
    HashtagIds: number[];
}
interface HashtagOption {
    id: number;
    name: string;
}
interface ChannelOverviewPageContextProps {
    channelDetail: ChannelDetail;
}
export const ChannelOverviewPageContext = createContext<ChannelOverviewPageContextProps | null>(null);
const ChannelOverview = () => {
    const { id } = useParams<{ id: string }>();
    const authSlice = useSelector((state: RootState) => state.auth);
    const [channelDetail, setChannelDetail] = useState<ChannelDetail | null>(null);
    const [originalChannel, setOriginalChannel] = useState<ChannelDetail | null>(null);
    const [categoryList, setCategoryList] = useState<PodcastCategory[]>([])
    const [selectedHashtags, setSelectedHashtags] = useState<HashtagOption[]>([]);
    const [hashtagInput, setHashtagInput] = useState<string>('');
    const [suggestions, setSuggestions] = useState<HashtagOption[]>([]);
    const [suggestLoading, setSuggestLoading] = useState<boolean>(false);
    const [openSuggest, setOpenSuggest] = useState<boolean>(false);
    const [uploadImage, setUploadImage] = useState<string | null>(null);
    const [mainImageFile, setMainImageFile] = useState<File | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [isPublishing, setIsPublishing] = useState<boolean>(false);
    const navigate = useNavigate();
    const fileInputRef = useRef<HTMLInputElement>(null);

    const [uploadBackgroundImage, setUploadBackgroundImage] = useState<string | null>(null);
    const [backgroundImageFile, setBackgroundImageFile] = useState<File | null>(null);
    const backgroundFileInputRef = useRef<HTMLInputElement>(null);
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 20,
        intervalSeconds: 0.5,
    })
    const [formData, setFormData] = useState<ChannelUpdateInfo>({
        Name: '',
        Description: '',
        PodcasterId: authSlice.user?.Id || 0,
        PodcastCategoryId: 0,
        PodcastSubCategoryId: 0,
        HashtagIds: []
    });

    const [initialLoading, setInitialLoading] = useState(true);
    const [refreshing, setRefreshing] = useState(false);


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

    const fetchChannelDetail = async (opts: { initial?: boolean } = {}) => {
        opts.initial ? setInitialLoading(true) : setRefreshing(true);
        try {
            const channelDetailRes = await getChannelDetail(loginRequiredAxiosInstance, id);
            console.log("Fetched channel detail:", channelDetailRes.data.Channel);
            if (channelDetailRes.success && channelDetailRes.data) {
                const ch = channelDetailRes.data.Channel;
                setChannelDetail(ch);
                setOriginalChannel(prev => prev ?? ch);
                setSelectedHashtags(ch.Hashtags.map((h: any) => ({ id: h.Id, name: h.Name })));
                setFormData({
                    Name: ch.Name,
                    Description: ch.Description || '',
                    PodcasterId: authSlice.user?.Id || 0,
                    PodcastCategoryId: ch.PodcastCategory?.Id || 0,
                    PodcastSubCategoryId: ch.PodcastSubCategory?.Id || 0,
                    HashtagIds: ch.Hashtags?.map((h: any) => h.Id) || []
                });
            } else {
                console.error('API Error:', channelDetailRes.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch channel list:', error);
        } finally {
            opts.initial ? setInitialLoading(false) : setRefreshing(false);
        }
    }
    const fetchCategory = async () => {
        setLoading(true);
        try {
            const categoryList = await getCategories(loginRequiredAxiosInstance);
            console.log("Fetched category list:", categoryList);
            if (categoryList.success) {
                setCategoryList(categoryList.data.PodcastCategoryList);
            } else {
                console.error('API Error:', categoryList.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch category list:', error);
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        fetchChannelDetail({ initial: true });
        fetchCategory()
    }, [id]);


    useEffect(() => {
        if (!quill) return;
        const serverHtml = channelDetail?.Description ?? '';
        const currentHtml = quill.root.innerHTML;
        if (serverHtml !== currentHtml) {
            (quill.clipboard as any).dangerouslyPasteHTML(serverHtml);
        }
    }, [quill, channelDetail?.Description]);

    useEffect(() => {
        if (!quill) return;
        const onTextChange = (_delta: any, _oldDelta: any, source: 'user' | 'api') => {
            if (source !== 'user') return;
            const htmlContent = quill.root.innerHTML;
            setChannelDetail(prev => {
                if (!prev || prev.Description === htmlContent) return prev;
                return { ...prev, Description: htmlContent };
            });
        };
        quill.on('text-change', onTextChange);
        return () => {
            quill.off?.('text-change', onTextChange);
        };
    }, [quill]);


    const getSubCategoriesForCategory = (categoryId: number): PodcastSubCategory[] => {
        const found = categoryList.find(cat => cat.Id === categoryId);
        return found?.PodcastSubCategoryList ?? [];
    };

    const handleCategoryChange = (categoryId: number) => {
        const firstSubId = getSubCategoriesForCategory(categoryId)[0]?.Id || 0;
        setFormData(prev => ({
            ...prev,
            PodcastCategoryId: categoryId,
            PodcastSubCategoryId: firstSubId,
        }));
    };

    const handleSubCategoryChange = (subCategoryId: number) => {
        setFormData(prev => ({
            ...prev,
            PodcastSubCategoryId: subCategoryId
        }));
    };
    const nameChanged = useMemo(() => {
        if (!originalChannel || !channelDetail) return false;
        return originalChannel.Name !== channelDetail.Name;
    }, [originalChannel, channelDetail?.Name]);

    const descriptionChanged = useMemo(() => {
        if (!originalChannel || !channelDetail) return false;
        // chuẩn hóa khoảng trắng nhỏ
        const norm = (s: string) => (s || '').trim();
        return norm(originalChannel.Description || '') !== norm(channelDetail.Description || '');
    }, [originalChannel, channelDetail?.Description]);

    const categoryChanged = useMemo(() => {
        if (!originalChannel) return false;
        return (
            originalChannel.PodcastCategory?.Id !== formData.PodcastCategoryId ||
            originalChannel.PodcastSubCategory?.Id !== formData.PodcastSubCategoryId
        );
    }, [originalChannel, formData.PodcastCategoryId, formData.PodcastSubCategoryId]);

    const hashtagsChanged = useMemo(() => {
        if (!originalChannel) return false;
        const originalIds = (originalChannel.Hashtags || []).map(h => h.Id).sort();
        const currentIds = selectedHashtags.map(h => h.id).sort();
        return !isEqual(originalIds, currentIds);
    }, [originalChannel, selectedHashtags]);

    const imageChanged = useMemo(() =>
        !!mainImageFile || !!backgroundImageFile,
        [mainImageFile, backgroundImageFile]
    );
    const isDirty = useMemo(
        () => nameChanged || descriptionChanged || categoryChanged || hashtagsChanged || imageChanged,
        [nameChanged, descriptionChanged, categoryChanged, hashtagsChanged, imageChanged]
    );

    const handleSave = async () => {
        if (!channelDetail || !isDirty) return;
        try {
            setIsSaving(true);
            const payload = {
                ChannelUpdateInfo: {
                    Name: channelDetail.Name,
                    Description: channelDetail.Description || formData.Description || '',
                    PodcasterId: authSlice.user?.Id || 0,
                    PodcastCategoryId: formData.PodcastCategoryId || channelDetail.PodcastCategory?.Id || 0,
                    PodcastSubCategoryId: formData.PodcastSubCategoryId || channelDetail.PodcastSubCategory?.Id || 0,
                    HashtagIds: selectedHashtags.map(h => h.id),
                },
                MainImageFile: mainImageFile || undefined,
                BackgroundImageFile: backgroundImageFile || undefined,
            };
            const res = await updateChannel(loginRequiredAxiosInstance, String(channelDetail.Id), payload);
            const sagaId = res?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Channel Updated failed, please try again.")
                return
            }
            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: () => {
                    toast.success('Channel updated successfully');
                    setOriginalChannel(prev => ({
                        ...(prev || channelDetail),
                        Name: channelDetail.Name,
                        Description: channelDetail.Description,
                        PodcastCategory: { ...channelDetail.PodcastCategory, Id: formData.PodcastCategoryId },
                        PodcastSubCategory: { ...channelDetail.PodcastSubCategory, Id: formData.PodcastSubCategoryId },
                        Hashtags: selectedHashtags.map(h => ({ Id: h.id, Name: h.name }))
                    }));
                    setMainImageFile(null);
                    setBackgroundImageFile(null);
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error creating channel");
        } finally {
            setIsSaving(false);
        }
    };


    const handleUnpublish = async (isPublish: boolean) => {
        const alert = await confirmAlert("Are you sure to " + (isPublish ? "publish" : "unpublish") + " this channel?");
        if (!alert.isConfirmed) return;
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        try {
            setIsPublishing(true);
            const res = await publishChannel(loginRequiredAxiosInstance, String(channelDetail.Id), isPublish);
            const sagaId = res?.data?.SagaInstanceId
            if (!res.success && res.message.content) {
                toast.error(res.message.content)
                return
            }
            if (!sagaId) {
                toast.error(`Channel ${isPublish ? 'published' : 'unpublished'} failed, please try again.`)
                return
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    toast.success(`Channel ${isPublish ? 'published' : 'unpublished'} successfully.`)
                    await fetchChannelDetail();
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error publishing channel");
        } finally {
            setIsPublishing(false);
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

    const handleBackgroundImageUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (file) {
            const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/svg+xml'];
            if (!allowedTypes.includes(file.type)) {
                toast.error('Invalid file type. Allowed: JPG, JPEG, PNG, GIF, WEBP, SVG');
                return;
            }

            const maxSize = 5 * 1024 * 1024;
            if (file.size > maxSize) {
                toast.error('Image file size must be less than 5MB');
                return;
            }
            setBackgroundImageFile(file);
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
        if (!backgroundImageFile) {
            setUploadBackgroundImage(null)
            return
        }
        const url = URL.createObjectURL(backgroundImageFile)
        setUploadBackgroundImage(url)
        return () => {
            URL.revokeObjectURL(url)
        }
    }, [backgroundImageFile])


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
                const list = (res.data?.HashtagList || []).map((h: any) => ({ id: h.Id, name: h.Name })) as HashtagOption[];
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

    if (loading || !channelDetail) {
        return (
            <div className="flex justify-center items-center h-screen ">
                <Loading />
            </div>
        );
    }
    return (
        <ChannelOverviewPageContext.Provider value={{ channelDetail: channelDetail }}>
            <div className="channel-overview-page">
                <Typography variant="h4" className="channel-overview-page__title">
                    Channel Details
                </Typography>
                <div className="channel-overview-page__actions">
                    <Modal_Button
                        className="channel-overview-page__action-btn channel-overview-page__action-btn--remove"
                        content="Delete"
                        variant="outlined"
                        size='sm'
                        startIcon={<X size={15} />}
                    >
                        <DeletionOptionModal showList={channelDetail.ShowList || []} />
                    </Modal_Button>
                    {channelDetail.CurrentStatus?.Id === 1 ? (
                        <Button
                            variant="outlined"
                            className="channel-overview-page__action-btn channel-overview-page__action-btn--unpublish"
                            onClick={() => handleUnpublish(true)}
                            disabled={isPublishing}
                        >
                            {isPublishing ? 'Publishing...' : 'Publish'}
                        </Button>
                    ) : (
                        <Button
                            variant="outlined"
                            className="channel-overview-page__action-btn channel-overview-page__action-btn--unpublish"
                            onClick={() => handleUnpublish(false)}
                            disabled={isPublishing}
                        >
                            {isPublishing ? 'Unpublishing...' : 'Unpublish'}
                        </Button>
                    )}

                    <Button
                        variant="contained"
                        className="channel-overview-page__action-btn channel-overview-page__action-btn--save"
                        onClick={handleSave}
                        disabled={!isDirty || isSaving}

                    >
                        {isSaving ? 'Saving...' : 'Save'}
                    </Button>
                </div>

                <div className="channel-overview-page__content">
                    <div className="channel-overview-page__form">
                        <div className="channel-overview-page__row">
                            <TextField
                                label="Name"
                                value={channelDetail.Name}
                                variant="standard"
                                onChange={(e) => setChannelDetail({ ...channelDetail, Name: e.target.value })}
                                className="channel-overview-page__input channel-overview-page__input--name"
                                sx={{
                                    '& .MuiOutlinedInput-root': {
                                        '& fieldset': { borderColor: '#999999 !important' },
                                        '&:hover fieldset': { borderColor: '#999999 !important' },
                                        '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                    },
                                }}
                            />

                            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                                <Chip
                                    label={channelDetail.CurrentStatus.Name}
                                    sx={{
                                        // Glassmorphism effect
                                        background: channelDetail.CurrentStatus.Id === 1
                                            ? 'rgba(255, 193, 7, 0.2)'
                                            : 'rgba(174, 227, 57, 0.2)',
                                        backdropFilter: 'blur(10px)',
                                        WebkitBackdropFilter: 'blur(10px)', // Safari support
                                        minWidth: 150,
                                        padding: '0 10px',
                                        borderRadius: 50,
                                        fontWeight: 700,
                                        fontSize: '0.9rem',
                                        // Border with gradient
                                        border: channelDetail.CurrentStatus.Id === 1
                                            ? '1.5px solid #ffb300'
                                            : '1.5px solid #aee339',
                                        color: channelDetail.CurrentStatus.Id === 1 ? '#ffb300' : '#aee339',
                                        height: '44px',
                                        boxShadow: channelDetail.CurrentStatus.Id === 1
                                            ? '0 6px 32px rgba(255, 193, 7, 0.1)'
                                            : '0 6px 32px rgba(174, 227, 57, 0.1)',
                                        '& .MuiChip-label': {
                                            padding: '0 12px',
                                            textShadow: '0 2px 4px rgba(0, 0, 0, 0.2)',

                                        },
                                    }}
                                />
                            </Box>
                        </div>

                        <div className="channel-overview-page__row">
                            <TextField
                                select
                                label="Category"
                                variant="standard"
                                value={formData.PodcastCategoryId}
                                onChange={(e) => handleCategoryChange(e.target.value as unknown as number)}
                                className="channel-overview-page__select"
                            >
                                {categoryList.map((category) => (
                                    <MenuItem
                                        key={category.Id}
                                        value={category.Id}
                                        sx={{
                                            '& .MuiPaper-root': { backgroundColor: '#77898e9d' },
                                        }}
                                    >
                                        {category.Name}
                                    </MenuItem>
                                ))}
                            </TextField>
                            <TextField
                                select
                                label="Subcategory"
                                variant="standard"
                                value={formData.PodcastSubCategoryId}
                                onChange={(e) => handleSubCategoryChange(e.target.value as unknown as number)}
                                className="channel-overview-page__select"
                            >
                                {getSubCategoriesForCategory(formData.PodcastCategoryId || channelDetail.PodcastCategory.Id).map(sub => (
                                    <MenuItem key={sub.Id} value={sub.Id}>{sub.Name}</MenuItem>
                                ))}
                            </TextField>
                        </div>

                        {/* Dates and Numbers Row */}
                        <div className="channel-overview-page__row">
                            <TextField
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Created At"
                                value={formatDate(channelDetail.CreatedAt)}
                                className="channel-overview-page__input-small"

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
                                value={formatDate(channelDetail.UpdatedAt)}
                                className="channel-overview-page__input-small"

                            />
                            <TextField
                                id="filled-helperText"
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Total Favorite"
                                value={channelDetail.TotalFavorite}
                                className="channel-overview-page__input-small"

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
                                value={channelDetail.ListenCount}
                                className="channel-overview-page__input-small"

                            />
                        </div>

                        {/* Hashtags */}
                        <div className="channel-overview-page__hashtags">
                            <div className="channel-overview-page__hashtag-input">
                                <TextField
                                    label="Add hashtag"
                                    value={hashtagInput}
                                    onChange={(e) => setHashtagInput(e.target.value)}
                                    onKeyPress={handleHashtagKeyPress}
                                    size="small"
                                    className="channel-overview-page__hashtag-field"
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
                                        className="channel-overview-page__hashtag-suggest"
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
                            <div className="channel-overview-page__hashtag-chips">
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

                        {/* Description */}
                        <div className="channel-overview-page__description">
                            <Typography variant="body2" className="channel-overview-page__description-label">
                                Description
                            </Typography>
                            <div className="channel-overview-page__description-editor">
                                <div ref={quillRef} />
                            </div>

                        </div>
                    </div>

                    <div className="channel-overview-page__preview">
                        <Typography variant="h6" className="channel-overview-page__preview-title">
                            Main Image
                        </Typography>
                        <div className="channel-overview-page__main-image-container">
                            {uploadImage ? (
                                <img
                                    src={uploadImage}
                                    alt="Preview"
                                    className="channel-overview-page__main-image-file"
                                />
                            ) : (
                                <Image
                                    mainImageFileKey={channelDetail.MainImageFileKey}
                                    alt={channelDetail.Name}
                                    className="channel-overview-page__main-image-file"
                                />
                            )}

                            <Button
                                className="channel-overview-page__change-artwork-btn"
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

                        <Typography variant="h6" className="channel-overview-page__preview-title">
                            Background Image
                        </Typography>
                        <div className="channel-overview-page__main-image-container">
                            {uploadBackgroundImage ? (
                                <img
                                    src={uploadBackgroundImage}
                                    alt="Background Preview"
                                    className="channel-overview-page__main-image-file"
                                />
                            ) : (
                                <Image
                                    mainImageFileKey={channelDetail.BackgroundImageFileKey}
                                    alt={`${channelDetail.Name} Background`}
                                    className="channel-overview-page__main-image-file"
                                />
                            )}

                            <Button
                                className="channel-overview-page__change-artwork-btn"
                                onClick={() => backgroundFileInputRef.current?.click()}
                            >
                                Change Artwork
                            </Button>
                        </div>

                        <input
                            type="file"
                            ref={backgroundFileInputRef}
                            onChange={handleBackgroundImageUpload}
                            accept=".jpg,.jpeg,.png,.gif,.webp,.svg"
                            style={{ display: 'none' }}
                        />
                    </div>
                </div>
            </div>
        </ChannelOverviewPageContext.Provider>
    );
};

export default ChannelOverview;
