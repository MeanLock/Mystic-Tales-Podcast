import React, { useContext, useState } from 'react';
import { CButton, CSpinner } from '@coreui/react';
import { formatDate } from '@/core/utils/date.util';
import { toast } from 'react-toastify';
import { confirmTransaction } from '@/core/services/transaction/transaction.service';
import { adminAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { TransactionViewContext } from './index';
import Image from '@/views/components/common/image';
import './transaction-modal.scss';
import { useSagaPolling } from '@/hooks/useSagaPolling';

interface TransactionModalProps {
    transaction: {
        Id: string;
        Amount: number;
        TransferReceiptImageFileKey: string;
        RejectReason: string;
        IsRejected: boolean | null;
        CompletedAt: string | null;
        CreatedAt: string;
        UpdatedAt: string;
        Account: {
            FullName: string;
            Email: string;
        };
    };
    onClose: () => void;
}

const TransactionModal: React.FC<TransactionModalProps> = ({ transaction, onClose }) => {
    const context = useContext(TransactionViewContext);
    const [isLoading, setIsLoading] = useState(false);
    const [rejectReason, setRejectReason] = useState('');
    const [receiptFile, setReceiptFile] = useState<File | null>(null);
    const [previewUrl, setPreviewUrl] = useState<string | null>(null);
    const [showPopup, setShowPopup] = useState(false);
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 60,
        intervalSeconds: 2,
    })
    const isCompleted = transaction.CompletedAt !== null;
    const statusInfo = {
        text: transaction.IsRejected === null ? 'Pending' : transaction.IsRejected ? 'Rejected' : 'Success',
        color: transaction.IsRejected === null ? '#ffa500' : transaction.IsRejected ? '#ef4444' : '#aee339'
    };
    const handleClosePopup = () => {
        setShowPopup(false)
        setRejectReason('')
    }
    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/svg+xml'];
            if (!allowedTypes.includes(file.type)) {
                toast.error('Invalid file type. Allowed: JPG, JPEG, PNG, GIF, WEBP, SVG');
                return;
            }

            // Validate file size (3MB max)
            const maxSize = 3 * 1024 * 1024; // 3MB in bytes
            if (file.size > maxSize) {
                toast.error('Image file size must be less than 3MB');
                return;
            }
            setReceiptFile(file);
            const url = URL.createObjectURL(file);
            setPreviewUrl(url);
        }
    };

    const handleConfirm = async (isReject: boolean) => {
        if (!isReject && !receiptFile) {
            toast.error('Please upload transfer receipt image');
            return;
        }

        if (isReject && !rejectReason.trim()) {
            toast.error('Please provide a rejection reason');
            return;
        }

        setIsLoading(true);
        try {
            const payload = {
                TransferReceiptImageFile: receiptFile || new File([], ''),
                AccountBalanceWithdrawalRequestInfo: {
                    RejectedReason: isReject ? rejectReason : ''
                }
            };

            const res = await confirmTransaction(adminAxiosInstance, transaction.Id, isReject, payload);
            const sagaId = res?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Resolve failed, please try again.")
                return
            }
            await startPolling(sagaId, adminAxiosInstance, {
                onSuccess: async () => {
                    toast.success(isReject ? 'Transaction rejected successfully' : 'Transaction confirmed successfully');
                    await context?.handleDataChange();
                    onClose();
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })

        } catch (error) {
            console.error('Error confirming transaction:', error);
            toast.error('An error occurred while processing transaction');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="transaction-modal">
            <div className="transaction-modal__header">
                <h3 className="transaction-modal__title">Transaction Details</h3>
                <div
                    className="transaction-modal__status"
                    style={{ backgroundColor: `${statusInfo.color}20`, borderColor: statusInfo.color }}
                >
                    <span style={{ color: statusInfo.color }}>{statusInfo.text}</span>
                </div>
            </div>

            <div className="transaction-modal__content">
                {/* Customer Information */}
                <div className="transaction-modal__section">
                    <h4 className="transaction-modal__section-title">Customer Information</h4>
                    <div className="transaction-modal__info-grid">
                        <div className="transaction-modal__info-item">
                            <label>Full Name</label>
                            <p>{transaction.Account.FullName}</p>
                        </div>
                        <div className="transaction-modal__info-item">
                            <label>Email</label>
                            <p>{transaction.Account.Email}</p>
                        </div>
                    </div>
                </div>

                {/* Transaction Information */}
                <div className="transaction-modal__section">
                    <h4 className="transaction-modal__section-title">Transaction Information</h4>
                    <div className="transaction-modal__info-grid">
                        <div className="transaction-modal__info-item">
                            <label>Amount</label>
                            <p className="transaction-modal__amount">{transaction.Amount.toLocaleString()} VND</p>
                        </div>
                        <div className="transaction-modal__info-item">
                            <label>Created At</label>
                            <p>{formatDate(transaction.CreatedAt)}</p>
                        </div>
                        {isCompleted && (
                            <div className="transaction-modal__info-item">
                                <label>Completed At</label>
                                <p>{formatDate(transaction.CompletedAt!)}</p>
                            </div>
                        )}
                    </div>
                </div>

                {/* Rejection Reason (if rejected) */}
                {isCompleted && transaction.IsRejected && (
                    <div className="transaction-modal__section">
                        <h4 className="transaction-modal__section-title">Rejection Reason</h4>
                        <div className="transaction-modal__reject-display">
                            <p>{transaction.RejectReason}</p>
                        </div>
                    </div>
                )}

                {/* Transfer Receipt (if completed and not rejected) */}
                {isCompleted && !transaction.IsRejected && transaction.TransferReceiptImageFileKey && (
                    <div className="transaction-modal__section">
                        <h4 className="transaction-modal__section-title">Transfer Receipt</h4>
                        <div className="transaction-modal__receipt-display">
                            <Image
                                mainImageFileKey={transaction.TransferReceiptImageFileKey}
                                alt="Transfer Receipt"
                                className="transaction-modal__receipt-image"
                                type="receipt"
                            />
                        </div>
                    </div>
                )}

                {/* Action Section (if not completed) */}
                {!isCompleted && (
                    <>
                        <div className="transaction-modal__section">
                            <h4 className="transaction-modal__section-title">Upload Transfer Receipt</h4>
                            <div className="transaction-modal__upload">
                                <input
                                    type="file"
                                    accept=".jpg,.jpeg,.png,.gif,.webp,.svg"
                                    onChange={handleFileChange}
                                    className="transaction-modal__file-input"
                                    id="receipt-upload"
                                />
                                <label htmlFor="receipt-upload" className="transaction-modal__upload-label">
                                    {previewUrl ? 'Change Image' : 'Choose Image'}
                                </label>
                                {previewUrl && (
                                    <div className="transaction-modal__preview">
                                        <img src={previewUrl} alt="Preview" className="transaction-modal__preview-image" />
                                    </div>
                                )}
                            </div>
                        </div>


                    </>
                )}
            </div>

            {/* Action Buttons */}
            {!isCompleted && (
                <div className="transaction-modal__actions">
                    <CButton
                        className="transaction-modal__button transaction-modal__button--reject"
                        onClick={() => setShowPopup(true)}
                        disabled={isLoading}
                    >
                        {isLoading ? <CSpinner size="sm" /> : 'Reject'}
                    </CButton>
                    <CButton
                        className="transaction-modal__button transaction-modal__button--confirm"
                        onClick={() => handleConfirm(false)}
                        disabled={isLoading}
                    >
                        {isLoading ? <CSpinner size="sm" /> : 'Confirm'}
                    </CButton>
                </div>
            )}
            {showPopup && (
                <div className="resolve-popup-overlay" >
                    <div className="resolve-popup" onClick={(e) => e.stopPropagation()}>
                        <div className="resolve-popup__header">
                            <button
                                className="resolve-popup__close-btn"
                                onClick={handleClosePopup}
                                type="button"
                            >
                                ×
                            </button>
                        </div>
                        <div className="resolve-popup__body">
                            <div className="resolve-popup__field">
                                <label className="resolve-popup__label">
                                    Reject Reason <span className="text-danger">*</span>
                                </label>
                                <input
                                    type="text"
                                    className="resolve-popup__input"
                                    placeholder="Enter Reject Reason"
                                    value={rejectReason}
                                    onChange={(e) => setRejectReason(e.target.value)}
                                    onKeyPress={(e) => {
                                        if (e.key === 'Enter') {
                                            handleConfirm(true)
                                        }
                                    }}
                                />
                            </div>
                            <div className="resolve-popup__actions">

                                <button
                                    type="button"
                                    className="resolve-popup__btn resolve-popup__btn--resolve"
                                    onClick={() => handleConfirm(true)}
                                    disabled={isLoading}
                                >
                                    Confirm
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

        </div>
    );
};

export default TransactionModal;