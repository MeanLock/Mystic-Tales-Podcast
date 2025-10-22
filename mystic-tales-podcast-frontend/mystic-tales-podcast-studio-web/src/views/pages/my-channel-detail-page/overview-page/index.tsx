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

export const mockData = {
    Channel: {
        Id: 1,
        Name: "Mystic Tales",
        Description: "A podcast channel sharing mysterious stories and real-life paranormal events.",
        MainImageFileKey: "main_mystic_tales.png",
        TotalFavorite: 1520,
        ListenCount: 34900,
        PodcastCategory: {
            Id: 1,
            Name: "True Crime ",
        },
        PodcastSubCategory: {
            Id: 1,
            Name: "Serial Killers",
            PodcastCategoryId: 1,
        },
        Hashtags: [
            { Id: 1, Name: "#Mystery" },
            { Id: 2, Name: "#HorrorStories" },
            { Id: 3, Name: "#Paranormal" },
        ],
        CreatedAt: "2025-10-17T05:44:11.252Z",
        UpdatedAt: "2025-10-17T05:44:11.252Z",
        CurrentStatus: {
            Id: 1,
            Name: "Published",
        },
    }
};

// Mock available hashtags for autocomplete
const availableHashtags = [
    '#Mystery', '#HorrorStories', '#Paranormal', '#TrueCrime', '#Supernatural',
    '#Investigation', '#Thriller', '#Creepy', '#Folklore', '#Legend',
    '#SerialKiller', '#UnsolvedMystery', '#ColdCase', '#Detective'
];

