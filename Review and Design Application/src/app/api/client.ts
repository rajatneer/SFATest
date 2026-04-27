import type { SyncQueueItem } from "../context/AppContext";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:8080";
const API_TOKEN_KEY = "sfa_api_token";

interface LoginResponse {
  accessToken: string;
}

async function loginApiUser(): Promise<string> {
  const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      username: import.meta.env.VITE_API_USERNAME || "apiadmin",
      password: import.meta.env.VITE_API_PASSWORD || "Admin@12345"
    })
  });

  if (!response.ok) {
    throw new Error("API login failed.");
  }

  const payload = (await response.json()) as LoginResponse;
  localStorage.setItem(API_TOKEN_KEY, payload.accessToken);
  return payload.accessToken;
}

async function getToken(): Promise<string> {
  const existing = localStorage.getItem(API_TOKEN_KEY);
  if (existing) {
    return existing;
  }

  return loginApiUser();
}

export async function pushSyncItems(items: SyncQueueItem[]): Promise<void> {
  if (items.length === 0) {
    return;
  }

  const token = await getToken();

  const response = await fetch(`${API_BASE_URL}/api/mobile/sync/push`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify({
      items: items.map((item) => ({
        entityType: item.entity_type,
        entityRef: item.entity_ref,
        payload: {
          description: item.description,
          createdAt: item.created_at
        }
      }))
    })
  });

  if (response.status === 401) {
    localStorage.removeItem(API_TOKEN_KEY);
    const refreshedToken = await loginApiUser();
    const retry = await fetch(`${API_BASE_URL}/api/mobile/sync/push`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${refreshedToken}`
      },
      body: JSON.stringify({
        items: items.map((item) => ({
          entityType: item.entity_type,
          entityRef: item.entity_ref,
          payload: {
            description: item.description,
            createdAt: item.created_at
          }
        }))
      })
    });

    if (!retry.ok) {
      throw new Error("Sync push retry failed.");
    }

    return;
  }

  if (!response.ok) {
    throw new Error("Sync push failed.");
  }
}
