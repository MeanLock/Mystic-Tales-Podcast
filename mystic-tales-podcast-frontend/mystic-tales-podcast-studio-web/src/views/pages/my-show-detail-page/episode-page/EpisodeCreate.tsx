import React, { useEffect, useState, useRef, use, useCallback, useContext } from 'react';
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
    Rating,
    Tabs,
    Tab,
} from '@mui/material';
import { Add } from '@mui/icons-material';
import { useQuill } from 'react-quilljs';
 import 'quill/dist/quill.snow.css';
import logo from '../../../../assets/logoMTP.jpg';
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';
import { createHashtag, getHashtags } from '@/core/services/misc/hashtag.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { toast } from 'react-toastify';
import { urlToFile } from '@/core/utils/image.util';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { set } from 'lodash';
import { createEpisode } from '@/core/services/episode/episode.service';
import { getShowList } from '@/core/services/show/show.service';
import { Filter } from 'lucide-react';
import { ShowEpisodeViewContext } from '.';
import { useNavigate } from 'react-router-dom';



export const mockSubscriptionTypes = [
    { Id: 1, Name: "Free" },
    { Id: 2, Name: "Subscriber-Only" },
    { Id: 3, Name: "Bonus" },
    { Id: 4, Name: "Archive" },
];


interface HashtagOption {
    id: number;
    name: string;
}

