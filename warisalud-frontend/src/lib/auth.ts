import type { AuthTokenPayload, Rol } from '../types';

const TOKEN_KEY = 'wari_token';

export function saveToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function removeToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

function b64DecodeUnicode(str: string): string {
  return decodeURIComponent(
    Array.prototype.map
      .call(
        atob(str),
        (c: string) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2),
      )
      .join(''),
  );
}

export function parseToken(token: string): AuthTokenPayload | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    const payload = b64DecodeUnicode(parts[1].replace(/-/g, '+').replace(/_/g, '/'));
    const raw = JSON.parse(payload);
    const role = raw.role || raw.rol || raw.Role || raw['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
    const email = raw.email || raw.Email || raw['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || '';
    const sub = raw.sub || raw['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || '';
    return {
      ...raw,
      role,
      email,
      sub,
    } as AuthTokenPayload;
  } catch {
    return null;
  }
}

export function getCurrentUser(): AuthTokenPayload | null {
  const token = getToken();
  if (!token) return null;
  const payload = parseToken(token);
  if (!payload) return null;
  if (payload.exp * 1000 < Date.now()) {
    removeToken();
    return null;
  }
  return payload;
}

export function getUserRole(): Rol | null {
  return getCurrentUser()?.role ?? null;
}

export function isAuthenticated(): boolean {
  return getCurrentUser() !== null;
}
