// import type React from "react"
// import { useEffect, useMemo, useRef, useState, useCallback } from "react"
// import WaveSurfer from "wavesurfer.js"
// import { Music, Download, Play, Minus, Pause as LucidePause } from "lucide-react"
// import "./sequencer-styles.scss"
// import { ArrowCounterClockwise, Database, FolderSimple, Plus, Question } from "phosphor-react"
// import { IconButton, MenuItem, Select, Tooltip } from "@mui/material"
// import { Rnd } from "react-rnd"
// import ghost from "../../../../assets/ghost.mp3"
// import { PlayArrow, Pause } from "@mui/icons-material"
// import { toast } from "react-toastify"
// import { loginRequiredAxiosInstance, publicAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
// import { audioTuning } from "@/core/services/episode/audio.service"
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


// const secondsToTime = (s: number) => {
//     if (s < 0) s = 0;
//     const m = Math.floor(s / 60);
//     const sec = Math.floor(s % 60)
//         .toString()
//         .padStart(2, "0");
//     return `${m}:${sec}`;
// };

// // Sequencer Clip interface
// interface Clip {
//     id: string
//     name: string
//     file: File
//     buffer: AudioBuffer
//     duration: number
//     start: number
//     trimStart: number
//     trimEnd: number
//     track: number
//     volume?: number // Volume in dB for background clips
// }

// // System background sounds (predefined)
// const SYSTEM_BACKGROUND_SOUNDS = [
//     { id: 'ghost', name: 'Ghost', file: ghost },
//     { id: 'rain', name: 'Rain', file: ghost }, // placeholder - replace with actual rain file
//     { id: 'forest', name: 'Forest', file: ghost }, // placeholder - replace with actual forest file
//     { id: 'ocean', name: 'Ocean', file: ghost }, // placeholder - replace with actual ocean file
// ]

// // Utility functions
// const clamp = (v: number, min: number, max: number) => Math.max(min, Math.min(max, v))

// let _ac: AudioContext
// const getAC = () => {
//     if (!_ac) _ac = new (window.AudioContext || (window as any).webkitAudioContext)()
//     return _ac
// }

// const EpisodeAudio: React.FC<EpisodeAudioProps> = ({ initialAudio }) => {
//     // ============ REFS ============
//     const fileInputRef = useRef<HTMLInputElement>(null)
//     const waveformRefOriginal = useRef<HTMLDivElement>(null)
//     const waveformRefPreview = useRef<HTMLDivElement>(null)
//     const waveformRefBg = useRef<HTMLDivElement>(null)
//     const wavesurferRefOriginal = useRef<WaveSurfer | null>(null)
//     const wavesurferRefPreview = useRef<WaveSurfer | null>(null)
//     const wavesurferRefBg = useRef<WaveSurfer | null>(null)
//     const progressBarRefOriginal = useRef<HTMLDivElement>(null)
//     const progressBarRefPreview = useRef<HTMLDivElement>(null)
//     const progressBarRefSequencer = useRef<HTMLDivElement>(null)
//     const timelineScrollRef = useRef<HTMLDivElement>(null)

//     // ============ STATE ============
//     const [uploadedFile, setUploadedFile] = useState<File | null>(null)
//     const [audioUrl, setAudioUrl] = useState<string | null>(initialAudio || null)
//     const [previewUrl, setPreviewUrl] = useState<string | null>(null)
//     const [isDragging, setIsDragging] = useState(false)

//     // Sequencer state
//     const [clips, setClips] = useState<Clip[]>([])
//     const [availableBackgrounds, setAvailableBackgrounds] = useState(SYSTEM_BACKGROUND_SOUNDS)
//     const [showSequencer, setShowSequencer] = useState(false)
//     const [pixelsPerSecond, setPPS] = useState(30) // Thu hẹp từ 120 xuống 30 để giảm scroll ngang
//     const [isPlayingSequencer, setIsPlayingSequencer] = useState(false)
//     const [playhead, setPlayhead] = useState(0)
//     const [trackVolumes, setTrackVolumes] = useState([1, 1])

//     // Audio context refs for sequencer
//     const acRef = useRef<AudioContext | null>(null)
//     const startWallClockRef = useRef(0)
//     const startPlayheadRef = useRef(0)
//     const activeNodesRef = useRef<Array<{ src: AudioBufferSourceNode }>>([])
//     const rafRef = useRef<number | undefined>(undefined)
//     const trackGainsRef = useRef<Array<{ gain: GainNode }>>([])

//     const rowH = 120 // Tăng từ 90 lên 120 để hiển thị waveform rõ hơn

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

//     // ============ SEQUENCER HANDLERS ============
//     const handleClipsChange = useCallback((newClips: Clip[]) => {
//         setClips(newClips)
//     }, [])

