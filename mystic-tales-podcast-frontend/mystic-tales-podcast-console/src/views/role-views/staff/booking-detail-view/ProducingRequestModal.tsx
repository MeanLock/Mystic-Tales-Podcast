import React, { useEffect, useState, useCallback, useContext } from 'react';
import { Box, Typography, Button, IconButton, Chip, TextField } from '@mui/material';
import { CloudUpload, PlayArrow, Delete, Edit, CheckCircle, Cancel } from '@mui/icons-material';
import { formatDate } from '@/core/utils/date.util';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import Loading from '@/views/components/common/loading';
import { toast } from 'react-toastify';
import { SmartAudioPlayer } from '@/views/components/common/audio';
import Modal_Button from '@/views/components/common/modal/ModalButton';
import { BookingDetailViewContext } from '.';
import { useSagaPolling } from '@/hooks/useSagaPolling';
import { getProducingRequestAudio, getProducingRequestDetail } from '@/core/services/booking/producing.service';

interface ProducingRequestModalProps {
    bookingProducingRequestId: any;
    onClose: () => void;
}
interface AudioUploadItem {
    requirementId: string;
    file: File | null;
}


const ProducingRequestModal: React.FC<ProducingRequestModalProps> = ({ bookingProducingRequestId, onClose }) => {
    const context = useContext(BookingDetailViewContext);
    const [data, setData] = useState<any>(null);
    const [loading, setLoading] = useState(false);
    const [reuploadFiles, setReuploadFiles] = useState<Record<string, File | null>>({});
    // smart audio refresh handled by hook; no manual source cache needed
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [rejectReason, setRejectReason] = useState<string>('');



    const fetchProducingRequest = async () => {
        let alive = true;
        (async () => {
            setLoading(true);
            try {
                const response = await getProducingRequestDetail(loginRequiredAxiosInstance, (bookingProducingRequestId));
                if (!alive) return;
                if (response.success) {
                    setData(response.data.BookingProducingRequest);
                } else {
                    console.error('API Error:', response.message);
                }
            } catch (error) {
                if (alive) console.error('Error fetching booking detail:', error);
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    };

    const fetchTrackAudioUrl = useCallback(async (fileKey: string) => {
        try {
            const res: any = await getProducingRequestAudio(loginRequiredAxiosInstance, fileKey);
            if (res?.success && res?.data?.FileUrl) {
                return { success: true, data: { FileUrl: res.data.FileUrl } };
            }
            return { success: false, message: typeof res?.message === 'string' ? res.message : 'Unable to fetch audio URL' };
        } catch (e: any) {
            return { success: false, message: e?.message || 'Error fetching audio URL' };
        }
    }, []);

    const uploadedTracks: any[] = data?.FinishedAt
        ? (data?.BookingPodcastTrackList || data?.BookingPodcastTracks || [])
        : [];

    const reuploadTracks: any[] = !data?.FinishedAt && Array.isArray(data?.EditRequirementList)
        ? Array.from(
            new Map(
                data.EditRequirementList
                    .filter((req: any) => req.BookingPodcastTrack)
                    .map((req: any) => [req.BookingPodcastTrack.Id, req.BookingPodcastTrack])
            ).values()
        )
        : [];

    useEffect(() => {
        fetchProducingRequest();
    }, [bookingProducingRequestId]);

    // SmartAudio handles fetching URLs lazily; no prefetch useEffect required now.
    const hasEditRequirement = (trackId: string) => {
        return (data.EditRequirementList || []).some((req: any) => req?.BookingPodcastTrack?.Id === trackId);
    };

    const getEditRequirementNames = (trackId: string) => {
        return (data.EditRequirementList || [])
            .filter((req: any) => req?.BookingPodcastTrack?.Id === trackId)
            .map((req: any) => req.Name);
    };
    const handleFileChange = (requirementId: string, file: File | null) => {
        setReuploadFiles(prev => ({ ...prev, [requirementId]: file }));
    };
    const requiredRequirementIds: string[] = React.useMemo(() => {
        if (!data) return [];
        const ids = new Set<string>();

        if (data.FinishedAt) {
            (uploadedTracks || []).forEach((t: any) => {
                if (hasEditRequirement(t.Id) && t.BookingRequirementId) {
                    ids.add(t.BookingRequirementId);
                }
            });
        } else {
            // chưa finished: lấy từ EditRequirementList -> BookingPodcastTrack.BookingRequirementId
            (data.EditRequirementList || [])
                .filter((req: any) => req?.BookingPodcastTrack?.BookingRequirementId)
                .forEach((req: any) => ids.add(req.BookingPodcastTrack.BookingRequirementId));
        }
        return Array.from(ids);
    }, [data, uploadedTracks]);

    const isSubmitDisabled = React.useMemo(() => {
        if (requiredRequirementIds.length === 0) return true;
        return requiredRequirementIds.some(id => !reuploadFiles[id]);
    }, [requiredRequirementIds, reuploadFiles]);

    const formatDuration = (seconds: number) => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    };

    const formatFileSize = (bytes: number) => {
        return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
    };


    if (!data || loading) {
        return (
            <div className="flex justify-center items-center h-100">
                <Loading />
            </div>
        );
    }

    const labelSx = {
        color: "#000",
        fontSize: "0.75rem",
        fontWeight: 600,
        textTransform: "uppercase",
        letterSpacing: "0.5px",
        mb: 0.75,
    };

    const valueSx = {
        color: "#000",
        fontSize: "0.95rem",
        lineHeight: 1.5,
        fontWeight: 500,
    };

    return (
        <Box sx={{ color: "white", p: 3, display: "flex", flexDirection: "column", gap: 3, minWidth: 700, maxWidth: 900 }}>
            <Box>
                <Typography variant="h6" sx={{ color: "#000", fontWeight: 700, fontSize: "1.1rem", mb: 2 }}>
                </Typography>

                <Box sx={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 2, mb: 2 }}>
                    <Box>
                        <Typography sx={labelSx}>Status</Typography>
                        <Chip
                            label={data.IsAccepted ? "Accepted" : data.IsAccepted === false ? "Rejected" : "Pending"}
                            sx={{
                                background: data.IsAccepted ? "rgba(76, 175, 80, 0.15)" : data.IsAccepted === false ? "rgba(244, 67, 54, 0.15)" : "rgba(255, 152, 0, 0.15)",
                                color: data.IsAccepted ? "#4caf50" : data.IsAccepted === false ? "#ef5350" : "#ff9800",
                                border: `1px solid ${data.IsAccepted ? "#4caf50" : data.IsAccepted === false ? "#ef5350" : "#ff9800"}`,
                                fontWeight: 600,
                            }}
                        />
                    </Box>
                    <Box>
                        <Typography sx={labelSx}>Deadline</Typography>
                        <Typography sx={valueSx}>{formatDate(data.Deadline)}</Typography>
                    </Box>
                    <Box>
                        <Typography sx={labelSx}>Created At</Typography>
                        <Typography sx={valueSx}>{formatDate(data.CreatedAt)}</Typography>
                    </Box>
                    {data.FinishedAt && (
                        <Box>
                            <Typography sx={labelSx}>Finished At</Typography>
                            <Typography sx={valueSx}>{formatDate(data.FinishedAt)}</Typography>
                        </Box>
                    )}
                </Box>

                {data.Note && (
                    <Box sx={{ mt: 2 }}>
                        <Typography sx={labelSx}>Note</Typography>
                        <Typography sx={{ ...valueSx, color: "#000" }}>{data.Note}</Typography>
                    </Box>
                )}

                {data.RejectReason && (
                    <Box sx={{ mt: 2, p: 2, background: "rgba(239, 83, 80, 0.1)", borderRadius: "8px", border: "1px solid rgba(239, 83, 80, 0.3)" }}>
                        <Typography sx={labelSx}>Reject Reason</Typography>
                        <Typography sx={{ ...valueSx, color: "#ef5350" }}>{data.RejectReason}</Typography>
                    </Box>
                )}
            </Box>

            {/* Edit Requirements Alert */}
            {data.EditRequirementList.length > 0 && (
                <Box sx={{
                    p: 2.5,
                    background: "rgba(255, 30, 0, 0.1)",
                    borderRadius: "12px",
                    border: "1.5px solid rgba(255, 0, 0, 0.3)",
                    display: "flex",
                    flexDirection: "column",
                    gap: 1.5
                }}>
                    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                        <Edit sx={{ color: "#ff0000ff", fontSize: "1.3rem" }} />
                        <Typography sx={{ color: "#ff0000ff", fontWeight: 700, fontSize: "0.95rem" }}>
                            Edit Requirements Requested
                        </Typography>
                    </Box>
                    <Box sx={{ pl: 4 }}>
                        {data.EditRequirementList.map((req, idx) => (
                            <Typography key={req.Id} sx={{ color: "#000", fontSize: "0.85rem", mb: 0.5 }}>
                                {idx + 1}. {req.Name}
                            </Typography>
                        ))}
                    </Box>
                    <Typography sx={{ color: "rgba(0,0,0,0.7)", fontSize: "0.8rem", fontStyle: "italic", pl: 4 }}>
                        Please re-upload the corresponding audio files after making the required edits.
                    </Typography>
                </Box>
            )}

            {uploadedTracks.length > 0 ? (
                <Box>
                    <Typography sx={{ ...labelSx, fontSize: "0.85rem", mb: 2 }}>Podcast Tracks ({uploadedTracks.length})</Typography>

                    <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                        {uploadedTracks.map((track: any, index: number) => {
                            const reuploadFile = reuploadFiles[track.BookingRequirementId];
                            return (
                                <Box
                                    key={track.Id}
                                    sx={{
                                        p: 2.5,
                                        background: "linear-gradient(145deg, rgba(174, 227, 57, 0.08), rgba(174, 227, 57, 0.03))",
                                        borderRadius: "12px",
                                        border: "1px solid rgba(174, 227, 57, 0.2)",
                                        transition: "all 0.3s ease",
                                    }}
                                >
                                    <Box sx={{ display: "flex", alignItems: "flex-start", gap: 2, mb: 2 }}>
                                        <Box
                                            sx={{
                                                width: 36,
                                                height: 36,
                                                background: "linear-gradient(135deg, var(--primary-green), #7BA225)",
                                                borderRadius: "10px",
                                                display: "flex",
                                                alignItems: "center",
                                                justifyContent: "center",
                                                color: "#000",
                                                fontWeight: 700,
                                                fontSize: "1rem",
                                                flexShrink: 0,
                                            }}
                                        >
                                            {index + 1}
                                        </Box>
                                        <Box sx={{ flex: 1 }}>
                                            <Typography sx={{ color: "#000", fontWeight: 600, fontSize: "0.95rem", mb: 0.5 }}>
                                                Audio Requirment  {index + 1}
                                            </Typography>
                                            <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap" }}>
                                                <Typography sx={{ color: "rgba(0,0,0,0.7)", fontSize: "0.8rem" }}>
                                                    ⏱ {formatDuration(track.AudioLength)}
                                                </Typography>
                                                <Typography sx={{ color: "rgba(0,0,0,0.7)", fontSize: "0.8rem" }}>
                                                    📦 {formatFileSize(track.AudioFileSize)}
                                                </Typography>
                                                <Typography sx={{ color: "rgba(0,0,0,0.7)", fontSize: "0.8rem" }}>
                                                    👁 {track.RemainingPreviewListenSlot} preview slots
                                                </Typography>
                                            </Box>
                                        </Box>
                                    </Box>


                                    <Box sx={{ mt: 2, display: "flex", gap: 1 }}>
                                        <SmartAudioPlayer
                                            audioId={track.AudioFileKey}
                                            fetchUrlFunction={fetchTrackAudioUrl}
                                            className="flex-1"
                                        />
                                    </Box>
                                </Box>
                            );
                        })}
                    </Box>
                </Box>
            ) : (!data.FinishedAt && reuploadTracks.length > 0 ? (
                <Box>
                    <Typography sx={{ ...labelSx, fontSize: "0.85rem", mb: 2 }}>Tracks Requiring Re-upload ({reuploadTracks.length})</Typography>
                    <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                        {reuploadTracks.map((track: any, index: number) => {
                            const editReqNames = getEditRequirementNames(track.Id);
                            const reuploadFile = reuploadFiles[track.BookingRequirementId];
                            return (
                                <Box key={track.Id} sx={{ p: 2.5, background: "linear-gradient(145deg, rgba(255, 152, 0, 0.08), rgba(255, 152, 0, 0.03))", borderRadius: "12px", border: "1.5px solid rgba(255, 152, 0, 0.3)" }}>
                                    <Box sx={{ display: "flex", alignItems: "flex-start", gap: 2, mb: 2 }}>
                                        <Box sx={{ width: 36, height: 36, background: "linear-gradient(135deg, #ff9800, #f57c00)", borderRadius: "10px", display: "flex", alignItems: "center", justifyContent: "center", color: "#000", fontWeight: 700, fontSize: "1rem", flexShrink: 0 }}>
                                            {index + 1}
                                        </Box>
                                        <Box sx={{ flex: 1 }}>
                                            <Typography sx={{ color: "#000", fontWeight: 600, fontSize: "0.95rem", mb: 0.5 }}>
                                                {editReqNames.join(', ')}
                                            </Typography>
                                            <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap" }}>
                                                <Typography sx={{ color: "rgba(0,0,0,0.7)", fontSize: "0.8rem" }}>⏱ {formatDuration(track.AudioLength)}</Typography>
                                                <Typography sx={{ color: "rgba(0,0,0,0.7)", fontSize: "0.8rem" }}>📦 {formatFileSize(track.AudioFileSize)}</Typography>
                                                <Typography sx={{ color: "rgba(0,0,0,0.7)", fontSize: "0.8rem" }}>👁 {track.RemainingPreviewListenSlot} preview slots</Typography>
                                            </Box>
                                        </Box>
                                    </Box>


                                    {!reuploadFile ? (
                                        <>
                                            <Box sx={{ my: 1, display: "flex" }}>
                                                <SmartAudioPlayer
                                                    audioId={track.AudioFileKey}
                                                    fetchUrlFunction={fetchTrackAudioUrl}
                                                    className="flex-1"
                                                />
                                            </Box>
                                           
                                        </>
                                    ) : (
                                        <Box>
                                            <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", p: 1.5, background: "rgba(255, 152, 0, 0.15)", border: "1px solid rgba(255, 152, 0, 0.3)", borderRadius: "8px", mt: 2 }}>
                                                <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
                                                    <Box sx={{ fontSize: "1.5rem" }}>🎵</Box>
                                                    <Box>
                                                        <Typography sx={{ color: "#000", fontSize: "0.85rem", fontWeight: 600 }}>{reuploadFile.name}</Typography>
                                                        <Typography sx={{ color: "rgba(0,0,0,0.7)", fontSize: "0.75rem" }}>{formatFileSize(reuploadFile.size)}</Typography>
                                                    </Box>
                                                </Box>
                                                <IconButton size="small" onClick={() => handleFileChange(track.BookingRequirementId, null)} sx={{ color: "#ef5350" }}>
                                                    <Delete fontSize="small" />
                                                </IconButton>
                                            </Box>
                                            <Box sx={{ mt: 2, display: "flex" }}>
                                                <audio
                                                    controls
                                                    src={URL.createObjectURL(reuploadFile)}
                                                    style={{ flex: 1 }}

                                                />
                                            </Box>
                                        </Box>
                                    )}
                                </Box>
                            );
                        })}
                    </Box>
                </Box>
            ) : (
                <>
                    <Typography sx={{ ...labelSx, fontSize: "0.85rem" }}>Podcast Tracks</Typography>
                    <Typography sx={{ color: "rgba(0,0,0,0.7)", fontSize: "0.9rem" }}>
                        {data.FinishedAt ? 'No podcast tracks available.' : 'Please upload podcast tracks once they are available.'}
                    </Typography>
                </>
            ))}


            {/* Footer Actions */}
          
        </Box>
    );
};

export default ProducingRequestModal;