import { FC, useState, useEffect, useContext } from 'react'
import { CButton, CForm, CFormInput, CFormLabel, CFormTextarea } from '@coreui/react'
import { toast } from 'react-toastify'
import { CloudArrowUp, Image, MusicNote } from 'phosphor-react'
import './modal-styles.scss'
import { BackgroundSoundViewContext } from '.'

interface BgSoundModalProps {
    soundData?: any // For update mode
    onClose: () => void
}

const BgSoundModal: FC<BgSoundModalProps> = ({ soundData, onClose }) => {
    const context = useContext(BackgroundSoundViewContext);

    const [formData, setFormData] = useState({
        Name: '',
        Description: ''
    })
    const [mainImageFile, setMainImageFile] = useState<File | null>(null)
    const [audioFile, setAudioFile] = useState<File | null>(null)
    const [mainImagePreview, setMainImagePreview] = useState<string>('')
    const [isSubmitting, setIsSubmitting] = useState(false)

    const isUpdateMode = !!soundData

    useEffect(() => {
        if (soundData) {
            setFormData({
                Name: soundData.Name || '',
                Description: soundData.Description || ''
            })
            setMainImagePreview(soundData.MainImageFileKey || '')
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
        const file = e.target.files?.[0]
        if (file) {
            if (!file.type.startsWith('image/')) {
                toast.error('Please select a valid image file')
                return
            }
            if (file.size > 5 * 1024 * 1024) { // 5MB limit
                toast.error('Image file size must be less than 5MB')
                return
            }
            setMainImageFile(file)
            setMainImagePreview(URL.createObjectURL(file))
        }
    }

    const handleAudioChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0]
        if (file) {
            if (!file.type.startsWith('audio/')) {
                toast.error('Please select a valid audio file')
                return
            }
            if (file.size > 50 * 1024 * 1024) { // 50MB limit
                toast.error('Audio file size must be less than 50MB')
                return
            }
            setAudioFile(file)
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

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        
        if (!validateForm()) return

        setIsSubmitting(true)
        try {
            const formDataToSend = new FormData()
            
            // Add text fields
            formDataToSend.append('BackgroundSoundTrackUpdateInfo', JSON.stringify({
                Name: formData.Name,
                Description: formData.Description
            }))

            // Add files if selected
            if (mainImageFile) {
                formDataToSend.append('MainImageFile', mainImageFile)
            }
            if (audioFile) {
                formDataToSend.append('AudioFile', audioFile)
            }

            // TODO: Call API
            if (isUpdateMode) {
                // await updateBackgroundSound(soundData.Id, formDataToSend)
                console.log('Update:', soundData.Id, formDataToSend)
                  context?.handleDataChange();
                toast.success('Background sound updated successfully')
            } else {
                // await createBackgroundSound(formDataToSend)
                console.log('Create:', formDataToSend)
                context?.handleDataChange();
                toast.success('Background sound created successfully')
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
                            {mainImagePreview && (
                                <div className="bg-sound-modal__image-preview">
                                    <img 
                                        src={mainImagePreview} 
                                        alt="Preview" 
                                        className="bg-sound-modal__preview-img"
                                    />
                                </div>
                            )}
                            <label className="bg-sound-modal__upload-btn">
                                <input
                                    type="file"
                                    accept="image/*"
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
                    </div>
                </div>

                {/* Actions */}
                <div className="bg-sound-modal__actions">
                   
                    <CButton
                        type="submit"
                        color="success"
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
