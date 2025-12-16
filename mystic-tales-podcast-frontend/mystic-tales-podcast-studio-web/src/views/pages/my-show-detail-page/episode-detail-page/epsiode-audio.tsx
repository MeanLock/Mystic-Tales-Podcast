import type React from "react"
import { useCallback, useContext, useEffect, useLayoutEffect, useMemo, useRef, useState } from "react"
import WaveSurfer from "wavesurfer.js"
import { Music, Download, Play, Minus, CloudUpload } from "lucide-react"
import { ArrowCounterClockwise, Database, FolderSimple, Plus, Question } from "phosphor-react"
import { IconButton, MenuItem, Modal, Select, Skeleton, Tooltip } from "@mui/material"
import ghost from "../../../../assets/ghost.mp3"
import { PlayArrow, Pause, ContentCopy, PublishedWithChangesOutlined, Delete } from "@mui/icons-material"
import { toast } from "react-toastify"
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import { useNavigate, useParams } from "react-router-dom"
import { audioTuning, getAudioFile, getBackgroundSoundFile, getBackgroundSounds, uploadAudio } from "@/core/services/episode/audio.service"
import { BackgroundSound } from "@/core/types"
import { useSagaPolling } from "@/core/hooks/useSagaPolling"
import { set } from "lodash"
import Loading2 from "@/views/components/common/loading2"
import Image from "@/views/components/common/image"
import { Episode } from "@/core/types/episode"
import { getEpisodeDetail } from "@/core/services/episode/episode.service"
import Loading from "@/views/components/common/loading"
import Modal_Button from "@/views/components/common/modal/ModalButton"
import { buildEpisodeAudioFileName, secondsToTime } from "@/core/utils/audio.util"
import { confirmAlert } from "@/core/utils/alert.util"
import { EpisodeDetailViewContext } from "."
import { SmartAudioPlayer } from "@/views/components/common/audio"
import SequencerTimeline from "./components/SequencerTimeline"
import SequencerRuler from "./components/SequencerRuler"
import * as signalR from "@microsoft/signalr";
import { useSelector } from "react-redux"
import { RootState } from "@/redux/rootReducer"
interface EpisodeAudioProps {
    initialAudio?: string
}
const presets: Record<string, Record<string, number>> = {
    Flat: {
        SubBass: 0,
        Bass: 0,
        Low: 0,
        LowMid: 0,
        Mid: 0,
        Presence: 0,
        HighMid: 0,
        Treble: 0,
        Air: 0,
    },
    Podcast: {
        SubBass: -3,
        Bass: -2,
        Low: -1,
        LowMid: 2,
        Mid: 3,
        Presence: 2,
        HighMid: 1,
        Treble: 1,
        Air: 0,
    },
    BassBoost: {
        SubBass: 5,
        Bass: 4,
        Low: 2,
        LowMid: 0,
        Mid: -2,
        Presence: -2,
        HighMid: 0,
        Treble: 1,
        Air: 1,
    },
    TrebleBoost: {
        SubBass: -2,
        Bass: -1,
        Low: 0,
        LowMid: 1,
        Mid: 1,
        Presence: 3,
        HighMid: 4,
        Treble: 5,
        Air: 3,
    },
    MysticVoice: {
        SubBass: -4,
        Bass: -2,
        Low: 1,
        LowMid: 3,
        Mid: 4,
        Presence: 3,
        HighMid: 2,
        Treble: 1,
        Air: 2,
    },
    DeepMystery: {
        SubBass: 3,
        Bass: 2,
        Low: 1,
        LowMid: -1,
        Mid: 1,
        Presence: 2,
        HighMid: 1,
        Treble: -1,
        Air: 0,
    },
}
const MOOD_OPTIONS = [
    { value: 'Mysterious', label: 'Mysterious' },
    { value: 'Eerie', label: ' Eerie' },
]

interface Clip {
    id: string
    name: string
    fileKey: string
    file: File
    buffer: AudioBuffer
    duration: number
    start: number
    trimStart: number
    trimEnd: number
    track: number
    volume?: number

    fadeInSec?: number
    fadeOutSec?: number
}


let _ac: AudioContext
const getAC = () => {
    if (!_ac) _ac = new (window.AudioContext || (window as any).webkitAudioContext)()
    return _ac
}
let currentSegment: { src: AudioBufferSourceNode | null; clipId: string | null } = { src: null, clipId: null }


