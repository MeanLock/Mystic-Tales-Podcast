import React, { useEffect, useState, useRef, useContext, useMemo } from 'react';
import {
    Box,
    Typography,
    TextField,
    Button,
    Card,
    CardMedia,
    MenuItem,
    Dialog,
    DialogTitle,
    IconButton,
    DialogContent,
    FormControlLabel,
    Switch,
    SwitchProps,
    styled,
    Tooltip,
    Chip,
    Skeleton,
    Collapse,
    InputBase,
    InputAdornment,
} from '@mui/material';
import { ExpandMore, ExpandLess, Search } from '@mui/icons-material';

import { useQuill } from 'react-quilljs';
import 'quill/dist/quill.snow.css';
import { ProfileViewContext } from '.';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { updatePodcasterProfile } from '@/core/services/account/account.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { toast } from 'react-toastify';
import Loading from '@/views/components/common/loading';
import { s } from 'graphql-ws/dist/common-DY-PBNYy';
import { format } from 'path';
import { formatDate } from '@/core/utils/date.util';
import { getBuddyCommitment } from '@/core/services/file/file.service';
import { DocumentViewer } from '@/views/components/common/document';
import { Close } from '@mui/icons-material';
import { Question } from 'phosphor-react';
import { useDispatch, useSelector } from 'react-redux';
import { setAuthToken } from '@/redux/auth/authSlice';
import { RootState } from '@/redux/rootReducer';
import { getBookingTone, getBookingToneList, updateBookingTone } from '@/core/services/booking/booking.service';
import { BookingTone } from '@/core/types';
import { set } from 'lodash';


interface ProfileInfoProps {
    loading: boolean;
}

const IOSSwitch = styled((props: SwitchProps) => (
    <Switch focusVisibleClassName=".Mui-focusVisible" disableRipple {...props} />
))(({ theme }) => ({
    width: 50,
    height: 26,
    padding: 0,
    '& .MuiSwitch-switchBase': {
        padding: 0,
        margin: 2,
        transitionDuration: '300ms',
        '&.Mui-checked': {
            transform: 'translateX(24px)',
            color: '#fff',
            '& + .MuiSwitch-track': {
                backgroundColor: '#aee339',
                border: 0,
                opacity: 2,
            },
            '&.Mui-disabled + .MuiSwitch-track': {
                opacity: 0.5,
            },
        },
        '&.Mui-focusVisible .MuiSwitch-thumb': {
            color: '#aee339',
            border: '6px solid #fff',
        },
        '&.Mui-disabled .MuiSwitch-thumb': {
            color: theme.palette.grey[600],
        },
        '&.Mui-disabled + .MuiSwitch-track': {
            opacity: 0.1,
        },
    },
    '& .MuiSwitch-thumb': {
        boxSizing: 'border-box',
        width: 22,
        height: 22,
    },
    '& .MuiSwitch-track': {
        borderRadius: 26 / 2,
        backgroundColor: '#39393D',
        opacity: 1,
        transition: theme.transitions.create(['background-color'], {
            duration: 500,
        }),
    },
}));

