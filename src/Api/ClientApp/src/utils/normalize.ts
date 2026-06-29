function toCamelCase(key: string): string {
  if (!key) return key
  return key.charAt(0).toLowerCase() + key.slice(1)
}

export function normalizeKeys<T>(data: unknown): T {
  if (Array.isArray(data)) {
    return data.map((item) => normalizeKeys(item)) as T
  }
  if (data !== null && typeof data === 'object') {
    return Object.fromEntries(
      Object.entries(data as Record<string, unknown>).map(([key, value]) => [
        toCamelCase(key),
        normalizeKeys(value),
      ])
    ) as T
  }
  return data as T
}
