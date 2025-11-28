import { useEffect, useRef, useState } from "react"
import './glassmorphism.scss'
import Hls from 'hls.js'
import axios from "axios"

export default function Clean() {
    const [playlistFile, setPlaylistFile] = useState<string | null>(null)
    const hlsInstanceRef = useRef<Hls | null>(null)
    const audioRef = useRef<HTMLAudioElement | null>(null)
    const [isLoading, setIsLoading] = useState(false)
    const [isReady, setIsReady] = useState(false)
    const [isPlaying, setIsPlaying] = useState(false)
    const [currentTime, setCurrentTime] = useState(0)
    const [duration, setDuration] = useState(0)
    const [token, setToken] = useState<string | null>(null)
    const [playlistFileKey, setPlaylistFileKey] = useState<string | null>(null)


    const isSeekingRef = useRef<boolean>(false)
    const pendingSeekRef = useRef<number | null>(null)


    const formatTime = (sec: number) => {
        if (!isFinite(sec) || sec < 0) sec = 0
        const m = Math.floor(sec / 60)
        const s = Math.floor(sec % 60)
        return `${m}:${s.toString().padStart(2, '0')}`
    }
const PodcastEpisodeId = 'episode-12345' 
    //nhận file playlist.m3u8 từ backend
    const handleGetPlaylist = async () => {
        const listen = await axios.get(`https://72b5d2d82b8e.ngrok-free.app/api/podcast-service/api/episodes/${PodcastEpisodeId}/listen`)
        setToken(listen.data.Token)
        setPlaylistFileKey(listen.data.PlaylistFileKey)

        const playlist =  `https://72b5d2d82b8e.ngrok-free.app/api/podcast-service/api/episodes/hls-playlist/get-file-url/${playlistFileKey}`
        setPlaylistFile(playlist)
    }

    useEffect(() => {
        if (!playlistFile) return
        setIsLoading(true)
        setIsReady(false)
        setIsPlaying(false)

        if (hlsInstanceRef.current) { try { hlsInstanceRef.current.destroy() } catch { } hlsInstanceRef.current = null }
        if (audioRef.current) { try { audioRef.current.pause(); audioRef.current.src = '' } catch { } audioRef.current = null }

        const audioEl = document.createElement('audio')
        audioEl.preload = 'auto'
        audioEl.crossOrigin = 'anonymous'
        audioRef.current = audioEl

        const onLoadedMeta = () => {
            setDuration(isFinite(audioEl.duration) ? audioEl.duration : 0)
            setCurrentTime(audioEl.currentTime || 0)
            setIsReady(true)
            setIsLoading(false)
            audioEl.play().then(() => setIsPlaying(true)).catch(() => { })
        }
        const onTime = () => setCurrentTime(audioEl.currentTime || 0)
        const onEnded = () => setIsPlaying(false)

        audioEl.addEventListener('loadedmetadata', onLoadedMeta)
        audioEl.addEventListener('timeupdate', onTime)
        audioEl.addEventListener('ended', onEnded)

        if (Hls.isSupported()) {
            // ví dụ: lấy token từ localStorage/session
            tokenRef.current = localStorage.getItem('hlsKeyToken') || 'your-signed-token'

            const h = new Hls({
                enableWorker: true,
                fetchSetup: (context: any, init?: RequestInit) => {
                    const headers = new Headers(init?.headers || {})
                    headers.set('ngrok-skip-browser-warning', '69420')
                    // có thể thêm auth nếu BE yêu cầu
                    const bearer = localStorage.getItem('hlsBearer')
                    if (bearer) headers.set('Authorization', `Bearer ${bearer}`)

                    let url = context.url

                    if (context.type === 'key') {
                        // playlist chỉ trả keyId -> dựng lại URL + token
                        const buildKeyUrl = (raw: string) => {
                            // cố gắng lấy kid từ raw (có thể là 'kid=...' hoặc chỉ là hex)
                            try {
                                // nếu raw là URL hoặc query
                                const base = new URL(KEY_BASE)
                                const u = new URL(raw, base) // hỗ trợ cả 'kid=...' hoặc '/api/...'
                                const qsKid = u.searchParams.get('kid')
                                const kid = qsKid || u.pathname.split('/').pop() || raw
                                const token = tokenRef.current || ''
                                // ví dụ BE: GET /api/hls/key/{kid}?token=...
                                return `${KEY_BASE}/api/hls/key/${encodeURIComponent(kid)}?token=${encodeURIComponent(token)}`
                            } catch {
                                // fallback: raw là chuỗi kid thuần
                                const token = tokenRef.current || ''
                                return `${KEY_BASE}/api/hls/key/${encodeURIComponent(raw)}?token=${encodeURIComponent(token)}`
                            }
                        }

                        const keyUrl = buildKeyUrl(url)
                        // nếu muốn dùng POST:
                        // return new Request(`${KEY_BASE}/api/hls/key`, {
                        //   method: 'POST',
                        //   headers: new Headers({ ...Object.fromEntries(headers as any), 'Content-Type': 'application/json' }),
                        //   body: JSON.stringify({ kid: extractedKid, token: tokenRef.current }),
                        //   credentials: 'include'
                        // })

                        return new Request(keyUrl, { ...init, method: 'GET', headers, credentials: 'include' })
                    }

                    // manifest/segment... giữ nguyên nhưng có thể thêm headers/credentials
                    return new Request(url, { ...init, headers, credentials: 'include' })
                }
            })
            // ...existing code...
            h.loadSource(playlistFile)
            h.attachMedia(audioEl)
            hlsInstanceRef.current = h
        } else {
            audioEl.src = playlistFile
        }

        return () => {
            audioEl.removeEventListener('loadedmetadata', onLoadedMeta)
            audioEl.removeEventListener('timeupdate', onTime)
            audioEl.removeEventListener('ended', onEnded)
            try { audioEl.pause() } catch { }
            if (hlsInstanceRef.current) { try { hlsInstanceRef.current.destroy() } catch { } hlsInstanceRef.current = null }
            audioRef.current = null
        }
    }, [playlistFile])


    return (
        <>
            <div className="mystic-container">
                <h1 className="mystic-title">🔮 Mystic Podcast EQ Studio</h1>
                <div className="glass-card">
                    <div className="preset-section">
                        <div className="ws-section">
                            <div className="ws-header">
                                <strong>Player (Simulated)</strong>
                                <div className="ws-actions">
                                    <button
                                        className="export-button"
                                        onClick={() => { handleGetPlaylist() }}
                                    >
                                        Load & Play
                                    </button>
                                    <button className="export-button" onClick={() => {
                                        if (!isReady || !audioRef.current) return
                                        if (isPlaying) {
                                            audioRef.current.pause();
                                            setIsPlaying(false);
                                        } else {
                                            audioRef.current.play().then(() => setIsPlaying(true)).catch(() => { })
                                        }
                                    }} disabled={!isReady}>
                                        {isPlaying ? '⏸️ Pause' : '▶️ Play'}
                                    </button>
                                    <button className="export-button" onClick={() => {
                                        if (!audioRef.current) return
                                        audioRef.current.pause();
                                        audioRef.current.currentTime = 0; setIsPlaying(false); setCurrentTime(0)
                                    }} disabled={!isReady}>
                                        ⏹️ Stop
                                    </button>
                                </div>
                            </div>

                            <div style={{ marginTop: 12 }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 12 }}>
                                    <div className="ws-time">{formatTime(currentTime)} / {formatTime(duration)}</div>
                                    <input
                                        className="ws-seek"
                                        type="range"
                                        min={0}
                                        max={1000}
                                        value={duration > 0 ? Math.round((currentTime / duration) * 1000) : 0}
                                        onChange={(e) => {
                                            if (!audioRef.current || duration <= 0) return
                                            const p = Number(e.target.value) / 1000
                                            audioRef.current.currentTime = p * duration
                                            setCurrentTime(audioRef.current.currentTime)
                                        }}
                                        onMouseDown={() => { isSeekingRef.current = true }}
                                        onTouchStart={() => { isSeekingRef.current = true }}
                                        onMouseUp={() => {
                                            isSeekingRef.current = false
                                            pendingSeekRef.current = null
                                        }}
                                        onTouchEnd={() => {
                                            isSeekingRef.current = false
                                            const pos = pendingSeekRef.current ?? Math.floor(audioRef.current?.currentTime || 0)
                                            pendingSeekRef.current = null
                                        }}
                                        disabled={!isReady}
                                    />
                                </div>
                                <div style={{ marginTop: 8, fontSize: 12, opacity: 0.8 }}>
                                    {isLoading && 'Loading playlist...'}{!isLoading && !isReady && 'No track loaded'}{isReady && 'Streaming'}
                                </div>
                                {playlistFile && <div style={{ marginTop: 6, fontSize: 12, opacity: 0.6 }}>Playlist: {playlistFile.split('?')[0]}</div>}
                            </div>
                        </div>


                    </div>
                </div>
            </div>
        </>
    )
}
