// app/hls-quick-play.tsx  (hoặc src/screens/HlsQuickPlay.tsx)
import React, { useCallback, useRef, useState } from "react";
import { View, Text, TextInput, Pressable } from "react-native";
import { Audio, AVPlaybackStatus, AVPlaybackStatusSuccess } from "expo-av";

const DEFAULT_URL =
  "https://relegable-permissible-orval.ngrok-free.dev/api/hls/playlist?sid=123&p=RDpcQXVkaW9Db25maWdcVGVzdEF1ZGlvXEF1ZGlvRVFVcGxvYWRlclx1cGxvYWRzXHNlc3Npb25zXDEyM1xobHNccGxheWxpc3QubTN1OA%3D%3D";

export default function HlsQuickPlay() {
  const [url, setUrl] = useState(DEFAULT_URL);
  const [isPlaying, setIsPlaying] = useState(false);
  const [pos, setPos] = useState(0);
  const [dur, setDur] = useState<number | undefined>(undefined);
  const [err, setErr] = useState<string | undefined>(undefined);
  const soundRef = useRef<Audio.Sound | null>(null);

  const onStatus = useCallback((st: AVPlaybackStatus) => {
    if (!st.isLoaded) {
      if ("error" in st && st.error) setErr(st.error);
      return;
    }
    const s = st as AVPlaybackStatusSuccess;
    setErr(undefined);
    setIsPlaying(!!s.isPlaying);
    setPos(s.positionMillis ?? 0);
    if (s.durationMillis != null) setDur(s.durationMillis);
  }, []);

  const loadAndPlay = useCallback(async () => {
    try {
      setErr(undefined);
      if (soundRef.current) {
        try {
          await soundRef.current.stopAsync();
        } catch {}
        try {
          await soundRef.current.unloadAsync();
        } catch {}
        soundRef.current = null;
      }
      const sound = new Audio.Sound();
      soundRef.current = sound;
      sound.setOnPlaybackStatusUpdate(onStatus);

      // Nếu cần auth cho playlist: thêm headers ở đây
      // const source = { uri: url, headers: { Authorization: `Bearer ${token}` } };
      const source = { uri: url };

      await sound.loadAsync(source, {
        shouldPlay: true,
        progressUpdateIntervalMillis: 500,
      });
    } catch (e: any) {
      setErr(e?.message ?? String(e));
    }
  }, [onStatus, url]);

  const toggle = useCallback(async () => {
    if (!soundRef.current) return;
    const st = await soundRef.current.getStatusAsync();
    if (st.isLoaded) {
      if (st.isPlaying) await soundRef.current.pauseAsync();
      else await soundRef.current.playAsync();
    }
  }, []);

  const seekDelta = useCallback(async (deltaMs: number) => {
    if (!soundRef.current) return;
    const st = await soundRef.current.getStatusAsync();
    if (!st.isLoaded) return;
    const to = Math.max(0, (st.positionMillis ?? 0) + deltaMs);
    await soundRef.current.setPositionAsync(to);
  }, []);

  const stop = useCallback(async () => {
    if (!soundRef.current) return;
    try {
      await soundRef.current.stopAsync();
    } catch {}
    try {
      await soundRef.current.unloadAsync();
    } catch {}
    soundRef.current = null;
    setIsPlaying(false);
  }, []);

  return (
    <View style={{ padding: 16, gap: 12 }}>
      <Text style={{ fontWeight: "700", fontSize: 16 }}>HLS Quick Test</Text>
      <TextInput
        value={url}
        onChangeText={setUrl}
        placeholder="https://...m3u8 (API trả m3u8 content)"
        autoCapitalize="none"
        style={{
          borderWidth: 1,
          borderColor: "#ccc",
          borderRadius: 8,
          padding: 10,
        }}
      />

      <View style={{ flexDirection: "row", gap: 8, flexWrap: "wrap" }}>
        <Pressable
          onPress={loadAndPlay}
          style={{ padding: 10, backgroundColor: "#AEE339", borderRadius: 10 }}
        >
          <Text>Load & Play</Text>
        </Pressable>
        <Pressable
          onPress={toggle}
          style={{ padding: 10, backgroundColor: "#D9D9D9", borderRadius: 10 }}
        >
          <Text>{isPlaying ? "Pause" : "Play"}</Text>
        </Pressable>
        <Pressable
          onPress={() => seekDelta(-15_000)}
          style={{ padding: 10, backgroundColor: "#EEE", borderRadius: 10 }}
        >
          <Text>-15s</Text>
        </Pressable>
        <Pressable
          onPress={() => seekDelta(+15_000)}
          style={{ padding: 10, backgroundColor: "#EEE", borderRadius: 10 }}
        >
          <Text>+15s</Text>
        </Pressable>
        <Pressable
          onPress={stop}
          style={{ padding: 10, backgroundColor: "#F2A2A2", borderRadius: 10 }}
        >
          <Text>Stop</Text>
        </Pressable>
      </View>

      <Text>
        {Math.floor(pos / 1000)}s / {Math.floor((dur ?? 0) / 1000)}s
      </Text>
      {!!err && <Text style={{ color: "red" }}>{err}</Text>}

      <Text style={{ color: "#888", marginTop: 8 }}>
        Mẹo: nếu playlist/segment cần auth, hãy dùng{" "}
        <Text style={{ fontWeight: "600" }}>signed URLs / query token</Text>.
        Header chỉ chắc chắn áp vào request playlist, không đảm bảo áp vào các
        segment.
      </Text>
    </View>
  );
}
