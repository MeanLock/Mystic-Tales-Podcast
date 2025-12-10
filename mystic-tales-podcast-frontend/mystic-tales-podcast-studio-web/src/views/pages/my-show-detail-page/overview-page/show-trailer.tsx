import React, { useEffect, useRef, useState } from "react";
import WaveSurfer from "wavesurfer.js";
import {
    Button,
    IconButton,
    Slider,
    Typography,
} from "@mui/material";
import {
    PlayArrow,
    Pause,
    VolumeUp,
    VolumeOff,
    MusicNote,
} from "@mui/icons-material";
import { Database, FileAudio, FolderSimple } from "phosphor-react";
import { toast } from "react-toastify";
import { useSagaPolling } from "@/core/hooks/useSagaPolling";
import { getShowDetail, uploadTrailer } from "@/core/services/show/show.service";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";
import { useParams } from "react-router-dom";
import Loading2 from "@/views/components/common/loading2";
import { getPublicSource } from "@/core/services/file/file.service";
import Loading from "@/views/components/common/loading";
import { RootState } from "@/redux/rootReducer";
import { useSelector } from "react-redux";

interface ShowTrailerProps {
    initialAudio?: string;
}

const ShowTrailer: React.FC<ShowTrailerProps> = ({
    initialAudio = "",
}) => {

    const { id } = useParams<{ id: string }>();
    const authSlice = useSelector((state: RootState) => state.auth);

    const waveformRef = useRef<HTMLDivElement>(null);
    const wavesurferRef = useRef<WaveSurfer | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const progressBarRef = useRef<HTMLDivElement>(null);

    const [isPlaying, setIsPlaying] = useState(false);
    const [currentTime, setCurrentTime] = useState(0);
    const [duration, setDuration] = useState(0);
    const [volume, setVolume] = useState(0.7);
    const [isMuted, setIsMuted] = useState(false);
    const [audioUrl, setAudioUrl] = useState("");
    const [isDragging, setIsDragging] = useState(false);
    const [hasChanges, setHasChanges] = useState(false);
    const [uploadedFile, setUploadedFile] = useState<File | null>();
    const [isSeeking, setIsSeeking] = useState(false);

    const [loading, setLoading] = useState(false);
    const [uploading, setUploading] = useState(false);

    const { startPolling } = useSagaPolling({
        timeoutSeconds: 160,
        intervalSeconds: 2,
    })
    const fetchTrailerAudio = async () => {
        setLoading(true);
        try {
            const res = await getShowDetail(loginRequiredAxiosInstance, id);
            console.log("Fetched show detail:", res.data.Show);
            if (res.success && res.data.Show.TrailerAudioFileKey) {
                const fileurl = await getPublicSource(loginRequiredAxiosInstance, res.data.Show.TrailerAudioFileKey);
                if (fileurl.success && fileurl.data.FileUrl) {
                    setAudioUrl(fileurl.data.FileUrl);
                    const dummyFile = new File([], 'MTP_Existing_trailer.mp3', { type: 'audio/mpeg' });
                    setUploadedFile(dummyFile);
                }

            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch show detail:', error);
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        fetchTrailerAudio()
    }, [id]);

    const handleSeekMouseDown = (e: React.MouseEvent<HTMLDivElement>) => {
        if (!progressBarRef.current || !wavesurferRef.current) return;

        const rect = progressBarRef.current.getBoundingClientRect();
        const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1);
        const newTime = percent * duration;

        // Xử lý click ngay lập tức
        wavesurferRef.current.setTime(newTime);
        setCurrentTime(newTime);

        // Bắt đầu kéo
        setIsSeeking(true);
    };


    const handleSeekMouseMove = (e: MouseEvent) => {
        if (!isSeeking || !progressBarRef.current || !wavesurferRef.current) return;
        const rect = progressBarRef.current.getBoundingClientRect();
        const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1);
        const newTime = percent * duration;
        wavesurferRef.current.setTime(newTime);
        setCurrentTime(newTime);
    };

    const handleSeekMouseUp = () => {
        setIsSeeking(false);
    };
    useEffect(() => {
        if (isSeeking) {
            window.addEventListener("mousemove", handleSeekMouseMove);
            window.addEventListener("mouseup", handleSeekMouseUp);
        } else {
            window.removeEventListener("mousemove", handleSeekMouseMove);
            window.removeEventListener("mouseup", handleSeekMouseUp);
        }

        return () => {
            window.removeEventListener("mousemove", handleSeekMouseMove);
            window.removeEventListener("mouseup", handleSeekMouseUp);
        };
    }, [isSeeking]);

    useEffect(() => {
        if (!uploadedFile) return;
        const audio = document.createElement('audio');
        audio.src = URL.createObjectURL(uploadedFile);
        audio.onloadedmetadata = () => {
            // audio.duration là thời lượng (giây)
            // setDuration(audio.duration); // nếu muốn
        };
    }, [uploadedFile]);

    useEffect(() => {
        if (!waveformRef.current) return;

        const ws = WaveSurfer.create({
            container: waveformRef.current,
            waveColor: "#7BA225",
            progressColor: "#AEE339",
            cursorColor: "#AEE339",
            barWidth: 2,
            barGap: 2,
            fillParent: true,
            minPxPerSec: 30,
            barRadius: 2,
            height: 130,
            interact: false,        // Không cho phép click vào waveform
            hideScrollbar: true,    // Ẩn scrollbar
        });

        wavesurferRef.current = ws;
        ws.load(audioUrl);

        ws.on("ready", () => {
            setDuration(ws.getDuration());
        });

        ws.on("play", () => setIsPlaying(true));
        ws.on("pause", () => setIsPlaying(false));
        ws.on("finish", () => {
            setIsPlaying(false);
            setCurrentTime(0);
        });

        return () => {
            ws.destroy();
        };
    }, [audioUrl]);

    useEffect(() => {
        let animationFrameId: number;

        const updateTime = () => {
            if (wavesurferRef.current && wavesurferRef.current.isPlaying()) {
                const time = wavesurferRef.current.getCurrentTime();
                setCurrentTime(time);
            }
            animationFrameId = requestAnimationFrame(updateTime);
        };

        animationFrameId = requestAnimationFrame(updateTime);

        return () => cancelAnimationFrame(animationFrameId);
    }, []);

    const handlePlayPause = () => {
        wavesurferRef.current?.playPause();
    };

    const handleVolumeChange = (_: Event, newValue: number | number[]) => { const vol = newValue as number; setVolume(vol); setIsMuted(vol === 0); wavesurferRef.current?.setVolume(vol); };

    const handleMuteToggle = () => {
        if (!wavesurferRef.current) return;
        if (isMuted) {
            wavesurferRef.current.setVolume(volume);
            setIsMuted(false);
        } else {
            wavesurferRef.current.setVolume(0);
            setIsMuted(true);
        }
    };

    const formatTime = (seconds: number) => {
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs.toString().padStart(2, "0")}`;
    };

    const handleProgressClick = (e: React.MouseEvent<HTMLDivElement>) => {
        if (!wavesurferRef.current || !progressBarRef.current) return;
        const rect = progressBarRef.current.getBoundingClientRect();
        const clickX = e.clientX - rect.left;
        const percent = clickX / rect.width;
        const newTime = percent * duration;
        wavesurferRef.current.setTime(newTime);
        setCurrentTime(newTime);
    };

    const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        const allowedExtensions = ['wav', 'flac', 'mp3', 'm4a', 'aac'];
        const ext = file.name.split('.').pop()?.toLowerCase();
        if (!ext || !allowedExtensions.includes(ext)) {
            toast.error('Allowed audio types: wav, flac, mp3, m4a, aac');
            return;
        }
        if (file.size > 15 * 1024 * 1024) {
            toast.error("File size exceeds 15MB limit.");
            return;
        }
        if (file && file.type.startsWith("audio/")) {
            const url = URL.createObjectURL(file);
            setAudioUrl(url);
            setHasChanges(true);
            setUploadedFile(file); // Lưu file
        }
    };

    const handleDragOver = (e: React.DragEvent) => {
        e.preventDefault();
        setIsDragging(true);
    };

    const handleDragLeave = () => setIsDragging(false);

    const handleDrop = (e: React.DragEvent) => {
        e.preventDefault();
        setIsDragging(false);
        const file = e.dataTransfer.files?.[0];
        if (file.size > 15 * 1024 * 1024) {
            toast.error("File size exceeds 15MB limit.");
            return;
        }
        if (file && file.type.startsWith("audio/")) {
            const url = URL.createObjectURL(file);
            setAudioUrl(url);
            setHasChanges(true);
            setUploadedFile(file); // Lưu file
        }
    };

    const handleBrowseClick = () => fileInputRef.current?.click();

    const handleUploadTrailer = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        try {
            setUploading(true);
            console.log('Uploading trailer with file:', uploadedFile);
            const res = await uploadTrailer(loginRequiredAxiosInstance, id, uploadedFile);
            const sagaId = res?.data?.SagaInstanceId
            if (!res.success && res.message.content) {
                toast.error(res.message.content)
                return
            }
            if (!sagaId) {
                toast.error(`Upload trailer failed, please try again.`)
                return
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: () => {
                    toast.success(`Trailer uploaded successfully.`)
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error uploading trailer");
        } finally {
            setUploading(false);
        }
    };
    if (loading) {
        return (
            <div className=" flex justify-center items-center mt-20">
                <Loading />
            </div>
        );
    }
    return (
        <div className="show-trailer">
            <div className="show-trailer__header">
                <Typography variant="h6" className="show-trailer__title">
                    Trailer Audio
                </Typography>
                <Button
                    variant="contained"
                    className="show-trailer__save-btn"
                    onClick={() => handleUploadTrailer()}
                    disabled={!hasChanges || uploading}
                >
                    Save
                </Button>
            </div>

            {/* Chỉ hiển thị player khi có file */}
            {uploadedFile && (
                <div className="show-trailer__player">
                    <div className="show-trailer__waveform-container">
                        <div ref={waveformRef} className="show-trailer__waveform" />
                    </div>
                    <div className="show-trailer__controls">
                        <div className="show-trailer__controls-left">
                            <IconButton onClick={handlePlayPause} className="show-trailer__play-btn">
                                {isPlaying ? <Pause /> : <PlayArrow />}
                            </IconButton>
                            <Typography className="show-trailer__time-text">
                                {formatTime(currentTime)}
                            </Typography>
                        </div>

                        <div
                            className="show-trailer__progress-bar"
                            ref={progressBarRef}
                            onMouseDown={handleSeekMouseDown}
                        >
                            <div
                                className="show-trailer__progress-fill"
                                style={{ width: `${(currentTime / duration) * 100}%` }}
                            />
                            <div
                                className="show-trailer__progress-thumb"
                                style={{ left: `${(currentTime / duration) * 100}%` }}
                            />
                        </div>

                        <div className="show-trailer__controls-right">
                            <Typography className="show-trailer__time-text">
                                {formatTime(duration)}
                            </Typography>
                            <IconButton onClick={handleMuteToggle} className="show-trailer__volume-btn" size="small">
                                {isMuted ? <VolumeOff /> : <VolumeUp />}
                            </IconButton>
                            <Slider
                                value={isMuted ? 0 : volume}
                                onChange={handleVolumeChange}
                                min={0}
                                max={1}
                                step={0.01}
                                className="show-trailer__volume-slider"
                            />
                        </div>
                    </div>
                </div>
            )}
            {uploading ? (
                <div className=" flex justify-center items-center mt-20">
                    <Loading2 title="Uploading" />
                </div>
            ) : (
                <div className={`show-trailer__upload-section ${!uploadedFile ? 'show-trailer__upload-section--no-file' : ''}`}>
                    <div
                        className={`show-trailer__upload ${isDragging ? "show-trailer__upload--dragging" : ""} ${!uploadedFile ? 'show-trailer__upload--full-width' : ''}`}
                        onDragOver={handleDragOver}
                        onDragLeave={handleDragLeave}
                        onDrop={handleDrop}
                        onClick={handleBrowseClick}
                    >
                        <MusicNote className="show-trailer__upload-icon" />
                        <Typography className="show-trailer__upload-text">
                            Drop your audio file here
                        </Typography>
                        <Typography className="show-trailer__upload-subtext">
                            or click to browse
                        </Typography>
                        <input
                            ref={fileInputRef}
                            type="file"
                            accept=".wav,.flac,.mp3,.m4a,.aac"
                            onChange={handleFileSelect}
                            className="show-trailer__upload-input"
                        />
                    </div>
                    {(uploadedFile && uploadedFile.name !== 'MTP_Existing_trailer.mp3') && (
                        <div className="show-trailer__file-info">
                            <div className="show-trailer__file-box">
                                <div className="show-trailer__file-row">
                                    <FolderSimple size={20} color="#B6E04A" />
                                    <Typography variant="body2" className="show-trailer__file-name">
                                        <strong>File Name:  </strong>{uploadedFile.name}
                                    </Typography>
                                </div>
                                <div className="show-trailer__file-row">
                                    <Database size={20} color="#B6E04A" />
                                    <Typography variant="body2" className="show-trailer__file-size">
                                        <strong>Size:  </strong> {(uploadedFile.size / 1024 / 1024).toFixed(2)} MB
                                    </Typography>
                                </div>
                                <div className="show-trailer__file-row">
                                    <FileAudio size={20} color="#B6E04A" />
                                    <Typography variant="body2" className="show-trailer__file-type">
                                        <strong>Type:  </strong>{uploadedFile.type.replace('audio/', '')}
                                    </Typography>
                                </div>
                            </div>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

export default ShowTrailer;
