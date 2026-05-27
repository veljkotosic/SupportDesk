async function parseJsonOrThrow(res: Response) {
  const text = await res.text();
  try {
    return text ? JSON.parse(text) : undefined;
  } catch (e) {
    return text;
  }
}

async function refreshTokens(): Promise<boolean> {
  try {
    const res = await fetch(`/api/Auth/refreshLogin`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
    });

    return res.ok;
  } catch (e) {
    return false;
  }
}

async function request<T = any>(method: string, url: string, body?: any, attemptRefresh = true): Promise<T> {
  const options: RequestInit = {
    method,
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
  };

  if (body !== undefined && body !== null) {
    options.body = JSON.stringify(body);
  }

  const originalResponse = await fetch(url, options);

  if (originalResponse.status === 401 && attemptRefresh) {
    const isRefreshed = await refreshTokens();

    if (isRefreshed) {
      return await request<T>(method, url, body, false);
    } else {
      throw new Error('Session expired');
    }
  }

  if (!originalResponse.ok) {
    const errorBody = await parseJsonOrThrow(originalResponse);

    if (errorBody?.errors && Array.isArray(errorBody.errors)) {
      const messages = errorBody.errors.map((e: any) => e.message ?? e);
      const err = new Error(messages[0]) as any;
      err.messages = messages;
      err.status = originalResponse.status;
      throw err;
    }

    throw Object.assign(new Error(originalResponse.statusText || 'Request failed'), {
      status: originalResponse.status,
      body: errorBody
    });
  }

  return await parseJsonOrThrow(originalResponse);
}

export const api = {
  get: <T = any>(url: string) => request<T>('GET', url),
  post: <T = any>(url: string, body?: any) => request<T>('POST', url, body),
  put: <T = any>(url: string, body?: any) => request<T>('PUT', url, body),
  delete: <T = any>(url: string, body?: any) => request<T>('DELETE', url, body),
}
