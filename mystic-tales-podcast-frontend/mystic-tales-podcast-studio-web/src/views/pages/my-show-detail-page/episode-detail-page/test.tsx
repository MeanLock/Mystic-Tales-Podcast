// "use client"

// import type React from "react"
// import { useCallback, useEffect, useMemo, useRef, useState } from "react"
// import WaveSurfer from "wavesurfer.js"
// import { Rnd } from "react-rnd"

// /**
//  * AudioSequencer v4 - Combined with EpisodeAudio sidebar
//  * - 2 fixed tracks: Original (0) & Background (1)
//  * - Original: 1 audio only, NO trim, NO delete
//  * - Background: Multiple audios, CAN trim
//  * - Ruler: follows Original duration (not trimmed)
//  */

// const secondsToTime = (s: number) => {
//   if (s < 0) s = 0
//   const m = Math.floor(s / 60)
//   const sec = Math.floor(s % 60)
//     .toString()
//     .padStart(2, "0")
//   return `${m}:${sec}`
// }

// const clamp = (v: number, min: number, max: number) => Math.max(min, Math.min(max, v))

// let _ac: AudioContext
// const getAC = () => {
//   if (!_ac) _ac = new (window.AudioContext || (window as any).webkitAudioContext)()
//   return _ac
// }

// interface Clip {
//   id: string
//   name: string
//   file: File
//   buffer: AudioBuffer
//   duration: number
//   start: number
//   trimStart: number
//   trimEnd: number
//   track: number
// }

// interface AudioSequencerProps {
//   originalAudioFile?: File
//   backgroundAudioFile?: File
//   onClipsChange?: (clips: Clip[]) => void
//   availableBackgrounds?: Array<{ id: string; name: string; file: string }>
// }

// export default function AudioSequencer({ originalAudioFile, backgroundAudioFile, onClipsChange, availableBackgrounds }: AudioSequencerProps) {
//   const [clips, setClips] = useState<Clip[]>([])
//   const [pixelsPerSecond, setPPS] = useState(120)
//   const rowH = 90
//   const [isPlaying, setIsPlaying] = useState(false)
//   const [playhead, setPlayhead] = useState(0)
//   const [globalLoop, setGlobalLoop] = useState(false)

//   // Per-track volume
//   const [trackVolumes, setTrackVolumes] = useState([1, 1])

//   const acRef = useRef<AudioContext | null>(null)
//   const startWallClockRef = useRef(0)
//   const startPlayheadRef = useRef(0)
//   const activeNodesRef = useRef<Array<{ src: AudioBufferSourceNode }>>([])
//   const rafRef = useRef<number>()
//   const trackGainsRef = useRef<Array<{ gain: GainNode }>>([])

//   useEffect(() => {
//     const ac = getAC()
//     acRef.current = ac

//     const loadFile = async (file: File, trackIdx: number) => {
//       if (!file) return
//       try {
//         const buffer = await ac.decodeAudioData(await file.arrayBuffer())
//         const newClip: Clip = {
//           id: `${file.name}-${Date.now()}-${trackIdx}`,
//           name: file.name,
//           file,
//           buffer,
//           duration: buffer.duration,
//           start: 0,
//           trimStart: 0,
//           trimEnd: 0,
//           track: trackIdx,
//         }
//         setClips((prev) => {
//           const filtered = prev.filter((c) => c.track !== trackIdx)
//           return [...filtered, newClip]
//         })
//       } catch (e) {
//         console.error("Failed to load audio:", e)
//       }
//     }

//     if (originalAudioFile) loadFile(originalAudioFile, 0) // Track 0 = Original
//     if (backgroundAudioFile) loadFile(backgroundAudioFile, 1) // Track 1 = Background
//   }, [originalAudioFile, backgroundAudioFile])

//   useEffect(() => {
//     onClipsChange?.(clips)
//   }, [clips, onClipsChange])

//   // Ensure gain nodes
//   const ensureTrackGains = useCallback(() => {
//     const ac = acRef.current || getAC()
//     while (trackGainsRef.current.length < 2) {
//       const gain = ac.createGain()
//       gain.gain.value = 1
//       gain.connect(ac.destination)
//       trackGainsRef.current.push({ gain })
//     }
//     trackGainsRef.current[0].gain.gain.value = trackVolumes[0] ?? 1
//     trackGainsRef.current[1].gain.gain.value = trackVolumes[1] ?? 1
//   }, [trackVolumes])

