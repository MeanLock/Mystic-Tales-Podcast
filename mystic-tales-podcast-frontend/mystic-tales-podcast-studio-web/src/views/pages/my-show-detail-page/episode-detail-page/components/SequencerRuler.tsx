import { secondsToTime } from "@/core/utils/audio.util"

interface SequencerRulerProps {
    totalLengthSec: number
    pixelsPerSecond: number
    playhead: number
}

export default function SequencerRuler({ totalLengthSec, pixelsPerSecond, playhead }: SequencerRulerProps) {
    const totalWidth = Math.max(Math.ceil(totalLengthSec * pixelsPerSecond), 1)

    // Tính tickInterval dựa vào pixelsPerSecond để ruler rõ hơn khi zoom
    // Mục tiêu: mỗi tick cách nhau ít nhất ~40-80px
    const MIN_TICK_PX = 50; // khoảng cách tối thiểu giữa 2 tick (px)
    const MAX_TICK_PX = 150; // khoảng cách tối đa

    // Các bước thời gian có thể dùng (giây)
    const intervals = [1, 2, 5, 10, 15, 20, 30, 60, 120, 300, 600]; // 1s, 2s, 5s, 10s, 15s, 20s, 30s, 1m, 2m, 5m, 10m

    // Chọn interval sao cho khoảng cách px nằm trong khoảng hợp lý
    let tickInterval = 5; // fallback
    for (const interval of intervals) {
        const tickPx = interval * pixelsPerSecond;
        if (tickPx >= MIN_TICK_PX && tickPx <= MAX_TICK_PX) {
            tickInterval = interval;
            break;
        }
        if (tickPx > MAX_TICK_PX) break; // đã quá dày
        tickInterval = interval; // chưa đủ dày, nhưng đây là tốt nhất cho đến giờ
    }

    const numTicks = Math.ceil(totalLengthSec / tickInterval) + 1;

    // Major tick: mỗi 5 lần tickInterval hoặc điều chỉnh theo interval
    // VD: nếu tickInterval=1s thì major ở 0,5,10,... (mỗi 5s)
    //     nếu tickInterval=60s thì major ở 0,5*60,10*60,... (mỗi 5 phút)
    const majorStep = tickInterval < 10 ? 5 : (tickInterval < 60 ? 3 : 5);

    return (
        <div className="relative border border-slate-800 rounded bg-slate-900 h-[44px]" style={{ width: totalWidth }}>
            <div style={{ width: totalWidth }} className="relative h-full">
                {Array.from({ length: numTicks }).map((_, i) => {
                    const timeSec = i * tickInterval;
                    if (timeSec > totalLengthSec) return null;
                    const left = timeSec * pixelsPerSecond;
                    const major = i % majorStep === 0;
                    return (
                        <div key={i} className="absolute top-0 h-full" style={{ left, width: 1 }}>
                            <div className={`w-px ${major ? "h-full bg-slate-600" : "h-1/2 bg-slate-700"}`} />
                            {major && <div className="absolute top-0 left-1 text-xs text-slate-300">{secondsToTime(timeSec)}</div>}
                        </div>
                    )
                })}
                <div className="absolute top-0 bottom-0 w-px bg-rose-400" style={{ left: playhead * pixelsPerSecond }} />
            </div>
        </div>
    )
}