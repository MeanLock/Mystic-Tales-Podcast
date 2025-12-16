// import type React from "react"
// import { useContext, useEffect, useMemo, useRef, useState } from "react"
// import WaveSurfer from "wavesurfer.js"
// import { Music, Download, Play, Minus, CloudUpload } from "lucide-react"
// import { ArrowCounterClockwise, Database, FolderSimple, Plus, Question } from "phosphor-react"
// import { IconButton, MenuItem, Modal, Select, Skeleton, Tooltip } from "@mui/material"
// import ghost from "../../../../assets/ghost.mp3"
// import { PlayArrow, Pause, ContentCopy, PublishedWithChangesOutlined } from "@mui/icons-material"
// import { toast } from "react-toastify"
// import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
// import { useNavigate, useParams } from "react-router-dom"
// import { audioTuning, getAudioFile, getBackgroundSoundFile, getBackgroundSounds, uploadAudio } from "@/core/services/episode/audio.service"
// import { BackgroundSound } from "@/core/types"
// import { useSagaPolling } from "@/core/hooks/useSagaPolling"
// import { set } from "lodash"
// import Loading2 from "@/views/components/common/loading2"
// import Image from "@/views/components/common/image"
// import { Episode } from "@/core/types/episode"
// import { getEpisodeDetail } from "@/core/services/episode/episode.service"
// import Loading from "@/views/components/common/loading"
// import Modal_Button from "@/views/components/common/modal/ModalButton"
// import { buildEpisodeAudioFileName } from "@/core/utils/audio.util"
// import { confirmAlert } from "@/core/utils/alert.util"
// import { EpisodeDetailViewContext } from "."
// interface EpisodeAudioProps {
//     initialAudio?: string
// }
// const presets: Record<string, Record<string, number>> = {
//     Flat: {
//         SubBass: 0,
//         Bass: 0,
//         Low: 0,
//         LowMid: 0,
//         Mid: 0,
//         Presence: 0,
//         HighMid: 0,
//         Treble: 0,
//         Air: 0,
//     },
//     Podcast: {
//         SubBass: -3,
//         Bass: -2,
//         Low: -1,
//         LowMid: 2,
//         Mid: 3,
//         Presence: 2,
//         HighMid: 1,
//         Treble: 1,
//         Air: 0,
//     },
//     BassBoost: {
//         SubBass: 5,
//         Bass: 4,
//         Low: 2,
//         LowMid: 0,
//         Mid: -2,
//         Presence: -2,
//         HighMid: 0,
//         Treble: 1,
//         Air: 1,
//     },
//     TrebleBoost: {
//         SubBass: -2,
//         Bass: -1,
//         Low: 0,
//         LowMid: 1,
//         Mid: 1,
//         Presence: 3,
//         HighMid: 4,
//         Treble: 5,
//         Air: 3,
//     },
//     MysticVoice: {
//         SubBass: -4,
//         Bass: -2,
//         Low: 1,
//         LowMid: 3,
//         Mid: 4,
//         Presence: 3,
//         HighMid: 2,
//         Treble: 1,
//         Air: 2,
//     },
//     DeepMystery: {
//         SubBass: 3,
//         Bass: 2,
//         Low: 1,
//         LowMid: -1,
//         Mid: 1,
//         Presence: 2,
//         HighMid: 1,
//         Treble: -1,
//         Air: 0,
//     },
// }
// const MOOD_OPTIONS = [
//     { value: 'Mysterious', label: 'Mysterious' },
//     { value: 'Eerie', label: ' Eerie' },
// ]
// const EpisodeAudio: React.FC<EpisodeAudioProps> = ({ initialAudio }) => {
//     const { episodeId } = useParams<{ episodeId: string }>();
//     const ctx = useContext(EpisodeDetailViewContext);
//     const authSlice = ctx?.authSlice;
//     const episodeDetail = ctx?.episodeDetail;
//     const refreshEpisode = ctx?.refreshEpisode;
//     // ============ REFS ============
//     const waveformRefOriginal = useRef<HTMLDivElement>(null)
//     const waveformRefPreview = useRef<HTMLDivElement>(null)
//     const wavesurferRefOriginal = useRef<WaveSurfer | null>(null)
//     const wavesurferRefPreview = useRef<WaveSurfer | null>(null)
//     const fileInputRef = useRef<HTMLInputElement>(null)
//     const progressBarRefOriginal = useRef<HTMLDivElement>(null)
//     const progressBarRefPreview = useRef<HTMLDivElement>(null)

//     // ============ STATE ============
//     const [uploadedFile, setUploadedFile] = useState<File | null>(null)
//     const [audioUrl, setAudioUrl] = useState<string | null>(initialAudio || null)
//     const [previewUrl, setPreviewUrl] = useState<string | null>(null)
//     const [previewFile, setPreviewFile] = useState<File | null>(null)
//     const [isDragging, setIsDragging] = useState(false)

//     // Original audio player state
//     const [isPlayingOriginal, setIsPlayingOriginal] = useState(false)
//     const [currentTimeOriginal, setCurrentTimeOriginal] = useState(0)
//     const [durationOriginal, setDurationOriginal] = useState(0)
//     const [isSeekingOriginal, setIsSeekingOriginal] = useState(false)

//     // Preview audio player state
//     const [isPlayingPreview, setIsPlayingPreview] = useState(false)
//     const [currentTimePreview, setCurrentTimePreview] = useState(0)
//     const [durationPreview, setDurationPreview] = useState(0)
//     const [isSeekingPreview, setIsSeekingPreview] = useState(false)

//     // Sidebar state
//     const [backgroundSounds, setBackgroundSounds] = useState<BackgroundSound[]>([])
//     const [selectedBgSound, setSelectedBgSound] = useState<string>("")
//     const [bgInfo, setBgInfo] = useState<BackgroundSound | null>(null)
//     const [bgSoundFile, setBgSoundFile] = useState<string | null>(null)
//     const [bgSoundVolume, setBgSoundVolume] = useState(0)
//     const [showBgSoundSelector, setShowBgSoundSelector] = useState(false)
//     const [showMoodSelector, setShowMoodSelector] = useState(false)