const ChannelOverview = () => {
    const [channelDetail, setChannelDetail] = useState<any>(mockData.Channel);
    const [selectedCategory, setSelectedCategory] = useState<number>(mockData.Channel.PodcastCategory.Id);
    const [selectedSubCategory, setSelectedSubCategory] = useState<number>(mockData.Channel.PodcastSubCategory.Id);
    const [selectedHashtags, setSelectedHashtags] = useState<string[]>(
        mockData.Channel.Hashtags.map((tag: any) => tag.Name)
    );
    const [description, setDescription] = useState<string>(mockData.Channel.Description || '');

    const [hashtagInput, setHashtagInput] = useState<string>('');
    const [previewImage, setPreviewImage] = useState<string>('https://i.pinimg.com/736x/e8/c4/d3/e8c4d39d44c8945d62cd6f35e45959df.jpg');
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [formData, setFormData] = useState({
        name: mockData.Channel.Name,
        status: mockData.Channel.CurrentStatus.Name,
        createdAt: mockData.Channel.CreatedAt.split('T')[0],
        updatedAt: mockData.Channel.UpdatedAt.split('T')[0],
        totalFavorites: mockData.Channel.TotalFavorite,
        listenCount: mockData.Channel.ListenCount,
        description: description
    });

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

    const fetchChannelDetail = async () => {
        setChannelDetail(mockData.Channel)
    }

    useEffect(() => {
        fetchChannelDetail()
    }, []);

    // Set initial description in Quill
    useEffect(() => {
        if (quill) {
            const initialDescription = mockData.Channel.Description || '';
            if (initialDescription) {
                quill.setContents([
                    { insert: initialDescription }
                ]);
            }

            // Listen for text changes
            quill.on('text-change', () => {
                const content = quill.getText(); // Get plain text
                const htmlContent = quill.root.innerHTML; // Get HTML content

                // Update description state
                setDescription(content);
                // Update formData with description
                setFormData(prev => ({
                    ...prev,
                    description: htmlContent // Save HTML format or use 'content' for plain text
                }));

                // Optional: Auto-save to backend
                // handleAutoSave(htmlContent);
            });
        }
    }, [quill]);


    // Get subcategories for selected category
    const getSubCategoriesForCategory = (categoryId: number) => {
        return mockPodcastSubCategories.filter(sub => sub.PodcastCategoryId === categoryId);
    };

    const handleCategoryChange = (categoryId: number) => {
        setSelectedCategory(categoryId);
        setSelectedSubCategory(0); // Reset subcategory
    };

    const handleSave = () => {
        console.log('Saving channel data with description:', formData);
    };

    const handleRemove = () => {
        console.log('Removing channel...');
    };

    const handleUnpublish = () => {
        console.log('Unpublishing channel...');
    };

    const handleImageUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (e) => {
                const imageUrl = e.target?.result as string;
                // Here you would normally call an image cropping library
                // For now, we'll just set the preview
                setPreviewImage(imageUrl);
            };
            reader.readAsDataURL(file);
        }
    };

    const handleAddHashtag = () => {
        if (hashtagInput.trim() && !selectedHashtags.includes(hashtagInput.trim())) {
            // TODO: Call API to save tag first
            setSelectedHashtags(prev => [...prev, hashtagInput.trim()]);
            setHashtagInput('');
        }
    };

    const handleHashtagKeyPress = (event: React.KeyboardEvent) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            handleAddHashtag();
        }
    };

    const handleRemoveHashtag = (tagToRemove: string) => {
        setSelectedHashtags(prev => prev.filter(tag => tag !== tagToRemove));
    };

    return (
        <div className="channel-overview-page">
            <Typography variant="h4" className="channel-overview-page__title">
                Channel Details
            </Typography>
            <div className="channel-overview-page__actions">
                <Button
                    variant="contained"
                    color="error"
                    className="channel-overview-page__action-btn channel-overview-page__action-btn--remove"
                    onClick={handleRemove}
                >
                    Remove
                </Button>
                <Button
                    variant="outlined"
                    className="channel-overview-page__action-btn channel-overview-page__action-btn--unpublish"
                    onClick={handleUnpublish}
                >
                    Unpublish
                </Button>
                <Button
                    variant="contained"
                    className="channel-overview-page__action-btn channel-overview-page__action-btn--save"
                    onClick={handleSave}
                >
                    Save
                </Button>
                <Button
                    variant="text"
                    className="channel-overview-page__action-btn channel-overview-page__action-btn--more"
                >
                    ⋮
                </Button>
            </div>

            <div className="channel-overview-page__content">
                {/* Form Section */}
                <div className="channel-overview-page__form">
                    {/* Channel Name and Status Row */}
                    <div className="channel-overview-page__row">
                        <TextField
                            label="Name"
                            value={formData.name}
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                            className="channel-overview-page__input channel-overview-page__input--name"
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    '& fieldset': { borderColor: '#999999 !important' },
                                    '&:hover fieldset': { borderColor: '#999999 !important' },
                                    '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                },
                            }}
                        />

                        <TextField
                            label="Status"
                            value={formData.status}
                            disabled
                            className="channel-overview-page__input channel-overview-page__input--status"
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    backgroundColor: '#77898e9d',
                                }
                            }}
                        />
                    </div>

                    {/* Category and Subcategory Row */}
                    <div className="channel-overview-page__row">
                        <FormControl className="channel-overview-page__select">
                            <InputLabel >
                                Category
                            </InputLabel>
                            <Select
                                value={selectedCategory}
                                onChange={(e) => handleCategoryChange(e.target.value as number)}
                                sx={{
                                    '& .MuiOutlinedInput-notchedOutline': { borderColor: '#999999' },
                                    '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: '#999999' },
                                    '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: '#999999' },
                                    '& .MuiSvgIcon-root': { color: 'white' }
                                }}
                            >
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
                            </Select>
                        </FormControl>

                        <FormControl className="channel-overview-page__select">
                            <InputLabel sx={{ color: '#888', '&.Mui-focused': { color: 'var(--primary-green)' } }}>
                                Sub Category
                            </InputLabel>
                            <Select
                                value={selectedSubCategory}
                                onChange={(e) => setSelectedSubCategory(e.target.value as number)}
                                sx={{
                                    '& .MuiOutlinedInput-notchedOutline': { borderColor: '#999999' },
                                    '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: '#999999' },
                                    '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: '#999999' },
                                    '& .MuiSvgIcon-root': { color: 'white' }
                                }}
                            >
                                {getSubCategoriesForCategory(selectedCategory).length > 0 ? (
                                    getSubCategoriesForCategory(selectedCategory).map((subCategory) => (
                                        <MenuItem key={subCategory.Id} value={subCategory.Id}>
                                            {subCategory.Name}
                                        </MenuItem>
                                    ))
                                ) : (
                                    <MenuItem value={0} disabled>
                                        --
                                    </MenuItem>
                                )}
                            </Select>
                        </FormControl>
                    </div>

                    {/* Dates and Numbers Row */}
                    <div className="channel-overview-page__row">
                        <TextField
                            label="Created At"
                            type="date"
                            value={formData.createdAt}
                            disabled
                            className="channel-overview-page__input-small"
                            sx={{
                                  '& .MuiOutlinedInput-root': {
                                    backgroundColor: '#77898e9d',
                                }
                            }}
                        />
                        <TextField
                            label="Updated At"
                            type="date"
                            value={formData.updatedAt}
                            disabled
                            className="channel-overview-page__input-small"
                            sx={{
                               '& .MuiOutlinedInput-root': {
                                    backgroundColor: '#77898e9d',
                                }
                            }}
                        />
                        <TextField
                            label="Total Favorite"
                            value={formData.totalFavorites}
                            disabled
                            className="channel-overview-page__input-small"
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    backgroundColor: '#77898e9d',
                                }
                            }}
                        />
                        <TextField
                            label="Listen Count"
                            value={formData.listenCount}
                            disabled
                            className="channel-overview-page__input-small"
                            sx={{
                               '& .MuiOutlinedInput-root': {
                                    backgroundColor: '#77898e9d',
                                }
                            }}
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
                                InputProps={{
                                    endAdornment: (
                                        <InputAdornment position="end">
                                            <IconButton
                                                onClick={handleAddHashtag}
                                                disabled={!hashtagInput.trim() || selectedHashtags.includes(hashtagInput.trim())}
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
                                    label={tag}
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
                <div className="channel-overview-page__preview">
                    <div className="channel-overview-page__main-image-container">
                        <img
                            src={previewImage}
                            alt={formData.name}
                            className="channel-overview-page__main-image-file"
                        />
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
                        accept="image/*"
                        style={{ display: 'none' }}
                    />

                    <Typography variant="h6" className="channel-overview-page__preview-title">
                        Preview
                    </Typography>
                    <Card className="channel-overview-page__preview-card">
                        <div className="channel-overview-page__preview-image-container">
                            <CardMedia
                                component="img"
                                image={previewImage}
                                alt={formData.name}
                                className="channel-overview-page__preview-bg-image"
                            />
                            <div className="channel-overview-page__preview-overlay">
                                <div className="channel-overview-page__preview-content">
                                    <img
                                        src={previewImage}
                                        alt={formData.name}
                                        className="channel-overview-page__preview-avatar"
                                    />
                                    <div className="channel-overview-page__preview-info">
                                        <Typography variant="h6" className="channel-overview-page__preview-name">
                                            {formData.name}
                                        </Typography>
                                        <Typography variant="body2" className="channel-overview-page__preview-subtitle">
                                            SAMURICE
                                        </Typography>
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

export default ChannelOverview;
