import React, { useContext, useEffect, useState } from 'react';
import {
    Button,
    Typography,
    Checkbox,
    FormControlLabel,
    Box,
    Card,
    CardContent,
    Divider
} from '@mui/material';
import { ChannelOverviewPageContext } from '.';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { useNavigate, useParams } from 'react-router-dom';
import { confirmAlert } from '@/core/utils/alert.util';
import { deleteChannel } from '@/core/services/channel/channel.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { toast } from 'react-toastify';
import Image from '@/views/components/common/image';
interface Props {
    showList: any[] | [];
}
const DeletionOptionModal: React.FC<Props> = ({ showList }) => {
    const {id} = useParams<{id: string}>();
    const [isDeleting, setIsDeleting] = useState(false);
    const [selectedShowIds, setSelectedShowIds] = useState<string[]>([]);
    const navigate = useNavigate();
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 30,
        intervalSeconds: 1,
    })

    // const showList = context?.channelDetail?.ShowList || [];
    const isAllSelected = selectedShowIds.length === showList.length && showList.length > 0;
    const isIndeterminate = selectedShowIds.length > 0 && selectedShowIds.length < showList.length;

    const handleSelectAll = () => {
        if (isAllSelected) {
            setSelectedShowIds([]);
        } else {
            setSelectedShowIds(showList.map(show => show.Id));
        }
    };

    const handleSelectShow = (showId: string) => {
        setSelectedShowIds(prev => {
            if (prev.includes(showId)) {
                return prev.filter(id => id !== showId);
            } else {
                return [...prev, showId];
            }
        });
    };

    const handleRemove = async () => {
        const alert = await confirmAlert("Are you sure you want to delete this channel?");
        if (!alert.isConfirmed) return;

        try {
            const payload = {
                ChannelDeletionOptions: {
                    KeptShowIds: selectedShowIds
                }
            }
            console.log('Deletion payload:', payload);
            setIsDeleting(true);
            const res = await deleteChannel(loginRequiredAxiosInstance, id, payload);
            const sagaId = res?.data?.SagaInstanceId
            if (!res.success && res.message.content) {
                toast.error(res.message.content)
                return
            }
            if (!sagaId) {
                toast.error(`Channel Deleted failed, please try again.`)
                return
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    toast.success(`Channel Deleted successfully.`)
                    setSelectedShowIds([]);
                    await new Promise((r) => setTimeout(r, 100));
                    navigate('/channel');
                    navigate(0);
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error deleting channel");
        } finally {
            setIsDeleting(false);
        }
    };



    return (
        <Box className="deletion-option-modal__content">
            <Box className="deletion-option-modal__header">
                <Typography variant="body1" className="deletion-option-modal__description ">
                    ⚠️ When you delete this channel, you can choose which shows to keep as Single Show.
                    Selected shows will become independent and won't be deleted.
                </Typography>
            </Box>


            <Divider className="deletion-option-modal__divider" />

            {showList.length > 0 ? (
                <>
                    <Box className="deletion-option-modal__select-all">
                        <FormControlLabel
                            control={
                                <Checkbox
                                    checked={isAllSelected}
                                    indeterminate={isIndeterminate}
                                    onChange={handleSelectAll}
                                    className="deletion-option-modal__checkbox"
                                />
                            }
                            label={`Select All Shows (${selectedShowIds.length}/${showList.length} selected)`}
                            className="deletion-option-modal__select-all-label"
                        />
                    </Box>

                    <Box className="deletion-option-modal__show-list">
                        {showList.map((show) => (
                            <Card
                                key={show.Id}
                                className={`deletion-option-modal__show-card ${selectedShowIds.includes(show.Id) ? 'deletion-option-modal__show-card--selected' : ''
                                    }`}
                                onClick={() => handleSelectShow(show.Id)}
                            >
                                <Box className="deletion-option-modal__show-card-content">
                                    <Checkbox
                                        checked={selectedShowIds.includes(show.Id)}
                                        className="deletion-option-modal__show-checkbox"
                                    />

                                    <Box className="deletion-option-modal__show-image">
                                        <Image
                                            mainImageFileKey={show.MainImageFileKey}
                                            alt={show.Name}
                                            className="deletion-option-modal__show-image-content"
                                        />
                                    </Box>

                                    <CardContent className="deletion-option-modal__show-info">
                                        <Typography variant="h6" className="deletion-option-modal__show-name">
                                            {show.Name}
                                        </Typography>
                                        <Typography variant="body2" className="deletion-option-modal__show-description">
                                            {show.Description}
                                        </Typography>
                                        <Box className="deletion-option-modal__show-stats">
                                            <Typography variant="caption" className="deletion-option-modal__show-stat">
                                                Episodes: {show.EpisodeCount}
                                            </Typography>
                                            <Typography variant="caption" className="deletion-option-modal__show-stat">
                                                Listens: {show.ListenCount?.toLocaleString() || 0}
                                            </Typography>
                                        </Box>
                                    </CardContent>
                                </Box>
                            </Card>
                        ))}
                    </Box>
                </>
            ) : (
                <Typography variant="body2" className="deletion-option-modal__no-shows">
                    This channel has no shows.
                </Typography>
            )}

            <Divider className="deletion-option-modal__divider" />

            <Box className="deletion-option-modal__actions">
                <Button
                    onClick={handleRemove}
                    variant="contained"
                    color="error"
                    className="deletion-option-modal__button deletion-option-modal__button--delete"
                    disabled={isDeleting}
                    fullWidth
                >
                    {isDeleting ? 'Deleting...' : `Delete Channel & Keep ${selectedShowIds.length} ${selectedShowIds.length === 1 ? 'Show' : 'Shows'}`}
                </Button>
            </Box>

        </Box>
    );
};

export default DeletionOptionModal;