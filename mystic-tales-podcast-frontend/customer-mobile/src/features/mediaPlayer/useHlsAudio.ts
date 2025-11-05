// src/features/mediaPlayer/useHlsAudio.ts
import { useEffect, useMemo, useRef, useState, useCallback } from "react";
import { Audio, AVPlaybackStatus, AVPlaybackStatusSuccess } from "expo-av";
import { useGetAudioQuery } from "@/src/services/mediaPlayer/mediaPlayerApi"; // <- đường import hook RTK của bạn

type UseHlsAudioOpts = {
  autoplay?: boolean;
  initialMs?: number;
  headers?: Record<string, string>; // nếu PlaylistUrl cần header (hiếm gặp)
  progressUpdateIntervalMs?: number;
};

export type HlsAudioState = {
  ready: boolean;
  loading: boolean;
  isPlaying: boolean;
  positionMs: number;
  durationMs?: number;
  bufferedMs?: number;
  error?: string;
  playlistUrl?: string;
};

export type HlsAudioControls = {
  play: () => Promise<void>;
  pause: () => Promise<void>;
  toggle: () => Promise<void>;
  seekTo: (ms: number) => Promise<void>;
  stop: () => Promise<void>;
  reload: () => Promise<void>;
  setOnStatus: (cb: ((s: AVPlaybackStatusSuccess) => void) | null) => void;
};

export function useHlsAudio(
  mainFileKey: string | undefined,
  opts: UseHlsAudioOpts = {}
): [HlsAudioState, HlsAudioControls] {
  const {
    autoplay = false,
    initialMs = 0,
    headers,
    progressUpdateIntervalMs = 500,
  } = opts;

  // 1) gọi API lấy PlaylistUrl
  const { data, isFetching, isLoading, isError, error, refetch } =
    useGetAudioQuery(mainFileKey ?? "", {
      skip: !mainFileKey,
    });

  const [state, setState] = useState<HlsAudioState>({
    ready: false,
    loading: false,
    isPlaying: false,
    positionMs: 0,
    durationMs: undefined,
    bufferedMs: undefined,
    error: undefined,
    playlistUrl: undefined,
  });

  const soundRef = useRef<Audio.Sound | null>(null);
  const userStatusCbRef = useRef<((s: AVPlaybackStatusSuccess) => void) | null>(
    null
  );

  // helper: clear current sound
  const unload = useCallback(async () => {
    if (soundRef.current) {
      try {
        await soundRef.current.stopAsync();
      } catch {}
      try {
        await soundRef.current.unloadAsync();
      } catch {}
      soundRef.current = null;
    }
  }, []);

  // helper: status update
  const onStatus = (st: AVPlaybackStatus) => {
    if (!st.isLoaded) {
      if ("error" in st && st.error) {
        setState((s) => ({
          ...s,
          error: st.error,
          loading: false,
          ready: false,
          isPlaying: false,
        }));
      }
      return;
    }
    const s = st as AVPlaybackStatusSuccess;

    // một số SDK/thiết bị có field playableDurationMillis, một số không
    const bufferedMs =
      (s as any).playableDurationMillis ??
      (s as any).buffered ?? // fallback phòng khi vendor trả tên khác
      state.bufferedMs; // giữ nguyên giá trị cũ nếu không có

    setState((prev) => ({
      ...prev,
      error: undefined,
      loading: false,
      ready: true,
      isPlaying: !!s.isPlaying,
      positionMs: s.positionMillis ?? 0,
      durationMs: s.durationMillis ?? prev.durationMs,
      bufferedMs,
    }));

    userStatusCbRef.current?.(s);
  };

  // 2) (re)load khi có PlaylistUrl mới
  useEffect(() => {
    let active = true;
    (async () => {
      if (!mainFileKey) {
        // reset state khi chưa có key
        await unload();
        setState((s) => ({
          ...s,
          ready: false,
          loading: false,
          isPlaying: false,
          positionMs: 0,
          durationMs: undefined,
          bufferedMs: undefined,
          error: undefined,
          playlistUrl: undefined,
        }));
        return;
      }
      if (isFetching || isLoading) {
        setState((s) => ({ ...s, loading: true, error: undefined }));
        return;
      }
      if (isError) {
        await unload();
        setState((s) => ({
          ...s,
          loading: false,
          error: (error as any)?.message || "Failed to fetch playlist",
        }));
        return;
      }

      const playlistUrl = data?.PlaylistUrl;
      if (!playlistUrl) return;

      // unload trước khi load nguồn mới
      await unload();

      const sound = new Audio.Sound();
      soundRef.current = sound;
      sound.setOnPlaybackStatusUpdate(onStatus);

      try {
        setState((s) => ({
          ...s,
          loading: true,
          error: undefined,
          playlistUrl,
        }));
        const source: any = headers
          ? { uri: playlistUrl, headers }
          : { uri: playlistUrl };

        await sound.loadAsync(source, {
          shouldPlay: autoplay,
          progressUpdateIntervalMillis: progressUpdateIntervalMs,
        });

        if (initialMs > 0) {
          await sound.setPositionAsync(initialMs);
        }
      } catch (e: any) {
        if (!active) return;
        setState((s) => ({
          ...s,
          loading: false,
          error: e?.message || "Cannot load HLS",
        }));
      }
    })();

    return () => {
      active = false;
      // không unload ở đây vì có thể muốn giữ khi dep thay đổi nhỏ;
      // nhưng khi component unmount, ta sẽ unload trong effect dưới
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [mainFileKey, isFetching, isLoading, isError, data?.PlaylistUrl]);

  // 3) cleanup khi unmount
  useEffect(() => {
    return () => {
      unload();
    };
  }, [unload]);

  // 4) controls
  const controls = useMemo<HlsAudioControls>(() => {
    return {
      play: async () => {
        if (!soundRef.current) return;
        await soundRef.current.playAsync();
      },
      pause: async () => {
        if (!soundRef.current) return;
        await soundRef.current.pauseAsync();
      },
      toggle: async () => {
        if (!soundRef.current) return;
        const st = await soundRef.current.getStatusAsync();
        if (st.isLoaded && st.isPlaying) await soundRef.current.pauseAsync();
        else if (st.isLoaded) await soundRef.current.playAsync();
      },
      seekTo: async (ms: number) => {
        if (!soundRef.current) return;
        await soundRef.current.setPositionAsync(Math.max(0, Math.floor(ms)));
      },
      stop: async () => {
        await unload();
        setState((s) => ({ ...s, ready: false, isPlaying: false }));
      },
      reload: async () => {
        await unload();
        // gọi lại API (lấy lại signed URL nếu cần)
        await refetch();
      },
      setOnStatus: (cb) => {
        userStatusCbRef.current = cb;
      },
    };
  }, [refetch, unload]);

  const derived: HlsAudioState = {
    ...state,
    loading: state.loading || isLoading || isFetching,
  };

  return [derived, controls];
}