//   useEffect(() => {
//     ensureTrackGains()
//   }, [ensureTrackGains])

//   // Ruler length = Original track duration (no trim)
//   const originalDuration = useMemo(() => {
//     const clip = clips.find((c) => c.track === 0)
//     return clip ? clip.duration : 30
//   }, [clips])

//   const totalLengthSec = useMemo(() => {
//     const mx = clips.reduce((m, c) => {
//       const vis = Math.max(0, c.duration - c.trimStart - c.trimEnd)
//       return Math.max(m, c.start + vis)
//     }, originalDuration)
//     return Math.max(mx, originalDuration)
//   }, [clips, originalDuration])

//   // Transport control
//   const stopAll = () => {
//     activeNodesRef.current.forEach(({ src }) => {
//       try {
//         src.stop()
//       } catch { }
//     })
//     activeNodesRef.current = []
//     cancelAnimationFrame(rafRef.current!)
//   }

//   const scheduleFrom = useCallback(
//     (fromSec: number) => {
//       const ac = acRef.current || getAC()
//       ensureTrackGains()

//       stopAll()

//       startWallClockRef.current = ac.currentTime
//       startPlayheadRef.current = fromSec

//       for (let t = 0; t < 2; t++) {
//         const tGain = trackGainsRef.current[t]?.gain || ac.destination
//         const tClips = clips.filter((c) => c.track === t)

//         const scheduleClipAt = (clip: Clip, offsetSec: number) => {
//           const playStart = clip.start + offsetSec
//           const clipDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
//           const playEnd = playStart + clipDur
//           if (clipDur <= 0) return
//           if (fromSec < playEnd) {
//             const when = Math.max(0, playStart - fromSec)
//             const offset = Math.max(0, fromSec - playStart) + clip.trimStart
//             const dur = clipDur - Math.max(0, fromSec - playStart)
//             const src = ac.createBufferSource()
//             src.buffer = clip.buffer
//             src.connect(tGain)
//             try {
//               src.start(ac.currentTime + when, offset, Math.max(0, dur))
//               activeNodesRef.current.push({ src })
//             } catch { }
//           }
//         }

//         tClips.forEach((c) => scheduleClipAt(c, 0))
//       }

//       const tick = () => {
//         const elapsed = ac.currentTime - startWallClockRef.current
//         const ph = startPlayheadRef.current + elapsed
//         setPlayhead(ph)
//         if (globalLoop && ph >= totalLengthSec) {
//           scheduleFrom(0)
//           return
//         }
//         rafRef.current = requestAnimationFrame(tick)
//       }
//       rafRef.current = requestAnimationFrame(tick)
//     },
//     [clips, globalLoop, totalLengthSec, ensureTrackGains],
//   )

//   const onPlay = () => {
//     if (isPlaying) return
//     getAC().resume()
//     setIsPlaying(true)
//     scheduleFrom(playhead)
//   }
//   const onPause = () => {
//     setIsPlaying(false)
//     stopAll()
//   }
//   const onStop = () => {
//     onPause()
//     setPlayhead(0)
//   }

//   useEffect(() => () => stopAll(), [])

//   // Add background from available sounds
//   const addBackgroundSound = useCallback(async (backgroundSound: { id: string; name: string; file: string }) => {
//     const ac = acRef.current || getAC()
//     try {
//       const response = await fetch(backgroundSound.file)
//       const arrayBuffer = await response.arrayBuffer()
//       const buffer = await ac.decodeAudioData(arrayBuffer)

//       // Check for time conflicts with existing background clips
//       const existingBgClips = clips.filter(c => c.track === 1)
//       const hasConflict = existingBgClips.some(clip => {
//         const clipEnd = clip.start + Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
//         return !(0 >= clipEnd || buffer.duration <= clip.start) // Overlap check
//       })

//       if (hasConflict) {
//         alert("Cannot add background sound: it would overlap with existing background audio")
//         return
//       }

//       const blob = await response.blob()
//       const file = new File([blob], backgroundSound.name, { type: 'audio/mpeg' })

//       const newClip: Clip = {
//         id: `${backgroundSound.id}-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
//         name: backgroundSound.name,
//         file,
//         buffer,
//         duration: buffer.duration,
//         start: 0, // Will be positioned by user
//         trimStart: 0,
//         trimEnd: 0,
//         track: 1, // Background track
//       }
//       setClips((prev) => [...prev, newClip])
//     } catch (error) {
//       console.error("Failed to load background sound:", error)
//     }
//   }, [clips])

