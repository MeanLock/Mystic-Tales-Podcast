import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { assignChannel, getShowAssignableChannels } from '@/core/services/show/show.service';
import { confirmAlert } from '@/core/utils/alert.util';
import Image from '@/views/components/common/image';
import { useContext, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { ShowInfoViewContext } from './show-info';
import { renderDescriptionHTML } from '@/core/utils/htmlRender.utils';
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';


interface ShowAssignableChannel {
    Id: string;
    Name: string;
    Description: string;
    MainImageFileKey: string;
}
const AssignChannelModal = ({ onclose }: { onclose: () => void }) => {
    const { id } = useParams<{ id: string }>();
    const authSlice = useSelector((state: RootState) => state.auth);
    const context = useContext(ShowInfoViewContext);
    const [channels, setChannels] = useState<ShowAssignableChannel[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })

    const fetchShowAssignableChannels = async () => {
        setIsLoading(true);
        try {
            const res = await getShowAssignableChannels(loginRequiredAxiosInstance, id);
            console.log("Fetched show assignable channels:", res.data.ShowAssignableChannelList);
            if (res.success && res.data) {
                setChannels(res.data.ShowAssignableChannelList);
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch show detail:', error);
        } finally {
            setIsLoading(false);
        }
    }
    const assignShowToChannel = async (channelId: string) => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        const alert = await confirmAlert("Are you sure?");
        if (!alert.isConfirmed) return;
        setIsLoading(true);
        try {
            const payload = { PodcastChannelId: channelId };
            const res = await assignChannel(loginRequiredAxiosInstance, id, payload);
            const sagaId = res?.data?.SagaInstanceId
            if (!res.success && res.message.content) {
                toast.error(res.message.content)
                return
            }
            if (!sagaId) {
                toast.error(`Show assignment failed, please try again.`)
                return
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: () => {
                    onclose();
                    context?.handleDataChange();
                    toast.success(`Show assigned successfully.`)

                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error assigning show");
        } finally {
            setIsLoading(false);
        }
    }
    useEffect(() => {
        fetchShowAssignableChannels();
    }, [id]);

    return (
        <div className="assign-channel-modal">
            <div className="assign-channel-modal__header">
                <h2 className="assign-channel-modal__title">Assign Show to Channel</h2>
                <p className="assign-channel-modal__subtitle">
                    Select a channel to assign this show, or keep it as a standalone show
                </p>
            </div>

            {isLoading && !channels ? (
                <div className="assign-channel-modal__loading">
                    <div className="spinner"></div>
                    <p>Loading channels...</p>
                </div>
            ) : (
                <div className="assign-channel-modal__content">


                    {/* Channel List */}
                    {channels.length === 0 ? (
                        <div className="assign-channel-modal__empty">
                            <p>No available channels to assign</p>
                        </div>
                    ) : (
                        <div className="assign-channel-modal__list">
                            <div className="assign-channel-modal__list-header">
                                <h3>Available Channels ({channels.length})</h3>
                            </div>
                            {context.channel !== null && (
                                <div
                                    className="assign-channel-modal__option assign-channel-modal__option"
                                    onClick={() => assignShowToChannel('')}
                                >
                                    <div className="assign-channel-modal__option-icon">
                                        <svg width="48" height="48" viewBox="0 0 24 24" fill="none">
                                            <path
                                                d="M12 2L2 7L12 12L22 7L12 2Z"
                                                stroke="currentColor"
                                                strokeWidth="2"
                                                strokeLinecap="round"
                                                strokeLinejoin="round"
                                            />
                                            <path
                                                d="M2 17L12 22L22 17"
                                                stroke="currentColor"
                                                strokeWidth="2"
                                                strokeLinecap="round"
                                                strokeLinejoin="round"
                                            />
                                            <path
                                                d="M2 12L12 17L22 12"
                                                stroke="currentColor"
                                                strokeWidth="2"
                                                strokeLinecap="round"
                                                strokeLinejoin="round"
                                            />
                                        </svg>
                                    </div>
                                    <div className="assign-channel-modal__option-content">
                                        <h3 className="assign-channel-modal__option-title">Single Show</h3>
                                        <p className="assign-channel-modal__option-description">
                                            Keep this show independent without a channel
                                        </p>
                                    </div>
                                    <div className="assign-channel-modal__option-arrow">→</div>
                                </div>
                            )}


                            {channels.map((channel) => (
                                <div
                                    key={channel.Id}
                                    className="assign-channel-modal__option"
                                    onClick={() => assignShowToChannel(channel.Id)}
                                >
                                    <div className="assign-channel-modal__option-image">
                                        {channel.MainImageFileKey ? (
                                            <img
                                                src={channel.MainImageFileKey}
                                                alt={channel.Name}
                                                onError={(e) => {
                                                    e.currentTarget.style.display = 'none';
                                                    e.currentTarget.nextElementSibling?.classList.remove('hidden');
                                                }}
                                            />
                                        ) : null}
                                        <div className={channel.MainImageFileKey ? 'hidden' : ''}>
                                            <Image
                                                mainImageFileKey={channel.MainImageFileKey}
                                                alt={channel.Name}
                                                size={48}
                                            />
                                        </div>
                                    </div>
                                    <div className="assign-channel-modal__option-content">
                                        <h3 className="assign-channel-modal__option-title">{channel.Name}</h3>
                                        <p className="assign-channel-modal__option-description">
                                            {renderDescriptionHTML(channel.Description) || 'No description available'}
                                        </p>
                                    </div>
                                    <div className="assign-channel-modal__option-arrow">→</div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

export default AssignChannelModal;