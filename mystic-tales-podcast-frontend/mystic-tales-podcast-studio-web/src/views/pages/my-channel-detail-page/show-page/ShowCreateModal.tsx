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
    IconButton,
    InputAdornment,

} from '@mui/material';
import { Add } from '@mui/icons-material';
import { useQuill } from 'react-quilljs';
import 'quill/dist/quill.snow.css';
import logo from '../../../../assets/logoMTP.jpg';
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';
import { PodcastCategory, PodcastSubCategory } from '@/core/types/category';
import { createHashtag, getHashtags } from '@/core/services/misc/hashtag.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { toast } from 'react-toastify';
import Loading from '@/views/components/common/loading';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { urlToFile } from '@/core/utils/image.util';
import { createShow } from '@/core/services/show/show.service';
import { ChannelShowViewContext } from '.';
import { getCategories } from '@/core/services/misc/category.service';
import { useParams } from 'react-router-dom';


export const Language = [
    { Id: 1, Name: "English" },
    { Id: 2, Name: "Vietnamese" },
];


export const mockSubscriptionTypes = [
    { Id: 1, Name: "Free" },
    { Id: 2, Name: "Subscriber only" },
];
export const UploadFrequencyList = [
    { Name: "Daily" },
    { Name: "Weekly" },
    { Name: "Monthly" },
];


