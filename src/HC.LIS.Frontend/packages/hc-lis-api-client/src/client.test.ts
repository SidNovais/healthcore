import { describe, it, expect, vi, beforeEach } from 'vitest';

vi.mock('./generated/sdk.gen', () => ({
  client: {
    setConfig: vi.fn(),
    interceptors: { response: { use: vi.fn() } },
  },
}));

import { client as sdkClient } from './generated/sdk.gen';
import { configureClient } from './client';

describe('configureClient', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('configures the SDK client with the provided baseUrl', () => {
    configureClient('http://test');

    expect(sdkClient.setConfig).toHaveBeenCalledWith(
      expect.objectContaining({ baseUrl: 'http://test' }),
    );
  });

  it('includes credentials: "include" in the config', () => {
    configureClient('http://test');

    expect(sdkClient.setConfig).toHaveBeenCalledWith(
      expect.objectContaining({ credentials: 'include' }),
    );
  });
});
