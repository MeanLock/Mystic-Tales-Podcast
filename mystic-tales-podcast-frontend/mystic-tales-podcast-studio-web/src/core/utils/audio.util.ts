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