//     // EQ state
//     const [eqConfig, setEqConfig] = useState(presets["Flat"])
//     const [selectedPreset, setSelectedPreset] = useState("Flat")
//     const [selectedMood, setSelectedMood] = useState("")

//     const [currentFileSource, setCurrentFileSource] = useState<'server' | 'local'>('server');
//     const [previewReady, setPreviewReady] = useState(false);
//     const [lastPreviewSignature, setLastPreviewSignature] = useState<{
//         fileName: string;
//         fileSize: number;
//         eq: Record<string, number>;
//         mood: string;
//         bg: string;
//         vol: number;
//     } | null>(null);

//     const [loading, setLoading] = useState(false);
//     const [previewLoading, setPreviewLoading] = useState(false);
//     const [saving, setSaving] = useState(false);
//     const [saveChoiceOpen, setSaveChoiceOpen] = useState(false);

//     const { startPolling } = useSagaPolling({
//         timeoutSeconds: 200,
//         intervalSeconds: 5,
//     })


//     const fetchBackgroundSounds = async () => {
//         setLoading(true);
//         try {
//             const res = await getBackgroundSounds(loginRequiredAxiosInstance);
//             console.log("Fetched bg list:", res.data.BackgroundSoundTrackList);
//             if (res.success && res.data) {
//                 setBackgroundSounds(res.data.BackgroundSoundTrackList || []);
//             } else {
//                 console.error('API Error:', res.message);
//             }
//         } catch (error) {
//             console.error('Lỗi khi fetch show detail:', error);
//         } finally {
//             setLoading(false);
//         }
//     }

//     useEffect(() => {
//         const loadServerAudio = async () => {
//             if (!episodeDetail?.AudioFileKey) {
//                 setAudioUrl(null);
//                 setUploadedFile(null);
//                 return;
//             }
//             try {
//                 const res = await getAudioFile(loginRequiredAxiosInstance, episodeDetail.AudioFileKey);
//                 if (res.success && res.data?.FileUrl) {
//                     setAudioUrl(res.data.FileUrl);
//                     const blob = await fetch(res.data.FileUrl).then(r => r.blob());
//                     const fileName = buildEpisodeAudioFileName(episodeDetail, blob.type);
//                     setUploadedFile(new File([blob], fileName, { type: blob.type }));
//                     setCurrentFileSource('server');
//                     setPreviewReady(false);
//                     setLastPreviewSignature(null);
//                 }
//             } catch (e) {
//                 toast.error('Load audio failed');
//             }
//         };
//         loadServerAudio();
//         fetchBackgroundSounds()
//     }, [episodeDetail?.AudioFileKey]);

//     const fetchBackgroundSoundAudio = async (fileKey: string) => {
//         const bgSound = backgroundSounds.find(bg => bg.AudioFileKey === fileKey) || null;
//         setBgInfo(bgSound);
//         try {
//             setSelectedBgSound(fileKey);
//             const response = await getBackgroundSoundFile(loginRequiredAxiosInstance, fileKey)
//             if (response.success && response.data) {
//                 setBgSoundFile(response.data.FileUrl)
//             }
//         } catch (error) {
//             console.error('Error fetching PDF:', error)
//             toast.error('Failed to load PDF')
//         }
//     }

//     const handlePresetChange = (preset: string) => {
//         setSelectedPreset(preset)
//         setEqConfig(presets[preset])
//     }
//     const handleMoodChange = (mood: string) => {
//         setSelectedMood(mood)
//     }

//     // ============ CHANGE TRACKING FOR PREVIEW ============
//     const baselineRef = useRef<{
//         eqConfig: Record<string, number>
//         selectedMood: string
//         selectedBgSound: string
//         bgSoundVolume: number
//         moodEnabled: boolean
//     } | null>(null)
//     const [baselineTick, setBaselineTick] = useState(0)

//     useEffect(() => {
//         if (!audioUrl) {
//             baselineRef.current = null
//         } else {
//             baselineRef.current = {
//                 eqConfig: { ...eqConfig },
//                 selectedMood,
//                 selectedBgSound,
//                 bgSoundVolume,
//                 moodEnabled: showMoodSelector,
//             }
//         }
//         setBaselineTick((t) => t + 1)
//     }, [audioUrl])

//     const isEqEqual = (a: Record<string, number>, b: Record<string, number>) => {
//         const keys = new Set([...Object.keys(a || {}), ...Object.keys(b || {})])
//         for (const k of keys) {
//             if ((a?.[k] ?? 0) !== (b?.[k] ?? 0)) return false
//         }
//         return true
//     }

//     const hasPreviewChanges = useMemo(() => {
//         if (!audioUrl) return false
//         const base = baselineRef.current
//         if (!base) return false
//         return (
//             !isEqEqual(eqConfig, base.eqConfig) ||
//             selectedMood !== base.selectedMood ||
//             selectedBgSound !== base.selectedBgSound ||
//             bgSoundVolume !== base.bgSoundVolume ||
//             showMoodSelector !== base.moodEnabled
//         )
//     }, [audioUrl, eqConfig, selectedMood, selectedBgSound, bgSoundVolume, showMoodSelector, baselineTick])
//     // ============ WAVEFORM INITIALIZATION ============
//     useEffect(() => {
//         if (!waveformRefOriginal.current) return

//         const ws = WaveSurfer.create({
//             container: waveformRefOriginal.current,
//             waveColor: "#7BA225",
//             progressColor: "#AEE339",
//             cursorColor: "#AEE339",
//             barWidth: 2,
//             barGap: 2,
//             fillParent: true,
//             minPxPerSec: 30,
//             barRadius: 2,
//             height: 130,
//             interact: false,
//             hideScrollbar: true,
//         })

//         wavesurferRefOriginal.current = ws

//         if (audioUrl) {
//             ws.load(audioUrl)
//         }

//         ws.on("ready", () => {
//             setDurationOriginal(ws.getDuration())
//         })