const ProfileInfo: React.FC<ProfileInfoProps> = ({ loading }) => {
    const context = useContext(ProfileViewContext);
    const profile = context?.profile;
    const refreshProfile = context?.refreshProfile;
    const dispatch = useDispatch();
    const authSlice = useSelector((state: RootState) => state.auth);

    const [profileData, setProfileData] = useState<any | null>(null);
    const [originalProfile, setOriginalProfile] = useState<any | null>(null);

    const [toneList, setToneList] = useState<BookingTone[]>([]); // complete catalog
    const [tones, setTones] = useState<BookingTone[]>([]); // currently applied
    const [selectedToneIds, setSelectedToneIds] = useState<string[]>([]); // working selection
    const [originalSelectedToneIds, setOriginalSelectedToneIds] = useState<string[]>([]); // baseline for change detection

    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [viewingFile, setViewingFile] = useState<{ url: string } | null>(null);

    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [filterText, setFilterText] = useState<string>('');
    const [openCategories, setOpenCategories] = useState<Record<string, boolean>>({});
    const [showAllCategory, setShowAllCategory] = useState<Record<string, boolean>>({});
    const displayLimit = 10;

        const [displayPrice, setDisplayPrice] = useState<number>(0);



    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
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
    const fetchBookingToneList = async () => {
        setIsLoading(true);
        try {
            const res = await getBookingToneList(loginRequiredAxiosInstance);
            console.log("Fetched tone lisst :", res.data.PodcastBookingToneList);
            if (res.success && res.data) {
                setToneList(res.data.PodcastBookingToneList);
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch detail:', error);
        } finally {
            setIsLoading(false);
        }
    }

    const fetchPodcasterBookingTone = async () => {
        setIsLoading(true);
        try {
            const res = await getBookingTone(loginRequiredAxiosInstance);
            console.log("Fetched tone :", res.data.PodcastBookingToneList);
            if (res.success && res.data) {
                const applied = res.data.PodcastBookingToneList || [];
                setTones(applied);
                setSelectedToneIds(applied.map(t => t.Id));
                setOriginalSelectedToneIds(applied.map(t => t.Id));
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch detail:', error);
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        if (!profile) return;
        setProfileData(profile);
        setOriginalProfile(prev => prev ?? profile);
                setDisplayPrice(profile.PodcasterProfile.PricePerBookingWord * 1000);
        fetchBookingToneList();
        fetchPodcasterBookingTone();
    }, [profile]);



    useEffect(() => {
        if (!quill) return;
        const serverHtml = profileData?.PodcasterProfile.Description ?? '';
        const currentHtml = quill.root.innerHTML;
        if (serverHtml !== currentHtml) {
            (quill.clipboard as any).dangerouslyPasteHTML(serverHtml);
        }
    }, [quill, profileData?.PodcasterProfile.Description]);

    useEffect(() => {
        if (!quill) return;
        const onTextChange = (_delta: any, _oldDelta: any, source: 'user' | 'api') => {
            if (source !== 'user') return;
            const htmlContent = quill.root.innerHTML;
            setProfileData(prev => {
                if (!prev || prev.PodcasterProfile.Description === htmlContent) return prev;
                return { ...prev, PodcasterProfile: { ...prev.PodcasterProfile, Description: htmlContent } };
            });
        };
        quill.on('text-change', onTextChange);
        return () => {
            quill.off?.('text-change', onTextChange);
        };
    }, [quill]);

    const nameChanged = useMemo(() => {
        if (!originalProfile || !profileData) return false;
        return originalProfile.PodcasterProfile.Name !== profileData.PodcasterProfile.Name.trim();
    }, [originalProfile, profileData?.PodcasterProfile.Name]);

    const buddyChanged = useMemo(() => {
        if (!originalProfile || !profileData) return false;
        return originalProfile.PodcasterProfile.IsBuddy !== profileData.PodcasterProfile.IsBuddy;
    }, [originalProfile, profileData?.PodcasterProfile.IsBuddy]);

    const priceChanged = useMemo(() => {
        if (!originalProfile || !profileData) return false;
        // So sánh với giá trị gốc (đã nhân 1000)
        return (originalProfile.PodcasterProfile.PricePerBookingWord * 1000) !== displayPrice;
    }, [originalProfile, displayPrice]);

    const descriptionChanged = useMemo(() => {
        if (!originalProfile || !profileData) return false;
        const norm = (s: string) => (s || '').trim();
        return norm(originalProfile.PodcasterProfile.Description || '') !== norm(profileData.PodcasterProfile.Description || '');
    }, [originalProfile, profileData?.PodcasterProfile.Description]);

    const toneSelectionChanged = useMemo(() => {
        if (originalSelectedToneIds.length !== selectedToneIds.length) return true;
        const a = new Set(originalSelectedToneIds);
        for (const id of selectedToneIds) if (!a.has(id)) return true;
        return false;
    }, [originalSelectedToneIds, selectedToneIds]);

    const isDirty = useMemo(
        () => nameChanged || descriptionChanged || priceChanged || buddyChanged || toneSelectionChanged,
        [nameChanged, descriptionChanged, priceChanged, buddyChanged, toneSelectionChanged]
    );
    const isDirtyTone = useMemo(
        () => toneSelectionChanged,
        [toneSelectionChanged]
    );


    const groupedTones = useMemo(() => {
        const groups: Record<string, BookingTone[]> = {};
        toneList
            .filter(t => !filterText || t.Name.toLowerCase().includes(filterText.toLowerCase()))
            .forEach(t => {
                const catName = t.PodcastBookingToneCategory?.Name || 'Other';
                if (!groups[catName]) groups[catName] = [];
                groups[catName].push(t);
            });
        return groups;
    }, [toneList, filterText]);

    const handleToggleTone = (toneId: string) => {
        setSelectedToneIds(prev => prev.includes(toneId) ? prev.filter(id => id !== toneId) : [...prev, toneId]);
    };

    const handleSaveTones = async () => {
        if (!isDirtyTone) return;
        try {
            setIsSaving(true);
            const payload = { PodcasterBookingToneApplyInfo: selectedToneIds };
            const res = await updateBookingTone(loginRequiredAxiosInstance, profileData.PodcasterProfile.IsBuddy, payload);
            const sagaId = res?.data.SagaInstanceId
            if (!sagaId) {
                toast.error("Profile update failed, please try again.")
                return
            }
            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    toast.success('Booking tones updated');
                    await fetchPodcasterBookingTone();
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (err) {
            console.error(err);
            toast.error('Error updating booking tones');
        } finally {
            setIsSaving(false);
        }
    };



    const handleSave = async () => {
        if (!profileData || !isDirty) return;

        if (profileData.PodcasterProfile.IsBuddy && selectedToneIds.length === 0) {
            toast.error("As a Buddy, you must select at least one booking tone.");
            return;
        }
        if (isDirtyTone) {
            await handleSaveTones();
        }
        if (isDirty) {
            try {
                setIsSaving(true);
                const payload = {
                    PodcasterProfileUpdateInfo: {
                        Name: profileData.PodcasterProfile.Name,
                        Description: profileData.PodcasterProfile.Description || '',
                        PricePerBookingWord: displayPrice / 1000,
                        IsBuddy: profileData.PodcasterProfile.IsBuddy,
                    },
                    BuddyAudioFile: null
                };
                console.log("Updating profile with payload:", profileData);
                const res = await updatePodcasterProfile(loginRequiredAxiosInstance, String(profileData.Id), payload);
                const sagaId = res?.data.SagaInstanceId
                if (!sagaId) {
                    toast.error("Profile update failed, please try again.")
                    return
                }
                await startPolling(sagaId, loginRequiredAxiosInstance, {
                    onSuccess: async () => {
                        toast.success('Profile updated successfully');
                        setOriginalProfile(prev => ({
                            ...(prev || profileData),
                            Name: profileData.PodcasterProfile.Name,
                            Description: profileData.PodcasterProfile.Description,
                            PricePerBookingWord: displayPrice / 1000,
                            IsBuddy: profileData.PodcasterProfile.IsBuddy,
                        }));
                        dispatch(setAuthToken({ ...authSlice, user: { ...authSlice.user, IsBuddy: profileData.PodcasterProfile.IsBuddy, PricePerBookingWord: (displayPrice/1000) } }));

                        await refreshProfile?.();

                    },
                    onFailure: (err) => toast.error(err || "Saga failed!"),
                    onTimeout: () => toast.error("System not responding, please try again."),
                })
            } catch (error) {
                toast.error("Error updating profile");
            } finally {
                setIsSaving(false);
            }
        }
    };

    const handleViewFile = async (fileKey: string) => {
        try {
            const response = await getBuddyCommitment(loginRequiredAxiosInstance, fileKey)
            if (response.success && response.data) {
                setViewingFile({ url: response.data.FileUrl })
            }
        } catch (error) {
            console.error('Error fetching PDF:', error)
            toast.error('Failed to load PDF')
        }
    }

    if (loading || !profileData) {
        return (
            <div className="flex justify-center items-center h-100">
                <Loading />
            </div>
        );
    }

    return (
        <div className="profile-info-page ">
            <div className="profile-info-page__actions">
                <Button
                    variant="contained"
                    className="profile-info-page__action-btn profile-info-page__action-btn--save"
                    disabled={(!isDirty && !isDirtyTone) || isSaving}
                    onClick={handleSave}
                >
                    Save
                </Button>
            </div>


            <div className="profile-info-page__content">
                <div className="profile-info-page__form">
                    <div className="profile-info-page__row">
                        <TextField
                            label="Name"
                            value={profileData.PodcasterProfile.Name}
                            variant="standard"
                            onChange={(e) => setProfileData({ ...profileData, PodcasterProfile: { ...profileData.PodcasterProfile, Name: e.target.value } })}
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
                            value={profileData.Email}
                            className="profile-info-page__input profile-info-page__input--email"
                        />
                    </div>

                    <div className="profile-info-page__row">
                        <TextField
                            label="Price Per Booking Word "
                            value={displayPrice} // Dùng displayPrice thay vì profileData.PodcasterProfile.PricePerBookingWord
                            variant="standard"
                            InputProps={{
                                endAdornment: (
                                    <InputAdornment position="end" sx={{ whiteSpace: 'nowrap' }}>
                                        <Typography variant="body2" sx={{ color: '#999999' }}>/ 1000 Words</Typography>
                                    </InputAdornment>
                                ),
                            }}
                            type="number"
                           onChange={(e) => {
                                let val = e.target.value;
                                if (val === '' || Number(val) < 1000) {
                                    setDisplayPrice(1000); // Min 1000 VND
                                } else {
                                    setDisplayPrice(Number(val));
                                }
                            }}
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
                            value={profileData.Balance}
                            className="profile-info-page__input-small"
                        />
                        <div>

                            <FormControlLabel
                                value={profileData.PodcasterProfile.IsBuddy ? true : false}
                                control={<IOSSwitch sx={{ m: 1 }}
                                    checked={!!profileData.PodcasterProfile.IsBuddy}
                                    onChange={(_e, checked) => {
                                        setProfileData({
                                            ...profileData,
                                            PodcasterProfile: {
                                                ...profileData.PodcasterProfile,
                                                IsBuddy: checked,
                                            }
                                        });
                                    }}
                                />}
                                label={
                                    <div className="flex items-center gap-2">
                                        <label className="episode-audio__selector-label">Is Buddy</label>
                                        <Tooltip placement="top-start" title="If you are a Buddy, you can receive booking requests from customers.">
                                            <Question color="var(--third-grey)" size={16} />
                                        </Tooltip >
                                    </div>
                                }
                                labelPlacement="top"
                                sx={{
                                    '& .MuiFormControlLabel-label': {
                                        color: '#999999',
                                        fontSize: '0.75rem',
                                        fontWeight: 600,
                                        letterSpacing: '0.5px',
                                        mb: 0.75
                                    },

                                }}
                            />
                        </div>
                    </div>
                    <div className="profile-info-page__row">
                        <TextField
                            variant="filled"
                            slotProps={{
                                input: {
                                    readOnly: true,
                                },
                            }}
                            label="Violation Level"
                            value={profileData.ViolationLevel ? profileData.ViolationLevel : '0'}
                            className="profile-info-page__input-small"

                        />
                        <TextField
                            variant="filled"
                            slotProps={{
                                input: {
                                    readOnly: true,
                                },
                            }}
                            label="Violation Point"
                            value={profileData.ViolationPoint ? profileData.ViolationPoint : '0'}
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
                            value={formatDate(profileData.PodcasterProfile.UpdatedAt)}
                            className="profile-info-page__input-small"

                        />
                        <TextField
                            id="filled-helperText"
                            variant="filled"
                            slotProps={{ input: { readOnly: true } }}
                            label="Total Followers"
                            value={profileData.PodcasterProfile.TotalFollow}
                            className="profile-info-page__input-small"
                        />
                        <TextField
                            id="filled-helperText"
                            variant="filled"
                            slotProps={{ input: { readOnly: true } }}
                            label="Listen Count"
                            value={profileData.PodcasterProfile.ListenCount}
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
                            value={`${profileData.PodcasterProfile.AverageRating} ⭐ (${profileData.PodcasterProfile.RatingCount})`}
                            className="profile-info-page__input-small"

                        />
                    </div>
                    <div className='flex mb-0'>
                        <Button
                            size="small"
                            onClick={() => handleViewFile(profileData.PodcasterProfile.CommitmentDocumentFileKey)}
                            sx={{
                                color: 'var(--primary-green)',
                                backgroundColor: 'transparent',
                                textTransform: 'none',
                                fontStyle: 'italic',
                                '&:hover': { textDecoration: 'underline', backgroundColor: 'transparent' },
                            }}
                        >
                            Buddy Commitment

                        </Button>
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
                    <div>
                        <p className="font-bold text-left mb-4">
                            Booking Tone
                        </p>

                        <Box className="profile-info-page__search-container mb-4">
                            <Box className="profile-info-page__search-icon">
                                <Search />
                            </Box>
                            <InputBase
                                placeholder="Search booking tones..."
                                value={filterText}
                                onChange={(e) => setFilterText(e.target.value)}
                                className="profile-info-page__search-input"
                            />
                        </Box>
                        {isLoading && toneList.length === 0 ? (
                            <div className="flex flex-col gap-2">
                                <Skeleton variant="rectangular" height={32} />
                                <Skeleton variant="rectangular" height={32} />
                                <Skeleton variant="rectangular" height={32} />
                            </div>
                        ) : (
                            <div className="booking-tone-selector flex flex-col gap-2" style={{ maxHeight: 420, overflow: 'auto' }}>
                                {Object.entries(groupedTones).map(([category, tones]) => {
                                    const isOpen = openCategories[category] ?? false;
                                    const showAll = showAllCategory[category] ?? false;
                                    const visibleTones = showAll ? tones : tones.slice(0, displayLimit);
                                    const collapsedSelectedCount = !isOpen ? tones.filter(t => selectedToneIds.includes(t.Id)).length : 0;
                                    return (
                                        <div key={category} className="booking-tone-selector__group">
                                            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 0.5 }}>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <IconButton size="small" onClick={() => setOpenCategories(prev => ({ ...prev, [category]: !isOpen }))}>
                                                        {isOpen ? <ExpandLess fontSize="small" className='text-[#aee339]' /> : <ExpandMore className='text-[#aee339]' fontSize="small" />}
                                                    </IconButton>
                                                    <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                                                        {category}
                                                    </Typography>
                                                </Box>
                                                {!isOpen && collapsedSelectedCount > 0 && (
                                                    <Typography variant="caption" className='text-[#aee339]'>{collapsedSelectedCount} selected</Typography>
                                                )}
                                            </Box>
                                            <Collapse in={isOpen} timeout="auto" unmountOnExit>
                                                <Box className="booking-tone-selector__tones" sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 0.5 }}>
                                                    {visibleTones.map(t => {
                                                        const selected = selectedToneIds.includes(t.Id);
                                                        return (
                                                            <Chip
                                                                key={t.Id}
                                                                label={t.Name}
                                                                onClick={() => handleToggleTone(t.Id)}
                                                                sx={{
                                                                    backgroundColor: selected ? '#aee339' : 'transparent',
                                                                    color: selected ? 'black' : 'var(--primary-green)',
                                                                    border: '1px solid var(--primary-green)',
                                                                    boxShadow: '1px 1px 5px rgba(12, 254, 4, 0.18)',
                                                                    '& .MuiChip-label': {
                                                                        fontSize: '0.8rem',
                                                                        maxWidth: '200px',
                                                                        overflow: 'hidden',
                                                                        textOverflow: 'ellipsis'
                                                                    },
                                                                    '&:hover': {
                                                                        cursor: 'pointer',
                                                                        backgroundColor: selected ? '#aee339' : 'transparent',
                                                                        color: selected ? 'black' : 'var(--primary-green)',
                                                                        border: '1px solid var(--primary-green)',
                                                                        boxShadow: '1px 1px 5px rgba(12, 254, 4, 0.18)',
                                                                    },
                                                                }}
                                                            />
                                                        );
                                                    })}
                                                    {tones.length > displayLimit && (
                                                        <Chip
                                                            label={showAll ? 'Show Less' : `Show More (${tones.length - displayLimit})`}
                                                            color={showAll ? 'warning' : 'warning'}
                                                            variant='outlined'
                                                            onClick={() => setShowAllCategory(prev => ({ ...prev, [category]: !showAll }))}
                                                            sx={{ cursor: 'pointer' }}
                                                        />
                                                    )}
                                                </Box>
                                            </Collapse>
                                        </div>
                                    );
                                })}
                                {toneList.length === 0 && !isLoading && (
                                    <Typography variant="body2" color="text.secondary">No booking tones available.</Typography>
                                )}
                            </div>
                        )}
                        {/* {toneSelectionChanged && (
                            <Typography variant="caption" color="warning.main" sx={{ mt: 1, display: 'block' }}>
                                You have unsaved booking tone changes.
                            </Typography>
                        )} */}
                    </div>
                </div>
            </div>

            <Dialog
                open={!!viewingFile}
                onClose={() => setViewingFile(null)}
                maxWidth="md"
                fullWidth
                PaperProps={{
                    sx: {
                        backgroundColor: '#1a1a1a',
                        color: 'white',
                        minHeight: '500px'
                    }
                }}
            >
                <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography variant="h6">Buddy Commitment Preview</Typography>
                    <IconButton onClick={() => setViewingFile(null)} sx={{ color: 'white' }}>
                        <Close />
                    </IconButton>
                </DialogTitle>
                <DialogContent>
                    {viewingFile && (
                        <DocumentViewer url={viewingFile.url} height={600} />
                    )}
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default ProfileInfo;
