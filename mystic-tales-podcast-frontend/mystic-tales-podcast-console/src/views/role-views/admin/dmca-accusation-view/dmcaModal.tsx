import { FC, useState, useContext, useRef, useEffect } from 'react'
import { CButton, CForm, CFormInput, CFormLabel } from '@coreui/react'
import { toast } from 'react-toastify'
import { CloudArrowUp, Paperclip, X } from 'phosphor-react'
import { DMCAAccusationViewContext } from '.'
import { createEpisodeDMCAAccusation, createShowDMCAAccusation } from '@/core/services/dmca/dmca.service'
import { useSagaPolling } from '@/hooks/useSagaPolling'
import { adminAxiosInstance } from '@/core/api/rest-api/config/instances/v2'
import { getShowDMCAList } from '@/core/services/show/show.service'
import { getEpisodeDMCAList } from '@/core/services/episode/episode.service'
import { formatDate } from '@/core/utils/date.util'
import Image from '@/views/components/common/image'

interface DmcaModalProps {
    type: 'show' | 'episode'
    onClose: () => void
}

const DmcaModal: FC<DmcaModalProps> = ({ type, onClose }) => {
    const context = useContext(DMCAAccusationViewContext)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [attachFiles, setAttachFiles] = useState<File[]>([])
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [formData, setFormData] = useState({
        AccuserEmail: '',
        AccuserPhone: '',
        AccuserFullName: '',
        ShowId: '',
        EpisodeId: ''
    })
    const [data, setData] = useState<any[]>([])
    const [isLoading, setIsLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');

    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })

    const fetchShowList = async () => {
        setIsLoading(true);
        try {
            const res = await getShowDMCAList(adminAxiosInstance);
            console.log("Fetched shows:", res);
            if (res.success) {
                setData(res.data.ShowList || []);
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch shows:', error);
        } finally {
            setIsLoading(false);
        }
    }

    const fetchEpisodeList = async () => {
        setIsLoading(true);
        try {
            const res = await getEpisodeDMCAList(adminAxiosInstance);
            console.log("Fetched episodes:", res);
            if (res.success) {
                setData(res.data.EpisodeList || []);
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch shows:', error);
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        if (type === 'show') {
            fetchShowList();
        } else if (type === 'episode') {
            fetchEpisodeList();
        }
    }, [type]);

    const filteredData = data.filter((item) => {
        const id = String(item?.Id || '').toLowerCase();
        const name = String(item?.Name || '').toLowerCase();
        const q = searchTerm.toLowerCase().trim();
        if (!q) return true;
        return id.includes(q) || name.includes(q);
    });

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target
        setFormData(prev => ({
            ...prev,
            [name]: value
        }))
    }
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

    const validateForm = () => {
        if (!formData.AccuserEmail.trim()) {
            toast.error('Please enter accuser email')
            return false
        }
        if (!formData.AccuserPhone.trim()) {
            toast.error('Please enter accuser phone')
            return false
        }
        if (!formData.AccuserFullName.trim()) {
            toast.error('Please enter accuser full name')
            return false
        }
        if (type === 'show' && !formData.ShowId.trim()) {
            toast.error('Please select a Show')
            return false
        }
        if (type === 'episode' && !formData.EpisodeId.trim()) {
            toast.error('Please select an Episode')
            return false
        }
        return true
    }

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        if (!validateForm()) return
        if (attachFiles.length <= 0) {
            toast.error('Please attach at least one file')
            setIsSubmitting(false)
            return
        }

        setIsSubmitting(true)
        const payload: any = {
            DMCANoticeCreateInfo: {
                AccuserEmail: formData.AccuserEmail,
                AccuserPhone: formData.AccuserPhone,
                AccuserFullName: formData.AccuserFullName
            }
        }
        if (attachFiles.length > 0) {
            payload.DMCANoticeAttachFiles = attachFiles
        }

        if (type === 'show') {
            payload.DMCANoticeCreateInfo.PodcastShowId = formData.ShowId
            try {
                const response = await createShowDMCAAccusation(adminAxiosInstance, formData.ShowId, payload)
                const sagaId = response?.data?.SagaInstanceId
                if (!sagaId) {
                    toast.error("Create DMCA Accusation failed, please try again.")
                    return
                }
                await startPolling(sagaId, adminAxiosInstance, {
                    onSuccess: () => {
                        toast.success('DMCA Accusation created successfully')
                        context?.handleDataChange()
                        onClose()
                    },
                    onFailure: (err: any) => toast.error(err || "Saga failed!"),
                    onTimeout: () => toast.error("System not responding, please try again."),
                })
            } catch (error) {
                console.error('Error creating DMCA Accusation:', error)
                toast.error('Failed to create DMCA Accusation')
            } finally {
                setIsSubmitting(false)
            }
        } else {
            payload.DMCANoticeCreateInfo.PodcastEpisodeId = formData.EpisodeId
            try {
                const response = await createEpisodeDMCAAccusation(adminAxiosInstance, formData.EpisodeId, payload)
                const sagaId = response?.data?.SagaInstanceId
                if (!sagaId) {
                    toast.error("Create DMCA Accusation failed, please try again.")
                    return
                }
                await startPolling(sagaId, adminAxiosInstance, {
                    onSuccess: () => {
                        toast.success('DMCA Accusation created successfully')
                        context?.handleDataChange()
                        onClose()
                    },
                    onFailure: (err: any) => toast.error(err || "Saga failed!"),
                    onTimeout: () => toast.error("System not responding, please try again."),
                })
            } catch (error) {
                console.error('Error creating DMCA Accusation:', error)
                toast.error('Failed to create DMCA Accusation')
            } finally {
                setIsSubmitting(false)
            }
        }


    }

    return (
        <div className="dmca-modal">
            <CForm onSubmit={handleSubmit} className="dmca-modal__form">
                <div className="dmca-modal__header">
                    <h3 className="dmca-modal__title">
                        Create {type === 'show' ? 'Show' : 'Episode'} DMCA Accusation
                    </h3>
                </div>

                <div className="dmca-modal__content">
                    {/* Accuser Information Section */}
                    <div className="dmca-modal__section">
                        <h4 className="dmca-modal__section-title">Accuser Information</h4>

                        <div className="dmca-modal__field">
                            <CFormLabel className="dmca-modal__label">
                                Full Name <span className="text-danger">*</span>
                            </CFormLabel>
                            <CFormInput
                                type="text"
                                name="AccuserFullName"
                                value={formData.AccuserFullName}
                                onChange={handleInputChange}
                                placeholder="Enter accuser full name"
                                className="dmca-modal__input"
                            />
                        </div>

                        <div className="dmca-modal__field">
                            <CFormLabel className="dmca-modal__label">
                                Email <span className="text-danger">*</span>
                            </CFormLabel>
                            <CFormInput
                                type="email"
                                name="AccuserEmail"
                                value={formData.AccuserEmail}
                                onChange={handleInputChange}
                                placeholder="Enter accuser email"
                                className="dmca-modal__input"
                            />
                        </div>

                        <div className="dmca-modal__field">
                            <CFormLabel className="dmca-modal__label">
                                Phone <span className="text-danger">*</span>
                            </CFormLabel>
                            <CFormInput
                                type="tel"
                                name="AccuserPhone"
                                value={formData.AccuserPhone}
                                onChange={handleInputChange}
                                placeholder="Enter accuser phone"
                                className="dmca-modal__input"
                            />
                        </div>
                    </div>

                    {/* Target Section */}
                    <div className="dmca-modal__section">
                        <h4 className="dmca-modal__section-title">
                            {type === 'show' ? 'Show' : 'Episode'} Information
                        </h4>

                        <div className="dmca-modal__picker">
                            <div className="dmca-modal__search">
                                <CFormLabel className="dmca-modal__label m-0">Search</CFormLabel>
                                <CFormInput
                                    type="text"
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                    placeholder={`Search ${type} by name or ID`}
                                    className="dmca-modal__input"
                                />
                            </div>

                            {isLoading ? (
                                <div>Loading {type} list...</div>
                            ) : (
                                <div className="dmca-modal__list-wrap">
                                    <table className="dmca-modal__table">
                                        <thead>
                                            <tr>
                                                <th></th>
                                                <th></th>
                                                <th>Name</th>
                                                <th>Released</th>
                                                <th>Release Date</th>
                                                <th>Id</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {filteredData.length === 0 && (
                                                <tr>
                                                    <td colSpan={6} style={{ padding: '0.9rem', color: '#6c757d' }}>No results</td>
                                                </tr>
                                            )}
                                            {filteredData.map((item: any) => {
                                                const isReleased = !!item?.IsReleased;
                                                const releaseDate = item?.ReleaseDate ? formatDate(item.ReleaseDate) : '-';
                                                return (
                                                    <tr key={item.Id}>
                                                        <td>
                                                            <button
                                                                type="button"
                                                                className="dmca-modal__select-btn"
                                                                onClick={() => setFormData(prev => ({
                                                                    ...prev,
                                                                    ShowId: type === 'show' ? item.Id : prev.ShowId,
                                                                    EpisodeId: type === 'episode' ? item.Id : prev.EpisodeId,
                                                                }))}
                                                            >
                                                                Select
                                                            </button>
                                                        </td>
                                                        <td className='min-w-[100px]'>  
                                                             <Image
                                                            mainImageFileKey={item.MainImageFileKey}
                                                            alt={item.Name}
                                                            className="w-[40px] h-[40px] object-cover flex-shrink-0"
                                                        /></td>
                                                        <td>{item.Name || '-'}</td>
                                                        <td>
                                                            <span className={`dmca-modal__status ${isReleased ? 'dmca-modal__status--released' : 'dmca-modal__status--unreleased'}`}>
                                                                {isReleased ? 'Released' : 'Unreleased'}
                                                            </span>
                                                        </td>
                                                        <td>{releaseDate}</td>
                                                        <td><span className="dmca-modal__id" title={item.Id}>{item.Id}</span></td>

                                                    </tr>
                                                )
                                            })}
                                        </tbody>
                                    </table>
                                </div>
                            )}

                            <div className="dmca-modal__selected">
                                Selected {type}: {' '}
                                <span>
                                    {(() => {
                                        const selectedId = type === 'show' ? formData.ShowId : formData.EpisodeId;
                                        const found = data.find((x: any) => x?.Id === selectedId);
                                        return found ? `${found.Name}` : '---';
                                    })()}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Attachments Section */}
                    <div className="dmca-modal__section">
                        <h4 className="dmca-modal__section-title">Attachments</h4>

                        <div className="dmca-modal__field">
                            <label className="dmca-modal__upload-btn">
                                <input
                                    type="file"
                                    multiple
                                    onChange={handleFileChange}
                                    className="d-none"
                                    ref={fileInputRef}
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
                        {isSubmitting ? 'Creating...' : 'Create DMCA Accusation'}
                    </CButton>
                </div>
            </CForm>
        </div>
    )
}

export default DmcaModal