//         ws.on("play", () => setIsPlayingOriginal(true))
//         ws.on("pause", () => setIsPlayingOriginal(false))
//         ws.on("finish", () => {
//             setIsPlayingOriginal(false)
//             setCurrentTimeOriginal(0)
//         })

//         return () => {
//             ws.destroy()
//         }
//     }, [audioUrl])

//     useEffect(() => {
//         return () => {
//             if (previewUrl) URL.revokeObjectURL(previewUrl)
//         }
//     }, [previewUrl])

//     useEffect(() => {
//         if (!waveformRefPreview.current || !previewUrl) return

//         const ws = WaveSurfer.create({
//             container: waveformRefPreview.current,
//             waveColor: "#7BA225",
//             progressColor: "#AEE339",
//             cursorColor: "#AEE339",
//             barWidth: 2,
//             barGap: 2,
//             fillParent: true,
//             minPxPerSec: 30,
//             barRadius: 2,
//             height: 130,
//             interact: false,
//             hideScrollbar: true,
//         })

//         wavesurferRefPreview.current = ws
//         ws.load(previewUrl)

//         ws.on("ready", () => {
//             setDurationPreview(ws.getDuration())
//         })

//         ws.on("play", () => setIsPlayingPreview(true))
//         ws.on("pause", () => setIsPlayingPreview(false))
//         ws.on("finish", () => {
//             setIsPlayingPreview(false)
//             setCurrentTimePreview(0)
//         })

//         return () => {
//             ws.destroy()
//         }
//     }, [previewUrl])

//     // ============ TIME UPDATE ANIMATION FRAMES ============
//     useEffect(() => {
//         let animationFrameId: number

//         const updateTime = () => {
//             if (wavesurferRefOriginal.current?.isPlaying()) {
//                 setCurrentTimeOriginal(wavesurferRefOriginal.current.getCurrentTime())
//             }
//             if (wavesurferRefPreview.current?.isPlaying()) {
//                 setCurrentTimePreview(wavesurferRefPreview.current.getCurrentTime())
//             }
//             animationFrameId = requestAnimationFrame(updateTime)
//         }

//         animationFrameId = requestAnimationFrame(updateTime)
//         return () => cancelAnimationFrame(animationFrameId)
//     }, [])

//     // ============ SEEKING HANDLERS ============
//     const handleSeekMouseDown = (e: React.MouseEvent<HTMLDivElement>, isPreview: boolean) => {
//         if (isPreview) {
//             const rect = progressBarRefPreview.current.getBoundingClientRect();
//             const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1);
//             const newTime = percent * durationPreview;

//             // Xử lý click ngay lập tức
//             wavesurferRefPreview.current.setTime(newTime);
//             setCurrentTimePreview(newTime);
//             setIsSeekingPreview(true)
//         } else {
//             const rect = progressBarRefOriginal.current.getBoundingClientRect();
//             const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1);
//             const newTime = percent * durationOriginal;

//             // Xử lý click ngay lập tức
//             wavesurferRefOriginal.current.setTime(newTime);
//             setIsSeekingOriginal(true)
//         }
//     }

//     useEffect(() => {
//         const handleMouseMove = (e: MouseEvent) => {
//             if (isSeekingOriginal && progressBarRefOriginal.current && wavesurferRefOriginal.current) {
//                 const rect = progressBarRefOriginal.current.getBoundingClientRect()
//                 const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1)
//                 const newTime = percent * durationOriginal
//                 wavesurferRefOriginal.current.setTime(newTime)
//                 setCurrentTimeOriginal(newTime)
//             }

//             if (isSeekingPreview && progressBarRefPreview.current && wavesurferRefPreview.current) {
//                 const rect = progressBarRefPreview.current.getBoundingClientRect()
//                 const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1)
//                 const newTime = percent * durationPreview
//                 wavesurferRefPreview.current.setTime(newTime)
//                 setCurrentTimePreview(newTime)
//             }
//         }

//         const handleMouseUp = () => {
//             setIsSeekingOriginal(false)
//             setIsSeekingPreview(false)
//         }

//         if (isSeekingOriginal || isSeekingPreview) {
//             window.addEventListener("mousemove", handleMouseMove)
//             window.addEventListener("mouseup", handleMouseUp)
//         }

//         return () => {
//             window.removeEventListener("mousemove", handleMouseMove)
//             window.removeEventListener("mouseup", handleMouseUp)
//         }
//     }, [isSeekingOriginal, isSeekingPreview, durationOriginal, durationPreview])

//     // ============ FILE HANDLING ============
//     const resetForNewFile = (file: File) => {
//         const url = URL.createObjectURL(file);
//         setAudioUrl(url);
//         setUploadedFile(file);
//         setCurrentFileSource('local');

//         // Always reset preview-related state
//         setPreviewReady(false);
//         setPreviewUrl(null);
//         setPreviewFile(null);
//         setLastPreviewSignature(null);
//         baselineRef.current = null;
//         setBaselineTick(t => t + 1);

//         setEqConfig(presets["Flat"]);
//         setSelectedPreset("Flat");
//         setSelectedMood("");
//         setShowMoodSelector(false);
//         setSelectedBgSound("");
//         setBgSoundFile(null);
//         setBgSoundVolume(0);
//         setShowBgSoundSelector(false);

//     };

//     const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
//         const file = e.target.files?.[0];
//         if (!file) return;


//         if (file.size > 150 * 1024 * 1024) {
//             toast.error("File size exceeds 150MB limit.");
//             return;
//         }
//         if (!file.type.startsWith('audio/')) {
//             toast.error("Unsupported file type.");
//             return;
//         }
//         resetForNewFile(file);
//     };

//     const handleDragOver = (e: React.DragEvent) => {
//         e.preventDefault()
//         setIsDragging(true)
//     }

//     const handleDragLeave = () => setIsDragging(false)

//     const handleDrop = (e: React.DragEvent) => {
//         e.preventDefault()
//         setIsDragging(false)
//         const file = e.dataTransfer.files?.[0]
//         if (!file) return

//         if (file.size > 150 * 1024 * 1024) {
//             alert("File size exceeds 150MB limit.")
//             return
//         }

