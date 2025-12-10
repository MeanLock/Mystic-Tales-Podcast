import React, { useContext, useRef, useState } from 'react';
import { DMCAAccusationDetailViewContext } from '.';
import { CButton, CForm, CFormInput, CFormLabel, CFormSelect, CFormTextarea } from '@coreui/react';
import { toast } from 'react-toastify';
import { useSagaPolling } from '@/hooks/useSagaPolling';
import { createReport, updateStatus } from '@/core/services/dmca/dmca.service';
import { useParams } from 'react-router-dom';
import { staffAxiosInstance } from '@/core/api/rest-api/config/instances/v2/staff-axios-instance';
import { Paperclip, X } from 'phosphor-react';




interface ValidModalProps {
    onClose: () => void;
    status: string;
}

const ValidModal: React.FC<ValidModalProps> = ({ onClose, status }) => {
    const context = useContext(DMCAAccusationDetailViewContext);
    const { id } = useParams<{ id: string }>()
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [attachFiles, setAttachFiles] = useState<File[]>([])
    const fileInputRef = useRef<HTMLInputElement>(null);

    // Valid DMCA Notice reason selection
    const VALID_TAKEDOWN_REASONS = [
        'DuplicateContent',
        'RestrictedTermsViolation',
        'ExplicitOrAdultContent',
        'HateSpeech',
        'HarassmentAbuse',
        'PrivacyViolation',
        'Impersonation',
        'MisinformationFalseClaims',
        'PromotingIllegalActivity',
        'LawsuitDMCA',
    ] as const
    const [selectedValidReason, setSelectedValidReason] = useState<string>('DuplicateContent')
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })

    const handleVerify = async () => {
        setIsSubmitting(true);
        try {
            const payload = {
                AttachmentFiles: attachFiles
            }
            const response = await updateStatus(staffAxiosInstance, status, Number(id), selectedValidReason, payload)
            const sagaId = response?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Verify failed, please try again.")
                return
            }
            await startPolling(sagaId, staffAxiosInstance, {
                onSuccess: () => {
                    toast.success('Verify successfully')
                    context?.handleDataChange();
                    onClose();
                },
                onFailure: (err: any) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            console.error('Error verifying status:', error)
            toast.error('Failed to verify status')
        } finally {
            setIsSubmitting(false);
        }
    }

    const ALLOWED_EXTENSIONS = [
        "pdf", "doc", "docx", "xls", "xlsx", "txt", "csv",
        "wav", "flac", "mp3", "zip", "rar"
    ];

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files;
        if (files) {
            const newFiles = Array.from(files);
            const validFiles = newFiles.filter(file => {
                if (file.size > 50 * 1024 * 1024) {
                    toast.error(`File ${file.name} exceeds 50MB limit`);
                    return false;
                }
                const ext = file.name.split('.').pop()?.toLowerCase();
                if (!ext || !ALLOWED_EXTENSIONS.includes(ext)) {
                    toast.error(`File ${file.name} is not an allowed type`);
                    return false;
                }
                return true;
            });
            setAttachFiles(prev => [...prev, ...validFiles]);
            if (fileInputRef.current) fileInputRef.current.value = "";
        }
    }

    const removeFile = (index: number) => {
        setAttachFiles(prev => prev.filter((_, i) => i !== index))
        if (fileInputRef.current) fileInputRef.current.value = ""
    }


    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="valid-modal" onClick={(e) => e.stopPropagation()}>
                <CForm className="valid-modal__form">
                    <div className="valid-modal__header">
                        <h3 className="valid-modal__title">Verify DMCA Notice</h3>
                        <button 
                            type="button" 
                            className="valid-modal__close" 
                            onClick={onClose}
                            aria-label="Close"
                        >
                            <X size={24} weight="bold" />
                        </button>
                    </div>

                    <div className="valid-modal__body">
                        {/* Reason selector */}
                        {status === "VALID_DMCA_NOTICE" && (
                            <div className="valid-modal__section">
                                <CFormLabel className="valid-modal__label">
                                    Taken Down Reason <span className="valid-modal__required">*</span>
                                </CFormLabel>
                                <CFormSelect
                                    className="valid-modal__select"
                                    value={selectedValidReason}
                                    onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setSelectedValidReason(e.target.value)}
                                    required
                                >
                                    {VALID_TAKEDOWN_REASONS.map((r) => (
                                        <option key={r} value={r}>{r}</option>
                                    ))}
                                </CFormSelect>
                            </div>
                        )}

                        {/* Attachments */}
                        <div className="valid-modal__section">
                            <CFormLabel className="valid-modal__label">
                                Attachments <span className="valid-modal__optional">(Optional)</span>
                            </CFormLabel>

                            <label className="valid-modal__upload-btn">
                                <input
                                    type="file"
                                    multiple
                                    accept=".pdf,.doc,.docx,.xls,.xlsx,.txt,.csv,.wav,.flac,.mp3,.zip,.rar"
                                    onChange={handleFileChange}
                                    className="d-none"
                                    ref={fileInputRef}
                                />
                                <Paperclip size={20} />
                                <span>Add Files</span>
                            </label>
                            <p className="valid-modal__hint">Max 50MB per file. Allowed: PDF, DOC, XLS, TXT, CSV, Audio, Archives</p>

                            {attachFiles.length > 0 && (
                                <div className="valid-modal__files">
                                    {attachFiles.map((file, index) => (
                                        <div key={index} className="valid-modal__file">
                                            <Paperclip size={16} className="valid-modal__file-icon" />
                                            <span className="valid-modal__file-name">{file.name}</span>
                                            <button
                                                type="button"
                                                onClick={() => removeFile(index)}
                                                className="valid-modal__file-remove"
                                                aria-label="Remove file"
                                            >
                                                <X size={16} />
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>

                    <div className="valid-modal__footer">
                        <CButton
                            type="button"
                            className="valid-modal__btn valid-modal__btn--cancel"
                            onClick={onClose}
                            disabled={isSubmitting}
                        >
                            Cancel
                        </CButton>
                        <CButton
                            type="button"
                            className="valid-modal__btn valid-modal__btn--submit"
                            disabled={isSubmitting}
                            onClick={handleVerify}
                        >
                            {isSubmitting ? 'Verifying...' : 'Verify & Confirm'}
                        </CButton>
                    </div>
                </CForm>
            </div>
        </div>
    );
};

export default ValidModal;