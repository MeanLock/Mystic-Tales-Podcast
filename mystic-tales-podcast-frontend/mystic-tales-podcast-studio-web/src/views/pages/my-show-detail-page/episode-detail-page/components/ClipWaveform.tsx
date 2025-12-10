import { secondsToTime } from "@/core/utils/audio.util"
import { Delete, PlayArrow } from "@mui/icons-material"
import { IconButton } from "@mui/material"
import { Pause } from "lucide-react"
import { useEffect, useRef } from "react"
import WaveSurfer from "wavesurfer.js"

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
interface ClipWaveformProps {
    clip: Clip
    width: number
    height: number
    dragHandle: string
    isOriginal: boolean
    onDelete: () => void
    isSelected?: boolean
    onSelect?: () => void
    onNudgeViewport?: (nextTrimStart: number) => void
    onPlaySegment?: (fromSec: number, durationSec: number) => void

    isSegmentPlaying?: boolean
    onStopSegment?: () => void
}

export function ClipWaveform({ clip, width, height, dragHandle, isOriginal, onDelete, isSelected, onSelect, onNudgeViewport, onPlaySegment, isSegmentPlaying, onStopSegment }: ClipWaveformProps) {
    const containerRef = useRef<HTMLDivElement>(null)
    const wsRef = useRef<WaveSurfer | null>(null)

    useEffect(() => {
        if (!containerRef.current) return
        const ws = WaveSurfer.create({
            container: containerRef.current,
            height: Math.max(15, height - 50),
            waveColor: "#7BA225",
            progressColor: "#AEE339",
            cursorWidth: 0,
            interact: false,
            hideScrollbar: true,
            minPxPerSec: 10,
        })
        wsRef.current = ws
        ws.loadBlob(clip.file).catch(() => { })

        return () => {
            try {
                ws.destroy()
            } catch { }
        }
    }, [clip.file, height])

    // Tính toán vùng thời gian hiển thị (chỉ ghi nhớ start, end)
    const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
    const leftTrimPercent = (clip.trimStart / clip.duration) * 100

    const isDraggingRef = useRef(false)
    const lastXRef = useRef(0)

    const onInnerMouseDown = (e: React.MouseEvent) => {
        if (isOriginal) return
        isDraggingRef.current = true
        lastXRef.current = e.clientX
        // Ngăn drag container RND khi đang kéo nội bộ waveform
        e.stopPropagation()
    }
    const onInnerMouseMove = (e: React.MouseEvent) => {
        if (!isDraggingRef.current || isOriginal) return
        const deltaX = e.clientX - lastXRef.current
        lastXRef.current = e.clientX

        // Tỷ lệ px -> giây: containerRef.width biểu diễn toàn bộ duration
        const containerWidth = containerRef.current?.clientWidth || width || 200
        const secPerPx = clip.duration / Math.max(containerWidth, 1)

        const deltaSec = deltaX * secPerPx
        const maxTrimStart = Math.max(0, clip.duration - visibleDur)
        const nextTrimStart = Math.max(0, Math.min(maxTrimStart, clip.trimStart - deltaSec)) // trừ deltaSec vì kéo phải → tăng offset

        onNudgeViewport?.(nextTrimStart)

        e.stopPropagation()
    }
    const onInnerMouseUp = () => { isDraggingRef.current = false }
    const onInnerMouseLeave = () => { isDraggingRef.current = false }


    const handlePlaySegment = (e: React.MouseEvent) => {
        e.stopPropagation()
        const from = clip.trimStart
        const dur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
        onPlaySegment?.(from, dur)
    }

    const handleStopSegment = (e: React.MouseEvent) => {
        e.stopPropagation()
        onStopSegment?.()
    }
    const handlePlayPauseSegment = (e: React.MouseEvent) => {
        e.stopPropagation()
        if (isSegmentPlaying) {
            handleStopSegment(e)
        } else {
            handlePlaySegment(e)
        }
    }

    return (
        <div className="w-full h-full flex flex-col"
            onMouseDown={onSelect}
        >
            <div
                className={`px-2 py-1 text-xs text-slate-200 flex items-center justify-between border-b border-slate-700 ${dragHandle}`}
            >
                <div className="truncate" title={clip.name}
                    style={{
                        color: isSelected ? '#AEE339' : 'white',
                        fontWeight: isSelected ? 'bold' : 'normal'
                    }}
                >
                    {clip.name}
                </div>

                <div className="truncate flex items-center gap-2">
                    {!isOriginal && (
                        <>
                            <IconButton
                                onClick={handlePlayPauseSegment}
                                size="small"
                                sx={{ color: '#888', '&:hover': { color: '#aee339' } }}
                                title={isSegmentPlaying ? "Pause this segment" : "Play this segment"}
                            >
                                {isSegmentPlaying ? <Pause /> : <PlayArrow />}
                            </IconButton>
                            <IconButton
                                onClick={(e) => {
                                    e.stopPropagation()
                                    onDelete()
                                }}
                                size="small"
                                sx={{
                                    color: '#888',
                                    '&:hover':
                                    {
                                        color: '#f44336',

                                    }
                                }}
                            >
                                <Delete />
                            </IconButton>
                        </>
                    )}
                    <div className="text-slate-400">{secondsToTime(visibleDur)}</div>

                </div>
            </div>

            <div className="flex-1 relative overflow-hidden">
                <div
                    ref={containerRef}
                    className="absolute top-0 bottom-0 left-0"
                    style={{
                        width: `${(clip.duration / visibleDur) * 100}%`,
                        transform: `translateX(-${leftTrimPercent}%)`,
                        cursor: isOriginal ? 'default' : 'grab'
                    }}
                    onMouseDown={onInnerMouseDown}
                    onMouseMove={onInnerMouseMove}
                    onMouseUp={onInnerMouseUp}
                    onMouseLeave={onInnerMouseLeave}
                />
            </div>
        </div>
    )
}