//         if (file.type.startsWith('audio/')) {
//             const url = URL.createObjectURL(file);
//             setAudioUrl(url);
//             setUploadedFile(file);
//             setCurrentFileSource('local');
//             setPreviewReady(false);
//             setPreviewUrl(null);
//             setPreviewFile(null);
//             setLastPreviewSignature(null);
//         }
//     }
//     const currentSignature = useMemo(() => {
//         if (!uploadedFile) return null;
//         return {
//             fileName: uploadedFile.name,
//             fileSize: uploadedFile.size,
//             eq: eqConfig,
//             mood: selectedMood,
//             bg: selectedBgSound,
//             vol: bgSoundVolume,
//         };
//     }, [uploadedFile, eqConfig, selectedMood, selectedBgSound, bgSoundVolume]);

//     const hasUnsavedPreview = useMemo(() => {
//         if (!previewReady || !lastPreviewSignature || !currentSignature) return false;
//         const diff =
//             lastPreviewSignature.fileName !== currentSignature.fileName ||
//             lastPreviewSignature.fileSize !== currentSignature.fileSize ||
//             !isEqEqual(lastPreviewSignature.eq, currentSignature.eq) ||
//             lastPreviewSignature.mood !== currentSignature.mood ||
//             lastPreviewSignature.bg !== currentSignature.bg ||
//             lastPreviewSignature.vol !== currentSignature.vol;
//         return diff;
//     }, [previewReady, lastPreviewSignature, currentSignature]);

//     // ============ UTILITY FUNCTIONS ============
//     const formatTime = (seconds: number) => {
//         const mins = Math.floor(seconds / 60)
//         const secs = Math.floor(seconds % 60)
//         return `${mins}:${secs.toString().padStart(2, "0")}`
//     }

//     const handlePlayPause = (isPreview: boolean) => {
//         if (isPreview) {
//             wavesurferRefPreview.current?.playPause()
//         } else {
//             wavesurferRefOriginal.current?.playPause()
//         }
//     }

//     const handleAddBackgroundSound = () => {
//         if (showBgSoundSelector) {
//             setSelectedBgSound("")
//             setBgSoundFile(null)
//             setBgSoundVolume(0)
//         }
//         setShowBgSoundSelector(!showBgSoundSelector)
//     }

//     const handleAddMood = () => {
//         setShowMoodSelector((prev) => {
//             const next = !prev
//             if (next) {
//                 if (!selectedMood) setSelectedMood('Mysterious')
//             } else {
//                 setSelectedMood('')
//             }
//             return next
//         })
//     }

//     const handlePreview = async () => {
//         if (authSlice.user?.ViolationLevel > 0) {
//             toast.error('Your account is currently under violation !!');
//             return;
//         }
//         if (!uploadedFile) return;
//         setPreviewLoading(true)
//         const payload = {
//             GeneralTuningProfileRequestInfo: {
//                 EqualizerProfile: {
//                     ExpandEqualizer: { Mood: selectedMood },
//                     BaseEqualizer: {
//                         HighMid: eqConfig.HighMid,
//                         Low: eqConfig.Low,
//                         LowMid: eqConfig.LowMid,
//                         Mid: eqConfig.Mid,
//                         Presence: eqConfig.Presence,
//                         SubBass: eqConfig.SubBass,
//                         Treble: eqConfig.Treble,
//                         Air: eqConfig.Air,
//                         Bass: eqConfig.Bass
//                     }
//                 },
//                 BackgroundMergeProfile: {
//                     BackgroundSoundTrackFileKey: selectedBgSound,
//                     VolumeGainDb: bgSoundVolume
//                 },
//                 AITuningProfile: null,
//             },
//             AudioFile: uploadedFile
//         }
//         console.log("Audio Tuning Payload:", payload)
//         const response = await audioTuning(loginRequiredAxiosInstance, episodeId, payload)
//         console.log("Audio Tuning Response:", response)
//         if (response.success && response.data) {
//             const blob = response.data;
//             const file = new File([blob], "preview-audio.mp3", { type: blob.type || "audio/mpeg" });
//             const url = URL.createObjectURL(blob);
//             setPreviewUrl(url);
//             setPreviewFile(file);
//             setPreviewReady(true);
//             setLastPreviewSignature(currentSignature); // chốt snapshot
//         } else {
//             toast.error(response.message.content || "Thất bại, vui lòng thử lại !")
//         }
//         setPreviewLoading(false);
//         baselineRef.current = {
//             eqConfig: { ...eqConfig },
//             selectedMood,
//             selectedBgSound,
//             bgSoundVolume,
//             moodEnabled: showMoodSelector,
//         }
//         setBaselineTick((t) => t + 1)
//         console.log('Previewing audio...', { eqConfig, selectedMood, selectedBgSound, bgSoundVolume })
//     }

//     const performSave = async (fileToSave: File) => {
//         if (authSlice.user?.ViolationLevel > 0) {
//             toast.error('Your account is currently under violation !!');
//             return;
//         }
//         try {
//             setSaving(true);
//             const payload = { AudioFile: fileToSave };
//             console.log("Upload Audio Payload:", payload);
//             const res = await uploadAudio(loginRequiredAxiosInstance, episodeId, payload);
//             const sagaId = res?.data?.SagaInstanceId;
//             if (!res.success && res.message.content) {
//                 toast.error(res.message.content);
//                 return;
//             }
//             if (!sagaId) {
//                 toast.error('Saving episode failed, please try again.');
//                 return;
//             }
//             await startPolling(sagaId, loginRequiredAxiosInstance, {
//                 onSuccess: async () => {
//                     toast.success('Audio saved successfully.');
//                     await refreshEpisode?.();
//                 },
//                 onFailure: (err) => toast.error(err || 'Saga failed!'),
//                 onTimeout: () => toast.error('System not responding, please try again.'),
//             });
//         } catch (error) {
//             toast.error('Error saving audio');
//         } finally {
//             setSaving(false);
//         }
//     };