//     // Ensure gain nodes for sequencer
//     const ensureTrackGains = useCallback(() => {
//         const ac = acRef.current || getAC()
//         while (trackGainsRef.current.length < 2) {
//             const gain = ac.createGain()
//             gain.gain.value = 1
//             gain.connect(ac.destination)
//             trackGainsRef.current.push({ gain })
//         }
//         trackGainsRef.current[0].gain.gain.value = trackVolumes[0] ?? 1
//         trackGainsRef.current[1].gain.gain.value = trackVolumes[1] ?? 1
//     }, [trackVolumes])

//     // Stop all audio nodes
//     const stopAll = useCallback(() => {
//         activeNodesRef.current.forEach(({ src }) => {
//             try {
//                 src.stop()
//             } catch { }
//         })
//         activeNodesRef.current = []
//         if (rafRef.current) cancelAnimationFrame(rafRef.current)
//     }, [])

//     // Helper function to find next available position for background clip
//     const findAvailablePosition = useCallback((newDuration: number, existingBgClips: Clip[]) => {
//         if (existingBgClips.length === 0) {
//             return 0 // No existing clips, start at beginning
//         }

//         // Sort existing clips by start time
//         const sortedClips = existingBgClips
//             .map(clip => ({
//                 start: clip.start,
//                 end: clip.start + Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
//             }))
//             .sort((a, b) => a.start - b.start)

//         // Check if we can fit at the beginning
//         if (sortedClips[0].start >= newDuration) {
//             return 0
//         }

//         // Check gaps between clips
//         for (let i = 0; i < sortedClips.length - 1; i++) {
//             const gapStart = sortedClips[i].end
//             const gapEnd = sortedClips[i + 1].start
//             const gapSize = gapEnd - gapStart

//             if (gapSize >= newDuration) {
//                 return gapStart
//             }
//         }

//         // Place after the last clip
//         const lastClipEnd = sortedClips[sortedClips.length - 1].end
//         return lastClipEnd
//     }, [])

//     // Load background sound to sequencer
//     const handleAddBackgroundToSequencer = useCallback(async (bgSound: typeof SYSTEM_BACKGROUND_SOUNDS[0]) => {
//         if (!uploadedFile) return

//         try {
//             const ac = acRef.current || getAC()
//             acRef.current = ac

//             const response = await fetch(bgSound.file)
//             const arrayBuffer = await response.arrayBuffer()
//             const buffer = await ac.decodeAudioData(arrayBuffer.slice(0)) // Use slice to create a copy

//             // Get existing background clips
//             const existingBgClips = clips.filter(c => c.track === 1)

//             // Find the best position for this new clip
//             const optimalStart = findAvailablePosition(buffer.duration, existingBgClips)

//             // Create blob and file from the same arrayBuffer
//             const blob = new Blob([arrayBuffer], { type: 'audio/mpeg' })
//             const file = new File([blob], bgSound.name, { type: 'audio/mpeg' })

//             const newClip: Clip = {
//                 id: `${bgSound.id}-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
//                 name: bgSound.name,
//                 file,
//                 buffer,
//                 duration: buffer.duration,
//                 start: optimalStart,
//                 trimStart: 0,
//                 trimEnd: 0,
//                 track: 1, // Background track
//                 volume: -5, // Default volume -5dB for background
//             }

//             setClips(prev => [...prev, newClip])
//             setShowSequencer(true)
//             toast.success(`Added ${bgSound.name} to sequencer at ${Math.floor(optimalStart)}s`)
//         } catch (error) {
//             console.error('Failed to load background sound:', error)
//             toast.error('Failed to load background sound')
//         }
//     }, [uploadedFile, clips, findAvailablePosition])

//     // Load original audio to sequencer
//     useEffect(() => {
//         if (!uploadedFile) return

//         const loadOriginalAudio = async () => {
//             try {
//                 const ac = getAC()
//                 acRef.current = ac

//                 const buffer = await ac.decodeAudioData(await uploadedFile.arrayBuffer())
//                 const originalClip: Clip = {
//                     id: `${uploadedFile.name}-${Date.now()}-0`,
//                     name: uploadedFile.name,
//                     file: uploadedFile,
//                     buffer,
//                     duration: buffer.duration,
//                     start: 0,
//                     trimStart: 0,
//                     trimEnd: 0,
//                     track: 0, // Original track
//                 }

//                 setClips(prev => {
//                     const filtered = prev.filter(c => c.track !== 0)
//                     return [...filtered, originalClip]
//                 })
//             } catch (error) {
//                 console.error("Failed to load original audio:", error)
//             }
//         }

//         loadOriginalAudio()
//     }, [uploadedFile])

//     // Sequencer playback functions
//     const originalDuration = useMemo(() => {
//         const clip = clips.find((c) => c.track === 0)
//         return clip ? clip.duration : 30
//     }, [clips])