const EpisodeAudio: React.FC<EpisodeAudioProps> = ({ initialAudio }) => {
    const { episodeId } = useParams<{ episodeId: string }>();
    const ctx = useContext(EpisodeDetailViewContext);
    const authSlice = ctx?.authSlice;
    const episodeDetail = ctx?.episodeDetail;
    const refreshEpisode = ctx?.refreshEpisode;
    
    // ============ REFS ============
    const waveformRefOriginal = useRef<HTMLDivElement>(null)
    const waveformRefPreview = useRef<HTMLDivElement>(null)
    const wavesurferRefOriginal = useRef<WaveSurfer | null>(null)
    const wavesurferRefPreview = useRef<WaveSurfer | null>(null)
    const fileInputRef = useRef<HTMLInputElement>(null)
    const progressBarRefOriginal = useRef<HTMLDivElement>(null)
    const progressBarRefPreview = useRef<HTMLDivElement>(null)

    // ============ STATE ============
    const [uploadedFile, setUploadedFile] = useState<File | null>(null)
    const [audioUrl, setAudioUrl] = useState<string | null>(initialAudio || null)
    const [previewUrl, setPreviewUrl] = useState<string | null>(null)
    const [previewFile, setPreviewFile] = useState<File | null>(null)
    const [isDragging, setIsDragging] = useState(false)

    // Original audio player state
    const [isPlayingOriginal, setIsPlayingOriginal] = useState(false)
    const [currentTimeOriginal, setCurrentTimeOriginal] = useState(0)
    const [durationOriginal, setDurationOriginal] = useState(0)
    const [isSeekingOriginal, setIsSeekingOriginal] = useState(false)

    // Preview audio player state
    const [isPlayingPreview, setIsPlayingPreview] = useState(false)
    const [currentTimePreview, setCurrentTimePreview] = useState(0)
    const [durationPreview, setDurationPreview] = useState(0)
    const [isSeekingPreview, setIsSeekingPreview] = useState(false)

    // Sidebar state
    const [backgroundSounds, setBackgroundSounds] = useState<BackgroundSound[]>([])
    const [showBgSoundSelector, setShowBgSoundSelector] = useState(false)
    const [showMoodSelector, setShowMoodSelector] = useState(false)

    // EQ state
    const [eqConfig, setEqConfig] = useState(presets["Flat"])
    const [selectedPreset, setSelectedPreset] = useState("Flat")
    const [selectedMood, setSelectedMood] = useState("")

    const [currentFileSource, setCurrentFileSource] = useState<'server' | 'local'>('server');
    const [previewReady, setPreviewReady] = useState(false);
    const [lastPreviewSignature, setLastPreviewSignature] = useState<{
        fileName: string;
        fileSize: number;
        eq: Record<string, number>;
        mood: string;
    } | null>(null);

    const [loading, setLoading] = useState(false);
    const [previewLoading, setPreviewLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [saveChoiceOpen, setSaveChoiceOpen] = useState(false);

    const { startPolling } = useSagaPolling({
        timeoutSeconds: 200,
        intervalSeconds: 5,
    })


    const fetchBackgroundSounds = async () => {
        setLoading(true);
        try {
            const res = await getBackgroundSounds(loginRequiredAxiosInstance);
            console.log("Fetched bg list:", res.data.BackgroundSoundTrackList);
            if (res.success && res.data) {
                setBackgroundSounds(res.data.BackgroundSoundTrackList || []);
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
        const loadServerAudio = async () => {
            if (!episodeDetail?.AudioFileKey) {
                setAudioUrl(null);
                setUploadedFile(null);
                return;
            }
            try {
                const res = await getAudioFile(loginRequiredAxiosInstance, episodeDetail.AudioFileKey);
                if (res.success && res.data?.FileUrl) {
                    setAudioUrl(res.data.FileUrl);
                    const blob = await fetch(res.data.FileUrl).then(r => r.blob());
                    const fileName = buildEpisodeAudioFileName(episodeDetail, blob.type);
                    setUploadedFile(new File([blob], fileName, { type: blob.type }));
                    setCurrentFileSource('server');
                    setPreviewReady(false);
                    setLastPreviewSignature(null);
                }
            } catch (e) {
                toast.error('Load audio failed');
            }
        };
        loadServerAudio();
        fetchBackgroundSounds()
    }, [episodeDetail?.AudioFileKey]);



    const handlePresetChange = (preset: string) => {
        setSelectedPreset(preset)
        setEqConfig(presets[preset])
    }
    const handleMoodChange = (mood: string) => {
        setSelectedMood(mood)
    }
    // ============ Background merge ============

    const timelineScrollRef = useRef<HTMLDivElement>(null)

    const progressBarRefSequencer = useRef<HTMLDivElement>(null)
    const [selectedClipId, setSelectedClipId] = useState<string | null>(null);
    const rulerScrollRef = useRef<HTMLDivElement>(null) 

    // Sequencer state
    const [clips, setClips] = useState<Clip[]>([])
    const [pixelsPerSecond, setPPS] = useState(30) 

    const [isPlayingSequencer, setIsPlayingSequencer] = useState(false)
    const [playhead, setPlayhead] = useState(0)
    const [trackVolumes, setTrackVolumes] = useState([1, 1])

    // Audio context refs for sequencer
    const acRef = useRef<AudioContext | null>(null)
    const startWallClockRef = useRef(0)
    const startPlayheadRef = useRef(0)
    const activeNodesRef = useRef<Array<{ src: AudioBufferSourceNode }>>([])
    const rafRef = useRef<number | undefined>(undefined)
    const trackGainsRef = useRef<Array<{ gain: GainNode }>>([])
    const rowH = 120 

    const [segmentPlayingClipId, setSegmentPlayingClipId] = useState<string | null>(null);



    const ensureTrackGains = useCallback(() => {
        const ac = acRef.current || getAC()
        while (trackGainsRef.current.length < 2) {
            const gain = ac.createGain()
            gain.gain.value = 1
            gain.connect(ac.destination)
            trackGainsRef.current.push({ gain })
        }
        trackGainsRef.current[0].gain.gain.value = trackVolumes[0] ?? 1
        trackGainsRef.current[1].gain.gain.value = trackVolumes[1] ?? 1
    }, [trackVolumes])

    // Stop all audio nodes
    const stopAll = useCallback(() => {
        activeNodesRef.current.forEach(({ src }) => {
            try {
                src.stop()
            } catch { }
        })
        activeNodesRef.current = []
        if (rafRef.current) cancelAnimationFrame(rafRef.current)
    }, [])

    const findAvailablePosition = useCallback((newDuration: number, existingBgClips: Clip[]) => {
        if (existingBgClips.length === 0) {
            return 0 // No existing clips, start at beginning
        }

        // Sort existing clips by start time
        const sortedClips = existingBgClips
            .map(clip => ({
                start: clip.start,
                end: clip.start + Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
            }))
            .sort((a, b) => a.start - b.start)

        // Check if we can fit at the beginning
        // if (sortedClips[0].start >= newDuration) {
        //     return 0
        // }

        // Check gaps between clips
        for (let i = 0; i < sortedClips.length - 1; i++) {
            const gapStart = sortedClips[i].end
            const gapEnd = sortedClips[i + 1].start
            const gapSize = gapEnd - gapStart

            if (gapSize >= newDuration) {
                return gapStart
            }
        }

        // Place after the last clip
        const lastClipEnd = sortedClips[sortedClips.length - 1].end
        return lastClipEnd
    }, [])

    const fetchTrackAudioUrl = useCallback(async (fileKey: string) => {
        try {
            const res: any = await getBackgroundSoundFile(loginRequiredAxiosInstance, fileKey);
            if (res?.success && res?.data?.FileUrl) {
                return { success: true, data: { FileUrl: res.data.FileUrl } };
            }
            return { success: false, message: typeof res?.message === 'string' ? res.message : 'Unable to fetch audio URL' };
        } catch (e: any) {
            return { success: false, message: e?.message || 'Error fetching audio URL' };
        }
    }, []);
    const getTotalBackgroundVisibleSeconds = useCallback(() => {
        return clips
            .filter(c => c.track === 1)
            .reduce((sum, c) => sum + Math.max(0, c.duration - c.trimStart - c.trimEnd), 0);
    }, [clips]);

    const computeBgGaps = useCallback((originalDurationSec: number) => {
        const existing = clips
            .filter(c => c.track === 1)
            .map(c => {
                const vis = Math.max(0, c.duration - c.trimStart - c.trimEnd)
                return { start: c.start, end: c.start + vis }
            })
            .sort((a, b) => a.start - b.start)

        const gaps: Array<{ start: number; end: number; size: number }> = []
        // Gap đầu từ 0 → clip đầu tiên
        if (existing.length === 0) {
            gaps.push({ start: 0, end: originalDurationSec, size: originalDurationSec })
            return gaps
        }
        if (existing[0].start > 0) {
            gaps.push({ start: 0, end: existing[0].start, size: existing[0].start - 0 })
        }
        // Gaps giữa các clip
        for (let i = 0; i < existing.length - 1; i++) {
            const gapStart = existing[i].end
            const gapEnd = existing[i + 1].start
            if (gapEnd > gapStart) {
                gaps.push({ start: gapStart, end: gapEnd, size: gapEnd - gapStart })
            }
        }
        // Gap cuối từ sau clip cuối → hết original
        const lastEnd = existing[existing.length - 1].end
        if (originalDurationSec > lastEnd) {
            gaps.push({ start: lastEnd, end: originalDurationSec, size: originalDurationSec - lastEnd })
        }
        return gaps
    }, [clips])

    // const handleAddBackgroundToSequencer = useCallback(async (bgSound: BackgroundSound) => {

    //     if (!uploadedFile) return

    //     try {
    //         const ac = acRef.current || getAC()
    //         acRef.current = ac
    //         const res = await fetchTrackAudioUrl(bgSound.AudioFileKey)
    //         if (!res.success) {
    //             toast.error("Không lấy được file âm thanh");
    //             return;
    //         }
    //         const audioUrl = res.data.FileUrl;

    //         const response = await fetch(audioUrl);
    //         const arrayBuffer = await response.arrayBuffer();
    //         const buffer = await ac.decodeAudioData(arrayBuffer.slice(0));


    //         const originalClip = clips.find(c => c.track === 0);
    //         const originalDurationSec = originalClip?.duration ?? 0;
    //         const currentBgTotal = getTotalBackgroundVisibleSeconds();

    //         console.log("Original Duration:", originalDurationSec, "Current BG Total:", currentBgTotal);
    //         const gaps = computeBgGaps(originalDurationSec)

    //         if (gaps.length === 0) {
    //             toast.error("No available gap to place background")
    //             return
    //         }

    //         // 3. Tìm thời điểm đặt clip
    //         const existingBgClips = clips.filter(c => c.track === 1);
    //         const optimalStart = findAvailablePosition(buffer.duration, existingBgClips);
    //         const blob = new Blob([arrayBuffer], { type: 'audio/mpeg' });
    //         const file = new File([blob], bgSound.Name, { type: 'audio/mpeg' });
    //         if ((originalDurationSec - optimalStart) >= buffer.duration) {
    //             const newClip: Clip = {
    //                 id: `${bgSound.Id}-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
    //                 name: bgSound.Name,
    //                 file,
    //                 fileKey: bgSound.AudioFileKey,
    //                 buffer,
    //                 duration: buffer.duration,
    //                 start: optimalStart,
    //                 trimStart: 0,
    //                 trimEnd: 0,
    //                 track: 1, // Background track
    //                 volume: -5, // Default volume -5dB for background

    //                 fadeInSec: 0.5,
    //                 fadeOutSec: 0.5,
    //             }

    //             setClips(prev => [...prev, newClip])
    //             setShowBgSoundSelector(true)
    //             toast.success(`Added ${bgSound.Name} to sequencer at ${Math.floor(optimalStart)}s`)
    //             return;
    //         } else if ((originalDurationSec - optimalStart) < buffer.duration && (originalDurationSec - optimalStart) >= 5) {
    //             const remainingSec = Math.max(0, originalDurationSec - optimalStart); // khoảng trống còn lại
    //             const visibleDur = remainingSec; // hiển thị đúng bằng khoảng trống
    //             const trimStart = 0;
    //             const trimEnd = Math.max(0, buffer.duration - visibleDur); // cắt bên phải
    //             const newClip: Clip = {
    //                 id: `${bgSound.Id}-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
    //                 name: bgSound.Name,
    //                 file,
    //                 fileKey: bgSound.AudioFileKey,
    //                 buffer,
    //                 duration: buffer.duration,
    //                 start: optimalStart,
    //                 trimStart,
    //                 trimEnd,
    //                 track: 1,
    //                 volume: -5,
    //                 fadeInSec: 0.5,
    //                 fadeOutSec: 0.5,
    //             };
    //             const existingBgClips = clips.filter(c => c.track === 1);
    //             const newEnd = optimalStart + visibleDur;
    //             const hasConflict = existingBgClips.some(c => {
    //                 const cVis = Math.max(0, c.duration - c.trimStart - c.trimEnd);
    //                 const cStart = c.start;
    //                 const cEnd = c.start + cVis;
    //                 return !(newEnd <= cStart || optimalStart >= cEnd);
    //             });
    //             if (hasConflict) {
    //                 toast.error("Cannot add: it would overlap another background sound");
    //                 return;
    //             }
    //             setClips(prev => [...prev, newClip])
    //             setShowBgSoundSelector(true)
    //             toast.success(`Added ${bgSound.Name} trimmed to ${visibleDur.toFixed(1)}s at ${Math.floor(optimalStart)}s`);
    //             return;
    //         } else if (
    //             (originalDurationSec - optimalStart) < 5
    //         ) {
    //             toast.error('Not enough space to add this background sound.')
    //             return;
    //         }

    //     } catch (error) {
    //         console.error('Failed to load background sound:', error)
    //         toast.error('Failed to load background sound')
    //     }
    // }, [uploadedFile, clips, findAvailablePosition])

    const handleAddBackgroundToSequencer = useCallback(async (bgSound: BackgroundSound) => {
        if (!uploadedFile) return;

        try {
            const ac = acRef.current || getAC();
            acRef.current = ac;
            const res = await fetchTrackAudioUrl(bgSound.AudioFileKey);
            if (!res.success) {
                toast.error("Không lấy được file âm thanh");
                return;
            }
            const audioUrl = res.data.FileUrl;

            const response = await fetch(audioUrl);
            const arrayBuffer = await response.arrayBuffer();
            const buffer = await ac.decodeAudioData(arrayBuffer.slice(0));

            const originalClip = clips.find(c => c.track === 0);
            const originalDurationSec = originalClip?.duration ?? 0;

            // Chỉ tìm background xa nhất hoặc đặt ở 0
            const existingBgClips = clips.filter(c => c.track === 1);
            let optimalStart = 0;
            if (existingBgClips.length > 0) {
                // luôn đặt sau clip background cuối
                const sorted = existingBgClips
                    .map(c => ({
                        start: c.start,
                        end: c.start + Math.max(0, c.duration - c.trimStart - c.trimEnd)
                    }))
                    .sort((a, b) => a.start - b.start);
                optimalStart = sorted[sorted.length - 1].end;
            }

            // Kiểm tra có đủ chỗ không
            const spaceAvailable = originalDurationSec - optimalStart;
            if (spaceAvailable < 5) {
                toast.error('Not enough space after the last background (min 5s required).');
                return;
            }

            const blob = new Blob([arrayBuffer], { type: 'audio/mpeg' });
            const file = new File([blob], bgSound.Name, { type: 'audio/mpeg' });

            if (spaceAvailable >= buffer.duration) {
                // Đủ chỗ cho cả clip
                const newClip: Clip = {
                    id: `${bgSound.Id}-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
                    name: bgSound.Name,
                    file,
                    fileKey: bgSound.AudioFileKey,
                    buffer,
                    duration: buffer.duration,
                    start: optimalStart,
                    trimStart: 0,
                    trimEnd: 0,
                    track: 1,
                    volume: -5,
                    fadeInSec: 0.5,
                    fadeOutSec: 0.5,
                };
                setClips(prev => [...prev, newClip]);
                setShowBgSoundSelector(true);
                toast.success(`Added ${bgSound.Name} to sequencer at ${Math.floor(optimalStart)}s`);
            } else {
                // Trim bên phải để vừa khoảng còn lại
                const visibleDur = spaceAvailable;
                const trimEnd = Math.max(0, buffer.duration - visibleDur);
                const newClip: Clip = {
                    id: `${bgSound.Id}-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
                    name: bgSound.Name,
                    file,
                    fileKey: bgSound.AudioFileKey,
                    buffer,
                    duration: buffer.duration,
                    start: optimalStart,
                    trimStart: 0,
                    trimEnd,
                    track: 1,
                    volume: -5,
                    fadeInSec: 0.5,
                    fadeOutSec: 0.5,
                };
                setClips(prev => [...prev, newClip]);
                setShowBgSoundSelector(true);
                toast.success(`Added ${bgSound.Name} trimmed to ${visibleDur.toFixed(1)}s at ${Math.floor(optimalStart)}s`);
            }
        } catch (error) {
            console.error('Failed to load background sound:', error);
            toast.error('Failed to load background sound');
        }
    }, [uploadedFile, clips]);



    // Sequencer playback functions
    const originalDuration = useMemo(() => {
        const clip = clips.find((c) => c.track === 0)
        return clip ? clip.duration : 30
    }, [clips])

    const totalLengthSec = useMemo(() => {
        const mx = clips.reduce((m, c) => {
            const vis = Math.max(0, c.duration - c.trimStart - c.trimEnd)
            return Math.max(m, c.start + vis)
        }, originalDuration)
        return Math.max(mx, originalDuration)
    }, [clips, originalDuration])

    const computeDynamicFades = (clipsIn: Clip[]) => {
        const DEFAULT = 0.5, BUTT_HALF = 0.25, EPS = 1e-6;
        const bg = clipsIn
            .filter(c => c.track === 1)
            .map(c => {
                const vis = Math.max(0, c.duration - c.trimStart - c.trimEnd);
                return { id: c.id, start: c.start, end: c.start + vis, vis };
            })
            .sort((a, b) => a.start - b.start);

        const map = new Map<string, { fi: number; fo: number }>();
        // mặc định: 0.5s ở hai đầu
        bg.forEach(seg => map.set(seg.id, { fi: DEFAULT, fo: DEFAULT }));

        // chỉ điều chỉnh khi sát nhau (butt-join)
        for (let i = 0; i < bg.length - 1; i++) {
            const a = bg[i], b = bg[i + 1];
            const gap = Math.max(0, b.start - a.end);
            if (gap <= EPS) {
                // A.fo = 0.25s, B.fi = 0.25s
                map.set(a.id, { fi: map.get(a.id)!.fi, fo: BUTT_HALF });
                map.set(b.id, { fi: BUTT_HALF, fo: map.get(b.id)!.fo });
            }
            // Không có else: các trường hợp khác giữ DEFAULT
        }

        // Debug optional
        bg.forEach(s => {
            const v = map.get(s.id)!;
            console.log(`(${s.id}) fi=${v.fi.toFixed(3)}s, fo=${v.fo.toFixed(3)}s`);
        });

        return map;
    }

    const scheduleFrom = useCallback((fromSec: number) => {
        const ac = acRef.current || getAC()
        ensureTrackGains()

        stopAll()

        startWallClockRef.current = ac.currentTime
        startPlayheadRef.current = fromSec

        const fadeMap = computeDynamicFades(clips)

        for (let t = 0; t < 2; t++) {
            const tGain = trackGainsRef.current[t]?.gain || ac.destination
            const tClips = clips.filter((c) => c.track === t)

            const scheduleClipAt = (clip: Clip, offsetSec: number) => {
                const playStart = clip.start + offsetSec
                const clipVis = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
                const playEnd = playStart + clipVis
                if (clipVis <= 0) return
                if (fromSec < playEnd) {
                    const when = Math.max(0, playStart - fromSec)
                    const offset = Math.max(0, fromSec - playStart) + clip.trimStart
                    let dur = clipVis - Math.max(0, fromSec - playStart)
                    if (dur <= 0) return

                    const src = ac.createBufferSource()
                    src.buffer = clip.buffer

                    if (clip.track === 1) {
                        const clipGain = ac.createGain()
                        const linearBase = Math.pow(10, (clip.volume ?? -5) / 20)

                        // Lấy fade động, không dựa state
                        let { fi, fo } = fadeMap.get(clip.id) ?? { fi: 0.5, fo: 0.5 }
                        fi = Math.max(0, Math.min(fi, dur))
                        fo = Math.max(0, Math.min(fo, dur))
                        const totalFade = fi + fo
                        if (totalFade > dur && totalFade > 0) {
                            const scale = dur / totalFade
                            fi *= scale
                            fo *= scale
                        }

                        clipGain.gain.value = 0
                        src.connect(clipGain)
                        clipGain.connect(tGain)

                        const startTime = ac.currentTime + when
                        const endTime = startTime + dur
                        console.log('[Schedule BG]', {
                            name: clip.name,
                            fi: fi.toFixed(3),
                            fo: fo.toFixed(3),
                            linearBase: linearBase.toFixed(3),
                        })
                        if (fi > 0) {
                            clipGain.gain.cancelScheduledValues(startTime)
                            clipGain.gain.setValueAtTime(0, startTime)
                            clipGain.gain.linearRampToValueAtTime(linearBase, startTime + fi)
                        } else {
                            clipGain.gain.setValueAtTime(linearBase, startTime)
                        }
                        if (fo > 0) {
                            const fadeOutStart = Math.max(startTime, endTime - fo)
                            clipGain.gain.setValueAtTime(linearBase, fadeOutStart)
                            clipGain.gain.linearRampToValueAtTime(0.0001, endTime)
                        }
                    } else {
                        src.connect(tGain)
                    }

                    try {
                        src.start(ac.currentTime + when, offset, Math.max(0, dur))
                        activeNodesRef.current.push({ src })
                    } catch { }
                }
            }

            tClips.forEach((c) => scheduleClipAt(c, 0))
        }

        const tick = () => {
            const elapsed = ac.currentTime - startWallClockRef.current
            const ph = startPlayheadRef.current + elapsed
            setPlayhead(ph)
            if (ph >= totalLengthSec) {
                setIsPlayingSequencer(false)
                setPlayhead(0)
                return
            }
            rafRef.current = requestAnimationFrame(tick)
        }
        rafRef.current = requestAnimationFrame(tick)
    }, [clips, totalLengthSec, ensureTrackGains, stopAll])

    const handleSequencerPlay = useCallback(() => {
        if (isPlayingSequencer) return
        getAC().resume()
        setIsPlayingSequencer(true)
        scheduleFrom(playhead)
    }, [isPlayingSequencer, playhead, scheduleFrom])

    const handleSequencerPause = useCallback(() => {
        setIsPlayingSequencer(false)
        stopAll()
    }, [stopAll])

    const handleSequencerStop = useCallback(() => {
        handleSequencerPause()
        setPlayhead(0)
    }, [handleSequencerPause])

    useEffect(() => {
        ensureTrackGains()
    }, [ensureTrackGains])

    useEffect(() => () => stopAll(), [stopAll])

    // Auto-scroll timeline to follow playhead
    useEffect(() => {
        if (isPlayingSequencer && timelineScrollRef.current) {
            const scrollContainer = timelineScrollRef.current
            const playheadPosition = playhead * pixelsPerSecond
            const containerWidth = scrollContainer.clientWidth
            const scrollLeft = scrollContainer.scrollLeft

            // Scroll if playhead is near the right edge or out of view
            if (playheadPosition > scrollLeft + containerWidth - 200) {
                scrollContainer.scrollTo({
                    left: playheadPosition - containerWidth / 2,
                    behavior: 'smooth'
                })
            }
        }
    }, [playhead, isPlayingSequencer, pixelsPerSecond])

    useEffect(() => {
        const rulerEl = rulerScrollRef.current;
        const timelineEl = timelineScrollRef.current;
        if (!rulerEl || !timelineEl) return;

        let isSyncing = false;

        const onRulerScroll = () => {
            if (isSyncing) return;
            isSyncing = true;
            timelineEl.scrollLeft = rulerEl.scrollLeft;
            requestAnimationFrame(() => { isSyncing = false; });
        };

        const onTimelineScroll = () => {
            if (isSyncing) return;
            isSyncing = true;
            rulerEl.scrollLeft = timelineEl.scrollLeft;
            requestAnimationFrame(() => { isSyncing = false; });
        };

        rulerEl.addEventListener('scroll', onRulerScroll, { passive: true });
        timelineEl.addEventListener('scroll', onTimelineScroll, { passive: true });

        return () => {
            rulerEl.removeEventListener('scroll', onRulerScroll);
            timelineEl.removeEventListener('scroll', onTimelineScroll);
        };
    }, [showBgSoundSelector]);


    const buildBgMergePayload = useCallback(() => {
        if (!showBgSoundSelector) return null;
        const bgClips = clips.filter(c => c.track === 1);
        if (bgClips.length === 0) return null;

        const ranges = bgClips
            .map((clip) => {
                const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd);
                return {
                    VolumeGainDb: Number.isFinite(clip.volume as number) ? Number((clip.volume as number).toFixed(1)) : -5,
                    BackgroundSoundTrackFileKey: clip.fileKey,
                    BackgroundCutStartSecond: Number(clip.trimStart.toFixed(3)),
                    BackgroundCutEndSecond: Number((clip.duration - clip.trimEnd).toFixed(3)),
                    OriginalMergeStartSecond: Number(clip.start.toFixed(3)),
                    OriginalMergeEndSecond: Number((clip.start + visibleDur).toFixed(3)),
                };
            })
            .sort((a, b) => a.OriginalMergeStartSecond - b.OriginalMergeStartSecond);

        return { TimeRangeMergeBackgrounds: ranges };
    }, [clips, showBgSoundSelector]);

    // Signature để theo dõi thay đổi background merge
    const bgMergeSig = useMemo(() => {
        const payload = buildBgMergePayload();
        return payload ? JSON.stringify(payload) : null;
    }, [buildBgMergePayload]);

    useEffect(() => {
        if (!uploadedFile) return

        const loadOriginalAudio = async () => {
            try {
                const ac = getAC()
                acRef.current = ac

                const buffer = await ac.decodeAudioData(await uploadedFile.arrayBuffer())
                const originalClip: Clip = {
                    id: `${uploadedFile.name}-${Date.now()}-0`,
                    name: uploadedFile.name,
                    file: uploadedFile,
                    fileKey: "",
                    buffer,
                    duration: buffer.duration,
                    start: 0,
                    trimStart: 0,
                    trimEnd: 0,
                    track: 0, // Original track
                }

                setClips(prev => {
                    const filtered = prev.filter(c => c.track !== 0)
                    return [...filtered, originalClip]
                })

            } catch (error) {
                console.error("Failed to load original audio:", error)
            }
        }
        loadOriginalAudio()
    }, [uploadedFile])

    // ============ CHANGE TRACKING FOR PREVIEW ============

    const baselineRef = useRef<{
        eqConfig: Record<string, number>
        selectedMood: string
        moodEnabled: boolean
        bgSig: string | null
    } | null>(null)
    const [baselineTick, setBaselineTick] = useState(0)

    useEffect(() => {
        if (!audioUrl) {
            baselineRef.current = null
        } else {
            baselineRef.current = {
                eqConfig: { ...eqConfig },
                selectedMood,
                moodEnabled: showMoodSelector,
                bgSig: null,
            }
        }
        setBaselineTick((t) => t + 1)
    }, [audioUrl])

    const isEqEqual = (a: Record<string, number>, b: Record<string, number>) => {
        const keys = new Set([...Object.keys(a || {}), ...Object.keys(b || {})])
        for (const k of keys) {
            if ((a?.[k] ?? 0) !== (b?.[k] ?? 0)) return false
        }
        return true
    }

    const hasPreviewChanges = useMemo(() => {
        if (!audioUrl) return false
        const base = baselineRef.current
        if (!base) return false
        const eqOrMoodChanged =
            !isEqEqual(eqConfig, base.eqConfig) ||
            selectedMood !== base.selectedMood ||
            showMoodSelector !== base.moodEnabled

        const bgChanged = (base.bgSig ?? null) !== (bgMergeSig ?? null)

        return eqOrMoodChanged || bgChanged
    }, [audioUrl, eqConfig, selectedMood, showMoodSelector, bgMergeSig, baselineTick])

    // ============ WAVEFORM INITIALIZATION ============

    useEffect(() => {
        if (!waveformRefOriginal.current) return

        const ws = WaveSurfer.create({
            container: waveformRefOriginal.current,
            waveColor: "#7BA225",
            progressColor: "#AEE339",
            cursorColor: "#AEE339",
            barWidth: 2,
            barGap: 2,
            fillParent: true,
            minPxPerSec: 30,
            barRadius: 2,
            height: 130,
            interact: false,
            hideScrollbar: true,
        })

        wavesurferRefOriginal.current = ws

        if (audioUrl) {
            ws.load(audioUrl)
        }

        ws.on("ready", () => {
            setDurationOriginal(ws.getDuration())
        })

        ws.on("play", () => setIsPlayingOriginal(true))
        ws.on("pause", () => setIsPlayingOriginal(false))
        ws.on("finish", () => {
            setIsPlayingOriginal(false)
            setCurrentTimeOriginal(0)
        })

        return () => {
            ws.destroy()
        }
    }, [audioUrl])

    useEffect(() => {
        return () => {
            if (previewUrl) URL.revokeObjectURL(previewUrl)
        }
    }, [previewUrl])

    useEffect(() => {
        if (!waveformRefPreview.current || !previewUrl) return

        const ws = WaveSurfer.create({
            container: waveformRefPreview.current,
            waveColor: "#7BA225",
            progressColor: "#AEE339",
            cursorColor: "#AEE339",
            barWidth: 2,
            barGap: 2,
            fillParent: true,
            minPxPerSec: 30,
            barRadius: 2,
            height: 130,
            interact: false,
            hideScrollbar: true,
        })

        wavesurferRefPreview.current = ws
        ws.load(previewUrl)

        ws.on("ready", () => {
            setDurationPreview(ws.getDuration())
        })

        ws.on("play", () => setIsPlayingPreview(true))
        ws.on("pause", () => setIsPlayingPreview(false))
        ws.on("finish", () => {
            setIsPlayingPreview(false)
            setCurrentTimePreview(0)
        })

        return () => {
            ws.destroy()
        }
    }, [previewUrl])

    // ============ TIME UPDATE ANIMATION FRAMES ============
    useEffect(() => {
        let animationFrameId: number

        const updateTime = () => {
            if (wavesurferRefOriginal.current?.isPlaying()) {
                setCurrentTimeOriginal(wavesurferRefOriginal.current.getCurrentTime())
            }
            if (wavesurferRefPreview.current?.isPlaying()) {
                setCurrentTimePreview(wavesurferRefPreview.current.getCurrentTime())
            }
            animationFrameId = requestAnimationFrame(updateTime)
        }

        animationFrameId = requestAnimationFrame(updateTime)
        return () => cancelAnimationFrame(animationFrameId)
    }, [])

    // ============ SEEKING HANDLERS ============
    const handleSeekMouseDown = (e: React.MouseEvent<HTMLDivElement>, isPreview: boolean) => {
        if (isPreview) {
            const rect = progressBarRefPreview.current.getBoundingClientRect();
            const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1);
            const newTime = percent * durationPreview;

            // Xử lý click ngay lập tức
            wavesurferRefPreview.current.setTime(newTime);
            setCurrentTimePreview(newTime);
            setIsSeekingPreview(true)
        } else {
            const rect = progressBarRefOriginal.current.getBoundingClientRect();
            const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1);
            const newTime = percent * durationOriginal;

            // Xử lý click ngay lập tức
            wavesurferRefOriginal.current.setTime(newTime);
            setIsSeekingOriginal(true)
        }
    }

    useEffect(() => {
        const handleMouseMove = (e: MouseEvent) => {
            if (isSeekingOriginal && progressBarRefOriginal.current && wavesurferRefOriginal.current) {
                const rect = progressBarRefOriginal.current.getBoundingClientRect()
                const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1)
                const newTime = percent * durationOriginal
                wavesurferRefOriginal.current.setTime(newTime)
                setCurrentTimeOriginal(newTime)
            }

            if (isSeekingPreview && progressBarRefPreview.current && wavesurferRefPreview.current) {
                const rect = progressBarRefPreview.current.getBoundingClientRect()
                const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1)
                const newTime = percent * durationPreview
                wavesurferRefPreview.current.setTime(newTime)
                setCurrentTimePreview(newTime)
            }
        }

        const handleMouseUp = () => {
            setIsSeekingOriginal(false)
            setIsSeekingPreview(false)
        }

        if (isSeekingOriginal || isSeekingPreview) {
            window.addEventListener("mousemove", handleMouseMove)
            window.addEventListener("mouseup", handleMouseUp)
        }

        return () => {
            window.removeEventListener("mousemove", handleMouseMove)
            window.removeEventListener("mouseup", handleMouseUp)
        }
    }, [isSeekingOriginal, isSeekingPreview, durationOriginal, durationPreview])

    // ============ FILE HANDLING ============
    const resetForNewFile = (file: File) => {
        const url = URL.createObjectURL(file);
        setAudioUrl(url);
        setUploadedFile(file);
        setCurrentFileSource('local');

        // Always reset preview-related state
        setPreviewReady(false);
        setPreviewUrl(null);
        setPreviewFile(null);
        setLastPreviewSignature(null);
        baselineRef.current = null;
        setBaselineTick(t => t + 1);

        setEqConfig(presets["Flat"]);
        setSelectedPreset("Flat");
        setSelectedMood("");
        setShowMoodSelector(false);
        setShowBgSoundSelector(false);

        setClips([]);

    };

    const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;
        const allowedExtensions = ['wav', 'flac', 'mp3', 'm4a', 'aac'];
        const ext = file.name.split('.').pop()?.toLowerCase();
        if (!ext || !allowedExtensions.includes(ext)) {
            toast.error('Allowed audio types: wav, flac, mp3, m4a, aac');
            return;
        }

        if (file.size > 150 * 1024 * 1024) {
            toast.error("File size exceeds 150MB limit.");
            return;
        }
        if (!file.type.startsWith('audio/')) {
            toast.error("Unsupported file type.");
            return;
        }
        resetForNewFile(file);
    };

    const handleDragOver = (e: React.DragEvent) => {
        e.preventDefault()
        setIsDragging(true)
    }

    const handleDragLeave = () => setIsDragging(false)

    const handleDrop = (e: React.DragEvent) => {
        e.preventDefault()
        setIsDragging(false)
        const file = e.dataTransfer.files?.[0]
        if (!file) return

        if (file.size > 150 * 1024 * 1024) {
            alert("File size exceeds 150MB limit.")
            return
        }

        if (file.type.startsWith('audio/')) {
            const url = URL.createObjectURL(file);
            setAudioUrl(url);
            setUploadedFile(file);
            setCurrentFileSource('local');
            setPreviewReady(false);
            setPreviewUrl(null);
            setPreviewFile(null);
            setLastPreviewSignature(null);
        }
    }
    const currentSignature = useMemo(() => {
        if (!uploadedFile) return null;
        return {
            fileName: uploadedFile.name,
            fileSize: uploadedFile.size,
            eq: eqConfig,
            mood: selectedMood,

        };
    }, [uploadedFile, eqConfig, selectedMood]);

    const hasUnsavedPreview = useMemo(() => {
        if (!previewReady || !lastPreviewSignature || !currentSignature) return false;
        const diff =
            lastPreviewSignature.fileName !== currentSignature.fileName ||
            lastPreviewSignature.fileSize !== currentSignature.fileSize ||
            !isEqEqual(lastPreviewSignature.eq, currentSignature.eq) ||
            lastPreviewSignature.mood !== currentSignature.mood
        return diff;
    }, [previewReady, lastPreviewSignature, currentSignature]);

    // ============ UTILITY FUNCTIONS ============
    const formatTime = (seconds: number) => {
        const mins = Math.floor(seconds / 60)
        const secs = Math.floor(seconds % 60)
        return `${mins}:${secs.toString().padStart(2, "0")}`
    }

    const handlePlayPause = (isPreview: boolean) => {
        if (isPreview) {
            wavesurferRefPreview.current?.playPause()
        } else {
            wavesurferRefOriginal.current?.playPause()
        }
    }


    const handleAddMood = () => {
        setShowMoodSelector((prev) => {
            const next = !prev
            if (next) {
                if (!selectedMood) setSelectedMood('Mysterious')
            } else {
                setSelectedMood('')
            }
            return next
        })
    }

    const handlePreview = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        if (!uploadedFile) return;
        setPreviewLoading(true)
        const bgPayload = buildBgMergePayload();
        console.log("Background Merge Payload:", bgPayload)
        const payload = {
            GeneralTuningProfileRequestInfo: {
                EqualizerProfile: {
                    ExpandEqualizer: { Mood: selectedMood },
                    BaseEqualizer: {
                        HighMid: eqConfig.HighMid,
                        Low: eqConfig.Low,
                        LowMid: eqConfig.LowMid,
                        Mid: eqConfig.Mid,
                        Presence: eqConfig.Presence,
                        SubBass: eqConfig.SubBass,
                        Treble: eqConfig.Treble,
                        Air: eqConfig.Air,
                        Bass: eqConfig.Bass
                    }
                },
                BackgroundMergeProfile: null,
                MultipleTimeRangeBackgroundMergeProfile: bgPayload, // null nếu đóng / không có clip
                AITuningProfile: null,
            },
            AudioFile: uploadedFile
        }
        console.log("Audio Tuning Payload:", payload)
        const response = await audioTuning(loginRequiredAxiosInstance, episodeId, payload)
        console.log("Audio Tuning Response:", response)
        if (response.success && response.data) {
            const blob = response.data;
            const file = new File([blob], "preview-audio.mp3", { type: blob.type || "audio/mpeg" });
            const url = URL.createObjectURL(blob);
            setPreviewUrl(url);
            setPreviewFile(file);
            setPreviewReady(true);
            setLastPreviewSignature(currentSignature);
        } else {
            toast.error(response.message.content || "Thất bại, vui lòng thử lại !")
        }
        setPreviewLoading(false);
        baselineRef.current = {
            eqConfig: { ...eqConfig },
            selectedMood,
            moodEnabled: showMoodSelector,
            bgSig: bgMergeSig ?? null,

        }
        setBaselineTick((t) => t + 1)
        console.log('Previewing audio...', { eqConfig, selectedMood })
    }

    const performSave = async (fileToSave: File) => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        try {
            setSaving(true);
            const payload = { AudioFile: fileToSave };
            console.log("Upload Audio Payload:", payload);
            const res = await uploadAudio(loginRequiredAxiosInstance, episodeId, payload);
            const sagaId = res?.data?.SagaInstanceId;
            if (!res.success && res.message.content) {
                toast.error(res.message.content);
                return;
            }
            if (!sagaId) {
                toast.error('Saving episode failed, please try again.');
                return;
            }
            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: async () => {
                    toast.success('Audio saved successfully.');
                    await refreshEpisode?.();
                },
                onFailure: (err) => toast.error(err || 'Saga failed!'),
                onTimeout: () => toast.error('System not responding, please try again.'),
            });
        } catch (error) {
            toast.error('Error saving audio');
        } finally {
            setSaving(false);
        }
    };

    const handleSaveClick = async () => {
        if (authSlice.user?.ViolationLevel > 0) {
            toast.error('Your account is currently under violation !!');
            return;
        }
        if (episodeDetail && episodeDetail.CurrentStatus.Id === 5) {
            toast.error("Cannot change audio for Published episodes, please Unpublish first");
            return;
        }
        if (episodeDetail && episodeDetail.CurrentStatus.Id === 4) {
            const alert = await confirmAlert("If you save changes now, you must Request to Publish again. Do you want to continue?");
            if (!alert.isConfirmed) return;
        }
        if (episodeDetail && episodeDetail.CurrentStatus.Id === 2) {
            toast.error("This episode is Pending Review, please Discard Publish Request first");
            return;
        }

        if (saving || previewLoading) return;
        if (saveDisabled) {
            if (currentFileSource === 'server') {
                toast.info('Preview the server audio before saving.');
            }
            return;
        }
        if (!uploadedFile) return;

        // Server file: luôn save bản preview (đã được đảm bảo hợp lệ bởi saveDisabled)
        if (currentFileSource === 'server') {
            performSave(previewFile!);
            return;
        }

        // Local file: nếu có preview hợp lệ thì hỏi chọn; nếu chưa preview hoặc preview đã outdated thì lưu bản gốc
        const canChoose = previewReady && previewFile && !hasUnsavedPreview;
        if (canChoose) {
            setSaveChoiceOpen(true);
        } else {
            performSave(uploadedFile);
        }
    };

    const handleProgressClick = (e: React.MouseEvent<HTMLDivElement>, isPreview: boolean) => {
        if (isPreview) {
            if (!wavesurferRefPreview.current || !progressBarRefPreview.current) return;
            const rect = progressBarRefPreview.current.getBoundingClientRect();
            const clickX = e.clientX - rect.left;
            const percent = clickX / rect.width;
            const newTime = percent * durationPreview;
            wavesurferRefPreview.current.setTime(newTime);
            setCurrentTimePreview(newTime);
        } else {
            if (!wavesurferRefOriginal.current || !progressBarRefOriginal.current) return;
            const rect = progressBarRefOriginal.current.getBoundingClientRect();
            const clickX = e.clientX - rect.left;
            const percent = clickX / rect.width;
            const newTime = percent * durationOriginal;
            wavesurferRefOriginal.current.setTime(newTime);
            setCurrentTimeOriginal(newTime);
        }
    };
    // ============ RENDER ============
    const handleResetAudio = (isPreview: boolean) => {
        if (isPreview) {
            wavesurferRefPreview.current?.setTime(0);
            setCurrentTimePreview(0);
        } else {
            wavesurferRefOriginal.current?.setTime(0);
            setCurrentTimeOriginal(0);
        }
    };
    const hasAnyBg = useMemo(() => clips.some(c => c.track === 1), [clips])

    const previewDisabled =
        !uploadedFile ||
        previewLoading ||
        saving ||
        !hasPreviewChanges ||
        (showBgSoundSelector && !hasAnyBg && !hasPreviewChanges)


    const saveDisabled = useMemo(() => {
        if (!uploadedFile) return true; // rule 1
        if (currentFileSource === 'server') {
            // Chỉ được save khi đã render preview hợp lệ
            if (!previewReady) return true;
            if (!previewFile) return true;
            if (hasUnsavedPreview) return true;
            return false; // có preview hợp lệ
        }


        // local file: luôn có thể save
        return false;
    }, [uploadedFile, currentFileSource, previewReady, previewFile, hasUnsavedPreview, episodeDetail]);
    const shortenFileName = (name: string, max = 32) => {
        if (!name) return "";
        if (name.length <= max) return name;
        const match = name.match(/^(.*?)(\.[^.]+)$/);
        const base = match ? match[1] : name;
        const ext = match ? match[2] : "";
        const room = max - ext.length - 5; // 5 cho "...".
        const head = base.slice(0, Math.ceil(room / 2));
        const tail = base.slice(-Math.floor(room / 2));
        return `${head}...${tail}${ext}`;
    };
