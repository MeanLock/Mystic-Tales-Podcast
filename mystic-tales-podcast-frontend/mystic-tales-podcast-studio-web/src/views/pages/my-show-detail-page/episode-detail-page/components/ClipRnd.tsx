import { Rnd } from "react-rnd"
import { toast } from "react-toastify"
import { ClipWaveform } from "./ClipWaveform"
import { useState } from "react"

interface Clip {
    id: string
    name: string
    file: File
    buffer: AudioBuffer
    duration: number
    start: number
    trimStart: number
    trimEnd: number
    track: number
    volume?: number

}
interface ClipRndProps {
    clip: Clip
    pps: number
    rowH: number
    isOriginal: boolean
    onChange: (update: Partial<Clip> & { __delete?: boolean }) => void
    allClips: Clip[]
    selectedClipId?: string | null
    onSelectClip?: (id: string) => void
    setClips: React.Dispatch<React.SetStateAction<Clip[]>>

}
const clamp = (v: number, min: number, max: number) => Math.max(min, Math.min(max, v))
let _ac: AudioContext
const getAC = () => {
    if (!_ac) _ac = new (window.AudioContext || (window as any).webkitAudioContext)()
    return _ac
}
let currentSegment: { src: AudioBufferSourceNode | null; clipId: string | null } = { src: null, clipId: null }

export function ClipRnd({ clip, pps, rowH, isOriginal, onChange, allClips, selectedClipId, onSelectClip, setClips }: ClipRndProps) {
    const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
    const width = Math.max(8, visibleDur * pps)
    const x = clip.start * pps
    const y = clip.track === 0 ? 0 : rowH + 10 // Thêm khoảng cách 10px giữa tracks
    const height = rowH
    const dragHandle = "clip-drag-handle"
    const snapGridX = Math.max(1, Math.round(pps / 20))

    const MIN_VISIBLE_SEC = 5
    const minVisibleSec = Math.min(MIN_VISIBLE_SEC, clip.duration)
    const minWidthPx = Math.max(8, minVisibleSec * pps)

    const [segmentPlayingClipId, setSegmentPlayingClipId] = useState<string | null>(null)
    const isSegmentPlaying = segmentPlayingClipId === clip.id

    const playSegment = (fromSec: number, durationSec: number) => {
        const ac = getAC()

        // Stop bất kỳ segment đang phát trước khi play clip mới
        if (currentSegment.src) {
            try { currentSegment.src.stop() } catch { }
            currentSegment = { src: null, clipId: null }
            setSegmentPlayingClipId(null) // ensure UI update
        }

        if (isOriginal) return

        try {
            const source = ac.createBufferSource()
            source.buffer = clip.buffer
            const gain = ac.createGain()
            const linearBase = Math.pow(10, (clip.volume ?? -5) / 20)
            gain.gain.value = 0
            source.connect(gain).connect(ac.destination)
            source.start(0, fromSec, durationSec)

            // áp dụng fade cho preview segment
            const fi = Math.max(0, 0.5)
            const fo = Math.max(0, 0.5)
            const now = ac.currentTime
            const segStart = now
            const segEnd = now + durationSec

            if (fi > 0) {
                gain.gain.setValueAtTime(0, segStart)
                gain.gain.linearRampToValueAtTime(linearBase, segStart + Math.min(fi, durationSec))
            } else {
                gain.gain.setValueAtTime(linearBase, segStart)
            }
            if (fo > 0) {
                const fadeOutStart = Math.max(segStart, segEnd - fo)
                gain.gain.setValueAtTime(linearBase, fadeOutStart)
                gain.gain.linearRampToValueAtTime(0.0001, segEnd)
            }

            currentSegment = { src: source, clipId: clip.id }
            setSegmentPlayingClipId(clip.id) // UI: đang phát clip này

            source.onended = () => {
                if (currentSegment.src === source) {
                    currentSegment = { src: null, clipId: null }
                }
                setSegmentPlayingClipId(null) // UI: dừng
            }
        } catch (e) {
            console.warn('Segment play failed', e)
            toast.info(`Playing segment ${fromSec.toFixed(2)}s → ${(fromSec + durationSec).toFixed(2)}s`)
        }
    }

    const stopSegment = () => {
        if (currentSegment.src) {
            try { currentSegment.src.stop() } catch { }
            currentSegment = { src: null, clipId: null }
        }
        setSegmentPlayingClipId(null)
    }
    return (
        <Rnd
            size={{ width, height }}
            position={{ x, y }}
            bounds="parent"
            dragAxis="x"
            dragGrid={[snapGridX, 0]}
            minWidth={minWidthPx}
            enableResizing={
                isOriginal
                    ? false
                    : {
                        left: true,
                        right: true,
                        top: false,
                        bottom: false,
                    }
            }
            dragHandleClassName={dragHandle}
            onDragStart={() => { if (!isOriginal) stopSegment() }}
            onDragStop={(e, d) => {
                const newStart = Math.max(0, d.x / pps);

                // Ngăn chồng lấn background track
                if (clip.track === 1) {
                    const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd);
                    const newEnd = newStart + visibleDur;
                    const hasConflict = allClips.some((c) => {
                        if (c.id === clip.id || c.track !== 1) return false;
                        const otherDur = Math.max(0, c.duration - c.trimStart - c.trimEnd);
                        return !(newEnd <= c.start || newStart >= c.start + otherDur);
                    });
                    if (hasConflict) {
                        toast.error("Cannot move clip: it would overlap another background sound");
                        return;
                    }
                }

                onChange({ start: newStart });
            }}
            onResizeStart={() => { if (!isOriginal) stopSegment() }}
            onResizeStop={(e, dir, ref, delta, pos) => {
                if (isOriginal) return;
                const newWpx = ref.offsetWidth;
                let newVisible = Math.max(minVisibleSec, newWpx / pps);

                if (dir === "left") {
                    const deltaSeconds = -delta.width / pps;
                    const newStart = Math.max(0, pos.x / pps);
                    let nextTrimStart = clip.trimStart + deltaSeconds;

                    const maxTrimStart = clip.duration - clip.trimEnd - minVisibleSec;
                    nextTrimStart = clamp(nextTrimStart, 0, maxTrimStart);

                    onChange({ start: newStart, trimStart: +nextTrimStart });
                }

                if (dir === "right") {
                    const nextTrimEnd = clamp(
                        clip.duration - clip.trimStart - newVisible,
                        0,
                        clip.duration - clip.trimStart - minVisibleSec
                    );
                    onChange({ trimEnd: +nextTrimEnd });
                }
            }}
            className="rounded-xl shadow-lg border border-slate-700 bg-slate-800"
        >
            <ClipWaveform
                clip={clip}
                height={height}
                width={width}
                dragHandle={dragHandle}
                isOriginal={isOriginal}
                onDelete={() => onChange({ __delete: true })}
                isSelected={selectedClipId === clip.id}
                onSelect={() => onSelectClip?.(clip.id)}
                onNudgeViewport={(nextTrimStart) => {
                    if (!isOriginal) stopSegment()
                    const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
                    const nextTrimEnd = Math.max(0, clip.duration - visibleDur - nextTrimStart)
                    setClips(prev => prev.map(c =>
                        c.id === clip.id ? { ...c, trimStart: nextTrimStart, trimEnd: nextTrimEnd } : c
                    ))
                }}
                isSegmentPlaying={isSegmentPlaying}
                onPlaySegment={(fromSec, durationSec) => playSegment(fromSec, durationSec)}
                onStopSegment={() => stopSegment()}
            />
        </Rnd>

    )
}