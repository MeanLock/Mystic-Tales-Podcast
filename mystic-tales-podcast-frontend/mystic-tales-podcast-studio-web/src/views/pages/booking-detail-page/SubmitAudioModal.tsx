import React, { FC, useCallback, useContext, useEffect, useState } from 'react';
import {
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  IconButton,
  Typography,
  Card,
  CardContent,
  Chip,
  Box,
} from '@mui/material';
import { Close as CloseIcon, CloudUpload, Delete, Edit } from '@mui/icons-material';
import { formatDate } from '@/core/utils/date.util';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { getProducingRequestAudio, getProducingRequestDetail, submitAudio } from '@/core/services/booking/producing.service';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { toast } from 'react-toastify';
import { BookingDetailPageContext } from '.';
import Loading2 from '@/views/components/common/loading2';
import Loading from '@/views/components/common/loading';





interface AudioUploadItem {
  requirementId: string;
  file: File | null;
}

interface SubmitAudioModalProps {
  booking: any;
  onClose: () => void;
}


const SubmitAudioModal: FC<SubmitAudioModalProps> = ({ booking, onClose }) => {
  const context = useContext(BookingDetailPageContext);
  const Booking = booking;
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [loading, setLoading] = useState(false);
  const [data, setData] = useState<any>(null);
  const [reuploadFiles, setReuploadFiles] = useState<Record<string, File | null>>({});
  const [audioSources, setAudioSources] = useState<Record<string, { url: string; expiresAt: number }>>({});
  const AUDIO_LIFETIME_MS = 5000;
  const REFRESH_BUFFER_MS = 500;
  const [audioFiles, setAudioFiles] = useState<AudioUploadItem[]>(
    Booking.BookingRequirementFileList.map(req => ({
      requirementId: req.Id,
      file: null
    }))
  );

  const { startPolling } = useSagaPolling({
    timeoutSeconds: 60,
    intervalSeconds: 2,
  })

  const fetchProducingRequest = async () => {
    let alive = true;
    (async () => {
      setLoading(true);
      try {
        const response = await getProducingRequestDetail(loginRequiredAxiosInstance, (Booking.BookingProducingRequestList[0])?.Id);
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
  const fetchAudioUrl = useCallback(async (trackId: string, fileKey: string) => {
    try {
      const res = await getProducingRequestAudio(loginRequiredAxiosInstance, fileKey);
      if (res.success) {
        setAudioSources(prev => ({
          ...prev,
          [trackId]: { url: res.data.FileUrl, expiresAt: Date.now() + AUDIO_LIFETIME_MS }
        }));
      }
    } catch (e) {
      console.error('fetchAudioUrl error', e);
    }
  }, []);
  useEffect(() => {
    if (Booking.BookingProducingRequestList[0].IsAccepted && Booking.BookingProducingRequestList[0].Note !== "")
      fetchProducingRequest();
  }, []);
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
    if (reuploadTracks?.length > 0) {
      reuploadTracks.forEach((track: any) => {
        const fileKey = track.AudioFileKey;
        if (fileKey) fetchAudioUrl(track.Id, fileKey);
      });
    } else {
      return;
    }

  }, [data?.FinishedAt, reuploadTracks?.length]);

  const getEditRequirementNames = (trackId: string) => {
    return (data.EditRequirementList || [])
      .filter((req: any) => req?.BookingPodcastTrack?.Id === trackId)
      .map((req: any) => req.Name);
  };
  const formatDuration = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const formatFileSize = (bytes: number) => {
    return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
  };
  const ensureFreshUrl = useCallback((trackId: string, fileKey: string) => {
    const entry = audioSources[trackId];
    if (!entry || entry.expiresAt - Date.now() < REFRESH_BUFFER_MS) {
      fetchAudioUrl(trackId, fileKey);
    }
  }, [audioSources, fetchAudioUrl]);
  const requiredRequirementIds: string[] = React.useMemo(() => {
    if (!data) return [];
    const ids = new Set<string>();

    if (data.FinishedAt === null) {
      (data.EditRequirementList || [])
        .filter((req: any) => req?.BookingPodcastTrack?.BookingRequirementId)
        .forEach((req: any) => ids.add(req.BookingPodcastTrack.BookingRequirementId));
    }
    return Array.from(ids);
  }, [data]);
  const isSubmitDisabled = React.useMemo(() => {
    if (requiredRequirementIds.length === 0) return true;
    return requiredRequirementIds.some(id => !reuploadFiles[id]);
  }, [requiredRequirementIds, reuploadFiles]);

  const handleFileChange = (requirementId: string, file: File | null) => {
    const allowedExtensions = ['wav', 'flac', 'mp3', 'm4a', 'aac'];
     if (file ) {
     const ext = file?.name.split('.').pop()?.toLowerCase();
    if (!ext || !allowedExtensions.includes(ext)) {
      toast.error('Allowed audio types: wav, flac, mp3, m4a, aac');
      return;
    }
    }
   
    if (file && file?.size > 150 * 1024 * 1024) {
      toast.error("File size exceeds 150 MB limit.");
      return;
    }
    setAudioFiles(prev =>
      prev.map(item =>
        item.requirementId === requirementId
          ? { ...item, file }
          : item
      )
    );
    setReuploadFiles(prev => ({ ...prev, [requirementId]: file }));

  };

  const handleRemoveFile = (requirementId: string) => {
    handleFileChange(requirementId, null);
  };

  const handleSubmitAudio = async () => {
    const producingRequestId = Booking.BookingProducingRequestList[0]?.Id;
    const audioFileArray: File[] = [];

    audioFiles.forEach(item => {
      if (item.file) {
        const fileExtension = item.file.name.split('.').pop();
        const newFileName = `${item.requirementId}.${fileExtension}`;
        const renamedFile = new File([item.file], newFileName, { type: item.file.type });
        audioFileArray.push(renamedFile);
      }
    });

    try {
      setIsSubmitting(true);
      const res = await submitAudio(loginRequiredAxiosInstance, producingRequestId, audioFileArray);
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
  const handleSubmitProducingRequest = async () => {
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
      const res = await submitAudio(loginRequiredAxiosInstance, Booking.BookingProducingRequestList[0]?.Id, audioFileArray);
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
  if (loading) {
    return (
      <div className="flex justify-center items-center h-100 ">
        <Loading />
      </div>
    )
  }


  return (
    <div >
      {Booking.CurrentStatus.Name === "Producing" && data != null && data.EditRequirementList.length > 0 ? (
        <>
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
          <Box>
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
                          <audio
                            controls
                            src={audioSources[track.Id]?.url}
                            style={{ flex: 1 }}
                            onError={() => ensureFreshUrl(track.Id, track.AudioFileKey)}
                          />
                        </Box>
                        <Button variant="outlined" component="label" startIcon={<CloudUpload />} sx={{ color: "#ff9800", borderColor: "#ff9800", textTransform: "none", borderRadius: "10px", padding: "10px 20px", fontWeight: 600, mt: 2, "&:hover": { backgroundColor: "rgba(255, 152, 0, 0.1)", borderColor: "#ff9800" } }}>
                          Re-upload Audio
                          <input type="file" hidden accept=".wav,.flac,.mp3,.m4a,.aac" onChange={(e) => { const file = e.target.files?.[0]; if (file) handleFileChange(track.BookingRequirementId, file); }} />
                        </Button>
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
          <Box sx={{ display: "flex", justifyContent: "flex-end", gap: 2, pt: 2, borderTop: "1px solid rgba(255,255,255,0.1)" }}>

            {data.EditRequirementList.length > 0 && data.IsAccepted !== false && (
              <>

                {isSubmitting ? (
                  <div className="flex justify-center items-center m-8 ">
                    <Loading2 title="Audio Uploading" />
                  </div>
                ) : (<>
                  <Button
                    variant="contained"
                    onClick={handleSubmitProducingRequest}
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
          </Box>
        </>
      ) : (
        <>
          <div className="booking-detail__modal-content">
            {Booking.BookingRequirementFileList.map((req, index) => {
              const audioItem = audioFiles.find(item => item.requirementId === req.Id);
              return (
                <div key={req.Id} className="booking-detail__upload-card">
                  <div className="booking-detail__upload-card-header">
                    <div className="booking-detail__requirement-badge">
                      {index + 1}
                    </div>
                    <div className="booking-detail__upload-info">
                      <Typography className="booking-detail__upload-title">
                        {req.Name}
                      </Typography>
                    </div>
                  </div>

                  <div className="booking-detail__upload-area">
                    {!audioItem?.file ? (
                      <div className="booking-detail__upload-zone">
                        <Button
                          variant="outlined"
                          component="label"
                          startIcon={<CloudUpload />}
                          className="booking-detail__upload-btn"
                          fullWidth
                        >
                          Choose Audio File
                          <input
                            type="file"
                            hidden
                            accept=".wav,.flac,.mp3,.m4a,.aac"
                            onChange={(e) => {
                              const file = e.target.files?.[0];
                              if (file) handleFileChange(req.Id, file);
                            }}
                          />
                        </Button>
                        <Typography className="booking-detail__upload-hint">
                          Supported formats: MP3, WAV, FLAC
                        </Typography>
                      </div>
                    ) : (
                      <div>
                        <div className="booking-detail__file-preview">
                          <div className="booking-detail__file-info">
                            <div className="booking-detail__file-icon">🎵</div>
                            <div className="booking-detail__file-details">
                              <Typography className="booking-detail__file-name">
                                {audioItem.file.name}
                              </Typography>
                              <Typography className="booking-detail__file-size">
                                {(audioItem.file.size / (1024 * 1024)).toFixed(2)} MB
                              </Typography>
                            </div>
                          </div>
                          <IconButton
                            size="small"
                            onClick={() => handleRemoveFile(req.Id)}
                            className="booking-detail__remove-btn"
                          >
                            <Delete fontSize="small" />
                          </IconButton>

                        </div>
                        <Box className="mt-4 flex">
                          <audio
                            controls
                            src={URL.createObjectURL(audioItem.file)}
                            style={{ flex: 1 }}
                          />
                        </Box>
                      </div>
                    )}
                  </div>
                </div>
              );
            })}

          </div>
          {isSubmitting ? (
            <div className="flex justify-center items-center m-8 ">
              <Loading2 title="Audio Uploading" />
            </div>
          ) :
            (
              <div className="booking-detail__modal-footer">
                <div className="booking-detail__upload-summary">
                  <Typography className="booking-detail__summary-text">
                    {audioFiles.filter(item => item.file).length} of {audioFiles.length} files uploaded
                  </Typography>
                </div>
                <Button
                  variant="contained"
                  onClick={handleSubmitAudio}
                  disabled={audioFiles.some(item => !item.file)}
                  className="booking-detail__submit-modal-btn"
                  size="large"
                >
                  Submit
                </Button>
              </div>
            )}
        </>
      )}
    </div>
  );
};

export default SubmitAudioModal;


