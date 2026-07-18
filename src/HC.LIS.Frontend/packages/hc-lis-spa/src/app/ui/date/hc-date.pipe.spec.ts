import { describe, expect, it } from 'vitest';
import { HcDatePipe } from './hc-date.pipe';

describe('HcDatePipe', () => {
  const pipe = new HcDatePipe();

  it('renders a human-readable calendar date', () => {
    const result = pipe.transform('1990-01-01');
    expect(result).toContain('1990');
    // Not the raw ISO string.
    expect(result).not.toContain('-');
  });

  it('ignores a time component and the UTC marker (no timezone shift)', () => {
    // A DOB is a calendar date: the same day must render regardless of any
    // attached time or Z marker, in every viewer timezone.
    const dateOnly = pipe.transform('1990-01-01');
    expect(pipe.transform('1990-01-01T00:00:00Z')).toBe(dateOnly);
    expect(pipe.transform('1990-01-01T23:59:59Z')).toBe(dateOnly);
    expect(pipe.transform('1990-01-01T00:00:00')).toBe(dateOnly);
  });

  it('does not roll a late-in-day UTC date back to the previous day', () => {
    // The classic trap: new Date('1990-12-31T23:00:00Z') reads back as Dec 31
    // in UTC but Jan/Dec boundary shifts in other zones. hcDate must not shift.
    const result = pipe.transform('1990-12-31T23:00:00Z');
    expect(result).toBe(pipe.transform('1990-12-31'));
    expect(result).toContain('1990');
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
