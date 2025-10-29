import React, { useEffect, useState, useRef, useContext } from 'react';
import {
    Box,
    Typography,
    TextField,
    Button,
    Card,
    CardMedia,
    MenuItem,


} from '@mui/material';

import { useQuill } from 'react-quilljs';
import 'quill/dist/quill.snow.css';
import { ProfileViewContext } from '.';



const ProfileInfo = () => {
    const context = useContext(ProfileViewContext);
    const profile = context?.profile ?? null;
    const [profileData, setProfileData] = useState<any | null>(null);
    const [description, setDescription] = useState<string>('');
    const [previewImage, setPreviewImage] = useState<string>('https://i.pinimg.com/736x/e8/c4/d3/e8c4d39d44c8945d62cd6f35e45959df.jpg');
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [formData, setFormData] = useState({
        name: '',
        email: '',
        role: '',
        phone: '',
        gender: '',
        address: '',
        balance: 0,
        createdAt: '',
        updatedAt: '',
        studioName: '',
        avgRating: 0,
        ratingCount: 0,
        totalFollow: 0,
        listenCount: 0,
        ownedStorage: 0,
        usedStorage: 0,
        isVerified: false,
        description: ''
    });
    // populate local state when context provides data
    useEffect(() => {
        console.log('Profile data changed:', profile);
        if (!profile) return;
        setProfileData(profile);
        const pp = profile.PodcasterProfile ?? {};
        console.log('Podcaster Profile Description:', pp.Description);
        setDescription(pp.Description);
        setPreviewImage(profile.MainImageFileKey || previewImage);
        setFormData({
            name: profile.FullName ?? '',
            email: profile.Email ?? '',
            role: profile.Role?.Name ?? '',
            phone: profile.Phone ?? '',
            gender: profile.Gender ?? '',
            address: profile.Address ?? '',
            balance: profile.Balance ?? 0,
            createdAt: (profile.CreatedAt ?? '').split('T')[0] ?? '',
            updatedAt: (profile.UpdatedAt ?? '').split('T')[0] ?? '',
            studioName: pp.Name ?? '',
            avgRating: pp.AverageRating ?? 0,
            ratingCount: pp.RatingCount ?? 0,
            totalFollow: pp.TotalFollow ?? 0,
            listenCount: pp.ListenCount ?? 0,
            ownedStorage: pp.OwnedBookingStorageSize ?? 0,
            usedStorage: pp.UsedBookingStorageSize ?? 0,
            isVerified: !!(profile.IsVerified || pp.IsVerified),
            description: pp.Description ?? ''
        });
    }, [profile]);

    const data = profileData ?? profile;

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
        if (quill && data?.PodcasterProfile.Description) {
            const initialDescription = data.PodcasterProfile.Description || '';
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

    if (!profileData) return <div>Loading...</div>;

    return (
        <div className="profile-info-page ">
            <div className="profile-info-page__actions">
                <Button
                    variant="contained"
                    color="error"
                    className="profile-info-page__action-btn profile-info-page__action-btn--remove"
                    onClick={handleRemove}
                >
                    Remove
                </Button>
                <Button
                    variant="outlined"
                    className="profile-info-page__action-btn profile-info-page__action-btn--unpublish"
                    onClick={handleUnpublish}
                >
                    Unpublish
                </Button>
                <Button
                    variant="contained"
                    className="profile-info-page__action-btn profile-info-page__action-btn--save"
                    onClick={handleSave}
                >
                    Save
                </Button>
                <Button
                    variant="text"
                    className="profile-info-page__action-btn profile-info-page__action-btn--more"
                >
                    ⋮
                </Button>
            </div>


            <div className="profile-info-page__content">
                {/* Form Section */}
                <div className="profile-info-page__form">
                    {/* Profile Name and Verified Row */}
                    <div className="profile-info-page__row">
                        <TextField
                            label="Name"
                            value={formData.name}
                            variant="standard"
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                            className="profile-info-page__input profile-info-page__input--name"
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
                            slotProps={{ input: { readOnly: true } }}
                            label="Email"
                            value={formData.email}
                            className="profile-info-page__input profile-info-page__input--email"
                        />
                    </div>
                    {/* Contact & Account Info */}
                    <div className="profile-info-page__row">
                        <TextField
                            select
                            variant="standard"
                            label="Gender"
                            value={formData.gender}
                            onChange={(e) => setFormData({ ...formData, gender: e.target.value })}
                            className="profile-info-page__select"
                        >
                            <MenuItem value="Male">Male</MenuItem>
                            <MenuItem value="Female">Female</MenuItem>
                            <MenuItem value="Other">Other</MenuItem>
                        </TextField>
                       <TextField
                            label="Phone"
                            value={formData.phone}
                            variant="standard"
                            type='number'
                            onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                            className="profile-info-page__input profile-info-page__input--name"
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    '& fieldset': { borderColor: '#999999 !important' },
                                    '&:hover fieldset': { borderColor: '#999999 !important' },
                                    '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                },
                            }}
                        />
                        <TextField
                            variant="filled"
                            slotProps={{ input: { readOnly: true } }}
                            label="Phone"
                            value={formData.phone}
                            className="profile-info-page__input-small"
                        />
                        <TextField
                            variant="filled"
                            slotProps={{ input: { readOnly: true } }}
                            label="DoB"
                            value={formData.phone}
                            className="profile-info-page__input-small"
                        />
                    </div>

                    <div className="profile-info-page__row">
                        <TextField
                            label="Address"
                            value={formData.address}
                            variant="standard"
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                            className="profile-info-page__input profile-info-page__input--name"
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    '& fieldset': { borderColor: '#999999 !important' },
                                    '&:hover fieldset': { borderColor: '#999999 !important' },
                                    '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                },
                            }}
                        />
                        <TextField
                            variant="filled"
                            slotProps={{ input: { readOnly: true } }}
                            label="Balance"
                            value={formData.balance}
                            className="profile-info-page__input-small"
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
                            value={`${formData.avgRating} ⭐`}
                            className="profile-info-page__input-small"

                        />
                    </div>




                    <div className="profile-info-page__row">
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
                            className="profile-info-page__input-small"

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
                            className="profile-info-page__input-small"

                        />
                        <TextField
                            id="filled-helperText"
                            variant="filled"
                            slotProps={{ input: { readOnly: true } }}
                            label="Total Follow"
                            value={formData.totalFollow}
                            className="profile-info-page__input-small"
                        />
                        <TextField
                            id="filled-helperText"
                            variant="filled"
                            slotProps={{ input: { readOnly: true } }}
                            label="Listen Count"
                            value={formData.listenCount}
                            className="profile-info-page__input-small"
                        />
                        <TextField
                            id="filled-helperText"
                            variant="filled"
                            slotProps={{ input: { readOnly: true } }}
                            label="Owned Storage (MB)"
                            value={formData.ownedStorage}
                            className="profile-info-page__input-small"
                        />
                        <TextField
                            id="filled-helperText"
                            variant="filled"
                            slotProps={{ input: { readOnly: true } }}
                            label="Used Storage (MB)"
                            value={formData.usedStorage}
                            className="profile-info-page__input-small"
                        />
                    </div>

                    {/* Description */}
                    <div className="profile-info-page__description">
                        <Typography variant="body2" className="profile-info-page__description-label">
                            Description
                        </Typography>
                        <div className="profile-info-page__description-editor">
                            <div ref={quillRef} />
                        </div>

                    </div>
                </div>

                {/* Preview Section */}
                <div className="profile-info-page__preview">
                    <div className="profile-info-page__main-image-container">
                        <img
                            src={previewImage}
                            alt={formData.name}
                            className="profile-info-page__main-image-file"
                        />
                        <Button
                            className="profile-info-page__change-artwork-btn"
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

                    <Typography variant="h6" className="profile-info-page__preview-title">
                        Preview
                    </Typography>
                    <Card className="profile-info-page__preview-card">
                        <div className="profile-info-page__preview-image-container">
                            <CardMedia
                                component="img"
                                image={previewImage}
                                alt={formData.name}
                                className="profile-info-page__preview-bg-image"
                            />
                            <div className="profile-info-page__preview-overlay">
                                <div className="profile-info-page__preview-content">
                                    <img
                                        src={previewImage}
                                        alt={formData.name}
                                        className="profile-info-page__preview-avatar"
                                    />
                                    <div className="profile-info-page__preview-info">
                                        <Typography variant="h6" className="profile-info-page__preview-name">
                                            {formData.name}
                                        </Typography>
                                        <Typography variant="body2" className="profile-info-page__preview-subtitle">
                                            {formData.studioName || formData.role}
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

export default ProfileInfo;
