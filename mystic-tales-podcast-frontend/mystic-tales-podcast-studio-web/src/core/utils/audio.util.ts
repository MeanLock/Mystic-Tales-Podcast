import { Episode } from "../types/episode";

export const buildEpisodeAudioFileName = (episode: Episode, mime: string) => {
    const extMap: Record<string, string> = {
        "audio/mpeg": ".mp3",
        "audio/wav": ".wav",
        "audio/x-wav": ".wav",
        "audio/ogg": ".ogg",
        "audio/webm": ".webm",
    };
    const ext = extMap[mime] || "";
    const base = "episode_uploaded_audio";
    return `${base}${ext}`;
};

export const secondsToTime = (s: number) => {
    if (s < 0) s = 0;
    const m = Math.floor(s / 60);
    const sec = Math.floor(s % 60)
        .toString()
        .padStart(2, "0");
    return `${m}:${sec}`;
};