//     const totalLengthSec = useMemo(() => {
//         const mx = clips.reduce((m, c) => {
//             const vis = Math.max(0, c.duration - c.trimStart - c.trimEnd)
//             return Math.max(m, c.start + vis)
//         }, originalDuration)
//         return Math.max(mx, originalDuration)
//     }, [clips, originalDuration])

//     const scheduleFrom = useCallback((fromSec: number) => {
//         const ac = acRef.current || getAC()
//         ensureTrackGains()

//         stopAll()

//         startWallClockRef.current = ac.currentTime
//         startPlayheadRef.current = fromSec

//         for (let t = 0; t < 2; t++) {
//             const tGain = trackGainsRef.current[t]?.gain || ac.destination
//             const tClips = clips.filter((c) => c.track === t)

//             const scheduleClipAt = (clip: Clip, offsetSec: number) => {
//                 const playStart = clip.start + offsetSec
//                 const clipDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
//                 const playEnd = playStart + clipDur
//                 if (clipDur <= 0) return
//                 if (fromSec < playEnd) {
//                     const when = Math.max(0, playStart - fromSec)
//                     const offset = Math.max(0, fromSec - playStart) + clip.trimStart
//                     const dur = clipDur - Math.max(0, fromSec - playStart)
//                     const src = ac.createBufferSource()
//                     src.buffer = clip.buffer

//                     // Apply individual clip volume for background tracks
//                     if (clip.track === 1 && clip.volume !== undefined) {
//                         const clipGain = ac.createGain()
//                         // Convert dB to linear gain: gain = 10^(dB/20)
//                         const linearGain = Math.pow(10, clip.volume / 20)
//                         clipGain.gain.value = linearGain
//                         src.connect(clipGain)
//                         clipGain.connect(tGain)
//                     } else {
//                         src.connect(tGain)
//                     }

//                     try {
//                         src.start(ac.currentTime + when, offset, Math.max(0, dur))
//                         activeNodesRef.current.push({ src })
//                     } catch { }
//                 }
//             }

//             tClips.forEach((c) => scheduleClipAt(c, 0))
//         }

//         const tick = () => {
//             const elapsed = ac.currentTime - startWallClockRef.current
//             const ph = startPlayheadRef.current + elapsed
//             setPlayhead(ph)
//             if (ph >= totalLengthSec) {
//                 setIsPlayingSequencer(false)
//                 setPlayhead(0)
//                 return
//             }
//             rafRef.current = requestAnimationFrame(tick)
//         }
//         rafRef.current = requestAnimationFrame(tick)
//     }, [clips, totalLengthSec, ensureTrackGains, stopAll])

//     const handleSequencerPlay = useCallback(() => {
//         if (isPlayingSequencer) return
//         getAC().resume()
//         setIsPlayingSequencer(true)
//         scheduleFrom(playhead)
//     }, [isPlayingSequencer, playhead, scheduleFrom])

//     const handleSequencerPause = useCallback(() => {
//         setIsPlayingSequencer(false)
//         stopAll()
//     }, [stopAll])

//     const handleSequencerStop = useCallback(() => {
//         handleSequencerPause()
//         setPlayhead(0)
//     }, [handleSequencerPause])

//     useEffect(() => {
//         ensureTrackGains()
//     }, [ensureTrackGains])

//     useEffect(() => () => stopAll(), [stopAll])

//     // Auto-scroll timeline to follow playhead
//     useEffect(() => {
//         if (isPlayingSequencer && timelineScrollRef.current) {
//             const scrollContainer = timelineScrollRef.current
//             const playheadPosition = playhead * pixelsPerSecond
//             const containerWidth = scrollContainer.clientWidth
//             const scrollLeft = scrollContainer.scrollLeft

//             // Scroll if playhead is near the right edge or out of view
//             if (playheadPosition > scrollLeft + containerWidth - 200) {
//                 scrollContainer.scrollTo({
//                     left: playheadPosition - containerWidth / 2,
//                     behavior: 'smooth'
//                 })
//             }
//         }
//     }, [playhead, isPlayingSequencer, pixelsPerSecond])

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

//     // Waveform background (chỉ hiển thị, không cần chức năng tương tác)
//     useEffect(() => {
//         if (!waveformRefBg.current || !selectedBgSound) return;

