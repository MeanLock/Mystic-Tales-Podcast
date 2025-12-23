import React, { useState, useEffect, use, useContext, FC } from 'react';
import {
    Box,
    Typography,
    TextField,
    Button,
    Divider,
    IconButton,
    InputAdornment,
    Chip,
    FormControlLabel,
    Checkbox,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    Collapse
} from '@mui/material';
import { Add, Delete, ExpandMore, ExpandLess } from '@mui/icons-material';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { activeSubscription, addChannelSubscription, addShowSubscription, deleteSubscription, getSubscriptionDetail, updateSubscription } from '@/core/services/subscription/subscription.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { toast } from 'react-toastify';
import { ShowSubscriptionContext } from '.';
import Loading from '@/views/components/common/loading';
import { formatDate } from '@/core/utils/date.util';
import { confirmAlert } from '@/core/utils/alert.util';
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';
import { useQuill } from 'react-quilljs';

interface SubscriptionModalProps {
    subscription?: any;
    podcastShowId?: string;
    onClose?: () => void;
}

interface CycleTypePrice {
    SubscriptionCycleTypeId: number;
    Price: number;
}

const availableCycleTypes = [
    { Id: 1, Name: "Monthly" },
    { Id: 2, Name: "Annually" },
];

const availableBenefits = [
    { Id: 1, Name: "Non-Quota Listening" },
    { Id: 2, Name: "Subscriber-Only Shows" },
    { Id: 3, Name: "Subscriber-Only Episodes" },
    { Id: 4, Name: "Bonus Episodes" },
    { Id: 5, Name: "Shows/Episodes Early Access" },
    { Id: 6, Name: "Archive Episodes Access" },
];

