import React, { useEffect, useState, useCallback, useContext } from 'react';
import { Box, Typography, Button, IconButton, Chip, TextField } from '@mui/material';
import { CloudUpload, PlayArrow, Delete, Edit, CheckCircle, Cancel } from '@mui/icons-material';
import { formatDate } from '@/core/utils/date.util';
import { acceptProducingRequest, getProducingRequestAudio, getProducingRequestDetail, submitAudio } from '@/core/services/booking/producing.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import Loading from '@/views/components/common/loading';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { toast } from 'react-toastify';
import { BookingDetailPageContext } from '.';
import Loading2 from '@/views/components/common/loading2';
import { SmartAudioPlayer } from '@/views/components/common/audio';
import Modal_Button from '@/views/components/common/modal/ModalButton';

interface ProducingRequestModalProps {
    bookingProducingRequestId: any;
    onClose: () => void;
}
interface AudioUploadItem {
    requirementId: string;
    file: File | null;
}


const ProducingRequestModal: React.FC<ProducingRequestModalProps> = ({ bookingProducingRequestId, onClose }) => {
    const context = useContext(BookingDetailPageContext);
    const [data, setData] = useState<any>(null);
    const [loading, setLoading] = useState(false);
    const [reuploadFiles, setReuploadFiles] = useState<Record<string, File | null>>({});
    // smart audio refresh handled by hook; no manual source cache needed
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [rejectReason, setRejectReason] = useState<string>('');

    // Removed manual lifetime constants; SmartAudio handles expiry refresh (default 5s)
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 60,
        intervalSeconds: 2,
    })

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
        }
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


    const handleSubmitAudio = async () => {
        const audioFileArray: File[] = [];

        Object.entries(reuploadFiles).forEach(([requirementId, file]) => {
            if (file) {
                const ext = file.name.split('.').pop();
                const newName = `${requirementId}.${ext}`;
                const renamed = new File([file], newName, { type: file.type });
                audioFileArray.push(renamed);
            }
        });
        console.log("Submitting files:", audioFileArray);
        try {
            setIsSubmitting(true);
            const res = await submitAudio(loginRequiredAxiosInstance, bookingProducingRequestId, audioFileArray);
            const sagaId = res?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Submit failed, please try again.")
                return
            }
            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: () => {
                    onClose();
                    context?.handleDataChange();
                    toast.success(`Submit successfully!`);
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error submitting audio");
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleAccept = async (isAccept: boolean) => {
        try {
            if (!isAccept && rejectReason.trim() === '') {
                toast.error("Please enter reject reason")
                return
            }
            if (!isAccept) {
                setRejectReason('')
            }
            setIsSubmitting(true);
            const res = await acceptProducingRequest(loginRequiredAxiosInstance, bookingProducingRequestId, isAccept, rejectReason);
            const sagaId = res?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Failed, please try again.")
                return
            }
            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    onClose();
                    await context?.handleDataChange();
                    toast.success(`Request ${isAccept ? "accepted" : "rejected"} Successfully!`);
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error processing request");
        } finally {
            setIsSubmitting(false);
        }
    };


    if (!data || loading) {
        return (
            <div className="flex justify-center items-center h-100">
                <Loading />
            </div>
        );
    }

    const labelSx = {
        color: "var(--white-75)",
        fontSize: "0.75rem",
        fontWeight: 600,
        textTransform: "uppercase",
        letterSpacing: "0.5px",
        mb: 0.75,
    };

    const valueSx = {
        color: "#fff",
        fontSize: "0.95rem",
        lineHeight: 1.5,
        fontWeight: 500,
    };

    return (
        <Box sx={{ color: "white", p: 3, display: "flex", flexDirection: "column", gap: 3, minWidth: 700, maxWidth: 900 }}>
            <Box>
                <Typography variant="h6" sx={{ color: "#fff", fontWeight: 700, fontSize: "1.1rem", mb: 2 }}>
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
                        <Typography sx={{ ...valueSx, color: "rgba(255,255,255,0.85)" }}>{data.Note}</Typography>
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
                        <Typography sx={{ color: "white", fontWeight: 700, fontSize: "0.95rem" }}>
                            Edit Requirements Requested
                        </Typography>
                    </Box>
                    <Box sx={{ pl: 4 }}>
                        {data.EditRequirementList.map((req, idx) => (
                            <Typography key={req.Id} sx={{ color: "rgba(255,255,255,0.9)", fontSize: "0.85rem", mb: 0.5 }}>
                                {idx + 1}. {req.Name}
                            </Typography>
                        ))}
                    </Box>
                    <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.8rem", fontStyle: "italic", pl: 4 }}>
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
                                            <Typography sx={{ color: "#fff", fontWeight: 600, fontSize: "0.95rem", mb: 0.5 }}>
                                                Audio Requirment  {index + 1}
                                            </Typography>
                                            <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap" }}>
                                                <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.8rem" }}>
                                                    ⏱ {formatDuration(track.AudioLength)}
                                                </Typography>
                                                <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.8rem" }}>
                                                    📦 {formatFileSize(track.AudioFileSize)}
                                                </Typography>
                                                <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.8rem" }}>
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
                                            <Typography sx={{ color: "#fff", fontWeight: 600, fontSize: "0.95rem", mb: 0.5 }}>
                                                {editReqNames.join(', ')}
                                            </Typography>
                                            <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap" }}>
                                                <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.8rem" }}>⏱ {formatDuration(track.AudioLength)}</Typography>
                                                <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.8rem" }}>📦 {formatFileSize(track.AudioFileSize)}</Typography>
                                                <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.8rem" }}>👁 {track.RemainingPreviewListenSlot} preview slots</Typography>
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
                                            {context.CurrentStatus !== "Producing Requested" && data.IsAccepted !== false && (
                                                <Button variant="outlined" component="label" startIcon={<CloudUpload />} sx={{ color: "#ff9800", borderColor: "#ff9800", textTransform: "none", borderRadius: "10px", padding: "10px 20px", fontWeight: 600, mt: 2, "&:hover": { backgroundColor: "rgba(255, 152, 0, 0.1)", borderColor: "#ff9800" } }}>
                                                    Re-upload Audio
                                                    <input type="file" hidden accept=".wav,.flac,.mp3,.m4a,.aac" onChange={(e) => { const file = e.target.files?.[0]; if (file) handleFileChange(track.BookingRequirementId, file); }} />
                                                </Button>
                                            )}
                                        </>
                                    ) : (
                                        <Box>
                                            <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", p: 1.5, background: "rgba(255, 152, 0, 0.15)", border: "1px solid rgba(255, 152, 0, 0.3)", borderRadius: "8px", mt: 2 }}>
                                                <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
                                                    <Box sx={{ fontSize: "1.5rem" }}>🎵</Box>
                                                    <Box>
                                                        <Typography sx={{ color: "#fff", fontSize: "0.85rem", fontWeight: 600 }}>{reuploadFile.name}</Typography>
                                                        <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.75rem" }}>{formatFileSize(reuploadFile.size)}</Typography>
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
                    <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.9rem" }}>
                        {data.FinishedAt ? 'No podcast tracks available.' : 'Please upload podcast tracks once they are available.'}
                    </Typography>
                </>
            ))}


            {/* Footer Actions */}
            <Box sx={{ display: "flex", justifyContent: "flex-end", gap: 2, pt: 2, borderTop: "1px solid rgba(255,255,255,0.1)" }}>

                {data.EditRequirementList.length > 0 && data.IsAccepted !== false && (
                    <>
                        {context.CurrentStatus === "Producing Requested" ? (
                            <>
                                <Modal_Button
                                    className="booking-detail__reject-btn-contained"
                                    content="Reject"
                                    variant="contained"
                                    size='sm'
                                >
                                    <div className="booking-detail__cancel-modal">
                                        <label className="booking-detail__label">
                                            Reject Reason <span className="text-red-500">*</span>
                                        </label>
                                        <input
                                            type="text"
                                            className="booking-detail__input"
                                            placeholder="Enter reject reason"
                                            value={rejectReason}
                                            onChange={(e) => setRejectReason(e.target.value)}
                                            onKeyPress={(e) => {
                                                if (e.key === 'Enter') {
                                                    handleAccept(false)
                                                }
                                            }}
                                        />
                                        <button
                                            type="button"
                                            className="booking-detail__btn booking-detail__btn--resolve "
                                            onClick={() => handleAccept(false)}
                                            disabled={isSubmitting}
                                        >
                                            {isSubmitting ? "Rejecting..." : "Confirm"}
                                        </button>
                                    </div>


                                </Modal_Button>

                                <Button
                                    variant="contained"
                                    onClick={() => handleAccept(true)}
                                    disabled={isSubmitting}
                                    sx={{
                                        background: "linear-gradient(90deg, #ff9800 0%, #f57c00 100%)",
                                        color: "#fff",
                                        fontWeight: 700,
                                        textTransform: "none",
                                        borderRadius: "10px",
                                        px: 3,
                                        "&:hover": { background: "linear-gradient(90deg, #f57c00 0%, #e65100 100%)" },
                                        "&:disabled": { background: "#555", color: "#999" }
                                    }}
                                >
                                    Accept
                                </Button>
                            </>
                        ) :
                            (<>
                                {isSubmitting ? (
                                    <div className="flex justify-center items-center m-8 ">
                                        <Loading2 title="Audio Uploading" />
                                    </div>
                                ) : (<>
                                    <Button
                                        variant="contained"
                                        onClick={handleSubmitAudio}
                                        disabled={isSubmitDisabled || isSubmitting}
                                        sx={{
                                            background: "linear-gradient(90deg, #ff9800 0%, #f57c00 100%)",
                                            color: "#fff",
                                            fontWeight: 700,
                                            textTransform: "none",
                                            borderRadius: "10px",
                                            px: 3,
                                            "&:hover": { background: "linear-gradient(90deg, #f57c00 0%, #e65100 100%)" },
                                            "&:disabled": { background: "#555", color: "#999" }
                                        }}
                                    >
                                        Submit Re-uploaded Files
                                    </Button>
                                </>)}
                            </>
                            )}
                    </>
                )}
            </Box>
        </Box>
    );
};

export default ProducingRequestModal;