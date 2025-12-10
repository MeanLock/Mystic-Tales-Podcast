import { ClipRnd } from "./ClipRnd"

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

interface SequencerTimelineProps {
    clips: Clip[]
    setClips: React.Dispatch<React.SetStateAction<Clip[]>>
    pixelsPerSecond: number
    rowH: number
    totalLengthSec: number
    playhead: number
    selectedClipId?: string | null
    onSelectClip?: (id: string) => void
}

export default function SequencerTimeline({ clips, setClips, pixelsPerSecond, rowH, totalLengthSec, playhead, selectedClipId, onSelectClip }: SequencerTimelineProps) {
    const totalWidth = Math.max(Math.ceil(totalLengthSec * pixelsPerSecond), 100) // Minimum 1200px cho full width

    return (
        <div className="relative bg-slate-900" style={{ width: totalWidth, height: rowH * 2 + 10 }}>
            <div style={{ width: totalWidth, height: rowH * 2 + 10 }} className="relative">
                {/* Track backgrounds */}
                <div className="absolute left-0" style={{ top: 0, height: rowH, width: totalWidth, background: "#0b1220" }} />
                <div className="absolute left-0" style={{ top: rowH + 10, height: rowH, width: totalWidth, background: "#0f172a" }} />

                {/* Clips */}
                {clips.map((clip) => (
                    <ClipRnd
                        key={clip.id}
                        clip={clip}
                        pps={pixelsPerSecond}
                        rowH={rowH}
                        isOriginal={clip.track === 0}
                        allClips={clips}
                        onChange={(next) => {
                            if (next && next.__delete) {
                                setClips((prev) => prev.filter((c) => c.id !== clip.id))
                            } else {
                                setClips((prev) => prev.map((c) => (c.id === clip.id ? { ...c, ...next } : c)))
                            }
                        }}
                        setClips={setClips}
                        selectedClipId={selectedClipId}
                        onSelectClip={() => onSelectClip?.(clip.id)}
                    />
                ))}

                {/* Playhead */}
                <div
                    className="pointer-events-none absolute top-0 bottom-0 w-px bg-rose-400"
                    style={{ left: playhead * pixelsPerSecond }}
                />
            </div>
        </div>
    )
}