const SubscriptionModal: FC<SubscriptionModalProps> = ({
    subscription,
    podcastShowId,
    onClose
}) => {
    const isUpdateMode = !!subscription;
    const authSlice = useSelector((state: RootState) => state.auth);
    const context = useContext(ShowSubscriptionContext);
    const [subDetail, setSubDetail] = useState<any>(null);
    const [cycleTypePrices, setCycleTypePrices] = useState<CycleTypePrice[]>([]);
    const [selectedBenefits, setSelectedBenefits] = useState<number[]>([]);
    const [loading, setLoading] = useState(false);
    const [showPriceHistory, setShowPriceHistory] = useState(false);
    const [showBenefitHistory, setShowBenefitHistory] = useState(false);
    const [showRegistrations, setShowRegistrations] = useState(false);
    const [activating, setActivating] = useState(false);
    const [fetchingDetails, setFetchingDetails] = useState(false);
    const [deleting, setDeleting] = useState(false);
    const [originalCycleTypeIds, setOriginalCycleTypeIds] = useState<number[]>([]);

    const { startPolling } = useSagaPolling({
        timeoutSeconds: 10,
        intervalSeconds: 0.5,
    })
    const [formData, setFormData] = useState({
        name: '',
        description: '',
    });
   const { quill, quillRef } = useQuill({
        theme: 'snow',
        modules: {
            toolbar: [
                ['bold', 'italic', 'underline'],
                ['link'],
                ['clean'],
            ],
        },
        placeholder: 'Add description...'
    });
    useEffect(() => {
        if (isUpdateMode && subscription) {
            fetchSubscriptionDetails(subscription.Id);
        } else {
            setFormData({
                name: '',
                description: '',
            });
            setCycleTypePrices([]);
            setSelectedBenefits([]);
            setOriginalCycleTypeIds([]);

        }
    }, [subscription, isUpdateMode]);

    const fetchSubscriptionDetails = async (subscriptionId: number) => {
        try {
            setFetchingDetails(true);
            const response = await getSubscriptionDetail(loginRequiredAxiosInstance, subscriptionId);
            if (response.success && response.data) {
                const subscription = response.data.PodcastSubscription;
                setSubDetail(subscription);
                setFormData({
                    name: subscription.Name || '',
                    description: subscription.Description || '',
                });

                const allPrices = subscription.PodcastSubscriptionCycleTypePriceList || [];
                if (allPrices.length > 0) {
                    const maxVersion = Math.max(...allPrices.map((item: any) => item.Version));
                    const currentPrices = allPrices.filter((item: any) => item.Version === maxVersion);
                    const existingPrices = currentPrices.map((item: any) => ({
                        SubscriptionCycleTypeId: item.SubscriptionCycleType.Id,
                        Price: item.Price,
                    }));
                    setCycleTypePrices(existingPrices);
                    setOriginalCycleTypeIds(existingPrices.map(p => p.SubscriptionCycleTypeId));

                } else {
                    setCycleTypePrices([]);
                    setOriginalCycleTypeIds([]);
                }

                const allBenefits = subscription.PodcastSubscriptionBenefitMappingList || [];
                if (allBenefits.length > 0) {
                    const maxVersion = Math.max(...allBenefits.map((item: any) => item.Version));
                    const currentBenefits = allBenefits.filter((item: any) => item.Version === maxVersion);
                    const existingBenefits = currentBenefits.map((item: any) => item.PodcastSubscriptionBenefit.Id);
                    setSelectedBenefits(existingBenefits);
                } else {
                    setSelectedBenefits([]);
                }
            }
        } catch (error) {
            console.error('Error fetching subscription details:', error);
        } finally {
            setFetchingDetails(false);
        }
    };

    useEffect(() => {
            if (!quill) return;
            const serverHtml = formData?.description ?? '';
            const currentHtml = quill.root.innerHTML;
            if (serverHtml !== currentHtml) {
                (quill.clipboard as any).dangerouslyPasteHTML(serverHtml);
            }
        }, [quill, formData?.description]);
    
        useEffect(() => {
            if (!quill) return;
            const onTextChange = (_delta: any, _oldDelta: any, source: 'user' | 'api') => {
                if (source !== 'user') return;
                const htmlContent = quill.root.innerHTML;
                setFormData(prev => {
                    if (!prev || prev.description === htmlContent) return prev;
                    return { ...prev, description: htmlContent };
                });
            };
            quill.on('text-change', onTextChange);
            return () => {
                quill.off?.('text-change', onTextChange);
            };
        }, [quill]);

    const handleAddCycleTypePrice = (cycleTypeId: number) => {
        if (!cycleTypePrices.find(p => p.SubscriptionCycleTypeId === cycleTypeId)) {
            setCycleTypePrices([...cycleTypePrices, { SubscriptionCycleTypeId: cycleTypeId, Price: 0 }]);
        }
    };

    const handleRemoveCycleTypePrice = (cycleTypeId: number) => {
        setCycleTypePrices(cycleTypePrices.filter(p => p.SubscriptionCycleTypeId !== cycleTypeId));
    };

    const handlePriceChange = (cycleTypeId: number, price: number) => {
        setCycleTypePrices(
            cycleTypePrices.map(p =>
                p.SubscriptionCycleTypeId === cycleTypeId ? { ...p, Price: price } : p
            )
        );
    };

    const handleBenefitToggle = (benefitId: number) => {
        if (selectedBenefits.includes(benefitId)) {
            setSelectedBenefits(selectedBenefits.filter(id => id !== benefitId));
        } else {
            setSelectedBenefits([...selectedBenefits, benefitId]);
        }
    };

    const handleSave = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        const invalidPrices = cycleTypePrices.filter(p => p.Price <= 0);
        if (invalidPrices.length > 0) {
            toast.error('All cycle type prices must be greater than 0');
            return;
        }
        setLoading(true);
        const payload: any = {
            PodcastSubscriptionCreateInfo: {
                Name: formData.name,
                Description: formData.description,
                PodcastSubscriptionCycleTypePriceCreateInfoList: cycleTypePrices,
                PodcastSubscriptionBenefitMappingCreateInfoList: selectedBenefits,
            }
        };
        const payloadUpdate: any = {
            PodcastSubscriptionUpdateInfo: {
                Name: formData.name,
                Description: formData.description,
                PodcastSubscriptionCycleTypePriceUpdateInfoList: cycleTypePrices,
                PodcastSubscriptionBenefitMappingUpdateInfoList: selectedBenefits,
            }
        };

        if (isUpdateMode) {
            try {
                const res = await updateSubscription(loginRequiredAxiosInstance, subscription.Id, payloadUpdate);
                const sagaId = res?.data?.SagaInstanceId
                if (!sagaId) {
                    toast.error("Update subscription failed, please try again.")
                    return
                }
                await startPolling(sagaId, loginRequiredAxiosInstance, {
                    onSuccess: async () => {
                        onClose();
                        await context?.handleDataChange();
                        toast.success(`Subscription updated successfully!`);

                    },
                    onFailure: (err) => toast.error(err || "Saga failed!"),
                    onTimeout: () => toast.error("System not responding, please try again."),
                })
            } catch (error) {
                console.error('Error updating subscription:', error);
                toast.error('Failed to update subscription');
            } finally {
                setLoading(false);
            }
        } else {
            try {
                const res = await addShowSubscription(loginRequiredAxiosInstance, podcastShowId, payload);
                const sagaId = res?.data?.SagaInstanceId
                if (!sagaId) {
                    toast.error("Create subscription failed, please try again.")
                    return
                }
                await startPolling(sagaId, loginRequiredAxiosInstance, {
                    onSuccess: async () => {
                        onClose();
                        await context?.handleDataChange();
                        toast.success(`Subscription created successfully!`);
                    },
                    onFailure: (err) => toast.error(err || "Saga failed!"),
                    onTimeout: () => toast.error("System not responding, please try again."),
                })
            } catch (error) {
                console.error('Error creating subscription:', error);
                toast.error('Failed to create subscription');
            } finally {
                setLoading(false);
            }
        }
    };

    const handleActivate = async (isActive: boolean) => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        setActivating(true);
        try {
            const res = await activeSubscription(loginRequiredAxiosInstance, subscription.Id, isActive);
            const sagaId = res?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Activate subscription failed, please try again.")
                return
            }
            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    onClose();
                    await context?.handleDataChange();
                    toast.success(`Subscription activated successfully!`);
                },
                onFailure: (err) => {
                    if (err.includes("An Active Channel Podcast Subscription exists")) toast.error("Channel of this show is already active with another subscription!");
                    else toast.error(err || "Saga failed!");
                },
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            console.error('Error creating subscription:', error);
            toast.error('Failed to create subscription');
        } finally {
            setActivating(false);
        }
    };

    const handleDelete = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
                    toast.error('Your account is currently under violation !!');
                    return;
                }
        const alert = await confirmAlert("Are you sure to DELETE this subscription?");
        if (!alert.isConfirmed) return;
        setDeleting(true);
        try {
            const res = await deleteSubscription(loginRequiredAxiosInstance, subscription.Id);
            const sagaId = res?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Delete subscription failed, please try again.")
                return
            }
            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: () => {
                    onClose();
                    context?.handleDataChange();
                    toast.success(`Subscription deleted successfully!`);
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            console.error('Error deleting subscription:', error);
            toast.error('Failed to delete subscription');
        } finally {
            setDeleting(false);
        }
    };

    const getCycleTypeName = (id: number) => {
        return availableCycleTypes.find(ct => ct.Id === id)?.Name || 'Unknown';
    };

    const getBenefitName = (id: number) => {
        return availableBenefits.find(b => b.Id === id)?.Name || 'Unknown';
    };


    const formatCurrency = (amount: number) => {
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
    };

    // Get all price versions grouped by cycle type
    const getPriceHistory = () => {
        if (!subscription?.PodcastSubscriptionCycleTypePriceList) return [];
        const grouped: Record<number, any[]> = {};
        subscription.PodcastSubscriptionCycleTypePriceList.forEach((item: any) => {
            const typeId = item.SubscriptionCycleType.Id;
            if (!grouped[typeId]) grouped[typeId] = [];
            grouped[typeId].push(item);
        });
        // Sort each group by version descending
        Object.keys(grouped).forEach(key => {
            grouped[parseInt(key)].sort((a, b) => b.Version - a.Version);
        });
        return grouped;
    };

    // Get all benefit versions
    const getBenefitHistory = () => {
        if (!subscription?.PodcastSubscriptionBenefitMappingList) return [];
        const allVersions: Record<number, any[]> = {};
        subscription.PodcastSubscriptionBenefitMappingList.forEach((item: any) => {
            if (!allVersions[item.Version]) allVersions[item.Version] = [];
            allVersions[item.Version].push(item);
        });
        return Object.keys(allVersions)
            .map(v => parseInt(v))
            .sort((a, b) => b - a)
            .map(v => ({ version: v, benefits: allVersions[v] }));
    };

    const getRegistrationList = () => {
        return subDetail?.PodcastSubscriptionRegistrationList || [];
    };
    if (fetchingDetails) {
        return (
            <div className="flex justify-center items-center h-100">
                <Loading />
            </div>
        )
    }

    return (
        <Box className="subscription-modal-content " sx={{ p: 3 }}>
            <Typography className='text-center' variant="h5" fontWeight={600} mb={3} sx={{ color: '#fff' }}>
                {isUpdateMode ? 'Update Subscription' : 'Create New Subscription'}
            </Typography>

            <Box mb={3}>
                <TextField
                    label="Subscription Name"
                    fullWidth
                    variant="outlined"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    sx={{
                        mb: 2,
                        '& .MuiOutlinedInput-root': {
                            color: '#fff',
                            '& fieldset': { borderColor: '#444' },
                            '&:hover fieldset': { borderColor: '#666' },
                            '&.Mui-focused fieldset': { borderColor: 'var(--primary-green)' }
                        },
                        '& .MuiInputLabel-root': { color: '#888' },
                        '& .MuiInputLabel-root.Mui-focused': { color: 'var(--primary-green)' }
                    }}
                />

                {/* <TextField
                    label="Description"
                    fullWidth
                    multiline
                    rows={3}
                    variant="outlined"
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    sx={{
                        '& .MuiOutlinedInput-root': {
                            color: '#fff',
                            '& fieldset': { borderColor: '#444' },
                            '&:hover fieldset': { borderColor: '#666' },
                            '&.Mui-focused fieldset': { borderColor: 'var(--primary-green)' }
                        },
                        '& .MuiInputLabel-root': { color: '#888' },
                        '& .MuiInputLabel-root.Mui-focused': { color: 'var(--primary-green)' }
                    }}
                /> */}

                     <div className="subscription-modal-content__description-editor">
                    <div ref={quillRef} />
                </div>
            </Box>

            <Divider sx={{ borderColor: '#333', mb: 3 }} />

            {/* Pricing Section */}
            <Box mb={3}>
                <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                    <Typography variant="subtitle1" fontWeight={600} sx={{ color: '#fff' }}>
                        Pricing Plans
                    </Typography>
                    {isUpdateMode && subscription?.PodcastSubscriptionCycleTypePriceList?.length > 0 && (
                        <Button
                            size="small"
                            onClick={() => setShowPriceHistory(!showPriceHistory)}
                            sx={{ color: 'var(--primary-green)' }}
                            endIcon={showPriceHistory ? <ExpandLess /> : <ExpandMore />}
                        >
                            {showPriceHistory ? 'Hide' : 'Show'} Price History
                        </Button>
                    )}
                </Box>

                {/* Price History Table */}
                {isUpdateMode && showPriceHistory && (
                    <Box mb={3} sx={{ maxHeight: '300px', overflow: 'auto' }}>
                        {Object.entries(getPriceHistory()).map(([typeId, versions]: [string, any[]]) => (
                            <Box key={typeId} mb={2}>
                                <Typography variant="caption" sx={{ color: '#aaa', mb: 1, display: 'block' }}>
                                    {getCycleTypeName(parseInt(typeId))} Price History:
                                </Typography>
                                <TableContainer component={Paper} sx={{ backgroundColor: 'rgba(255,255,255,0.05)' }}>
                                    <Table size="small">
                                        <TableHead>
                                            <TableRow>
                                                <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Version</TableCell>
                                                <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Price</TableCell>
                                                <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Created At</TableCell>
                                                <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Updated At</TableCell>
                                            </TableRow>
                                        </TableHead>
                                        <TableBody>
                                            {versions.map((item: any, idx: number) => (
                                                <TableRow key={idx} sx={{ backgroundColor: item.Version === subscription?.CurrentVersion ? 'rgba(174, 227, 57, 0.1)' : 'transparent' }}>
                                                    <TableCell sx={{ color: '#fff' }}>
                                                        <Box display="flex" alignItems="center" gap={1}>
                                                            {item.Version}
                                                            {item.Version === subscription?.CurrentVersion && (
                                                                <Chip label="Current" size="small" sx={{ backgroundColor: 'var(--primary-green)', color: '#000', height: '20px' }} />
                                                            )}
                                                        </Box>
                                                    </TableCell>
                                                    <TableCell sx={{ color: '#fff' }}>{formatCurrency(item.Price)}</TableCell>
                                                    <TableCell sx={{ color: '#aaa', fontSize: '0.8rem' }}>{formatDate(item.CreatedAt)}</TableCell>
                                                    <TableCell sx={{ color: '#aaa', fontSize: '0.8rem' }}>{formatDate(item.UpdatedAt)}</TableCell>
                                                </TableRow>
                                            ))}
                                        </TableBody>
                                    </Table>
                                </TableContainer>
                            </Box>
                        ))}
                    </Box>
                )}

                <Typography variant="caption" sx={{ color: '#aaa', display: 'block', mb: 2 }}>
                    {isUpdateMode ? 'You are editing the latest version. Changes will create a new version.' : 'Set up pricing plans for your subscription.'}
                </Typography>

                <Box display="flex" gap={1} mb={2} flexWrap="wrap">
                    {availableCycleTypes.map((cycleType) => (
                        <Button
                            key={cycleType.Id}
                            variant={cycleTypePrices.some(p => p.SubscriptionCycleTypeId === cycleType.Id) ? 'contained' : 'outlined'}
                            size="small"
                            onClick={() => handleAddCycleTypePrice(cycleType.Id)}
                            disabled={cycleTypePrices.some(p => p.SubscriptionCycleTypeId === cycleType.Id)}
                            sx={{
                                borderColor: 'var(--primary-green)',
                                color: cycleTypePrices.some(p => p.SubscriptionCycleTypeId === cycleType.Id) ? '#000' : 'var(--primary-green)',
                                backgroundColor: cycleTypePrices.some(p => p.SubscriptionCycleTypeId === cycleType.Id) ? 'var(--primary-green)' : 'transparent',
                                '&:hover': {
                                    backgroundColor: cycleTypePrices.some(p => p.SubscriptionCycleTypeId === cycleType.Id) ? '#c4f04d' : 'rgba(174, 227, 57, 0.1)',
                                }
                            }}
                        >
                            <Add fontSize="small" sx={{ mr: 0.5 }} />
                            {cycleType.Name}
                        </Button>
                    ))}
                </Box>

                <Box display="flex" flexDirection="column" gap={2}>
                    {cycleTypePrices.map((cyclePrice) => {
                        // NEW: Check if this is an original cycle type
                        const isOriginal = isUpdateMode && originalCycleTypeIds.includes(cyclePrice.SubscriptionCycleTypeId);

                        return (
                            <Box
                                key={cyclePrice.SubscriptionCycleTypeId}
                                display="flex"
                                alignItems="center"
                                gap={2}
                                p={2}
                                className="pricing-container"
                            >
                                <Typography sx={{ minWidth: '100px', color: '#fff' }}>
                                    {getCycleTypeName(cyclePrice.SubscriptionCycleTypeId)}
                                </Typography>
                                <TextField
                                    type="number"
                                    value={cyclePrice.Price}
                                    onChange={(e) => handlePriceChange(cyclePrice.SubscriptionCycleTypeId, parseFloat(e.target.value) || 0)}
                                    size="small"
                                    fullWidth
                                    InputProps={{
                                        endAdornment: <InputAdornment position="end">VND</InputAdornment>,
                                    }}
                                    sx={{
                                        '& .MuiOutlinedInput-root': {
                                            color: '#fff',
                                            '& fieldset': { borderColor: cyclePrice.Price <= 0 ? '#f44336' : '#444' },
                                            '&:hover fieldset': { borderColor: cyclePrice.Price <= 0 ? '#f44336' : '#666' },
                                            '&.Mui-focused fieldset': { borderColor: cyclePrice.Price <= 0 ? '#f44336' : 'var(--primary-green)' }
                                        },
                                    }}
                                />
                                {/* NEW: Conditionally show delete button */}
                                {!isOriginal && (
                                    <IconButton
                                        onClick={() => handleRemoveCycleTypePrice(cyclePrice.SubscriptionCycleTypeId)}
                                        size="small"
                                        sx={{ color: '#f44336' }}
                                    >
                                        <Delete />
                                    </IconButton>
                                )}

                            </Box>
                        );
                    })}
                </Box>
            </Box>

            <Divider sx={{ borderColor: '#333', mb: 3 }} />

            {/* Benefits Section */}
            <Box mb={3}>
                <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                    <Typography variant="subtitle1" fontWeight={600} sx={{ color: '#fff' }}>
                        Benefits
                    </Typography>
                    {isUpdateMode && subscription?.PodcastSubscriptionBenefitMappingList?.length > 0 && (
                        <Button
                            size="small"
                            onClick={() => setShowBenefitHistory(!showBenefitHistory)}
                            sx={{ color: 'var(--primary-green)' }}
                            endIcon={showBenefitHistory ? <ExpandLess /> : <ExpandMore />}
                        >
                            {showBenefitHistory ? 'Hide' : 'Show'} Benefit History
                        </Button>
                    )}
                </Box>

                {/* Benefit History Table */}
                {isUpdateMode && showBenefitHistory && (
                    <Box mb={3} sx={{ maxHeight: '300px', overflow: 'auto' }}>
                        <TableContainer component={Paper} sx={{ backgroundColor: 'rgba(255,255,255,0.05)' }}>
                            <Table size="small">
                                <TableHead>
                                    <TableRow>
                                        <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Version</TableCell>
                                        <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Benefits</TableCell>
                                        <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Created At</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {getBenefitHistory().map((versionData: any, idx: number) => (
                                        <TableRow key={idx} sx={{ backgroundColor: versionData.version === subscription?.CurrentVersion ? 'rgba(174, 227, 57, 0.1)' : 'transparent' }}>
                                            <TableCell sx={{ color: '#fff' }}>
                                                <Box display="flex" alignItems="center" gap={1}>
                                                    {versionData.version}
                                                    {versionData.version === subscription?.CurrentVersion && (
                                                        <Chip label="Current" size="small" sx={{ backgroundColor: 'var(--primary-green)', color: '#000', height: '20px' }} />
                                                    )}
                                                </Box>
                                            </TableCell>
                                            <TableCell sx={{ color: '#fff' }}>
                                                <Box display="flex" flexWrap="wrap" gap={0.5}>
                                                    {versionData.benefits.map((b: any, bidx: number) => (
                                                        <Chip key={bidx} label={b.PodcastSubscriptionBenefit.Name} size="small"
                                                            sx={{
                                                                backgroundColor: 'transparent',
                                                                color: 'var(--primary-green)',
                                                            }} />
                                                    ))}
                                                </Box>
                                            </TableCell>
                                            <TableCell sx={{ color: '#aaa', fontSize: '0.8rem' }}>
                                                {versionData.benefits[0] ? formatDate(versionData.benefits[0].CreatedAt) : 'N/A'}
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    </Box>
                )}

                <Typography variant="caption" sx={{ color: '#aaa', display: 'block', mb: 2 }}>
                    {isUpdateMode ? 'You are editing the latest version. Changes will create a new version.' : 'Select benefits for your subscribers.'}
                </Typography>

                <Box display="flex" flexWrap="wrap" gap={1}>
                    {availableBenefits.map((benefit) => (
                        <Chip
                            key={benefit.Id}
                            label={benefit.Name}
                            onClick={() => handleBenefitToggle(benefit.Id)}
                            sx={{
                                backgroundColor: selectedBenefits.includes(benefit.Id) ? 'var(--primary-green)' : '#333',
                                color: selectedBenefits.includes(benefit.Id) ? '#000' : '#fff',
                                cursor: 'pointer',
                                '&:hover': {
                                    backgroundColor: selectedBenefits.includes(benefit.Id) ? '#c4f04d' : '#444',
                                }
                            }}
                        />
                    ))}
                </Box>
            </Box>

            {/* Registration List Section */}
            {isUpdateMode && getRegistrationList().length > 0 && (
                <>
                    <Divider sx={{ borderColor: '#333', mb: 3 }} />
                    <Box mb={3}>
                        <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                            <Typography variant="subtitle1" fontWeight={600} sx={{ color: '#fff' }}>
                                Subscribers ({getRegistrationList().length})
                            </Typography>
                            <Button
                                size="small"
                                onClick={() => setShowRegistrations(!showRegistrations)}
                                sx={{ color: 'var(--primary-green)' }}
                                endIcon={showRegistrations ? <ExpandLess /> : <ExpandMore />}
                            >
                                {showRegistrations ? 'Hide' : 'Show'} Details
                            </Button>
                        </Box>

                        <Collapse in={showRegistrations}>
                            <TableContainer component={Paper} sx={{ backgroundColor: 'rgba(255,255,255,0.05)', maxHeight: '400px', overflow: 'auto' }}>
                                <Table size="small">
                                    <TableHead>
                                        <TableRow>
                                            <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Customer</TableCell>
                                            <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Cycle Type</TableCell>
                                            <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Version</TableCell>
                                            <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Switch Version</TableCell>
                                            <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Revenue</TableCell>
                                            <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Last Paid</TableCell>
                                            <TableCell sx={{ color: '#fff', fontWeight: 600 }}>Cancelled</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {getRegistrationList().map((reg: any, idx: number) => (
                                            <TableRow key={idx}>
                                                <TableCell sx={{ color: '#fff' }}>{reg.Account.FullName}</TableCell>
                                                <TableCell sx={{ color: '#fff' }}>{reg.SubscriptionCycleType?.Name || 'N/A'}</TableCell>
                                                <TableCell sx={{ color: '#fff' }}>
                                                    <Chip
                                                        label={`v${reg.CurrentVersion}`}
                                                        size="small"
                                                        sx={{
                                                            backgroundColor: reg.CurrentVersion === subscription?.CurrentVersion ? 'var(--primary-green)' : '#555',
                                                            color: reg.CurrentVersion === subscription?.CurrentVersion ? '#000' : '#fff'
                                                        }}
                                                    />
                                                </TableCell>
                                                <TableCell sx={{ color: '#fff' }}>
                                                    <Chip
                                                        label={reg.IsAcceptNewestVersionSwitch === null ? 'Pending' : reg.IsAcceptNewestVersionSwitch ? 'Yes' : 'No'}
                                                        size="small"
                                                        sx={{
                                                            backgroundColor: reg.IsAcceptNewestVersionSwitch ? '#4caf50' : '#ff9800',
                                                            color: '#fff'
                                                        }}
                                                    />
                                                </TableCell>
                                                <TableCell sx={{ color: '#fff' }}>
                                                    <Chip
                                                        label={reg.IsIncomeTaken ? 'Taken' : 'Pending'}
                                                        size="small"
                                                        sx={{
                                                            backgroundColor: reg.IsIncomeTaken ? '#4caf50' : '#ff9800',
                                                            color: '#fff'
                                                        }}
                                                    />
                                                </TableCell>
                                                <TableCell sx={{ color: '#aaa', fontSize: '0.75rem' }}>
                                                    {reg.LastPaidAt ? formatDate(reg.LastPaidAt) : 'N/A'}
                                                </TableCell>
                                                <TableCell sx={{ color: '#aaa', fontSize: '0.75rem' }}>
                                                    {reg.CancelledAt ? formatDate(reg.CancelledAt) : 'Active'}
                                                </TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        </Collapse>
                    </Box>
                </>
            )}

            {/* Actions */}
            <Box display="flex" justifyContent="flex-end" gap={2} mt={4}>
                {isUpdateMode && (
                    <Button
                        onClick={handleDelete}
                        disabled={deleting || loading || activating}
                        sx={{
                            borderRadius: '8px',
                            backgroundColor: 'transparent',
                            border: '1px solid #f44336',
                            color: '#f44336',
                            fontWeight: 600,
                            '&:hover': {
                                backgroundColor: 'transparent',
                                border: '1px solid #f44336',
                                color: '#f44336',

                            },
                            '&:disabled': {
                                backgroundColor: '#333',
                                border: '1px solid #333',
                                color: '#666',
                            }
                        }}
                    >
                        {deleting ? 'Deleting...' : 'Delete'}
                    </Button>
                )}

                {isUpdateMode && subscription && !subscription.IsActive && (
                    <Button
                        onClick={() => handleActivate(true)}
                        disabled={activating || loading}
                        sx={{
                            borderRadius: '8px',
                            backgroundColor: 'transparent',
                            border: '1px solid var(--primary-green)',
                            color: 'var(--primary-green)',
                            fontWeight: 600,
                            '&:hover': {
                                backgroundColor: 'transparent',
                                border: '1px solid var(--primary-green)',
                                color: 'var(--primary-green)',
                            },
                            '&:disabled': {
                                backgroundColor: '#333',
                                border: '1px solid #333',
                                color: '#666',
                            }
                        }}
                    >
                        {activating ? 'Activating...' : 'Activate'}
                    </Button>
                )}

                {isUpdateMode && subscription && subscription.IsActive && (
                    <Button
                        onClick={() => handleActivate(false)}
                        disabled={activating || loading || deleting}
                        sx={{
                            borderRadius: '8px',
                            backgroundColor: 'transparent',
                            border: '1px solid var(--primary-green)',
                            color: 'var(--primary-green)',
                            fontWeight: 600,
                            '&:hover': {
                                backgroundColor: 'transparent',
                                border: '1px solid var(--primary-green)',
                                color: 'var(--primary-green)',
                            },
                            '&:disabled': {
                                backgroundColor: '#333',
                                border: '1px solid #333',
                                color: '#666',
                            }
                        }}
                    >
                        {activating ? 'Inactivating...' : 'Inactive'}
                    </Button>
                )}


                <Button
                    onClick={handleSave}
                    variant="contained"
                    disabled={!formData.name || cycleTypePrices.length === 0 || selectedBenefits.length === 0 || loading || activating}
                    sx={{
                        backgroundColor: 'var(--primary-green)',
                        color: '#000',
                        fontWeight: 600,
                        '&:hover': {
                            backgroundColor: '#c4f04d',
                        },
                        '&:disabled': {
                            backgroundColor: '#333',
                            color: '#666',
                        }
                    }}
                >
                    {loading
                        ? (isUpdateMode ? 'Updating...' : 'Creating...')
                        : (isUpdateMode ? 'Update' : 'Create')}

                </Button>
            </Box>
        </Box>
    );
};


export default SubscriptionModal;