//         const wsBg = WaveSurfer.create({
//             container: waveformRefBg.current,
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
//         });
//         wavesurferRefBg.current = wsBg;
//         // Nếu là ghost.mp3 demo thì dùng ghost, còn lại dùng selectedBgSound
//         const bgUrl = selectedBgSound === "main_files/PodcastBackgroundSoundTracks/afdc0507-0e6d-4696-8467-1fc7b4d26514/audio.mp3" ? ghost : selectedBgSound;
//         wsBg.load(bgUrl);
//         return () => {
//             wsBg.destroy();
//         };
//     }, [selectedBgSound]);

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
//         const response = await audioTuning(loginRequiredAxiosInstance,"hihi", payload)
//         console.log("Audio Tuning Response:", response)
//         if (response.success && response.data) {
//             const blob = response.data;
//             const url = URL.createObjectURL(blob);
//             setPreviewUrl(url);
//         } else {
//             //console.log(response.message)
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
//                         {selectedBgSound && (
//                             <div className="episode-audio__player-section">
//                                 <h3 className="episode-audio__player-title">Background Audio</h3>
//                                 <div className="episode-audio__waveform-container">
//                                     <div ref={waveformRefBg} className="episode-audio__waveform" />
//                                 </div>
//                             </div>
//                         )}
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

//                 {/* Background Audio Waveform Section (chỉ hiển thị) */}


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

//                 {/* Audio Sequencer */}
//                 {showSequencer && uploadedFile && (
//                     <div className="episode-audio__sequencer-container">
//                         <div className="episode-audio__sequencer-header">
//                         <h3 className="text-lg font-semibold text-white mb-4">Audio Sequencer</h3>
//                         <p className="text-sm text-gray-400 mb-4">
//                             Drag and trim background sounds on the timeline. Multiple backgrounds cannot overlap in time.
//                         </p>
//                     </div>
//                         <div className="ml-4 flex items-center gap-2">
//                             <label className="text-sm">Zoom</label>
//                             <input
//                                 type="range"
//                                 min={15}
//                                 max={80}
//                                 value={pixelsPerSecond}
//                                 onChange={(e) => setPPS(Number.parseInt(e.target.value))}
//                                 className="accent-emerald-600"
//                             />
//                             <span className="text-xs text-slate-400">{pixelsPerSecond}px/s</span>
//                             <button
//                                 onClick={() => {
//                                     console.log('=== SEQUENCER CLIP INFO ===')
//                                     const originalClip = clips.find(c => c.track === 0)
//                                     const bgClips = clips.filter(c => c.track === 1)

//                                     if (originalClip) {
//                                         console.log('🎵 ORIGINAL AUDIO:')
//                                         console.log(`  File: ${originalClip.name}`)
//                                         console.log(`  Duration: ${secondsToTime(originalClip.duration)} (${originalClip.duration.toFixed(2)}s)`)
//                                         console.log(`  Timeline Start: ${secondsToTime(originalClip.start)} (${originalClip.start.toFixed(2)}s)`)
//                                         console.log(`  Timeline End: ${secondsToTime(originalClip.start + originalClip.duration)} (${(originalClip.start + originalClip.duration).toFixed(2)}s)`)
//                                     }

//                                     bgClips.forEach((clip, idx) => {
//                                         const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
//                                         console.log(`\n🎧 BACKGROUND ${idx + 1}:`)
//                                         console.log(`  File: ${clip.name}`)
//                                         console.log(`  Full Duration: ${secondsToTime(clip.duration)} (${clip.duration.toFixed(2)}s)`)
//                                         console.log(`  Visible Duration: ${secondsToTime(visibleDur)} (${visibleDur.toFixed(2)}s)`)
//                                         console.log(`  Timeline Start: ${secondsToTime(clip.start)} (${clip.start.toFixed(2)}s)`)
//                                         console.log(`  Timeline End: ${secondsToTime(clip.start + visibleDur)} (${(clip.start + visibleDur).toFixed(2)}s)`)
//                                         console.log(`  Trim Start: ${secondsToTime(clip.trimStart)} (${clip.trimStart.toFixed(2)}s)`)
//                                         console.log(`  Trim End: ${secondsToTime(clip.trimEnd)} (${clip.trimEnd.toFixed(2)}s)`)

//                                         // Show volume for background clips
//                                         if (clip.track === 1 && clip.volume !== undefined) {
//                                             console.log(`  Volume: ${clip.volume.toFixed(1)} dB`)
//                                         }

//                                         if (originalClip) {
//                                             console.log(`  Position in Original: ${secondsToTime(clip.start)} → ${secondsToTime(clip.start + visibleDur)}`)
//                                         }
//                                     })

//                                     console.log('\n=== END CLIP INFO ===')
//                                 }}
//                                 className="px-3 py-1 rounded bg-slate-700 hover:bg-slate-600 transition-all text-xs flex items-center gap-1"
//                                 title="Show clip timeline info in console"
//                             >
//                                 <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
//                                     <circle cx="12" cy="12" r="10" />
//                                     <line x1="12" y1="16" x2="12" y2="12" />
//                                     <line x1="12" y1="8" x2="12.01" y2="8" />
//                                 </svg>
//                                 Info
//                             </button>
//                         </div>
//                         {/* Sequencer Controls */}
//                         <div className="w-full text-slate-100 p-4 flex flex-col gap-3 select-none bg-slate-900 rounded">
//                             {/* Toolbar */}