interface HashtagOption {
    id: number;
    name: string;
}
const ShowCreate = ({ onClose }: { onClose?: () => void }) => {
    const { id } = useParams<{ id: string }>();
    const context = useContext(ChannelShowViewContext);
    const authSlice = useSelector((state: RootState) => state.auth);
    const [categoryList, setCategoryList] = useState<PodcastCategory[]>([])
    const [selectedHashtags, setSelectedHashtags] = useState<HashtagOption[]>([]);
    const [hashtagInput, setHashtagInput] = useState<string>('');
    const [suggestions, setSuggestions] = useState<HashtagOption[]>([]);
    const [suggestLoading, setSuggestLoading] = useState<boolean>(false);
    const [openSuggest, setOpenSuggest] = useState<boolean>(false);
    const [previewImage, setPreviewImage] = useState<string>('');
    const [mainImageFile, setMainImageFile] = useState<File | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [submitting, setSubmitting] = useState<boolean>(false);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [formData, setFormData] = useState({
        Name: '',
        Copyright: '',
        Language: 'English',
        UploadFrequency: 'Daily',
        Description: '',
        PodcastShowSubscriptionTypeId: 1,
        PodcasterId: authSlice.user?.Id || 0,
        PodcastChannelId: id,
        PodcastCategoryId: 0,
        PodcastSubCategoryId: 0,
        HashtagIds: []

    });
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 10,
        intervalSeconds: 0.5,
    })

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
        fetchCategory();
    }, []);

    const getSubCategoriesForCategory = (categoryId: number): PodcastSubCategory[] => {
        const found = categoryList.find(cat => cat.Id === categoryId);
        return found?.PodcastSubCategoryList ?? [];
    };

    const handleCategoryChange = (categoryId: number) => {
        setFormData(prev => ({
            ...prev,
            PodcastCategoryId: categoryId,
            PodcastSubCategoryId: 0 // Reset subcategory
        }));
    };

    const handleSubCategoryChange = (subCategoryId: number) => {
        setFormData(prev => ({
            ...prev,
            PodcastSubCategoryId: subCategoryId
        }));
    };

    const handleSave = async () => {
        let payload;
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation. .');
            return;
        }
        try {
            setSubmitting(true);
            if (!formData.Name.trim()) {
                toast.error('Please enter a channel name');
                return;
            }

            if (!formData.PodcastCategoryId) {
                toast.error('Please select a category');
                return;
            }

            if (!formData.PodcastSubCategoryId) {
                toast.error('Please select a subcategory');
                return;
            }

            let fileToSend: File | null = mainImageFile;
            if (!mainImageFile) {
                fileToSend = await urlToFile(logo, "default-logo.jpg", "image/jpeg");
            }


            if (formData.PodcastChannelId === "1") {
                payload = {
                    ShowCreateInfo: {
                        ...formData,
                        PodcastChannelId: null,
                        HashtagIds: selectedHashtags.map(tag => tag.id)
                    },
                    MainImageFile: fileToSend
                };
            } else {
                payload = {
                    ShowCreateInfo: {
                        ...formData,
                        HashtagIds: selectedHashtags.map(tag => tag.id)
                    },
                    MainImageFile: fileToSend
                };
            }

            console.log('Creating show with data:', payload);
            try {
                const res = await createShow(loginRequiredAxiosInstance, payload);
                const sagaId = res?.data?.SagaInstanceId
                if (!sagaId) {
                    toast.error("Create show failed, please try again.")
                    return
                }
                await startPolling(sagaId, loginRequiredAxiosInstance, {
                    onSuccess: () => {
                        onClose();
                        context.handleDataChange();
                        toast.success(`Show created successfully!`);
                    },
                    onFailure: (err) => toast.error(err || "Saga failed!"),
                    onTimeout: () => toast.error("System not responding, please try again."),
                })
            } catch (error) {
                toast.error("Error creating show");
            }
        } catch (error) {
            console.error('Error creating show:', error);
            toast.error('Failed to create show');
        } finally {
            setSubmitting(false);
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
    if (loading) {
        return (
            <div className="flex justify-center items-center h-100">
                <Loading />
            </div>
        );
    }
    return (
        <div className="show-info-page">
            <Typography variant="h4" className="show-info-page__title">
                Create New Show
            </Typography>
            <div className="show-info-page__actions">
                <Button
                    disabled={submitting}
                    variant="contained"
                    className="show-info-page__action-btn show-info-page__action-btn--save"
                    onClick={handleSave}
                >
                    {submitting ? 'Saving...' : 'Save'}
                </Button>
            </div>


            <div className="show-info-page__content">
                {/* Form Section */}
                <div className="show-info-page__form">
                    {/* Channel Name and Status Row */}
                    <div className="show-info-page__row">
                        <TextField
                            label="Name"
                            value={formData.Name}
                            variant="standard"
                            onChange={(e) => setFormData({ ...formData, Name: e.target.value })}
                            className="show-info-page__input show-info-page__input--name"
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    '& fieldset': { borderColor: '#999999 !important' },
                                    '&:hover fieldset': { borderColor: '#999999 !important' },
                                    '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                },
                            }}
                        />
                    </div>
                    <div className="show-info-page__row">
                        <TextField
                            select
                            label="Category"
                            variant="standard"
                            value={formData.PodcastCategoryId}
                            onChange={(e) => handleCategoryChange(e.target.value as unknown as number)}
                            className="show-info-page__select"
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
                            value={formData.PodcastSubCategoryId}
                            onChange={(e) => handleSubCategoryChange(e.target.value as unknown as number)}
                            className="show-info-page__select"
                        >
                            <MenuItem value={0} disabled>
                                Select Subcategory
                            </MenuItem>
                            {getSubCategoriesForCategory(formData.PodcastCategoryId).map(sub => (
                                <MenuItem key={sub.Id} value={sub.Id}>{sub.Name}</MenuItem>
                            ))}
                        </TextField>
                    </div>
                    <div className="show-info-page__row">
                        <TextField
                            select
                            label="Language"
                            variant="standard"
                            value={formData.Language}
                            onChange={(e) => setFormData({ ...formData, Language: e.target.value })}

                            className="show-info-page__select"
                        >
                            {Language.map((l) => (
                                <MenuItem
                                    key={l.Id}
                                    value={l.Name}
                                    sx={{
                                        '& .MuiPaper-root': { backgroundColor: '#77898e9d' },
                                    }}
                                >
                                    {l.Name}
                                </MenuItem>
                            ))}
                        </TextField>
                        <TextField
                            select
                            label="Upload Frequency"
                            variant="standard"
                            value={formData.UploadFrequency}
                            onChange={(e) => {
                                setFormData({ ...formData, UploadFrequency: e.target.value });
                            }}
                            className="show-info-page__select"
                        >
                            {UploadFrequencyList.map((freq) => (
                                <MenuItem
                                    key={freq.Name}
                                    value={freq.Name}
                                    sx={{
                                        '& .MuiPaper-root': { backgroundColor: '#77898e9d' },
                                    }}
                                >
                                    {freq.Name}
                                </MenuItem>
                            ))}
                        </TextField>
                        <TextField
                            select
                            label="Subscription Type"
                            variant="standard"
                            value={formData.PodcastShowSubscriptionTypeId}
                            onChange={(e) => setFormData({ ...formData, PodcastShowSubscriptionTypeId: e.target.value as unknown as number })}
                            className="show-info-page__select"
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
                    </div>
                    <TextField
                        label="Copyright"
                        value={formData.Copyright}
                        variant="standard"
                        onChange={(e) => setFormData({ ...formData, Copyright: e.target.value })}
                        className="show-info-page__input show-info-page__input--name"
                        sx={{
                            '& .MuiOutlinedInput-root': {
                                '& fieldset': { borderColor: '#999999 !important' },
                                '&:hover fieldset': { borderColor: '#999999 !important' },
                                '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                            },
                        }}
                    />
                    <div className="show-info-page__hashtags">
                        <div className="show-info-page__hashtag-input ">
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
                    <div className="show-info-page__description">
                        <Typography variant="body2" className="show-info-page__description-label">
                            Description
                        </Typography>
                        <div className="show-info-page__description-editor">
                            <div ref={quillRef} />
                        </div>

                    </div>
                </div>

                {/* Preview Section */}
                <div className="show-info-page__preview">
                    <div className="show-info-page__main-image-container">
                        {previewImage ? (
                            <img
                                src={previewImage}
                                alt={formData.Name || 'Preview'}
                                className="show-info-page__main-image-file"
                            />
                        ) : (
                            <img
                                src={logo}
                                alt={formData.Name || 'Preview'}
                                className="show-info-page__main-image-file"
                            />
                        )}
                        <Button
                            className="show-info-page__change-artwork-btn"
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

export default ShowCreate;
