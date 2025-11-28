import React, { useState, useEffect } from 'react';
import {
    TextField,
    Button,
    Typography,
    Box,
    IconButton,
    Chip,
    Divider,
    InputAdornment,
} from '@mui/material';
import { Add, Delete } from '@mui/icons-material';
import './modal-styles.scss';

/**
 * SubscriptionModal Component
 * Supports both CREATE and UPDATE operations
 * 
 * CREATE mode requires: PodcastChannelId
 * UPDATE mode requires: PodcastSubscriptionId (from subscription prop)
 * 
 * API Payload:
 * {
 *   "PodcastSubscriptionCreateInfo": {
 *     "Name": "string",
 *     "Description": "string",
 *     "PodcastSubscriptionCycleTypePriceCreateInfoList": [
 *       { "SubscriptionCycleTypeId": 0, "Price": 0 }
 *     ],
 *     "PodcastSubscriptionBenefitMappingCreateInfoList": [0]
 *   }
 * }
 */

interface SubscriptionModalProps {
    subscription?: any; // If provided, it's UPDATE mode
    podcastChannelId?: string; // Required for CREATE mode
    onClose?: () => void;
    onSave?: (data: any) => void;
}

interface CycleTypePrice {
    SubscriptionCycleTypeId: number;
    Price: number;
}

// Mock available cycle types
const availableCycleTypes = [
    { Id: 1, Name: "Monthly" },
    { Id: 2, Name: "Annually" },
];

// Mock available benefits
const availableBenefits = [
    { Id: 1, Name: "Non-Quota Listening" },
    { Id: 2, Name: "Subscriber-Only Shows" },
    { Id: 3, Name: "Subscriber-Only Episodes" },
    { Id: 4, Name: "Shows/Episodes Early Access" },
    { Id: 5, Name: "Ad-Free Experience" },
    { Id: 6, Name: "Download Episodes" },
];

const SubscriptionModal: React.FC<SubscriptionModalProps> = ({
    subscription,
    podcastChannelId,
    onClose,
    onSave
}) => {
    const isUpdateMode = !!subscription;

    // Form state
    const [formData, setFormData] = useState({
        name: '',
        description: '',
    });

    const [cycleTypePrices, setCycleTypePrices] = useState<CycleTypePrice[]>([]);
    const [selectedBenefits, setSelectedBenefits] = useState<number[]>([]);

    useEffect(() => {
        if (isUpdateMode && subscription) {
            // Populate form for UPDATE mode
            setFormData({
                name: subscription.Name || '',
                description: subscription.Description || '',
            });

            // Map existing cycle type prices
            const existingPrices = subscription.PodcastSubscriptionCycleTypePriceList?.map((item: any) => ({
                SubscriptionCycleTypeId: item.SubscriptionCycleType.Id,
                Price: item.Price,
            })) || [];
            setCycleTypePrices(existingPrices);

            // Map existing benefits
            const existingBenefits = subscription.PodcastSubscriptionBenefitMappingList?.map(
                (item: any) => item.PodcastSubscriptionBenefit.Id
            ) || [];
            setSelectedBenefits(existingBenefits);
        } else {
            // Initialize empty form for CREATE mode
            setFormData({
                name: '',
                description: '',
            });
            setCycleTypePrices([]);
            setSelectedBenefits([]);
        }
    }, [subscription, isUpdateMode]);

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

    const handleSave = () => {
        // Prepare API payload
        const payload: any = {
            PodcastSubscriptionCreateInfo: {
                Name: formData.name,
                Description: formData.description,
                PodcastSubscriptionCycleTypePriceCreateInfoList: cycleTypePrices,
                PodcastSubscriptionBenefitMappingCreateInfoList: selectedBenefits,
            }
        };

        if (isUpdateMode) {
            payload.PodcastSubscriptionId = subscription.Id;
        } else {
            payload.PodcastChannelId = podcastChannelId;
        }

        console.log(isUpdateMode ? 'Updating subscription:' : 'Creating subscription:', payload);
        onSave?.(payload);
        onClose?.();
    };

    const getCycleTypeName = (id: number) => {
        return availableCycleTypes.find(ct => ct.Id === id)?.Name || 'Unknown';
    };

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

                <TextField
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
                />
            </Box>

            <Divider sx={{ borderColor: '#333', mb: 3 }} />

            {/* Pricing Section */}
            <Box mb={3}>
                <Typography variant="subtitle1" fontWeight={600} mb={2} sx={{ color: '#fff' }}>
                    Pricing Plans
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
                    {cycleTypePrices.map((cyclePrice) => (
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
                                        '& fieldset': { borderColor: '#444' },
                                        '&:hover fieldset': { borderColor: '#666' },
                                        '&.Mui-focused fieldset': { borderColor: 'var(--primary-green)' }
                                    }
                                }}
                            />
                            <IconButton
                                onClick={() => handleRemoveCycleTypePrice(cyclePrice.SubscriptionCycleTypeId)}
                                size="small"
                                sx={{ color: '#f44336' }}
                            >
                                <Delete />
                            </IconButton>
                        </Box>
                    ))}
                </Box>
            </Box>

            <Divider sx={{ borderColor: '#333', mb: 3 }} />

            {/* Benefits Section */}
            <Box mb={3}>
                <Typography variant="subtitle1" fontWeight={600} mb={2} sx={{ color: '#fff' }}>
                    Benefits
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

            {/* Actions */}
            <Box display="flex" justifyContent="flex-end" gap={2} mt={4}>
                <Button onClick={onClose} sx={{ color: '#888' }}>
                    Cancel
                </Button>
                <Button
                    onClick={handleSave}
                    variant="contained"
                    disabled={!formData.name || cycleTypePrices.length === 0}
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
                    {isUpdateMode ? 'Update' : 'Create'}
                </Button>
            </Box>
        </Box>
    );
};

export default SubscriptionModal;