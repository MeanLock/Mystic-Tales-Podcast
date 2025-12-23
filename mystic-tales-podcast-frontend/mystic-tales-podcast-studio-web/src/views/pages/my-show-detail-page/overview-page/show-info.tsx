import React, { useEffect, useState, useRef, use, useCallback, useMemo, createContext } from 'react';
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
    Modal,
} from '@mui/material';
import Slider from 'react-slick';

import { Add } from '@mui/icons-material';
import { useQuill } from 'react-quilljs';
 import 'quill/dist/quill.snow.css';
import './styles.scss';
import { useNavigate, useParams } from 'react-router-dom';
import { deleteShow, getShowDetail, publishShow, updateShow } from '@/core/services/show/show.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';
import { PodcastShow } from '@/core/types/show';
import { PodcastCategory, PodcastSubCategory } from '@/core/types/category';
import { HashtagOption } from '@/core/types';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { getCategories } from '@/core/services/misc/category.service';
import Loading from '@/views/components/common/loading';
import { createHashtag, getHashtags } from '@/core/services/misc/hashtag.service';
import { toast } from 'react-toastify';
import { fetchImage } from '@/core/utils/image.util';
import { formatDate, getTimeAgo } from '@/core/utils/date.util';
import Image from '@/views/components/common/image';
import Modal_Button from '@/views/components/common/modal/ModalButton';
import { X } from 'phosphor-react';
import { isEqual } from 'lodash';
import { confirmAlert } from '@/core/utils/alert.util';
import AssignChannelModal from './AssignChannelModal';
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

export const mockSubscriptionTypes = [
    { Id: 1, Name: "Free" },
    { Id: 2, Name: "Subscriber only" },
];
export const UploadFrequencyList = [
    { Name: "Daily" },
    { Name: "Weekly" },
    { Name: "Monthly" },
];

interface ShowInfoViewContextProps {
    handleDataChange: () => void
    channel: any | null
}

export const ShowInfoViewContext = createContext<ShowInfoViewContextProps | null>(null)

