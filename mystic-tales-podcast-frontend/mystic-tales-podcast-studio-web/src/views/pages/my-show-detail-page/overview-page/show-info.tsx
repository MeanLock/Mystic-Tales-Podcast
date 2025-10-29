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
    InputAdornment,
    Rating,
    Tabs,
    Tab,
} from '@mui/material';
import { Add } from '@mui/icons-material';
import { useQuill } from 'react-quilljs';
import 'quill/dist/quill.snow.css';
import './styles.scss';
export const Language = [
    { Id: 1, Name: "English" },
    { Id: 2, Name: "Vietnamese" },
];
export const mockPodcastCategories = [
    { Id: 1, Name: "True Crime " },
    { Id: 2, Name: "Horror " },
    { Id: 3, Name: "Society & Culture " },
    { Id: 4, Name: "Psychology " },
    { Id: 5, Name: "Philosophy " },
    { Id: 6, Name: "History " },
    { Id: 7, Name: "Comics" },
];
export const mockSubscriptionTypes = [
    { Id: 1, Name: "Free" },
    { Id: 2, Name: "Subscriber only" },
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
    Show: {
        Id: 1,
        Name: "Mystic Tales",
        Description: "A podcast channel sharing mysterious stories and real-life paranormal events.",
        Language: "English",
        Copyright: "© 2025 Mystic Tales",
        ReleaseDate: "2025-10-20T07:29:44.480Z",
        UploadFrequency: "weekly",
        RatingCount: 10,
        AverageRating: 4.5,
        MainImageFileKey: "main_mystic_tales.png",
        TrailerAudioFileKey: null,
        TotalFollow: 1520,
        ListenCount: 34900,
        PodcastShowSubscriptionType: {
            Id: 2,
            Name: "Subscriber only"
        },
        PodcastChannel: {
            Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            Name: "Thần Tiên Podcast",
            MainImageFileKey: "main_mystic_tales.png"
        },
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
        TakenDownReason: "Content violation due to inappropriate material.",
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

const ShowInfo = () => {
    const [showDetail, setShowDetail] = useState<any>(mockData.Show);
    const [selectedCategory, setSelectedCategory] = useState<number>(mockData.Show.PodcastCategory.Id);
    const [selectedSubCategory, setSelectedSubCategory] = useState<number>(mockData.Show.PodcastSubCategory.Id);
    const [selectedSubscriptionType, setSelectedSubscriptionType] = useState<number>(mockData.Show.PodcastShowSubscriptionType.Id);

    const [selectedHashtags, setSelectedHashtags] = useState<string[]>(
        mockData.Show.Hashtags.map((tag: any) => tag.Name)
    );
    const [description, setDescription] = useState<string>(mockData.Show.Description || '');

    const [hashtagInput, setHashtagInput] = useState<string>('');
    const [previewImage, setPreviewImage] = useState<string>('https://i.pinimg.com/736x/e8/c4/d3/e8c4d39d44c8945d62cd6f35e45959df.jpg');
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [formData, setFormData] = useState({
        name: mockData.Show.Name,
        status: mockData.Show.CurrentStatus.Name,
        createdAt: mockData.Show.CreatedAt.split('T')[0],
        updatedAt: mockData.Show.UpdatedAt.split('T')[0],
        totalFollow: mockData.Show.TotalFollow,
        language: mockData.Show.Language,

        listenCount: mockData.Show.ListenCount,
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

    const fetchShowDetail = async () => {
        setShowDetail(mockData.Show)
    }

    useEffect(() => {
        fetchShowDetail()
    }, []);

    // Set initial description in Quill
    useEffect(() => {
        if (quill) {
            const initialDescription = mockData.Show.Description || '';
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
        setSelectedSubCategory(1); // Reset subcategory
    };
    const handleSubscriptionTypeChange = (typeId: number) => {
        setSelectedSubscriptionType(typeId);
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

    const [activeTab, setActiveTab] = useState("show-info");

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
        <div className="show-info-page">
            {showDetail.TakenDownReason && (
                <div className="flex items-center gap-2 bg-red-100 border border-red-400  rounded px-3 py-2 mb-3" style={{ width: "fit-content" }}>
                    <svg className="w-5 h-5 text-red-500 shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                    </svg>
                    <span className="text-xs text-red-700 font-medium">
                        <strong>Taken Down Reason:</strong> {showDetail.TakenDownReason}
                    </span>
                </div>
            )}
            <div className="show-info-page__actions">
                <Button
                    variant="contained"
                    color="error"
                    className="show-info-page__action-btn show-info-page__action-btn--remove"
                    onClick={handleRemove}
                >
                    Remove
                </Button>
                <Button
                    variant="outlined"
                    className="show-info-page__action-btn show-info-page__action-btn--unpublish"
                    onClick={handleUnpublish}
                >
                    Unpublish
                </Button>
                <Button
                    variant="contained"
                    className="show-info-page__action-btn show-info-page__action-btn--save"
                    onClick={handleSave}
                >
                    Save
                </Button>
                <Button
                    variant="text"
                    className="show-info-page__action-btn show-info-page__action-btn--more"
                >
                    ⋮
                </Button>
            </div>


            <div className="show-info-page__content">
                {/* Form Section */}
                <div className="show-info-page__form">
                    {/* Channel Name and Status Row */}
                    <div className="show-info-page__row">
                        <TextField
                            label="Name"
                            value={formData.name}
                            variant="standard"
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                            className="show-info-page__input show-info-page__input--name"
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
                            value={showDetail.CurrentStatus.Name}
                            className="show-info-page__input show-info-page__input--status"

                        />
                    </div>
                    <div className="show-info-page__row">
                        <TextField
                            select
                            label="Channel"
                            variant="standard"
                            value={selectedCategory}
                            onChange={(e) => handleCategoryChange(e.target.value as unknown as number)}
                            className="show-info-page__select"
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
                        </TextField>
                        <TextField
                            select
                            label="Language"
                            variant="standard"
                            value={formData.language}
                            onChange={(e) => setFormData({ ...formData, language: e.target.value })}

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
                            value={selectedCategory}
                            onChange={(e) => handleCategoryChange(e.target.value as unknown as number)}
                            className="show-info-page__select"
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
                        </TextField>

                    </div>

                    {/* Category and Subcategory Row */}
                    <div className="show-info-page__row">
                        <TextField
                            select
                            label="Category"
                            variant="standard"
                            value={selectedCategory}
                            onChange={(e) => handleCategoryChange(e.target.value as unknown as number)}
                            className="show-info-page__select"
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
                        </TextField>
                        <TextField
                            select
                            label="Subcategory"
                            variant="standard"
                            value={selectedSubCategory}
                            onChange={(e) => setSelectedSubCategory(e.target.value as unknown as number)}
                            className="show-info-page__select"
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
                        </TextField>

                        <TextField
                            select
                            label="Subscription Type"
                            variant="standard"
                            value={selectedSubscriptionType}
                            onChange={(e) => setSelectedSubscriptionType(e.target.value as unknown as number)}
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

                    {/* Dates and Numbers Row */}
                    <div className="show-info-page__row">
                        {showDetail.ReleaseDate !== null && (
                            <TextField
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Release Date"
                                type="date"
                                value={showDetail.ReleaseDate.split('T')[0]}
                                className="show-info-page__input-small"

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
                            type="date"
                            value={formData.createdAt}
                            className="show-info-page__input-small"

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
                            type="date"
                            value={formData.updatedAt}
                            className="show-info-page__input-small"

                        />
                        <TextField
                            id="filled-helperText"
                            variant="filled"
                            slotProps={{
                                input: {
                                    readOnly: true,
                                },
                            }}
                            label="Total Followers"
                            value={showDetail.TotalFollow}
                            className="show-info-page__input-small"

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
                            value={showDetail.ListenCount}
                            className="show-info-page__input-small"

                        />
                        <TextField
                            id="filled-helperText"
                            variant="filled"
                            slotProps={{
                                input: {
                                    readOnly: true,
                                },
                            }}
                            label="Rating Average"
                            value={`${showDetail.AverageRating} ⭐`}
                            className="show-info-page__input-small"

                        />

                    </div>


                    <div className="show-info-page__hashtags">
                        <div className="show-info-page__hashtag-input ">
                            <TextField
                                label="Add hashtag"
                                value={hashtagInput}
                                onChange={(e) => setHashtagInput(e.target.value)}
                                onKeyPress={handleHashtagKeyPress}
                                size="small"
                                className="show-info-page__hashtag-field"
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

                            <TextField
                                label="Copyright"
                                value={formData.name}
                                variant="standard"
                                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
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
                        <div className="show-info-page__hashtag-chips">
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
                        <img
                            src={previewImage}
                            alt={formData.name}
                            className="show-info-page__main-image-file"
                        />
                        <Button
                            className="show-info-page__change-artwork-btn"
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

                    <Typography variant="h6" className="show-info-page__preview-title">
                        Preview
                    </Typography>
                    <Card className="show-info-page__preview-card">
                        <div className="show-info-page__preview-image-container">
                            <CardMedia
                                component="img"
                                image={previewImage}
                                alt={formData.name}
                                className="show-info-page__preview-bg-image"
                            />
                            <div className="show-info-page__preview-overlay">
                                <div className="show-info-page__preview-content">
                                    <img
                                        src={previewImage}
                                        alt={formData.name}
                                        className="show-info-page__preview-avatar"
                                    />
                                    <div className="show-info-page__preview-info">
                                        <Typography variant="h6" className="show-info-page__preview-name">
                                            {formData.name}
                                        </Typography>
                                        <Typography variant="body2" className="show-info-page__preview-subtitle">
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

export default ShowInfo;
