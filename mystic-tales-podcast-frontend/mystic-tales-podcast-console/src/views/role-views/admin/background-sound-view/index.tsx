import { createContext, FC, useEffect, useRef, useState } from 'react'
import { CButton } from '@coreui/react'
import { Pencil, Trash, Plus, Play, Pause } from 'phosphor-react'
import { toast } from 'react-toastify'
import Modal_Button from '@/views/components/common/modal/ModalButton'
import BgSoundModal from './BgSoundModal'
import './styles.scss'
import { adminAxiosInstance } from '@/core/api/rest-api/config/instances/v1/admin-instance'
import { deleteBackgroundSoundTracks, getAudioBackgroundSound, getBackgroundSoundTracks } from '@/core/services/background/background.service'
import Image from '@/views/components/common/image'
import { set } from 'lodash'
import { confirmAlert } from '@/core/utils/alert.util'
import { useSagaPolling } from '@/hooks/useSagaPolling'




interface BackgroundSoundViewProps { }
interface BackgroundSoundViewContextProps {
    handleDataChange: () => void;
}


export const BackgroundSoundViewContext = createContext<BackgroundSoundViewContextProps | null>(null);

const BackgroundSoundView: FC<BackgroundSoundViewProps> = () => {
    const [backgroundList, setBackgroundList] = useState<any[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [playingId, setPlayingId] = useState<string | null>(null);
    const [audioElement, setAudioElement] = useState<HTMLAudioElement | null>(null);
    const [audioUrl, setAudioUrl] = useState<string | null>(null)
    const [isFetchingAudioId, setIsFetchingAudioId] = useState<string | null>(null)
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })
    const handleDataChange = async () => {
        setIsLoading(true);
        try {
            const backgroundList = await getBackgroundSoundTracks(adminAxiosInstance);
            if (backgroundList.success) {
                setBackgroundList(backgroundList.data.BackgroundSoundTrackList);
            } else {
                console.error('API Error:', backgroundList.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch background sound tracks:', error);
        } finally {
            setIsLoading(false);
        }
    }

    const handleDelete = async (id: string) => {
        const alert = await confirmAlert("Are you sure to delete?");
        if (!alert.isConfirmed) return;
        try {
            const response = await deleteBackgroundSoundTracks(adminAxiosInstance, id)
            const sagaId = response?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Delete background sound failed, please try again.")
                return
            }
            await startPolling(sagaId, adminAxiosInstance, {
                onSuccess: () => {
                    handleDataChange();
                    toast.success('Background sound deleted successfully')
                },
                onFailure: (err: any) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            console.error('Error creating background sound:', error)
            toast.error('Failed to create background sound')
        }
    }

    const handlePlayPause = async (sound: any) => {
        if (playingId === sound.Id) {
            setAudioUrl(null)
            setPlayingId(null)
            return
        }
        setIsFetchingAudioId(sound.Id)
        try {
            const res = await getAudioBackgroundSound(adminAxiosInstance, sound.AudioFileKey)
            if (res.success && res.data?.FileUrl) {
                setAudioUrl(res.data.FileUrl)
                setPlayingId(sound.Id)
            } else {
                setAudioUrl(null)
                setPlayingId(null)
            }
        } finally {
            setIsFetchingAudioId(null)
        }
    }

    const handleAudioEnded = () => {
        setAudioUrl(null)
        setPlayingId(null)
    }

    useEffect(() => {
        handleDataChange();

        return () => {
            if (audioElement) {
                audioElement.pause();
                audioElement.src = '';
            }
        };
    }, [])


    return (
        <BackgroundSoundViewContext.Provider value={{ handleDataChange: handleDataChange }}>
            <div className="background-sound-view">
                <div className="background-sound-view__header">
                    <h2 className="background-sound-view__title">Background Sound Management</h2>
                    <Modal_Button
                        color="success"
                        className="background-sound-view__add-btn"
                        size="lg"
                        title="Add New Background Sound"
                        content={
                            <>
                                <Plus size={20} weight="bold" className="me-2" />
                                Add New Sound
                            </>
                        }
                    >
                        <BgSoundModal onClose={() => { }} />
                    </Modal_Button>
                </div>

                {isLoading ? (
                    <div className="background-sound-view__loading">
                        <div className="spinner-border text-primary" role="status">
                            <span className="visually-hidden">Loading...</span>
                        </div>
                    </div>
                ) : (
                    <div>
                        {audioUrl && (
                            <audio
                                src={audioUrl}
                                autoPlay
                                controls
                                style={{ display: 'none' }}
                                onEnded={handleAudioEnded}
                            />
                        )}
                        <div className="background-sound-view__list">
                            {backgroundList.length === 0 ? (
                                <div className="background-sound-view__empty">
                                    <p>No background sounds found</p>
                                </div>
                            ) : (
                                backgroundList.map((sound) => (
                                    <div key={sound.Id} className="sound-card">
                                        <div className="sound-card__image-container">
                                            <Image
                                                mainImageFileKey={sound.MainImageFileKey}
                                                alt={sound.Name}
                                                className="sound-card__image"
                                            />
                                            <button
                                                className="sound-card__play-btn"
                                                onClick={() => handlePlayPause(sound)}
                                                disabled={isFetchingAudioId === sound.Id}
                                            >
                                                {isFetchingAudioId === sound.Id ? (
                                                    <span className="spinner-border spinner-border-sm" />
                                                ) : playingId === sound.Id ? (
                                                    <Pause size={24} weight="fill" />
                                                ) : (
                                                    <Play size={24} weight="fill" />
                                                )}
                                            </button>
                                        </div>

                                        <div className="sound-card__content">
                                            <h3 className="sound-card__title">{sound.Name}</h3>
                                            <p className="sound-card__description">{sound.Description}</p>

                                            <div className="sound-card__meta">
                                                <span className="sound-card__date">
                                                    Created: {new Date(sound.CreatedAt).toLocaleDateString()}
                                                </span>
                                                {sound.UpdatedAt !== sound.CreatedAt && (
                                                    <span className="sound-card__date">
                                                        Updated: {new Date(sound.UpdatedAt).toLocaleDateString()}
                                                    </span>
                                                )}
                                            </div>

                                            <div className="sound-card__actions">
                                                <Modal_Button
                                                    className="sound-card__action-btn"
                                                    color="warning"
                                                    size="lg"
                                                    title={`Update Background Sound - ${sound.Name}`}
                                                    content={
                                                        <>
                                                            <Pencil size={16} className="me-1" />
                                                            Update
                                                        </>
                                                    }
                                                >
                                                    <BgSoundModal soundData={sound} onClose={() => { }} />
                                                </Modal_Button>
                                                <CButton

                                                    color="danger"
                                                    variant="outline"
                                                    size="sm"
                                                    className="sound-card__action-btn"
                                                    onClick={() => handleDelete(sound.Id)}
                                                >
                                                    <Trash size={16} className="me-1" />
                                                    Delete
                                                </CButton>
                                            </div>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>

                )}
            </div>
        </BackgroundSoundViewContext.Provider>
    )
}

export default BackgroundSoundView;