//                             {/* Tracks */}
//                             <div className="relative border border-slate-800 rounded overflow-hidden">
//                                 {/* Track headers */}
//                                 <div className="bg-slate-900 border-b border-slate-800">
//                                     {/* Original track */}
//                                     {/* <div className="flex items-center gap-3 px-3" style={{ height: rowH, borderBottom: "1px solid #0b1320" }}>
//                                         <div className="text-xs w-20 font-semibold">Original</div>
//                                         <div className="flex items-center gap-2">
//                                             <label className="text-xs">Vol</label>
//                                             <input
//                                                 type="range"
//                                                 min={0}
//                                                 max={1}
//                                                 step={0.01}
//                                                 value={trackVolumes[0] ?? 1}
//                                                 onChange={(e) => {
//                                                     const v = Number.parseFloat(e.target.value)
//                                                     setTrackVolumes([v, trackVolumes[1]])
//                                                 }}
//                                             />
//                                         </div>
//                                     </div> */}

//                                     {/* Background track */}
//                                     {/* <div className="flex items-center gap-3 px-3" style={{ height: rowH, borderBottom: "1px solid #0b1320" }}>
//                                         <div className="text-xs w-20 font-semibold">Background</div>
//                                         <div className="flex items-center gap-2">
//                                             <label className="text-xs">Vol</label>
//                                             <input
//                                                 type="range"
//                                                 min={0}
//                                                 max={1}
//                                                 step={0.01}
//                                                 value={trackVolumes[1] ?? 1}
//                                                 onChange={(e) => {
//                                                     const v = Number.parseFloat(e.target.value)
//                                                     setTrackVolumes([trackVolumes[0], v])
//                                                 }}
//                                             />
//                                         </div>
//                                     </div> */}
//                                 </div>

//                                 {/* Timeline body - với container có scroll để giới hạn chiều rộng */}
//                                 <div className="overflow-x-auto max-w-full" ref={timelineScrollRef} >
//                                     <SequencerTimeline
//                                         clips={clips}
//                                         setClips={setClips}
//                                         pixelsPerSecond={pixelsPerSecond}
//                                         rowH={rowH}
//                                         totalLengthSec={totalLengthSec}
//                                         playhead={playhead}
//                                     />
//                                 </div>

//                             </div>

//                             {/* Ruler - với scroll tương tự */}
//                             <div className="overflow-x-auto max-w-full">
//                                 <SequencerRuler
//                                     totalLengthSec={totalLengthSec}
//                                     pixelsPerSecond={pixelsPerSecond}
//                                     playhead={playhead}
//                                 />
//                             </div>
//                         </div>

//                         <header className="flex flex-wrap items-center gap-3">
//                             <IconButton onClick={isPlayingSequencer ? handleSequencerPause : handleSequencerPlay} className="episode-audio__play-btn">
//                                 {isPlayingSequencer ? <Pause /> : <PlayArrow />}
//                             </IconButton>
//                             <IconButton onClick={() => handleSequencerStop()} className="episode-audio__play-btn">
//                                 <ArrowCounterClockwise size={26} weight="bold" />
//                             </IconButton>
//                             <span className="episode-audio__time-display">{secondsToTime(playhead)}</span>
//                             <div
//                                 className="episode-audio__progress-bar"
//                                 ref={progressBarRefSequencer}
//                                 onMouseDown={(e) => {
//                                     if (!progressBarRefSequencer.current) return
//                                     const rect = progressBarRefSequencer.current.getBoundingClientRect()
//                                     const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1)
//                                     const newTime = percent * totalLengthSec
//                                     setPlayhead(newTime)
//                                 }}
//                                 onClick={(e) => {
//                                     if (!progressBarRefSequencer.current) return
//                                     const rect = progressBarRefSequencer.current.getBoundingClientRect()
//                                     const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1)
//                                     const newTime = percent * totalLengthSec
//                                     setPlayhead(newTime)
//                                 }}
//                             >
//                                 <div
//                                     className="episode-audio__progress-fill"
//                                     style={{
//                                         width: `${(playhead / totalLengthSec) * 100}%`,
//                                     }}
//                                 />
//                                 <div
//                                     className="episode-audio__progress-thumb"
//                                     style={{
//                                         left: `${(playhead / totalLengthSec) * 100}%`,
//                                     }}
//                                 />
//                             </div>

//                             <div className="episode-audio__controls-right">
//                                 <span className="episode-audio__time-display">{secondsToTime(totalLengthSec)}</span>
//                             </div>
//                         </header>
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

