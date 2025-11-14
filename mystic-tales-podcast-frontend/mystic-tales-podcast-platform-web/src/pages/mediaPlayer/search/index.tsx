// @ts-nocheck

import { useEffect, useRef, useState } from "react";

import Hls from "hls.js";
import axios from "axios";
const KEY_BASE = "https://65662aa8a6e5.ngrok-free.app";
const PodcastEpisodeId = "9735b0a8-f38e-4f93-a86d-1fda7558872a";
const auth =
  "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEwMTIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVsWpIFRo4buLIEYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ2dXRoaWZAZW1haWwuY29tIiwiaWQiOiIxMDEyIiwicm9sZV9pZCI6IjEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDdXN0b21lciIsImJhbGFuY2UiOiIwLjAwIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9zZXJpYWxudW1iZXIiOiJhZDI0NGMwMy04ZWEyLTQ5NDUtOTZhNy1hZmRjZDIxZDE3ZTciLCJleHAiOjc3NjEzODUzOTUsImlzcyI6ImxvY2FsaG9zdCIsImF1ZCI6ImxvY2FsaG9zdCJ9.hKMmv2Ax0EhF1pwAw5LLLdv3YKXb-LaoVMIBu-hYUBhoJjoLHt0LdCa-yfxeHlF-0Kxuc6WG8VWci_oO9eEU8ySOAca9EsFF4LARqf1xXqwA295ync6TBrWMpM1Wjf5UCw_GxGq9iSfESYn9aMfxlEdvj33CRI39xD-xhaqgg_JpCk1BV1Iz0pYjEW_Jibo9Qm6VfxVadciZxfGa89h6YVEOOGTlaGWwCKYk0HuK5ygXKpcGGGjtswH-dhcwoBUl1XHK_g9czryS-tiHlTTPV6lPd1m7IWq4VhbIrtV_Qaz2OuGTT3NjOg8ARlgzNo5qWfNC2cIe-KBAaIENHTReiQ";