//   // File input for background (legacy - can be removed if not needed)
//   const onInputChange = useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
//     if (!e.target.files?.length) return
//     const file = e.target.files[0]
//     const ac = acRef.current || getAC()
//     try {
//       const buffer = await ac.decodeAudioData(await file.arrayBuffer())
//       const newClip: Clip = {
//         id: `${file.name}-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
//         name: file.name,
//         file,
//         buffer,
//         duration: buffer.duration,
//         start: 0,
//         trimStart: 0,
//         trimEnd: 0,
//         track: 1, // Background track
//       }
//       setClips((prev) => [...prev, newClip])
//     } catch (e) {
//       console.error("Failed to load audio:", e)
//     }
//   }, [])

//   const totalWidth = Math.ceil(totalLengthSec * pixelsPerSecond)

//   return (
//     <div className="w-full text-slate-100 p-4 flex flex-col gap-3 select-none">
//       {/* Toolbar */}
//       <header className="flex flex-wrap items-center gap-3">
//         <div className="text-sm font-semibold">Audio Sequencer</div>
//         <button onClick={onPlay} className="px-3 py-1 rounded bg-emerald-600 hover:bg-emerald-700">
//           Play
//         </button>
//         <button onClick={onPause} className="px-3 py-1 rounded bg-amber-600 hover:bg-amber-700">
//           Pause
//         </button>
//         <button onClick={onStop} className="px-3 py-1 rounded bg-rose-600 hover:bg-rose-700">
//           Stop
//         </button>
//         <div className="ml-4 flex items-center gap-2">
//           <label className="text-sm">Zoom</label>
//           <input
//             type="range"
//             min={40}
//             max={320}
//             value={pixelsPerSecond}
//             onChange={(e) => setPPS(Number.parseInt(e.target.value))}
//           />
//         </div>
//         <label className="ml-4 flex items-center gap-2 text-sm">
//           <span>Loop</span>
//           <input type="checkbox" checked={globalLoop} onChange={(e) => setGlobalLoop(e.target.checked)} />
//         </label>
//         {availableBackgrounds && availableBackgrounds.length > 0 && (
//           <div className="ml-auto flex gap-2">
//             {availableBackgrounds.map((bg) => (
//               <button
//                 key={bg.id}
//                 onClick={() => addBackgroundSound(bg)}
//                 className="px-3 py-1 rounded bg-sky-600 hover:bg-sky-700 text-sm"
//                 title={`Add ${bg.name}`}
//               >
//                 + {bg.name}
//               </button>
//             ))}
//           </div>
//         )}
//         <label className="cursor-pointer px-3 py-1 rounded bg-slate-600 hover:bg-slate-700 text-sm">
//           + Upload File
//           <input type="file" accept="audio/*" onChange={onInputChange} className="hidden" />
//         </label>
//       </header>

//       {/* Tracks */}
//       <div className="relative border border-slate-800 rounded overflow-hidden">
//         {/* Track headers */}
//         <div className="bg-slate-900 border-b border-slate-800">
//           {/* Original track */}
//           <div className="flex items-center gap-3 px-3" style={{ height: rowH, borderBottom: "1px solid #0b1320" }}>
//             <div className="text-xs w-20 font-semibold">Original</div>
//             <div className="flex items-center gap-2">
//               <label className="text-xs">Vol</label>
//               <input
//                 type="range"
//                 min={0}
//                 max={1}
//                 step={0.01}
//                 value={trackVolumes[0] ?? 1}
//                 onChange={(e) => {
//                   const v = Number.parseFloat(e.target.value)
//                   setTrackVolumes([v, trackVolumes[1]])
//                 }}
//               />
//             </div>
//           </div>

//           {/* Background track */}
//           <div className="flex items-center gap-3 px-3" style={{ height: rowH, borderBottom: "1px solid #0b1320" }}>
//             <div className="text-xs w-20 font-semibold">Background</div>
//             <div className="flex items-center gap-2">
//               <label className="text-xs">Vol</label>
//               <input
//                 type="range"
//                 min={0}
//                 max={1}
//                 step={0.01}
//                 value={trackVolumes[1] ?? 1}
//                 onChange={(e) => {
//                   const v = Number.parseFloat(e.target.value)
//                   setTrackVolumes([trackVolumes[0], v])
//                 }}
//               />
//             </div>
//           </div>
//         </div>

