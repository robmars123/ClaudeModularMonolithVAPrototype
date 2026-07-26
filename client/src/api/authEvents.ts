// Small pub/sub so the http client can announce "the session is dead" without
// importing React or navigation concerns. AuthContext subscribes to this and
// reacts (clears state, lets route guards redirect to /login). Keeps
// httpClient decoupled from routing/UI entirely.
type SessionExpiredListener = () => void;

const listeners = new Set<SessionExpiredListener>();

export function onSessionExpired(listener: SessionExpiredListener): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

export function emitSessionExpired(): void {
  for (const listener of listeners) listener();
}
