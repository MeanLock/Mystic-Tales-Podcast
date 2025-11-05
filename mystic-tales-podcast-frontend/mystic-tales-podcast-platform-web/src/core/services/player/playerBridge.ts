type EngineImpl = {
  play: () => Promise<void> | void;
  pause: () => void;
  seek: (s: number) => void;
  setVolume: (v: number) => void;
  getCurrentTime: () => number;
  getDuration: () => number;
  attachListeners: (l: any) => void;
  detachListeners: () => void;
  getCurrentTrackId: () => string | null;
};

let impl: EngineImpl | null = null;

export function registerPlayerImpl(e: EngineImpl) {
  impl = e;
}

export function unregisterPlayerImpl() {
  impl = null;
}

export function getAudioEngine() {
  return {
    play: async () => impl?.play(),
    pause: () => impl?.pause(),
    seek: (s: number) => impl?.seek(s),
    setVolume: (v: number) => impl?.setVolume(v),
    getCurrentTime: () => impl?.getCurrentTime() ?? 0,
    getDuration: () => impl?.getDuration() ?? 0,
    attachListeners: (l: any) => impl?.attachListeners(l),
    detachListeners: () => impl?.detachListeners(),
    getCurrentTrackId: () => impl?.getCurrentTrackId() ?? null,
  } as EngineImpl;
}

export default getAudioEngine;