const EpisodeCreate = ({ onClose }: { onClose?: () => void }) => {
    const navigation = useSelector((state: RootState) => state.navigation);
    const context = useContext(ShowEpisodeViewContext);
    const authSlice = useSelector((state: RootState) => state.auth);
    const [selectedHashtags, setSelectedHashtags] = useState<HashtagOption[]>([]);
    const [hashtagInput, setHashtagInput] = useState<string>('');
    const [suggestions, setSuggestions] = useState<HashtagOption[]>([]);
    const [suggestLoading, setSuggestLoading] = useState<boolean>(false);
    const [openSuggest, setOpenSuggest] = useState<boolean>(false);
    const [previewImage, setPreviewImage] = useState<string>('');
    const [mainImageFile, setMainImageFile] = useState<File | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [loading, setLoading] = useState(false);
    const [showList, setShowList] = useState<any[]>([]);

    const navigate = useNavigate();
    const hasShowContext = navigation.currentContext.type === 'Show' ? true : false;

    const [formData, setFormData] = useState({
        Name: '',
        Description: '',
        ExplicitContent: false,
        PodcastEpisodeSubscriptionTypeId: 1,
        PodcastShowId: hasShowContext ? navigation.currentContext?.id : '',
        SeasonNumber: 1,
        EpisodeOrder: 1,
        HashtagIds: []
    });

    const { startPolling } = useSagaPolling({
        timeoutSeconds: 60,
        intervalSeconds: 0.5,
    })

    const fetchShowList = async () => {
        setLoading(true);
        try {
            const showList = await getShowList(loginRequiredAxiosInstance);
            if (showList.success) {
                const shows = showList.data?.ShowList.filter((show: any) => show.CurrentStatus.Id !== 4 && show.CurrentStatus.Id !== 5) || [];
                setShowList(shows);
            } else {
                console.error('API Error:', showList.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch show list:', error);
        } finally {
            setLoading(false);
        }
    }
    useEffect(() => {
        fetchShowList();
    }, []);
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

    useEffect(() => {
        if (quill) {
            quill.on('text-change', () => {
                const htmlContent = quill.root.innerHTML;
                setFormData(prev => ({
                    ...prev,
                    Description: htmlContent
                }));
            });
        }
    }, [quill]);

    const handleSave = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        try {
            setLoading(true);
            if (!formData.Name.trim()) {
                toast.error('Please enter an episode name');
                return;
            }

            if (!hasShowContext && !formData.PodcastShowId) {
                toast.error('Please select a podcast show');
                return;
            }


            let fileToSend: File | null = mainImageFile;
            if (!mainImageFile) {
                fileToSend = await urlToFile(logo, "default-logo.jpg", "image/jpeg");
            }

            const payload = {
                EpisodeCreateInfo: {
                    ...formData,
                    HashtagIds: selectedHashtags.map(tag => tag.id)
                },
                MainImageFile: fileToSend
            };

            console.log('Creating Episode with data:', payload);
            try {
                const res = await createEpisode(loginRequiredAxiosInstance, payload);
                const sagaId = res?.data?.SagaInstanceId
                if (!sagaId) {
                    toast.error("Create Episode failed, please try again.")
                    return
                }
                await startPolling(sagaId, loginRequiredAxiosInstance, {
                    onSuccess: (data) => {
                        onClose();
                        if (hasShowContext) {
                            context?.handleDataChange();
                        } else {
                            navigate(`/show/${data.PodcastShowId}/episode/${data.PodcastEpisodeId}`);
                        }
                        toast.success(`Episode created successfully!`);
                    },
                    onFailure: (err) => toast.error(err || "Saga failed!"),
                    onTimeout: () => toast.error("System not responding, please try again."),
                })
            } catch (error) {
                toast.error("Error creating episode");
            }
        } catch (error) {
            console.error('Error creating episode:', error);
            toast.error('Failed to create episode');
        } finally {
            setLoading(false);
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
            const reader = new FileReader();
            reader.onload = (e) => {
                const imageUrl = e.target?.result as string;
                setPreviewImage(imageUrl);
            };
            reader.readAsDataURL(file);
        }
    };

    const handleAddHashtag = async (hashtagInput: string) => {
        try {
            const res = await createHashtag(loginRequiredAxiosInstance, { HashtagName: hashtagInput });
            if (res?.success) {
                const newHashtag: HashtagOption = {
                    id: res.data.NewHashtag.Id,
                    name: res.data.NewHashtag.Name
                };
                // Allow same letters with different case (Abc vs aBc treated as different)
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

    return (
        <div className="show-episode-modal">
            <Typography variant="h4" className="show-episode-modal__title">
                Create New Episode
            </Typography>
            <div className="show-episode-modal__actions">
                <Button
                    disabled={loading}
                    variant="contained"
                    className="show-episode-modal__action-btn show-episode-modal__action-btn--save"
                    onClick={handleSave}
                >
                    {loading ? 'Creating...' : 'Create'}
                </Button>
            </div>


            <div className="show-episode-modal__content">
                {/* Form Section */}
                <div className="show-episode-modal__form">
                    {/* Channel Name and Status Row */}
                    <div className="show-episode-modal__row">
                        <TextField
                            label="Name"
                            value={formData.Name}
                            required
                            variant="standard"
                            onChange={(e) => setFormData({ ...formData, Name: e.target.value })}
                            className="show-episode-modal__input show-episode-modal__input--name"
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    '& fieldset': { borderColor: '#999999 !important' },
                                    '&:hover fieldset': { borderColor: '#999999 !important' },
                                    '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                },
                            }}
                        />
                    </div>
                    <div className="show-episode-modal__row">
                        {hasShowContext ? (
                            <TextField
                                label="Podcast Show"
                                variant="standard"
                                value={navigation.currentContext?.name || ''}
                                className="show-episode-modal__select"
                                InputProps={{
                                    readOnly: true,
                                }}
                                sx={{
                                    '& .MuiInputBase-input': {
                                        color: 'rgba(255, 255, 255, 0.7)',
                                        cursor: 'not-allowed',
                                    }
                                }}
                            />
                        ) : (
                            <TextField
                                select
                                label="Podcast Show"
                                variant="standard"
                                value={formData.PodcastShowId}
                                onChange={(e) => setFormData({ ...formData, PodcastShowId: e.target.value })}
                                className="show-episode-modal__select"
                                required
                                error={!formData.PodcastShowId}
                            >
                                {showList.map((show) => (
                                    <MenuItem
                                        key={show.Id}
                                        value={show.Id}
                                        sx={{
                                            '& .MuiPaper-root': { backgroundColor: '#77898e9d' },
                                        }}
                                    >
                                        {show.Name}
                                    </MenuItem>
                                ))}
                            </TextField>
                        )}
                        <TextField
                            select
                            label="Subscription Type"
                            variant="standard"
                            value={formData.PodcastEpisodeSubscriptionTypeId}
                            onChange={(e) => setFormData({ ...formData, PodcastEpisodeSubscriptionTypeId: e.target.value as unknown as number })}
                            className="show-episode-modal__select"
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
                            value={formData.ExplicitContent}
                            onChange={(e) => setFormData({ ...formData, ExplicitContent: e.target.value === 'true' })}
                            className="show-episode-modal__select"
                        >
                            <MenuItem value="false">No</MenuItem>
                            <MenuItem value="true">Yes</MenuItem>
                        </TextField>
                    </div>

                    {/* Numbers Row */}
                    <div className="show-episode-modal__row">
                        <TextField
                            label="Season Number"
                            value={formData.SeasonNumber}
                            type="number"
                            variant="standard"
                            inputProps={{ min: 1 }}
                            onChange={(e) => {
                                let val = e.target.value;
                                if (val === '' || Number(val) < 1) {
                                    setFormData({ ...formData, SeasonNumber: 1 });
                                } else {
                                    setFormData({ ...formData, SeasonNumber: Number(val) });
                                }
                            }}
                            className="show-episode-modal__input show-episode-modal__input--number"
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
                            value={formData.EpisodeOrder}
                            type="number"
                            variant="standard"
                            inputProps={{ min: 1 }}
                            onChange={(e) => {
                                let val = e.target.value;
                                if (val === '' || Number(val) < 1) {
                                    setFormData({ ...formData, EpisodeOrder: 1 });
                                } else {
                                    setFormData({ ...formData, EpisodeOrder: Number(val) });
                                }
                            }}
                            className="show-episode-modal__input show-episode-modal__input--number"
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    '& fieldset': { borderColor: '#999999 !important' },
                                    '&:hover fieldset': { borderColor: '#999999 !important' },
                                    '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                },
                            }}
                        />
                    </div>

                    <div className="show-episode-modal__hashtags">
                        <div className="show-episode-modal__hashtag-input ">
                            <TextField
                                label="Add hashtag"
                                value={hashtagInput}
                                onChange={(e) => setHashtagInput(e.target.value)}
                                onKeyPress={handleHashtagKeyPress}
                                size="small"
                                className="show-info-page__hashtag-field"
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
                        <div className="show-episode-modal__hashtag-chips">
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
                    <div className="show-episode-modal__description">
                        <Typography variant="body2" className="show-episode-modal__description-label">
                            Description
                        </Typography>
                        <div className="show-episode-modal__description-editor">
                            <div ref={quillRef} />
                        </div>

                    </div>
                </div>

                {/* Preview Section */}
                <div className="show-episode-modal__preview">
                    <div className="show-episode-modal__main-image-container">
                        {previewImage ? (
                            <img
                                src={previewImage}
                                alt={formData.Name || 'Preview'}
                                className="show-episode-modal__main-image-file"
                            />
                        ) : (
                            <img
                                src={logo}
                                alt={formData.Name || 'Preview'}
                                className="show-episode-modal__main-image-file"
                            />
                        )}
                        <Button
                            className="show-episode-modal__change-artwork-btn"
                            onClick={() => fileInputRef.current?.click()}
                        >
                            {previewImage ? 'Change Artwork' : 'Upload Artwork'}
                        </Button>
                    </div>
                    <input
                        type="file"
                        ref={fileInputRef}
                        onChange={handleImageUpload}
                        accept=".jpg,.jpeg,.png,.gif,.webp,.svg"
                        style={{ display: 'none' }}
                    />
                </div>
            </div>
        </div>
    );
};

export default EpisodeCreate;
