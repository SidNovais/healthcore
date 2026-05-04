import { client as sdkClient } from './generated/sdk.gen';

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly detail: string,
  ) {
    super(detail);
    this.name = 'ApiError';
  }
}

export function configureClient(baseUrl: string): void {
  sdkClient.setConfig({ baseUrl, credentials: 'include' });

  sdkClient.interceptors.response.use(async (response: Response) => {
    if (!response.ok) {
      let detail = response.statusText;
      try {
        const body = (await response.clone().json()) as { detail?: string };
        if (typeof body.detail === 'string') detail = body.detail;
      } catch {
        // non-JSON error body — use statusText
      }
      throw new ApiError(response.status, detail);
    }
    return response;
  });
}
