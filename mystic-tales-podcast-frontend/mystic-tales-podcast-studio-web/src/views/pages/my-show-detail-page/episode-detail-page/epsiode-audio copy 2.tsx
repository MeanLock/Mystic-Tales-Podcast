// import type React from "react"
// import { useEffect, useMemo, useRef, useState } from "react"
// import WaveSurfer from "wavesurfer.js"
// import { Music, Download, Play, Minus } from "lucide-react"
// import { ArrowCounterClockwise, Database, FolderSimple, Plus, Question } from "phosphor-react"
// import { IconButton, MenuItem, Select, Tooltip } from "@mui/material"
// import ghost from "../../../../assets/ghost.mp3"
// import { PlayArrow, Pause } from "@mui/icons-material"
// import { toast } from "react-toastify"
// import { AudioTuning } from "@/core/services/account/account.service"
// import { loginRequiredAxiosInstance, publicAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
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
//     const [eqPreset, setEqPreset] = useState("flat")
//     const [mood, setMood] = useState("Mysterious")
//     const [backgroundSounds, setBackgroundSounds] = useState<string[]>([])
//     const [selectedBgSound, setSelectedBgSound] = useState<string>("")
//     const [bgSoundVolume, setBgSoundVolume] = useState(0)
//     const [showBgSoundSelector, setShowBgSoundSelector] = useState(false)
//     const [showMoodSelector, setShowMoodSelector] = useState(false)

//     // EQ state
//     const [eqConfig, setEqConfig] = useState(presets["Flat"])
//     const [selectedPreset, setSelectedPreset] = useState("Flat")
//     const [selectedMood, setSelectedMood] = useState("Mysterious")
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

//     // Reset baseline when a new audio is loaded: baseline = current state (Preview stays disabled until changes)
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
//     // Preview waveform
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
//     const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
//         const file = event.target.files?.[0]
//         if (!file) return

//         if (file.size > 150 * 1024 * 1024) {
//             alert("File size exceeds 150MB limit.")
//             return
//         }

//         if (file.type.startsWith("audio/")) {
//             const url = URL.createObjectURL(file)
//             setAudioUrl(url)
//             setUploadedFile(file)
//         }
//     }

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

//         if (file.type.startsWith("audio/")) {
//             const url = URL.createObjectURL(file)
//             setAudioUrl(url)
//             setUploadedFile(file)
//         }
//     }

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
//         setShowBgSoundSelector(!showBgSoundSelector)
//     }

//     const handleAddMood = () => {
//         setShowMoodSelector((prev) => {
//             const next = !prev
//             if (next) {
//                 // auto-add a default mood when opening selector
//                 if (!selectedMood) setSelectedMood('balance')
//             } else {
//                 // remove mood when closing selector
//                 setSelectedMood('')
//             }
//             return next
//         })
//     }

//     const handlePreview = async () => {
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
//         const response = await AudioTuning(loginRequiredAxiosInstance, payload)
//         console.log("Audio Tuning Response:", response)
//         if (response.success && response.data) {
//             const blob = response.data;
//             const url = URL.createObjectURL(blob);
//             setPreviewUrl(url);
//         } else {
//             toast.error(response.message.content || "Thất bại, vui lòng thử lại !")
//         }
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

//     const handleSave = () => {
//         alert("Audio saved with current settings!")
//     }

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
//     return (
//         <div className="episode-audio">
//             {/* ============ MAIN CONTENT (LEFT) ============ */}
//             <div className="episode-audio__content">
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
//                    {audioUrl && (
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
//                     <div
//                         className={`episode-audio__upload-area ${isDragging ? "episode-audio__upload-area--dragging" : ""}`}
//                         onDragOver={handleDragOver}
//                         onDragLeave={handleDragLeave}
//                         onDrop={handleDrop}
//                         onClick={() => fileInputRef.current?.click()}
//                     >
//                         <Music className="episode-audio__upload-icon" size={40} />
//                         <p className="episode-audio__upload-text">Drop your audio file here</p>
//                         <p className="episode-audio__upload-subtext">or click to browse</p>
//                         <input
//                             ref={fileInputRef}
//                             type="file"
//                             accept="audio/*"
//                             onChange={handleFileSelect}
//                             className="episode-audio__upload-input"
//                         />
//                     </div>

//                     {uploadedFile && (
//                         <div className="episode-audio__file-info">
//                             <div className="episode-audio__file-info__row">
//                                 <FolderSimple size={20} color="#B6E04A" />
//                                 <span className="episode-audio__file-info__label">{uploadedFile.name}</span>
//                                 <span className="episode-audio__file-info__value"></span>
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
//                                 onChange={(e) => setSelectedBgSound(e.target.value)}
//                                 disabled={!audioUrl}
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
//                                 <MenuItem value="main_files/PodcastBackgroundSoundTracks/afdc0507-0e6d-4696-8467-1fc7b4d26514/audio.mp3">Ghost.mp3</MenuItem>
//                                 <MenuItem value="main_files/PodcastBackgroundSoundTracks/rain.mp3">Rain.mp3</MenuItem>
//                                 <MenuItem value="main_files/PodcastBackgroundSoundTracks/forest.mp3">Forest.mp3</MenuItem>
//                                 <MenuItem value="main_files/PodcastBackgroundSoundTracks/ocean.mp3">Ocean.mp3</MenuItem>
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
//                                 <div className="episode-audio__background-preview mt-3 ">
//                                     <audio controls src={ghost} controlsList="nodownload noplaybackrate" />

//                                 </div>
//                             )}
//                         </>
//                     )}
//                 </div>

//                 {/* Action Buttons */}
//                 <div className="episode-audio__actions">
//                     <button className="episode-audio__btn episode-audio__btn--primary" onClick={handleSave} disabled={!audioUrl}>
//                         <Download size={18} />
//                         Save
//                     </button>
//                     <button
//                         className="episode-audio__btn episode-audio__btn--secondary"
//                         onClick={handlePreview}
//                         disabled={!audioUrl || !hasPreviewChanges}
//                     >
//                         <Play size={18} />
//                         Preview
//                     </button>
//                 </div>
//             </div>
//         </div >
//     )
// }

// export default EpisodeAudio