export default function SearchPage() {
  const [playlistFile, setPlaylistFile] = useState<any | null>(null);
  const hlsInstanceRef = useRef<Hls | null>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isReady, setIsReady] = useState(false);
  const [isPlaying, setIsPlaying] = useState(false);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const tokenRef = useRef<string | null>(null);
  const [playlistFileKey, setPlaylistFileKey] = useState<any | null>(null);

  const isSeekingRef = useRef<boolean>(false);
  const pendingSeekRef = useRef<number | null>(null);

  const formatTime = (sec: number) => {
    if (!isFinite(sec) || sec < 0) sec = 0;
    const m = Math.floor(sec / 60);
    const s = Math.floor(sec % 60);
    return `${m}:${s.toString().padStart(2, "0")}`;
  };

  const handleGetPlaylist = async () => {
    const url = `${KEY_BASE}/api/podcast-service/api/episodes/${PodcastEpisodeId}/listen`;

    const listen = await axios.get(url, {
      headers: {
        Authorization: `Bearer ${auth}`,
        "ngrok-skip-browser-warning": "69420",
      },
      withCredentials: true,
    });

    tokenRef.current = listen.data.Token;
    console.log("listen", listen);
    const keyFromApi = listen.data.PlaylistFileKey;

    const playlistUrl = `${KEY_BASE}/api/podcast-service/api/episodes/hls-playlist/get-file-data/${keyFromApi}`;

    setPlaylistFile(playlistUrl);
  };

  useEffect(() => {
    if (!playlistFile) return;
    setIsLoading(true);
    setIsReady(false);
    setIsPlaying(false);

    if (hlsInstanceRef.current) {
      try {
        hlsInstanceRef.current.destroy();
      } catch {}
      hlsInstanceRef.current = null;
    }
    if (audioRef.current) {
      try {
        audioRef.current.pause();
        audioRef.current.src = "";
      } catch {}
      audioRef.current = null;
    }

    const audioEl = document.createElement("audio");
    audioEl.preload = "auto";
    audioEl.crossOrigin = "anonymous";
    audioRef.current = audioEl;

    const onLoadedMeta = () => {
      setDuration(isFinite(audioEl.duration) ? audioEl.duration : 0);
      setCurrentTime(audioEl.currentTime || 0);
      setIsReady(true);
      setIsLoading(false);
      audioEl
        .play()
        .then(() => setIsPlaying(true))
        .catch(() => {});
    };
    const onTime = () => setCurrentTime(audioEl.currentTime || 0);
    const onEnded = () => setIsPlaying(false);

    audioEl.addEventListener("loadedmetadata", onLoadedMeta);
    audioEl.addEventListener("timeupdate", onTime);
    audioEl.addEventListener("ended", onEnded);
    console.log("Hls supported:", Hls.isSupported());
    if (Hls.isSupported()) {
      const h = new Hls({
        enableWorker: true,
        maxBufferLength: 120, // buffer ~2 segment (60s)
        maxBufferHole: 0.5,
        maxMaxBufferLength: 300,
        maxBufferSize: 60 * 1000 * 1000,
        // XHR fallback (don’t call xhr.open() yourself)
        xhrSetup: (xhr: any, url: string) => {
          const originalOpen = xhr.open.bind(xhr);
          xhr.open = (method: string, u: string, async?: boolean) => {
            let next = u;
            if (/[0-9a-fA-F-]{36}$/.test(u)) {
              const kid = u.split("/").pop();
              next = `${KEY_BASE}/api/podcast-service/api/episodes/${PodcastEpisodeId}/hls-encryption-key/${kid}?token=${tokenRef.current}`;
            } else if (u.includes(".ts")) {
              const idx = url.lastIndexOf("main_files/PodcastEpisodes/");
              const fileKey = idx !== -1 ? url.substring(idx) : null;
              console.log("Matched segment file key:", fileKey);

              if (fileKey)
                next = `${KEY_BASE}/api/podcast-service/api/episodes/hls-segment/get-file-data/${fileKey}`;
            }
            return originalOpen(method, next, async);
          };
          xhr.withCredentials = true;
          xhr.setRequestHeader("Authorization", `Bearer ${auth}`);
          xhr.setRequestHeader("ngrok-skip-browser-warning", "69420");
        },
      });

      h.loadSource(`${playlistFile}?t=${Date.now()}`);
      h.attachMedia(audioRef.current!);
      hlsInstanceRef.current = h;
    } else {
      audioRef.current!.src = playlistFile;
    }

    return () => {
      audioEl.removeEventListener("loadedmetadata", onLoadedMeta);
      audioEl.removeEventListener("timeupdate", onTime);
      audioEl.removeEventListener("ended", onEnded);
      try {
        audioEl.pause();
      } catch {}
      if (hlsInstanceRef.current) {
        try {
          hlsInstanceRef.current.destroy();
        } catch {}
        hlsInstanceRef.current = null;
      }
      audioRef.current = null;
    };
  }, [playlistFile]);

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
                    onClick={() => {
                      handleGetPlaylist();
                    }}
                  >
                    Load & Play
                  </button>
                  <button
                    className="export-button"
                    onClick={() => {
                      if (!isReady || !audioRef.current) return;
                      if (isPlaying) {
                        audioRef.current.pause();
                        setIsPlaying(false);
                      } else {
                        audioRef.current
                          .play()
                          .then(() => setIsPlaying(true))
                          .catch(() => {});
                      }
                    }}
                    disabled={!isReady}
                  >
                    {isPlaying ? "⏸️ Pause" : "▶️ Play"}
                  </button>
                  <button
                    className="export-button"
                    onClick={() => {
                      if (!audioRef.current) return;
                      audioRef.current.pause();
                      audioRef.current.currentTime = 0;
                      setIsPlaying(false);
                      setCurrentTime(0);
                    }}
                    disabled={!isReady}
                  >
                    ⏹️ Stop
                  </button>
                </div>
              </div>

              <div style={{ marginTop: 12 }}>
                <div
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    gap: 12,
                  }}
                >
                  <div className="ws-time">
                    {formatTime(currentTime)} / {formatTime(duration)}
                  </div>
                  <input
                    className="ws-seek"
                    type="range"
                    min={0}
                    max={1000}
                    value={
                      duration > 0
                        ? Math.round((currentTime / duration) * 1000)
                        : 0
                    }
                    onChange={(e) => {
                      if (!audioRef.current || duration <= 0) return;
                      const p = Number(e.target.value) / 1000;
                      audioRef.current.currentTime = p * duration;
                      setCurrentTime(audioRef.current.currentTime);
                    }}
                    onMouseDown={() => {
                      isSeekingRef.current = true;
                    }}
                    onTouchStart={() => {
                      isSeekingRef.current = true;
                    }}
                    onMouseUp={() => {
                      isSeekingRef.current = false;
                      pendingSeekRef.current = null;
                    }}
                    onTouchEnd={() => {
                      isSeekingRef.current = false;
                      const pos =
                        pendingSeekRef.current ??
                        Math.floor(audioRef.current?.currentTime || 0);
                      pendingSeekRef.current = null;
                    }}
                    disabled={!isReady}
                  />
                </div>
                <div style={{ marginTop: 8, fontSize: 12, opacity: 0.8 }}>
                  {isLoading && "Loading playlist..."}
                  {!isLoading && !isReady && "No track loaded"}
                  {isReady && "Streaming"}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
