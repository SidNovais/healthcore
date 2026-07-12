// Vitest setup — the builder's DOM environment does not provide a functional
// Web Storage implementation, which ThemeService needs at construction time.
function createStorage(): Storage {
  const store = new Map<string, string>();
  return {
    get length() {
      return store.size;
    },
    clear: () => store.clear(),
    getItem: (key: string) => store.get(key) ?? null,
    key: (index: number) => [...store.keys()][index] ?? null,
    removeItem: (key: string) => {
      store.delete(key);
    },
    setItem: (key: string, value: string) => {
      store.set(key, String(value));
    },
  };
}

for (const name of ['localStorage', 'sessionStorage'] as const) {
  const existing = (globalThis as Record<string, unknown>)[name] as Storage | undefined;
  if (typeof existing?.getItem !== 'function') {
    Object.defineProperty(globalThis, name, {
      value: createStorage(),
      writable: true,
      configurable: true,
    });
  }
}
