import { describe, it, expect, vi, beforeEach } from 'vitest';

vi.mock('@hey-api/client-fetch', () => ({
  createClient: vi.fn(() => ({ interceptors: { response: { use: vi.fn() } } })),
  createConfig: vi.fn((cfg: unknown) => cfg),
}));

import { createClient, createConfig } from '@hey-api/client-fetch';
import { configureClient } from './client';

describe('configureClient', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('calls createClient with the provided baseUrl', () => {
    configureClient('http://test');

    expect(createConfig).toHaveBeenCalledWith(
      expect.objectContaining({ baseUrl: 'http://test' }),
    );
    expect(createClient).toHaveBeenCalledOnce();
  });

  it('includes credentials: "include" in the config', () => {
    configureClient('http://test');

    expect(createConfig).toHaveBeenCalledWith(
      expect.objectContaining({ credentials: 'include' }),
    );
  });
});
