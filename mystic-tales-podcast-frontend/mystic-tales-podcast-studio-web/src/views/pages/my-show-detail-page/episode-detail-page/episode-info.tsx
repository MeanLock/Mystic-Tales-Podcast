import React, { useEffect, useState, useRef, use, useContext } from 'react';
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
import { Add } from '@mui/icons-material';
import { useQuill } from 'react-quilljs';
import 'quill/dist/quill.snow.css';
import { EpisodeDetailViewContext } from '.';
import { s } from 'graphql-ws/dist/common-DY-PBNYy';

export const mockSubscriptionTypes = [
    { Id: 1, Name: "Free" },
    { Id: 2, Name: "Subscriber only" },
    { Id: 3, Name: "Bonus" },
    { Id: 4, Name: "Archive" },
];



const availableHashtags = [
    '#Mystery', '#HorrorStories', '#Paranormal', '#TrueCrime', '#Supernatural',
    '#Investigation', '#Thriller', '#Creepy', '#Folklore', '#Legend',
    '#SerialKiller', '#UnsolvedMystery', '#ColdCase', '#Detective'
];

const EpisodeInfo = () => {
    const context = useContext(EpisodeDetailViewContext);
    const episodeDetail = context?.episodeDetail ?? null;
    const [episodeData, setEpisodeData] = useState<any | null>(null);
    const [selectedSubscriptionType, setSelectedSubscriptionType] = useState<number | null>(null);
    const [selectedHashtags, setSelectedHashtags] = useState<string[]>([]);
    const [description, setDescription] = useState<string>('');
    const [hashtagInput, setHashtagInput] = useState<string>('');
    const [previewImage, setPreviewImage] = useState<string>('https://i.pinimg.com/736x/e8/c4/d3/e8c4d39d44c8945d62cd6f35e45959df.jpg');
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [formData, setFormData] = useState({
        name: '',
        explicitContent: false,
        createdAt: '',
        updatedAt: '',
        PodcastEpisodeSubscriptionTypeId: 0,
        seasonNumber: 0,
        episodeOrder: 0,
        description: ''
    });
    // populate local state when context provides data
    useEffect(() => {
        if (!episodeDetail) return;
        setEpisodeData(episodeDetail);
        setSelectedSubscriptionType(episodeDetail.PodcastEpisodeSubscriptionType?.Id ?? null);
        setSelectedHashtags((episodeDetail.Hashtags ?? []).map((h: any) => h.Name));
        setDescription(episodeDetail.Description ?? '');
        setPreviewImage(episodeDetail.MainImageFileKey ?? previewImage);
        setFormData({
            name: episodeDetail.Name ?? '',
            explicitContent: episodeDetail.ExplicitContent ?? false,
            createdAt: (episodeDetail.CreatedAt ?? '').split('T')[0] ?? '',
            updatedAt: (episodeDetail.UpdatedAt ?? '').split('T')[0] ?? '',
            PodcastEpisodeSubscriptionTypeId: episodeDetail.PodcastEpisodeSubscriptionTypeId ?? 1,
            seasonNumber: episodeDetail.SeasonNumber ?? 0,
            episodeOrder: episodeDetail.EpisodeOrder ?? 0,
            description: episodeDetail.Description ?? ''

        });
    }, [episodeDetail]);

    // use a single source for rendering to avoid null access
    const data = episodeData ?? episodeDetail;

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

    // Set initial description in Quill
    useEffect(() => {
        if (quill && data?.Description) {
            const initialDescription = data.Description || '';
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
    }, [quill, data]);




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

    if (!episodeDetail)
        return <div>Loading...</div>;

    return (
        <div className="episode-info-page ">
            {data?.TakenDownReason != null && (
                <div className="flex items-center gap-2 bg-red-100 border border-red-400  rounded px-3 py-2 mb-3 " style={{ width: "fit-content" }}>
                    <svg className="w-5 h-5 text-red-500 shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                    </svg>
                    <span className="text-xs text-red-700 font-medium">
                        <strong>Taken Down Reason:</strong> {data?.TakenDownReason}
                    </span>
                </div>
            )}
            <div className="episode-info-page__actions">
                <Button
                    variant="contained"
                    color="error"
                    className="episode-info-page__action-btn episode-info-page__action-btn--remove"
                    onClick={handleRemove}
                >
                    Remove
                </Button>
                <Button
                    variant="outlined"
                    className="episode-info-page__action-btn episode-info-page__action-btn--unpublish"
                    onClick={handleUnpublish}
                >
                    Unpublish
                </Button>
                <Button
                    variant="contained"
                    className="episode-info-page__action-btn episode-info-page__action-btn--save"
                    onClick={handleSave}
                >
                    Save
                </Button>
                <Button
                    variant="text"
                    className="episode-info-page__action-btn episode-info-page__action-btn--more"
                >
                    ⋮
                </Button>
            </div>


            <div className="episode-info-page__content">
                {/* Form Section */}
                <div className="episode-info-page__form">
                    {/* Channel Name and Status Row */}
                    <div className="episode-info-page__row">
                        <TextField
                            label="Name"
                            value={formData.name}
                            variant="standard"
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
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
                            value={data?.CurrentStatus?.Name ?? ''}
                            className="episode-info-page__input episode-info-page__input--status"

                        />
                    </div>
                    <div className="episode-info-page__row">
                        <TextField
                            select
                            label="Subscription Type"
                            variant="standard"
                            value={formData.PodcastEpisodeSubscriptionTypeId ?? 1}
                            onChange={(e) => setFormData({ ...formData, PodcastEpisodeSubscriptionTypeId: e.target.value as unknown as number })}
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
                            value={formData.explicitContent ?? ''}
                            onChange={(e) => setFormData({ ...formData, explicitContent: e.target.value === 'true' })}
                            className="episode-info-page__select"
                        >
                            <MenuItem value="true">True</MenuItem>
                            <MenuItem value="false">False</MenuItem>
                        </TextField>
                        <TextField
                            label="Season"
                            value={formData.seasonNumber}
                            type="number"
                            variant="standard"
                            inputProps={{ min: 1 }}
                            onChange={(e) => {
                                const val = e.target.value;
                                setFormData({ ...formData, seasonNumber: val === '' ? 0 : Number(val) });
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
                            value={formData.episodeOrder}
                            type="number"
                            variant="standard"
                            inputProps={{ min: 1 }}
                            onChange={(e) => {
                                const val = e.target.value;
                                setFormData({ ...formData, episodeOrder: val === '' ? 0 : Number(val) });
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
                            value={data?.PodcastShow?.Name ?? ''}
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
                            value={data.IsReleased ? 'Yes' : 'No'}
                            className="episode-info-page__input-small"

                        />
                    </div>

                    {/* Dates and Numbers Row */}
                    <div className="episode-info-page__row">
                        {data?.ReleaseDate != null && (
                            <TextField
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Release Date"
                                type="date"
                                value={data?.ReleaseDate?.split('T')[0] ?? ''}
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
                            type="date"
                            value={formData.createdAt}
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
                            type="date"
                            value={formData.updatedAt}
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
                            value={data?.TotalSave ?? 0}
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
                            value={data?.ListenCount ?? 0}
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
                        <div className="episode-info-page__hashtag-chips">
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
                    <div className="episode-info-page__description">
                        <Typography variant="body2" className="episode-info-page__description-label">
                            Description
                        </Typography>
                        <div className="episode-info-page__description-editor">
                            <div ref={quillRef} />
                        </div>

                    </div>
                </div>

                {/* Preview Section */}
                <div className="episode-info-page__preview">
                    <div className="episode-info-page__main-image-container">
                        <img
                            src={previewImage}
                            alt={formData.name}
                            className="episode-info-page__main-image-file"
                        />
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
                        accept="image/*"
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
                                alt={formData.name}
                                className="episode-info-page__preview-bg-image"
                            />
                            <div className="episode-info-page__preview-overlay">
                                <div className="episode-info-page__preview-content">
                                    <img
                                        src={previewImage}
                                        alt={formData.name}
                                        className="episode-info-page__preview-avatar"
                                    />
                                    <div className="episode-info-page__preview-info">
                                        <Typography variant="h6" className="episode-info-page__preview-name">
                                            {formData.name}
                                        </Typography>
                                        <Typography variant="body2" className="episode-info-page__preview-subtitle">
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

export default EpisodeInfo;