//     const handleSaveClick = async () => {
//         if (authSlice.user?.ViolationLevel > 0) {
//             toast.error('Your account is currently under violation !!');
//             return;
//         }
//         if (episodeDetail && episodeDetail.CurrentStatus.Id === 5) {
//             toast.error("Cannot change audio for Published episodes, please Unpublish first");
//             return;
//         }
//         if (episodeDetail && episodeDetail.CurrentStatus.Id === 4) {
//             const alert = await confirmAlert("If you save changes now, you must Request to Publish again. Do you want to continue?");
//             if (!alert.isConfirmed) return;
//         }
//         if (episodeDetail && episodeDetail.CurrentStatus.Id === 2) {
//             toast.error("This episode is Pending Review, please Discard Publish Request first");
//             return;
//         }

//         if (saving || previewLoading) return;
//         if (saveDisabled) {
//             if (currentFileSource === 'server') {
//                 toast.info('Preview the server audio before saving.');
//             }
//             return;
//         }
//         if (!uploadedFile) return;

//         // Server file: luôn save bản preview (đã được đảm bảo hợp lệ bởi saveDisabled)
//         if (currentFileSource === 'server') {
//             performSave(previewFile!);
//             return;
//         }

//         // Local file: nếu có preview hợp lệ thì hỏi chọn; nếu chưa preview hoặc preview đã outdated thì lưu bản gốc
//         const canChoose = previewReady && previewFile && !hasUnsavedPreview;
//         if (canChoose) {
//             setSaveChoiceOpen(true);
//         } else {
//             performSave(uploadedFile);
//         }
//     };

//     const handleProgressClick = (e: React.MouseEvent<HTMLDivElement>, isPreview: boolean) => {
//         if (isPreview) {
//             if (!wavesurferRefPreview.current || !progressBarRefPreview.current) return;
//             const rect = progressBarRefPreview.current.getBoundingClientRect();
//             const clickX = e.clientX - rect.left;
//             const percent = clickX / rect.width;
//             const newTime = percent * durationPreview;
//             wavesurferRefPreview.current.setTime(newTime);
//             setCurrentTimePreview(newTime);
//         } else {
//             if (!wavesurferRefOriginal.current || !progressBarRefOriginal.current) return;
//             const rect = progressBarRefOriginal.current.getBoundingClientRect();
//             const clickX = e.clientX - rect.left;
//             const percent = clickX / rect.width;
//             const newTime = percent * durationOriginal;
//             wavesurferRefOriginal.current.setTime(newTime);
//             setCurrentTimeOriginal(newTime);
//         }
//     };
//     // ============ RENDER ============
//     const handleResetAudio = (isPreview: boolean) => {
//         if (isPreview) {
//             wavesurferRefPreview.current?.setTime(0);
//             setCurrentTimePreview(0);
//         } else {
//             wavesurferRefOriginal.current?.setTime(0);
//             setCurrentTimeOriginal(0);
//         }
//     };
//     const previewDisabled =
//         !uploadedFile ||
//         previewLoading ||
//         saving ||
//         !hasPreviewChanges;


//     const saveDisabled = useMemo(() => {
//         if (!uploadedFile) return true; // rule 1
//         if (currentFileSource === 'server') {
//             // Chỉ được save khi đã render preview hợp lệ
//             if (!previewReady) return true;
//             if (!previewFile) return true;
//             if (hasUnsavedPreview) return true;
//             return false; // có preview hợp lệ
//         }


//         // local file: luôn có thể save
//         return false;
//     }, [uploadedFile, currentFileSource, previewReady, previewFile, hasUnsavedPreview, episodeDetail]);
//     const shortenFileName = (name: string, max = 32) => {
//         if (!name) return "";
//         if (name.length <= max) return name;
//         const match = name.match(/^(.*?)(\.[^.]+)$/);
//         const base = match ? match[1] : name;
//         const ext = match ? match[2] : "";
//         const room = max - ext.length - 5; // 5 cho "...".
//         const head = base.slice(0, Math.ceil(room / 2));
//         const tail = base.slice(-Math.floor(room / 2));
//         return `${head}...${tail}${ext}`;
//     };

//     if (!episodeDetail) {
//         return (
//             <div className="flex justify-center items-center h-100">
//                 <Loading />
//             </div>
//         );
//     }
//     return (
//         <div className="episode-audio">

//             {/* ============ MAIN CONTENT (LEFT) ============ */}
//             <div className="episode-audio__content">
//                 {episodeDetail && episodeDetail.CurrentStatus?.Id === 8 && (
//                     <div
//                         className="flex items-center gap-2 bg-[#29b6f626] border border-[#61a7f2ff] rounded-xs px-3 py-2 mb-3"
//                         style={{ width: "fit-content" }}
//                     >
//                         <svg className="w-5 h-5 text-[#61a7f2ff] shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
//                             <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
//                         </svg>
//                         <span className="text-sm text-[#61a7f2ff] font-medium">
//                             <strong>Your episode audio is being processed, it will be available soon</strong>
//                         </span>
//                     </div>
//                 )}
//                 {episodeDetail && episodeDetail.CurrentStatus?.Id === 3 && (
//                     <div className="flex items-center gap-2 bg-red-100 border border-red-400  rounded px-3 py-2 mb-3 " style={{ width: "fit-content" }}>
//                         <svg className="w-5 h-5 text-red-500 shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
//                             <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
//                         </svg>
//                         <span className="text-xs text-red-700 font-medium">
//                             <strong>Your episode is being required to edit, please upload new audio</strong>
//                         </span>
//                     </div>
//                 )}

//                 {/* Original Audio Section */}
//                 {audioUrl && (
//                     <div className="episode-audio__player-section">
//                         <h3 className="episode-audio__player-title">Original Audio</h3>
//                         <div className="episode-audio__waveform-container">
//                             <div ref={waveformRefOriginal} className="episode-audio__waveform" />
//                         </div>
//                         <div className="episode-audio__controls">
//                             <div className="episode-audio__controls-left">
//                                 <IconButton onClick={() => handlePlayPause(false)} className="episode-audio__play-btn">
//                                     {isPlayingOriginal ? <Pause /> : <PlayArrow />}
//                                 </IconButton>
//                                 <IconButton onClick={() => handleResetAudio(false)} className="episode-audio__play-btn">
//                                     <ArrowCounterClockwise size={26} weight="bold" />
//                                 </IconButton>
//                                 <span className="episode-audio__time-display">{formatTime(currentTimeOriginal)}</span>
//                             </div>

