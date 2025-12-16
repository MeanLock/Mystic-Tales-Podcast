import React, { FC, use, useContext, useState } from 'react';
import {
    Button,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    IconButton,
    Typography,
    Card,
    CardContent,
    Chip,
    Box,
    TextField,
} from '@mui/material';
import { Close as CloseIcon, CloudUpload, Delete, Timer } from '@mui/icons-material';
import { formatDate } from '@/core/utils/date.util';
import { set } from 'lodash';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { toast } from 'react-toastify';
import { addDealing } from '@/core/services/booking/booking.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { BookingDetailPageContext } from '.';
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';


interface BookingRequirementInfo {
    Id: string;
    WordCount: number;
}

interface DealingModalProps {
    booking: any;
    onClose: () => void;
}


const DealingModal: FC<DealingModalProps> = ({ booking, onClose }) => {
    const context = useContext(BookingDetailPageContext);
    const Booking = booking;
    const authSlice = useSelector((state: RootState) => state.auth);
    const [deadline, setDeadline] = useState<number>(0);
    const [submitting, setSubmitting] = useState(false);
    const [requirementInfoList, setRequirementInfoList] = useState<BookingRequirementInfo[]>(
        Booking.BookingRequirementFileList.map(req => ({
            Id: req.Id,
            WordCount: 0
        }))
    );
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })

    const unitPricePerThousand = Number(authSlice.user?.PricePerBookingWord ?? 0);
    const calcPrice = (wordCount: number) => (Math.max(0, wordCount) ) * unitPricePerThousand;
    const fmtPoints = (n: number) => `${Math.round(n).toLocaleString('vi-VN')} coins`;
    const totalPrice = React.useMemo(
        () => requirementInfoList.reduce((sum, item) => sum + calcPrice(item.WordCount || 0), 0),
        [requirementInfoList, unitPricePerThousand]
    );

    const handleWordCountChange = (requirementId: string, wordCount: number) => {
        setRequirementInfoList(prev =>
            prev.map(item =>
                item.Id === requirementId
                    ? { ...item, WordCount: wordCount }
                    : item
            )
        );
    };

    const handleSubmitDealing = async () => {
        setSubmitting(true);
        const payload = {
            BookingDealingInfo: {
                BookingRequirementInfoList: requirementInfoList,
                DeadlineDayCount: deadline
            }
        };
        try {
            const res = await addDealing(loginRequiredAxiosInstance, Booking.Id, payload);
            const sagaId = res?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Dealing failed, please try again.")
                return
            }
            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    onClose();
                    await context?.handleDataChange();
                    toast.success(`Dealing successfully!`);
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error dealing");
        } finally {
            setSubmitting(false);
        }

    };

    return (
        <div>
            <div className="booking-detail__modal-content">
                {/* Deadline Input */}
                <div className="booking-detail__upload-card">
                    <div className="booking-detail__upload-card-header">

                        <div className="booking-detail__upload-info">
                            <Typography className="booking-detail__upload-title">
                                Deadline Day
                            </Typography>
                        </div>
                    </div>

                    <div className="booking-detail__upload-area">
                        <TextField
                            type="number"
                            inputProps={{ min: 0 }}
                            value={deadline}
                            onChange={(e) => setDeadline(Number(e.target.value))}
                            fullWidth
                            InputLabelProps={{
                                shrink: true,
                            }}
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    color: 'white',
                                    backgroundColor: 'rgba(255,255,255,0.08)',
                                    border: '1.5px solid #AEE339',
                                    borderRadius: '10px',
                                    '&:hover fieldset': {
                                        borderColor: '#AEE339',
                                    },
                                    '&.Mui-focused fieldset': {
                                        borderColor: '#AEE339',
                                    },
                                },
                                '& .MuiInputLabel-root': {
                                    color: 'rgba(255,255,255,0.7)',
                                },
                            }}
                        />
                    </div>
                </div>

                {/* Requirements Word Count */}
                {Booking.BookingRequirementFileList.map((req, index) => {
                    const requirementInfo = requirementInfoList.find(item => item.Id === req.Id);
                    const price = calcPrice(requirementInfo?.WordCount || 0);

                    return (
                        <div key={req.Id} className="booking-detail__upload-card">
                            <div className="booking-detail__upload-card-header">
                                <div className="booking-detail__requirement-badge">
                                    {index + 1}
                                </div>
                                <div className="booking-detail__upload-info">
                                    <Typography className="booking-detail__upload-title">
                                        {req.Name}
                                    </Typography>
                                </div>
                            </div>

                            <div className="booking-detail__upload-area">
                                <Typography
                                    variant="caption"
                                    sx={{
                                        color: 'rgba(255,255,255,0.5)',
                                        mt: 1,
                                        display: 'block',
                                        fontSize: '0.9rem'
                                    }}
                                >
                                    Word Count
                                </Typography>
                                <input
                                    type="number"
                                    min={0}
                                    className="booking-detail__input w-full rounded-md"
                                    value={requirementInfo?.WordCount || 0}
                                    onChange={(e) => handleWordCountChange(req.Id, parseInt(e.target.value) || 0)}

                                />
                                <Typography variant="caption" color="warning.main" sx={{ mt: 1, display: 'block' }}>
                                    Price: {fmtPoints(price)}{' '}

                                </Typography>
                            </div>
                        </div>
                    );
                })}
            </div>

            <div className="booking-detail__modal-footer">
                <div className="booking-detail__upload-summary">
                    <Typography className="booking-detail__summary-text">
                        {requirementInfoList.filter(item => item.WordCount > 0).length} of {requirementInfoList.length} requirements configured ·
                        <span style={{ marginLeft: 8, color: '#AEE339', fontWeight: 700 }}>
                            Total: {fmtPoints(totalPrice)}
                        </span>
                    </Typography>
                </div>
                <Button
                    variant="contained"
                    onClick={handleSubmitDealing}
                    disabled={requirementInfoList.some(item => item.WordCount <= 0) || submitting || deadline <= 0}
                    className="booking-detail__submit-modal-btn"
                    size="large"
                >
                    Submit Dealing
                </Button>
            </div>
        </div>
    );
};

export default DealingModal;