//         {/* Timeline body */}
//         <div className="relative overflow-auto bg-slate-900" style={{ height: rowH * 2 }}>
//           <div style={{ width: totalWidth, height: rowH * 2 }} className="relative">
//             {/* Track backgrounds */}
//             <div className="absolute left-0 right-0" style={{ top: 0, height: rowH, background: "#0b1220" }} />
//             <div className="absolute left-0 right-0" style={{ top: rowH, height: rowH, background: "#0f172a" }} />

//             {/* Clips */}
//             {clips.map((clip) => (
//               <ClipRnd
//                 key={clip.id}
//                 clip={clip}
//                 pps={pixelsPerSecond}
//                 rowH={rowH}
//                 isOriginal={clip.track === 0}
//                 allClips={clips}
//                 onChange={(next) => {
//                   if (next && next.__delete) {
//                     setClips((prev) => prev.filter((c) => c.id !== clip.id))
//                   } else {
//                     setClips((prev) => prev.map((c) => (c.id === clip.id ? { ...c, ...next } : c)))
//                   }
//                 }}
//               />
//             ))}

//             {/* Playhead */}
//             <div
//               className="pointer-events-none absolute top-0 bottom-0 w-px bg-rose-400"
//               style={{ left: playhead * pixelsPerSecond }}
//             />
//           </div>
//         </div>
//       </div>

//       {/* Ruler */}
//       <div className="relative overflow-x-auto overflow-y-hidden border border-slate-800 rounded bg-slate-900 h-[44px]">
//         <div style={{ width: totalWidth }} className="relative h-full">
//           {Array.from({ length: Math.ceil(totalLengthSec) + 1 }).map((_, i) => {
//             const left = i * pixelsPerSecond
//             const major = i % 5 === 0
//             return (
//               <div key={i} className="absolute top-0 h-full" style={{ left, width: 1 }}>
//                 <div className={`w-px ${major ? "h-full bg-slate-600" : "h-1/2 bg-slate-700"}`} />
//                 {major && <div className="absolute top-0 left-1 text-xs text-slate-300">{secondsToTime(i)}</div>}
//               </div>
//             )
//           })}
//           <div className="absolute top-0 bottom-0 w-px bg-rose-400" style={{ left: playhead * pixelsPerSecond }} />
//         </div>
//       </div>

//       <div className="text-xs text-slate-400">
//         Original: {secondsToTime(originalDuration)} • Playhead: {secondsToTime(playhead)} /{" "}
//         {secondsToTime(totalLengthSec)}
//       </div>
//     </div>
//   )
// }

// // ---------- Clip Component ----------
// interface ClipRndProps {
//   clip: Clip
//   pps: number
//   rowH: number
//   isOriginal: boolean
//   onChange: (update: Partial<Clip> & { __delete?: boolean }) => void
//   allClips: Clip[]
// }

// function ClipRnd({ clip, pps, rowH, isOriginal, onChange, allClips }: ClipRndProps) {
//   const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
//   const width = Math.max(8, visibleDur * pps)
//   const x = clip.start * pps
//   const y = clip.track * rowH
//   const height = rowH
//   const dragHandle = "clip-drag-handle"
//   const snapGridX = Math.max(1, Math.round(pps / 20))

//   return (
//     <Rnd
//       size={{ width, height }}
//       position={{ x, y }}
//       bounds="parent"
//       dragAxis="x"
//       dragGrid={[snapGridX, 0]}
//       enableResizing={
//         isOriginal
//           ? false
//           : {
//             left: true,
//             right: true,
//             top: false,
//             bottom: false,
//             topLeft: false,
//             topRight: false,
//             bottomLeft: false,
//             bottomRight: false,
//           }
//       }
//       dragHandleClassName={dragHandle}
//       onDragStop={(e, d) => {
//         const newStart = Math.max(0, d.x / pps)
//         onChange({ start: newStart })
//       }}
//       onResizeStop={(e, dir, ref, delta, pos) => {
//         if (isOriginal) return; // Không cho cắt bản gốc

//         // pps = pixel per second (tỉ lệ hiển thị)
//         const newVisible = Math.max(0.01, ref.offsetWidth / pps);

//         if (dir === "left") {
//           // delta.width < 0 nghĩa là kéo sang phải (cắt đầu)
//           const deltaSeconds = -delta.width / pps;