//                             <div
//                                 className="episode-audio__progress-bar"
//                                 ref={progressBarRefOriginal}
//                                 onMouseDown={(e) => handleSeekMouseDown(e, false)}
//                                 onClick={(e) => handleProgressClick(e, false)}
//                             >
//                                 <div
//                                     className="episode-audio__progress-fill"
//                                     style={{
//                                         width: `${(currentTimeOriginal / durationOriginal) * 100}%`,
//                                     }}
//                                 />
//                                 <div
//                                     className="episode-audio__progress-thumb"
//                                     style={{
//                                         left: `${(currentTimeOriginal / durationOriginal) * 100}%`,
//                                     }}
//                                 />
//                             </div>

//                             <div className="episode-audio__controls-right">
//                                 <span className="episode-audio__time-display">{formatTime(durationOriginal)}</span>
//                             </div>
//                         </div>
//                     </div>
//                 )}

//                 {/* Preview Audio Section */}
//                 {previewUrl && (
//                     <div className="episode-audio__player-section">
//                         <h3 className="episode-audio__player-title">Preview Audio</h3>
//                         <div className="episode-audio__waveform-container">
//                             <div ref={waveformRefPreview} className="episode-audio__waveform" />
//                         </div>
//                         <div className="episode-audio__controls">
//                             <div className="episode-audio__controls-left">
//                                 <IconButton onClick={() => handlePlayPause(true)} className="episode-audio__play-btn">
//                                     {isPlayingPreview ? <Pause /> : <PlayArrow />}
//                                 </IconButton>
//                                 <IconButton onClick={() => handleResetAudio(true)} className="episode-audio__play-btn">
//                                     <ArrowCounterClockwise size={26} weight="bold" />
//                                 </IconButton>
//                                 <span className="episode-audio__time-display">{formatTime(currentTimePreview)}</span>
//                             </div>

//                             <div
//                                 className="episode-audio__progress-bar"
//                                 ref={progressBarRefPreview}
//                                 onMouseDown={(e) => handleSeekMouseDown(e, true)}
//                                 onClick={(e) => handleProgressClick(e, true)}
//                             >
//                                 <div
//                                     className="episode-audio__progress-fill"
//                                     style={{
//                                         width: `${(currentTimePreview / durationPreview) * 100}%`,
//                                     }}
//                                 />
//                                 <div
//                                     className="episode-audio__progress-thumb"
//                                     style={{
//                                         left: `${(currentTimePreview / durationPreview) * 100}%`,
//                                     }}
//                                 />
//                             </div>

//                             <div className="episode-audio__controls-right">
//                                 <span className="episode-audio__time-display">{formatTime(durationPreview)}</span>
//                             </div>
//                         </div>
//                     </div>
//                 )}

//                 {/* EQ Table Section */}
//                 {audioUrl && (
//                     <div className="episode-audio__eq-table">
//                         {Object.keys(eqConfig).map((band) => (
//                             <div key={band} className="episode-audio__eq-band">
//                                 <div className="episode-audio__eq-slider-container">
//                                     <div className="eq-slider-marks eq-slider-marks--left">
//                                         <span /><span /><span /><span /><span />
//                                     </div>
//                                     {/* Gạch phải */}
//                                     <div className="eq-slider-marks eq-slider-marks--right">
//                                         <span /><span /><span /><span /><span />
//                                     </div>
//                                     <input
//                                         className="episode-audio__eq-slider"
//                                         type="range"
//                                         min="-10"
//                                         max="10"
//                                         step="1"
//                                         value={eqConfig[band]}
//                                         onChange={(e) => setEqConfig({ ...eqConfig, [band]: Number.parseInt(e.target.value) })}
//                                     />
//                                 </div>
//                                 <div className="episode-audio__eq-band-label">{band}</div>
//                                 <div className="episode-audio__eq-band-value">{eqConfig[band]} dB</div>
//                             </div>
//                         ))}
//                     </div>
//                 )}

//                 {/* Empty State */}
//                 {!audioUrl && (
//                     <div className="episode-audio__empty-state">
//                         <div className="episode-audio__empty-state__icon">
//                             <Music size={48} />
//                         </div>
//                         <p className="episode-audio__empty-state__text">Upload an audio file to get started</p>
//                     </div>
//                 )}
//             </div>

//             {/* ============ SIDEBAR (RIGHT) ============ */}
//             <div className="episode-audio__sidebar">
//                 {/* Audio Upload */}
//                 <div className="episode-audio__upload-box">
//                     <h3 className="episode-audio__section-title">Audio Upload</h3>
//                     {episodeDetail && episodeDetail.CurrentStatus?.Id !== 8 && (
//                         <div
//                             className={`episode-audio__upload-area ${isDragging ? "episode-audio__upload-area--dragging" : ""}`}
//                             onDragOver={handleDragOver}
//                             onDragLeave={handleDragLeave}
//                             onDrop={handleDrop}
//                             onClick={() => fileInputRef.current?.click()}
//                         >
//                             <Music className="episode-audio__upload-icon" size={40} />
//                             <p className="episode-audio__upload-text">Drop your audio file here</p>
//                             <p className="episode-audio__upload-subtext">or click to browse</p>
//                             <input
//                                 ref={fileInputRef}
//                                 type="file"
//                                 accept="audio/*"
//                                 onChange={handleFileSelect}
//                                 className="episode-audio__upload-input"
//                             />
//                         </div>
//                     )}


