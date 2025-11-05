import { createContext, FC, useEffect, useMemo, useState } from 'react'
import { CButton } from '@coreui/react'
import { Pencil, Trash, Plus, Play, Pause } from 'phosphor-react'
import { toast } from 'react-toastify'
import Modal_Button from '@/views/components/common/modal/ModalButton'
import BgSoundModal from './BgSoundModal'
import './styles.scss'


export const mockdata = {
    BackgroundSoundTrackList: [
        {
            Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            Name: "Mystic Forest Whisper",
            Description: "Âm thanh nền nhẹ nhàng, mô phỏng tiếng gió và chim trong rừng huyền ảo.",
            MainImageFileKey: "https://picsum.photos/200/200?1",
            AudioFileKey: "/assets/audio/mystic_forest.mp3",
            CreatedAt: "2025-11-04T08:01:36.412Z",
            UpdatedAt: "2025-11-04T08:01:36.412Z"
        },
        {
            Id: "7b21c3d4-4c2f-4a69-94c0-2fdc92b2e223",
            Name: "Temple Bells Echo",
            Description: "Tiếng chuông vang vọng trong đền cổ, mang lại cảm giác tĩnh lặng và thiền định.",
            MainImageFileKey: "https://picsum.photos/200/200?2",
            AudioFileKey: "/assets/audio/temple_bells.mp3",
            CreatedAt: "2025-11-04T08:05:20.001Z",
            UpdatedAt: "2025-11-04T08:05:20.001Z"
        },
        {
            Id: "a14b8c9f-65c3-4e3e-9f1a-7bb19de37c7f",
            Name: "Night Sky Ambience",
            Description: "Âm thanh của đêm yên tĩnh, kết hợp tiếng dế, gió và cảm giác không gian bao la.",
            MainImageFileKey: "https://picsum.photos/200/200?3",
            AudioFileKey: "/assets/audio/night_sky.mp3",
            CreatedAt: "2025-11-04T08:10:00.312Z",
            UpdatedAt: "2025-11-04T08:10:00.312Z"
        },
        {
            Id: "4f94f274-c7e5-4b52-8f7d-5dc7f9f5b2a1",
            Name: "Mystic Waterfall",
            Description: "Tiếng nước chảy róc rách và hơi sương tỏa mờ, tạo cảm giác thư giãn sâu.",
            MainImageFileKey: "https://picsum.photos/200/200?4",
            AudioFileKey: "/assets/audio/mystic_waterfall.mp3",
            CreatedAt: "2025-11-04T08:15:12.897Z",
            UpdatedAt: "2025-11-04T08:15:12.897Z"
        }
    ]
};


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

    const handleDataChange = async () => {
        setIsLoading(true);
        try {
            // Simulate API call
            setBackgroundList(mockdata.BackgroundSoundTrackList);
        } catch (error) {
            console.error('Error fetching background sounds:', error);
            toast.error('Failed to load background sounds');
        } finally {
            setIsLoading(false);
        }
    }

    const handleAdd = () => {
        toast.info('Add new background sound');
        // TODO: Open modal to add new background sound
    }

    const handleUpdate = (id: string) => {
        toast.info(`Update background sound ${id}`);
        // TODO: Open modal to update background sound
    }

    const handleDelete = async (id: string) => {
        if (window.confirm('Are you sure you want to delete this background sound?')) {
            try {
                // TODO: Call API to delete
                setBackgroundList(prev => prev.filter(item => item.Id !== id));
                toast.success('Background sound deleted successfully');
            } catch (error) {
                toast.error('Failed to delete background sound');
            }
        }
    }

    const handlePlayPause = async (sound: any) => {
        if (playingId === sound.Id) {
            // Pause current audio
            audioElement?.pause();
            setPlayingId(null);
        } else {
            // Stop previous audio if any
            if (audioElement) {
                audioElement.pause();
            }

            try {
                // TODO: Call API to get audio file
                // const audioUrl = await fetchAudioFile(sound.AudioFileKey);
                const audioUrl = sound.AudioFileKey; // Mock for now

                const audio = new Audio(audioUrl);
                audio.addEventListener('ended', () => {
                    setPlayingId(null);
                });
                audio.addEventListener('error', () => {
                    toast.error('Failed to load audio');
                    setPlayingId(null);
                });

                audio.play();
                setAudioElement(audio);
                setPlayingId(sound.Id);
            } catch (error) {
                toast.error('Failed to play audio');
            }
        }
    }

    useEffect(() => {
        handleDataChange();

        return () => {
            // Cleanup audio on unmount
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
                    <div className="background-sound-view__list">
                        {backgroundList.length === 0 ? (
                            <div className="background-sound-view__empty">
                                <p>No background sounds found</p>
                            </div>
                        ) : (
                            backgroundList.map((sound) => (
                                <div key={sound.Id} className="sound-card">
                                    <div className="sound-card__image-container">
                                        <img
                                            src={sound.MainImageFileKey || "/placeholder.svg"}
                                            alt={sound.Name}
                                            className="sound-card__image"
                                        />
                                        <button
                                            className="sound-card__play-btn"
                                            onClick={() => handlePlayPause(sound)}
                                        >
                                            {playingId === sound.Id ? (
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
                )}
            </div>
        </BackgroundSoundViewContext.Provider>
    )
}

export default BackgroundSoundView;