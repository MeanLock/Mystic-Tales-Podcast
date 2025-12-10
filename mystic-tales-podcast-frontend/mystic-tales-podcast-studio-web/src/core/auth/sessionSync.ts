// Session synchronization across browser tabs
// Handles forcing logout when a different user logs in in another tab

export interface AuthSessionPayload {
  userId: string;
  token: string;
  ts: number; // timestamp
}

const STORAGE_KEY = 'auth.session';
const CHANNEL_NAME = 'auth-sync';

let bc: BroadcastChannel | null = null;
try {
  bc = new BroadcastChannel(CHANNEL_NAME);
} catch (_) {
  bc = null; // Older browsers or unsupported environment
}

export function readSession(): AuthSessionPayload | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

export function writeSession(payload: AuthSessionPayload) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(payload));
  bc?.postMessage({ type: 'SESSION_UPDATE', data: payload });
}

export function clearSession() {
  localStorage.removeItem(STORAGE_KEY);
  bc?.postMessage({ type: 'SESSION_CLEAR' });
}

export function initSessionSync(currentUserId: string, onDifferentUser: (otherUserId: string) => void) {
  const handleIncoming = (payload: AuthSessionPayload | null) => {
    if (!payload) return;
    if (payload.userId !== currentUserId) onDifferentUser(payload.userId);
  };

  const bcHandler = (ev: MessageEvent) => {
    const { type, data } = ev.data || {};
    if (type === 'SESSION_UPDATE') handleIncoming(data as AuthSessionPayload);
    else if (type === 'SESSION_CLEAR') { /* optional */ }
  };

  if (bc) bc.onmessage = bcHandler;

  const storageHandler = (e: StorageEvent) => {
    if (e.key !== STORAGE_KEY || !e.newValue) return;
    try { handleIncoming(JSON.parse(e.newValue) as AuthSessionPayload); } catch { /* ignore */ }
  };
  window.addEventListener('storage', storageHandler);

  return () => {
    if (bc) bc.onmessage = null;
    window.removeEventListener('storage', storageHandler);
  };
}