//                     {uploadedFile && (
//                         <div className="episode-audio__file-info">
//                             <div className="episode-audio__file-info__row">
//                                 <FolderSimple size={20} color="#B6E04A" />
//                                 <span
//                                     className="episode-audio__file-info__label"
//                                     title={uploadedFile.name}
//                                 >
//                                     {shortenFileName(uploadedFile.name, 30)}
//                                 </span>
//                                 <IconButton
//                                     size="small"
//                                     onClick={() => {
//                                         navigator.clipboard.writeText(uploadedFile.name);
//                                         toast.success("Copied filename");
//                                     }}
//                                     className="episode-audio__file-info__copy"
//                                 >
//                                     <ContentCopy fontSize="inherit" />
//                                 </IconButton>
//                             </div>
//                             <div className="episode-audio__file-info__row">
//                                 <Database size={20} color="#B6E04A" />
//                                 <span className="episode-audio__file-info__value">
//                                     {(uploadedFile.size / 1024 / 1024).toFixed(2)} MB
//                                 </span>
//                             </div>
//                         </div>
//                     )}
//                 </div>

//                 {/* Audio Style */}
//                 <div style={{ borderBottom: "2px solid var(--border-grey)" }}>
//                     <h3 className="episode-audio__section-title mb-3">Audio Style</h3>

//                     <div className="episode-audio__selector-group">
//                         <div className="flex justify-between items-center">
//                             <label className="episode-audio__selector-label">EQ Preset</label>
//                             <Tooltip placement="top-start" title="Quickly adjust the EQ bands to shape your sound">
//                                 <Question color="var(--third-grey)" size={16} />
//                             </Tooltip >
//                         </div>
//                         <Select
//                             value={selectedPreset}
//                             onChange={(e) => handlePresetChange(e.target.value as string)}
//                             displayEmpty
//                             variant="outlined"
//                             className="episode-audio__selector"
//                             disabled={!audioUrl}
//                             sx={{
//                                 '& .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--border-grey)' },
//                                 '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--primary-green)' },
//                                 '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--primary-green)' },
//                                 '& .MuiSelect-icon': { color: 'var(--primary-green)' },
//                             }}
//                         >
//                             {Object.keys(presets).map((preset) => (
//                                 <MenuItem key={preset} value={preset}>
//                                     {preset === "MysticVoice" ? " Mystic Voice" : preset === "DeepMystery" ? " Deep Mystery" : preset}
//                                 </MenuItem>
//                             ))}
//                         </Select>
//                     </div>

//                     <div className={`episode-audio__selector-group ${showMoodSelector ? 'pb-8' : 'pb-2'}`}>
//                         <div className="flex justify-between items-center">
//                             <div className="flex items-center gap-2">
//                                 <label className="episode-audio__selector-label">Mood</label>
//                                 <Tooltip placement="top-start" title="Combine EQ and audio filters to create a unique atmosphere">
//                                     <Question color="var(--third-grey)" size={16} />
//                                 </Tooltip >
//                             </div>
//                             <IconButton
//                                 className="episode-audio__add-btn"
//                                 onClick={handleAddMood}
//                                 title={showMoodSelector ? "Close" : "Add mood"}
//                                 aria-label={showMoodSelector ? "Close mood" : "Add mood"}
//                                 disabled={!audioUrl}
//                             >
//                                 {showMoodSelector ? <Minus color="white" size={20} /> : <Plus size={20} />}
//                             </IconButton>
//                         </div>
//                         {showMoodSelector && (
//                             <>
//                                 <Select
//                                     value={selectedMood}
//                                     onChange={(e) => handleMoodChange(e.target.value as string)}
//                                     variant="outlined"
//                                     displayEmpty
//                                     className="episode-audio__selector"
//                                     disabled={!audioUrl}
//                                     sx={{
//                                         '& .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--border-grey)' },
//                                         '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--primary-green)' },
//                                         '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--primary-green)' },
//                                         '& .MuiSelect-icon': { color: 'var(--primary-green)' },
//                                     }}
//                                 >
//                                     {MOOD_OPTIONS.map(m => (
//                                         <MenuItem key={m.value} value={m.value}>{m.label}</MenuItem>
//                                     ))}
//                                 </Select>
//                             </>
//                         )}
//                     </div>
//                 </div>

//                 {/* Background Sound */}
//                 <div className="episode-audio__background-sound ">
//                     <div className="episode-audio__background-sound-header">
//                         <div className="flex items-center gap-2">
//                             <h3 className={`episode-audio__section-title ${showBgSoundSelector ? '' : 'episode-audio__section-title--muted'}`}>
//                                 Background Sound
//                             </h3>
//                             <Tooltip placement="top-start" title="Add a background sound that plays throughout your entire audio">
//                                 <Question color="var(--third-grey)" size={16} />
//                             </Tooltip >
//                         </div>
//                         <IconButton
//                             className="episode-audio__add-btn"
//                             onClick={handleAddBackgroundSound}
//                             title={showBgSoundSelector ? "Close" : "Add background sound"}
//                             aria-label={showBgSoundSelector ? "Close background sound" : "Add background sound"}
//                             disabled={!audioUrl}
//                         >
//                             {showBgSoundSelector ? <Minus color="white" size={20} /> : <Plus size={20} />}
//                         </IconButton>
//                     </div>

//                     {showBgSoundSelector && (
//                         <>
//                             <Select
//                                 variant="outlined"
//                                 className="episode-audio__selector"
//                                 value={selectedBgSound}
//                                 displayEmpty
//                                 onChange={(e) => fetchBackgroundSoundAudio(e.target.value as string)}
//                                 disabled={!audioUrl}
//                                 renderValue={(value) => {
//                                     const v = value as string;
//                                     if (!v) return <span style={{ color: 'var(--third-grey)' }}>Select A sound...</span>;
//                                     const bg = backgroundSounds.find(b => b.AudioFileKey === v);
//                                     return bg ? bg.Name : v; // chỉ hiện Name khi đã chọn
//                                 }}
//                                 sx={{
//                                     '& .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--border-grey)' },
//                                     '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--primary-green)' },
//                                     '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--primary-green)' },
//                                     '& .MuiSelect-icon': { color: 'var(--primary-green)' },
//                                 }}
//                             >
//                                 <MenuItem value="">
//                                     <p style={{ color: 'var(--third-grey)' }}>Select A sound...</p>
//                                 </MenuItem>

