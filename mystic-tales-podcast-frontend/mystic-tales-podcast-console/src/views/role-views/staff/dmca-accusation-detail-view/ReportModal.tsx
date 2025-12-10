import React, { useContext, useState } from 'react';
import { DMCAAccusationDetailViewContext } from '.';
import { CButton, CForm, CFormInput, CFormLabel, CFormSelect, CFormTextarea } from '@coreui/react';
import { toast } from 'react-toastify';
import { useSagaPolling } from '@/hooks/useSagaPolling';
import { createReport } from '@/core/services/dmca/dmca.service';
import { useParams } from 'react-router-dom';
import { staffAxiosInstance } from '@/core/api/rest-api/config/instances/v2/staff-axios-instance';


export const reportTypes = [
    { Id: 5, Name: "Podcaster Lawsuit Win" },
    { Id: 6, Name: "Accuser Lawsuit Win" }
];

interface ReportModalProps {
    onClose: () => void;
    DmcaAccusationConclusionReportTypeId?: number | null;
}

const ReportModal: React.FC<ReportModalProps> = ({ onClose, DmcaAccusationConclusionReportTypeId }) => {
    const context = useContext(DMCAAccusationDetailViewContext);
    const { id } = useParams<{ id: string }>()
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [formData, setFormData] = useState({
        DmcaAccusationConclusionReportTypeId: DmcaAccusationConclusionReportTypeId || 0,
        Description: '',
        InvalidReason: ''
    });
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        if (!formData.DmcaAccusationConclusionReportTypeId) {
            toast.error('Please select a report type');
            return;
        }
        const payload = {
            DMCAAccusationConclusationReportInfo: {
                DmcaAccusationConclusionReportTypeId: formData.DmcaAccusationConclusionReportTypeId,
                Description: formData.Description || undefined,
                InvalidReason: formData.InvalidReason
            }
        };

        try {
            const response = await createReport(staffAxiosInstance, context?.dmcaAccusationId || Number(id) , payload)
            const sagaId = response?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Submit failed, please try again.")
                return
            }
            await startPolling(sagaId, staffAxiosInstance, {
                onSuccess: () => {
                    toast.success('Submit successfully')
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
    };


    return (
        <div>
            <div className="report-modal" onClick={(e) => e.stopPropagation()}>
                <CForm  className="report-modal__form">
                    <div className="report-modal__body">
                        {DmcaAccusationConclusionReportTypeId === null && (
                            <div className="report-modal__section">
                                <CFormLabel className="report-modal__label">
                                    Report Type <span className="report-modal__required">*</span>
                                </CFormLabel>
                                <CFormSelect
                                    className="report-modal__select"
                                    value={formData.DmcaAccusationConclusionReportTypeId}
                                    onChange={(e) => setFormData({ ...formData, DmcaAccusationConclusionReportTypeId: Number(e.target.value) })}
                                    required
                                >
                                    <option value={0}>Select report type...</option>
                                    {reportTypes.map((type) => (
                                        <option key={type.Id} value={type.Id}>
                                            {type.Name}
                                        </option>
                                    ))}
                                </CFormSelect>
                            </div>
                        )}

                        <div className="report-modal__section">
                            <CFormLabel className="report-modal__label">
                                Invalid Reason <span className="report-modal__required">*</span>
                            </CFormLabel>
                            <CFormTextarea
                                className="report-modal__textarea"
                                rows={1}
                                placeholder="Enter invalid reason..."
                                value={formData.InvalidReason}
                                onChange={(e) => setFormData({ ...formData, InvalidReason: e.target.value })}
                                required
                            />
                        </div>
                        <div className="report-modal__section">
                            <CFormLabel className="report-modal__label">
                                Description <span className="report-modal__optional">(Optional)</span>
                            </CFormLabel>
                            <CFormTextarea
                                className="report-modal__textarea"
                                rows={3}
                                placeholder="Enter detailed description of the conclusion..."
                                value={formData.Description}
                                onChange={(e) => setFormData({ ...formData, Description: e.target.value })}

                            />
                        </div>
                    </div>

                    <div className="report-modal__footer">

                        <CButton
                            type="submit"
                            className="report-modal__btn report-modal__btn--submit"
                            disabled={isSubmitting}
                            onClick={handleSubmit}
                        >
                             {isSubmitting ? 'Submitting...' : 'Submit Report'}
                        </CButton>
                    </div>
                </CForm>
            </div>
        </div>
    );
};

export default ReportModal;