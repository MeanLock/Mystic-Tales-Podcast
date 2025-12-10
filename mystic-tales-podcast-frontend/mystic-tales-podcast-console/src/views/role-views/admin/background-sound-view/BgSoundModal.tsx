import { FC, useState, useEffect, useContext } from 'react'
import { CButton, CForm, CFormInput, CFormLabel, CFormTextarea } from '@coreui/react'
import { toast } from 'react-toastify'
import { CloudArrowUp, Image, MusicNote } from 'phosphor-react'
import ImageComponent from '@/views/components/common/image'
import './modal-styles.scss'
import { BackgroundSoundViewContext } from '.'
import { addBackgroundSoundTracks, getAudioBackgroundSound, updateBackgroundSoundTracks } from '@/core/services/background/background.service'
import { adminAxiosInstance } from '@/core/api/rest-api/config/instances/v2'
import { useSagaPolling } from '@/hooks/useSagaPolling'

interface BgSoundModalProps {
    soundData?: any // For update mode
    onClose: () => void
}

const BgSoundModal: FC<BgSoundModalProps> = ({ soundData, onClose }) => {
    const context = useContext(BackgroundSoundViewContext);
    const [audioPreviewUrl, setAudioPreviewUrl] = useState<string | null>(null)
    const [isLoadingAudioPreview, setIsLoadingAudioPreview] = useState(false)
    const [formData, setFormData] = useState({
        Name: '',
        Description: ''
    })
    const [mainImageFile, setMainImageFile] = useState<File | null>(null)
    const [audioFile, setAudioFile] = useState<File | null>(null)
    const [mainImagePreview, setMainImagePreview] = useState<string | null>(null)
    const [mainImageUrl, setMainImageUrl] = useState<string | null>(null)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })
    const isUpdateMode = !!soundData

    useEffect(() => {
        if (soundData) {
            setFormData({
                Name: soundData.Name || '',
                Description: soundData.Description || ''
            })
            setMainImageUrl(soundData.MainImageFileKey || null)
        }
    }, [soundData])

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target
        setFormData(prev => ({
            ...prev,
            [name]: value
        }))
    }

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) {
            setMainImageFile(null);
            return;
        }

        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/svg+xml'];
        if (!allowedTypes.includes(file.type)) {
            toast.error('Invalid file type. Allowed: JPG, JPEG, PNG, GIF, WEBP, SVG');
            return;
        }

        const maxSize = 3 * 1024 * 1024; // 3MB in bytes
        if (file.size > maxSize) {
            toast.error('Image file size must be less than 3MB');
            return;
        }

        setMainImageFile(file);
    };
    useEffect(() => {
        if (!mainImageFile) {
            setMainImagePreview(null)
            return
        }
        const url = URL.createObjectURL(mainImageFile)
        setMainImagePreview(url)
        return () => {
            URL.revokeObjectURL(url)
        }
    }, [mainImageFile])

   const handleAudioChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
        const allowedExtensions = ['wav', 'flac', 'mp3', 'm4a', 'aac'];
        const ext = file.name.split('.').pop()?.toLowerCase();
        if (!ext || !allowedExtensions.includes(ext)) {
            toast.error('Allowed audio types: wav, flac, mp3, m4a, aac');
            return;
        }
        if (file.size > 150 * 1024 * 1024) {
            toast.error('Audio file size must be less than 150MB');
            return;
        }
        setAudioFile(file);
    }
}

    const validateForm = () => {
        if (!formData.Name.trim()) {
            toast.error('Please enter sound name')
            return false
        }
        if (!formData.Description.trim()) {
            toast.error('Please enter description')
            return false
        }
        if (!isUpdateMode && !mainImageFile) {
            toast.error('Please select a main image')
            return false
        }
        if (!isUpdateMode && !audioFile) {
            toast.error('Please select an audio file')
            return false
        }
        return true
    }
    useEffect(() => {
        if (!audioFile) {
            setAudioPreviewUrl(null)
            return
        }
        const url = URL.createObjectURL(audioFile)
        setAudioPreviewUrl(url)
        return () => URL.revokeObjectURL(url)
    }, [audioFile])

    const handlePreviewExistingAudio = async () => {
        if (!soundData?.AudioFileKey) {
            toast.warn('No existing audio to preview')
            return
        }
        setIsLoadingAudioPreview(true)
        try {
            const res = await getAudioBackgroundSound(adminAxiosInstance, soundData.AudioFileKey)
            if (res?.success && res.data?.FileUrl) {
                setAudioPreviewUrl(res.data.FileUrl) // URL presigned (hết hạn nhanh), bấm lại để lấy link mới
            } else {
                toast.error('Cannot get audio preview URL')
            }
        } catch (e) {
            toast.error('Failed to load audio preview')
        } finally {
            setIsLoadingAudioPreview(false)
        }
    }
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()

        if (!validateForm()) return

        setIsSubmitting(true)
        try {
            let payload;
            if (isUpdateMode) {
                payload = {
                    BackgroundSoundTrackUpdateInfo: {
                        Name: formData.Name,
                        Description: formData.Description
                    },
                    MainImageFile: mainImageFile || null,
                    AudioFile: audioFile || null
                };
            } else {
                payload = {
                    BackgroundSoundTrackCreateInfo: {
                        Name: formData.Name,
                        Description: formData.Description
                    },
                    MainImageFile: mainImageFile || null,
                    AudioFile: audioFile || null
                };
            }
            if (isUpdateMode) {
                try {
                    const response = await updateBackgroundSoundTracks(adminAxiosInstance, soundData.Id, payload)
                    const sagaId = response?.data?.SagaInstanceId
                    if (!sagaId) {
                        toast.error("Update background sound failed, please try again.")
                        return
                    }
                    await startPolling(sagaId, adminAxiosInstance, {
                        onSuccess: () => {
                            context?.handleDataChange();
                            toast.success('Background sound updated successfully')
                        },
                        onFailure: (err: any) => toast.error(err || "Saga failed!"),
                        onTimeout: () => toast.error("System not responding, please try again."),
                    })
                } catch (error) {
                    console.error('Error updating background sound:', error)
                    toast.error('Failed to update background sound')
                }
            } else {
                try {
                    const response = await addBackgroundSoundTracks(adminAxiosInstance, payload)
                    const sagaId = response?.data?.SagaInstanceId
                    if (!sagaId) {
                        toast.error("Create background sound failed, please try again.")
                        return
                    }
                    await startPolling(sagaId, adminAxiosInstance, {
                        onSuccess: () => {
                            context?.handleDataChange();
                            toast.success('Background sound created successfully')
                        },
                        onFailure: (err: any) => toast.error(err || "Saga failed!"),
                        onTimeout: () => toast.error("System not responding, please try again."),
                    })
                } catch (error) {
                    console.error('Error creating background sound:', error)
                    toast.error('Failed to create background sound')
                }
            }
            onClose()
        } catch (error) {
            console.error('Error submitting form:', error)
            toast.error('Failed to save background sound')
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <div className="bg-sound-modal">
            <CForm onSubmit={handleSubmit} className="bg-sound-modal__form">
                <div className="bg-sound-modal__content">
                    {/* Name Field */}
                    <div className="bg-sound-modal__field">
                        <CFormLabel className="bg-sound-modal__label">
                            Name <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormInput
                            type="text"
                            name="Name"
                            value={formData.Name}
                            onChange={handleInputChange}
                            placeholder="Enter sound name"
                            className="bg-sound-modal__input"
                        />
                    </div>

                    {/* Description Field */}
                    <div className="bg-sound-modal__field">
                        <CFormLabel className="bg-sound-modal__label">
                            Description <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormTextarea
                            name="Description"
                            value={formData.Description}
                            onChange={handleInputChange}
                            placeholder="Enter description"
                            rows={4}
                            className="bg-sound-modal__textarea"
                        />
                    </div>

                    {/* Main Image Upload */}
                    <div className="bg-sound-modal__field">
                        <CFormLabel className="bg-sound-modal__label">
                            Main Image {!isUpdateMode && <span className="text-danger">*</span>}
                        </CFormLabel>
                        <div className="bg-sound-modal__upload-container">
                            {mainImagePreview ? (
                                <img
                                    src={mainImagePreview}
                                    alt="Avatar preview"
                                    className="bg-sound-modal__image-preview"
                                />
                            ) : (
                                <ImageComponent mainImageFileKey={mainImageUrl} className="bg-sound-modal__image-preview" />
                            )}
                            <label className="bg-sound-modal__upload-btn">
                                <input
                                    type="file"
                                    accept=".jpg,.jpeg,.png,.gif,.webp,.svg"
                                    onChange={handleImageChange}
                                    className="d-none"
                                />
                                <Image size={24} className="me-2" />
                                {mainImageFile ? mainImageFile.name : 'Choose Image'}
                            </label>
                        </div>
                    </div>

                    {/* Audio File Upload */}
                    <div className="bg-sound-modal__field">
                        <CFormLabel className="bg-sound-modal__label">
                            Audio File {!isUpdateMode && <span className="text-danger">*</span>}
                        </CFormLabel>
                        <label className="bg-sound-modal__upload-btn">
                            <input
                                type="file"
                                accept="audio/*"
                                onChange={handleAudioChange}
                                className="d-none"
                            />
                            <MusicNote size={24} className="me-2" />
                            {audioFile ? audioFile.name : 'Choose Audio File'}
                        </label>
                        {audioPreviewUrl ? (
                            <audio
                                className="bg-sound-modal__audio-preview"
                                src={audioPreviewUrl}
                                controls
                                autoPlay
                                onError={() => toast.error('Audio preview failed to play (URL may have expired)')}
                            />
                        ) : (
                            isUpdateMode && soundData?.AudioFileKey && (
                                <div className="bg-sound-modal__audio-actions">
                                    <CButton
                                        color="secondary"
                                        variant="outline"
                                        size="sm"
                                        disabled={isLoadingAudioPreview}
                                        onClick={handlePreviewExistingAudio}
                                    >
                                        {isLoadingAudioPreview ? 'Loading...' : 'Preview current audio'}
                                    </CButton>
                                </div>
                            )
                        )}
                    </div>
                </div>

                {/* Actions */}
                <div className="bg-sound-modal__actions">
                    <CButton
                        type="submit"
                        color="success"
                        onClick={handleSubmit}
                        disabled={isSubmitting}
                        className="bg-sound-modal__btn bg-sound-modal__btn--submit"
                    >
                        <CloudArrowUp size={20} className="me-2" />
                        {isSubmitting ? 'Saving...' : (isUpdateMode ? 'Update' : 'Create')}
                    </CButton>
                </div>
            </CForm>
        </div>
    )
}

export default BgSoundModal