//                                 {backgroundSounds.map((bg) => (
//                                     <MenuItem key={bg.Id} value={bg.AudioFileKey}>
//                                         <div className="flex items-center">
//                                             <Image
//                                                 mainImageFileKey={`${bg?.MainImageFileKey || ''}`}
//                                                 alt="Background Sound Image"
//                                                 className="w-7 h-7 object-cover rounded-sm mr-2"
//                                             />
//                                             {bg.Name}
//                                         </div>
//                                     </MenuItem>
//                                 ))}
//                             </Select>

//                             {selectedBgSound && (
//                                 <div className="episode-audio__background-sound-volume pt-4">

//                                     <div className="episode-audio__volume-label gap-3">
//                                         <div>
//                                             <span> Volume: </span>
//                                             <span>  {bgSoundVolume.toFixed(1)} dB</span>
//                                         </div>
//                                         <Tooltip placement="top-start" title="Adjust background sound volume after merging (recommended: -1 dB to -8 dB)">
//                                             <Question color="var(--third-grey)" size={16} />
//                                         </Tooltip >
//                                     </div>
//                                     <input
//                                         type="range"
//                                         className="episode-audio__volume-slider"
//                                         min="-10"
//                                         max="10"
//                                         step="0.1"
//                                         value={bgSoundVolume}
//                                         onChange={(e) => setBgSoundVolume(Number.parseFloat(e.target.value))}
//                                         disabled={!audioUrl}
//                                         style={
//                                             {
//                                                 "--value": `${((bgSoundVolume + 20) / 20) * 100}%`,
//                                             } as React.CSSProperties
//                                         }
//                                     />
//                                 </div>
//                             )}

//                             {selectedBgSound && (
//                                 <div className="flex flex-col mt-4 gap-4">
//                                     <div className="flex  gap-4">
//                                         <Image mainImageFileKey={`${bgInfo?.MainImageFileKey || ''}`} alt={'Background Sound Image'} className="w-12 h-12 object-cover rounded-sm" />
//                                         <div className="flex flex-col items-start">
//                                             <p className="text-[#aee339] font-medium text-sm">{bgInfo?.Name}</p>
//                                             <p className="text-[#d9d9d9] text-left font-light text-xs">{bgInfo?.Description}</p>
//                                         </div>

//                                     </div>

//                                     <audio controls src={bgSoundFile || undefined} controlsList="nodownload noplaybackrate" />

//                                 </div>
//                             )}
//                         </>
//                     )}
//                 </div>
//                 {previewLoading || saving ? (
//                     <div className="flex justify-center items-center m-8 ">
//                         <Loading2 title="Audio Processing" />
//                     </div>
//                 ) : (
//                     <>
//                         {episodeDetail && (episodeDetail.CurrentStatus.Id !== 8 && episodeDetail.CurrentStatus.Id !== 7 && episodeDetail.CurrentStatus.Id !== 6) && (
//                             <div className="episode-audio__actions">
//                                 <button className="episode-audio__btn episode-audio__btn--primary"
//                                     onClick={handleSaveClick}
//                                     disabled={saveDisabled}>
//                                     <CloudUpload size={18} />
//                                     Save
//                                 </button>
//                                 <button
//                                     className="episode-audio__btn episode-audio__btn--secondary"
//                                     onClick={handlePreview}
//                                     disabled={previewDisabled}
//                                 >
//                                     <Play size={18} />
//                                     Preview
//                                 </button>

//                             </div>
//                         )}
//                     </>

//                 )}
//                 <Modal open={saveChoiceOpen} onClose={() => setSaveChoiceOpen(false)}>
//                     <div style={{
//                         background: '#171717e6',
//                         padding: '24px',
//                         borderRadius: '12px',
//                         width: '360px',
//                         display: 'flex',
//                         flexDirection: 'column',
//                         gap: '16px',
//                         position: 'absolute',
//                         top: '50%',
//                         left: '50%',
//                         transform: 'translate(-50%, -50%)',
//                         boxShadow: '0 8px 24px rgba(0,0,0,0.4)'
//                     }}>
//                         <h4 style={{ margin: 0, color: 'var(--primary-green)' }}>Choose what you want to save</h4>
//                         <p style={{ margin: 0, fontSize: '0.85rem', color: 'var(--third-grey)' }}>
//                             You can save the original or the processed version.
//                         </p>
//                         {currentFileSource === 'local' ? (
//                             <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
//                                 <button
//                                     className="episode-audio__btn episode-audio__btn--secondary"
//                                     onClick={() => { if (uploadedFile) performSave(uploadedFile); setSaveChoiceOpen(false); }}
//                                     disabled={!uploadedFile || saving}
//                                 >
//                                     Save Original
//                                 </button>
//                                 <button
//                                     className="episode-audio__btn episode-audio__btn--primary"
//                                     onClick={() => { if (previewFile) performSave(previewFile); setSaveChoiceOpen(false); }}
//                                     disabled={!previewFile || saving || hasUnsavedPreview}
//                                 >
//                                     Save Preview
//                                 </button>
//                             </div>
//                         ) : (
//                             // Server source sẽ không hiển thị lựa chọn (chỉ cho save bản preview) – modal không nên mở nhưng fallback
//                             <div>
//                                 <button
//                                     className="episode-audio__btn episode-audio__btn--primary"
//                                     onClick={() => { if (previewFile) performSave(previewFile); setSaveChoiceOpen(false); }}
//                                     disabled={!previewFile || saving || hasUnsavedPreview}
//                                 >
//                                     Save Preview
//                                 </button>
//                             </div>
//                         )}
//                         <button
//                             style={{
//                                 background: 'transparent',
//                                 color: 'var(--third-grey)',
//                                 border: 'none',
//                                 cursor: 'pointer',
//                                 fontSize: '0.75rem',
//                                 alignSelf: 'center'
//                             }}
//                             onClick={() => setSaveChoiceOpen(false)}
//                         >
//                             Cancel
//                         </button>
//                     </div>
//                 </Modal>
//             </div >
//         </div >
//     )
// }

// export default EpisodeAudio
