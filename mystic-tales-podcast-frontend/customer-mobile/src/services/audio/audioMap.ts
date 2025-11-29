// Allow values to be either a remote URL (string) or a local require() (number)
export const audioMap: Record<string, string | number> = {
  "audio-1": "https://cdn.pixabay.com/audio/2025/10/17/audio_fe5dcfb7e0.mp3",
  "audio-2": "https://cdn.pixabay.com/audio/2025/01/21/audio_8903d9fb49.mp3",
  "audio-3": "https://cdn.pixabay.com/audio/2023/06/13/audio_7465eb6bfd.mp3",
  "audio-4": require("../../../assets/audio/audio-4.mp3"),
  "audio-5": require("../../../assets/audio/audio-5.mp3"),
  // ... thêm các bài của bạn vào đây
};
