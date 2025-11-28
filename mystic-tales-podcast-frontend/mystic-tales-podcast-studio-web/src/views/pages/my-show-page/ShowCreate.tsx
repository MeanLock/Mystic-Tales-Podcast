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
import logo from '../../../assets/logoMTP.jpg';

/**
 * ShowCreate Component - Create new podcast show
 * 
 * API Payload Structure:
 * {
 *   "Copyright": "string",
 *   "Name": "string",
 *   "HashtagIds": [0],
 *   "PodcasterId": 0,
 *   "PodcastShowSubscriptionTypeId": 0,
 *   "PodcastSubCategoryId": 0,
 *   "Language": "English",
 *   "PodcastChannelId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" | null,
 *   "UploadFrequency": "string",
 *   "Description": "string",
 *   "PodcastCategoryId": 0
 * }
 * + MainImageFile: File
 */

export const Language = [
    { Id: 1, Name: "English" },
    { Id: 2, Name: "Vietnamese" },
];
export const mockChannel = [
    { Id: null, Name: "Single Show" },
    { Id: '3fa85f64-5717-4562-b3fc-2c963f66afa6', Name: "Thần Tiên Podcast" },
    { Id: '3fa85f64-5717-4562-b3fc-2c963f66afa7', Name: "Channel 2" },
    { Id: '3fa85f64-5717-4562-b3fc-2c963f66afa8', Name: "Channel 4" },
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
export const UploadFrequencyList = [
    { Name: "Daily" },
    { Name: "Weekly" },
    { Name: "Monthly" },
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

const ShowCreate = () => {
    const [channel, setChannel] = useState<string | null>(null);
    const [selectedCategory, setSelectedCategory] = useState<number>(1);
    const [selectedSubCategory, setSelectedSubCategory] = useState<number>(1);
    const [selectedSubscriptionType, setSelectedSubscriptionType] = useState<number>(1);
    const [uploadFrequency, setUploadFrequency] = useState<string>('');
    const [selectedHashtags, setSelectedHashtags] = useState<number[]>([]);
    const [description, setDescription] = useState<string>('');
    const [hashtagInput, setHashtagInput] = useState<string>('');
    const [previewImage, setPreviewImage] = useState<string>('');
    const [mainImageFile, setMainImageFile] = useState<File | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const [formData, setFormData] = useState({
        name: '',
        copyright: '',
        language: 'English',
        uploadFrequency: '',
        description: ''
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

    // Set up Quill editor
    useEffect(() => {
        if (quill) {
            // Listen for text changes
            quill.on('text-change', () => {
                const htmlContent = quill.root.innerHTML;
                setDescription(htmlContent);
                setFormData(prev => ({
                    ...prev,
                    description: htmlContent
                }));
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
        // Prepare API payload
        const payload = {
            Copyright: formData.copyright,
            Name: formData.name,
            HashtagIds: selectedHashtags,
            PodcasterId: 0, // TODO: Get from user context
            PodcastShowSubscriptionTypeId: selectedSubscriptionType,
            PodcastSubCategoryId: selectedSubCategory,
            Language: formData.language,
            PodcastChannelId: channel,
            UploadFrequency: uploadFrequency,
            Description: formData.description,
            PodcastCategoryId: selectedCategory
        };

        console.log('Creating show with data:', payload);
        console.log('Main image file:', mainImageFile);
        // TODO: Call API with payload and mainImageFile
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
        const hashtagId = parseInt(hashtagInput.trim());
        if (hashtagId && !isNaN(hashtagId) && !selectedHashtags.includes(hashtagId)) {
            // TODO: Validate hashtag ID exists via API
            setSelectedHashtags(prev => [...prev, hashtagId]);
            setHashtagInput('');
        }
    };

    const handleHashtagKeyPress = (event: React.KeyboardEvent) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            handleAddHashtag();
        }
    };

    const handleRemoveHashtag = (tagToRemove: number) => {
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
            <Typography variant="h4" className="show-info-page__title">
                Create New Show
            </Typography>
            <div className="show-info-page__actions">
                <Button
                    variant="contained"
                    className="show-info-page__action-btn show-info-page__action-btn--save"
                    onClick={handleSave}
                >
                    Save
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
                    </div>
                    <div className="show-info-page__row">
                        <TextField
                            select
                            label="Channel"
                            variant="standard"
                            value={channel === null ? '' : channel}
                            onChange={(e) => setChannel(e.target.value === '' ? null : e.target.value)}
                            className="show-info-page__select"
                        >
                            {mockChannel.map((channelItem) => (
                                <MenuItem
                                    key={channelItem.Id === null ? 'single-show' : channelItem.Id}
                                    value={channelItem.Id === null ? '' : channelItem.Id}
                                    sx={{
                                        '& .MuiPaper-root': { backgroundColor: '#77898e9d' },
                                        fontStyle: channelItem.Id === null ? 'italic' : 'normal',
                                        color: channelItem.Id === null ? '#888' : 'inherit'
                                    }}
                                >
                                    {channelItem.Name}
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
                            value={uploadFrequency}
                            onChange={(e) => {
                                setUploadFrequency(e.target.value);
                                setFormData({ ...formData, uploadFrequency: e.target.value });
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

                    {/* Dates and Numbers Row - Remove for Create */}
                    {/* These fields are not needed for create form */}

                    <div className="show-info-page__hashtags">
                        <div className="show-info-page__hashtag-input ">
                            <TextField
                                label="Add hashtag ID"
                                value={hashtagInput}
                                onChange={(e) => setHashtagInput(e.target.value)}
                                onKeyPress={handleHashtagKeyPress}
                                size="small"
                                type="number"
                                className="show-info-page__hashtag-field"
                                InputProps={{
                                    endAdornment: (
                                        <InputAdornment position="end">
                                            <IconButton
                                                onClick={handleAddHashtag}
                                                disabled={!hashtagInput.trim() || selectedHashtags.includes(parseInt(hashtagInput.trim()))}
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
                                value={formData.copyright}
                                variant="standard"
                                onChange={(e) => setFormData({ ...formData, copyright: e.target.value })}
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
                                    label={`Hashtag #${tag}`}
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
                                alt={formData.name || 'Preview'}
                                className="show-info-page__main-image-file"
                            />
                        ) : (
                            <img
                                src={logo}
                                alt={formData.name || 'Preview'}
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
                        accept="image/*"
                        style={{ display: 'none' }}
                    />
                </div>
            </div>
        </div>
    );
};

export default ShowCreate;
