import { secondsToTime } from "@/core/utils/audio.util"

interface SequencerRulerProps {
    totalLengthSec: number
    pixelsPerSecond: number
    playhead: number
}

export default function SequencerRuler({ totalLengthSec, pixelsPerSecond, playhead }: SequencerRulerProps) {
    const totalWidth = Math.max(Math.ceil(totalLengthSec * pixelsPerSecond), 1)

    // Xác định interval dựa theo mức độ zoom (pixelsPerSecond)
    // Tính khoảng thời gian hiển thị trên ~1200px (ước lượng container width)
    const ESTIMATED_CONTAINER_WIDTH = 1200;
    const visibleSeconds = ESTIMATED_CONTAINER_WIDTH / pixelsPerSecond;
    const visibleMinutes = visibleSeconds / 60;

    let tickInterval = 60; // mặc định 1 phút
    let majorStep = 5;

    // Khoảng cách tối thiểu và tối đa giữa các tick (pixels)
    const MIN_TICK_PX = 40;
    const MAX_TICK_PX = 120;

    if (visibleMinutes < 0.5) {
        // Zoom cực to (xem < 30 giây): 1s, 2s, 5s
        const intervals = [1, 2, 5];
        tickInterval = 1; // fallback
        majorStep = 5;
        
        for (const interval of intervals) {
            const tickPx = interval * pixelsPerSecond;
            if (tickPx >= MIN_TICK_PX) {
                tickInterval = interval;
                majorStep = interval === 1 ? 10 : (interval === 2 ? 5 : 4);
                break;
            }
        }
    } else if (visibleMinutes < 1.5) {
        // Zoom rất to (xem < 1.5 phút): 5s, 10s, 15s
        const intervals = [5, 10, 15];
        tickInterval = 5; // fallback
        majorStep = 4;
        
        for (const interval of intervals) {
            const tickPx = interval * pixelsPerSecond;
            if (tickPx >= MIN_TICK_PX) {
                tickInterval = interval;
                majorStep = interval === 5 ? 6 : (interval === 10 ? 3 : 4);
                break;
            }
        }
    } else if (visibleMinutes < 3) {
        // Khi zoom to (xem < 3 phút trên màn hình): 10s, 20s, 30s
        const intervals = [10, 20, 30];
        tickInterval = 10; // fallback
        majorStep = 3;
        
        for (const interval of intervals) {
            const tickPx = interval * pixelsPerSecond;
            if (tickPx >= MIN_TICK_PX) {
                tickInterval = interval;
                majorStep = interval === 10 ? 3 : (interval === 20 ? 3 : 2);
                break;
            }
        }
    } else if (visibleMinutes < 5) {
        // Xem < 5 phút trên màn hình: 20s, 40s, 60s
        const intervals = [20, 40, 60];
        tickInterval = 20; // fallback
        majorStep = 3;
        
        for (const interval of intervals) {
            const tickPx = interval * pixelsPerSecond;
            if (tickPx >= MIN_TICK_PX) {
                tickInterval = interval;
                majorStep = interval === 20 ? 3 : (interval === 40 ? 3 : 2);
                break;
            }
        }
    } else if (visibleMinutes < 8) {
        // Xem < 8 phút trên màn hình: 30s, 60s
        const intervals = [30, 60];
        tickInterval = 30; // fallback
        majorStep = 2;
        
        for (const interval of intervals) {
            const tickPx = interval * pixelsPerSecond;
            if (tickPx >= MIN_TICK_PX) {
                tickInterval = interval;
                majorStep = 2;
                break;
            }
        }
    } else if (visibleMinutes < 10) {
        // Xem < 10 phút trên màn hình: 60s
        tickInterval = 60;
        majorStep = 5;
        
        const tickPx = 60 * pixelsPerSecond;
        if (tickPx < MIN_TICK_PX) {
            // Nếu 60s quá nhỏ, dùng 120s hoặc 300s
            const intervals = [120, 300];
            for (const interval of intervals) {
                if (interval * pixelsPerSecond >= MIN_TICK_PX) {
                    tickInterval = interval;
                    majorStep = interval === 120 ? 3 : 2;
                    break;
                }
            }
        }
    } else {
        // Xem >= 10 phút trên màn hình: 60s, 120s, 300s
        const intervals = [60, 120, 300];
        tickInterval = 60; // fallback
        majorStep = 5;
        
        for (const interval of intervals) {
            const tickPx = interval * pixelsPerSecond;
            if (tickPx >= MIN_TICK_PX && tickPx <= MAX_TICK_PX) {
                tickInterval = interval;
                majorStep = interval === 60 ? 5 : (interval === 120 ? 3 : 2);
                break;
            }
            // Nếu tickPx quá lớn, dùng interval nhỏ hơn
            if (tickPx > MAX_TICK_PX) {
                break;
            }
            tickInterval = interval; // chọn interval lớn nhất mà vẫn < MAX_TICK_PX
        }
    }

    const numTicks = Math.ceil(totalLengthSec / tickInterval) + 1;

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