//           // trimStart tăng lên tương ứng
//           let nextTrimStart = clip.trimStart + deltaSeconds;
//           // không cho vượt quá duration - trimEnd - 0.01
//           nextTrimStart = clamp(
//             nextTrimStart,
//             0,
//             Math.max(0, clip.duration - clip.trimEnd - 0.01)
//           );

//           // cập nhật vị trí hiển thị thực tế (vì clip bị "ngắn" lại)
//           const nextStart = clip.start + deltaSeconds;

//           onChange({
//             trimStart: +nextTrimStart,
//             start: +Math.max(0, nextStart),
//           });
//         }

//         if (dir === "right") {
//           // cắt phía phải dựa vào visible width
//           const nextTrimEnd = clamp(
//             Math.max(0, clip.duration - clip.trimStart - newVisible),
//             0,
//             Math.max(0, clip.duration - clip.trimStart - 0.01)
//           );
//           onChange({ trimEnd: +nextTrimEnd });
//         }
//       }}



//       className="rounded-xl shadow-lg border border-slate-700 bg-slate-800"
//     >
//       <ClipWaveform
//         clip={clip}
//         height={height}
//         width={width}
//         dragHandle={dragHandle}
//         isOriginal={isOriginal}
//         onDelete={() => onChange({ __delete: true })}
//       />
//     </Rnd>
//   )
// }

// interface ClipWaveformProps {
//   clip: Clip
//   width: number
//   height: number
//   dragHandle: string
//   isOriginal: boolean
//   onDelete: () => void
// }

// function ClipWaveform({ clip, width, height, dragHandle, isOriginal, onDelete }: ClipWaveformProps) {
//   const containerRef = useRef<HTMLDivElement>(null)
//   const wsRef = useRef<WaveSurfer | null>(null)

//   useEffect(() => {
//     if (!containerRef.current) return
//     const ws = WaveSurfer.create({
//       container: containerRef.current,
//       height: Math.max(28, height - 28),
//       waveColor: "#7BA225",
//       progressColor: "#AEE339",
//       cursorWidth: 0,
//       interact: false,
//       normalize: true,

//       minPxPerSec: 10,
//     })
//     wsRef.current = ws
//     ws.loadBlob(clip.file).catch(() => { })
//     return () => {
//       try {
//         ws.destroy()
//       } catch { }
//     }
//   }, [clip.file])

//   useEffect(() => {
//   if (!wsRef.current) return;
//   const ws = wsRef.current;

//   const start = clip.trimStart;
//   const end = clip.duration - clip.trimEnd;

//   // Giữ nguyên tỉ lệ zoom ban đầu, chỉ cắt phần hiển thị
//   try {
//     ws.setTimeRange(start, end);
//   } catch (err) {
//     console.warn("setTimeRange failed", err);
//   }
// }, [clip.trimStart, clip.trimEnd]);


//   const visibleDur = Math.max(0, clip.duration - clip.trimStart - clip.trimEnd)
//   const leftTrimPx = (clip.trimStart / clip.duration) * width;
//   const rightTrimPx = (clip.trimEnd / clip.duration) * width;

//   return (
//     <div className="w-full h-full flex flex-col">
//       <div
//         className={`px-2 py-1 text-xs text-slate-200 flex items-center justify-between border-b border-slate-700 ${dragHandle}`}
//       >
//         <div className="truncate" title={clip.name}>
//           {clip.name}
//         </div>
//         <div className="flex items-center gap-2">
//           {!isOriginal && (
//             <button
//               className="px-2 py-0.5 rounded bg-rose-600 hover:bg-rose-700"
//               onClick={(e) => {
//                 e.stopPropagation()
//                 onDelete()
//               }}
//             >
//               Delete
//             </button>
//           )}
//           <div className="text-slate-400">{secondsToTime(visibleDur)}</div>
//         </div>
//       </div>

//       <div className="flex-1 relative">
//         <div ref={containerRef} className="absolute inset-0" />
//         {!isOriginal && (
//           <>
//             {/* Overlay trái: che phần đã trim đầu */}
//             <div className="absolute top-0 bottom-0 bg-black/40" style={{ left: 0, width: leftTrimPx }} />
//             {/* Overlay phải: che phần đã trim cuối */}
//             <div className="absolute top-0 bottom-0 bg-black/40" style={{ right: 0, width: rightTrimPx }} />
//           </>
//         )}
//       </div>
//     </div>
//   )
// }