const connectionRef = useRef<signalR.HubConnection | null>(null);
    const authSlice2 = useSelector((state: RootState) => state.auth);

    const token = authSlice2.token || "";
    const REST_API_BASE_URL = import.meta.env.VITE_BACKEND_URL;

    useEffect(() => {
        // Build connection
        console.log("Setting up SignalR connection...", token);
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${REST_API_BASE_URL}/api/podcast-service/hubs/podcast-content-notification`, {
                accessTokenFactory: () => {
                    return token;
                }
            })
            .withAutomaticReconnect()
            .build();

        connectionRef.current = connection;

        // Register events
        connection.on("PodcastEpisodeAudioProcessingCompletedNotification", async (data) => {
            console.log("Audio processing :", data);

            if (!data.IsSuccess) {
                console.error("Audio processing failed:", data.ErrorMessage);
                return;
            }

            //alert(`Audio processing completed for Podcast ID: ${data}`);
            await refreshEpisode?.();
        });

        // Start connection
        connection.start()
            .then(() => console.log("SignalR connected"))
            .catch(err => console.error("SignalR connection error:", err));

        // Cleanup
        return () => {
            connection.stop();
        };
    }, []);
    // if (!episodeDetail) {
    //     return (
    //         <div className="flex justify-center items-center h-100">
    //             <Loading />
    //         </div>
    //     );
    // }
    return (
        <div className="episode-audio">

            {/* ============ MAIN CONTENT (LEFT) ============ */}
            <div className="episode-audio__content">
                {episodeDetail && episodeDetail.CurrentStatus?.Id === 8 && (
                    <div
                        className="flex items-center gap-2 bg-[#29b6f626] border border-[#61a7f2ff] rounded-xs px-3 py-2 mb-3"
                        style={{ width: "fit-content" }}
                    >
                        <svg className="w-5 h-5 text-[#61a7f2ff] shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                        </svg>
                        <span className="text-sm text-[#61a7f2ff] font-medium">
                            <strong>Your episode audio is being processed, it will be available soon</strong>
                        </span>
                    </div>
                )}
                {episodeDetail && episodeDetail.CurrentStatus?.Id === 3 && (
                    <div className="flex items-center gap-2 bg-red-100 border border-red-400  rounded px-3 py-2 mb-3 " style={{ width: "fit-content" }}>
                        <svg className="w-5 h-5 text-red-500 shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                        </svg>
                        <span className="text-xs text-red-700 font-medium">
                            <strong>Your episode is being required to edit, please upload new audio</strong>
                        </span>
                    </div>
                )}

                {/* Original Audio Section */}
                {audioUrl && (
                    <div className="episode-audio__player-section">
                        <h3 className="episode-audio__player-title">Original Audio</h3>
                        <div className="episode-audio__waveform-container">
                            <div ref={waveformRefOriginal} className="episode-audio__waveform" />
                        </div>
                        <div className="episode-audio__controls">
                            <div className="episode-audio__controls-left">
                                <IconButton onClick={() => handlePlayPause(false)} className="episode-audio__play-btn">
                                    {isPlayingOriginal ? <Pause /> : <PlayArrow />}
                                </IconButton>
                                <IconButton onClick={() => handleResetAudio(false)} className="episode-audio__play-btn">
                                    <ArrowCounterClockwise size={26} weight="bold" />
                                </IconButton>
                                <span className="episode-audio__time-display">{formatTime(currentTimeOriginal)}</span>
                            </div>

                            <div
                                className="episode-audio__progress-bar"
                                ref={progressBarRefOriginal}
                                onMouseDown={(e) => handleSeekMouseDown(e, false)}
                                onClick={(e) => handleProgressClick(e, false)}
                            >
                                <div
                                    className="episode-audio__progress-fill"
                                    style={{
                                        width: `${(currentTimeOriginal / durationOriginal) * 100}%`,
                                    }}
                                />
                                <div
                                    className="episode-audio__progress-thumb"
                                    style={{
                                        left: `${(currentTimeOriginal / durationOriginal) * 100}%`,
                                    }}
                                />
                            </div>

                            <div className="episode-audio__controls-right">
                                <span className="episode-audio__time-display">{formatTime(durationOriginal)}</span>
                            </div>
                        </div>
                    </div>
                )}

                {/* Audio Sequencer */}
                {showBgSoundSelector && uploadedFile && (
                    <div className="episode-audio__sequencer-container">
                        <div className="episode-audio__sequencer-header">
                            <h3 className="text-lg font-semibold text-white mb-4">Background Sounds Merge</h3>
                            <p className="text-sm text-gray-400 mb-4">
                                Drag and trim background sounds on the timeline. Multiple backgrounds cannot overlap in time.
                            </p>
                        </div>
                        <div className="ml-4 flex items-center gap-2">
                            <label className="text-sm">Zoom</label>
                            <input
                                type="range"
                                min="10"
                                max="50"
                                value={pixelsPerSecond}
                                onChange={(e) => setPPS(Number.parseInt(e.target.value))}
                                className="accent-emerald-600"
                            />
                            <span className="text-xs text-slate-400">{pixelsPerSecond}px/s</span>
                            <button
                                onClick={() => {
                                    console.log('=== SEQUENCER CLIP INFO ===')
                                    const originalClip = clips.find(c => c.track === 0)
                                    const bgClips = clips.filter(c => c.track === 1)

                                    if (originalClip) {
                                        console.log(' ORIGINAL AUDIO:')
                                        console.log(`  File: ${originalClip.name}`)
                                        console.log(`  Duration: ${secondsToTime(originalClip.duration)} (${originalClip.duration.toFixed(2)}s)`)
                                        console.log(`  Timeline Start: ${secondsToTime(originalClip.start)} (${originalClip.start.toFixed(2)}s)`)
                                        console.log(`  Timeline End: ${secondsToTime(originalClip.start + originalClip.duration)} (${(originalClip.start + originalClip.duration).toFixed(2)}s)`)
                                    }

                                    bgClips.forEach((clip, idx) => {
                                        const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
                                        console.log(`\n BACKGROUND ${idx + 1}:`)
                                        console.log(`  File: ${clip.name}`)
                                        console.log(`  Full Duration: ${secondsToTime(clip.duration)} (${clip.duration.toFixed(2)}s)`)
                                        console.log(`  Visible Duration: ${secondsToTime(visibleDur)} (${visibleDur.toFixed(2)}s)`)
                                        console.log(`  Timeline Start: ${secondsToTime(clip.start)} (${clip.start.toFixed(2)}s)`)
                                        console.log(`  Timeline End: ${secondsToTime(clip.start + visibleDur)} (${(clip.start + visibleDur).toFixed(2)}s)`)
                                        console.log(`  Trim Start: ${secondsToTime(clip.trimStart)} (${clip.trimStart.toFixed(2)}s)`)
                                        console.log(`  Trim End: ${secondsToTime(clip.trimEnd)} (${clip.trimEnd.toFixed(2)}s)`)

                                        // Show volume for background clips
                                        if (clip.track === 1 && clip.volume !== undefined) {
                                            console.log(`  Volume: ${clip.volume.toFixed(1)} dB`)
                                        }

                                        if (originalClip) {
                                            console.log(`  Position in Original: ${secondsToTime(clip.start)} → ${secondsToTime(clip.start + visibleDur)}`)
                                        }
                                    })

                                    console.log('\n=== END CLIP INFO ===')
                                }}
                                className="px-3 py-1 rounded bg-slate-700 hover:bg-slate-600 transition-all text-xs flex items-center gap-1"
                                title="Show clip timeline info in console"
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                    <circle cx="12" cy="12" r="10" />
                                    <line x1="12" y1="16" x2="12" y2="12" />
                                    <line x1="12" y1="8" x2="12.01" y2="8" />
                                </svg>
                                Info
                            </button>
                        </div>
                        {/* Sequencer Controls */}
                        <div className="w-full text-slate-100 p-4 flex flex-col gap-3 select-none bg-slate-900 rounded">
                            {/* Toolbar */}

                            {/* Tracks */}
                            <div className="relative border border-slate-800 rounded overflow-hidden">
                                {/* Track headers */}
                                <div className="bg-slate-900 border-b border-slate-800">
                                    {/* Original track */}
                                    {/* <div className="flex items-center gap-3 px-3" style={{ height: rowH, borderBottom: "1px solid #0b1320" }}>
                                        <div className="text-xs w-20 font-semibold">Original</div>
                                        <div className="flex items-center gap-2">
                                            <label className="text-xs">Vol</label>
                                            <input
                                                type="range"
                                                min={0}
                                                max={1}
                                                step={0.01}
                                                value={trackVolumes[0] ?? 1}
                                                onChange={(e) => {
                                                    const v = Number.parseFloat(e.target.value)
                                                    setTrackVolumes([v, trackVolumes[1]])
                                                }}
                                            />
                                        </div>
                                    </div> */}

                                    {/* Background track */}
                                    {/* <div className="flex items-center gap-3 px-3" style={{ height: rowH, borderBottom: "1px solid #0b1320" }}>
                                        <div className="text-xs w-20 font-semibold">Background</div>
                                        <div className="flex items-center gap-2">
                                            <label className="text-xs">Vol</label>
                                            <input
                                                type="range"
                                                min={0}
                                                max={1}
                                                step={0.01}
                                                value={trackVolumes[1] ?? 1}
                                                onChange={(e) => {
                                                    const v = Number.parseFloat(e.target.value)
                                                    setTrackVolumes([trackVolumes[0], v])
                                                }}
                                            />
                                        </div>
                                    </div> */}
                                </div>

                                {/* Timeline body - với container có scroll để giới hạn chiều rộng */}
                                <div
                                    className="max-w-full"
                                    ref={timelineScrollRef}
                                    style={{
                                        overflowX: 'hidden', // NEW: hide timeline scrollbar
                                    }}
                                >
                                    <SequencerTimeline
                                        clips={clips}
                                        setClips={setClips}
                                        pixelsPerSecond={pixelsPerSecond}
                                        rowH={rowH}
                                        totalLengthSec={totalLengthSec}
                                        playhead={playhead}
                                        selectedClipId={selectedClipId}
                                        onSelectClip={(id: string) => setSelectedClipId(id)}
                                    />
                                </div>

                            </div>

                            {/* Ruler - với scroll tương tự */}
                            <div className="overflow-x-auto max-w-full"
                                ref={rulerScrollRef}  // NEW

                                style={{
                                    scrollbarWidth: 'thin',
                                    scrollbarColor: 'rgba(173, 227, 57, 0.71) rgba(23, 23, 23, 0.4)'
                                }}
                            >
                                <SequencerRuler
                                    totalLengthSec={totalLengthSec}
                                    pixelsPerSecond={pixelsPerSecond}
                                    playhead={playhead}
                                />
                            </div>
                        </div>

                        <header className="flex flex-wrap items-center gap-3">
                            <IconButton onClick={isPlayingSequencer ? handleSequencerPause : handleSequencerPlay} className="episode-audio__play-btn">
                                {isPlayingSequencer ? <Pause /> : <PlayArrow />}
                            </IconButton>
                            <IconButton onClick={() => handleSequencerStop()} className="episode-audio__play-btn">
                                <ArrowCounterClockwise size={26} weight="bold" />
                            </IconButton>
                            <span className="episode-audio__time-display">{secondsToTime(playhead)}</span>
                            <div
                                className="episode-audio__progress-bar"
                                ref={progressBarRefSequencer}
                                onMouseDown={(e) => {
                                    if (!progressBarRefSequencer.current) return
                                    const rect = progressBarRefSequencer.current.getBoundingClientRect()
                                    const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1)
                                    const newTime = percent * totalLengthSec
                                    setPlayhead(newTime)
                                }}
                                onClick={(e) => {
                                    if (!progressBarRefSequencer.current) return
                                    const rect = progressBarRefSequencer.current.getBoundingClientRect()
                                    const percent = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1)
                                    const newTime = percent * totalLengthSec
                                    setPlayhead(newTime)
                                }}
                            >
                                <div
                                    className="episode-audio__progress-fill"
                                    style={{
                                        width: `${(playhead / totalLengthSec) * 100}%`,
                                    }}
                                />
                                <div
                                    className="episode-audio__progress-thumb"
                                    style={{
                                        left: `${(playhead / totalLengthSec) * 100}%`,
                                    }}
                                />
                            </div>

                            <div className="episode-audio__controls-right">
                                <span className="episode-audio__time-display">{secondsToTime(totalLengthSec)}</span>
                            </div>
                        </header>


                        {selectedClipId && clips.find(c => c.id === selectedClipId && c.track === 1) && (() => {
                            const selectedClip = clips.find(c => c.id === selectedClipId)!;
                            const visibleDur = Math.max(0, selectedClip.duration - selectedClip.trimStart - selectedClip.trimEnd);
                            const isPlaying = segmentPlayingClipId === selectedClip.id; // cần tách state này ra ngoài ClipRnd

                            return (
                                <div className="mt-4 p-3 rounded border border-slate-700 bg-slate-800">
                                    <div className="flex items-center justify-between mb-3">
                                        <div className="flex items-center gap-2">
                                            <span className="text-sm font-semibold text-white">Selected:</span>
                                            <span className="text-sm text-[#AEE339]" title={selectedClip.name}>{selectedClip.name}</span>
                                        </div>
                                        <div className="flex items-center gap-2">
                                            {/* Play/Stop */}
                                            <IconButton
                                                onClick={() => {
                                                    const from = selectedClip.trimStart;
                                                    const dur = visibleDur;
                                                    if (isPlaying) {
                                                        // stop
                                                        if (currentSegment.src && currentSegment.clipId === selectedClip.id) {
                                                            try { currentSegment.src.stop() } catch { }
                                                            currentSegment = { src: null, clipId: null };
                                                        }
                                                        setSegmentPlayingClipId(null);
                                                    } else {
                                                        // play
                                                        const ac = getAC();
                                                        if (currentSegment.src) {
                                                            try { currentSegment.src.stop() } catch { }
                                                            currentSegment = { src: null, clipId: null };
                                                            setSegmentPlayingClipId(null);
                                                        }
                                                        try {
                                                            const source = ac.createBufferSource();
                                                            source.buffer = selectedClip.buffer;
                                                            const gain = ac.createGain();
                                                            const linearBase = Math.pow(10, (selectedClip.volume ?? -5) / 20);
                                                            gain.gain.value = 0;
                                                            source.connect(gain).connect(ac.destination);
                                                            source.start(0, from, dur);

                                                            // fade
                                                            const fi = 0.5, fo = 0.5;
                                                            const now = ac.currentTime;
                                                            const segStart = now;
                                                            const segEnd = now + dur;
                                                            if (fi > 0) {
                                                                gain.gain.setValueAtTime(0, segStart);
                                                                gain.gain.linearRampToValueAtTime(linearBase, segStart + Math.min(fi, dur));
                                                            } else {
                                                                gain.gain.setValueAtTime(linearBase, segStart);
                                                            }
                                                            if (fo > 0) {
                                                                const fadeOutStart = Math.max(segStart, segEnd - fo);
                                                                gain.gain.setValueAtTime(linearBase, fadeOutStart);
                                                                gain.gain.linearRampToValueAtTime(0.0001, segEnd);
                                                            }

                                                            currentSegment = { src: source, clipId: selectedClip.id };
                                                            setSegmentPlayingClipId(selectedClip.id);
                                                            source.onended = () => {
                                                                if (currentSegment.src === source) {
                                                                    currentSegment = { src: null, clipId: null };
                                                                }
                                                                setSegmentPlayingClipId(null);
                                                            };
                                                        } catch (e) {
                                                            toast.info(`Playing segment ${from.toFixed(2)}s → ${(from + dur).toFixed(2)}s`);
                                                        }
                                                    }
                                                }}
                                                size="small"
                                                sx={{ color: '#AEE339', '&:hover': { color: '#7BA225' } }}
                                                title={isPlaying ? "Stop segment" : "Play segment"}
                                            >
                                                {isPlaying ? <Pause /> : <PlayArrow />}
                                            </IconButton>

                                            {/* Delete */}
                                            <IconButton
                                                onClick={() => {
                                                    setClips(prev => prev.filter(c => c.id !== selectedClip.id));
                                                    setSelectedClipId(null);
                                                }}
                                                size="small"
                                                sx={{ color: '#888', '&:hover': { color: '#f44336' } }}
                                                title="Delete clip"
                                            >
                                                <Delete />
                                            </IconButton>
                                        </div>
                                    </div>

                                    {/* Volume slider */}
                                    <div>
                                        <div className="flex justify-between items-center mb-1">
                                            <span className="text-xs text-slate-300">Volume</span>
                                            <span className="text-xs text-slate-400">{(selectedClip.volume ?? -5).toFixed(1)} dB</span>
                                        </div>
                                        <input
                                            type="range"
                                            min={-20}
                                            max={10}
                                            step={0.5}
                                            value={selectedClip.volume ?? -5}
                                            onChange={(e) => {
                                                const newVolume = Number.parseFloat(e.target.value);
                                                setClips(prev => prev.map(c =>
                                                    c.id === selectedClip.id ? { ...c, volume: newVolume } : c
                                                ));
                                            }}
                                            className="episode-audio__volume-slider is-active"
                                        />
                                        <div className="flex justify-between text-xs mt-1" style={{ color: 'rgba(255, 255, 255, 0.5)' }}>
                                            <span>-20</span><span>0</span><span>+10</span>
                                        </div>
                                    </div>
                                </div>
                            );
                        })()}
                    </div>
                )}
                {/* Preview Audio Section */}
                {previewUrl && (
                    <div className="episode-audio__player-section">
                        <h3 className="episode-audio__player-title">Preview Audio</h3>
                        <div className="episode-audio__waveform-container">
                            <div ref={waveformRefPreview} className="episode-audio__waveform" />
                        </div>
                        <div className="episode-audio__controls">
                            <div className="episode-audio__controls-left">
                                <IconButton onClick={() => handlePlayPause(true)} className="episode-audio__play-btn">
                                    {isPlayingPreview ? <Pause /> : <PlayArrow />}
                                </IconButton>
                                <IconButton onClick={() => handleResetAudio(true)} className="episode-audio__play-btn">
                                    <ArrowCounterClockwise size={26} weight="bold" />
                                </IconButton>
                                <span className="episode-audio__time-display">{formatTime(currentTimePreview)}</span>
                            </div>

                            <div
                                className="episode-audio__progress-bar"
                                ref={progressBarRefPreview}
                                onMouseDown={(e) => handleSeekMouseDown(e, true)}
                                onClick={(e) => handleProgressClick(e, true)}
                            >
                                <div
                                    className="episode-audio__progress-fill"
                                    style={{
                                        width: `${(currentTimePreview / durationPreview) * 100}%`,
                                    }}
                                />
                                <div
                                    className="episode-audio__progress-thumb"
                                    style={{
                                        left: `${(currentTimePreview / durationPreview) * 100}%`,
                                    }}
                                />
                            </div>

                            <div className="episode-audio__controls-right">
                                <span className="episode-audio__time-display">{formatTime(durationPreview)}</span>
                            </div>
                        </div>
                    </div>
                )}
                {/* EQ Table Section */}
                {audioUrl && (
                    <div className="episode-audio__eq-table">
                        {Object.keys(eqConfig).map((band) => (
                            <div key={band} className="episode-audio__eq-band">
                                <div className="episode-audio__eq-slider-container">
                                    <div className="eq-slider-marks eq-slider-marks--left">
                                        <span /><span /><span /><span /><span />
                                    </div>
                                    {/* Gạch phải */}
                                    <div className="eq-slider-marks eq-slider-marks--right">
                                        <span /><span /><span /><span /><span />
                                    </div>
                                    <input
                                        className="episode-audio__eq-slider"
                                        type="range"
                                        min="-10"
                                        max="10"
                                        step="1"
                                        value={eqConfig[band]}
                                        onChange={(e) => setEqConfig({ ...eqConfig, [band]: Number.parseInt(e.target.value) })}
                                    />
                                </div>
                                <div className="episode-audio__eq-band-label">{band}</div>
                                <div className="episode-audio__eq-band-value">{eqConfig[band]} dB</div>
                            </div>
                        ))}
                    </div>
                )}

                {!audioUrl && (
                    <div className="episode-audio__empty-state">
                        <div className="episode-audio__empty-state__icon">
                            <Music size={48} />
                        </div>
                        <p className="episode-audio__empty-state__text">Upload an audio file to get started</p>
                    </div>
                )}
            </div>

            {/* ============ SIDEBAR (RIGHT) ============ */}
            <div className="episode-audio__sidebar">
                <div className="episode-audio__upload-box">
                    <h3 className="episode-audio__section-title">Audio Upload</h3>
                    {episodeDetail && episodeDetail.CurrentStatus?.Id !== 8 && (
                        <div
                            className={`episode-audio__upload-area ${isDragging ? "episode-audio__upload-area--dragging" : ""}`}
                            onDragOver={handleDragOver}
                            onDragLeave={handleDragLeave}
                            onDrop={handleDrop}
                            onClick={() => fileInputRef.current?.click()}
                        >
                            <Music className="episode-audio__upload-icon" size={40} />
                            <p className="episode-audio__upload-text">Drop your audio file here</p>
                            <p className="episode-audio__upload-subtext">or click to browse</p>
                            <input
                                ref={fileInputRef}
                                type="file"
                                accept=".wav,.flac,.mp3,.m4a,.aac"
                                onChange={handleFileSelect}
                                className="episode-audio__upload-input"
                            />
                        </div>
                    )}


                    {uploadedFile && (
                        <div className="episode-audio__file-info">
                            <div className="episode-audio__file-info__row">
                                <FolderSimple size={20} color="#B6E04A" />
                                <span
                                    className="episode-audio__file-info__label"
                                    title={uploadedFile.name}
                                >
                                    {shortenFileName(uploadedFile.name, 30)}
                                </span>
                                <IconButton
                                    size="small"
                                    onClick={() => {
                                        navigator.clipboard.writeText(uploadedFile.name);
                                        toast.success("Copied filename");
                                    }}
                                    className="episode-audio__file-info__copy"
                                >
                                    <ContentCopy fontSize="inherit" />
                                </IconButton>
                            </div>
                            <div className="episode-audio__file-info__row">
                                <Database size={20} color="#B6E04A" />
                                <span className="episode-audio__file-info__value">
                                    {(uploadedFile.size / 1024 / 1024).toFixed(2)} MB
                                </span>
                            </div>
                        </div>
                    )}
                </div>

                {/* Audio Style */}
                <div style={{ borderBottom: "2px solid var(--border-grey)" }}>
                    <h3 className="episode-audio__section-title mb-3">Audio Style</h3>

                    <div className="episode-audio__selector-group">
                        <div className="flex justify-between items-center">
                            <label className="episode-audio__selector-label">EQ Preset</label>
                            <Tooltip placement="top-start" title="Quickly adjust the EQ bands to shape your sound">
                                <Question color="var(--third-grey)" size={16} />
                            </Tooltip >
                        </div>
                        <Select
                            value={selectedPreset}
                            onChange={(e) => handlePresetChange(e.target.value as string)}
                            displayEmpty
                            variant="outlined"
                            className="episode-audio__selector"
                            disabled={!audioUrl}
                            sx={{
                                '& .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--border-grey)' },
                                '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--primary-green)' },
                                '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--primary-green)' },
                                '& .MuiSelect-icon': { color: 'var(--primary-green)' },
                            }}
                        >
                            {Object.keys(presets).map((preset) => (
                                <MenuItem key={preset} value={preset}>
                                    {preset === "MysticVoice" ? " Mystic Voice" : preset === "DeepMystery" ? " Deep Mystery" : preset}
                                </MenuItem>
                            ))}
                        </Select>
                    </div>

                    <div className={`episode-audio__selector-group ${showMoodSelector ? 'pb-8' : 'pb-2'}`}>
                        <div className="flex justify-between items-center">
                            <div className="flex items-center gap-2">
                                <label className="episode-audio__selector-label">Mood</label>
                                <Tooltip placement="top-start" title="Combine EQ and audio filters to create a unique atmosphere">
                                    <Question color="var(--third-grey)" size={16} />
                                </Tooltip >
                            </div>
                            <IconButton
                                className="episode-audio__add-btn"
                                onClick={handleAddMood}
                                title={showMoodSelector ? "Close" : "Add mood"}
                                aria-label={showMoodSelector ? "Close mood" : "Add mood"}
                                disabled={!audioUrl}
                            >
                                {showMoodSelector ? <Minus color="white" size={20} /> : <Plus size={20} />}
                            </IconButton>
                        </div>
                        {showMoodSelector && (
                            <>
                                <Select
                                    value={selectedMood}
                                    onChange={(e) => handleMoodChange(e.target.value as string)}
                                    variant="outlined"
                                    displayEmpty
                                    className="episode-audio__selector"
                                    disabled={!audioUrl}
                                    sx={{
                                        '& .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--border-grey)' },
                                        '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--primary-green)' },
                                        '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: 'var(--primary-green)' },
                                        '& .MuiSelect-icon': { color: 'var(--primary-green)' },
                                    }}
                                >
                                    {MOOD_OPTIONS.map(m => (
                                        <MenuItem key={m.value} value={m.value}>{m.label}</MenuItem>
                                    ))}
                                </Select>
                            </>
                        )}
                    </div>
                </div>

                {/* Background Sound */}
                <div className="episode-audio__background-sound ">
                    <div className="episode-audio__background-sound-header">
                        <div className="flex items-center gap-2">
                            <h3 className={`episode-audio__section-title ${showBgSoundSelector ? '' : 'episode-audio__section-title--muted'}`}>
                                Background Sound
                            </h3>
                            <Tooltip placement="top-start" title="Add a background sound that plays throughout your entire audio">
                                <Question color="var(--third-grey)" size={16} />
                            </Tooltip >
                        </div>
                        <IconButton
                            className="episode-audio__add-btn"
                            onClick={() => setShowBgSoundSelector(!showBgSoundSelector)}
                            title={showBgSoundSelector ? "Close" : "Add background sound"}
                            aria-label={showBgSoundSelector ? "Close background sound" : "Add background sound"}
                            disabled={!audioUrl}
                        >
                            {showBgSoundSelector ? <Minus color="white" size={20} /> : <Plus size={20} />}
                        </IconButton>
                    </div>

                    {showBgSoundSelector && (
                        <div className="mt-3">
                            <div
                                className="grid grid-cols-1 gap-2 mb-3"
                                style={{
                                    maxHeight: '400px',
                                    overflowY: 'auto',
                                    paddingRight: '4px',
                                    scrollbarWidth: 'thin',
                                    scrollbarColor: 'rgba(173, 227, 57, 0.71) rgba(23, 23, 23, 0.4)'
                                }}
                            >
                                {backgroundSounds.map((bg) => (
                                    <button
                                        key={bg.Id}
                                        className="px-3 py-2 text-xs rounded transition-all text-white border"
                                        style={{
                                            background: 'rgba(23, 23, 23, 0.6)',
                                            borderColor: 'rgba(174, 227, 57, 0.2)',
                                        }}
                                        onMouseEnter={(e) => {
                                            e.currentTarget.style.background = 'rgba(174, 227, 57, 0.15)'
                                            e.currentTarget.style.borderColor = 'rgba(174, 227, 57, 0.4)'
                                        }}
                                        onMouseLeave={(e) => {
                                            e.currentTarget.style.background = 'rgba(23, 23, 23, 0.6)'
                                            e.currentTarget.style.borderColor = 'rgba(174, 227, 57, 0.2)'
                                        }}
                                        onClick={() => handleAddBackgroundToSequencer(bg)}
                                        title={`Add ${bg.Name}`}
                                    >
                                        <div className="flex flex-col mt-4 gap-4">
                                            <div className="flex gap-4">
                                                <Image
                                                    mainImageFileKey={`${bg?.MainImageFileKey || ''}`}
                                                    alt={'Background Sound Image'}
                                                    className="w-12 h-12 object-cover rounded-sm"
                                                />
                                                <div className="flex flex-col items-start">
                                                    <p className="text-[#aee339] font-medium text-sm">{bg?.Name}</p>
                                                    <p className="text-[#d9d9d9] text-left font-light text-xs">{bg?.Description}</p>
                                                </div>
                                            </div>

                                            <SmartAudioPlayer
                                                audioId={bg.AudioFileKey}
                                                fetchUrlFunction={fetchTrackAudioUrl}
                                                className="flex-1"
                                            />
                                        </div>
                                    </button>
                                ))}
                            </div>

                            {clips.filter(c => c.track === 1).length > 0 && (
                                <div className="mt-4 pt-3 border-t" style={{ borderColor: 'rgba(174, 227, 57, 0.15)' }}>
                                    <h4 className="text-sm font-medium mb-3 text-white flex items-center gap-2">
                                        <span style={{ color: '#999' }}>Background Volumes</span>
                                        <Tooltip placement="top" title="Adjust volume for each background before merging">
                                            <Question color="var(--third-grey)" size={14} />
                                        </Tooltip>
                                    </h4>
                                    {clips.filter(c => c.track === 1).map((clip) => {
                                        const isActive = selectedClipId === clip.id;
                                        return (
                                            <div
                                                key={clip.id}
                                                className="mb-3 p-2 rounded"
                                                style={{
                                                    background: 'rgba(23, 23, 23, 0.4)',

                                                }}
                                                onClick={() => setSelectedClipId(clip.id)}
                                            >
                                                <div className="flex justify-between items-center mb-1">
                                                    <span className="text-xs text-white truncate max-w-[120px]" title={clip.name}
                                                        style={{
                                                            color: isActive ? ' #AEE339' : 'white',  // NEW: highlight waveform
                                                            fontWeight: isActive ? '600' : '400',
                                                        }}
                                                    >
                                                        {clip.name}
                                                    </span>
                                                    <span className="text-xs" style={{ color: isActive ? ' #AEE339' : '#888' }}>
                                                        {(clip.volume ?? -5).toFixed(1)} dB
                                                    </span>
                                                </div>
                                                <input
                                                    type="range"
                                                    min={-20}
                                                    max={10}
                                                    step={0.5}
                                                    value={clip.volume ?? 0}
                                                    onChange={(e) => {
                                                        const newVolume = Number.parseFloat(e.target.value)
                                                        setClips(prev => prev.map(c =>
                                                            c.id === clip.id ? { ...c, volume: newVolume } : c
                                                        ))
                                                    }}
                                                    onFocus={() => setSelectedClipId(clip.id)} // focus slider -> chọn clip
                                                    className={`episode-audio__volume-slider ${isActive ? 'is-active' : ''}`}
                                                />
                                                <div className="flex justify-between text-xs mt-1" style={{ color: 'rgba(255, 255, 255, 0.5)' }}>
                                                    <span>-20</span><span>0</span><span>+10</span>
                                                </div>
                                            </div>
                                        )
                                    })}
                                </div>
                            )}
                        </div>
                    )}
                </div>


                {previewLoading || saving ? (
                    <div className="flex justify-center items-center m-8 ">
                        <Loading2 title="Audio Processing" />
                    </div>
                ) : (
                    <>
                        {episodeDetail && (episodeDetail.CurrentStatus.Id !== 8 && episodeDetail.CurrentStatus.Id !== 7 && episodeDetail.CurrentStatus.Id !== 6) && (
                            <div className="episode-audio__actions">
                                <button className="episode-audio__btn episode-audio__btn--primary"
                                    onClick={handleSaveClick}
                                    disabled={saveDisabled}>
                                    <CloudUpload size={18} />
                                    Save
                                </button>
                                <button
                                    className="episode-audio__btn episode-audio__btn--secondary"
                                    onClick={handlePreview}
                                    disabled={previewDisabled}
                                >
                                    <Play size={18} />
                                    Preview
                                </button>

                            </div>
                        )}
                    </>

                )}
                <Modal open={saveChoiceOpen} onClose={() => setSaveChoiceOpen(false)}>
                    <div style={{
                        background: '#171717e6',
                        padding: '24px',
                        borderRadius: '12px',
                        width: '360px',
                        display: 'flex',
                        flexDirection: 'column',
                        gap: '16px',
                        position: 'absolute',
                        top: '50%',
                        left: '50%',
                        transform: 'translate(-50%, -50%)',
                        boxShadow: '0 8px 24px rgba(0,0,0,0.4)'
                    }}>
                        <h4 style={{ margin: 0, color: 'var(--primary-green)' }}>Choose what you want to save</h4>
                        <p style={{ margin: 0, fontSize: '0.85rem', color: 'var(--third-grey)' }}>
                            You can save the original or the processed version.
                        </p>
                        {currentFileSource === 'local' ? (
                            <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                                <button
                                    className="episode-audio__btn episode-audio__btn--secondary"
                                    onClick={() => { if (uploadedFile) performSave(uploadedFile); setSaveChoiceOpen(false); }}
                                    disabled={!uploadedFile || saving}
                                >
                                    Save Original
                                </button>
                                <button
                                    className="episode-audio__btn episode-audio__btn--primary"
                                    onClick={() => { if (previewFile) performSave(previewFile); setSaveChoiceOpen(false); }}
                                    disabled={!previewFile || saving || hasUnsavedPreview}
                                >
                                    Save Preview
                                </button>
                            </div>
                        ) : (
                            // Server source sẽ không hiển thị lựa chọn (chỉ cho save bản preview) – modal không nên mở nhưng fallback
                            <div>
                                <button
                                    className="episode-audio__btn episode-audio__btn--primary"
                                    onClick={() => { if (previewFile) performSave(previewFile); setSaveChoiceOpen(false); }}
                                    disabled={!previewFile || saving || hasUnsavedPreview}
                                >
                                    Save Preview
                                </button>
                            </div>
                        )}
                        <button
                            style={{
                                background: 'transparent',
                                color: 'var(--third-grey)',
                                border: 'none',
                                cursor: 'pointer',
                                fontSize: '0.75rem',
                                alignSelf: 'center'
                            }}
                            onClick={() => setSaveChoiceOpen(false)}
                        >
                            Cancel
                        </button>
                    </div>
                </Modal>
            </div >
        </div >
    )
}

export default EpisodeAudio
