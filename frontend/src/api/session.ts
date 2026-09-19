export interface Session {
  token: string;
  householdId: string;
  userId: string;
  // Optional: sessions saved before these fields existed won't have them until the next login.
  displayName?: string;
  initials?: string;
  householdName?: string;
}

const STORAGE_KEY = "resit_session";

export function saveSession(session: Session): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
}

export function getSession(): Session | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  return raw ? (JSON.parse(raw) as Session) : null;
}

export function clearSession(): void {
  localStorage.removeItem(STORAGE_KEY);
}

export function requireHouseholdId(): string {
  const session = getSession();
  if (!session) {
    throw new Error("No active session.");
  }
  return session.householdId;
}
