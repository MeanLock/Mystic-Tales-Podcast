import React, { useEffect, useState, useRef, use, useContext, useCallback } from 'react';
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
    InputAdornment
} from '@mui/material';
import { Add } from '@mui/icons-material';
import { useQuill } from 'react-quilljs';
 import 'quill/dist/quill.snow.css';
import './styles.scss';
import logo from '../../../assets/logoMTP.jpg';
import { MyChannelPageContext } from '.';
import { PodcastCategory, PodcastSubCategory } from '@/core/types/category';
import { toast } from 'react-toastify';
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { createHashtag, getHashtags } from '@/core/services/misc/hashtag.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { createChannel } from '@/core/services/channel/channel.service';
import { urlToFile } from '@/core/utils/image.util';

interface ChannelCreateInfo {
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

const ChannelCreate = ({ onClose }: { onClose?: () => void }) => {
    const context = useContext(MyChannelPageContext);
    const authSlice = useSelector((state: RootState) => state.auth);
    const categoryList: PodcastCategory[] = context?.categoryList || [];
    const [selectedHashtags, setSelectedHashtags] = useState<HashtagOption[]>([]);
    const [hashtagInput, setHashtagInput] = useState<string>('');
    const [suggestions, setSuggestions] = useState<HashtagOption[]>([]);
    const [suggestLoading, setSuggestLoading] = useState<boolean>(false);
    const [openSuggest, setOpenSuggest] = useState<boolean>(false);
    const [previewImage, setPreviewImage] = useState<string>('');
    const [mainImageFile, setMainImageFile] = useState<File | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [previewBackgroundImage, setPreviewBackgroundImage] = useState<string>('');
    const [backgroundImageFile, setBackgroundImageFile] = useState<File | null>(null);
    const backgroundFileInputRef = useRef<HTMLInputElement>(null);
    const [loading, setLoading] = useState(false);

    const [channelData, setChannelData] = useState<ChannelCreateInfo>({
        Name: '',
        Description: '',
        PodcasterId: authSlice.user?.Id || 0,
        PodcastCategoryId: 0,
        PodcastSubCategoryId: 0,
        HashtagIds: []
    });
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })

    // Quill editor for description
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
                setChannelData(prev => ({
                    ...prev,
                    Description: htmlContent
                }));
            });
        }
    }, [quill]);


    const getSubCategoriesForCategory = (categoryId: number): PodcastSubCategory[] => {
        const found = categoryList.find(cat => cat.Id === categoryId);
        return found?.PodcastSubCategoryList ?? [];
    };

    const handleCategoryChange = (categoryId: number) => {
        setChannelData(prev => ({
            ...prev,
            PodcastCategoryId: categoryId,
            PodcastSubCategoryId: 0 // Reset subcategory
        }));
    };

    const handleSubCategoryChange = (subCategoryId: number) => {
        setChannelData(prev => ({
            ...prev,
            PodcastSubCategoryId: subCategoryId
        }));
    };

    const handleCreateChannel = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        try {
            setLoading(true);
            if (!channelData.Name.trim()) {
                toast.error('Please enter a channel name');
                return;
            }

            if (!channelData.PodcastCategoryId) {
                toast.error('Please select a category');
                return;
            }

            if (!channelData.PodcastSubCategoryId) {
                toast.error('Please select a subcategory');
                return;
            }

            let fileToSend: File | null = mainImageFile;
            if (!mainImageFile) {
                fileToSend = await urlToFile(logo, "default-logo.jpg", "image/jpeg");
            }
            const backgroundFileToSend = backgroundImageFile || null;

            const payload = {
                ChannelCreateInfo: {
                    ...channelData,
                    HashtagIds: selectedHashtags.map(tag => tag.id)
                },
                MainImageFile: fileToSend,
                BackgroundImageFile: backgroundFileToSend
            };

            console.log('Creating channel with data:', payload);
            try {
                const res = await createChannel(loginRequiredAxiosInstance, payload);
                const sagaId = res?.data?.SagaInstanceId
                if (!sagaId) {
                    toast.error("Create channel failed, please try again.")
                    return
                }
                await startPolling(sagaId, loginRequiredAxiosInstance, {
                    onSuccess: () => {
                        onClose();
                        context?.handleDataChange();
                        toast.success(`Channel created successfully!`);
                    },
                    onFailure: (err) => toast.error(err || "Saga failed!"),
                    onTimeout: () => toast.error("System not responding, please try again."),
                })
            } catch (error) {
                toast.error("Error creating channel");
            }
        } catch (error) {
            console.error('Error creating channel:', error);
            toast.error('Failed to create channel');
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
                toast.error('Image file size must be less than 3 MB');
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
            const reader = new FileReader();
            reader.onload = (e) => {
                const imageUrl = e.target?.result as string;
                setPreviewBackgroundImage(imageUrl);
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
        <div className="channel-overview-page">
            <Typography variant="h4" className="channel-overview-page__title">
                Create New Channel
            </Typography>
            <div className="channel-overview-page__actions">

                <Button
                    variant="contained"
                    className="channel-overview-page__action-btn channel-overview-page__action-btn--save"
                    onClick={handleCreateChannel}
                    disabled={loading}
                >
                    {loading ? 'Creating...' : 'Create'}
                </Button>
            </div>

            <div className="channel-overview-page__content">
                {/* Form Section */}
                <div className="channel-overview-page__form">
                    {/* Channel Name Row */}
                    <div className="channel-overview-page__row">
                        <TextField
                            label="Name"
                            value={channelData.Name}
                            variant="standard"
                            onChange={(e) => setChannelData({ ...channelData, Name: e.target.value })}
                            className="channel-overview-page__input channel-overview-page__input--name"
                            required
                            fullWidth
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    '& fieldset': { borderColor: '#999999 !important' },
                                    '&:hover fieldset': { borderColor: '#999999 !important' },
                                    '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                },
                            }}
                        />
                    </div>

                    {/* Category and Subcategory Row */}
                    <div className="channel-overview-page__row">
                        <TextField
                            select
                            label="Category"
                            variant="standard"
                            value={channelData.PodcastCategoryId}
                            onChange={(e) => handleCategoryChange(e.target.value as unknown as number)}
                            className="channel-overview-page__select"
                            required
                        >
                            <MenuItem value={0} disabled>
                                Select Category
                            </MenuItem>
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
                            value={channelData.PodcastSubCategoryId}
                            onChange={(e) => handleSubCategoryChange(e.target.value as unknown as number)}
                            className="channel-overview-page__select"
                            required
                        >
                            <MenuItem value={0} disabled>
                                Select Subcategory
                            </MenuItem>
                            {getSubCategoriesForCategory(channelData.PodcastCategoryId).map(sub => (
                                <MenuItem key={sub.Id} value={sub.Id}>{sub.Name}</MenuItem>
                            ))}
                        </TextField>
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

                {/* Preview Section */}
                <div className="channel-overview-page__preview ">
                    <Typography variant="h6" className="channel-overview-page__preview-title text-center">
                        Main Image
                    </Typography>
                    <div className="channel-overview-page__main-image-container">
                        {previewImage ? (
                            <img
                                src={previewImage}
                                alt={channelData.Name || 'Channel artwork'}
                                className="channel-overview-page__main-image-file"
                            />
                        ) : (
                            <img
                                src={logo}
                                alt='Channel artwork'
                                className="channel-overview-page__main-image-file"
                            />
                        )}
                        <Button
                            className="channel-overview-page__change-artwork-btn"
                            onClick={() => fileInputRef.current?.click()}
                        >
                            {previewImage ? 'Change Artwork' : 'Select Artwork *'}
                        </Button>
                    </div>

                    <input
                        type="file"
                        ref={fileInputRef}
                        onChange={handleImageUpload}
                        accept=".jpg,.jpeg,.png,.gif,.webp,.svg"
                        style={{ display: 'none' }}
                    />

                    <Typography variant="h6" className="channel-overview-page__preview-title text-center">
                        Background Image
                    </Typography>
                    <div className="channel-overview-page__main-image-container">
                        {previewBackgroundImage ? (
                            <img
                                src={previewBackgroundImage}
                                alt={`${channelData.Name} Background` || 'Background artwork'}
                                className="channel-overview-page__main-image-file"
                            />
                        ) : (
                            <img
                                src={logo}
                                alt='Background artwork'
                                className="channel-overview-page__main-image-file"
                            />
                        )}
                        <Button
                            className="channel-overview-page__change-artwork-btn"
                            onClick={() => backgroundFileInputRef.current?.click()}
                        >
                            {previewBackgroundImage ? 'Change Background' : 'Select Background'}
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
    );
};

export default ChannelCreate;
