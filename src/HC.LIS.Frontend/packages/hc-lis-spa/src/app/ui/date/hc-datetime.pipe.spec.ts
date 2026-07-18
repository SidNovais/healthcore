import { describe, expect, it } from 'vitest';
import { HcDateTimePipe } from './hc-datetime.pipe';

describe('HcDateTimePipe', () => {
  const pipe = new HcDateTimePipe();

  it('renders a human-readable date and time', () => {
    const result = pipe.transform('2026-07-18T14:30:00Z');
    expect(result).toContain('2026');
    // Includes a time component (locale time separator).
    expect(result).toMatch(/\d[:.]\d/);
    // Not the raw ISO string.
    expect(result).not.toContain('T');
    expect(result).not.toContain('Z');
  });

  it('treats an offset-less string as UTC', () => {
    // The backend contract is UTC. A datetime that arrives without a Z/offset
    // must be interpreted as UTC, not as the viewer's local time.
    const withZ = pipe.transform('2026-07-18T14:30:00Z');
    expect(pipe.transform('2026-07-18T14:30:00')).toBe(withZ);
    expect(pipe.transform('2026-07-18T14:30:00+00:00')).toBe(withZ);
  });

  it('returns the placeholder for empty or nullish input', () => {
    expect(pipe.transform(null)).toBe('—');
    expect(pipe.transform(undefined)).toBe('—');
    expect(pipe.transform('')).toBe('—');
  });

  it('returns the placeholder for an unparseable value', () => {
    expect(pipe.transform('not-a-date')).toBe('—');
  });
});