//                 {/* Background Sound Sequencer */}
//                 <div className="episode-audio__background-sound">
//                     <div className="episode-audio__background-sound-header">
//                         <div className="flex items-center gap-2">
//                             <h3 className="episode-audio__section-title">
//                                 Sequencer
//                             </h3>
//                             <Tooltip placement="top-start" title="Add multiple background sounds with precise timing control">
//                                 <Question color="var(--third-grey)" size={16} />
//                             </Tooltip>
//                             {clips.filter(c => c.track === 1).length > 0 && (
//                                 <span className="text-xs text-slate-400">
//                                     ({clips.filter(c => c.track === 1).length} backgrounds)
//                                 </span>
//                             )}
//                         </div>
//                         <IconButton
//                             className="episode-audio__add-btn"
//                             onClick={() => setShowSequencer(!showSequencer)}
//                             title={showSequencer ? "Hide Sequencer" : "Show Sequencer"}
//                             disabled={!audioUrl}
//                         >
//                             {showSequencer ? <Minus color="white" size={20} /> : <Plus size={20} />}
//                         </IconButton>
//                     </div>

//                     {/* Available Background Sounds - Moved to Sidebar */}
//                     {showSequencer && (
//                         <div className="mt-3">
//                             <h4 className="text-sm font-medium mb-2 text-white flex items-center gap-2">
//                                 <span style={{ color: 'var(--primary-green)' }}>Add Background Sounds</span>
//                                 <Tooltip placement="top" title="Click to add backgrounds - they will be auto-positioned">
//                                     <Question color="var(--third-grey)" size={14} />
//                                 </Tooltip>
//                             </h4>
//                             <div className="grid grid-cols-2 gap-2 mb-3">
//                                 {availableBackgrounds.map((bg) => (
//                                     <button
//                                         key={bg.id}
//                                         className="px-3 py-2 text-xs rounded transition-all text-white border"
//                                         style={{
//                                             background: 'rgba(23, 23, 23, 0.6)',
//                                             borderColor: 'rgba(174, 227, 57, 0.2)',
//                                         }}
//                                         onMouseEnter={(e) => {
//                                             e.currentTarget.style.background = 'rgba(174, 227, 57, 0.15)'
//                                             e.currentTarget.style.borderColor = 'rgba(174, 227, 57, 0.4)'
//                                         }}
//                                         onMouseLeave={(e) => {
//                                             e.currentTarget.style.background = 'rgba(23, 23, 23, 0.6)'
//                                             e.currentTarget.style.borderColor = 'rgba(174, 227, 57, 0.2)'
//                                         }}
//                                         onClick={() => handleAddBackgroundToSequencer(bg)}
//                                         title={`Add ${bg.name}`}
//                                     >
//                                         <span className="block truncate">{bg.name}</span>
//                                     </button>
//                                 ))}
//                             </div>

//                             {/* Background Clips Volume Controls */}
//                             {clips.filter(c => c.track === 1).length > 0 && (
//                                 <div className="mt-4 pt-3 border-t" style={{ borderColor: 'rgba(174, 227, 57, 0.15)' }}>
//                                     <h4 className="text-sm font-medium mb-3 text-white flex items-center gap-2">
//                                         <span style={{ color: 'var(--primary-green)' }}>Background Volumes</span>
//                                         <Tooltip placement="top" title="Adjust volume for each background before merging">
//                                             <Question color="var(--third-grey)" size={14} />
//                                         </Tooltip>
//                                     </h4>
//                                     {clips.filter(c => c.track === 1).map((clip) => (
//                                         <div key={clip.id} className="mb-3 p-2 rounded" style={{ background: 'rgba(23, 23, 23, 0.4)' }}>
//                                             <div className="flex justify-between items-center mb-1">
//                                                 <span className="text-xs text-white truncate max-w-[120px]" title={clip.name}>
//                                                     {clip.name}
//                                                 </span>
//                                                 <span className="text-xs" style={{ color: 'var(--primary-green)' }}>
//                                                     {(clip.volume ?? -5).toFixed(1)} dB
//                                                 </span>
//                                             </div>
//                                             <input
//                                                 type="range"
//                                                 min={-20}
//                                                 max={10}
//                                                 step={0.5}
//                                                 value={clip.volume ?? 0}
//                                                 onChange={(e) => {
//                                                     const newVolume = Number.parseFloat(e.target.value)
//                                                     setClips(prev => prev.map(c =>
//                                                         c.id === clip.id ? { ...c, volume: newVolume } : c
//                                                     ))
//                                                 }}
//                                                 className="episode-audio__volume-slider"
//                                                 style={
//                                                     {
//                                                         "--value": `${((bgSoundVolume + 20) / 20) * 100}%`,
//                                                     } as React.CSSProperties
//                                                 }
//                                             />
//                                             <div className="flex justify-between text-xs mt-1" style={{ color: 'rgba(255, 255, 255, 0.5)' }}>
//                                                 <span>-20</span>
//                                                 <span>0</span>
//                                                 <span>+10</span>
//                                             </div>
//                                         </div>
//                                     ))}
//                                 </div>
//                             )}
//                         </div>
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