const ShowInfo = () => {
    const { id } = useParams<{ id: string }>();
    const authSlice = useSelector((state: RootState) => state.auth);
    const [showDetail, setShowDetail] = useState<PodcastShow | null>(null);
    const [originalShow, setOriginalShow] = useState<PodcastShow | null>(null);
    const [categoryList, setCategoryList] = useState<PodcastCategory[]>([])
    const [selectedHashtags, setSelectedHashtags] = useState<HashtagOption[]>([]);
    const [hashtagInput, setHashtagInput] = useState<string>('');
    const [suggestions, setSuggestions] = useState<HashtagOption[]>([]);
    const [suggestLoading, setSuggestLoading] = useState<boolean>(false);
    const [openSuggest, setOpenSuggest] = useState<boolean>(false);
    const [previewImage, setPreviewImage] = useState<string>('');
    const [uploadImage, setUploadImage] = useState<string | null>(null);
    const [mainImageFile, setMainImageFile] = useState<File | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [isPublishing, setIsPublishing] = useState<boolean>(false);
    const [isDeleting, setIsDeleting] = useState<boolean>(false);
    const navigate = useNavigate();

    const [initialLoading, setInitialLoading] = useState(true);
    const [refreshing, setRefreshing] = useState(false);

    const fileInputRef = useRef<HTMLInputElement>(null);
    const [releaseDate, setReleaseDate] = useState<string>(() => {
        const today = new Date();
        return today.toISOString().split('T')[0];
    });
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })
    const [formData, setFormData] = useState({
        Name: '',
        Copyright: '',
        Language: 'English',
        UploadFrequency: 'Daily',
        Description: '',
        PodcastShowSubscriptionTypeId: 1,
        PodcasterId: authSlice.user?.Id || 0,
        PodcastCategoryId: 0,
        PodcastSubCategoryId: 0,
        HashtagIds: []

    });

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

    const fetchShowDetail = async (opts: { initial?: boolean } = {}) => {
        opts.initial ? setInitialLoading(true) : setRefreshing(true);
        try {
            const res = await getShowDetail(loginRequiredAxiosInstance, id);
            if (res.success && res.data) {
                const s = res.data.Show;
                setShowDetail(s);
                setOriginalShow(prev => prev ?? s);
                setSelectedHashtags(s.Hashtags.map((h: any) => ({ id: h.Id, name: h.Name })));
                setFormData({
                    Name: s.Name,
                    Copyright: s.Copyright,
                    Language: s.Language || 'English',
                    UploadFrequency: s.UploadFrequency || 'Daily',
                    PodcastShowSubscriptionTypeId: s.PodcastShowSubscriptionType?.Id || 1,
                    Description: s.Description || '',
                    PodcasterId: authSlice.user?.Id || 0,
                    PodcastCategoryId: s.PodcastCategory?.Id || 0,
                    PodcastSubCategoryId: s.PodcastSubCategory?.Id || 0,
                    HashtagIds: s.Hashtags?.map((h: any) => h.Id) || []
                });
            } else {
                console.error('API Error:', res.message);
            }
        } catch (e) {
            console.error('fetchShowDetail error', e);
        } finally {
            opts.initial ? setInitialLoading(false) : setRefreshing(false);
        }
    };

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
        fetchShowDetail({ initial: true });
        fetchCategory();
    }, [id]);

    useEffect(() => {
        if (!quill) return;
        const serverHtml = showDetail?.Description ?? '';
        const currentHtml = quill.root.innerHTML;
        if (serverHtml !== currentHtml) {
            (quill.clipboard as any).dangerouslyPasteHTML(serverHtml);
        }
    }, [quill, showDetail?.Description]);

    useEffect(() => {
        if (!quill) return;
        const onTextChange = (_delta: any, _oldDelta: any, source: 'user' | 'api') => {
            if (source !== 'user') return;
            const htmlContent = quill.root.innerHTML;
            setShowDetail(prev => {
                if (!prev || prev.Description === htmlContent) return prev;
                return { ...prev, Description: htmlContent };
            });
        };
        quill.on('text-change', onTextChange);
        return () => {
            quill.off?.('text-change', onTextChange);
        };
    }, [quill]);


    const getSubCategoriesForCategory = (categoryId: number): PodcastSubCategory[] => {
        const found = categoryList.find(cat => cat.Id === categoryId);
        return found?.PodcastSubCategoryList ?? [];
    };

    const handleCategoryChange = (categoryId: number) => {
        const firstSubId = getSubCategoriesForCategory(categoryId)[0]?.Id || 0;
        setFormData(prev => ({
            ...prev,
            PodcastCategoryId: categoryId,
            PodcastSubCategoryId: firstSubId,
        }));
    };

    const handleSubCategoryChange = (subCategoryId: number) => {
        setFormData(prev => ({
            ...prev,
            PodcastSubCategoryId: subCategoryId
        }));
    };

    const nameChanged = useMemo(() => {
        if (!originalShow || !showDetail) return false;
        return originalShow.Name !== showDetail.Name;
    }, [originalShow, showDetail?.Name]);

    const copyrightChanged = useMemo(() => {
        if (!originalShow || !showDetail) return false;
        return originalShow.Copyright !== showDetail.Copyright;
    }, [originalShow, showDetail?.Copyright]);

    const languageChanged = useMemo(() => {
        if (!originalShow || !showDetail) return false;
        return originalShow.Language !== showDetail.Language;
    }, [originalShow, showDetail?.Language]);

    const uploadFrequencyChanged = useMemo(() => {
        if (!originalShow || !showDetail) return false;
        return originalShow.UploadFrequency !== showDetail.UploadFrequency;
    }, [originalShow, showDetail?.UploadFrequency]);

    const subscriptionTypeChanged = useMemo(() => {
        if (!originalShow || !showDetail) return false;
        return originalShow.PodcastShowSubscriptionType?.Id !== showDetail.PodcastShowSubscriptionType?.Id;
    }, [originalShow, showDetail?.PodcastShowSubscriptionType?.Id]);

    const descriptionChanged = useMemo(() => {
        if (!originalShow || !showDetail) return false;
        const norm = (s: string) => (s || '').trim();
        return norm(originalShow.Description || '') !== norm(showDetail.Description || '');
    }, [originalShow, showDetail?.Description]);

    const categoryChanged = useMemo(() => {
        if (!originalShow) return false;
        return (
            originalShow.PodcastCategory?.Id !== formData.PodcastCategoryId ||
            originalShow.PodcastSubCategory?.Id !== formData.PodcastSubCategoryId
        );
    }, [originalShow, formData.PodcastCategoryId, formData.PodcastSubCategoryId]);

    const hashtagsChanged = useMemo(() => {
        if (!originalShow) return false;
        const originalIds = (originalShow.Hashtags || []).map(h => h.Id).sort();
        const currentIds = selectedHashtags.map(h => h.id).sort();
        return !isEqual(originalIds, currentIds);
    }, [originalShow, selectedHashtags]);

    const imageChanged = useMemo(() => !!mainImageFile, [mainImageFile]);

    const isDirty = useMemo(
        () => nameChanged || descriptionChanged || categoryChanged || hashtagsChanged || imageChanged || languageChanged || uploadFrequencyChanged || subscriptionTypeChanged || copyrightChanged,
        [nameChanged, descriptionChanged, categoryChanged, hashtagsChanged, imageChanged, languageChanged, uploadFrequencyChanged, subscriptionTypeChanged, copyrightChanged]
    );
    const handleSave = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        if (!showDetail || !isDirty) return;
        try {
            setIsSaving(true);
            const payload = {
                ShowUpdateInfo: {
                    Name: showDetail.Name,
                    Copyright: showDetail.Copyright,
                    Language: showDetail.Language || 'English',
                    UploadFrequency: showDetail.UploadFrequency || 'Daily',
                    PodcastShowSubscriptionTypeId: showDetail.PodcastShowSubscriptionType?.Id || 1,
                    Description: showDetail.Description || formData.Description || '',
                    PodcastCategoryId: formData.PodcastCategoryId || showDetail.PodcastCategory?.Id || 0,
                    PodcastSubCategoryId: formData.PodcastSubCategoryId || showDetail.PodcastSubCategory?.Id || 0,
                    HashtagIds: selectedHashtags.map(h => h.id),
                },
                MainImageFile: mainImageFile || undefined,
            };
            const res = await updateShow(loginRequiredAxiosInstance, String(showDetail.Id), payload);
            const sagaId = res?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Show Updated failed, please try again.")
                return
            }
            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    toast.success('Show updated successfully');
                    setOriginalShow(prev => ({
                        ...(prev || showDetail),
                        Name: showDetail.Name,
                        Copyright: showDetail.Copyright,
                        Language: showDetail.Language,
                        UploadFrequency: showDetail.UploadFrequency,
                        PodcastShowSubscriptionType: { ...showDetail.PodcastShowSubscriptionType, Id: formData.PodcastShowSubscriptionTypeId },
                        Description: showDetail.Description,
                        PodcastCategory: { ...showDetail.PodcastCategory, Id: formData.PodcastCategoryId },
                        PodcastSubCategory: { ...showDetail.PodcastSubCategory, Id: formData.PodcastSubCategoryId },
                        Hashtags: selectedHashtags.map(h => ({ Id: h.id, Name: h.name }))
                    }));
                    setMainImageFile(null);
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error updating show");
        } finally {
            setIsSaving(false);
        }
    };

    const handleDelete = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        const alert = await confirmAlert("Are you sure to DELETE this Show, Episodes under this show will be deleted as well");
        if (!alert.isConfirmed) return;
        try {
            setIsDeleting(true);
            const res = await deleteShow(loginRequiredAxiosInstance, String(showDetail.Id));
            const sagaId = res?.data?.SagaInstanceId
            if (!res.success && res.message.content) {
                toast.error(res.message.content)
                return
            }
            if (!sagaId) {
                toast.error(`Show deletion failed, please try again.`)
                return
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: () => {
                    toast.success(`Show deleted successfully.`)
                    navigate('/show');
                    navigate(0);
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error deleting show");
        } finally {
            setIsDeleting(false);
        }
    };

    const handlePublish = async (isPublish: boolean) => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        console.log(releaseDate)
        if (showDetail.CurrentStatus.Id === 1) {
            toast.warning("At least one Episode must be published before publishing the Show.");
            return
        }
        const alert = await confirmAlert("Are you sure to " + (isPublish ? "publish" : "unpublish") + " this show?");
        if (!alert.isConfirmed) return;
        try {
            const payload = isPublish ? { ShowPublishInfo: { ReleaseDate: releaseDate } } : { ShowPublishInfo: { ReleaseDate: "" } };
            setIsPublishing(true);
            const res = await publishShow(loginRequiredAxiosInstance, String(showDetail.Id), isPublish, payload);
            const sagaId = res?.data?.SagaInstanceId
            if (!res.success && res.message.content) {
                toast.error(res.message.content)
                return
            }
            if (!sagaId) {
                toast.error(`Show ${isPublish ? 'published' : 'unpublished'} failed, please try again.`)
                return
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    toast.success(`Show ${isPublish ? 'published' : 'unpublished'} successfully.`)
                    await fetchShowDetail();
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error publishing show");
        } finally {
            setIsPublishing(false);
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
        }
    };
    useEffect(() => {
        if (!mainImageFile) {
            setUploadImage(null)
            return
        }
        const url = URL.createObjectURL(mainImageFile)
        setUploadImage(url)
        return () => {
            URL.revokeObjectURL(url)
        }
    }, [mainImageFile])

    useEffect(() => {
        let alive = true;
        const load = async () => {
            if (uploadImage) {
                setPreviewImage(uploadImage);
                return;
            }
            const key = showDetail?.MainImageFileKey;
            if (key) {
                const url = await fetchImage(key);
                if (alive) setPreviewImage(url || '');
            } else {
                setPreviewImage('');
            }
        };
        load();
        return () => { alive = false; };
    }, [uploadImage, showDetail?.MainImageFileKey]);

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

    if (loading || !showDetail) {
        return (
            <div className="flex justify-center items-center h-100">
                <Loading />
            </div>
        );
    }

    return (
        <ShowInfoViewContext.Provider value={{ handleDataChange: fetchShowDetail, channel: showDetail.PodcastChannel ? showDetail.PodcastChannel : null }}>
            <div className="show-info-page">
                {showDetail.CurrentStatus?.Id === 5 && (
                    <div
                        className="flex items-center  px-3 py-2 mt-6 mb-10"
                        style={{ width: "fit-content" }}
                    >

                    </div>
                )}
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
                        className="show-info-page__action-btn show-info-page__action-btn--remove"
                        variant="outlined"
                        startIcon={<X size={15} />}
                        onClick={() => handleDelete()}
                        disabled={isSaving || isDeleting || isPublishing}
                    >
                        {isDeleting ? 'Deleting...' : 'Delete'}
                    </Button>
                    {showDetail.CurrentStatus?.Id !== 4 && showDetail.CurrentStatus?.Id !== 5 && (
                        <>
                            {(showDetail.CurrentStatus?.Id === 1 || showDetail.CurrentStatus?.Id === 2) && (
                                <Modal_Button
                                    className="show-info-page__action-btn show-info-page__action-btn--unpublish"
                                    content="Publish"
                                    variant="outlined"
                                    size='sm'
                                    disabled={isSaving || isDeleting || isPublishing}
                                >
                                    <div className="flex flex-col gap-4 p-8 ">
                                        <TextField
                                            label="Release Date"
                                            value={releaseDate}
                                            variant="standard"
                                            type='date'
                                            required
                                            onChange={(e) => setReleaseDate(e.target.value)}
                                            className="show-info-page__input show-info-page__input--name"
                                            inputProps={{
                                                min: new Date().toISOString().split('T')[0]
                                            }}
                                            sx={{
                                                '& .MuiOutlinedInput-root': {
                                                    '& fieldset': { borderColor: '#999999 !important' },
                                                    '&:hover fieldset': { borderColor: '#999999 !important' },
                                                    '&.Mui-focused fieldset': { borderColor: '#999999 !important' }
                                                },
                                            }}
                                        />
                                        <Button
                                            className="show-info-page__action-btn show-info-page__action-btn--save "
                                            onClick={() => handlePublish(true)}
                                            disabled={isPublishing || !releaseDate}
                                        >
                                            {isPublishing ? "Publishing..." : "Confirm"}
                                        </Button>
                                    </div>
                                </Modal_Button>
                            )}

                            {showDetail.CurrentStatus?.Id === 3 && (
                                <Button
                                    variant="outlined"
                                    className="show-info-page__action-btn show-info-page__action-btn--unpublish"
                                    onClick={() => handlePublish(false)}
                                    disabled={isSaving || isDeleting || isPublishing}
                                >
                                    {isPublishing ? 'Unpublishing...' : 'Unpublish'}
                                </Button>
                            )}
                            {showDetail.PodcastChannel ? (
                                <Modal_Button
                                    className="show-info-page__action-btn show-info-page__action-btn--unpublish"
                                    content="Update Channel"
                                    variant="outlined"
                                    disabled={isSaving || isDeleting || isPublishing}
                                    size='sm'
                                >
                                    <AssignChannelModal onclose={() => { }} />
                                </Modal_Button>
                            ) : (
                                <Modal_Button
                                    className="show-info-page__action-btn show-info-page__action-btn--unpublish"
                                    content="Assign to Channel"
                                    variant="outlined"
                                    size='sm'
                                    disabled={isSaving || isDeleting || isPublishing}
                                >
                                    <AssignChannelModal onclose={() => { }} />
                                </Modal_Button>
                            )}

                            <Button
                                variant="contained"
                                className="show-info-page__action-btn show-info-page__action-btn--save"
                                onClick={handleSave}
                                disabled={!isDirty || isSaving || isDeleting || isPublishing}

                            >
                                {isSaving ? 'Saving...' : 'Save'}
                            </Button>
                        </>
                    )}
                </div>


                <div className="show-info-page__content">
                    <div className="show-info-page__form">
                        {/* Stats Cards Section */}
                        <div className="show-info-page__stats-section">
                            <div className="show-info-page__stat-card">
                                <div className="show-info-page__stat-icon">👥</div>
                                <div className="show-info-page__stat-content">
                                    <div className="show-info-page__stat-label">Total Followers</div>
                                    <div className="show-info-page__stat-value">{showDetail.TotalFollow}</div>
                                </div>
                            </div>
                            <div className="show-info-page__stat-card">
                                <div className="show-info-page__stat-icon">🎧</div>
                                <div className="show-info-page__stat-content">
                                    <div className="show-info-page__stat-label">Listen Count</div>
                                    <div className="show-info-page__stat-value">{showDetail.ListenCount}</div>
                                </div>
                            </div>
                            <div className="show-info-page__stat-card">
                                <div className="show-info-page__stat-icon">⭐</div>
                                <div className="show-info-page__stat-content">
                                    <div className="show-info-page__stat-label">Rating Average</div>
                                    <div className="show-info-page__stat-value">{showDetail.AverageRating}</div>
                                </div>
                            </div>
                        </div>

                        {/* Metadata Section with Glassmorphism */}
                        <div className="show-info-page__metadata-section">
                            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                                <Typography variant="caption" sx={{ color: '#999', fontSize: '0.75rem', mb: 0.5 }}>
                                    Status
                                </Typography>
                                <Chip
                                    label={showDetail.CurrentStatus.Name}
                                    sx={{
                                        background: showDetail.CurrentStatus.Id === 1 
                                            ? 'rgba(255, 193, 7, 0.2)' 
                                            : 'rgba(76, 175, 80, 0.2)',
                                        backdropFilter: 'blur(10px)',
                                        WebkitBackdropFilter: 'blur(10px)',
                                        border: showDetail.CurrentStatus.Id === 1
                                            ? '1px solid rgba(255, 193, 7, 0.3)'
                                            : '1px solid rgba(76, 175, 80, 0.3)',
                                        color: showDetail.CurrentStatus.Id === 1 ? '#FFC107' : '#4CAF50',
                                        fontWeight: 700,
                                        fontSize: '0.875rem',
                                        height: '32px',
                                        boxShadow: showDetail.CurrentStatus.Id === 1
                                            ? '0 8px 32px rgba(255, 193, 7, 0.15)'
                                            : '0 8px 32px rgba(76, 175, 80, 0.15)',
                                        '& .MuiChip-label': {
                                            padding: '0 12px',
                                            textShadow: '0 2px 4px rgba(0, 0, 0, 0.2)',
                                        },
                                    }}
                                />
                            </Box>

                            <TextField
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Channel"
                                value={showDetail.PodcastChannel?.Name || 'Single Show'}
                                className="show-info-page__input-small"
                            />

                            {showDetail.ReleaseDate !== null && (
                                <TextField
                                    variant="filled"
                                    slotProps={{
                                        input: {
                                            readOnly: true,
                                        },
                                    }}
                                    label="Release Date"
                                    value={formatDate(showDetail.ReleaseDate)}
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
                                label="Is Released"
                                value={showDetail.IsReleased ? 'Yes' : 'No'}
                                className="show-info-page__input-small"
                            />

                            <TextField
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Created At"
                                value={formatDate(showDetail.CreatedAt)}
                                className="show-info-page__input-small"
                            />

                            <TextField
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Updated At"
                                value={formatDate(showDetail.UpdatedAt)}
                                className="show-info-page__input-small"
                            />
                        </div>

                        {/* Editable Information Section */}
                        <div className="show-info-page__row">
                            <TextField
                                label="Name"
                                value={showDetail.Name}
                                variant="standard"
                                onChange={(e) => setShowDetail({ ...showDetail, Name: e.target.value })}
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
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Channel"
                                value={showDetail.PodcastChannel?.Name || 'Single Show'}
                                className="show-info-page__input--name"
                            />

                        </div>
                        <div className="show-info-page__row">

                            <TextField
                                select
                                label="Language"
                                variant="standard"
                                value={showDetail.Language}
                                onChange={(e) => setShowDetail({ ...showDetail, Language: e.target.value })}

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
                                value={showDetail.UploadFrequency}
                                onChange={(e) => setShowDetail({ ...showDetail, UploadFrequency: e.target.value })}
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
                                value={formData.PodcastCategoryId}
                                onChange={(e) => handleCategoryChange(e.target.value as unknown as number)}
                                className="show-info-page__select"
                            >
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
                                {getSubCategoriesForCategory(formData.PodcastCategoryId || showDetail.PodcastCategory.Id).map(sub => (
                                    <MenuItem key={sub.Id} value={sub.Id}>{sub.Name}</MenuItem>
                                ))}
                            </TextField>

                            <TextField
                                select
                                label="Subscription Type"
                                variant="standard"
                                value={showDetail.PodcastShowSubscriptionType?.Id || 1}
                                onChange={(e) => setShowDetail({ ...showDetail, PodcastShowSubscriptionType: { ...showDetail.PodcastShowSubscriptionType, Id: e.target.value as unknown as number } })}
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
                            value={showDetail.Copyright}
                            variant="standard"
                            onChange={(e) => setShowDetail({ ...showDetail, Copyright: e.target.value })}
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
                                        className="show-info-page__hashtag-suggest"
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

                            <div className="show-info-page__hashtag-chips">
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
                            {uploadImage ? (
                                <img
                                    src={uploadImage}
                                    alt="Preview"
                                    className="show-info-page__main-image-file"
                                />
                            ) : (
                                <Image
                                    mainImageFileKey={showDetail.MainImageFileKey}
                                    alt={showDetail.Name}
                                    className="show-info-page__main-image-file"
                                />
                            )}

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
                            accept=".jpg,.jpeg,.png,.gif,.webp,.svg"
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
                                    alt={showDetail.Name}
                                    className="show-info-page__preview-bg-image"
                                />
                                <div className="show-info-page__preview-overlay">
                                    <div className="show-info-page__preview-content">
                                        <img
                                            src={previewImage}
                                            className="show-info-page__preview-avatar"
                                        />
                                        <div className="show-info-page__preview-info">
                                            <Typography variant="h6" className="show-info-page__preview-name">
                                                {showDetail.Name}
                                            </Typography>
                                            <Typography variant="body2" className="show-info-page__preview-subtitle"

                                            >
                                            </Typography>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </Card>
                    </div>
                </div>
                {showDetail.ReviewList && showDetail.ReviewList.length > 0 && (
                    <div style={{ marginTop: '2rem' }}>
                        <Typography variant="h6" style={{ color: 'white', marginBottom: '1rem', fontWeight: 'bold' }}>
                            Customer Reviews
                        </Typography>
                        <Slider
                            dots={true}
                            infinite={false}
                            speed={500}
                            slidesToShow={3}
                            slidesToScroll={1}
                            arrows={true}
                            responsive={[
                                {
                                    breakpoint: 1024,
                                    settings: {
                                        slidesToShow: 2,
                                        slidesToScroll: 1,
                                    }
                                },
                                {
                                    breakpoint: 600,
                                    settings: {
                                        slidesToShow: 1,
                                        slidesToScroll: 1,
                                    }
                                }
                            ]}
                        >
                            {showDetail.ReviewList.map((review) => (
                                <div key={review.Id} style={{ padding: '0 8px' }}>
                                    <div
                                        className="rounded-2xl p-6 h-full"
                                        style={{ backgroundColor: "rgba(255, 255, 255, 0.1)" }}
                                    >
                                        <div className="flex justify-between items-start mb-2">
                                            <div className="text-xs text-gray-200">
                                                {getTimeAgo(review.UpdatedAt)}
                                            </div>

                                            <div className="flex mb-4">
                                                {Array.from({ length: 5 }).map((_, i) => (
                                                    <svg
                                                        key={i}
                                                        className={`w-4 h-4 ${i < Math.floor(review.Rating)
                                                            ? "text-yellow-400"
                                                            : "text-gray-400"
                                                            }`}
                                                        fill="currentColor"
                                                        viewBox="0 0 20 20"
                                                    >
                                                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                                    </svg>
                                                ))}
                                            </div>
                                        </div>
                                        <div className='flex'>
                                            <div className='flex flex-col items-center mr-6'>

                                                <Image
                                                    mainImageFileKey={review.Account.MainImageFileKey}
                                                    alt={review.Account.FullName}
                                                    className="w-12 h-12 rounded-full object-cover mb-4"
                                                />
                                                <p className="text-xs text-gray-200">
                                                    {review.Account.FullName}
                                                </p>
                                            </div>

                                            <div>
                                                <h4 className="font-semibold  text-left  text-white text-base mb-2">
                                                    {review.Title}
                                                </h4>
                                                <p className="text-gray-100 text-sm text-left leading-relaxed">
                                                    {review.Content}
                                                </p>
                                            </div>
                                        </div>

                                    </div>
                                </div>
                            ))}
                        </Slider>
                    </div>
                )}
            </div>
        </ShowInfoViewContext.Provider>
    );
};

export default ShowInfo;
