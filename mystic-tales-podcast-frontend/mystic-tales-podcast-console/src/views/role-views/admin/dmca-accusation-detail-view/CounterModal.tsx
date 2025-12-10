import { FC, useState, useContext, useRef } from 'react'
import { CButton, CForm, CFormInput, CFormLabel } from '@coreui/react'
import { toast } from 'react-toastify'
import { CloudArrowUp, Paperclip, X } from 'phosphor-react'
import { createCounterNotice, createEpisodeDMCAAccusation, createShowDMCAAccusation } from '@/core/services/dmca/dmca.service'
import { useSagaPolling } from '@/hooks/useSagaPolling'
import { adminAxiosInstance } from '@/core/api/rest-api/config/instances/v2'
import { DMCAAccusationDetailViewContext } from '.'
import { useParams } from 'react-router-dom'

interface CounterModalProps {
    onClose: () => void
}

const CounterModal: FC<CounterModalProps> = ({ onClose }) => {
    const { id } = useParams<{ id: string }>()
    const context = useContext(DMCAAccusationDetailViewContext)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [attachFiles, setAttachFiles] = useState<File[]>([])
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files
        if (files) {
            const newFiles = Array.from(files)
            const validFiles = newFiles.filter(file => {
                if (file.size > 70 * 1024 * 1024) {
                    toast.error(`File ${file.name} exceeds 70MB limit`)
                    return false
                }
                if (file.type !== "application/pdf") {
                    toast.error(`File ${file.name} is not a PDF`)
                    return false
                }
                return true
            })
            setAttachFiles(prev => [...prev, ...validFiles])
            if (fileInputRef.current) fileInputRef.current.value = ""
        }
    }


    const removeFile = (index: number) => {
        setAttachFiles(prev => prev.filter((_, i) => i !== index))
        if (fileInputRef.current) fileInputRef.current.value = ""
    }


    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()

        setIsSubmitting(true)

        if (attachFiles.length <= 0) {
            toast.error('Please attach at least one file')
            setIsSubmitting(false)
            return
        }

        const payload = {
            CounterNoticeAttachFiles: attachFiles
        }
        try {
            const response = await createCounterNotice(adminAxiosInstance, context.dmcaAccusationId || Number(id), payload)
            const sagaId = response?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Add Counter Notice failed, please try again.")
                return
            }
            await startPolling(sagaId, adminAxiosInstance, {
                onSuccess: () => {
                    toast.success('Counter Notice added successfully')
                    context?.handleDataChange()
                    onClose()
                },
                onFailure: (err: any) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            console.error('Error creating Counter Notice:', error)
            toast.error('Failed to add Counter Notice')
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <div className="dmca-modal">
            <CForm onSubmit={handleSubmit} className="dmca-modal__form">
                <div className="dmca-modal__content">
                    <div className="dmca-modal__section">
                        <h4 className="dmca-modal__section-title">Attachments</h4>

                        <div className="dmca-modal__field">
                            <label className="dmca-modal__upload-btn">
                                <input
                                    type="file"
                                    multiple
                                    onChange={handleFileChange}
                                    ref={fileInputRef}
                                    className="d-none"
                                />
                                <Paperclip size={20} className="me-2" />
                                Add Files
                            </label>
                            <p className="dmca-modal__hint">Max 70MB per file, PDF only</p>
                        </div>

                        {attachFiles.length > 0 && (
                            <div className="dmca-modal__files">
                                {attachFiles.map((file, index) => (
                                    <div key={index} className="dmca-modal__file">
                                        <span className="dmca-modal__file-name">{file.name}</span>
                                        <button
                                            type="button"
                                            onClick={() => removeFile(index)}
                                            className="dmca-modal__file-remove"
                                        >
                                            <X size={16} />
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>

                <div className="dmca-modal__actions">
                    <CButton
                        type="submit"
                        color="success"
                        disabled={isSubmitting}
                        className="dmca-modal__btn dmca-modal__btn--submit"
                    >
                        <CloudArrowUp size={20} className="me-2" />
                        {isSubmitting ? 'Submitting...' : 'Submit'}
                    </CButton>
                </div>
            </CForm>
        </div>
    )
}

export default CounterModal