// // ============ SEQUENCER COMPONENTS ============

// interface SequencerTimelineProps {
//     clips: Clip[]
//     setClips: React.Dispatch<React.SetStateAction<Clip[]>>
//     pixelsPerSecond: number
//     rowH: number
//     totalLengthSec: number
//     playhead: number
// }

// function SequencerTimeline({ clips, setClips, pixelsPerSecond, rowH, totalLengthSec, playhead }: SequencerTimelineProps) {
//     const totalWidth = Math.max(Math.ceil(totalLengthSec * pixelsPerSecond), 1200) // Minimum 1200px cho full width

//     return (
//         <div className="relative bg-slate-900" style={{ width: totalWidth, height: rowH * 2 + 10, minWidth: '100%' }}>
//             <div style={{ width: totalWidth, height: rowH * 2 + 10 }} className="relative">
//                 {/* Track backgrounds */}
//                 <div className="absolute left-0" style={{ top: 0, height: rowH, width: totalWidth, background: "#0b1220" }} />
//                 <div className="absolute left-0" style={{ top: rowH + 10, height: rowH, width: totalWidth, background: "#0f172a" }} />

//                 {/* Clips */}
//                 {clips.map((clip) => (
//                     <ClipRnd
//                         key={clip.id}
//                         clip={clip}
//                         pps={pixelsPerSecond}
//                         rowH={rowH}
//                         isOriginal={clip.track === 0}
//                         allClips={clips}
//                         onChange={(next) => {
//                             if (next && next.__delete) {
//                                 setClips((prev) => prev.filter((c) => c.id !== clip.id))
//                             } else {
//                                 setClips((prev) => prev.map((c) => (c.id === clip.id ? { ...c, ...next } : c)))
//                             }
//                         }}
//                     />
//                 ))}

//                 {/* Playhead */}
//                 <div
//                     className="pointer-events-none absolute top-0 bottom-0 w-px bg-rose-400"
//                     style={{ left: playhead * pixelsPerSecond }}
//                 />
//             </div>
//         </div>
//     )
// }

// interface SequencerRulerProps {
//     totalLengthSec: number
//     pixelsPerSecond: number
//     playhead: number
// }

// function SequencerRuler({ totalLengthSec, pixelsPerSecond, playhead }: SequencerRulerProps) {
//     const totalWidth = Math.max(Math.ceil(totalLengthSec * pixelsPerSecond), 1200) // Minimum 1200px cho full width

//     return (
//         <div className="relative border border-slate-800 rounded bg-slate-900 h-[44px]" style={{ width: totalWidth, minWidth: '100%' }}>
//             <div style={{ width: totalWidth }} className="relative h-full">
//                 {Array.from({ length: Math.max(Math.ceil(totalLengthSec) + 1, Math.ceil(1200 / pixelsPerSecond) + 1) }).map((_, i) => {
//                     const left = i * pixelsPerSecond
//                     const major = i % 5 === 0
//                     return (
//                         <div key={i} className="absolute top-0 h-full" style={{ left, width: 1 }}>
//                             <div className={`w-px ${major ? "h-full bg-slate-600" : "h-1/2 bg-slate-700"}`} />
//                             {major && <div className="absolute top-0 left-1 text-xs text-slate-300">{secondsToTime(i)}</div>}
//                         </div>
//                     )
//                 })}
//                 <div className="absolute top-0 bottom-0 w-px bg-rose-400" style={{ left: playhead * pixelsPerSecond }} />
//             </div>
//         </div>
//     )
// }

// interface ClipRndProps {
//     clip: Clip
//     pps: number
//     rowH: number
//     isOriginal: boolean
//     onChange: (update: Partial<Clip> & { __delete?: boolean }) => void
//     allClips: Clip[]
// }

// function ClipRnd({ clip, pps, rowH, isOriginal, onChange, allClips }: ClipRndProps) {
//     const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
//     const width = Math.max(8, visibleDur * pps)
//     const x = clip.start * pps
//     const y = clip.track === 0 ? 0 : rowH + 10 // Thêm khoảng cách 10px giữa tracks
//     const height = rowH
//     const dragHandle = "clip-drag-handle"
//     const snapGridX = Math.max(1, Math.round(pps / 20))

