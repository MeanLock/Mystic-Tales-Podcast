import React, { useEffect, useState, useRef, use } from 'react';
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
export const mockPodcastCategories = [
    { Id: 1, Name: "True Crime " },
    { Id: 2, Name: "Horror " },
    { Id: 3, Name: "Society & Culture " },
    { Id: 4, Name: "Psychology " },
    { Id: 5, Name: "Philosophy " },
    { Id: 6, Name: "History " },
    { Id: 7, Name: "Comics" },
];

export const mockPodcastSubCategories = [
    // ===== True Crime (1) =====
    { Id: 1, Name: "Serial Killers", PodcastCategoryId: 1 },
    { Id: 2, Name: "Unsolved Mysteries ", PodcastCategoryId: 1 },
    { Id: 3, Name: "White Collar Crime", PodcastCategoryId: 1 },
    { Id: 4, Name: "Cybercrime ", PodcastCategoryId: 1 },
    { Id: 5, Name: "International Crimes", PodcastCategoryId: 1 },
    { Id: 6, Name: "Organized Crime", PodcastCategoryId: 1 },
    { Id: 7, Name: "Miscarriage of Justice", PodcastCategoryId: 1 },

    // ===== Horror (2) =====
    { Id: 8, Name: "Asian Folklore Horror", PodcastCategoryId: 2 },
    { Id: 9, Name: "Europe Folklore Horror", PodcastCategoryId: 2 },
    { Id: 10, Name: "Creepypasta", PodcastCategoryId: 2 },
    { Id: 11, Name: "Urban Legends", PodcastCategoryId: 2 },
    { Id: 12, Name: "Supernatural", PodcastCategoryId: 2 },

    // ===== Society & Culture (3) =====
    { Id: 13, Name: "Heterodox Faith", PodcastCategoryId: 3 },
    { Id: 14, Name: "Horrific Cultures", PodcastCategoryId: 3 },
    { Id: 15, Name: "Dark Prejudices", PodcastCategoryId: 3 },
    { Id: 16, Name: "Mysterious Tribes", PodcastCategoryId: 3 },
];



// Mock available hashtags for autocomplete
const availableHashtags = [
    '#Mystery', '#HorrorStories', '#Paranormal', '#TrueCrime', '#Supernatural',
    '#Investigation', '#Thriller', '#Creepy', '#Folklore', '#Legend',
    '#SerialKiller', '#UnsolvedMystery', '#ColdCase', '#Detective'
];

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
    const [channelData, setChannelData] = useState<ChannelCreateInfo>({
        Name: '',
        Description: '',
        PodcasterId: 0, // This should be set from user context
        PodcastCategoryId: 0,
        PodcastSubCategoryId: 0,
        HashtagIds: []
    });

    const [selectedHashtags, setSelectedHashtags] = useState<HashtagOption[]>([]);
    const [hashtagInput, setHashtagInput] = useState<string>('');
    const [previewImage, setPreviewImage] = useState<string>('');
    const [mainImageFile, setMainImageFile] = useState<File | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [loading, setLoading] = useState(false);

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

    // Set up Quill listener for description changes
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

    // Get subcategories for selected category
    const getSubCategoriesForCategory = (categoryId: number) => {
        return mockPodcastSubCategories.filter(sub => sub.PodcastCategoryId === categoryId);
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
        try {
            setLoading(true);

            // Validate required fields
            if (!channelData.Name.trim()) {
                alert('Please enter a channel name');
                return;
            }

            if (!channelData.PodcastCategoryId) {
                alert('Please select a category');
                return;
            }

            if (!mainImageFile) {
                alert('Please select a main image');
                return;
            }

            // Prepare form data for API call
            const formData = new FormData();

            // Add ChannelCreateInfo as JSON
            const channelCreateInfo = {
                ...channelData,
                HashtagIds: selectedHashtags.map(tag => tag.id)
            };

            formData.append('ChannelCreateInfo', JSON.stringify(channelCreateInfo));
            formData.append('MainImageFile', mainImageFile);

            // TODO: Call your API here
            console.log('Creating channel with data:', channelCreateInfo);
            console.log('Main image file:', mainImageFile);

            // Example API call:
            // await createChannelAPI(formData);

            alert('Channel created successfully!');
            onClose?.();

        } catch (error) {
            console.error('Error creating channel:', error);
            alert('Failed to create channel');
        } finally {
            setLoading(false);
        }
    };

    const handleImageUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (file) {
            setMainImageFile(file);
            const reader = new FileReader();
            reader.onload = (e) => {
                const imageUrl = e.target?.result as string;
                setPreviewImage(imageUrl);
            };
            reader.readAsDataURL(file);
        }
    };

    const handleAddHashtag = () => {
        if (hashtagInput.trim()) {
            // For create mode, we'll create new hashtags with temporary IDs
            // In a real app, you might want to search existing hashtags first
            const newHashtag: HashtagOption = {
                id: Date.now(), // Temporary ID, will be replaced by backend
                name: hashtagInput.trim()
            };

            const exists = selectedHashtags.some(tag =>
                tag.name.toLowerCase() === newHashtag.name.toLowerCase()
            );

            if (!exists) {
                setSelectedHashtags(prev => [...prev, newHashtag]);
                setHashtagInput('');
            }
        }
    };

    const handleHashtagKeyPress = (event: React.KeyboardEvent) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            handleAddHashtag();
        }
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
                            {mockPodcastCategories.map((category) => (
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
                        >
                            <MenuItem value={0}>
                                None
                            </MenuItem>
                            {getSubCategoriesForCategory(channelData.PodcastCategoryId).map((subCategory) => (
                                <MenuItem key={subCategory.Id} value={subCategory.Id}>
                                    {subCategory.Name}
                                </MenuItem>
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
                                InputProps={{
                                    endAdornment: (
                                        <InputAdornment position="end">
                                            <IconButton
                                                onClick={handleAddHashtag}
                                                disabled={!hashtagInput.trim() || selectedHashtags.some(tag => tag.name.toLowerCase() === hashtagInput.trim().toLowerCase())}
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
                        accept="image/*"
                        style={{ display: 'none' }}
                    />

                    <Typography variant="h6" className="channel-overview-page__preview-title text-center">
                        Preview
                    </Typography>
                    <Card className="channel-overview-page__preview-card">
                        <div className="channel-overview-page__preview-image-container">
                            <CardMedia
                                component="img"
                                image={previewImage || logo}
                                alt={channelData.Name || 'Channel artwork'}
                                className="channel-overview-page__preview-bg-image"
                            />
                            <div className="channel-overview-page__preview-overlay">
                                <div className="channel-overview-page__preview-content">
                                    {previewImage ? (
                                        <img
                                            src={previewImage}
                                            alt={channelData.Name || 'Channel artwork'}
                                            className="channel-overview-page__preview-avatar"
                                        />
                                    ) : (
                                        <img
                                            src={logo}
                                            alt='Channel artwork'
                                            className="channel-overview-page__preview-avatar"
                                        />
                                    )}
                                    <div className="channel-overview-page__preview-info">
                                        <Typography variant="h6" className="channel-overview-page__preview-name">
                                            {channelData.Name || 'Channel Name'}
                                        </Typography>
                                        <Typography
                                            variant="body2"
                                            className="channel-overview-page__preview-subtitle"
                                            component="div"
                                            dangerouslySetInnerHTML={{
                                                __html: (channelData.Description || 'Description')
                                            }}
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </Card>
                </div>
            </div>
        </div>
    );
};

export default ChannelCreate;