//     return (
//         <Rnd
//             size={{ width, height }}
//             position={{ x, y }}
//             bounds="parent"
//             dragAxis="x"
//             dragGrid={[snapGridX, 0]}
//             enableResizing={
//                 isOriginal
//                     ? false
//                     : {
//                         left: true,
//                         right: true,
//                         top: false,
//                         bottom: false,
//                     }
//             }
//             dragHandleClassName={dragHandle}
//             onDragStop={(e, d) => {
//                 const newStart = Math.max(0, d.x / pps);

//                 // Ngăn chồng lấn background track
//                 if (clip.track === 1) {
//                     const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd);
//                     const newEnd = newStart + visibleDur;
//                     const hasConflict = allClips.some((c) => {
//                         if (c.id === clip.id || c.track !== 1) return false;
//                         const otherDur = Math.max(0, c.duration - c.trimStart - c.trimEnd);
//                         return !(newEnd <= c.start || newStart >= c.start + otherDur);
//                     });
//                     if (hasConflict) {
//                         toast.error("Cannot move clip: it would overlap another background sound");
//                         return;
//                     }
//                 }

//                 onChange({ start: newStart });
//             }}
//             onResizeStop={(e, dir, ref, delta, pos) => {
//                 if (isOriginal) return;
//                 const newWpx = ref.offsetWidth;
//                 const newVisible = Math.max(0.01, newWpx / pps);

//                 if (dir === "left") {
//                     const deltaSeconds = -delta.width / pps;
//                     const newStart = Math.max(0, pos.x / pps);
//                     let nextTrimStart = clip.trimStart + deltaSeconds;
//                     nextTrimStart = clamp(nextTrimStart, 0, clip.duration - clip.trimEnd - 0.01);
//                     onChange({ start: newStart, trimStart: +nextTrimStart });
//                 }

//                 if (dir === "right") {
//                     const nextTrimEnd = clamp(
//                         clip.duration - clip.trimStart - newVisible,
//                         0,
//                         clip.duration - clip.trimStart - 0.01
//                     );
//                     onChange({ trimEnd: +nextTrimEnd });
//                 }
//             }}
//             className="rounded-xl shadow-lg border border-slate-700 bg-slate-800"
//         >
//             <ClipWaveform
//                 clip={clip}
//                 height={height}
//                 width={width}
//                 dragHandle={dragHandle}
//                 isOriginal={isOriginal}
//                 onDelete={() => onChange({ __delete: true })}
//             />
//         </Rnd>

//     )
// }

// interface ClipWaveformProps {
//     clip: Clip
//     width: number
//     height: number
//     dragHandle: string
//     isOriginal: boolean
//     onDelete: () => void
// }

// function ClipWaveform({ clip, width, height, dragHandle, isOriginal, onDelete }: ClipWaveformProps) {
//     const containerRef = useRef<HTMLDivElement>(null)
//     const wsRef = useRef<WaveSurfer | null>(null)

//     useEffect(() => {
//         if (!containerRef.current) return
//         const ws = WaveSurfer.create({
//             container: containerRef.current,
//             height: Math.max(15, height - 50),
//             waveColor: "#7BA225",
//             progressColor: "#AEE339",
//             cursorWidth: 0,
//             interact: false,
//             hideScrollbar: true,
//             minPxPerSec: 10,
//         })
//         wsRef.current = ws
//         ws.loadBlob(clip.file).catch(() => { })
//         return () => {
//             try {
//                 ws.destroy()
//             } catch { }
//         }
//     }, [clip.file, height])

//     // Tính toán vùng thời gian hiển thị (chỉ ghi nhớ start, end)
//     const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
//     const leftTrimPercent = (clip.trimStart / clip.duration) * 100

//     return (
//         <div className="w-full h-full flex flex-col">
//             <div
//                 className={`px-2 py-1 text-xs text-slate-200 flex items-center justify-between border-b border-slate-700 ${dragHandle}`}
//             >
//                 <div className="truncate" title={clip.name}>
//                     {clip.name}
//                 </div>
//                 <div className="flex items-center gap-2">
//                     {!isOriginal && (
//                         <button
//                             className="px-2 py-0.5 rounded bg-rose-600 hover:bg-rose-700"
//                             onClick={(e) => {
//                                 e.stopPropagation()
//                                 onDelete()
//                             }}
//                         >
//                             Delete
//                         </button>
//                     )}
//                     <div className="text-slate-400">{secondsToTime(visibleDur)}</div>
//                 </div>
//             </div>

//             <div className="flex-1 relative overflow-hidden">
//                 {/* Hiển thị waveform theo vùng đã cắt, không overlay */}
//                 <div
//                     ref={containerRef}
//                     className="absolute top-0 bottom-0 left-0"
//                     style={{
//                         width: `${(clip.duration / visibleDur) * 100}%`,
//                         transform: `translateX(-${leftTrimPercent}%)`,
//                     }}
//                 />
//             </div>
//         </div>
//     )
// }


// export default EpisodeAudio
